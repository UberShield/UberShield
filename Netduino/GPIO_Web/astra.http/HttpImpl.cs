/*
 * Driver independant Http library, for embedded web servers
 *      
 * Use this code for whatever you want. Modify it, redistribute it, at will
 * Just keep this header intact, however, and add your own modifications to it!
 * 
 * 10 Feb 2011  -- Quiche31 - Added HttpImplementation.Write() with format to save RAM
 * 29 Jan 2011  -- Quiche31 - Initial release, tested OK with two drivers: Socket and WiFly
 * 
 * */
using System;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace astra.http
{
    public class HttpImplementationClient
    {
        public delegate void RequestReceivedDelegate(HttpContext context);
    }

    /*
     * This interface is to be implemented by the hardware implementing the transport level,
     * such as wired NetduinoPlus, wired WizNet module, or wireless WiFly Shield
     **/
    public interface HttpImplementation
    {
        String getIP();
        void Listen();
        void Write(String response);
        void BinaryWrite(byte[] response, int start = 0, int length = -1);
        void Close();
        HttpResponse SendRequest(String host, int port, String data);
    }

    public class HttpRequestLines : IEnumerator
    {
        String[] lines;
        int index;
        public IEnumerator GetEnumerator() { return this; }
        public HttpRequestLines(String[] lines)
        {
            this.lines = lines;
            index = -1; // because foreach() calls MoveNext() at first
        }
        public object Current { get { if (index == -1 && lines != null) index = 0;  return lines[index].Trim(); } }
        public bool MoveNext()
        {
            if (index == -1 && lines != null)
                index = 0;
            else if (index < lines.Length - 1)
                index++;
            return lines != null && index < lines.Length-1;
        }
        public void Reset()
        {
            index = 0;
        }
    }

    /*
     * Parse a HTTP request
     */ 
    public class HttpRequestParser
    {
        public void parse(HttpRequest request, HttpResponse response, HttpRequestLines lines)
        {
            const String getVerb = "GET";
            const String postVerb = "POST";
            const String httpResponsePrefix = "HTTP/1.1 ";
            const String httpVerb = "HTTP/";
            const String hostVerb = "Host: ";
            const String connectionVerb = "Connection: ";
            const String acceptVerb = "Accept: ";
            const String userAgentVerb = "User-Agent: ";
            const String acceptEncodingVerb = "Accept-Encoding: ";
            const String acceptLanguageVerb = "Accept-Language: ";
            const String acceptCharsetVerb = "Accept-Charset: ";
            const String contentLengthVerb = "Content-Length: ";
            const String authorizationVerb = "Authorization: ";
            const String cookieVerb = "Cookie: ";
            int index;

            HttpBaseHeader header = null;
            if (request != null)
                header = request;
            else if (response != null)
                header = response;
            else throw new Exception("HttpRequestParser.parse: Parameter error");

            foreach (String received in lines)
            {
                Debug.Print("= " + received);

                if ((index = received.IndexOf(httpResponsePrefix)) == 0 && response != null)
                {
                    response.ErrorCode = received.Substring(index + httpResponsePrefix.Length).Trim();
                    continue;
                }

                if (request != null)
                {
                    if ((index = received.IndexOf(getVerb)) != -1)
                        request.RequestType = getVerb;
                    if ((index = received.IndexOf(getVerb)) != -1)
                        request.RequestType = getVerb;
                    else if ((index = received.IndexOf(postVerb)) != -1)
                        request.RequestType = postVerb;
                }

                if (index != -1)
                {
                    int spaceIndex = received.Substring(index).IndexOf(' ');
                    int httpIndex = received.IndexOf(httpVerb);
                    try
                    {
                        header.RawUrl = received.Substring(index + spaceIndex + 1, httpIndex - (index + spaceIndex) - 2);
                    }
                    catch (Exception)
                    {
                        Debug.Print("UNEXPECTED EXCEPTION on: " + received);
                    }
                }
                else if ((index = received.IndexOf(hostVerb)) != -1)
                {
                    header.Host = received.Substring(index + hostVerb.Length);
                }
                else if ((index = received.IndexOf(connectionVerb)) != -1)
                {
                    header.Connection = received.Substring(index + connectionVerb.Length);
                }
                else if ((index = received.IndexOf(acceptEncodingVerb)) != -1)
                {
                    header.AcceptEncoding = received.Substring(index + acceptEncodingVerb.Length);
                }
                else if ((index = received.IndexOf(acceptLanguageVerb)) != -1)
                {
                    header.AcceptLanguage = received.Substring(index + acceptLanguageVerb.Length);
                }
                else if ((index = received.IndexOf(acceptCharsetVerb)) != -1)
                {
                    header.AcceptCharSet = received.Substring(index + acceptCharsetVerb.Length);
                }
                else if ((index = received.IndexOf(acceptVerb)) != -1)
                {
                    header.Accept = received.Substring(index + acceptVerb.Length);
                }
                else if ((index = received.IndexOf(userAgentVerb)) != -1)
                {
                    header.UserAgent = received.Substring(index + userAgentVerb.Length);
                }
                else if ((index = received.IndexOf(authorizationVerb)) != -1)
                {
                    header.Authorization = received.Substring(index + authorizationVerb.Length);
                }
                else if ((index = received.IndexOf(cookieVerb)) != -1)
                {
                    header.RawCookies = received.Substring(index + cookieVerb.Length);
                }
                else if ((index = received.IndexOf(contentLengthVerb)) != -1)
                {
                    try
                    {
                        String l = received.Substring(index + contentLengthVerb.Length);
                        header.ContentLength = int.Parse(l);
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                    }
                }
                if (received.Length == 0)
                {
                    if (request != null && request.RequestType == "POST")
                    {
                        // We expect to see a line following this empty one
                        lines.MoveNext();
                        String postParams = (String)lines.Current;
                        request.parseContents(postParams);
                    }
                    else if (response != null)
                    {
                        // We expect to see a line following this empty one
                        int length = header.ContentLength;
                        if(length == 0)
                            length = 256;
                        StringBuilder sb = new StringBuilder(length + 10);
                        while (lines.MoveNext())
                        {
                            sb.Append(lines.Current.ToString());
                            sb.Append('\n');
                        }
                        response.Contents = sb.ToString();
                    }
                }
            }
        }
    }
}
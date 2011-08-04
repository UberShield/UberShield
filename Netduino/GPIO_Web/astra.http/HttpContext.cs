/*
 * Application layer for HTTP server - a pale imitation of System.Web
 *       
 * Use this code for whatever you want. Modify it, redistribute it, at will
 * Just keep this header intact, however, and add your own modifications to it!
 *
 * 10 Feb 2011  -- Quiche31 - added URL decode en posted parameters
 * 14 Jan 2011  -- Quiche31 - added support for URL and forms (POST) parameters
 * 10 Jan 2011  -- Quiche31 - Initial release, with cookies support (still missing: a working BinaryWrite)
 * 
 * */
using System;
using System.Collections;
using Microsoft.SPOT;
using MFToolkit.Net.Web;

namespace astra.http
{
    /*
     *  Provides a subset of: http://msdn.microsoft.com/en-us/library/system.web.httpcontext.aspx
     */
    public class HttpContext
    {
        public HttpRequest Request {get; set;}
        public HttpResponse Response {get; set;}

        public HttpContext(HttpRequest Request, HttpResponse Response)
        {
            this.Request = Request;
            this.Response = Response;
        }
    }

    public class HttpBaseHeader
    {
        public String Connection { get; set; }
        public String Accept { get; set; }
        public String UserAgent { get; set; }
        public String AcceptEncoding { get; set; }
        public String AcceptLanguage { get; set; }
        public String AcceptCharSet { get; set; }
        public String Authorization { get; set; }
        public String Host { get; set; }
        public int ContentLength { get; set; }
        public String ContentType { get; set; }
        public String Date { get; set; }
        public String LastModified { get; set; }
        public String Path { get; set; }
        public String RawUrl { get { return rawUrl; } set { parseUrl(rawUrl = value); } }
        public String Contents { get; set; }
        protected Hashtable additionalAttributes = null;

        private String rawUrl = null;
        private Hashtable parameters = null;
        private String rawCookies = null;
        private Hashtable cookies = null;
        public String getCookie(String value)
        {
            return cookies == null ? null : (String)cookies[value];
        }
        public String RawCookies
        {
            get { return rawCookies; }
            set
            {
                // Parse the cookie string to decompose attribute/value pairs
                int i1, i2;
                String att, val;
                rawCookies = value;
                while (value != null)
                {
                    if ((i1 = value.IndexOf('=')) != -1)
                    {
                        att = value.Substring(0, i1).Trim();
                        if ((i2 = value.IndexOf(';')) != -1)
                        {
                            val = value.Substring(i1 + 1, i2 - i1 - 1).Trim();
                            value = value.Substring(i2 + 1);
                        }
                        else
                        {
                            val = value.Substring(i1 + 1).Trim();
                            value = null;
                        }
                        if (cookies == null)
                            cookies = new Hashtable();
                        cookies[att] = val;
                    }
                    else value = null;
                }
            }
        }

        void parseUrl(String s)
        {
            int i;
            if ((i = s.IndexOf('?')) != -1)
            {
                Path = s.Substring(0, i);
                s = s.Substring(i + 1);
                parseContents(s);
            }
            else
                Path = s;
        }

        public void parseContents(String s)
        {
            int i0, i1;
            while (s.Length != 0)
            {
                if ((i0 = s.IndexOf('=')) != -1)
                {
                    if ((i1 = s.IndexOf('&')) != -1)
                    {
                        addParam(s.Substring(0, i0), HttpServerUtility.UrlDecode(s.Substring(i0 + 1, i1 - i0 - 1)));
                        s = s.Substring(i1 + 1);
                    }
                    else
                    {
                        addParam(s.Substring(0, i0), HttpServerUtility.UrlDecode(s.Substring(i0 + 1)));
                        break;
                    }
                }
                else
                    // Invalid paparemers (missing EQUAL sign)
                    break;
            }
        }

        public String getParameter(String value)
        {
            return parameters == null ? null : (String)parameters[value];
        }

        private void addParam(String key, String value)
        {
            if (parameters == null)
                parameters = new Hashtable();
            if (parameters.Contains(key))
                parameters.Remove(key);
            parameters.Add(key, value);
        }

        public Hashtable getParams()
        {
            return parameters;
        }

        public void Add(String attribute, String value)
        {
            if (additionalAttributes == null)
                additionalAttributes = new Hashtable();
            additionalAttributes[attribute] = value;
        }
    }

    /*
     *  Provides a subset of: http://msdn.microsoft.com/en-us/library/system.web.httprequest.aspx
     */
    public class HttpRequest : HttpBaseHeader
    {
        public String RequestType { get; set; }
    }


    /*
     *  Provides a subset of: http://msdn.microsoft.com/en-us/library/system.web.httpresponse.aspx
     */
    public class HttpResponse : HttpBaseHeader
    {
        HttpImplementation impl;
        private Boolean headerWritten = false;
        private Hashtable cookies = null;
        private String location = null;

        public String ErrorCode { get; set; }
        public String SetCookie { get; set; }
        public void setRedirect(String url)
        {
            ErrorCode = "302 Found";
            location = url;
        }

        public void addCookie(String attribute, String value)
        {
            if (cookies == null)
                cookies = new Hashtable();
            cookies[attribute] = value;
        }

        public void removeCookie(String attribute)
        {
            if (cookies != null)
                cookies.Remove(attribute);
        }

        void writeCookies(StringBuilder header)
        {
            if (cookies != null)
            {
                int n = 0;
                header.Append("Set-Cookie: ");
                foreach (var key in cookies.Keys)
                {
                    if (n++ != 0)
                        header.Append("; ");
                    header.Append(key.ToString() + '=' + cookies[key]);
                }
                header.Append('\n');
            }
        }


        private void WriteHeader()
        {
            if (!headerWritten)
            {
                StringBuilder header = new StringBuilder(512);
                header.Append("HTTP/1.1 ");
                if(ErrorCode != null)
                    header.Append(ErrorCode);
                else
                    header.Append("200 OK");
                header.Append('\n');
                if (location != null)
                    header.Append("Location: " + location + '\n');
                if (ContentType != null)
                    header.Append("Content-Type: " + ContentType + '\n');
                if (Date != null)
                    header.Append("Date: " + Date + '\n');
                if (LastModified != null)
                    header.Append("Last-Modified: " + LastModified + '\n');
                if (ContentLength != 0)
                    header.Append("Content-Length: " + ContentLength + '\n');
                if(additionalAttributes != null)
                    foreach (String key in additionalAttributes.Keys)
                    {
                        header.Append(key+": ");
                        header.Append((String)additionalAttributes[key] + '\n');
                    }
                writeCookies(header);
                header.Append('\n');
                impl.Write(header.ToString());
                headerWritten = true;
            }
        }
        public HttpResponse(HttpImplementation impl)
        {
            this.impl = impl;
        }
        public void Write(String response, Object []args = null)
        {
            WriteHeader();
            if (response != null && response.Length != 0)
            {
                if(args != null)
                    HttpUtils.expandString(impl, response, args);
                else
                    impl.Write(response);
            }
        }
        public void BinaryWrite(byte[] response)
        {
            WriteHeader();
            if (response != null && response.Length != 0)
                impl.BinaryWrite(response);
        }
        public void Close()
        {
            impl.Close();
        }
    }
}

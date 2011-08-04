/*
 * HttpLibrary implementation for a TCP Socket, such as the one from Netduino Plus
 *      
 * Use this code for whatever you want. Modify it, redistribute it, at will
 * Just keep this header intact, however, and add your own modifications to it!
 * 
 * 29 Jan 2011  -- Quiche31 - Initial release, tested OK with Netduino Plus
 * 
 * */
using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Microsoft.SPOT;

namespace astra.http
{
    public class HttpSocketImpl : HttpImplementation, IDisposable
    {
        private Socket m_listeningSocket = null;
        private Socket m_clientSocket = null;
        private HttpImplementationClient.RequestReceivedDelegate m_requestReceived = null;
        private const int maxRequestSize = 1024;

        public HttpSocketImpl(int localPort = 80)
        {
            m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public HttpSocketImpl(HttpImplementationClient.RequestReceivedDelegate requestReceived, int localPort = 80)
        {
            m_requestReceived = requestReceived;
            m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(new IPAddress(new byte[] { 195, 83, 132, 135 }), 123);
            m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, localPort));
            m_listeningSocket.Listen(10);
        }

        public HttpResponse SendRequest(String host, int port, String data)
        {
            int wait = 5;
            IPHostEntry entry = System.Net.Dns.GetHostEntry(host);
            HttpResponse response = new HttpResponse(this);

            if(entry != null)
            {
                IPAddress address = entry.AddressList[0];
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(address, port);
                serverSocket.Connect(ep);
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                serverSocket.Send(buffer);
                StringBuilder line = new StringBuilder(1024);
                while (wait-- >= 0)
                {
                    int availableBytes = serverSocket.Available;
                    //Debug.Print(DateTime.Now.ToString() + " " + availableBytes.ToString() + " request bytes available");

                    int bytesReceived = (availableBytes > maxRequestSize ? maxRequestSize : availableBytes);
                    if (bytesReceived > 0)
                    {
                        buffer = new byte[bytesReceived]; // Buffer probably should be larger than this.
                        int readByteCount = serverSocket.Receive(buffer, bytesReceived, SocketFlags.None);
                        String contents = new String(Encoding.UTF8.GetChars(buffer));
                        line.Append(contents);
                    }
                    Thread.Sleep(100);
                }
                String[] lines = line.ToString().Split('\n');
                new HttpRequestParser().parse(null, response, new HttpRequestLines(lines));
                serverSocket.Close();
            }
            return response;
        }

        public void Write(String response)
        {
            BinaryWrite(Encoding.UTF8.GetBytes(response));
        }
        public void BinaryWrite(byte[] response, int start = 0, int length = -1)
        {
            if (length == -1)
                m_clientSocket.Send(response);
            else
                m_clientSocket.Send(response, start, length, SocketFlags.None);
        }
        public void Close()
        {
            m_clientSocket.Close();
        }

        public String getIP()
        {
            return m_listeningSocket.LocalEndPoint.ToString();
        }

        public void Dispose()
        {
            if( m_listeningSocket != null )
                m_listeningSocket.Close();
        }

        public void Listen()
        {
            while (true)
            {
                using (Socket clientSocket = m_listeningSocket.Accept())
                {
                    int wait = 2;
                    m_clientSocket = clientSocket;
                    StringBuilder line = new StringBuilder();
                    HttpRequest request = new HttpRequest();
                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;
                    //Debug.Print("\nReceived request from " + clientIP.ToString());
                    // Need to read from socket more than once, as POST requests can be issued in multiple writes
                    while(wait-- >=0 )
                    {
                        int availableBytes = clientSocket.Available;
                        //Debug.Print(DateTime.Now.ToString() + " " + availableBytes.ToString() + " request bytes available");

                        int bytesReceived = (availableBytes > maxRequestSize ? maxRequestSize : availableBytes);
                        if (bytesReceived > 0)
                        {
                            byte[] buffer = new byte[bytesReceived]; // Buffer probably should be larger than this.
                            int readByteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);
                            String contents = new String(Encoding.UTF8.GetChars(buffer));
                            line.Append(contents);
                        }                        
                        Thread.Sleep(10);
                    }
                    String[] lines = line.ToString().Split('\n');
                    line.Clear();
                    new HttpRequestParser().parse(request, null, new HttpRequestLines(lines));
                    if (m_requestReceived != null)
                        m_requestReceived(new HttpContext(request, new HttpResponse(this)));
                }
            }
        }
    }
}

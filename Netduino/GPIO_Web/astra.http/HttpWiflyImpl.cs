/*
 * HttpLibrary implementation for Sparkfun's WiFly shield
 *      
 * Use this code for whatever you want. Modify it, redistribute it, at will
 * Just keep this header intact, however, and add your own modifications to it!
 * 
 * 29 Jan 2011  -- Quiche31 - Repackaged to fit the driver independent HttpLibrary
 * 20 Jan 2011  -- Quiche31 - fixed baudrate bug with quartz at 12.288MHz, and insufficient debug output with TraceAll
 * 14 Jan 2011  -- Quiche31 - SPI-UART at 38400 bauds to avoid garbled Tx, allow UART in parrallel to normal operation, support http post
 * 10 Jan 2011  -- Quiche31 - Addition of HTTP helper code
 * 10 Jan 2011  -- Quiche31 - Slim port from Azalea Galaxy code
 * 17 Dec 2010 -- "Phillip" / "Azalea Galaxy" Original code for Arduino (https://github.com/sparkfun/WiFly-Shield)
 * 
 * */
using System;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT;
using System.IO.Ports;

namespace astra.http
{
    public class HttpWiflyImpl : HttpImplementation, IDisposable
    {
        public enum DeviceType
        {
            crystal_12_288_MHz,
            crystal_14_MHz,
        }

        private enum WiflyRegister
        {
            THR = 0x00 << 3,
            RHR = 0x00 << 3,
            IER = 0x01 << 3,
            FCR = 0x02 << 3,
            IIR = 0x02 << 3,
            LCR = 0x03 << 3,
            MCR = 0x04 << 3,
            LSR = 0x05 << 3,
            MSR = 0x06 << 3,
            SPR = 0x07 << 3,
            DLL = 0x00 << 3,
            DLM = 0x01 << 3,
            EFR = 0x02 << 3,
        }
        public DeviceType m_deviceType { get; set; }
        public int LocalPort { get; set; }
        private SPI m_uart;
        private SPI.SPI_module m_spiModule;
        private Cpu.Pin m_chipSelect;
        private Boolean m_initialized = false;
        private Boolean m_opened = false;
        private SerialPort m_serialPort;
        private HttpImplementationClient.RequestReceivedDelegate m_requestReceived = null;

        public HttpWiflyImpl(HttpImplementationClient.RequestReceivedDelegate requestReceived, int localPort,
            DeviceType deviceType, SPI.SPI_module spiModule, Cpu.Pin chipSelect)
        {
            m_requestReceived = requestReceived;
            m_deviceType = deviceType;
            LocalPort = localPort;
            this.m_spiModule = spiModule;
            this.m_chipSelect = chipSelect;
        }

        public HttpWiflyImpl(int localPort,
            DeviceType deviceType, SPI.SPI_module spiModule, Cpu.Pin chipSelect)
        {
            m_deviceType = deviceType;
            LocalPort = localPort;
            this.m_spiModule = spiModule;
            this.m_chipSelect = chipSelect;
        }

        public void EnableGateway(String port = "COM1", int rate = 38400)
        {
            String hello = "WiFly-GSX ready\n\r";
            m_serialPort = new SerialPort(port, rate, Parity.None, 8, StopBits.One);
            m_serialPort.ReadTimeout = 0;
            m_serialPort.Open();
            m_serialPort.Write(getBytes(hello), 0, hello.Length);
        }

        private void Init()
        {
            if (!m_initialized)
            {
                m_uart = new SPI(new SPI.Configuration(m_chipSelect, false, 10, 10, false, true, 2000, m_spiModule));
                WriteRegister(WiflyRegister.LCR, 0x80); // 0x80 to program baudrate

                if (m_deviceType == DeviceType.crystal_12_288_MHz)
                    // value = (12.288*1024*1024) / (baudrate*16)
                    WriteRegister(WiflyRegister.DLL, 42);       // 4800=167, 9600=83, 19200=42, 38400=21
                else
                    // value = (14*1024*1024) / (baudrate*16)
                    WriteRegister(WiflyRegister.DLL, 48);     // 4800=191, 9600=96, 19200=48, 38400=24
                WriteRegister(WiflyRegister.DLM, 0);
                WriteRegister(WiflyRegister.LCR, 0xbf); // access EFR register
                WriteRegister(WiflyRegister.EFR, 0xd0); // enable enhanced registers and enable RTC/CTS on the SPI UART
                WriteRegister(WiflyRegister.LCR, 3);    // 8 data bit, 1 stop bit, no parity
                WriteRegister(WiflyRegister.FCR, 0x06); // reset TXFIFO, reset RXFIFO, non FIFO mode
                WriteRegister(WiflyRegister.FCR, 0x01); // enable FIFO mode
                WriteRegister(WiflyRegister.SPR, 0x55);

                if (ReadRegister(WiflyRegister.SPR) != 0x55)
                    throw new Exception("Failed to init SPI<->UART chip");
                m_initialized = true;
            }
        }

        public void Open()
        {
            if (!m_opened)
            {
                Init();
                Thread.Sleep(200);
                enterCommandMode();
                SendCommand("reboot", "Listen on", "ERR:", 0, 500);
                enterCommandMode();
                Send("\r");
                //SendCommand("ftp u wifly-221G.img");
                //SendCommand("get e");
                m_opened = true;
            }
        }

        public void Open(String ssid, String phrase)
        {
            if (!m_opened)
            {
                Init();
                Thread.Sleep(200);
                enterCommandMode();
                Send("\r");
                SendCommand("save", "Storing", "ERR:", 0, 100);
                SendCommand("reboot", "Listen on", "ERR:", 0, 500);
                enterCommandMode();
                Send("\r");
                SendCommand("factory RESET");
                SendCommand("set uart baud 19200");
                SendCommand("set wlan ssid " + ssid);
                if (phrase != null && phrase.Length != 0)
                    SendCommand("set wlan phrase " + phrase);
                SendCommand("set wlan rate 14");
                SendCommand("set wlan hide 1");
                SendCommand("set ip localport " + LocalPort);
                SendCommand("set uart flow 1");
                SendCommand("set uart mode 0");
                SendCommand("set comm remote 0");
                SendCommand("save", "Storing", "ERR:", 0, 100);
                SendCommand("reboot", "Listen on", "ERR:", 0, 500);
                enterCommandMode();
                m_opened = true;
            }
        }

        public HttpResponse SendRequest(String host, int port, String data)
        {
            HttpResponse response = new HttpResponse(this);
            enterCommandMode();
            Thread.Sleep(100);
            SendCommand("open " + host + " " + port, "AOK", "ERR:", 0, 500);
            //Thread.Sleep(50);
            //leaveCommandMode();
            Send(data);
            Thread.Sleep(200);

            StringBuilder line = new StringBuilder(512);
            while (true)
            {
                while ((ReadRegister(WiflyRegister.LSR) & 0x01) > 0)
                {
                    char c = (char)ReadRegister(WiflyRegister.RHR);
                    line.Append(c);
                }
                String contents = line.ToString();
                if (contents.Length != 0)
                {
                    String[] lines = contents.Split('\n');
                    new HttpRequestParser().parse(null, response, new HttpRequestLines(lines));
                    break;
                }
            }
            return response;
        }

        private void WriteArray(byte[] ba)
        {
            for (int i = 0; i < ba.Length; i++)
                WriteRegister(WiflyRegister.THR, ba[i]);
        }

        private void WriteArray(byte[] ba, int start, int length)
        {
            if (length == -1)
                WriteArray(ba);
            else
            {
                for (int i = start; length > 0; i++, length--)
                    WriteRegister(WiflyRegister.THR, ba[i]);
            }
        }

        private void WriteRegister(WiflyRegister reg, byte b)
        {
            m_uart.Write(new byte[] { (byte)reg, b });
        }

        private byte ReadRegister(WiflyRegister reg)
        {
            byte[] buffer = new byte[] { (byte)((byte)reg | 0x80), 0 };
            m_uart.WriteRead(buffer, buffer);
            return buffer[1];
        }

        public void Send(string str)
        {
            WriteArray(getBytes(str));
            Debug.Print(str);
        }

        byte[] getBytes(String str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        /*
         * Sends the command <command> to the module, with default timeout
         * 
         * Returns "AOK" or "ERR:xxx" or else the last line that the command has returned
         */
        public String SendCommand(String command)
        {
            return SendCommand(command, "AOK", "ERR:", 0, 100);
        }

        /*
         * Sends the command <command> to the module, with lookup key and with default timeout
         * 
         * Returns the first line containing the lookup <key> or "ERR:xxx" or else the last line that the command has returned
         */
        public String SendCommand(String command, String key)
        {
            return SendCommand(command, key, "ERR:", 0, 100);
        }

        /*
         * Sends the command <command> to the module, and returns a response starting with <key>; wait nLines to be consumed, or delay to ellapse
         * 
         * if <key> is non null and <nLines> is 0 => returns at the first received line that contains <key>
         * if <key> is non null and <nLines> is not 0 => wait to receive <nLines> lines, and returns at the first received line that contains <key>
         */
        public String SendCommand(String command, String key1, String key2, int nLines, int delay)
        {
            StringBuilder buffer = new StringBuilder();
            String result = "";
            Boolean countLines = nLines > 0;

            Send(command + '\r');
            while (--delay > 0)
            {
                while ((ReadRegister(WiflyRegister.LSR) & 0x01) > 0)
                {
                    char c = (char)ReadRegister(WiflyRegister.RHR);
                    if (c == '\r')
                    {
                        String line = buffer.ToString();
                        Debug.Print("> " + line);
                        if (line.IndexOf(key1) != -1 || line.IndexOf(key2) != -1)
                            result = line;
                        if (countLines && nLines-- <= 0)
                            return result;
                        buffer.Clear();
                    }
                    else if (c != '\r' && c != '\n')
                        buffer.Append(c);
                }
                Thread.Sleep(4);
            }
            return result;
        }

        protected void enterCommandMode()
        {
            // Need to frame a 250ms window around the $$$ sequence (we use 300 below, to stay safe), as stated in the WiFly documentation:
            // "Characters are PASSED until this exact sequence is seen. If any bytes are seen before these chars, or after these chars, in a
            //  250ms window, command mode will not be entered and these bytes will be passed on to other side"
            Thread.Sleep(300);
            Send("$$$");
            Thread.Sleep(400);
        }

        protected void leaveCommandMode()
        {
            Send("exit\r");
        }

        String receiveLine()
        {
            StringBuilder line = new StringBuilder();
            for (int i = 0; i < 200; i++)
            {
                while ((ReadRegister(WiflyRegister.LSR) & 0x01) > 0)
                {
                    char c = (char)ReadRegister(WiflyRegister.RHR);
                    if (c == '\n')
                        return line.ToString();
                    line.Append(c);
                }
            }
            return line.ToString();
        }

        public void getTime()
        {
            Open();
            byte[] ntpData = new byte[48];
            Array.Clear(ntpData, 0, 48);

            enterCommandMode();
            Thread.Sleep(200);

            SendCommand("set ip protocol 1");
            SendCommand("open 195.83.132.135 123", "Connect");
            //SendCommand("open 193.54.76.41 123\r", "Connect");
            
            ntpData[0] = 0x1B;
            BinaryWrite(ntpData);
            Thread.Sleep(100);

            int j = 0;
            for (int i = 0; i < 10; i++)
            {
                if(j < ntpData.Length - 1 && (ReadRegister(WiflyRegister.LSR) & 0x01) > 0)
                {
                    byte c = (byte)ReadRegister(WiflyRegister.RHR);
                    ntpData[j] = c;
                }
                Thread.Sleep(10);
            }
            String received = new String(UTF8Encoding.UTF8.GetChars(ntpData));
            Close();
            byte offsetTransmitTime = 40;
            ulong intpart = 0;
            ulong fractpart = 0;

            for (int i = 0; i <= 3; i++)
                intpart = (intpart << 8) | ntpData[offsetTransmitTime + i];

            for (int i = 4; i <= 7; i++)
                fractpart = (fractpart << 8) | ntpData[offsetTransmitTime + i];

            ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);

            TimeSpan timeSpan = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
            DateTime dateTime = new DateTime(1900, 1, 1);
            dateTime += timeSpan;

            TimeSpan offsetAmount = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            DateTime networkDateTime = (dateTime + offsetAmount);
            SendCommand("set ip protocol 2");
        }

        public void Write(String response)
        {
            Send(response);
        }

        public void BinaryWrite(byte[] response, int start = 0, int length = -1)
        {
            WriteArray(response, start, length);
        }

        public void Close()
        {
            enterCommandMode();
            Send("close\r");
        }

        public String getIP()
        {
            String key = "IP=";
            leaveCommandMode();
            enterCommandMode();
            String result = SendCommand("get ip", key);
            int i = result.IndexOf(key);
            if (i != -1)
                return result.Substring(i + key.Length);
            else
                return null;
        }

        public void Listen()
        {
            StringBuilder line = new StringBuilder();
            Open();
            while (true)
            {
                while ((ReadRegister(WiflyRegister.LSR) & 0x01) > 0)
                {
                    char c = (char)ReadRegister(WiflyRegister.RHR);
                    line.Append(c);
                }
                String contents = line.ToString();
                if (contents.Length != 0)
                {
                    String[] lines = contents.Split('\n');
                    HttpRequest request = new HttpRequest();
                    new HttpRequestParser().parse(request, null, new HttpRequestLines(lines));
                    line.Clear();
                    if (request.RawUrl != null && request.RawUrl.Length != 0 && m_requestReceived != null)
                        m_requestReceived(new HttpContext(request, new HttpResponse(this)));
                }
            }
        }
        public void Dispose()
        {
        }
    }
}
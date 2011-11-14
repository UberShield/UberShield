﻿// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Net;
using Prototype.Xilinx;
using Prototype.Xilinx.Uploader.Arduino;

namespace SendBitstream
{
    class Program
    {
        private static Stream GetFileOrUrlStream(string filenameOrUrl)
        {
            // This function should accept the following formats:
            //
            // http://example.com/downloads/mybitstream.bit
            // mybitstream.bin
            // ..\..\mybitstream.bin
            // C:\dir\mybitstream.bin

            // If there is no colon, then it must be a file reference. Make it
            // a full path (factor out any ".." and stuff) and let Uri do the
            // rest.

            // If it has a colon, it's either a) a drive reference before it,
            // or b) a URL scheme before it. In both cases the Uri constructor
            // will do the right thing.

            if (!filenameOrUrl.Contains(":"))
            {
                filenameOrUrl = Path.GetFullPath(filenameOrUrl);
            }

            return WebRequest.Create(new Uri(filenameOrUrl)).GetResponse().GetResponseStream();
        }

        private static bool IsBitFile(string filename)
        {
            return (Path.GetExtension(filename).ToLower() == ".bit");
        }

        private class ProgramArguments
        {
            [Argument("help", "Print out help")]
            public bool PrintHelp { get; private set; }

            [Argument("port", "Select the COM port to use")]
            public string Port { get; private set; }

            [Argument("speed", "Select the COM port baud rate")]
            public int Speed { get; private set; }

            public ProgramArguments()
            {
                Port = "COM4";
                Speed = 115200;
            }
        }

        static void Main(string[] args)
        {
            var arguments = new ProgramArguments();

            args = Arguments.Parse(args, arguments);

            if (arguments.PrintHelp)
            {
                System.Console.Write(Arguments.GetDescriptionText(arguments));
                Environment.Exit(1);
            }

            ArduinoConnection arduinoConnection = new ArduinoConnection(arguments.Port, arguments.Speed);

            using (var inputStream = GetFileOrUrlStream(args[0]))
            {
                IEnumerable pageEnumerable = IsBitFile(args[0]) ?
                    (IEnumerable) new BitFilePageCollection(inputStream, Constants.UserStartAddress) :
                    (IEnumerable) new BinFilePageCollection(inputStream, Constants.UserStartAddress);
                arduinoConnection.UploadPages(pageEnumerable);
            }
        }
    }
}

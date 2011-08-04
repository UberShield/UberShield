using System;
using System.Text;
using Microsoft.SPOT;

namespace astra.http
{
    public class HttpUtils
    {
        public static int getExpandedStringLength(String format, Object[] args)
        {
            if (args != null && args.Length != 0)
            {
                int length = 0;
                int start = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    String tag = "{" + i + "}";
                    int stop = format.IndexOf(tag);
                    if (stop != -1)
                    {
                        //debug = format.Substring(start, stop - start);
                        length += (stop - start);
                        start = stop + tag.Length;
                        if (args[i] != null)
                            length += args[i].ToString().Length;
                        if (i == args.Length - 1)
                        {
                            //debug = format.Substring(start);
                            length += format.Length - start;
                        }
                    }
                }
                return length;
            }
            else
                return format.Length;
        }

        public static void expandString(HttpImplementation impl, String format, Object[] args)
        {
            if (args != null && args.Length != 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(format);
                int start = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    String tag = "{" + i + "}";
                    int stop = format.IndexOf(tag);
                    if (stop != -1)
                    {
                        impl.BinaryWrite(bytes, start, stop - start);
                        start = stop + tag.Length;
                        if (args[i] != null)
                            impl.Write(args[i].ToString());
                        if (i == args.Length - 1)
                        {
                            // Write the tail
                            impl.BinaryWrite(bytes, start, bytes.Length - start);
                        }
                    }
                }
            }
            else
                // No formatting
                impl.Write(format);
        }
    }
}

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO.Ports;
using MFToolkit.Net.Web;
using astra.http;
using UberShield.Cores;

public class Program
{
    HttpImplementation webServer;
    static SPI spi;
    static GpioPwm GP;
    byte currentPin = 0;
    static bool pwmRunning = false;

    public static void Main()
    {
        SPI.Configuration spiConfig = new SPI.Configuration(
            Pins.GPIO_PIN_D2,
            false,
            100,
            100,
            false,
            true,
            1000,
            SPI.SPI_module.SPI1
        );
        spi = new SPI(spiConfig);
        GP = new GpioPwm(spi);
        GP.PwmStop();
        byte channel;
        for (channel = 0; channel < 32; channel++)
        {
            GP.SetPwmParameter(channel, GpioPwm.PwmParameter.Rise, 0);
            GP.SetPwmParameter(channel, GpioPwm.PwmParameter.Fall, 0);
            GP.SetPwmParameter(channel, GpioPwm.PwmParameter.Period, 10000);
            GP.SetPinType(channel, GpioPwm.PinType.Input);
        }
        for (channel = 32; channel < 64; channel++)
        {
            GP.SetPinType(channel, GpioPwm.PinType.Input);
        }
        GP.SetGroupPin(GpioPwm.PinGroup.Lower, 0);
        GP.SetGroupPin(GpioPwm.PinGroup.Upper, 0);
        GP.PwmGo();
        pwmRunning = true;
        new Program();
    }

    public Program()
    {
        webServer = new HttpSocketImpl(processResponse);
        webServer.Listen();
    }

    /*
     * Indicate the names of the attributes for each http form
     */
    private readonly String[] formAttributesHome = { Names.pin, Names.pwmRise, Names.pwmFall, Names.pwmPeriod, Names.pwmRunning, Names.terminate, Names.pinType, Names.outputState };

    /*
     * The web server business logic
     */
    protected void processResponse(HttpContext context)
    {
        String content = "";
        String target = context.Request.Path;
        Boolean redirecting = false;
        Object[] args = null;

        if (target == Names.targetHome)
        {
            args = loadAttributes(formAttributesHome);
            content = Resources.formHome;
        }
        else if (target == Names.targetDoHome)
        {
            string verb = context.Request.getParameter(Names.formAction);
            if (verb == Names.formSet)
            {
                saveAttributes(formAttributesHome, context.Request);
            }
            if (verb == Names.formUpdate)
            {
                currentPin = byte.Parse(context.Request.getParameter(Names.pin));
            }
            if (verb == Names.formPwmState)
            {
                if (pwmRunning)
                {
                    pwmRunning = false;
                    GP.PwmStop();
                }
                else
                {
                    pwmRunning = true;
                    GP.PwmGo();
                }
            }
            if (verb == Names.formTerminate)
            {
                GP.SetTerminate(uint.Parse(context.Request.getParameter(Names.terminate)));
            }
            target = Names.targetHome;
            context.Response.setRedirect(target);
            redirecting = true;
        }
//        else if (target == "/favicon.ico")
//        {
//            context.Response.ContentType = "image/png";
//            context.Response.LastModified = "Tue, 8 Feb 2011 06:45:19 GMT";
//            context.Response.ContentLength = Resources.favIcon.Length;
//            context.Response.BinaryWrite(Resources.favIcon);
//            context.Response.Add("Connection", "close");
//            context.Response.Close();
//            return;
//        }
        int l = args == null ? content.Length : HttpUtils.getExpandedStringLength(content, args);
        context.Response.ContentType = "text/html";
        context.Response.Write(content, args);
        if (!redirecting)
        {
            context.Response.Add("Connection", "close");
            context.Response.Close();
        }
        redirecting = false;
    }

    /*
     * Save to storage the attributes, which <names> are provided, and which values are
     * to be retrieved from the http request <request>
     */
    protected void saveAttributes(String[] names, HttpRequest request)
    {
        byte pin=0;
        GpioPwm.PinType pinType=GpioPwm.PinType.Input;
        uint rise=0;
        uint fall=0;
        uint period=0;
        bool state = false;
        foreach (String name in names)
        {
            if (name == Names.pin)
            {
                string ret=HttpServerUtility.UrlEncode(request.getParameter(name));
                Debug.Print(ret);
                pin=byte.Parse(ret);
            }
            if (name == Names.pinType)
            {
                pinType = (GpioPwm.PinType)byte.Parse(HttpServerUtility.UrlEncode(request.getParameter(name)));
            }
            if (name == Names.pwmRise)
            {
                rise=uint.Parse(HttpServerUtility.UrlEncode(request.getParameter(name)));
            }
            if (name == Names.pwmFall)
            {
                fall=uint.Parse(HttpServerUtility.UrlEncode(request.getParameter(name)));
            }
            if (name == Names.pwmPeriod)
            {
                period=uint.Parse(HttpServerUtility.UrlEncode(request.getParameter(name)));
            }
            if (name == Names.outputState)
            {
                string result = HttpServerUtility.UrlEncode(request.getParameter(name));
                if (result == bool.TrueString)
                {
                    state = true;
                }
            }

        }
        GP.SetPinType(pin, pinType);
        GP.SetPwmParameter(pin,GpioPwm.PwmParameter.Rise,rise);
        GP.SetPwmParameter(pin,GpioPwm.PwmParameter.Fall,fall);
        GP.SetPwmParameter(pin,GpioPwm.PwmParameter.Period,period);
        GP.SetPin(pin, state);
    }
    protected String[] loadAttributes(String[] names)
    {
        String[] result = new String[names.Length];
        for (int i = 0; i < names.Length; i++)
            result[i] = loadAttribute(names[i]);
        return result;
    }

    /*
     * Load from storage the attribute, which <name> is provided
     */
    protected String loadAttribute(String name)
    {
        String value=null;

        if (name == Names.pin)
        {
            value = currentPin.ToString();
        }
        if (name == Names.pinType)
        {
            value = GP.GetPinType(currentPin).ToString();
        }
        if (name == Names.pwmRise)
        {
            value=GP.GetPwmParameter(currentPin, GpioPwm.PwmParameter.Rise).ToString();
        }
        if (name == Names.pwmFall)
        {
            value=GP.GetPwmParameter(currentPin, GpioPwm.PwmParameter.Fall).ToString();
        }
        if (name == Names.pwmPeriod)
        {
            value=GP.GetPwmParameter(currentPin, GpioPwm.PwmParameter.Period).ToString();
        }
        if (name == Names.outputState)
        {
            value = GP.GetPinState(currentPin).ToString();
        }
        if (name == Names.pwmRunning)
        {
            if (pwmRunning)
            {
                value = "PWM Running";
            }
            else
            {
                value = "PWM Stopped";
            }
        }
        if (name == Names.terminate)
        {
            value = GP.GetTerminate().ToString();
        }
        return value == null ? "" : HttpServerUtility.UrlDecode(value);
    }
}

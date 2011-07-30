using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using UberShield;
using UberShield.Cores;

namespace NetduinoApplication1
{
    public class Program
    {
        public enum PinType { PinInput = (byte)0, PinOutput = (byte)1, PinPwm = (byte)2, PinInvertedPwm = (byte)3 };
        public enum PwmParam { PwmRise = (byte)0, PwmFall = (byte)1, PwmPeriod = (byte)2 };
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
            var spi = new SPI(spiConfig);
            var GP = new GpioPwm(spi);
            GP.PwmStop();
            uint i;
            uint[] LedLevels = {0,10,41,93,166,260,374,509,665,842,1040,1259,1498,1758,2039,2340,2662,3006,3370,3755,4161,4587,5035,5503,5992,6502,7033,7584,8157,8750,9364,9999};

            for (i = 0; i < 16; i++)
            {
                GP.SetPinType((byte)(16 + i), GpioPwm.PinType.PinPwm);
                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmRise, 0);
                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmFall, LedLevels[0]);
                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmPeriod, 10000);
            }
            byte Inten=0;
            GP.PwmGo();
            while (true)
            {
                Thread.Sleep(10);
                Inten = (byte)(Inten==31 ? 0 : Inten+1);
                Debug.Print(Inten.ToString());
                for (i = 0; i < 16; i++)
                {
                    GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmFall, LedLevels[Inten]);
                }
            }
//            for (i = 0; i < 16; i++)
//            {
//                GP.SetPinType((byte)(16 + i), GpioPwm.PinType.PinPwm);
//                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmRise, 100000 + (i * 100000));
//                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmFall, 1700000);
//                GP.SetPwmParameter((byte)(16 + i), GpioPwm.PwmParam.PwmPeriod, 1700000);
//            }
            GP.SetTerminate(0);
            GP.PwmGo();
        }
    }
}

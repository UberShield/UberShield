using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace UberShield.Cores
{
    public class GpioPwm
    {
        public enum Cmd :byte
        {
            CmdRamRead=0,
            CmdRamWrite=1,
            CmdSetGreenLed=2,
            CmdSetRedLed=3,
            CmdGetButton=4,
            CmdPwmRun=5,
            CmdSetTerminate=6,
            CmdGetTerminate=7,
            CmdSetPinInput=8,
            CmdSetPinOutput=9,
            CmdSetPinPwm=10,
            CmdSetPinInvertedPwm=11,
            CmdGetPinMode=12,
            CmdSetPinStateLow=13,
            CmdSetPinStateHigh=14,
            CmdGetPinStateLow=17,
            CmdGetPinStateHigh=18,
            CmdGetPinInputLow=19,
            CmdGetPinInputHigh=20,
            CmdGetCoreId=255
        };
        public enum PinType :byte
        {
            PinInput=0,
            PinOutput=1,
            PinPwm=2,
            PinInvertedPwm=3
        };
        public enum PwmParam :byte
        {
            PwmRise=0,
            PwmFall=1,
            PwmPeriod=2
        };

        SPI Spi;

        public GpioPwm(SPI spiInt)
        {
            Spi = spiInt;
            if (!IsGpioPwmCore())
            {
                throw(new SystemException("Tried to instantiate a GpioPwm class, but no GPIO/PWM core running on UberShield."));
            }
        }

        public bool IsGpioPwmCore()
        {
            var TxBuffer = new byte[6];
            var RxBuffer = new byte[6];
            TxBuffer[0] = (byte)Cmd.CmdGetCoreId;
            TxBuffer[1] = 0x00;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = 0x00;
            TxBuffer[4] = 0x00;
            TxBuffer[5] = 0x00;
            Spi.WriteRead(TxBuffer, RxBuffer);
            if ((RxBuffer[2] == 0x00) & (RxBuffer[3] == 0x01))
            {
                return(true);
            }
            else
            {
                return(false);
            }
        }

        public void SetGreenLED(bool state)
        {
            byte[] WriteBuffer = new byte[2];
            WriteBuffer[0] = (byte)Cmd.CmdSetGreenLed; //Command
            WriteBuffer[1] = state?(byte)0x01:(byte)0x00; //Operand
            Spi.Write(WriteBuffer);
        }

        public void SetRedLED(bool state)
        {
            byte[] WriteBuffer = new byte[2];
            WriteBuffer[0] = (byte)Cmd.CmdSetRedLed; //Command
            WriteBuffer[1] = state ? (byte)0x01 : (byte)0x00; //Operand
            Spi.Write(WriteBuffer);
        }

        public void SetPinType(byte pin, PinType type)
        {
            byte[] WriteBuffer = new byte[2];
            switch (type)
            {
                case PinType.PinInput:
                    WriteBuffer[0] = (byte)Cmd.CmdSetPinInput;
                    break;
                case PinType.PinOutput:
                    WriteBuffer[0] = (byte)Cmd.CmdSetPinOutput;
                    break;
                case PinType.PinPwm:
                    WriteBuffer[0] = (byte)Cmd.CmdSetPinPwm;
                    break;
                case PinType.PinInvertedPwm:
                    WriteBuffer[0] = (byte)Cmd.CmdSetPinInvertedPwm;
                    break;
                default:
                    throw(new SystemException("SetPinType called with invalid pin type."));
                    break;
            }
            WriteBuffer[1] = pin; //Operand
            Spi.Write(WriteBuffer);
        }

        public void PwmGo()
        {
            byte[] WriteBuffer = new byte[2];
            WriteBuffer[0] = (byte)Cmd.CmdPwmRun; //Command
            WriteBuffer[1] = 0x01; //Operand
            Spi.Write(WriteBuffer);
        }

        public void PwmStop()
        {
            byte[] WriteBuffer = new byte[2];
            WriteBuffer[0] = (byte)Cmd.CmdPwmRun; //Command
            WriteBuffer[1] = 0x00; //Operand
            Spi.Write(WriteBuffer);
        }

        public void SetPinState(byte pin, bool state)
        {
            byte[] WriteBuffer = new byte[6];
            if (pin < 32)
            {
                if (state)
                {
                    WriteBuffer[0] = 0x0D;
                }
                else
                {
                    WriteBuffer[0] = 0x0F;
                }
            }
            else
            {
                if (state)
                {
                    WriteBuffer[0] = 0x0E;
                }
                else
                {
                    WriteBuffer[0] = 0x10;
                }
            }
            WriteBuffer[1] = 0x00; //Operand
            int Data = 0x01 << pin;
            WriteBuffer[2] = (byte)(Data >> 24 & 0xFF);
            WriteBuffer[3] = (byte)(Data >> 16 & 0xFF);
            WriteBuffer[4] = (byte)(Data >> 8 & 0xFF);
            WriteBuffer[5] = (byte)(Data & 0xFF);
            Spi.Write(WriteBuffer);
        }

        public void SetPwmParameter(byte channel, PwmParam param, uint data)
        {
            byte address = (byte)((channel << 2) + (byte)param);
            SetMemory(address, data);
        }

        public uint GetPwmParameter(byte channel, PwmParam param)
        {
            byte address = (byte)((channel << 2) + (byte)param);
            return GetMemory(address);
        }

        public void SetMemory(byte address, uint Data)
        {
            byte[] WriteBuffer = new byte[6];
            WriteBuffer[0] = (byte)Cmd.CmdRamWrite; //Operand
            WriteBuffer[1] = address; //Operand
            WriteBuffer[2] = (byte)(Data >> 24 & 0xFF);
            WriteBuffer[3] = (byte)(Data >> 16 & 0xFF);
            WriteBuffer[4] = (byte)(Data >> 8 & 0xFF);
            WriteBuffer[5] = (byte)(Data & 0xFF);
            Spi.Write(WriteBuffer);
        }

        public void SetTerminate(uint Data)
        {
            byte[] WriteBuffer = new byte[6];
            WriteBuffer[0] = (byte)Cmd.CmdSetTerminate; //Operand
            WriteBuffer[1] = 0x00; //Operand
            WriteBuffer[2] = (byte)(Data >> 24 & 0xFF);
            WriteBuffer[3] = (byte)(Data >> 16 & 0xFF);
            WriteBuffer[4] = (byte)(Data >> 8 & 0xFF);
            WriteBuffer[5] = (byte)(Data & 0xFF);
            Spi.Write(WriteBuffer);
        }

        public uint GetMemory(byte address)
        {
            uint RetVal;
            var TxBuffer = new byte[6];
            var RxBuffer = new byte[6];
            TxBuffer[0] = (byte)Cmd.CmdRamRead;
            TxBuffer[1] = address;
            TxBuffer[2] = 0x00;
            TxBuffer[3] = 0x00;
            TxBuffer[4] = 0x00;
            TxBuffer[5] = 0x00;
            Spi.WriteRead(TxBuffer, RxBuffer);
            RetVal = (uint)((RxBuffer[2] << 24) + (RxBuffer[3] << 16) + (RxBuffer[4] << 8) + (RxBuffer[5]));
            return RetVal;
        }
    }
}
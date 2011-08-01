using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace UberShield.Cores
{
    public class GpioPwm
    {
        public enum Command : byte
        {
            RamRead = 0,
            RamWrite = 1,
            SetGreenLed = 2,
            SetRedLed = 3,
            GetButton = 4,
            PwmRun = 5,
            SetTerminate = 6,
            GetTerminate = 7,
            SetPinInput = 8,
            SetPinOutput = 9,
            CmdSetPinPwm = 10,
            SetPinInvertedPwm = 11,
            GetPinMode = 12,
            SetPinStateLow = 13,
            SetPinStateHigh = 14,
            GetPinStateLow = 17,
            GetPinStateHigh = 18,
            GetPinInputLow = 19,
            GetPinInputHigh = 20,
            GetCoreId = 255
        };
        public enum PinType : byte
        {
            Input = 0,
            Output = 1,
            Pwm = 2,
            InvertedPwm = 3
        };
        public enum PwmParameter : byte
        {
            Rise = 0,
            Fall = 1,
            Period = 2
        };
        public enum PinGroup : byte
        {
            Lower = 0,
            Upper = 1,
        };

        SPI spi;

        public GpioPwm(SPI spi)
        {
            this.spi = spi;
            if (!IsGpioPwmCore())
            {
                throw (new SystemException("Tried to instantiate a GpioPwm class, but no GPIO/PWM core running on UberShield."));
            }
        }

        public bool GetButton()
        {
            byte[] txBuffer = new byte[6];
            byte[] rxBuffer = new byte[6];
            bool buttonState;
            txBuffer[0] = (byte)Command.GetButton;
            txBuffer[1] = 0x00;
            txBuffer[2] = 0x00;
            txBuffer[3] = 0x00;
            txBuffer[4] = 0x00;
            txBuffer[5] = 0x00;
            spi.WriteRead(txBuffer, rxBuffer);
            buttonState = (rxBuffer[5] == 0x01) ? true : false;
            return buttonState;
        }

        public bool IsGpioPwmCore()
        {
            byte[] txBuffer = new byte[6];
            byte[] rxBuffer = new byte[6];
            txBuffer[0] = (byte)Command.GetCoreId;
            txBuffer[1] = 0x00;
            txBuffer[2] = 0x00;
            txBuffer[3] = 0x00;
            txBuffer[4] = 0x00;
            txBuffer[5] = 0x00;
            spi.WriteRead(txBuffer, rxBuffer);
            if ((rxBuffer[2] == 0x00) & (rxBuffer[3] == 0x01))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public void SetGreenLED(bool state)
        {
            byte[] txBuffer = new byte[2];
            txBuffer[0] = (byte)Command.SetGreenLed; //Command
            txBuffer[1] = state ? (byte)0x01 : (byte)0x00; //Operand
            spi.Write(txBuffer);
        }

        public void SetRedLED(bool state)
        {
            byte[] txBuffer = new byte[2];
            txBuffer[0] = (byte)Command.SetRedLed; //Command
            txBuffer[1] = state ? (byte)0x01 : (byte)0x00; //Operand
            spi.Write(txBuffer);
        }

        public void SetPinType(byte pin, PinType type)
        {
            byte[] txBuffer = new byte[2];
            switch (type)
            {
                case PinType.Input:
                    txBuffer[0] = (byte)Command.SetPinInput;
                    break;
                case PinType.Output:
                    txBuffer[0] = (byte)Command.SetPinOutput;
                    break;
                case PinType.Pwm:
                    txBuffer[0] = (byte)Command.CmdSetPinPwm;
                    break;
                case PinType.InvertedPwm:
                    txBuffer[0] = (byte)Command.SetPinInvertedPwm;
                    break;
                default:
                    throw (new SystemException("SetPinType called with invalid pin type."));
             }
            txBuffer[1] = pin; //Operand
            spi.Write(txBuffer);
        }

        public void PwmGo()
        {
            byte[] txBuffer = new byte[2];
            txBuffer[0] = (byte)Command.PwmRun; //Command
            txBuffer[1] = 0x01; //Operand
            spi.Write(txBuffer);
        }

        public void PwmStop()
        {
            byte[] txBuffer = new byte[2];
            txBuffer[0] = (byte)Command.PwmRun; //Command
            txBuffer[1] = 0x00; //Operand
            spi.Write(txBuffer);
        }

        public void SetPin(byte pin, bool state)
        {
            PinGroup group;
            uint groupState;
            byte maskedPin = (byte)(pin & 31); //The bottom six bits of the pin number
            group = (pin >= 32) ? PinGroup.Upper : PinGroup.Lower; // Establish which group the pin is in
            groupState = GetGroupPinState(group); //Read the present state
            if (state) //Modify the state
            {
                groupState |= (uint)(0x01 << maskedPin);
            }
            else
            {
                groupState &= ~(uint)(0x01 << maskedPin);
            }
            SetGroupPin(group, groupState); //Write the state
        }

        public bool GetPinState(byte pin)
        {
            PinGroup group;
            uint groupState;
            bool state;
            byte maskedPin = (byte)(pin & 31); //The bottom six bits of the pin number
            group = (pin >= 32) ? PinGroup.Upper : PinGroup.Lower; // Establish which group the pin is in
            groupState = GetGroupPinState(group);
            groupState = (uint)(groupState&(0x01<<maskedPin));
            state = (groupState>0) ? true : false;
            return state;
        }
            
        public void SetGroupPin(PinGroup group, uint state)
        {
            byte readCommand;
            var txBuffer = new byte[6];
            var rxBuffer = new byte[6];
            if (group==PinGroup.Lower)
            {
                readCommand = (byte)Command.SetPinStateLow;
            }
            else
            {
                readCommand = (byte)Command.SetPinStateHigh;
            }
            txBuffer[0] = readCommand;
            txBuffer[2] = (byte)(state >> 24 & 0xFF);
            txBuffer[3] = (byte)(state >> 16 & 0xFF);
            txBuffer[4] = (byte)(state >> 8 & 0xFF);
            txBuffer[5] = (byte)(state & 0xFF);
            spi.Write(txBuffer);
        }

        public uint GetGroupPinState(PinGroup group)
        {
            byte readCommand;
            uint RetVal;
            var txBuffer = new byte[6];
            var rxBuffer = new byte[6];
            if (group==PinGroup.Lower)
            {
                readCommand = (byte)Command.GetPinStateLow;
            }
            else
            {
                readCommand = (byte)Command.GetPinStateHigh;
            }
            txBuffer[0] = readCommand;
            txBuffer[1] = 0x00;
            txBuffer[2] = 0x00;
            txBuffer[3] = 0x00;
            txBuffer[4] = 0x00;
            txBuffer[5] = 0x00;
            spi.WriteRead(txBuffer, rxBuffer);
            RetVal = (uint)((rxBuffer[2] << 24) + (rxBuffer[3] << 16) + (rxBuffer[4] << 8) + (rxBuffer[5]));
            return RetVal;
        }

        public bool GetPin(byte pin)
        {
            PinGroup group;
            uint groupState;
            bool state;
            byte maskedPin = (byte)(pin & 31); //The bottom six bits of the pin number
            group = (pin >= 32) ? PinGroup.Upper : PinGroup.Lower; // Establish which group the pin is in
            groupState = GetGroupPin(group);
            groupState = (uint)(groupState & (0x01 << maskedPin));
            state = (groupState > 0) ? true : false;
            return state;
        }

        public uint GetGroupPin(PinGroup group)
        {
            byte readCommand;
            uint RetVal;
            var txBuffer = new byte[6];
            var rxBuffer = new byte[6];
            if (group == PinGroup.Lower)
            {
                readCommand = (byte)Command.GetPinInputLow;
            }
            else
            {
                readCommand = (byte)Command.GetPinInputHigh;
            }
            txBuffer[0] = readCommand;
            txBuffer[1] = 0x00;
            txBuffer[2] = 0x00;
            txBuffer[3] = 0x00;
            txBuffer[4] = 0x00;
            txBuffer[5] = 0x00;
            spi.WriteRead(txBuffer, rxBuffer);
            RetVal = (uint)((rxBuffer[2] << 24) + (rxBuffer[3] << 16) + (rxBuffer[4] << 8) + (rxBuffer[5]));
            return RetVal;
        }

        public void SetPwmParameter(byte channel, PwmParameter parameter, uint data)
        {
            byte address = (byte)((channel << 2) + (byte)parameter);
            SetMemory(address, data);
        }

        public uint GetPwmParameter(byte channel, PwmParameter parameter)
        {
            byte address = (byte)((channel << 2) + (byte)parameter);
            return GetMemory(address);
        }

        public void SetMemory(byte address, uint data)
        {
            byte[] txBuffer = new byte[6];
            txBuffer[0] = (byte)Command.RamWrite; //Operand
            txBuffer[1] = address; //Operand
            txBuffer[2] = (byte)(data >> 24 & 0xFF);
            txBuffer[3] = (byte)(data >> 16 & 0xFF);
            txBuffer[4] = (byte)(data >> 8 & 0xFF);
            txBuffer[5] = (byte)(data & 0xFF);
            spi.Write(txBuffer);
        }

        public void SetTerminate(uint data)
        {
            byte[] txBuffer = new byte[6];
            txBuffer[0] = (byte)Command.SetTerminate; //Operand
            txBuffer[1] = 0x00; //Operand
            txBuffer[2] = (byte)(data >> 24 & 0xFF);
            txBuffer[3] = (byte)(data >> 16 & 0xFF);
            txBuffer[4] = (byte)(data >> 8 & 0xFF);
            txBuffer[5] = (byte)(data & 0xFF);
            spi.Write(txBuffer);
        }

        public uint GetMemory(byte address)
        {
            uint RetVal;
            var txBuffer = new byte[6];
            var rxBuffer = new byte[6];
            txBuffer[0] = (byte)Command.RamRead;
            txBuffer[1] = address;
            txBuffer[2] = 0x00;
            txBuffer[3] = 0x00;
            txBuffer[4] = 0x00;
            txBuffer[5] = 0x00;
            spi.WriteRead(txBuffer, rxBuffer);
            RetVal = (uint)((rxBuffer[2] << 24) + (rxBuffer[3] << 16) + (rxBuffer[4] << 8) + (rxBuffer[5]));
            return RetVal;
        }
    }
}
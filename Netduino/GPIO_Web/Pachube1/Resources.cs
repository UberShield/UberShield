using System;
using UberShield.Cores;

class Resources
{
/*    public static byte[] favIcon = {
 0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,
 0x49,0x48,0x44,0x52,0x00,0x00,0x00,0x30,0x00,0x00,0x00,0x30,
 0x08,0x02,0x00,0x00,0x00,0xD8,0x60,0x6E,0xD0,0x00,0x00,0x00,
 0x01,0x73,0x52,0x47,0x42,0x00,0xAE,0xCE,0x1C,0xE9,0x00,0x00,
 0x00,0x04,0x67,0x41,0x4D,0x41,0x00,0x00,0xB1,0x8F,0x0B,0xFC,
 0x61,0x05,0x00,0x00,0x00,0x09,0x70,0x48,0x59,0x73,0x00,0x00,
 0x0E,0xC3,0x00,0x00,0x0E,0xC3,0x01,0xC7,0x6F,0xA8,0x64,0x00,
 0x00,0x01,0x73,0x49,0x44,0x41,0x54,0x58,0x47,0xED,0x97,0xB1,
 0x0D,0x83,0x30,0x10,0x45,0x59,0x24,0x6B,0x64,0x08,0xCA,0xF4,
 0x2C,0x11,0x16,0x60,0x04,0x4A,0x6A,0x7A,0x26,0xA0,0xA2,0x64,
 0x09,0x24,0x24,0xF6,0x70,0x2C,0x59,0x4A,0xE1,0xBB,0xF3,0x9D,
 0x63,0x6C,0x83,0xE2,0xC8,0x95,0x45,0xE0,0xF3,0xDF,0xDD,0xE7,
 0x5C,0x3D,0x9E,0xAF,0x4B,0xAD,0xEA,0x52,0x6A,0xB4,0x98,0x22,
 0x88,0xAB,0x90,0xE2,0xD0,0x7F,0x3A,0xD4,0xCF,0xCA,0xFA,0x1D,
 0x43,0xA3,0xD3,0x84,0xDA,0x77,0x05,0xCD,0x09,0x35,0x54,0x8F,
 0x87,0x25,0x67,0x1B,0xDF,0xBA,0x81,0xA9,0x7D,0x77,0xD0,0x84,
 0x0B,0x7A,0x0F,0x3B,0x6A,0x0F,0xB5,0xCF,0xE4,0x70,0xB0,0xA0,
 0x66,0xDA,0x2C,0x3D,0xFB,0x54,0xEB,0xCA,0xA5,0xF6,0x63,0x17,
 0xF5,0xB9,0xBC,0x4A,0x52,0x73,0xBC,0xEE,0xEB,0x50,0xB7,0xDA,
 0x41,0xA3,0x94,0xE9,0x6D,0xE9,0x82,0x35,0xAE,0xEF,0xB8,0xF4,
 0xF0,0xEF,0xA2,0x2E,0x6B,0x17,0xA8,0x67,0x6D,0xE5,0x6A,0xB0,
 0x4C,0x52,0xCA,0x84,0xA7,0xBD,0x24,0x82,0x60,0xE0,0xE2,0x2F,
 0x47,0xBB,0x05,0x33,0x49,0x29,0x93,0x0E,0xBF,0x08,0xC2,0x78,
 0xCD,0x9D,0x18,0x96,0x7E,0xA4,0x0F,0x71,0xDE,0xA1,0x70,0x5E,
 0xD8,0x1D,0x70,0x5E,0x92,0x2E,0x0B,0xE7,0x85,0xDD,0x81,0xE0,
 0x25,0x10,0x94,0x96,0x17,0x2F,0x28,0x31,0x2F,0x56,0x50,0x6A,
 0x5E,0x9C,0xA0,0xE4,0xBC,0xDC,0x82,0xB0,0xF0,0x50,0x7E,0x79,
 0xE8,0xD5,0x5F,0x26,0x93,0xE8,0xB6,0x17,0x87,0x3D,0x9D,0x87,
 0x7E,0xFD,0xC5,0x08,0x82,0x83,0x8E,0xFE,0x7C,0xC4,0xCB,0xC3,
 0xEF,0x5B,0x51,0x0E,0xE5,0xE1,0x45,0x23,0xCB,0xC4,0x8B,0x14,
 0x94,0x8B,0x17,0x25,0x28,0x12,0x2F,0x51,0x87,0x62,0x35,0x14,
 0x89,0x17,0x36,0x8E,0x89,0xC6,0x8F,0x48,0xBC,0x84,0x1D,0x0A,
 0x1D,0xCA,0xC9,0x0B,0xAD,0x21,0x2C,0xCD,0xE0,0x04,0xEB,0xBB,
 0x23,0xE3,0x85,0x09,0xC2,0xBE,0x5F,0xBE,0x4F,0x07,0xD7,0x93,
 0xE3,0x98,0xA0,0x86,0x22,0x08,0xF2,0x3A,0x9F,0x80,0x1A,0x42,
 0x5B,0x2C,0xC4,0x22,0x31,0x2C,0xC7,0xB7,0xEC,0xA4,0x32,0xA2,
 0xE7,0x54,0xC7,0x69,0x8E,0x1F,0xF2,0xA5,0x47,0x41,0x9F,0x63,
 0x5A,0x11,0x14,0xE0,0x56,0x41,0xC6,0x99,0x57,0x1C,0xBA,0x9D,
 0x43,0x1F,0x8F,0x45,0x19,0xC8,0xA8,0x96,0x95,0xAF,0x00,0x00,
 0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82 
};
*/
    public static readonly String formHome =
        "<html><body style='font-family:verdana'>" +
            "<form method=post action='" + Names.targetDoHome + "'>" +
                "<table style='font-size:10' border=0>" +
                "<tr>" +
                    "<td>Pin number</td>" +
                    "<td>" +
                    "<input type=text length=2 name='" + Names.pin + "' value='{0}' onblur='{this.form.action = \"" + Names.targetDoHome + "?" + Names.formAction + "=" + Names.formUpdate + "\"; this.form.submit();}'>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>Pin type</td>" +
                    "<td>" +
                    "<select name='" + Names.pinType + "'>" +
                        "<option id='type" + GpioPwm.PinType.Input + "' value=0>Input</option>" +
                        "<option id='type" + GpioPwm.PinType.Output + "' value=1>Output</option>" +
                        "<option id='type" + GpioPwm.PinType.Pwm + "' value=2>PWM</option>" +
                        "<option id='type" + GpioPwm.PinType.InvertedPwm + "' value=3>Inverted PWM</option>" +
                    "</select>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>Rise time</td>" +
                    "<td>" +
                    "<input type=text length=60 name='" + Names.pwmRise + "' value='{1}'>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>Fall time</td>" +
                    "<td>" +
                    "<input type=text length=60 name='" + Names.pwmFall + "' value='{2}'>" +
//                    "<input type=range min='0' max='9999' name='" + Names.pwmFall + "' value='{2}'>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>Period</td>" +
                    "<td>" +
                    "<input type=text length=60 name='" + Names.pwmPeriod + "' value='{3}'>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>Output State</td>" +
                    "<td>" +
                    "<input id='state" + bool.TrueString + "' type='radio' name='" + Names.outputState + "' value='" + bool.TrueString + "' /> High" +
                    "<input id='state" + bool.FalseString + "' type='radio' name='" + Names.outputState + "' value='" + bool.FalseString + "' /> Low" +
                    "</td>" +
                    "<script language='javascript' type='text/javascript'>" +
                    "{" +
                    "}" +
                    "</script>" +
                "</tr>" +
                "<tr>" +
                    "<td colspan=2>" +
                    "<input type='submit' name='" + Names.formAction + "' value='" + Names.formSet + "'>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>" +
                    "<button type='button' name='" + Names.pwmRunning + "' onclick='{this.form.action = \"" + Names.targetDoHome + "?" + Names.formAction + "=" + Names.formPwmState + "\"; this.form.submit();}'>{4}</button>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td>" +
                    "<input type=text length=60 name='" + Names.terminate + "' value='{5}'>" +
                    "</td>" +
                    "<td>" +
                    "<button type='button' onclick='{this.form.action = \"" + Names.targetDoHome + "?" + Names.formAction + "=" + Names.formTerminate + "\"; this.form.submit();}'>Set Terminate</button>" +
                    "</td>" +
                "</tr>" +
                    "<script language='javascript' type='text/javascript'>" +
                    "{" +
                        "var T = document.getElementById('type{6}');" +
                        "T.selected = true;" +
                        "T = document.getElementById('state{7}');" +
                        "T.checked = true;" +
                    "}" +
                    "</script>" +
                "</table>" +
                "</form>" +
            "</body></html>";
}

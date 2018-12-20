using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.DirectInput;
using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi
{
    public class Joystick
    {
        public Device Device { get; set; }
        public SysKey JoystickKey { get; set; }
        public bool JoystickPushed { get; set; }
    }
}

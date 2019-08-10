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
        public SysKey? JoystickKey { get; set; }
        public bool JoystickPushed { get; set; }

        public void GetJoystickState() {
            // デバイス未決定時は何もしない
            if (Device == null) {
                return;
            }

            try {
                // コントローラの状態をポーリングで取得
                Device.Poll();
                JoystickState state = Device.CurrentJoystickState;

                int count = 0;
                bool pushed = false;
                foreach (byte button in state.GetButtons()) {
                    if (count++ >= Device.Caps.NumberButtons) {
                        break;
                    }
                    if (button >= 100) {
                        if (JoystickKey != null && !JoystickPushed) {
                            System.Windows.Forms.SendKeys.SendWait(JoystickKey.ToString());
                        }
                        pushed = true;
                    }
                }
                JoystickPushed = pushed;

            } catch (Exception ex) {
                Console.WriteLine(ex.Message + Environment.NewLine);
            }
        }
    }
}

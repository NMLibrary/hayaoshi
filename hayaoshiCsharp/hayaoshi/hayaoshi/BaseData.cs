using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi {

    //MainWindowで最初に1つだけ作るクラス
    public class BaseData {

        public static SolidColorBrush backGroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x11, 0x11, 0x11));
        //public static SolidColorBrush backGroundColor = Brushes.Red;
        public static SolidColorBrush foreGroundColor = Brushes.WhiteSmoke;

        public Joystick[] Joysticks { get; set; }
        public int PlayerNumber { get; set; } = 4;
        public Dictionary<Joystick, int> JoyToNumDic { get; set; }
        public Dictionary<int, Joystick> NumToJoyDic { get; set; }
        public Dictionary<SysKey, int> KeyToNumDic { get; set; }
        public Dictionary<int, SysKey> NumToKeyDic { get; set; }

        public void JoystickSetup() {
            JoyToNumDic = new Dictionary<Joystick, int>();
            NumToJoyDic = new Dictionary<int, Joystick>();
            KeyToNumDic = new Dictionary<SysKey, int>();
            NumToKeyDic = new Dictionary<int, SysKey>();

            DeviceList devList;
            devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            int joystickCount = devList.Count;
            Joysticks = new Joystick[joystickCount];
            SysKey[] buttons = new SysKey[4] { SysKey.D2, SysKey.T, SysKey.K, SysKey.OemBackslash };

            int count = 0;
            foreach (DeviceInstance dev in devList) {
                Joysticks[count] = new Joystick();
                Joysticks[count].Device = new Device(dev.InstanceGuid);
                //joystick.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                //break;
                count++;
            }

            for (int i = 0; i < joystickCount; i++) {
                Joysticks[i].Device.SetDataFormat(DeviceDataFormat.Joystick);
                Joysticks[i].Device.Acquire();
                Joysticks[i].JoystickPushed = false;

                SysKey? key = null;
                if (i < 4) {
                    key = buttons[i];
                }
                //} else {
                //    key = SysKey.D2;
                //}
                Joysticks[i].JoystickKey = key;

                if (Joysticks[i] != null) {
                    JoyToNumDic.Add(Joysticks[i], i);
                    NumToJoyDic.Add(i, Joysticks[i]);
                }
            }

            for (int i = 0; i < PlayerNumber; i++) {
                if (i < 4) {
                    KeyToNumDic.Add(buttons[i], i);
                    NumToKeyDic.Add(i, buttons[i]);
                }
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(1);
            timer.Start();
            timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e) {
            DispatcherTimer timer = sender as DispatcherTimer;
            foreach (Joystick joystick in Joysticks) {
                joystick.GetJoystickState();
            }
        }

        public static void LabelAdd(ref Grid grid, ref Label label, string content, int row, int column,
    int rowspan = 1, int columnspan = 1, FontFamily font = null, SolidColorBrush color = null) {
            Viewbox box = new Viewbox();
            box.SetValue(Grid.RowProperty, row);
            box.SetValue(Grid.RowSpanProperty, rowspan);
            box.SetValue(Grid.ColumnProperty, column);
            box.SetValue(Grid.ColumnSpanProperty, columnspan);
            label = new Label();
            if (font != null) {
                label.FontFamily = font;
            }
            if (color == null) {
                label.Foreground = BaseData.foreGroundColor;
            } else {
                label.Foreground = color;
            }
            label.Content = content;
            box.Child = label;
            grid.Children.Add(box);
        }
    } 
}

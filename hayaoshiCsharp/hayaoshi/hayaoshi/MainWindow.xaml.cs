using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.DirectX.DirectInput;
using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //public Device[] Joystick { get; set; }
        //public List<Device> AssignedJoystick { get; set; }
        //public SysKey[] JoystickKey { get; set; }
        //public bool[] JoystickPushed { get; set; }
        public Joystick[] Joysticks { get; set; }
        public MainWindow()
        {
            InitializeComponent();

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

                if (i < 4) {
                    Joysticks[i].JoystickKey = buttons[i];
                } else {
                    Joysticks[i].JoystickKey = SysKey.D2;
                }
                Joysticks[i].JoystickPushed = false;
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(1);
            timer.Start();
            timer.Tick += TimerTick;
        }

        private void GetJoystickState(Joystick joystick) {
            // デバイス未決定時は何もしない
            if (joystick.Device == null) {
                return;
            }

            try {
                // コントローラの状態をポーリングで取得
                joystick.Device.Poll();
                JoystickState state = joystick.Device.CurrentJoystickState;

                int count = 0;
                bool pushed = false;
                foreach (byte button in state.GetButtons()) {
                    if (count++ >= joystick.Device.Caps.NumberButtons) {
                        break;
                    }
                    if (button >= 100) {
                        System.Windows.Forms.SendKeys.SendWait(joystick.JoystickKey.ToString());
                        pushed = true;
                    }
                }
                joystick.JoystickPushed = pushed;

            } catch (Exception ex) {
                Console.WriteLine(ex.Message + Environment.NewLine);
            }
        }

        private void TimerTick(object sender, EventArgs e) {
            DispatcherTimer timer = sender as DispatcherTimer;
            foreach (Joystick joystick in Joysticks) {
                GetJoystickState(joystick);
            }
        }

        private void Yomiage(object sender, RoutedEventArgs e)
        {
            Hayaoshi child = new Hayaoshi(this);
            child.Show();
        }
    }
}

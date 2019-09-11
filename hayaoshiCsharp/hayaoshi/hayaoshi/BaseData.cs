using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using FontFamily = System.Windows.Media.FontFamily;
using System.IO;

namespace hayaoshi {

    //MainWindowで最初に1つだけ作るクラス
    public class BaseData {

        public static SolidColorBrush backGroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x11, 0x11, 0x11));
        //public static SolidColorBrush backGroundColor = Brushes.Red;
        public static SolidColorBrush foreGroundColor = Brushes.WhiteSmoke;
        public static SolidColorBrush nameAndLightColor = Brushes.Green;
        public static SolidColorBrush winColor = Brushes.Goldenrod;
        public static SolidColorBrush loseColor = Brushes.Gray;
        public static SolidColorBrush pointColor = Brushes.Red;
        public static SolidColorBrush mistakeColor = Brushes.CornflowerBlue;
        public static Dictionary<string, SysKey> judgingButton = new Dictionary<string, SysKey>{
            ["ok"] = SysKey.O,
            ["ng"] = SysKey.X
        };

        public Joystick[] Joysticks { get; set; }
        public int PlayerNumber { get; set; } = 4;
        public Dictionary<Joystick, int> JoyToNumDic { get; set; }
        public Dictionary<int, Joystick> NumToJoyDic { get; set; }
        public Dictionary<SysKey, int> KeyToNumDic { get; set; }
        public Dictionary<int, SysKey> NumToKeyDic { get; set; }
        public Dictionary<int, string> PlayerNameDic { get; set; }
        public DispatcherTimer Timer { get; set; }
        public int WinPoints { get; set; } = 7;
        public int LoseMistakes { get; set; } = 3;

        public List<string> QuestionSounds { get; set; } = new List<string>();
        public List<(string sentence, string answer)> QuestionStrings { get; set; }
            = new List<(string sentence, string answer)>();
        private Random random = new Random();

        public void JoystickSetup() {
            JoyToNumDic = new Dictionary<Joystick, int>();
            NumToJoyDic = new Dictionary<int, Joystick>();
            KeyToNumDic = new Dictionary<SysKey, int>();
            NumToKeyDic = new Dictionary<int, SysKey>();
            PlayerNameDic = new Dictionary<int, string>();

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

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromTicks(1);
            Timer.Start();
            Timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e) {
            //DispatcherTimer timer = sender as DispatcherTimer;
            List<SysKey> pushedJoyKeys = new List<SysKey>();
            foreach (Joystick joystick in Joysticks) {
                bool pushed = joystick.GetJoystickState();
                if (pushed && joystick.JoystickKey != null) {
                    pushedJoyKeys.Add((SysKey)joystick.JoystickKey);
                }
            }
            int len = pushedJoyKeys.Count();
            if (len > 0) {
                int number = random.Next(len);
                System.Windows.Forms.SendKeys.SendWait(pushedJoyKeys[number].ToString());
            }
        }

        public static void LabelAdd(ref Grid grid, ref Label label, string content, int row, int column,
            string name = "", int rowspan = 1, int columnspan = 1, FontFamily font = null, SolidColorBrush color = null,
            int ZIndex = 1) {
            Viewbox box = new Viewbox();
            box.SetValue(Grid.RowProperty, row);
            box.SetValue(Grid.RowSpanProperty, rowspan);
            box.SetValue(Grid.ColumnProperty, column);
            box.SetValue(Grid.ColumnSpanProperty, columnspan);
            box.SetValue(Grid.ZIndexProperty, ZIndex);
            label = new Label();
            label.Name = name;
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

        public static void LabelAddWithoutViewbox(ref Grid grid, ref Label label, string content,
            int row, int column, string name = "", int rowspan = 1, int columnspan = 1,
            FontFamily font = null, SolidColorBrush color = null, int ZIndex = 1) {
            label = new Label();
            label.Name = name;
            label.SetValue(Grid.RowProperty, row);
            label.SetValue(Grid.RowSpanProperty, rowspan);
            label.SetValue(Grid.ColumnProperty, column);
            label.SetValue(Grid.ColumnSpanProperty, columnspan);
            label.SetValue(Grid.ZIndexProperty, ZIndex);
            if (font != null) {
                label.FontFamily = font;
            }
            if (color == null) {
                label.Foreground = BaseData.foreGroundColor;
            } else {
                label.Foreground = color;
            }
            label.Content = content;
            grid.Children.Add(label);
        }

        public static void ButtonAdd(ref Grid grid, ref Button button, RoutedEventHandler method,
            string name, string content, int row, int column, int rowspan = 1, int columnspan = 1) {
            button = new Button();
            button.Name = name;
            button.SetValue(Grid.RowProperty, row);
            button.SetValue(Grid.ColumnProperty, column);
            button.SetValue(Grid.RowSpanProperty, rowspan);
            button.SetValue(Grid.ColumnSpanProperty, columnspan);
            button.Click += method;
            grid.Children.Add(button);
            Viewbox box = new Viewbox();
            Label label = new Label();
            label.Content = content;
            button.Content = box;
            box.Child = label;
        }

        //上のButtonAddで作成したボタンの文字を変える(普通のボタンには使えないので注意)
        public static void ChangeButtonContent(ref Button button, string text) {
            Label label = (Label)((Viewbox)button.Content).Child;
            label.Content = text;
        }

        public static void TextBoxAdd(ref Grid grid, ref TextBox textBox, TextChangedEventHandler method,
    string name, string text, int row, int column, int rowspan = 1, int columnspan = 1) {
            textBox = new TextBox();
            textBox.Name = name;
            textBox.SetValue(Grid.RowProperty, row);
            textBox.SetValue(Grid.ColumnProperty, column);
            textBox.SetValue(Grid.RowSpanProperty, rowspan);
            textBox.SetValue(Grid.ColumnSpanProperty, columnspan);
            textBox.Text = text;
            textBox.TextChanged += method;
            grid.Children.Add(textBox);
            Viewbox box = new Viewbox();
            //Label label = new Label();
            //label.Content = content;
            //textBox.Content = textBox;
            //textBox.Child = label;
        }

        public static SolidColorBrush ColorGradation(int index, int wholenumber) {
            //Color colorOne = Color.FromScRgb((float)1.0, (float)1.0, 0, 0);
            //Color colorTwo = Color.FromScRgb((float)1.0, 0, 0, (float)1.0);
            //float div = wholenumber - 1;
            //float a = colorOne.ScA / div * (div - index) + colorTwo.ScA / div * index;
            //float r = colorOne.ScR / div * (div - index) + colorTwo.ScR / div * index;
            //float g = colorOne.ScG / div * (div - index) + colorTwo.ScG / div * index;
            //float b = colorOne.ScB / div * (div - index) + colorTwo.ScB / div * index;

            Color c = HSLToRGB((float)360 / wholenumber * index, (float)0.5, (float)0.5);
            
            return new SolidColorBrush(c);
        }

        public static Color HSLToRGB(float h, float s, float l) {
            float r1, g1, b1;
            if (s == 0) {
                r1 = l;
                g1 = l;
                b1 = l;
            } else {
                float difh = h / 60f;
                int i = (int)Math.Floor(difh);
                float f = difh - i;
                //float c = (1f - Math.Abs(2f * l - 1f)) * s;
                float c;
                if (l < 0.5f) {
                    c = 2f * s * l;
                } else {
                    c = 2f * s * (1f - l);
                }
                float m = l - c / 2f;
                float p = c + m;
                //float x = c * (1f - Math.Abs(h % 2f - 1f));
                float q; // q = x + m
                if (i % 2 == 0) {
                    q = l + c * (f - 0.5f);
                } else {
                    q = l - c * (f - 0.5f);
                }

                switch (i) {
                    case 0:
                        r1 = p;
                        g1 = q;
                        b1 = m;
                        break;
                    case 1:
                        r1 = q;
                        g1 = p;
                        b1 = m;
                        break;
                    case 2:
                        r1 = m;
                        g1 = p;
                        b1 = q;
                        break;
                    case 3:
                        r1 = m;
                        g1 = q;
                        b1 = p;
                        break;
                    case 4:
                        r1 = q;
                        g1 = m;
                        b1 = p;
                        break;
                    case 5:
                        r1 = p;
                        g1 = m;
                        b1 = q;
                        break;
                    default:
                        throw new ArgumentException(
                            "色相の値が不正です。", "hsl");
                }
            }

            return Color.FromArgb(
                255,
                (byte)Math.Round(r1 * 255f),
                (byte)Math.Round(g1 * 255f),
                (byte)Math.Round(b1 * 255f));
        }

        public bool AddQuestions(string path) {
            using (Stream fileStream = new FileStream(path, FileMode.Open)) {
                IWorkbook workbook = new XSSFWorkbook(fileStream);
                ISheet sheet = workbook.GetSheetAt(0);
                for (int i = 1; i <= sheet.LastRowNum; i++) {
                    IRow row = sheet.GetRow(i);
                    string question = row.GetCell(1).ToString();
                    string answer = row.GetCell(2).ToString();
                    if (question == "") {
                        break;
                    }
                    QuestionStrings.Add((question, answer));
                }
            }
            return true;
        }
    } 
}

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
    public partial class MainWindow : Window {
        BaseData baseData;

        public MainWindow()
        {
            InitializeComponent();

            //baseDataを作成する(最初)
            baseData = new BaseData();
            baseData.JoystickSetup();

            MakeWindow();
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;
            PlayerNumberLabel.Content = baseData.PlayerNumber.ToString();
        }

        private void Yomiage(object sender, RoutedEventArgs e)
        {
            Hayaoshi child = new Hayaoshi(baseData);
            child.Show();
        }

        private void KeyConfig(object sender, RoutedEventArgs e) {
            KeyConfig child = new KeyConfig(baseData);
            child.Show();
        }

        private void PlayerNumberChange(object sender, RoutedEventArgs e) {
            if (((System.Windows.Controls.Button)sender).Name == "PlayerNumberUp") {
                baseData.PlayerNumber++;
            } else {
                if (baseData.PlayerNumber >= 2) {
                    baseData.PlayerNumber--;
                }
            }
            PlayerNumberLabel.Content = baseData.PlayerNumber;
        }
    }
}

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
//using Microsoft.DirectX.DirectInput;
//using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        BaseData baseData;
        Dictionary<string, Label> locLabelDic;

        public MainWindow()
        {
            InitializeComponent();

            //baseDataを作成する(最初)
            baseData = new BaseData();
            baseData.JoystickSetup();

            locLabelDic = new Dictionary<string, Label>();

            MakeWindow();
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;
            Button startButton = new Button();
            BaseData.ButtonAdd(ref baseGrid, ref startButton, Yomiage, "", "Start", 1, 1);
            Button configButton = new Button();
            BaseData.ButtonAdd(ref baseGrid, ref configButton, KeyConfig, "", "Config", 1, 3);

            PlayerNumberGridMake();
            WinPointsGridMake();
            LoseMistakesGridMake();
        }

        private void PlayerNumberGridMake() {
            Label playerNumberTitleLabel = new Label();
            BaseData.LabelAdd(ref playerNumberGrid, ref playerNumberTitleLabel, "参加人数", 0, 0,
                "", 1, 2);
            Label playerNumberLabel = new Label();
            BaseData.LabelAdd(ref playerNumberGrid, ref playerNumberLabel,
                baseData.PlayerNumber.ToString(), 1, 0, "", 1, 2);
            locLabelDic["playerNumberLabel"] = playerNumberLabel;
            Button playerNumberDownButton = new Button();
            BaseData.ButtonAdd(ref playerNumberGrid, ref playerNumberDownButton, PlayerNumberChange,
                "playerNumberDownButton", "-", 2, 0);
            Button playerNumberUpButton = new Button();
            BaseData.ButtonAdd(ref playerNumberGrid, ref playerNumberUpButton, PlayerNumberChange,
                "playerNumberUpButton", "+", 2, 1);
        }

        private void WinPointsGridMake() {
            Label winPointsTitleLabel = new Label();
            BaseData.LabelAdd(ref winPointsGrid, ref winPointsTitleLabel, "勝抜○", 0, 0,
                "", 1, 2);
            Label winPointsLabel = new Label();
            BaseData.LabelAdd(ref winPointsGrid, ref winPointsLabel,
                baseData.WinPoints.ToString(), 1, 0, "", 1, 2);
            locLabelDic["winPointsLabel"] = winPointsLabel;
            Button winPointsDownButton = new Button();
            BaseData.ButtonAdd(ref winPointsGrid, ref winPointsDownButton, WinPointsChange,
                "winPointsDownButton", "-", 2, 0);
            Button winPointsUpButton = new Button();
            BaseData.ButtonAdd(ref winPointsGrid, ref winPointsUpButton, WinPointsChange,
                "winPointsUpButton", "+", 2, 1);
        }

        private void LoseMistakesGridMake() {
            Label loseMistakesTitleLabel = new Label();
            BaseData.LabelAdd(ref loseMistakesGrid, ref loseMistakesTitleLabel, "失格×", 0, 0,
                "", 1, 2);
            Label loseMistakesLabel = new Label();
            BaseData.LabelAdd(ref loseMistakesGrid, ref loseMistakesLabel,
                baseData.LoseMistakes.ToString(), 1, 0, "", 1, 2);
            locLabelDic["loseMistakesLabel"] = loseMistakesLabel;
            Button loseMistakesDownButton = new Button();
            BaseData.ButtonAdd(ref loseMistakesGrid, ref loseMistakesDownButton, LoseMistakesChange,
                "loseMistakesDownButton", "-", 2, 0);
            Button loseMistakesUpButton = new Button();
            BaseData.ButtonAdd(ref loseMistakesGrid, ref loseMistakesUpButton, LoseMistakesChange,
                "loseMistakesUpButton", "+", 2, 1);
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
            if (((Button)sender).Name == "playerNumberUpButton") {
                baseData.PlayerNumber++;
            } else {
                if (baseData.PlayerNumber >= 2) {
                    baseData.PlayerNumber--;
                }
            }
            locLabelDic["playerNumberLabel"].Content = baseData.PlayerNumber;
        }

        private void WinPointsChange(object sender, RoutedEventArgs e) {
            if (((Button)sender).Name == "winPointsUpButton") {
                baseData.WinPoints++;
            } else {
                if (baseData.WinPoints >= 2) {
                    baseData.WinPoints--;
                }
            }
            locLabelDic["winPointsLabel"].Content = baseData.WinPoints;
        }

        private void LoseMistakesChange(object sender, RoutedEventArgs e) {
            if (((Button)sender).Name == "loseMistakesUpButton") {
                baseData.LoseMistakes++;
            } else {
                if (baseData.LoseMistakes >= 2) {
                    baseData.LoseMistakes--;
                }
            }
            locLabelDic["loseMistakesLabel"].Content = baseData.LoseMistakes;
        }
    }
}

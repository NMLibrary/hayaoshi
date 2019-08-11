using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi {
    /// <summary>
    /// KeyConfig.xaml の相互作用ロジック
    /// </summary>
    public partial class KeyConfig : Window {

        BaseData baseData;
        Label[] controllerBackLabels;
        Label[] controllerTextLabels;
        Label[] controllerFrontLabels;
        Label[] keyBackLabels;
        Label[] keyTextLabels;
        Label[] keyFrontLabels;
        TextBox[] playerNameTextBoxes;
        int configIndex;
        SysKey? configKey;
        int cancelClicked;
        Button cancelButton;

        public KeyConfig(BaseData baseData) {
            InitializeComponent();

            this.baseData = baseData;

            controllerBackLabels = new Label[baseData.PlayerNumber];
            controllerTextLabels = new Label[baseData.PlayerNumber];
            controllerFrontLabels = new Label[baseData.PlayerNumber];
            keyBackLabels = new Label[baseData.PlayerNumber];
            keyTextLabels = new Label[baseData.PlayerNumber];
            keyFrontLabels = new Label[baseData.PlayerNumber];
            playerNameTextBoxes = new TextBox[baseData.PlayerNumber];
            configIndex = -1;
            cancelClicked = -1;

            for (int i = 0; i < baseData.PlayerNumber; i++) {
                if (!baseData.PlayerDic.ContainsKey(i)) {
                    baseData.PlayerDic.Add(i, new Player());
                }
            }

            MakeWindow();
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;

            double[] baseGridHeights = new double[4] { 1.0, 3.0, 1.0, 1.0 };
            for (int i = 0; i < baseGridHeights.Length; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(baseGridHeights[i], GridUnitType.Star);
                baseGrid.RowDefinitions.Add(row);
            }

            baseGrid.Focusable = true;
            baseGrid.Focus();
            baseGrid.KeyDown += (sender, e) => { KeyPush(sender, e); };

            MakeConfigGrid();

            Grid cancelGrid = new Grid();
            cancelGrid.SetValue(Grid.RowProperty, 2);
            cancelGrid.SetValue(Grid.ColumnProperty, 0);
            baseGrid.Children.Add(cancelGrid);
            for (int i = 0; i < 5; i++) {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1.0, GridUnitType.Star);
                cancelGrid.ColumnDefinitions.Add(column);
            }
            BaseData.ButtonAdd(ref cancelGrid, ref cancelButton, CancelClick, "cancelButton",
                "cancel", 0, 2);
        }

        private void MakeConfigGrid() {
            Grid configGrid = new Grid();
            configGrid.SetValue(Grid.RowProperty, 1);
            configGrid.SetValue(Grid.ColumnProperty, 0);
            baseGrid.Children.Add(configGrid);

            double[] configGridHeights = new double[4] { 1.0, 1.0, 1.0, 1.0 };
            for (int i = 0; i < configGridHeights.Length; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(configGridHeights[i], GridUnitType.Star);
                configGrid.RowDefinitions.Add(row);
            }
            for (int i = 0; i < baseData.PlayerNumber + 1; i++) {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1.0, GridUnitType.Star);
                configGrid.ColumnDefinitions.Add(column);
            }

            Label controllerTitleLabel = new Label();
            Label keyTitleLabel = new Label();
            Label nameTitleLabel = new Label();
            BaseData.LabelAdd(ref configGrid, ref controllerTitleLabel, "Controller", 1, 0);
            BaseData.LabelAdd(ref configGrid, ref keyTitleLabel, "Key", 2, 0);
            BaseData.LabelAdd(ref configGrid, ref nameTitleLabel, "Name", 3, 0);
            for (int i = 0; i < baseData.PlayerNumber; i++) {
                Label numberLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref numberLabel, (i + 1).ToString(), 0, i + 1);
            }

            for (int i = 0; i < baseData.PlayerNumber; i++) {
                string controllerContent = "";
                if (baseData.NumToJoyDic.ContainsKey(i)) {
                    controllerContent = "○";
                }
                BaseData.LabelAdd(ref configGrid, ref controllerTextLabels[i], controllerContent, 1, i + 1,
                    1, 1, null, null, 5);
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref controllerBackLabels[i], "", 1, i + 1,
                    1, 1, null, null, 1);
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref controllerFrontLabels[i], "", 1, i + 1,
                    1, 1, null, null, 10);
                //controllerBackLabels[i].MouseLeftButtonDown += (sender, e) => { ControllerSet(sender, e, i); };
                controllerFrontLabels[i].Name = "controllerFrontLabel" + i.ToString();
                controllerFrontLabels[i].MouseLeftButtonDown += (sender, e) => { ControllerSet(sender, e); };

                string keyContent = "";
                if (baseData.NumToKeyDic.ContainsKey(i)) {
                    keyContent = baseData.NumToKeyDic[i].ToString();
                }
                BaseData.LabelAdd(ref configGrid, ref keyTextLabels[i], keyContent, 2, i + 1,
                    1, 1, null, null, 5);
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref keyBackLabels[i], "", 2, i + 1,
                    1, 1, null, null, 1);
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref keyFrontLabels[i], "", 2, i + 1,
                    1, 1, null, null, 10);
                keyFrontLabels[i].Name = "keyFrontLabel" + i.ToString();
                keyFrontLabels[i].MouseLeftButtonDown += (sender, e) => { KeySet(sender, e); };

                string playerNameContent = "";
                if (baseData.PlayerDic.ContainsKey(i)) {
                    playerNameContent = baseData.PlayerDic[i].Name;
                }
                BaseData.TextBoxAdd(ref configGrid, ref playerNameTextBoxes[i], TextBoxChanged,
                    "playerNameTextBox" + i.ToString(), playerNameContent, 3, i + 1);
            }
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox)sender;
            string indexText = Regex.Match(box.Name, "[0-9]+$").Groups[0].Value;
            int index = int.Parse(indexText);
            Player player = baseData.PlayerDic[index];
            player.Name = box.Text;
        }

        //ジョイスティックの設定
        private void ControllerSet(object sender, MouseButtonEventArgs e) {
            if (configIndex == -1) {
                string indexText = Regex.Match(((Label)sender).Name, "[0-9]+$").Groups[0].Value;
                int index = int.Parse(indexText);
                controllerBackLabels[index].Background = Brushes.DarkGray;

                baseData.Timer.Tick += ControllerSetTimerTick;
                configIndex = index;
            }
        }

        //ジョイスティックが押されるまで待つ処理
        private void ControllerSetTimerTick(object sender, EventArgs e) {
            if (cancelClicked > -1) {
                baseData.Timer.Tick -= ControllerSetTimerTick;
                controllerBackLabels[configIndex].Background = Brushes.Transparent;
                configIndex = -1;
                return;
            }
            foreach (Joystick joystick in baseData.Joysticks) {
                if (joystick.JoystickPushed) {
                    if (baseData.JoyToNumDic.ContainsKey(joystick)) {
                        int oldIndex = baseData.JoyToNumDic[joystick];
                        controllerTextLabels[oldIndex].Content = "";
                        baseData.NumToJoyDic.Remove(oldIndex);
                        baseData.JoyToNumDic.Remove(joystick);
                    }
                    if (baseData.NumToJoyDic.ContainsKey(configIndex)) {
                        Joystick oldJoystick = baseData.NumToJoyDic[configIndex];
                        oldJoystick.JoystickKey = null;
                        baseData.NumToJoyDic.Remove(configIndex);
                        baseData.JoyToNumDic.Remove(oldJoystick);
                    }
                    baseData.NumToJoyDic.Add(configIndex, joystick);
                    baseData.JoyToNumDic.Add(joystick, configIndex);
                    controllerTextLabels[configIndex].Content = "○";
                    if (baseData.NumToKeyDic.ContainsKey(configIndex)) {
                        SysKey key = baseData.NumToKeyDic[configIndex];
                        joystick.JoystickKey = key;
                    }

                    baseData.Timer.Tick -= ControllerSetTimerTick;
                    controllerBackLabels[configIndex].Background = Brushes.Transparent;
                    configIndex = -1;
                    break;
                }
            }
        }

        //キーの設定
        private void KeySet(object sender, MouseButtonEventArgs e) {
            if (configIndex == -1) {
                string indexText = Regex.Match(((Label)sender).Name, "[0-9]+$").Groups[0].Value;
                int index = int.Parse(indexText);
                keyBackLabels[index].Background = Brushes.DarkGray;

                baseData.Timer.Tick += KeySetTimerTick;
                configIndex = index;
                configKey = null;
            }
        }
        
        //キーが押されるまで待つ処理
        private void KeySetTimerTick(object sender, EventArgs e) {
            if (cancelClicked > -1) {
                baseData.Timer.Tick -= KeySetTimerTick;
                keyBackLabels[configIndex].Background = Brushes.Transparent;
                configIndex = -1;
                return;
            }
            if (configKey != null) {
                SysKey key = (SysKey)configKey;
                if (baseData.KeyToNumDic.ContainsKey(key)) {
                    int oldIndex = baseData.KeyToNumDic[key];
                    keyTextLabels[oldIndex].Content = "";
                    baseData.NumToKeyDic.Remove(oldIndex);
                    baseData.KeyToNumDic.Remove(key);
                    if (baseData.NumToJoyDic.ContainsKey(oldIndex)) {
                        Joystick joystick = baseData.NumToJoyDic[oldIndex];
                        joystick.JoystickKey = null;
                    }
                }
                if (baseData.NumToKeyDic.ContainsKey(configIndex)) {
                    SysKey oldKey = baseData.NumToKeyDic[configIndex];
                    baseData.NumToKeyDic.Remove(configIndex);
                    baseData.KeyToNumDic.Remove(oldKey);
                }
                baseData.KeyToNumDic.Add(key, configIndex);
                baseData.NumToKeyDic.Add(configIndex, key);
                keyTextLabels[configIndex].Content = key.ToString();
                if (baseData.NumToJoyDic.ContainsKey(configIndex)) {
                    Joystick joystick = baseData.NumToJoyDic[configIndex];
                    joystick.JoystickKey = key;
                }

                baseData.Timer.Tick -= KeySetTimerTick;
                keyBackLabels[configIndex].Background = Brushes.Transparent;
                configIndex = -1;
            }
        }

        //キーが押されたか常に監視する
        private void KeyPush(object sender, KeyEventArgs e) {
            configKey = e.Key;
        }

        //ジョイスティックやキーを設定しようとした状態でキャンセルを押したときの処理
        private void CancelClick(object sender, RoutedEventArgs e) {
            if (configIndex != -1) {
                cancelClicked = 5;
                baseData.Timer.Tick += DecreaseCancelClickedTimer;
            }
        }

        //キャンセルを押した場合5Tickの間キャンセルモードにする
        private void DecreaseCancelClickedTimer(object sender, EventArgs e) {
            if (cancelClicked == -1) {
                baseData.Timer.Tick -= DecreaseCancelClickedTimer;
            } else {
                cancelClicked--;
            }
        }
    }
}

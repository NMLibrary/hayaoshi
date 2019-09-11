using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
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
        Label[] keyBackLabels;
        Label[] keyTextLabels;
        Label[] keyFrontLabels;
        Dictionary<string, Label> locLabelDic;
        TextBox[] playerNameTextBoxes;
        int configIndex;
        SysKey? configKey;
        int cancelClicked;
        Button cancelButton;
        Dictionary<string, string> messageDic = new Dictionary<string, string> {
            ["base"] = "設定したいキーやボタンを選択してください",
            ["joy"] = "設定したいゲームパッドのボタンを押してください",
            ["key"] = "設定したいキーを押してください",
            ["ok"] = "設定しました",
            ["usedKey"] = "oとxは設定できません",
            ["cancel"] = "キャンセルしました",
            ["default"] = "ゲームパッドを再読み込みし、配置を初期化しました",
            ["questionClear"] = "問題の音声をリセットしました。現在0問入っています"
        };

        public KeyConfig(BaseData baseData) {
            InitializeComponent();

            this.baseData = baseData;
            locLabelDic = new Dictionary<string, Label>();

            keyBackLabels = new Label[baseData.PlayerNumber];
            keyTextLabels = new Label[baseData.PlayerNumber];
            keyFrontLabels = new Label[baseData.PlayerNumber];
            playerNameTextBoxes = new TextBox[baseData.PlayerNumber];
            configIndex = -1;
            cancelClicked = -1;

            for (int i = 0; i < baseData.PlayerNumber; i++) {
                if (!baseData.PlayerNameDic.ContainsKey(i)) {
                    baseData.PlayerNameDic.Add(i, "No Name");
                }
            }

            MakeWindow();
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;

            //double[] baseGridHeights = new double[4] { 1.0, 3.0, 1.0, 1.0 };
            //for (int i = 0; i < baseGridHeights.Length; i++) {
            //    RowDefinition row = new RowDefinition();
            //    row.Height = new GridLength(baseGridHeights[i], GridUnitType.Star);
            //    baseGrid.RowDefinitions.Add(row);
            //}

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
                "cancel", 0, 4);

            MakeLoadGrid();

            Label messageLabel = new Label();
            BaseData.LabelAdd(ref baseGrid, ref messageLabel, messageDic["base"], 4, 0);
            locLabelDic["message"] = messageLabel;
            ((Viewbox)messageLabel.Parent).HorizontalAlignment = HorizontalAlignment.Left;
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
                Label controllerTextLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref controllerTextLabel, controllerContent, 1, i + 1,
                    "", 1, 1, null, null, 5);
                locLabelDic["controllerTextLabel" + i.ToString()] = controllerTextLabel;
                Label controllerBackLabel = new Label();
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref controllerBackLabel, "", 1, i + 1,
                    "", 1, 1, null, null, 1);
                locLabelDic["controllerBackLabel" + i.ToString()] = controllerBackLabel;
                Label controllerFrontLabel = new Label();
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref controllerFrontLabel, "", 1, i + 1,
                    "controllerFrontLabel" + i.ToString(), 1, 1, null, null, 10);
                locLabelDic["controllerFrontLabel" + i.ToString()] = controllerFrontLabel;
                //controllerBackLabels[i].MouseLeftButtonDown += (sender, e) => { ControllerSet(sender, e, i); };
                controllerFrontLabel.MouseLeftButtonDown
                    += (sender, e) => { ControllerSet(sender, e); };

                string keyContent = "";
                if (baseData.NumToKeyDic.ContainsKey(i)) {
                    keyContent = baseData.NumToKeyDic[i].ToString();
                }
                Label keyTextLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref keyTextLabel, keyContent, 2, i + 1,
                    "", 1, 1, null, null, 5);
                locLabelDic["keyTextLabel" + i.ToString()] = keyTextLabel;
                Label keyBackLabel = new Label();
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref keyBackLabel, "", 2, i + 1,
                    "", 1, 1, null, null, 1);
                locLabelDic["keyBackLabel" + i.ToString()] = keyBackLabel;
                Label keyFrontLabel = new Label();
                BaseData.LabelAddWithoutViewbox(ref configGrid, ref keyFrontLabel, "", 2, i + 1,
                    "keyFrontLabel" + i.ToString(), 1, 1, null, null, 10);
                locLabelDic["keyFrontLabel" + i.ToString()] = keyFrontLabel;
                keyFrontLabel.MouseLeftButtonDown += (sender, e) => { KeySet(sender, e); };

                string playerNameContent = "";
                if (baseData.PlayerNameDic.ContainsKey(i)) {
                    playerNameContent = baseData.PlayerNameDic[i];
                }
                BaseData.TextBoxAdd(ref configGrid, ref playerNameTextBoxes[i], TextBoxChanged,
                    "playerNameTextBox" + i.ToString(), playerNameContent, 3, i + 1);
            }
        }

        private void MakeLoadGrid() {
            Button controllerLoadButton = new Button();
            BaseData.ButtonAdd(ref loadGrid, ref controllerLoadButton, LoadController,
                "controllerLoadButton", "ジョイスティックの初期化", 0, 2);
            Button questionLoadButton = new Button();
            BaseData.ButtonAdd(ref loadGrid, ref questionLoadButton, LoadFile, "questionLoadButton",
                "音声のロード", 0, 0);
            Button questionClearButton = new Button();
            BaseData.ButtonAdd(ref loadGrid, ref questionClearButton, QuestionSoundsClear, "questionClearButton",
                "音声のリセット", 0, 1);
            Button questionStringLoadButton = new Button();
            BaseData.ButtonAdd(ref loadGrid, ref questionStringLoadButton, LoadQuestionFile, "questionStringLoadButton",
                "問題文のロード", 0, 3);
            Button questionStringClearButton = new Button();
            BaseData.ButtonAdd(ref loadGrid, ref questionStringClearButton, QuestionStringsClear, "questionStringClearButton",
                "問題文のリセット", 0, 4);
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox)sender;
            string indexText = Regex.Match(box.Name, "[0-9]+$").Groups[0].Value;
            int index = int.Parse(indexText);
            baseData.PlayerNameDic[index] = box.Text;
        }

        //ジョイスティックの設定
        private void ControllerSet(object sender, MouseButtonEventArgs e) {
            if (configIndex == -1) {
                string indexText = Regex.Match(((Label)sender).Name, "[0-9]+$").Groups[0].Value;
                int index = int.Parse(indexText);
                locLabelDic["controllerBackLabel" + index.ToString()].Background = Brushes.DarkGray;

                baseData.Timer.Tick += ControllerSetTimerTick;
                configIndex = index;
                locLabelDic["message"].Content = messageDic["joy"];
            }
        }

        //ジョイスティックが押されるまで待つ処理
        private void ControllerSetTimerTick(object sender, EventArgs e) {
            if (cancelClicked > -1) {
                baseData.Timer.Tick -= ControllerSetTimerTick;
                locLabelDic["controllerBackLabel" + configIndex.ToString()].Background = Brushes.Transparent;
                configIndex = -1;
                locLabelDic["message"].Content = messageDic["cancel"];
                return;
            }
            foreach (Joystick joystick in baseData.Joysticks) {
                if (joystick.JoystickPushed) {
                    if (baseData.JoyToNumDic.ContainsKey(joystick)) {
                        int oldIndex = baseData.JoyToNumDic[joystick];
                        locLabelDic["controllerTextLabel" + oldIndex.ToString()].Content = "";
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
                    locLabelDic["controllerTextLabel" + configIndex.ToString()].Content = "○";
                    if (baseData.NumToKeyDic.ContainsKey(configIndex)) {
                        SysKey key = baseData.NumToKeyDic[configIndex];
                        joystick.JoystickKey = key;
                    }

                    baseData.Timer.Tick -= ControllerSetTimerTick;
                    locLabelDic["controllerBackLabel" + configIndex.ToString()].Background = Brushes.Transparent;
                    configIndex = -1;
                    locLabelDic["message"].Content = messageDic["ok"];
                    break;
                }
            }
        }

        //キーの設定
        private void KeySet(object sender, MouseButtonEventArgs e) {
            if (configIndex == -1) {
                string indexText = Regex.Match(((Label)sender).Name, "[0-9]+$").Groups[0].Value;
                int index = int.Parse(indexText);
                locLabelDic["keyBackLabel" + index.ToString()].Background = Brushes.DarkGray;

                baseData.Timer.Tick += KeySetTimerTick;
                configIndex = index;
                configKey = null;
                locLabelDic["message"].Content = messageDic["key"];
            }
        }
        
        //キーが押されるまで待つ処理
        private void KeySetTimerTick(object sender, EventArgs e) {
            if (cancelClicked > -1) {
                baseData.Timer.Tick -= KeySetTimerTick;
                locLabelDic["keyBackLabel" + configIndex.ToString()].Background = Brushes.Transparent;
                configIndex = -1;
                locLabelDic["message"].Content = messageDic["cancel"];
                return;
            }
            if (configKey != null) {
                if (BaseData.judgingButton.ContainsValue((SysKey)configKey)) {
                    locLabelDic["message"].Content = messageDic["usedKey"];
                    return;
                }
                SysKey key = (SysKey)configKey;
                if (baseData.KeyToNumDic.ContainsKey(key)) {
                    int oldIndex = baseData.KeyToNumDic[key];
                    locLabelDic["keyTextLabel" + oldIndex.ToString()].Content = "";
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
                locLabelDic["keyTextLabel" + configIndex.ToString()].Content = key.ToString();
                if (baseData.NumToJoyDic.ContainsKey(configIndex)) {
                    Joystick joystick = baseData.NumToJoyDic[configIndex];
                    joystick.JoystickKey = key;
                }

                baseData.Timer.Tick -= KeySetTimerTick;
                locLabelDic["keyBackLabel" + configIndex.ToString()].Background = Brushes.Transparent;
                configIndex = -1;
                locLabelDic["message"].Content = messageDic["ok"];
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

        //コントローラーの再ロード
        private void LoadController(object sender, RoutedEventArgs e) {
            if (configIndex == -1) {
                baseData.JoystickSetup();
                for (int i = 0; i < baseData.PlayerNumber; i++) {
                    string controllerString = "";
                    if (baseData.NumToJoyDic.ContainsKey(i)) {
                        controllerString = "○";
                    }
                    locLabelDic["controllerTextLabel" + i.ToString()].Content = controllerString;
                    string keyString = "";
                    if (baseData.NumToKeyDic.ContainsKey(i)) {
                        keyString = baseData.NumToKeyDic[i].ToString();
                    }
                    locLabelDic["keyTextLabel" + i.ToString()].Content = keyString;
                }
                locLabelDic["message"].Content = messageDic["default"];
            }
        }

        //問題音声のリセット
        private void QuestionSoundsClear(object sender, RoutedEventArgs e) {
            if (configIndex == -1) {
                baseData.QuestionSounds = new List<string>();
                locLabelDic["message"].Content = messageDic["questionClear"];
            }
        }

        //音声用問題フォルダの選択
        private void LoadFile(object sender, RoutedEventArgs e) {
            if (configIndex == -1) {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                //dialog.Multiselect = true;
                //dialog.Filter = "問題ファイル(.xlsx)|*.xlsx";
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok) {
                    Console.WriteLine(dialog.FileName);
                    List<string> files = ReadFiles(dialog.FileName);
                    baseData.QuestionSounds = baseData.QuestionSounds.Concat(files).ToList();
                    locLabelDic["message"].Content = "問題の音声を追加しました。現在" + baseData.QuestionSounds.Count().ToString()
                        + "問入っています";
                    //foreach (string name in dialog.FileNames) {
                    //    Console.WriteLine(name);
                    //}
                }
            }
        }

        //LoadFileで使う音声を指定されたフォルダから読み込み
        private List<string> ReadFiles(String sourceDir) {
            string[] patterns = { ".wav", ".m4a" };
            string[] rawFiles = Directory.GetFiles(sourceDir, "*.*");
            List<string> filteredFiles = rawFiles.Where(file => patterns.Any(pattern => file.ToLower().EndsWith(pattern))).ToList();

            filteredFiles.Sort();
            return filteredFiles;
        }

        //問題文の読み込み
        private void LoadQuestionFile(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "問題ファイル(.xlsx)|*.xlsx";
            bool? result = dialog.ShowDialog();
            if (result == true) {
                string fileName = dialog.FileName;
                bool addResult = baseData.AddQuestions(fileName);
                if (addResult) {
                    locLabelDic["message"].Content = "表示用の問題文を追加しました。現在"
                        + baseData.QuestionStrings.Count().ToString() + "問入っています";
                } else {
                    locLabelDic["message"].Content = "失敗。要再起動。";
                }
            }
        }

        //問題文のリセット
        private void QuestionStringsClear(object sender, RoutedEventArgs e) {
            if (configIndex == -1) {
                baseData.QuestionStrings = new List<(string, string)>();
                locLabelDic["message"].Content = "表示用問題文をリセットしました。現在0問入っています";
            }
        }
    }
}

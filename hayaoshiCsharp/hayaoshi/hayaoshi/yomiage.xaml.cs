using System;
using System.Collections;
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
using System.Windows.Shapes;
using System.Windows.Threading;
//using Microsoft.DirectX.DirectInput;
//using DXKey = Microsoft.DirectX.DirectInput.Key;
using SysKey = System.Windows.Input.Key;

namespace hayaoshi {
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class Hayaoshi : Window {
        BaseData baseData;

        bool pushed = false;
        Player[] players;
        int playerSize;
        Player pushPlayer;
        List<(Player player, JudgeStatus judge)> history = new List<(Player player, JudgeStatus judge)>();
        int questionNumber = 0;
        Player throughPlayer;
        Dictionary<string, string> sounds = new Dictionary<string, string>() {
            {"button", "C:\\Users\\Tsurusaki\\Git\\hayaoshi\\buttonSound6.wav"},
            {"correct", "C:\\Users\\Tsurusaki\\Git\\hayaoshi\\correctSound6.wav"},
            {"wrong", "C:\\Users\\Tsurusaki\\Git\\hayaoshi\\wrongBuzzer2.wav" },
            {"buzzer", "C:\\Users\\Tsurusaki\\Git\\hayaoshi\\buzzer.wav" }
        };
        List<string> questionSounds;

        Joystick[] Joysticks;

        Dictionary<string, Label> locLabelDic;
        Dictionary<string, Button> locButtonDic;

        public Hayaoshi(BaseData baseData) {
            InitializeComponent();
            this.baseData = baseData;
            playerSize = baseData.PlayerNumber;
            locLabelDic = new Dictionary<string, Label>();
            locButtonDic = new Dictionary<string, Button>();
            questionSounds = baseData.QuestionSounds;

            MakeWindow();

            players = new Player[playerSize];
            for (int i = 0; i < playerSize; i++) {
                players[i] = new Player();
                if (baseData.PlayerNameDic.ContainsKey(i)) {
                    players[i].Name = baseData.PlayerNameDic[i];
                } else {
                    players[i].Name = "No Name";
                }
                string name = "name" + i.ToString();
                players[i].NameLabel = locLabelDic["name" + i.ToString()];
                players[i].KeyLabel = locLabelDic["key" + i.ToString()];
                players[i].LightLabel = locLabelDic["light" + i.ToString()];
                players[i].Point = 0;
                players[i].Mistake = 0;
                players[i].PointLabel = locLabelDic["point" + i.ToString()];
                players[i].MistakeLabel = locLabelDic["miss" + i.ToString()];
                if (baseData.NumToKeyDic.ContainsKey(i)) {
                    players[i].Button = baseData.NumToKeyDic[i];
                } else {
                    players[i].Button = null;
                }
            }

            for (int i = 0; i < playerSize; i++) {
                players[i].NameLabel.Content = players[i].Name;
                players[i].LightLabel.Content = "";
                players[i].PointLabel.Content = players[i].Point.ToString();
                players[i].MistakeLabel.Content = players[i].Mistake.ToString();
                if (players[i].Button != null) {
                    players[i].KeyLabel.Content = players[i].Button.ToString();
                }
            }

            pushed = false;

            Reset();

            Joysticks = baseData.Joysticks;
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;
            MakePlayerGrid();
            Button readButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref readButton, QuestionClick, "", "読み上げ", 0, 0);
            locButtonDic["read"] = readButton;
            Button undoButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref undoButton, UndoClick, "", "Undo", 0, 2);
            locButtonDic["undo"] = undoButton;
            Button throughButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref throughButton, ThroughClick, "", "Through", 0, 4);
            locButtonDic["through"] = throughButton;
        }

        private void MakePlayerGrid() {
            Grid playerGrid = new Grid();
            playerGrid.SetValue(Grid.RowProperty, 1);
            playerGrid.SetValue(Grid.ColumnProperty, 0);
            baseGrid.Children.Add(playerGrid);

            double[] playerGridHeights = new double[4] { 2.0, 2.0, 1.0, 1.0 };
            for (int i = 0; i < playerGridHeights.Length; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(playerGridHeights[i], GridUnitType.Star);
                playerGrid.RowDefinitions.Add(row);
            }
            for (int i = 0; i < baseData.PlayerNumber; i++) {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1.0, GridUnitType.Star);
                playerGrid.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i < baseData.PlayerNumber; i++) {
                Label nameLabel = new Label();
                BaseData.LabelAdd(ref playerGrid, ref nameLabel, "", 0, i, "name" + i.ToString(),
                    1, 1, null, BaseData.nameAndLightColor);
                locLabelDic["name" + i.ToString()] = nameLabel;
                Label lightLabel = new Label();
                BaseData.LabelAdd(ref playerGrid, ref lightLabel, "", 1, i, "light" + i.ToString(),
                    1, 1, null, BaseData.nameAndLightColor);
                locLabelDic["light" + i.ToString()] = lightLabel;

                Grid pointAndMissGrid = new Grid();
                playerGrid.Children.Add(pointAndMissGrid);
                pointAndMissGrid.SetValue(Grid.RowProperty, 2);
                pointAndMissGrid.SetValue(Grid.ColumnProperty, i);
                for (int j = 0; j < 2; j++) {
                    ColumnDefinition column = new ColumnDefinition();
                    column.Width = new GridLength(1.0, GridUnitType.Star);
                    pointAndMissGrid.ColumnDefinitions.Add(column);
                }
                Label pointLabel = new Label();
                BaseData.LabelAdd(ref pointAndMissGrid, ref pointLabel, "", 0, 0, "point" + i.ToString(),
                    1, 1, null, BaseData.pointColor);
                locLabelDic["point" + i.ToString()] = pointLabel;
                Label missLabel = new Label();
                BaseData.LabelAdd(ref pointAndMissGrid, ref missLabel, "", 0, 1, "miss" + i.ToString(),
                    1, 1, null, BaseData.mistakeColor);
                locLabelDic["miss" + i.ToString()] = missLabel;

                Label keyLabel = new Label();
                BaseData.LabelAdd(ref playerGrid, ref keyLabel, "", 3, i, "key" + i.ToString(),
                    1, 1, null, BaseData.ColorGradation(i, baseData.PlayerNumber));
                locLabelDic["key" + i.ToString()] = keyLabel;
            }
        }

        private void KeyPush(object sender, KeyEventArgs e) {
            for (int i = 0; i < playerSize; i++) {
                if (e.Key == players[i].Button && CanUseButton(players[i])) {
                    if (!pushed) {
                        players[i].LightLabel.Content = "!";
                        pushed = true;
                        Button throughbutton = locButtonDic["through"];
                        BaseData.ChangeButtonContent(ref throughbutton, "無効");
                        pushPlayer = players[i];
                        PlaySound(sounds["button"]);
                        message.Content = "正解ならばoを、不正解ならばxを押してください";
                    }
                }
            }
            if (BaseData.judgingButton.ContainsValue(e.Key)) {
                if (pushed) {
                    pushed = false;
                    pushPlayer.LightLabel.Content = "";
                    if (e.Key == BaseData.judgingButton["ok"]) {
                        PlaySound(sounds["correct"]);
                        history.Add((pushPlayer, JudgeStatus.Point));
                        Correct(pushPlayer);
                    } else {
                        PlaySound(sounds["wrong"]);
                        history.Add((pushPlayer, JudgeStatus.Mistake));
                        Wrong(pushPlayer);
                    }
                    message.Content = "出題中";
                    Button throughbutton = locButtonDic["through"];
                    BaseData.ChangeButtonContent(ref throughbutton, "through");
                }
            }
        }

        private bool CanUseButton(Player player) {
            bool result = true;
            if (player.Win) {
                result = false;
            }
            if (player.Lose) {
                result = false;
            }
            return result;
        }

        private void Correct(Player player) {
            player.Point++;
            player.PointLabel.Content = player.Point.ToString();
            if (player.Point == baseData.WinPoints) {
                player.Win = true;
                player.PointLabel.Background = BaseData.winColor;
                player.NameLabel.Background = BaseData.winColor;
            }
            questionNumber++;
            questionNumberLabel.Content = (questionNumber + 1).ToString() + "問目";
        }

        private void Wrong(Player player) {
            player.Mistake++;
            player.MistakeLabel.Content = player.Mistake.ToString();
            if (player.Mistake == baseData.LoseMistakes) {
                player.Lose = true;
                player.MistakeLabel.Background = BaseData.loseColor;
                player.NameLabel.Background = BaseData.loseColor;
            }
            questionNumber++;
            questionNumberLabel.Content = (questionNumber + 1).ToString() + "問目";
        }

        private void ThroughClick(object sender, RoutedEventArgs e) {
            if (!pushed) {
                history.Add((throughPlayer, JudgeStatus.Through));
                Through();
            } else {
                history.Add((throughPlayer, JudgeStatus.Invalid));
                Invalid();
                message.Content = "出題中";
                Button throughButton = locButtonDic["through"];
                BaseData.ChangeButtonContent(ref throughButton, "through");
                pushPlayer.LightLabel.Content = "";
            }
            StopSound();
        }

        private void Through() {
            questionNumber++;
            questionNumberLabel.Content = (questionNumber + 1).ToString() + "問目";
            pushed = false;
        }

        private void Invalid() {
            questionNumber++;
            questionNumberLabel.Content = (questionNumber + 1).ToString() + "問目";
            pushed = false;
        }

        private void UndoClick(object sender, RoutedEventArgs e) {
            if (!pushed) {
                if (history.Count > 0) {
                    history.RemoveAt(history.Count - 1);
                    StopSound();
                    Reset();
                    foreach ((Player player, JudgeStatus judge) in history) {
                        if (judge == JudgeStatus.Point) {
                            Correct(player);
                        } else if (judge == JudgeStatus.Mistake) {
                            Wrong(player);
                        } else if (judge == JudgeStatus.Through) {
                            Through();
                        } else if (judge == JudgeStatus.Invalid) {
                            Invalid();
                        }
                    }
                }
            }
        }

        private void Reset() {
            for (int i = 0; i < playerSize; i++) {
                players[i].Point = 0;
                players[i].Mistake = 0;
                players[i].PointLabel.Content = "0";
                players[i].MistakeLabel.Content = "0";
                players[i].Win = false;
                players[i].Lose = false;
                //players[i].NameLabel.Foreground = BaseData.nameAndLightColor;
                //players[i].PointLabel.Foreground = BaseData.pointColor;
                //players[i].MistakeLabel.Foreground = BaseData.mistakeColor;
                players[i].NameLabel.Background = Brushes.Transparent;
                players[i].PointLabel.Background = Brushes.Transparent;
                players[i].MistakeLabel.Background = Brushes.Transparent;
            }
            questionNumber = 0;
            questionNumberLabel.Content = "1問目";
            message.Content = "出題中";
        }

        private void QuestionClick(object sender, RoutedEventArgs e) {
            if (!pushed) {
                StopSound();
                int len = questionSounds.Count();
                if (questionNumber < len) {
                    PlaySound(questionSounds[questionNumber]);
                } else {
                    message.Content = "もう問題がありません";
                }
            }
        }

        private void GridLoaded(object sender, RoutedEventArgs e) {
            baseGrid.Focus();
        }

        private void PlaySound(string path) {
            StopSound();
            Microsoft.SmallBasic.Library.Sound.Play(path);
        }

        private void StopSound() {
            foreach (string item in sounds.Values) {
                Microsoft.SmallBasic.Library.Sound.Stop(item);
            }
            foreach (string item in questionSounds) {
                Microsoft.SmallBasic.Library.Sound.Stop(item);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;
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

        //bool pushed = false;
        HayaoshiPhase phase = HayaoshiPhase.Base;
        HayaoshiMode mode = HayaoshiMode.Base;

        Player[] players;
        int playerSize;
        Player pushPlayer;
        List<(HayaoshiMode mode, int questionNumber, Player player, JudgeStatus judge)> history
            = new List<(HayaoshiMode mode, int questionNumber, Player player, JudgeStatus judge)>();
        int questionNumber = 0;
        Player throughPlayer;
        string executingPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string soundDirectory;
        Dictionary<string, string> sounds;
        List<string> questionSounds;
        List<(string sentence, string answer)> questionStrings;
        string playingQuestionSound = null;

        Joystick[] Joysticks;

        Dictionary<string, Label> locLabelDic;
        Dictionary<string, Button> locButtonDic;
        Dictionary<string, RadioButton> locRadioDic;

        public Hayaoshi(BaseData baseData) {
            InitializeComponent();
            this.baseData = baseData;
            playerSize = baseData.PlayerNumber;
            locLabelDic = new Dictionary<string, Label>();
            locButtonDic = new Dictionary<string, Button>();
            locRadioDic = new Dictionary<string, RadioButton>();
            questionSounds = baseData.QuestionSounds;
            questionStrings = baseData.QuestionStrings;

            MakeWindow();

            soundDirectory = Path.GetDirectoryName(executingPath) + "\\sounds";
            sounds = new Dictionary<string, string>() {
                ["button"] = soundDirectory + "\\buttonSound6.wav",
                ["correct"] = soundDirectory + "\\correctSound6.wav",
                ["wrong"] = soundDirectory + "\\wrongBuzzer2.wav",
                ["buzzer"] = soundDirectory + "\\buzzer.wav"
            };

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

            phase = HayaoshiPhase.Base;
            mode = HayaoshiMode.Base;

            Reset();

            Joysticks = baseData.Joysticks;
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;
            MakePlayerGrid();
            MakeOperationGrid();
            MakeModeGrid();
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

        private void MakeOperationGrid() {
            Button readButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref readButton, QuestionClick, "", "読み上げ", 0, 0);
            locButtonDic["read"] = readButton;
            Button undoButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref undoButton, UndoClick, "", "Undo", 0, 1);
            locButtonDic["undo"] = undoButton;
            Button throughButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref throughButton, ThroughClick, "", "Through", 0, 2);
            locButtonDic["through"] = throughButton;
            Button showQuestionButton = new Button();
            BaseData.ButtonAdd(ref operationGrid, ref showQuestionButton, QuestionSentenceClick, "",
                "問題文表示", 0, 4);
            locButtonDic["showQuestion"] = showQuestionButton;
        }

        private void MakeModeGrid() {
            RadioButton baseRadio = new RadioButton();
            RadioButtonAdd(ref modeGrid, ref baseRadio, BaseMode, "", "通常", 0, 0);
            locRadioDic["base"] = baseRadio;
            RadioButton endlessRadio = new RadioButton();
            RadioButtonAdd(ref modeGrid, ref endlessRadio, EndlessMode, "", "エンドレスチャンス", 0, 1);
            locRadioDic["endless"] = endlessRadio;
            RadioButton checkRadio = new RadioButton();
            RadioButtonAdd(ref modeGrid, ref checkRadio, CheckMode, "", "ボタンチェック", 0, 2);
            locRadioDic["check"] = checkRadio;
            baseRadio.IsChecked = true;
        }

        private void RadioButtonAdd(ref Grid grid, ref RadioButton radio, RoutedEventHandler method,
            string name, string content, int row, int column, int rowspan = 1, int columnspan = 1,
            FontFamily font = null, SolidColorBrush color = null) {
            //Viewbox box = new Viewbox();
            //box.SetValue(Grid.RowProperty, row);
            //box.SetValue(Grid.RowSpanProperty, rowspan);
            //box.SetValue(Grid.ColumnProperty, column);
            //box.SetValue(Grid.ColumnSpanProperty, columnspan);
            radio.Click += method;
            radio.Name = name;
            radio.Content = content;
            radio.SetValue(Grid.RowProperty, row);
            radio.SetValue(Grid.ColumnProperty, column);
            radio.SetValue(Grid.RowSpanProperty, rowspan);
            radio.SetValue(Grid.ColumnSpanProperty, columnspan);
            radio.GroupName = "mode";
            radio.VerticalAlignment = VerticalAlignment.Center;
            radio.HorizontalAlignment = HorizontalAlignment.Center;
            //radio.FontSize = 25;
            radio.RenderTransformOrigin = new Point(0.5, 0.5);
            radio.RenderTransform = new ScaleTransform(1.8, 1.8);
            if (font != null) {
                radio.FontFamily = font;
            }
            if (color == null) {
                radio.Foreground = BaseData.foreGroundColor;
            } else {
                radio.Foreground = color;
            }
            //box.Child = radio;
            //grid.Children.Add(box);
            grid.Children.Add(radio);
        }

        private void BaseMode(object sender, RoutedEventArgs e) {
            mode = HayaoshiMode.Base;
        }

        private void CheckMode(object sender, RoutedEventArgs e) {
            mode = HayaoshiMode.Check;
        }

        private void EndlessMode(object sender, RoutedEventArgs e) {
            mode = HayaoshiMode.Endless;
        }

        private void KeyPush(object sender, KeyEventArgs e) {
            for (int i = 0; i < playerSize; i++) {
                if (e.Key == players[i].Button && CanUseButton(players[i])) {
                    if (phase == HayaoshiPhase.Base || phase == HayaoshiPhase.Yomiage) {
                        players[i].LightLabel.Content = "!";
                        if (phase == HayaoshiPhase.Base) {
                            phase = HayaoshiPhase.Push;
                        } else if (phase == HayaoshiPhase.Yomiage) {
                            phase = HayaoshiPhase.YomiagePush;
                        }
                        Button throughbutton = locButtonDic["through"];
                        BaseData.ChangeButtonContent(ref throughbutton, "無効");
                        pushPlayer = players[i];
                        PlaySound(sounds["button"]);
                        message.Content = "正解ならばoを、不正解ならばxを押してください";
                    }
                }
            }
            if (BaseData.judgingButton.ContainsValue(e.Key)) {
                if (phase == HayaoshiPhase.Push || phase == HayaoshiPhase.YomiagePush) {
                    pushPlayer.LightLabel.Content = "";
                    if (e.Key == BaseData.judgingButton["ok"]) {
                        PlaySound(sounds["correct"]);
                        history.Add((mode, questionNumber, pushPlayer, JudgeStatus.Point));
                        Correct(pushPlayer);
                    } else {
                        PlaySound(sounds["wrong"]);
                        history.Add((mode, questionNumber, pushPlayer, JudgeStatus.Mistake));
                        Wrong(pushPlayer);
                    }
                    message.Content = "出題中";
                    Button throughbutton = locButtonDic["through"];
                    BaseData.ChangeButtonContent(ref throughbutton, "through");
                    phase = HayaoshiPhase.Base;
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
            if (mode != HayaoshiMode.Check) {
                player.Point++;
                player.PointLabel.Content = player.Point.ToString();
                if (player.Point == baseData.WinPoints) {
                    player.Win = true;
                    player.PointLabel.Background = BaseData.winColor;
                    player.NameLabel.Background = BaseData.winColor;
                }
                QuestionNumberAdd();
            }
        }

        private void Wrong(Player player) {
            if (mode != HayaoshiMode.Check) {
                player.Mistake++;
                player.MistakeLabel.Content = player.Mistake.ToString();
                if (player.Mistake == baseData.LoseMistakes) {
                    player.Lose = true;
                    player.MistakeLabel.Background = BaseData.loseColor;
                    player.NameLabel.Background = BaseData.loseColor;
                }
                if (mode == HayaoshiMode.Base) {
                    QuestionNumberAdd();
                }
            }
        }

        private void QuestionNumberAdd() {
            questionNumber++;
            questionNumberLabel.Content = (questionNumber + 1).ToString() + "問目";
        }

        private void ThroughClick(object sender, RoutedEventArgs e) {
            if (phase == HayaoshiPhase.Base || phase == HayaoshiPhase.Yomiage) {
                history.Add((mode, questionNumber, throughPlayer, JudgeStatus.Through));
                Through();
            } else {
                history.Add((mode,questionNumber, throughPlayer, JudgeStatus.Invalid));
                Invalid();
                message.Content = "出題中";
                Button throughButton = locButtonDic["through"];
                BaseData.ChangeButtonContent(ref throughButton, "through");
                pushPlayer.LightLabel.Content = "";
            }
            StopSound();
        }

        private void Through() {
            QuestionNumberAdd();
        }

        private void Invalid() {
            QuestionNumberAdd();
        }

        private void UndoClick(object sender, RoutedEventArgs e) {
            if (phase == HayaoshiPhase.Base || phase == HayaoshiPhase.Yomiage) {
                if (history.Count > 0) {
                    history.RemoveAt(history.Count - 1);
                    StopSound();
                    Reset();
                    foreach ((HayaoshiMode mode, int number, Player player, JudgeStatus judge) in history) {
                        this.mode = mode;
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
                    if (mode == HayaoshiMode.Base) {
                        locRadioDic["base"].IsChecked = true;
                    } else if (mode == HayaoshiMode.Endless) {
                        locRadioDic["endless"].IsChecked = true;
                    } else if (mode == HayaoshiMode.Check) {
                        locRadioDic["check"].IsChecked = true;
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
            phase = HayaoshiPhase.Base;
        }

        private void QuestionClick(object sender, RoutedEventArgs e) {
            if (phase == HayaoshiPhase.Base) {
                StopSound();
                int len = questionSounds.Count();
                if (questionNumber < len) {
                    PlaySound(questionSounds[questionNumber]);
                    playingQuestionSound = questionSounds[questionNumber];
                } else {
                    message.Content = "もう問題がありません";
                }
                phase = HayaoshiPhase.Yomiage;
            }
        }

        private void QuestionSentenceClick(object sender, RoutedEventArgs e) {
            int len = questionStrings.Count();
            if (questionNumber == 0) {
                questionStringLabel.Text = "まだ始まっていません";
                questionAnswerLabel.Text = "A. ？？？";
            } else if (questionNumber - 1 < len) {
                questionStringLabel.Text = questionStrings[questionNumber - 1].sentence;
                questionAnswerLabel.Text = "A. " + questionStrings[questionNumber - 1].answer;
            } else {
                questionStringLabel.Text = "もう問題がありません";
                questionAnswerLabel.Text = "A. ？？？";
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
            //foreach (string item in questionSounds) {
            //    Microsoft.SmallBasic.Library.Sound.Stop(item);
            //}
            if (playingQuestionSound != null) {
                Microsoft.SmallBasic.Library.Sound.Stop(playingQuestionSound);
                playingQuestionSound = null;
            }
        }
    }
}

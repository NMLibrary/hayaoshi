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
using System.Windows.Shapes;

namespace hayaoshi {
    /// <summary>
    /// KeyConfig.xaml の相互作用ロジック
    /// </summary>
    public partial class KeyConfig : Window {

        BaseData baseData;

        public KeyConfig(BaseData baseData) {
            InitializeComponent();

            this.baseData = baseData;

            MakeWindow();
        }

        private void MakeWindow() {
            Background = BaseData.backGroundColor;

            double[] baseGridHeights = new double[3] { 1.0, 3.0, 1.0 };
            for (int i = 0; i < baseGridHeights.Length; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(baseGridHeights[i], GridUnitType.Star);
                baseGrid.RowDefinitions.Add(row);
            }

            Grid configGrid = new Grid();
            configGrid.SetValue(Grid.RowProperty, 1);
            configGrid.SetValue(Grid.ColumnProperty, 0);
            baseGrid.Children.Add(configGrid);

            double[] configGridHeights = new double[3] { 1.0, 1.0, 1.0 };
            for (int i = 0; i < configGridHeights.Length; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(configGridHeights[i], GridUnitType.Star);
                configGrid.RowDefinitions.Add(row);
            }
            for (int i = 0; i < baseData.PlayerNumber + 1; i++) {
                ColumnDefinition row = new ColumnDefinition();
                row.Width = new GridLength(1.0, GridUnitType.Star);
                configGrid.ColumnDefinitions.Add(row);
            }

            Label controllerTitleLabel = new Label();
            Label keyTitleLabel = new Label();
            BaseData.LabelAdd(ref configGrid, ref controllerTitleLabel, "Controller", 1, 0);
            BaseData.LabelAdd(ref configGrid, ref keyTitleLabel, "Key", 2, 0);
            for (int i = 0; i < baseData.PlayerNumber; i++) {
                Label numberLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref numberLabel, (i + 1).ToString(), 0, i + 1);
            }

            for (int i = 0; i < baseData.PlayerNumber; i++) {
                string controllerContent = "";
                if (baseData.NumToJoyDic.ContainsKey(i)) {
                    controllerContent = "○";
                }
                Label controllerLabel = new Label();
                Label controllerTextLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref controllerTextLabel, controllerContent, 1, i + 1);
                controllerLabel.MouseLeftButtonDown += new MouseButtonEventHandler(ControllerSet);
                string keyContent = "";
                if (baseData.NumToKeyDic.ContainsKey(i)) {
                    keyContent = baseData.NumToKeyDic[i].ToString();
                }
                Label keyLabel = new Label();
                BaseData.LabelAdd(ref configGrid, ref keyLabel, keyContent, 2, i + 1);
            }
        }

        private void ControllerSet(object sender, MouseButtonEventArgs e) {
            ((Label)sender).Background = Brushes.Red;
        }
    }
}

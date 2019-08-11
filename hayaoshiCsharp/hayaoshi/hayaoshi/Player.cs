using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace hayaoshi
{
    public class Player
    {
        public string Name { get; set; } = "No Name";
        public Label NameLabel { get; set; }
        public Label KeyLabel { get; set; }
        public Label LightLabel { get; set; }
        public int Point { get; set; } = 0;
        public int Mistake { get; set; } = 0;
        public Label PointLabel { get; set; }
        public Label MistakeLabel { get; set; }
        public Key Button { get; set; }
    }
}

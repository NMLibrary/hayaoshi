using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace hayaoshi
{
    class Player
    {
        public string Name { get; set; }
        public Label NameLabel { get; set; }
        public Label KeyLabel { get; set; }
        public Label LightLabel { get; set; }
        public int Point { get; set; }
        public int Mistake { get; set; }
        public Label PointLabel { get; set; }
        public Label MistakeLabel { get; set; }
        public Key Button { get; set; }
    }
}

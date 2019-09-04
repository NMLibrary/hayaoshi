using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hayaoshi
{
    enum JudgeStatus
    {
        Point, Mistake, Through, Invalid
    }

    enum HayaoshiPhase {
        Base, Yomiage, Push, YomiagePush
    }

    enum HayaoshiMode {
        Check, Base, Endless
    }
}

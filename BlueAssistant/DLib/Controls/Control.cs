using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drake.DLib.Controls
{
    interface Control
    {
        void SetPosition(int x, int y);
        void Show();
        void Hide();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModHearth
{
    //makes the scroll bar show up so its less ugly
    public class VerticalFlowPanel : FlowLayoutPanel
    {

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= 0x00200000; // WS_VSCROLL
                return cp;
            }
        }
    }
}

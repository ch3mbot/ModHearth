using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModHearth
{
    public static class Style
    {
        //modref
        public static Color modRefColor = Color.Gray;
        public static BorderStyle modRefBorder = BorderStyle.None;
        public static Font modRefFont = new Font("Arial", 9, FontStyle.Bold);
        public static int modRefHeight = 16;
        public static Padding modRefPadding = new Padding(2, 1, 2, 0);

        //false ref
        public static Color falseRefColor = Color.Teal;
        public static int falseRefHeight = 4;
        public static Padding falseRefPadding = new Padding(2, 1, 2, 0);

        //main form stuff
        public static int largeBorder = 12;
        public static int smallBorder = 6;

        //right panel
        public static int rightPanelW = 120;

        //left panel
        public static int leftPanelW = 636;
        public static int leftPanelH = 358;
    }
}

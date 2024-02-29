using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Drawing2D;

namespace ModHearth
{
    public class FalseModRef : Panel
    {
        private Pen invalidPen;

        public FalseModRef(Panel parent) 
        {
            this.BackColor = Style.falseRefColor;
            this.Height = 0;

            this.Margin = Style.falseRefPadding;
            this.Width = parent.Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth;

            this.Parent = parent;

            invalidPen = new Pen(Color.Red, 8);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Console.WriteLine("paintin");
            Graphics g = pe.Graphics;
            //Rectangle rect = new Rectangle(Location.X, Location.Y, ClientRectangle.Width, ClientRectangle.Height);
            Rectangle rect = new Rectangle(0, 0, 100, Style.falseRefHeight + 100);
            g.DrawRectangle(invalidPen, rect);

            Point[] pts = {
                new Point(0, 0),
                new Point(0, this.Width) };
            byte[] types = {
                0,
                1 };
            GraphicsPath path = new GraphicsPath(pts, types);
            this.Region = new Region(path);
        }
    }
}

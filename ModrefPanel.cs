using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ModHearth
{
    public class ModrefPanel : Panel
    {
        private bool isDragging = false;
        private Point offset;
        public MainForm form;
        public VerticalFlowPanel vParent => Parent as VerticalFlowPanel;

        public ModReference modref;
        public DFHackMod dfmodref => modref.ToDFHackMod();

        private Label label;

        public static ModrefPanel draggee;

        private Point lastPosition;

        public ModrefPanel(ModReference modref, MainForm form)
        {
            this.form = form;
            this.modref = modref;

            //#fix# name or ID?
            label = new Label();
            label.Text = modref.name + " " + modref.displayedVersion;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Font = Style.modRefFont;
            label.BackColor = Color.Transparent;
            label.Dock = DockStyle.Fill;
            this.Controls.Add(label);

            //this.Anchor = AnchorStyles.Left | AnchorStyles.Right; ;
            this.Margin = Style.modRefPadding;

            label.MouseDown += ModrefPanel_MouseDown;
            label.MouseMove += ModrefPanel_MouseMove;
            label.MouseUp += ModrefPanel_MouseUp;

            this.MouseDown += ModrefPanel_MouseDown;
            this.MouseMove += ModrefPanel_MouseMove;
            this.MouseUp += ModrefPanel_MouseUp;

            this.BackColor = Style.modRefColor;
            this.BorderStyle = Style.modRefBorder;
            this.Margin = Style.modRefPadding;

            this.Visible = true;
        }

        //#fix# needed?
        public void Initialize()
        {
            this.Width = Parent.Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth;
            label.Width = this.Width;
            this.Height = Style.modRefHeight;
        }

        private void ModrefPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            draggee = this;
            Cursor.Current = new Cursor(Resource1.grab_cursor.GetHicon());
        }
        private void ModrefPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if(isDragging)
            {
                Point mousePos = (Control.MousePosition);
                form.ModrefMouseMove(mousePos, this);
                lastPosition = mousePos;
            }
        }
        private void ModrefPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Cursor.Current = Cursors.Default;
            form.ModrefMouseUp(lastPosition, this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BackgroundImage != null)
            {
                e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            };
        }
    }
}

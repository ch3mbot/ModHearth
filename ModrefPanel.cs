using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ModHearth
{
    /// <summary>
    /// A panel representing a mod, to be dragged and dropped for GUI based modpack editing.
    /// Does not make actual changes, just notifies manager classes.
    /// </summary>
    public class ModRefPanel : Panel
    {
        private bool isDragging = false;

        // Reference to the form, to notify of changes.
        public MainForm form;

        // Get the parent of this control as a VerticalFlowPanel (this is always the case).
        public VerticalFlowPanel vParent => Parent as VerticalFlowPanel;

        // Which ModReference this panel represents.
        public ModReference modref;

        // Quick extraction of DFHMod.
        public DFHMod dfmodref => modref.ToDFHMod();

        // The label showing the modrefs name
        private Label label;

        // Which ModrefPanel is currently being dragged. Only one is dragged at once, hence static.
        public static ModRefPanel draggee;

        // Keep track of the last position the mouse was when this is being dragged.
        private Point lastPosition;

        public ModRefPanel(ModReference modref, MainForm form)
        {
            // Basic references.
            this.form = form;
            this.modref = modref;

            // Set up the mod name label.
            label = new Label();
            label.Text = modref.name + " " + modref.displayedVersion;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Font = Style.modRefFont;
            label.BackColor = Color.Transparent;
            label.ForeColor = Style.modRefTextColor;
            label.Dock = DockStyle.Fill;
            this.Controls.Add(label);

            // Set up anchors.
            this.Margin = Style.modRefPadding;

            // Mouse function mapping.
            label.MouseDown += ModrefPanel_MouseDown;
            label.MouseMove += ModrefPanel_MouseMove;
            label.MouseUp += ModrefPanel_MouseUp;

            this.MouseDown += ModrefPanel_MouseDown;
            this.MouseMove += ModrefPanel_MouseMove;
            this.MouseUp += ModrefPanel_MouseUp;
            this.Click += ModrefPanel_Click;

            // Some style things.
            this.BackColor = Style.modRefColor;
            this.BorderStyle = Style.modRefBorder;
            this.Margin = Style.modRefPadding;

            this.Visible = true;
        }

        // This is run once when this object is added to its parent.
        public void Initialize()
        {
            // Set width and height properly.
            this.Width = Parent.Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth;
            label.Width = this.Width;
            this.Height = Style.modRefHeight;
        }

        // On mouse down, set isDragging to true, this to be the draggee, and change cursor.
        private void ModrefPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            draggee = this;
            Cursor.Current = new Cursor(Resource1.grab_cursor.GetHicon());
        }

        // While this is being dragged and gets moved, notify the form, and update the last recorded position.
        private void ModrefPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if(isDragging)
            {
                Point mousePos = (Control.MousePosition);
                form.ModrefMouseMove(mousePos);
                lastPosition = mousePos;
            }
        }

        // When this panel is dropped, reset isDragging, reset the cursor, and notify the form.
        private void ModrefPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Cursor.Current = Cursors.Default;
            form.ModrefMouseUp(lastPosition, this);
        }

        // When this is clicked, show this mod info.
        private void ModrefPanel_Click(object sender, EventArgs e)
        {
            form.ChangeModInfoDisplay(modref);
        }

        // This control paints the background image on top of background color, for highlighting.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BackgroundImage != null)
            {
                e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            };
        }

        // Set this to display problems.
        public void SetProblems(List<ModProblem> problems)
        {
            // Set label color.
            label.ForeColor = Style.modRefTextBadColor;

            // Generate problem tooltip.
            string problemToolTipString = "Problems:";
            foreach (ModProblem problem in problems)
            {
                problemToolTipString += "\n" + problem.ToString();
            }

            // Set tooltips.
            form.toolTip1.SetToolTip(this, problemToolTipString);
            form.toolTip1.SetToolTip(label, problemToolTipString);
        }

        // Set this to be problem free.
        public void RemoveProblems()
        {
            // Reset text color and tooltips.
            label.ForeColor = Style.modRefTextColor;
            form.toolTip1.SetToolTip(this, null);
            form.toolTip1.SetToolTip(label, null);
        }

        // If a filter is applied and this is grayed out.
        public void SetFiltered(bool active)
        {
            if (active)
            {
                label.ForeColor = Style.modRefTextColor;
                label.Font = Style.modRefFont;
            }
            else
            {
                label.ForeColor = Style.modRefTextFilteredColor;
                label.Font = Style.modRefStrikeFont;
            }
        }
    }
}

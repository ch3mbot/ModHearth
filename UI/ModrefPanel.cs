using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ModHearth.UI
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

        // Should this be highlighted up or down
        private bool highlightUp;
        private bool highlightDown;

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
            label.BackColor = Color.Transparent;
            label.Dock = DockStyle.Fill;
            Controls.Add(label);

            // Set up anchors.
            Margin = Style.modRefPadding;

            // Mouse function mapping.
            label.MouseDown += ModrefPanel_MouseDown;
            label.MouseMove += ModrefPanel_MouseMove;
            label.MouseUp += ModrefPanel_MouseUp;

            MouseDown += ModrefPanel_MouseDown;
            MouseMove += ModrefPanel_MouseMove;
            MouseUp += ModrefPanel_MouseUp;
            Click += ModrefPanel_Click;

            // Some style things.
            BorderStyle = Style.modRefBorder;
            Margin = Style.modRefPadding;

            Visible = true;
            highlightUp = false;
            highlightDown = false;

            //BackgroundImage = Resource1.transparent_square;
        }

        // This is run once when this object is added to its parent.
        public void Initialize()
        {
            // Set width and height properly.
            Width = Parent.Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth;
            label.Width = Width;
            Height = Style.modRefHeight;

            // Fix colors as well.
            label.Font = Style.modRefFont;
            label.ForeColor = Style.instance.modRefTextColor;
            BackColor = Style.instance.modRefColor;
        }

        // On mouse down, set isDragging to true, this to be the draggee, and change cursor. Also change draggee color.
        private void ModrefPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            draggee = this;
            Cursor.Current = new Cursor(Resource1.grab_cursor.GetHicon());
            draggee.BackColor = Style.instance.modRefHighlightColor;
        }

        // While this is being dragged and gets moved, notify the form, and update the last recorded position.
        private void ModrefPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point mousePos = MousePosition;
                form.ModrefMouseMove(mousePos);
                lastPosition = mousePos;
            }
        }

        // When this panel is dropped, reset isDragging, reset the cursor, and notify the form. Also reset draggee color.
        private void ModrefPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Cursor.Current = Cursors.Default;
            form.ModrefMouseUp(lastPosition, this);
            draggee.BackColor = Style.instance.modRefColor;
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

            // Style based highlighting.
            if(highlightUp)
            {
                Rectangle topRect = new Rectangle(0, 0, this.Width, 2);
                e.Graphics.FillRectangle(new SolidBrush(Style.instance.modRefHighlightColor), topRect);
            }
            else if (highlightDown)
            {
                Rectangle bottomRect = new Rectangle(0, this.Height - 2, this.Width, 2);
                e.Graphics.FillRectangle(new SolidBrush(Style.instance.modRefHighlightColor), bottomRect);
            }
        }

        // Set the highlight status of this panel. Invalidate on change.
        public void SetHighlight(bool top, bool bottom)
        {
            // If not changed, do nothing.
            if (!(top != highlightUp || bottom != highlightDown))
                return;
            highlightUp = top; 
            highlightDown = bottom;
            Invalidate();
        }

        // Set this to display problems.
        public void SetProblems(List<ModProblem> problems)
        {
            // Set label color.
            label.ForeColor = Style.instance.modRefTextBadColor;

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
            label.ForeColor = Style.instance.modRefTextColor;
            form.toolTip1.SetToolTip(this, null);
            form.toolTip1.SetToolTip(label, null);
        }

        // If a filter is applied and this is grayed out.
        public void SetFiltered(bool active)
        {
            if (active)
            {
                label.ForeColor = Style.instance.modRefTextColor;
                label.Font = Style.modRefFont;
            }
            else
            {
                label.ForeColor = Style.instance.modRefTextFilteredColor;
                label.Font = Style.modRefStrikeFont;
            }
        }
    }
}

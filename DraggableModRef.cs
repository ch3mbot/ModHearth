using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth
{
    public class DraggableModRef : Panel
    {
        private bool isDragging = false;
        private Point offset;
        public Form1 form;

        private int lastIndex = -1;

        public static DraggableModRef draggee;

        private Panel lastInPanel;
        private bool inPanel;

        public ModReference modReference;
        private Label label;

        private bool initialized;

        public DraggableModRef(ModReference modref, Form1 form)
        {
            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);

            this.BackColor = Style.modRefColor;
            this.BorderStyle = Style.modRefBorder;

            //#fix# name or ID?
             label = new Label();
            label.Text = modref.name + " " + modref.displayedVersion;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Font = Style.modRefFont;
            label.BackColor = Color.Transparent;

            label.MouseDown += DraggableModRef_MouseDown;
            label.MouseMove += DraggableModRef_MouseMove;
            label.MouseUp += DraggableModRef_MouseUp;

            this.modReference = modref;

            this.Controls.Add(label);
            this.Margin = Style.modRefPadding;

            //i hate gui
            //this.Dock = DockStyle.Top;
            //this.AutoSize = true;

            //this.Anchor = AnchorStyles.Top;
            //this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            //this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.MouseDown += DraggableModRef_MouseDown;
            this.MouseMove += DraggableModRef_MouseMove;
            this.MouseUp += DraggableModRef_MouseUp;

            this.form = form;

            initialized = false;
        }

        public void Initialize(Control parent) 
        {
            if(initialized)
                return;
            initialized = true; 
            this.Width = parent.Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth;
            label.Width = this.Width;
            this.Height = Style.modRefHeight;
        }

        private void DraggableModRef_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            draggee = this;
            Cursor.Current = new Cursor(Resource1.grab_cursor.GetHicon());
        }

        private void DraggableModRef_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Get the current position of the mouse
                Point mousePos = (Control.MousePosition);
                int foundIndex = -1;
                bool inLeft = asd(form.LeftModlistPanel, form.LeftModlistPanel.PointToClient(mousePos), out int leftIndex);
                bool inRight = asd(form.RightModlistPanel, form.RightModlistPanel.PointToClient(mousePos), out int rightIndex);
                if (inLeft)
                {
                    lastInPanel = form.LeftModlistPanel;
                    foundIndex = leftIndex;
                    inPanel = true;
                }
                else if (inRight)
                {
                    lastInPanel = form.RightModlistPanel;
                    foundIndex = rightIndex;
                    inPanel = true;
                }
                else
                {
                    inPanel = false;
                }
                if(inPanel)
                {
                    if(lastInPanel.PointToClient(mousePos).Y > lastInPanel.Controls[lastInPanel.Controls.Count - 1].Location.Y + Style.modRefHeight / 2 + 1)
                    {
                        foundIndex += 1;
                    }

                    UnsetSurroundingToHighlight(lastIndex, lastInPanel);
                    SetSurroundingToHighlight(foundIndex, lastInPanel);
                    lastIndex = foundIndex;
                }
                else
                {
                    UnsetSurroundingToHighlight(lastIndex, lastInPanel);
                }
            }

        }

        private Panel GetOtherPanel()
        {
            if (lastInPanel == form.LeftModlistPanel)
                return form.RightModlistPanel;
            return form.LeftModlistPanel;
        }

        private void DraggableModRef_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            UnsetSurroundingToHighlight(lastIndex, lastInPanel);
            Cursor.Current = Cursors.Default;

            //do not change anything
            if (!inPanel)
                return;

            //two cases. intra-panel and extra-panel
            Panel sourcePanel = draggee.Parent as Panel;
            int newIndex = lastIndex;
            int oldIndex = sourcePanel.Controls.IndexOf(draggee);
            if (sourcePanel == lastInPanel)
            {
                if (sourcePanel == form.LeftModlistPanel)
                    return;
                //sourcePanel.Controls.RemoveAt(oldIndex);
                if (newIndex > oldIndex)
                {
                    if(newIndex == sourcePanel.Controls.Count)
                    {
                        sourcePanel.Controls.RemoveAt(oldIndex);
                        sourcePanel.Controls.Add(draggee);
                    }
                    else
                    {
                        sourcePanel.Controls.SetChildIndex(draggee, newIndex - 1);
                    }
                }
                else
                {
                    sourcePanel.Controls.SetChildIndex(draggee, newIndex);
                }
                form.manager.ModOrderChange(draggee.modReference, newIndex);
            }
            else
            {
                form.manager.ModAbledChange(draggee.modReference, sourcePanel == form.LeftModlistPanel, newIndex);
                sourcePanel.Controls.Remove(draggee);
                draggee.Parent = lastInPanel;
                lastInPanel.Controls.SetChildIndex(draggee, newIndex);
            }

            Console.WriteLine($"dropped. from is: {sourcePanel.Name} to is: {lastInPanel.Name}");
            //Console.WriteLine("mod list: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            for (int i = 0; i < form.manager.enabledMods.Count; i++)
            {
                Console.WriteLine(form.manager.enabledMods[i].ID);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private bool asd(Control container, Point mouis, out int index)
        {
            for (index = 0; index < container.Controls.Count; index++)
            {
                if (container.Controls[index].Bounds.Contains(mouis))
                {
                    return true;
                }
            }

            if(mouis.Y < container.Height && mouis.X > 0 && mouis.X < container.Width && mouis.Y > container.Controls[container.Controls.Count - 1].Location.Y + 10)
            {
                index = container.Controls.Count - 1;
                return true;
            }

            return false;
        }

        private void SetSurroundingToHighlight(int index, Panel parentPanel)
        {
            if (index > 0)
            {
                parentPanel.Controls[index - 1].BackgroundImage = Resource1.highlight_bottom;
            }
            if(index < parentPanel.Controls.Count)
            {
                parentPanel.Controls[index].BackgroundImage = Resource1.highlight_top;
            }
        }

        private void UnsetSurroundingToHighlight(int index, Panel parentPanel)
        {
            if (index > 0 && index < parentPanel.Controls.Count + 1)
            {
                parentPanel.Controls[index - 1].BackgroundImage = null;
            }
            if (index > -1 && index < parentPanel.Controls.Count)
            {
                parentPanel.Controls[index].BackgroundImage = null;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if(BackgroundImage != null)
            {
                e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            };
        }
    }
}

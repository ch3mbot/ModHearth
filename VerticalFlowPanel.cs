using Microsoft.VisualBasic.Devices;
using ModHearth.V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModHearth
{
    //makes the scroll bar show up so its less ugly
    public class VerticalFlowPanel : FlowLayoutPanel
    {
        public List<DFHackMod> visibleChildren;
        public Dictionary<DFHackMod, ModrefPanel> modrefMap;

        public bool sortedPanel;
        public bool initialized;

        public VerticalFlowPanel() : base()
        {
            initialized = false;
        }

        public Control GetVisibleControlAtIndex(int index)
        {
            return modrefMap[visibleChildren[index]];
        }
        
        public void ReInitialize(List<ModrefPanel> conts, List<DFHackMod> members)
        {
            initialized = false;
            Initialize(conts, members, sortedPanel);
        }

        //initialize, should be called once. Adds all the children controls, and does a single fix operation. #fix# should it?
        public void Initialize(List<ModrefPanel> conts, List<DFHackMod> members, bool sortedPanel) 
        {
            Console.WriteLine($"Initializing {Name} with {conts.Count} controls, {members.Count} of which are enabled.");
            if (initialized)
            {
                throw new Exception("bro what");
            }
            initialized = true;

            //setup map
            modrefMap = new Dictionary<DFHackMod, ModrefPanel>();
            conts.ForEach(x => { modrefMap.Add(x.modref.ToDFHackMod(), x); });

            //suspend, and replace all children
            SuspendLayout();
            Controls.Clear();
            Controls.AddRange(conts.ToArray());
            conts.ForEach(x => { x.Initialize(); });

            //set if this is sorted #fix# should be in some kind of one time constructor ever? Initialize gets re-called on reload.
            this.sortedPanel = sortedPanel;

            Console.WriteLine($"{modrefMap.Count} mods present in map");

            foreach(DFHackMod key in modrefMap.Keys)
            {
                if(key.ToString().StartsWith('d'))
                    Console.WriteLine(key.ToString());
            }

            //set visible children, and fix order
            visibleChildren = members;
            ResumeLayout(true);
            FixVisibleOrder();
        }

        public bool GetIndexAtPosition(Point position, out int index)
        {
            Point pos = PointToClient(position);
            List<Control> visList = GetVisibleModrefs();

            //#fix# make sure 0 is correct here
            if (visList.Count == 0)
            {
                index = 0;
                return true;
            }

            for (index = 0; index < visList.Count; index++)
            {
                if (visList[index].Bounds.Contains(pos) && visList[index].Visible)
                {
                    return true;
                }
            }
            if (pos.Y < Height && pos.X > 0 && pos.X < Width && pos.Y > visList[visList.Count - 1].Location.Y + 10)
            {
                index = visList.Count;
                return true;
            }
            return false;
        }

        public List<Control> GetVisibleModrefs()
        {
            List<Control> list = new List<Control>();
            foreach(Control control in Controls) 
            {
                if(control.Visible)
                    list.Add(control);
            }
            return list;
        }

        //given a new list of what should be visible, updates order and fixes visibility.
        //other classes handle changing mod lists, this just updates visuals given one.
        public void UpdateVisibleOrder(List<DFHackMod> newList)
        {
            visibleChildren = newList;
            FixVisibleOrder();
        }
        public void FixVisibleOrder()
        {
            SuspendLayout();
            if (sortedPanel)
            {
                FixVisibleOrderSorted();
            }
            else
            {
                FixVisibleUnsorted();
            }
            ResumeLayout(true);
        }
        private void FixVisibleOrderSorted()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if(i >= visibleChildren.Count) 
                {
                    Controls[i].Visible = false;
                }
                else if(((ModrefPanel)Controls[i]).dfmodref != visibleChildren[i]) 
                {
                    Controls.SetChildIndex(modrefMap[visibleChildren[i]], i);
                    Controls[i].Visible = true;
                    i--;
                }
            }
        }

        private void FixVisibleUnsorted() 
        {
            for(int i = 0; i < Controls.Count; i++) 
            {
                Controls[i].Visible = visibleChildren.Contains(((ModrefPanel)Controls[i]).dfmodref);
            }
        }

        public void SwapControls(int index1, int index2) 
        {
            Control control1 = Controls[index1];
            Control control2 = Controls[index2];
            Controls.SetChildIndex(control1, index2);
            Controls.SetChildIndex(control2, index1);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= 0x00200000; // WS_VSCROLL
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //if (!needsUpdate)
            //    return; 
            //needsUpdate = false;
            base.OnPaint(e);
        }
    }
}

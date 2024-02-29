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
        public List<DraggableModRef> visibleChildren;

        public bool sortedPanel;

        public VerticalFlowPanel() : base()
        {
            
        }
        
        //initialize, should be called once. Adds all the children controls, and does a single fix operation. #fix# should it?
        public void Initialize(List<DraggableModRef> conts, List<DraggableModRef> members, bool sortedPanel) 
        {
            //suspend, and replace all children
            SuspendLayout();
            Controls.Clear();
            Controls.AddRange(conts.ToArray());

            //set if this is sorted #fix# should be in some kind of one time constructor ever? Initialize gets re-called on reload.
            this.sortedPanel = sortedPanel;
            //set visible children, and fix order
            visibleChildren = members;
            FixVisibleOrder();
        }

        //given a new list of what should be visible, updates order and fixes visibility.
        //other classes handle changing mod lists, this just updates visuals given one.
        public void UpdateVisibleOrder(List<DraggableModRef> newList) 
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
                FixVisibleOrderUnsorted();
            }
            ResumeLayout(true);
        }

        private void FixVisibleOrderSorted() 
        {
            for(int i = 0; i < Controls.Count; i++) 
            {
                if(i >= visibleChildren.Count) 
                {
                    Controls[i].Visible = false;
                }
                else if(Controls[i] != visibleChildren[i]) 
                {
                    Controls.SetChildIndex(visibleChildren[i], i);
                    Controls[i].Visible = true;
                    i--;
                }
            }
        }

        private void FixVisibleOrderUnsorted() 
        {
            for(int i = 0; i < Controls.Count; i++) 
            {
                Controls[i].Visible = visibleChildren.Contains(Controls[i]);
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

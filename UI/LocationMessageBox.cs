using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModHearth.UI
{
    public class LocationMessageBox : Form
    {
        private DialogResult result = DialogResult.None;

        public LocationMessageBox(string message, string title, MessageBoxButtons buttons)
        {
            SuspendLayout();

            // Set form properties.
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.Manual;
            Text = title;

            // Create label to display message.
            Label label = new Label();
            label.Text = message;
            label.AutoSize = true;
            label.Location = new Point(Style.popupMessageBorder, Style.popupMessageBorder);

            // Add label to form.
            this.Controls.Add(label);

            ClientSize = new Size(label.Width + 2 * Style.popupMessageBorder, Style.popupHeight);

            // Get the center of the main form, and the position of the mouse. Calculate the difference between them.
            Point formCenter = new Point(MainForm.instance.Left + MainForm.instance.Width / 2, MainForm.instance.Top + MainForm.instance.Height / 2);
            Point pos = Cursor.Position;
            Point diff = new Point(pos.X - formCenter.X, pos.Y - formCenter.Y);

            // Spawn this form at the cursor position, but moved one 5th towards the center. (one 5th is not specific)
            pos.Offset(new Point(-ClientSize.Width / 2, -Style.popupHeight / 2));
            pos.Offset(-diff.X / 5, -diff.Y / 5);
            Location = pos;

            // Handle buttons.
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    CreateButton("Ok", DialogResult.OK, 1, 1);
                    break;
                case MessageBoxButtons.YesNo:
                    CreateButton("Yes", DialogResult.Yes, 1, 2);
                    CreateButton("No", DialogResult.No, 2, 2);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    CreateButton("Yes", DialogResult.Yes, 1, 3);
                    CreateButton("No", DialogResult.No, 2, 3);
                    CreateButton("Cancel", DialogResult.Cancel, 3, 3);
                    break;
            }

            this.ResumeLayout(false);
        }

        // Show popup to get response.
        public static DialogResult Show(string message, string title, MessageBoxButtons buttons)
        {
            LocationMessageBox box = new LocationMessageBox(message, title, buttons);
            box.ShowDialog();
            return box.result;
        }

        // Create a button given number of buttons, button width, and button area.
        private void CreateButton(string text, DialogResult result, int buttonIndex, int buttonNum)
        {
            Button button = new Button();
            button.Text = text;
            button.DialogResult = result;
            button.Size = new Size(Style.popupButtonWidth, Style.popupButtonHeight);
            button.Location = new Point
            (
                (ClientSize.Width - Style.popupButtonWidth * buttonNum) * buttonIndex / (buttonNum + 1) + Style.popupButtonWidth * (buttonIndex - 1),
                (Style.popupHeight - Style.popupButtonHeight - Style.popupMessageBorder)
            );
            button.Click += (s, e) => { this.Close(); };
            this.Controls.Add(button);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            result = this.DialogResult;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // LocationMessageBox
            // 
            ClientSize = new Size(284, 261);
            Name = "LocationMessageBox";
            ResumeLayout(false);
        }
    }
}

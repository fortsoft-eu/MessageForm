using Microsoft.Win32;
using FSTools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MessageForm {
    public partial class CodeForm : Form {
        private PersistWindowState persistWindowState;
        private Form dialog;

        public CodeForm(string code) {
            Text = Properties.Resources.CaptionCodeForm;
            Icon = Properties.Resources.Icon;

            persistWindowState = new PersistWindowState();
            persistWindowState.Parent = this;
            persistWindowState.WindowStateLoaded += new PersistWindowState.WindowStateEventHandler(OnWindowStateLoaded);
            persistWindowState.WindowStateSaved += new PersistWindowState.WindowStateEventHandler(OnWindowStateSaved);

            InitializeComponent();

            BuildContextMenu();

            richTextBox.Font = new Font("Courier New", 10);
            richTextBox.Text = code;
            buttonWordWrap.Text = richTextBox.WordWrap ? Properties.Resources.MenuItemDontWordWrap : Properties.Resources.MenuItemWordWrap;
        }

        private void BuildContextMenu() {
            richTextBox.ContextMenu = new ContextMenu();
            richTextBox.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler(Copy)));
            richTextBox.ContextMenu.MenuItems.Add("-");
            richTextBox.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemSelectAll, new EventHandler((sender, e) => { richTextBox.SelectAll(); })));
            richTextBox.ContextMenu.MenuItems.Add("-");
            richTextBox.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemWordWrap, new EventHandler((sender, e) => {
                richTextBox.WordWrap = !richTextBox.WordWrap;
                buttonWordWrap.Text = richTextBox.WordWrap ? Properties.Resources.MenuItemDontWordWrap : Properties.Resources.MenuItemWordWrap;
            })));
            richTextBox.ContextMenu.Popup += new EventHandler((sender, e) => { richTextBox.ContextMenu.MenuItems[4].Checked = richTextBox.WordWrap; });
        }

        private void Copy(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrWhiteSpace(richTextBox.SelectedText)) {
                    Clipboard.SetText(richTextBox.SelectedText.Trim());
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.ShowDialog();
            }
        }

        private void GripStyle(object sender, EventArgs e) {
            SizeGripStyle = WindowState == FormWindowState.Normal ? SizeGripStyle.Show : SizeGripStyle.Hide;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode == Keys.W) {
                richTextBox.WordWrap = !richTextBox.WordWrap;
                buttonWordWrap.Text = richTextBox.WordWrap ? Properties.Resources.MenuItemDontWordWrap : Properties.Resources.MenuItemWordWrap;
            } else if (e.Control && e.KeyCode == Keys.C) {
                e.SuppressKeyPress = true;
                try {
                    if (!string.IsNullOrWhiteSpace(richTextBox.SelectedText)) {
                        Clipboard.SetText(richTextBox.SelectedText.Trim());
                    }
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                    dialog.ShowDialog();
                }
            }
        }

        private void SelectionChanged(object sender, EventArgs e) {
            richTextBox.ContextMenu.MenuItems[0].Visible = richTextBox.SelectedText.Trim().Length > 0;
            richTextBox.ContextMenu.MenuItems[2].Visible = richTextBox.SelectedText.Trim().Length != richTextBox.Text.Trim().Length || richTextBox.SelectionStart > 0;
            richTextBox.ContextMenu.MenuItems[1].Visible = richTextBox.ContextMenu.MenuItems[0].Visible && richTextBox.ContextMenu.MenuItems[2].Visible;
            richTextBox.ContextMenu.MenuItems[3].Visible = richTextBox.ContextMenu.MenuItems[0].Visible || richTextBox.ContextMenu.MenuItems[2].Visible;
        }

        private void FormActivated(object sender, EventArgs e) {
            if (dialog != null) {
                dialog.Activate();
            }
        }

        private void CopyToClipboard(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrWhiteSpace(richTextBox.SelectedText)) {
                    Clipboard.SetText(richTextBox.Text.Trim());
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.ShowDialog();
            }
            richTextBox.SelectAll();
            richTextBox.Focus();
        }

        private void OnWindowStateLoaded(object sender, RegistryKey key) {
            try {
                richTextBox.WordWrap = Convert.ToBoolean((int)key.GetValue(Name + "WordWrap", 1));
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
            buttonWordWrap.Text = richTextBox.WordWrap ? Properties.Resources.MenuItemDontWordWrap : Properties.Resources.MenuItemWordWrap;
        }

        private void OnWindowStateSaved(object sender, RegistryKey key) {
            try {
                key.SetValue(Name + "WordWrap", richTextBox.WordWrap ? 1 : 0);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                MessageBox.Show(exception.Message);
            }
        }

        private void WordWrap(object sender, EventArgs e) {
            richTextBox.WordWrap = !richTextBox.WordWrap;
            buttonWordWrap.Text = richTextBox.WordWrap ? Properties.Resources.MenuItemDontWordWrap : Properties.Resources.MenuItemWordWrap;
        }
    }
}

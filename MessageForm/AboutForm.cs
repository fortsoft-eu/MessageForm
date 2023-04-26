/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2020-2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 **
 * Version 1.1.0.0
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace MessageForm {
    public partial class AboutForm : Form {
        private Form dialog;
        private int textBoxClicks;
        private Point location;
        private StringBuilder stringBuilder;
        private Timer textBoxClicksTimer;

        public AboutForm() {
            Text = new StringBuilder()
                .Append(Properties.Resources.CaptionAbout)
                .Append(Constants.Space)
                .Append(Program.GetTitle())
                .ToString();

            textBoxClicksTimer = new Timer();
            textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
            textBoxClicksTimer.Tick += new EventHandler((sender, e) => {
                textBoxClicksTimer.Stop();
                textBoxClicks = 0;
            });

            InitializeComponent();

            SuspendLayout();
            pictureBox.Image = Properties.Resources.Icon.ToBitmap();
            panelProductInfo.ContextMenu = new ContextMenu();
            panelProductInfo.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout,
                new EventHandler(CopyAbout)));
            panelWebsite.ContextMenu = new ContextMenu();
            panelWebsite.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout,
                new EventHandler(CopyAbout)));
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy,
                new EventHandler((sender, e) => textBox.Copy())));
            contextMenu.MenuItems.Add(Constants.Hyphen.ToString());
            contextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemSelectAll,
                new EventHandler((sender, e) => textBox.SelectAll())));
            contextMenu.Popup += new EventHandler((sender, e) => {
                if (!textBox.Focused) {
                    textBox.Focus();
                }
                ((ContextMenu)sender).MenuItems[0].Enabled = textBox.SelectionLength > 0;
                ((ContextMenu)sender).MenuItems[2].Enabled = textBox.SelectionLength < textBox.TextLength;
            });
            textBox.ContextMenu = contextMenu;
            ResumeLayout(false);
            PerformLayout();

            stringBuilder = new StringBuilder()
                .AppendLine(Program.GetTitle())
                .AppendLine(WordWrap(Properties.Resources.Description, labelProductInfo.Font, Width - 80))
                .Append(Properties.Resources.LabelVersion)
                .Append(Constants.Space)
                .AppendLine(Application.ProductVersion);
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length > 0) {
                AssemblyCopyrightAttribute assemblyCopyrightAttribute = (AssemblyCopyrightAttribute)attributes[0];
                stringBuilder.AppendLine(assemblyCopyrightAttribute.Copyright);
            }
            attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
            if (attributes.Length > 0) {
                TargetFrameworkAttribute assemblyCopyrightAttribute = (TargetFrameworkAttribute)attributes[0];
                stringBuilder.Append(Properties.Resources.LabelTargetFramework)
                    .Append(Constants.Space)
                    .AppendLine(assemblyCopyrightAttribute.FrameworkDisplayName);
            }

            labelProductInfo.Text = stringBuilder.ToString();
            labelWebsite.Text = Properties.Resources.LabelWebsite;

            linkLabel.ContextMenu = new ContextMenu();
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemOpenInDefaultBrowser,
                new EventHandler(OpenLink)));
            linkLabel.ContextMenu.MenuItems.Add(Constants.Hyphen.ToString());
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyUrl,
                new EventHandler(CopyLink)));
            linkLabel.ContextMenu.MenuItems.Add(Constants.Hyphen.ToString());
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout,
                new EventHandler(CopyAbout)));
            linkLabel.Text = new StringBuilder()
                .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                .Append(Constants.Slash)
                .Append(Application.ProductName.ToLowerInvariant())
                .Append(Constants.Slash)
                .ToString();
            toolTip.SetToolTip(linkLabel, Properties.Resources.ToolTipVisit);
            button.Text = Properties.Resources.ButtonClose;
            stringBuilder.AppendLine()
                .Append(labelWebsite.Text)
                .Append(Constants.Space)
                .Append(linkLabel.Text);
        }

        private void CopyAbout(object sender, EventArgs e) {
            try {
                Clipboard.SetText(stringBuilder.ToString());
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void CopyLink(object sender, EventArgs e) {
            try {
                Clipboard.SetText(((LinkLabel)((MenuItem)sender).GetContextMenu().SourceControl).Text);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (e.Button.Equals(MouseButtons.Left) || e.Button.Equals(MouseButtons.Middle)) {
                OpenLink((LinkLabel)sender);
            }
        }

        private void OpenLink(object sender, EventArgs e) {
            OpenLink((LinkLabel)((MenuItem)sender).GetContextMenu().SourceControl);
        }

        private void OpenLink(LinkLabel linkLabel) {
            try {
                Process.Start(linkLabel.Text);
                linkLabel.LinkVisited = true;
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                StringBuilder title = new StringBuilder(Program.GetTitle())
                    .Append(Constants.Space)
                    .Append(Constants.EnDash)
                    .Append(Constants.Space)
                    .Append(Properties.Resources.CaptionError);
                dialog = new MessageForm(this, exception.Message, title.ToString(), MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.ShowDialog(this);
            }
        }

        private void OnFormActivated(object sender, EventArgs e) {
            if (dialog != null) {
                dialog.Activate();
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) => textBoxClicksTimer.Dispose();

        private void OnFormLoad(object sender, EventArgs e) {
            linkLabel.Location = new Point(linkLabel.Location.X + labelWebsite.Width + 10, linkLabel.Location.Y);
            button.Select();
            button.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode.Equals(Keys.A)) {
                e.SuppressKeyPress = true;
                if (sender is TextBox) {
                    ((TextBox)sender).SelectAll();
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e) {
            if (!e.Button.Equals(MouseButtons.Left)) {
                textBoxClicks = 0;
                return;
            }
            TextBox textBox = (TextBox)sender;
            textBoxClicksTimer.Stop();
            if (textBox.SelectionLength > 0) {
                textBoxClicks = 2;
            } else if (textBoxClicks.Equals(0) || Math.Abs(e.X - location.X) < 2 && Math.Abs(e.Y - location.Y) < 2) {
                textBoxClicks++;
            } else {
                textBoxClicks = 0;
            }
            location = e.Location;
            if (textBoxClicks.Equals(3)) {
                textBoxClicks = 0;
                NativeMethods.MouseEvent(
                    Constants.MOUSEEVENTF_LEFTUP,
                    Convert.ToUInt32(Cursor.Position.X),
                    Convert.ToUInt32(Cursor.Position.Y),
                    0,
                    0);
                Application.DoEvents();
                if (textBox.Multiline) {
                    char[] chars = textBox.Text.ToCharArray();
                    int selectionEnd = Math.Min(
                        Array.IndexOf(chars, Constants.CarriageReturn, textBox.SelectionStart),
                        Array.IndexOf(chars, Constants.LineFeed, textBox.SelectionStart));
                    if (selectionEnd < 0) {
                        selectionEnd = textBox.TextLength;
                    }
                    selectionEnd = Math.Max(textBox.SelectionStart + textBox.SelectionLength, selectionEnd);
                    int selectionStart = Math.Min(textBox.SelectionStart, selectionEnd);
                    while (--selectionStart > 0
                        && !chars[selectionStart].Equals(Constants.LineFeed)
                        && !chars[selectionStart].Equals(Constants.CarriageReturn)) { }
                    textBox.Select(selectionStart, selectionEnd - selectionStart);
                } else {
                    textBox.SelectAll();
                }
                textBox.Focus();
            } else {
                textBoxClicksTimer.Start();
            }
        }

        private static string WordWrap(string text, Font font, int width) {
            StringBuilder stringBuilder = new StringBuilder();
            StringReader stringReader = new StringReader(text);
            for (string line; (line = stringReader.ReadLine()) != null;) {
                StringBuilder builder = new StringBuilder();
                foreach (string word in line.Split(Constants.Space)) {
                    if (builder.Length.Equals(0)) {
                        builder.Append(word);
                    } else {
                        string str = new StringBuilder()
                            .Append(builder.ToString())
                            .Append(Constants.Space)
                            .Append(word)
                            .ToString();
                        if (TextRenderer.MeasureText(str, font).Width <= width) {
                            builder.Append(Constants.Space)
                                .Append(word);
                        } else {
                            stringBuilder.AppendLine(builder.ToString());
                            builder = new StringBuilder(word);
                        }
                    }
                }
                stringBuilder.AppendLine(builder.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}

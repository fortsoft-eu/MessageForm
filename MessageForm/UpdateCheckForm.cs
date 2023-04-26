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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MessageForm {
    public partial class UpdateCheckForm : Form {
        private Form dialog;
        private string version;
        private Thread thread;

        private delegate void HandleErrorCallback(Exception exception);
        private delegate void ResponseCallback(string version);

        public event EventHandler<UpdateCheckEventArgs> StateSet;

        public UpdateCheckForm(string version) {
            Text = new StringBuilder()
                .Append(Program.GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(Properties.Resources.CaptionUpdateCheck)
                .ToString();
            this.version = version;

            InitializeComponent();

            SuspendLayout();
            label2.ContextMenu = new ContextMenu();
            label2.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyVersion,
                new EventHandler(CopyVersion)));
            label4.ContextMenu = new ContextMenu();
            label4.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyVersion,
                new EventHandler(CopyVersion)));
            label5.ContextMenu = new ContextMenu();
            label5.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy,
                new EventHandler(Copy)));
            label6.Text = Properties.Resources.LabelWebsite;
            linkLabel.ContextMenu = new ContextMenu();
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemOpenInDefaultBrowser,
                new EventHandler(OpenLink)));
            linkLabel.ContextMenu.MenuItems.Add(Constants.Hyphen.ToString());
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyUrl,
                new EventHandler(CopyLink)));
            linkLabel.Text = new StringBuilder()
                .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                .Append(Constants.Slash)
                .Append(Application.ProductName.ToLowerInvariant())
                .Append(Constants.Slash)
                .ToString();
            toolTip.SetToolTip(linkLabel, Properties.Resources.ToolTipVisit);
            button.Text = Properties.Resources.ButtonClose;
            ResumeLayout(false);
            PerformLayout();
        }

        private void CheckForUpdates() {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            StringBuilder stringBuilder = new StringBuilder()
                .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                .Append(Constants.Slash)
                .Append(Application.ProductName.ToLowerInvariant())
                .Append(Constants.Slash)
                .Append(Constants.RemoteApiScriptName)
                .Append(Constants.QuestionMark)
                .Append(Constants.RemoteVariableNameGet)
                .Append(Constants.EqualSign)
                .Append(Constants.RemoteProductLatestVersion);
            try {
                UriBuilder uriBuilder = new UriBuilder(stringBuilder.ToString());
                uriBuilder.Host = new StringBuilder()
                    .Append(Constants.RemoteApiSubdomain)
                    .Append(Constants.Period)
                    .Append(uriBuilder.Host)
                    .ToString();
                uriBuilder.Port = 80;
                uriBuilder.Scheme = Constants.SchemeHttp;
                webRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri.AbsoluteUri);
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (Stream stream = webResponse.GetResponseStream()) {
                    using (StreamReader streamReader = new StreamReader(stream)) {
                        string response = string.Empty;
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(streamReader.ReadToEnd());
                        XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName(Constants.XmlElementVersion);
                        foreach (XmlElement xmlElement in xmlNodeList) {
                            response = xmlElement.InnerText;
                        }
                        if (Regex.IsMatch(response, Constants.VersionRegexPattern)) {
                            Response(response);
                        } else {
                            throw new WebException(Properties.Resources.MessageInvalidResponse);
                        }
                    }
                }
            } catch (Exception exception) {
                HandleError(exception);
            } finally {
                if (webResponse != null) {
                    webResponse.Close();
                }
                if (webRequest != null) {
                    webRequest.Abort();
                }
            }
        }

        private void Response(string version) {
            try {
                if (InvokeRequired) {
                    Invoke(new ResponseCallback(Response), version);
                } else {
                    try {
                        SetState(UpdateChecker.CompareVersion(version, Application.ProductVersion) > 0
                            ? UpdateChecker.State.UpdateAvailable
                            : UpdateChecker.State.UpToDate);
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        SetState(UpdateChecker.State.Error);
                    }
                    SetVersion(version);
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void HandleError(Exception exception) {
            try {
                if (InvokeRequired) {
                    Invoke(new HandleErrorCallback(HandleError), exception);
                } else {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    SetState(UpdateChecker.State.Error, exception.Message);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
                ErrorLog.WriteLine(e);
            }
        }

        private void SetVersion(string version) {
            try {
                label4.Text = version;
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void SetState(UpdateChecker.State state) {
            try {
                SetState(state, null);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void SetState(UpdateChecker.State state, string message) {
            switch (state) {
                case UpdateChecker.State.Connecting:
                    Size = new Size(Size.Width, 158);
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    pictureBox.Image = SystemIcons.Information.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageConnecting : message;
                    break;
                case UpdateChecker.State.UpToDate:
                    Size = new Size(Size.Width, 210);
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    label1.Text = Properties.Resources.LabelCurrentVersion;
                    label2.Text = Application.ProductVersion;
                    label3.Text = Properties.Resources.LabelAvailableVersion;
                    pictureBox.Image = Properties.Resources.OK.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageUpToDate : message;
                    break;
                case UpdateChecker.State.UpdateAvailable:
                    Size = new Size(Size.Width, 210);
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    label1.Text = Properties.Resources.LabelCurrentVersion;
                    label2.Text = Application.ProductVersion;
                    label3.Text = Properties.Resources.LabelAvailableVersion;
                    label3.Font = new Font(label3.Font.FontFamily, label3.Font.Size, FontStyle.Bold);
                    label4.Font = new Font(label4.Font.FontFamily, label4.Font.Size, FontStyle.Bold);
                    pictureBox.Image = SystemIcons.Shield.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageUpdateAvailable : message;
                    label5.Font = new Font(label5.Font.FontFamily, label5.Font.Size, FontStyle.Bold);
                    break;
                default:
                    Size = new Size(Size.Width, 158);
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    pictureBox.Image = SystemIcons.Error.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageUpdateCheckError : message;
                    break;
            }
            StateSet?.Invoke(this, new UpdateCheckEventArgs(this, state, label5.Text));
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            while (thread != null && thread.IsAlive) {
                try {
                    thread.Interrupt();
                    thread = null;
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                }
            }
        }

        private void Copy(object sender, EventArgs e) {
            try {
                Clipboard.SetText(((Label)((MenuItem)sender).GetContextMenu().SourceControl).Text);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void CopyVersion(object sender, EventArgs e) {
            try {
                Clipboard.SetText(((Label)((MenuItem)sender).GetContextMenu().SourceControl).Text);
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
                StringBuilder title = new StringBuilder()
                    .Append(Program.GetTitle())
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

        private void OnFormLoad(object sender, EventArgs e) {
            linkLabel.Location = new Point(linkLabel.Location.X + label6.Width + 10, linkLabel.Location.Y);
            button.Select();
            button.Focus();
            if (string.IsNullOrEmpty(version)) {
                SetState(UpdateChecker.State.Connecting);
                thread = new Thread(new ThreadStart(CheckForUpdates));
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            } else {
                try {
                    if (UpdateChecker.CompareVersion(version, Application.ProductVersion) > 0) {
                        SetState(UpdateChecker.State.UpdateAvailable);
                    } else {
                        SetState(UpdateChecker.State.UpToDate);
                    }
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    SetState(UpdateChecker.State.Error);
                }
                SetVersion(version);
            }
        }
    }
}

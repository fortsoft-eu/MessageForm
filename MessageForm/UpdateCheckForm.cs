using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MessageForm {
    public partial class UpdateCheckForm : Form {
        private string version;
        private Thread thread;
        private Form dialog;

        private delegate void ResponseReceivedCallback(string version);
        private delegate void ErrorHasOccuredCallback(Exception exception);

        public delegate void UpdateCheckFormEventHandler(State state, string mesage);

        public event UpdateCheckFormEventHandler StateSet;

        public UpdateCheckForm(string version) {
            Text = Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionUpdateCheck;
            this.version = version;

            InitializeComponent();

            label2.ContextMenu = new ContextMenu();
            label2.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyVersion, new EventHandler(CopyVersion)));
            label4.ContextMenu = new ContextMenu();
            label4.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyVersion, new EventHandler(CopyVersion)));
            label5.ContextMenu = new ContextMenu();
            label5.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler(Copy)));
            label6.Text = Properties.Resources.LabelWebsite;
            linkLabel.ContextMenu = new ContextMenu();
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyLink, new EventHandler(CopyLink)));
            linkLabel.Text = Properties.Resources.Website.TrimEnd('/').ToLowerInvariant() + '/' + Application.ProductName.ToLowerInvariant() + '/';
            toolTip.SetToolTip(linkLabel, Properties.Resources.ToolTipVisit);
            button.Text = Properties.Resources.ButtonClose;
        }

        private void CheckForUpdates() {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            try {
                webRequest = (HttpWebRequest)WebRequest.Create(Properties.Resources.Website.TrimEnd('/').ToLowerInvariant() + '/' + Application.ProductName.ToLowerInvariant() + "/" + Constants.VersionUrlAppend);
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (Stream stream = webResponse.GetResponseStream()) {
                    StreamReader streamReader = new StreamReader(stream);
                    string response = streamReader.ReadToEnd().Trim();
                    if (Regex.IsMatch(response, Constants.VersionRegexPattern)) {
                        ResponseReceived(response);
                    } else {
                        throw new WebException(Properties.Resources.MessageInvalidResponse);
                    }
                }
            } catch (Exception exception) {
                ErrorHasOccured(exception);
            } finally {
                if (webResponse != null) {
                    webResponse.Close();
                }
                if (webRequest != null) {
                    webRequest.Abort();
                }
            }
        }

        private void ResponseReceived(string version) {
            try {
                if (InvokeRequired) {
                    Invoke(new ResponseReceivedCallback(ResponseReceived), new object[] { version });
                } else {
                    try {
                        if (UpdateChecker.CompareVersion(version, Application.ProductVersion) > 0) {
                            SetState(State.UpdateAvailable);
                        } else {
                            SetState(State.UpToDate);
                        }
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        SetState(State.Error);
                    }
                    SetVersion(version);
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void ErrorHasOccured(Exception exception) {
            try {
                if (InvokeRequired) {
                    Invoke(new ErrorHasOccuredCallback(ErrorHasOccured), new object[] { exception });
                } else {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    SetState(State.Error, exception.Message);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
                ErrorLog.WriteLine(e);
            }
        }

        private void SetVersion(string version) {
            try {
                label4.Text = version;
            } catch (Exception e) {
                Debug.WriteLine(e);
                ErrorLog.WriteLine(e);
            }
        }

        private void SetState(State state) {
            try {
                SetState(state, null);
            } catch (Exception e) {
                Debug.WriteLine(e);
                ErrorLog.WriteLine(e);
            }
        }

        private void SetState(State state, string message) {
            switch (state) {
                case State.Connecting:
                    Size = new Size(Size.Width, 158);
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    pictureBox.Image = SystemIcons.Information.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageConnecting : message;
                    break;
                case State.UpToDate:
                    Size = new Size(Size.Width, 210);
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    label1.Text = Properties.Resources.LabelThisVersion;
                    label2.Text = Application.ProductVersion;
                    label3.Text = Properties.Resources.LabelAvailableVersion;
                    pictureBox.Image = Properties.Resources.OK.ToBitmap();
                    label5.Text = string.IsNullOrEmpty(message) ? Properties.Resources.MessageUpToDate : message;
                    break;
                case State.UpdateAvailable:
                    Size = new Size(Size.Width, 210);
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    label1.Text = Properties.Resources.LabelThisVersion;
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
            StateSet?.Invoke(state, label5.Text);
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
            if (e.Button == MouseButtons.Left) {
                try {
                    Process.Start(((LinkLabel)sender).Text);
                    linkLabel.LinkVisited = true;
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                    dialog.ShowDialog();
                }
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
                SetState(State.Connecting);
                thread = new Thread(new ThreadStart(CheckForUpdates));
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            } else {
                try {
                    if (UpdateChecker.CompareVersion(version, Application.ProductVersion) > 0) {
                        SetState(State.UpdateAvailable);
                    } else {
                        SetState(State.UpToDate);
                    }
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    SetState(State.Error);
                }
                SetVersion(version);
            }
        }

        public enum State {
            Connecting, UpToDate, UpdateAvailable, Error
        }
    }
}

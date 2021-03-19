using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MessageForm {
    public class UpdateChecker {
        private System.Timers.Timer timer;
        private Thread thread;
        private Form parent, dialog;
        private CheckType checkType;
        private Size size;

        private delegate void ResponseReceivedCallback(string version);
        private delegate void ErrorHasOccuredCallback(Exception exception);

        public delegate void UpdateCheckerEventHandler(object sender, Form dialog);
        public delegate void UpdateCheckerStateEventHandler(State state, string mesage);

        public event UpdateCheckerEventHandler DialogCreated;
        public event UpdateCheckerStateEventHandler StateSet;
        public event HelpEventHandler HelpRequested;

        public UpdateChecker() {
            dialog = null;
            timer = new System.Timers.Timer(100);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            timer.Stop();
            thread = new Thread(new ThreadStart(CheckForUpdates));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void OnStateSet(UpdateCheckForm.State state, string mesage) {
            switch (state) {
                case UpdateCheckForm.State.Connecting:
                    StateSet?.Invoke(State.Connecting, mesage);
                    break;
                case UpdateCheckForm.State.UpToDate:
                    StateSet?.Invoke(State.UpToDate, mesage);
                    break;
                case UpdateCheckForm.State.UpdateAvailable:
                    StateSet?.Invoke(State.UpdateAvailable, mesage);
                    break;
                default:
                    StateSet?.Invoke(State.Error, mesage);
                    break;
            }
        }

        public void Check(CheckType checkType) {
            this.checkType = checkType;
            switch (checkType) {
                case CheckType.User:
                    if (dialog == null || !dialog.Visible) {
                        UpdateCheckForm updateCheckForm = new UpdateCheckForm(null);
                        updateCheckForm.Load += new EventHandler(OnLoad);
                        updateCheckForm.StateSet += new UpdateCheckForm.UpdateCheckFormEventHandler(OnStateSet);
                        updateCheckForm.HelpRequested += new HelpEventHandler(OnHelpRequested);
                        dialog = updateCheckForm;
                        DialogCreated?.Invoke(this, updateCheckForm);
                        StateSet?.Invoke(State.Connecting, Properties.Resources.MessageConnecting); //The Connecting state is set in constructor of the form and event is not fired!
                        updateCheckForm.ShowDialog(parent);
                    }
                    break;
                default:
                    timer.Start();
                    break;
            }
        }

        private void OnHelpRequested(object sender, HelpEventArgs hlpevent) {
            HelpRequested?.Invoke(sender, hlpevent);
        }

        private void OnLoad(object sender, EventArgs e) {
            Form form = (Form)sender;
            size = form.Size;
            form.SizeChanged += new EventHandler(OnSizeChanged);
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            Form form = (Form)sender;
            if (size == form.Size) {
                return;
            }
            Point point = new Point(form.Location.X - (form.Width - size.Width) / 2, form.Location.Y - (form.Height - size.Height) / 2);
            if (point.X + form.Size.Width > SystemInformation.VirtualScreen.Width) {
                point.X = SystemInformation.VirtualScreen.Width - form.Size.Width;
            } else if (point.X < SystemInformation.VirtualScreen.Left) {
                point.X = SystemInformation.VirtualScreen.Left;
            }
            if (point.Y + form.Size.Height > SystemInformation.VirtualScreen.Height) {
                point.Y = SystemInformation.VirtualScreen.Height - form.Size.Height;
            } else if (point.Y < SystemInformation.VirtualScreen.Top) {
                point.Y = SystemInformation.VirtualScreen.Top;
            }
            form.Location = point;
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
                if (parent.InvokeRequired) {
                    parent.Invoke(new ResponseReceivedCallback(ResponseReceived), new object[] { version });
                } else {
                    try {
                        if (CompareVersion(version, Application.ProductVersion) > 0) {
                            StateSet?.Invoke(State.UpdateAvailable, Properties.Resources.MessageUpdateAvailable);   //If auto or silent check is performed the StateSet event is fired only if update is available!
                            if (checkType == CheckType.Auto && dialog == null || !dialog.Visible) {
                                UpdateCheckForm updateCheckForm = new UpdateCheckForm(version);
                                dialog = updateCheckForm;
                                DialogCreated?.Invoke(this, updateCheckForm);
                                updateCheckForm.ShowDialog(parent);
                            }
                        }
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void ErrorHasOccured(Exception exception) {
            try {
                if (parent.InvokeRequired) {
                    parent.Invoke(new ErrorHasOccuredCallback(ErrorHasOccured), new object[] { exception });
                } else {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
                ErrorLog.WriteLine(e);
            }
        }

        private void OnParentClosing(object sender, FormClosingEventArgs e) {
            timer.Stop();
            timer.Dispose();
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

        public Form Parent {
            get {
                return parent;
            }
            set {
                parent = value;
                parent.FormClosing += new FormClosingEventHandler(OnParentClosing);
            }
        }

        public static int CompareVersion(string versionA, string versionB) {
            string[] versionASplitted = versionA.Split('.');
            string[] versionBSplitted = versionB.Split('.');
            for (int i = 0; i < versionASplitted.Length; i++) {
                int versionANumber = int.Parse(versionASplitted[i], System.Globalization.NumberStyles.None);
                int versionBNumber = int.Parse(versionBSplitted[i], System.Globalization.NumberStyles.None);
                if (versionANumber > versionBNumber) {
                    return 1;
                }
                if (versionANumber < versionBNumber) {
                    return -1;
                }
            }
            return 0;
        }

        public enum CheckType {
            User, Auto, Silent
        }

        public enum State {
            Connecting, UpToDate, UpdateAvailable, Error
        }
    }
}

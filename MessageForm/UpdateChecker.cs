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
    public sealed class UpdateChecker : IDisposable {
        private CheckType checkType;
        private Form parent, dialog;
        private Settings settings;
        private Size size;
        private System.Timers.Timer timer;
        private Thread thread;

        private delegate void HandleErrorCallback(Exception exception);
        private delegate void ResponseCallback(string version);

        public event EventHandler<UpdateCheckEventArgs> StateChanged;
        public event HelpEventHandler Help;

        public UpdateChecker(Settings settings) {
            this.settings = settings;
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

        public void Check(CheckType checkType) {
            this.checkType = checkType;
            switch (checkType) {
                case CheckType.User:
                    if (dialog == null || !dialog.Visible) {
                        UpdateCheckForm updateCheckForm = new UpdateCheckForm(null);
                        updateCheckForm.Load += new EventHandler(OnLoad);
                        updateCheckForm.StateSet += new EventHandler<UpdateCheckEventArgs>((sender, e) =>
                            StateChanged?.Invoke(sender, e));
                        updateCheckForm.HelpRequested += new HelpEventHandler((sender, hlpevent) => Help?.Invoke(sender, hlpevent));
                        dialog = updateCheckForm;
                        StateChanged?.Invoke(this,
                            new UpdateCheckEventArgs(dialog, State.Connecting, Properties.Resources.MessageConnecting));
                        updateCheckForm.ShowDialog(parent);
                    }
                    break;
                default:
                    try {
                        timer.Start();
                    } catch (Exception exception) {
                        HandleError(exception);
                    }
                    break;
            }
        }

        private void OnLoad(object sender, EventArgs e) {
            Form form = (Form)sender;
            size = form.Size;
            form.SizeChanged += new EventHandler(OnSizeChanged);
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            Form form = (Form)sender;
            if (size.Equals(form.Size)) {
                return;
            }
            Point point = new Point(
                form.Location.X - (form.Width - size.Width) / 2,
                form.Location.Y - (form.Height - size.Height) / 2);
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
                if (parent.InvokeRequired) {
                    parent.Invoke(new ResponseCallback(Response), version);
                } else {
                    try {
                        if (CompareVersion(version, Application.ProductVersion) > 0) {
                            if (checkType.Equals(CheckType.Auto) && dialog == null || !dialog.Visible) {
                                UpdateCheckForm updateCheckForm = new UpdateCheckForm(version);
                                dialog = updateCheckForm;
                                updateCheckForm.ShowDialog(parent);
                            }
                            StateChanged?.Invoke(this,
                                new UpdateCheckEventArgs(dialog, State.UpdateAvailable, Properties.Resources.MessageUpdateAvailable));
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

        private void HandleError(Exception exception) {
            try {
                if (parent.InvokeRequired) {
                    parent.Invoke(new HandleErrorCallback(HandleError), exception);
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

        private void OnParentLoad(object sender, EventArgs e) {
            if (settings.CheckForUpdates) {
                Check(settings.StatusBarNotifOnly ? CheckType.Silent : CheckType.Auto);
            }
        }

        public Form Parent {
            get {
                return parent;
            }
            set {
                parent = value;
                parent.FormClosing += new FormClosingEventHandler(OnParentClosing);
                parent.Load += new EventHandler(OnParentLoad);
            }
        }


        public static int CompareVersion(string versionA, string versionB) {
            string[] versionASplitted = versionA.Split(Constants.Period);
            string[] versionBSplitted = versionB.Split(Constants.Period);
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

        public void Dispose() {
            timer.Stop();
            timer.Dispose();
        }

        public enum CheckType {
            User,
            Auto,
            Silent
        }

        public enum State {
            Connecting,
            UpToDate,
            UpdateAvailable,
            Error
        }
    }
}

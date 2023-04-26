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

using FortSoft.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MessageForm {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            if (!Environment.OSVersion.Platform.Equals(PlatformID.Win32NT)) {
                MessageBox.Show(Properties.Resources.MessageApplicationCannotRun, GetTitle(Properties.Resources.CaptionError),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Settings settings = new Settings();
            if (!settings.DisableThemes) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                settings.RenderWithVisualStyles = Application.RenderWithVisualStyles;
            }
            ArgumentParser argumentParser = new ArgumentParser();
            try {
                argumentParser.Arguments = args;
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (argumentParser.HasArguments) {
                if (argumentParser.IsHelp) {
                    StringBuilder help = new StringBuilder()
                        .AppendLine(Properties.Resources.HelpLine1)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine2)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine3)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine4)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine5)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine6)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine7)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine8)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine9)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine10)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine11)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine12);
                    StringBuilder replace = new StringBuilder()
                        .Append(Constants.BackSlash)
                        .Append(Constants.LowerCaseT);
                    MessageBox.Show(help.ToString().Replace(replace.ToString(), Constants.VerticalTab.ToString()),
                        GetTitle(Properties.Resources.CaptionHelp), MessageBoxButtons.OK, MessageBoxIcon.Question);
                } else if (argumentParser.IsThisTest) {
                    try {
                        Application.Run(new ArgumentParserForm());
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                } else {
                    MessageForm messageForm = null;
                    BackgroundForm backgroundForm = null;
                    try {
                        if (Directory.Exists(Path.GetDirectoryName(argumentParser.OutputFilePath))
                                && !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(argumentParser.OutputFilePath))) {

                            messageForm = new MessageForm(argumentParser.Text, argumentParser.Caption, argumentParser.Buttons,
                                argumentParser.BoxIcon, argumentParser.DefaultButton, true, argumentParser.DisplayHelpButton,
                                argumentParser.MaximumWidth, argumentParser.NoWrap);
                            if (argumentParser.BasicTheme) {
                                messageForm.Show();
                                StaticMethods.ExportAsImage(messageForm, argumentParser.OutputFilePath);
                            } else {
                                backgroundForm = new BackgroundForm();
                                backgroundForm.Show();
                                backgroundForm.Location = new Point(
                                    SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot,
                                    SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
                                backgroundForm.Size = new Size(
                                    messageForm.Width + Constants.BackgroundFormOverShoot * 2,
                                    messageForm.Height + Constants.BackgroundFormOverShoot * 2);
                                messageForm.Show();
                                messageForm.Location = SystemInformation.WorkingArea.Location;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);

                                using (Bitmap bitmap = new Bitmap(
                                        Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width),
                                        Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height),
                                        PixelFormat.Format32bppArgb)) {

                                    Graphics graphics = Graphics.FromImage(bitmap);
                                    graphics.CopyFromScreen(SystemInformation.WorkingArea.Location, Point.Empty, bitmap.Size,
                                        CopyPixelOperation.SourceCopy);
                                    StaticMethods.SaveBitmap(bitmap, argumentParser.OutputFilePath);
                                }
                            }
                        }
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                    } finally {
                        if (messageForm != null) {
                            messageForm.Close();
                        }
                        if (backgroundForm != null) {
                            backgroundForm.Close();
                        }
                    }
                }
            } else {
                try {
                    SingleInstance.Run(new MainForm(settings), GetTitle(), StringComparison.InvariantCulture);
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        public static string GetTitle() {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string title = null;
            if (attributes.Length > 0) {
                AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)attributes[0];
                title = assemblyTitleAttribute.Title;
            }
            return string.IsNullOrEmpty(title) ? Application.ProductName : title;
        }

        public static string GetTitle(string title) {
            return new StringBuilder()
                .Append(GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(title)
                .ToString();
        }

        public static bool IsDebugging {
            get {
                bool isDebugging = false;
                Debugging(ref isDebugging);
                return isDebugging;
            }
        }

        [Conditional("DEBUG")]
        private static void Debugging(ref bool isDebugging) => isDebugging = true;
    }
}

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WebPWrapper;

namespace MessageForm {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                MessageBox.Show(Properties.Resources.MessageApplicationCannotRun, GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(exception.Message, GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (argumentParser.HasArguments) {
                if (argumentParser.IsHelp) {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine1.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine2.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine3.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine4.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine5.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine6.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine7.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine8.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine9.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine10.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine11.Replace("\\t", "\t")).AppendLine();
                    stringBuilder.AppendLine(Properties.Resources.HelpLine12.Replace("\\t", "\t"));
                    MessageBox.Show(stringBuilder.ToString(), GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionHelp, MessageBoxButtons.OK, MessageBoxIcon.Question);
                } else if (argumentParser.IsThisTest) {
                    try {
                        Application.Run(new ArgumentParserForm());
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        MessageBox.Show(exception.Message, GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                } else {
                    MessageForm messageForm = null;
                    BackgroundForm backgroundForm = null;
                    try {
                        if (Directory.Exists(Path.GetDirectoryName(argumentParser.OutputFilePath)) && !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(argumentParser.OutputFilePath))) {
                            messageForm = new MessageForm(argumentParser.Text, argumentParser.Caption, argumentParser.Buttons, argumentParser.BoxIcon, argumentParser.DefaultButton, true, argumentParser.DisplayHelpButton, argumentParser.MaximumWidth, argumentParser.NoWrap);
                            if (argumentParser.BasicTheme) {
                                messageForm.Show();
                                using (Bitmap bitmap = new Bitmap(messageForm.Width, messageForm.Height, PixelFormat.Format32bppArgb)) {
                                    messageForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                                    switch (Path.GetExtension(argumentParser.OutputFilePath).ToLowerInvariant()) {
                                        case Constants.ExtensionBmp:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Bmp);
                                            break;
                                        case Constants.ExtensionGif:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Gif);
                                            break;
                                        case Constants.ExtensionJpg:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Jpeg);
                                            break;
                                        case Constants.ExtensionTif:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Tiff);
                                            break;
                                        case Constants.ExtensionWebP:
                                            using (WebP webP = new WebP()) {
                                                File.WriteAllBytes(argumentParser.OutputFilePath, webP.EncodeLossless(bitmap));
                                            }
                                            break;
                                        default:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Png);
                                            break;
                                    }
                                }
                            } else {
                                backgroundForm = new BackgroundForm();
                                backgroundForm.Show();
                                backgroundForm.Location = new Point(SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot, SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
                                backgroundForm.Size = new Size(messageForm.Width + Constants.BackgroundFormOverShoot * 2, messageForm.Height + Constants.BackgroundFormOverShoot * 2);
                                messageForm.Show();
                                messageForm.Location = SystemInformation.WorkingArea.Location;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);
                                using (Bitmap bitmap = new Bitmap(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height), PixelFormat.Format32bppArgb)) {
                                    Graphics graphics = Graphics.FromImage(bitmap);
                                    graphics.CopyFromScreen(SystemInformation.WorkingArea.Location.X, SystemInformation.WorkingArea.Location.Y, 0, 0, new Size(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height)), CopyPixelOperation.SourceCopy);
                                    switch (Path.GetExtension(argumentParser.OutputFilePath).ToLowerInvariant()) {
                                        case Constants.ExtensionBmp:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Bmp);
                                            break;
                                        case Constants.ExtensionGif:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Gif);
                                            break;
                                        case Constants.ExtensionJpg:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Jpeg);
                                            break;
                                        case Constants.ExtensionTif:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Tiff);
                                            break;
                                        case Constants.ExtensionWebP:
                                            using (WebP webP = new WebP()) {
                                                File.WriteAllBytes(argumentParser.OutputFilePath, webP.EncodeLossless(bitmap));
                                            }
                                            break;
                                        default:
                                            bitmap.Save(argumentParser.OutputFilePath, ImageFormat.Png);
                                            break;
                                    }
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
                    SingleMainForm.Run(new MainForm(settings));
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    MessageBox.Show(exception.Message, GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        public static bool IsDebugging {
            get {
                bool isDebugging = false;
                Debugging(ref isDebugging);
                return isDebugging;
            }
        }

        [Conditional("DEBUG")]
        private static void Debugging(ref bool isDebugging) {
            isDebugging = true;
        }
    }
}

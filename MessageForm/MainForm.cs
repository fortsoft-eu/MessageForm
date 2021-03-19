using FSTools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WebPWrapper;

namespace MessageForm {
    public partial class MainForm : Form {
        private int textBoxClicks;
        private Timer textBoxClicksTimer;
        private Point location;
        private Settings settings;
        private UpdateChecker updateChecker;
        private CodeGenerator codeGenerator;
        private StatusBarPanel statusBarPanel, statusBarPanelInsert, statusBarPanelNumLock, statusBarPanelCapsLock, statusBarPanelScrollLock;
        private FileExtensionFilter fileExtensionFilter;
        private PersistWindowState persistWindowState;
        private System.Timers.Timer statusPanelTimer;
        private SaveFileDialog saveFileDialog;
        private PrintPreviewDialog printPreviewDialog;
        private PrintDocument printDocument;
        private PrintDialog printDialog;
        private PrintAction printAction;
        private Bitmap bitmap;
        private bool cleared;
        private Form dialog;

        public MainForm(Settings settings) {
            Text = Program.GetTitle();
            Icon = Properties.Resources.Icon;
            this.settings = settings;
            dialog = null;
            bitmap = null;

            textBoxClicks = 0;
            textBoxClicksTimer = new Timer();

            printDialog = new PrintDialog();
            printDocument = new PrintDocument();
            printDocument.DocumentName = Program.GetTitle() + Constants.Space + Properties.Resources.CaptionPrintOutput;
            printDocument.BeginPrint += new PrintEventHandler(BeginPrint);
            printDocument.PrintPage += new PrintPageEventHandler(PrintPage);
            printDocument.EndPrint += new PrintEventHandler(EndPrint);
            printDialog.Document = printDocument;
            printAction = PrintAction.PrintToPrinter;
            printPreviewDialog = new PrintPreviewDialog() {
                ShowIcon = false,
                UseAntiAlias = true,
                Document = printDocument
            };
            printPreviewDialog.WindowState = FormWindowState.Normal;
            printPreviewDialog.StartPosition = FormStartPosition.WindowsDefaultBounds;
            printPreviewDialog.HelpRequested += new HelpEventHandler(OpenHelp);

            fileExtensionFilter = new FileExtensionFilter(Constants.DefaultExtensionFilterIndex);

            persistWindowState = new PersistWindowState();
            persistWindowState.Parent = this;
            persistWindowState.DisableSavePosition = settings.MainFormCenterScreen;

            saveFileDialog = new SaveFileDialog() {
                AddExtension = true,
                CheckPathExists = true,
                FileName = Application.ProductName + Properties.Resources.CaptionExport,
                InitialDirectory = string.IsNullOrEmpty(settings.LastExportDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : settings.LastExportDirectory,
                OverwritePrompt = true,
                Title = Properties.Resources.CaptionExport,
                ValidateNames = true
            };
            saveFileDialog.HelpRequest += new EventHandler(OpenHelp);
            BuildMainMenu();

            InitializeComponent();

            statusBarPanel = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                AutoSize = StatusBarPanelAutoSize.Spring,
                Alignment = HorizontalAlignment.Left
            };
            statusBar.Panels.Add(statusBarPanel);

            statusBarPanelCapsLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelCapsLock);

            statusBarPanelNumLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelNumLock);

            statusBarPanelInsert = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelInsert);

            statusBarPanelScrollLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelScrollLock);

            statusBar.ContextMenu = new ContextMenu();
            statusBar.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler((sender, e) => {
                if (!string.IsNullOrEmpty(statusBarPanel.Text)) {
                    try {
                        Clipboard.SetText(statusBarPanel.Text);
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                    }
                }
            })));
            statusBar.ContextMenu.Popup += new EventHandler((sender, e) => {
                ((ContextMenu)sender).MenuItems[0].Visible = !string.IsNullOrEmpty(statusBarPanel.Text);
            });

            foreach (Control control in Controls) {
                if (control is GroupBox) {
                    foreach (RadioButton radioButton in ((GroupBox)control).Controls) {
                        radioButton.ContextMenu = BuildRadioButtonContextMenu();
                    }
                }
            }

            numericUpDown.Maximum = SystemInformation.VirtualScreen.Width;

            statusPanelTimer = new System.Timers.Timer(30000);
            statusPanelTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);

            updateChecker = new UpdateChecker();
            updateChecker.Parent = this;
            updateChecker.StateSet += new UpdateChecker.UpdateCheckerStateEventHandler(OnStateSet);
            updateChecker.DialogCreated += new UpdateChecker.UpdateCheckerEventHandler(OnDialogCreated);
            updateChecker.HelpRequested += new HelpEventHandler(OpenHelp);

            codeGenerator = new CodeGenerator();
            codeGenerator.Parent = this;
            codeGenerator.DialogCreated += new CodeGenerator.CodeGeneratorEventHandler(OnDialogCreated);
            codeGenerator.HelpRequested += new HelpEventHandler(OpenHelp);

            LoadSettings();
            numericUpDown.Select(0, numericUpDown.Text.Length);
        }

        private void LoadSettings() {
            textBox1.Text = string.IsNullOrEmpty(settings.Caption) ? Properties.Resources.DummyCaption : settings.Caption;
            textBox2.Text = string.IsNullOrEmpty(settings.Text) ? Properties.Resources.DummyText : settings.Text;
            checkBox6.Checked = settings.ShowMessageBox;
            checkBox5.Checked = settings.SetParentForm;
            checkBox4.Checked = settings.SetNoWrap;
            checkBox3.Checked = settings.SetMaximumWidth;
            checkBox2.Checked = settings.DisplayHelpButton;
            checkBox1.Checked = settings.BoxCenterScreen;
            numericUpDown.Value = settings.MaximumWidth < numericUpDown.Minimum + 1 || settings.MaximumWidth > numericUpDown.Maximum ? MessageForm.defaultWidth : settings.MaximumWidth;

            RadioButton radioButton;
            foreach (Control control in groupBox1.Controls) {
                if (control is RadioButton) {
                    radioButton = (RadioButton)control;
                    if (string.Equals(((MessageForm.BoxIcon)settings.IconIndex).ToString(), radioButton.Text, StringComparison.Ordinal)) {
                        radioButton.Checked = true;
                        break;
                    }
                }
            }
            foreach (Control control in groupBox2.Controls) {
                if (control is RadioButton) {
                    radioButton = (RadioButton)control;
                    if (string.Equals(((MessageForm.Buttons)settings.ButtonsIndex).ToString(), radioButton.Text, StringComparison.Ordinal)) {
                        radioButton.Checked = true;
                        break;
                    }
                }
            }
            foreach (Control control in groupBox3.Controls) {
                if (control is RadioButton) {
                    radioButton = (RadioButton)control;
                    if (string.Equals(((MessageForm.DefaultButton)settings.DefaultButtonIndex).ToString(), radioButton.Text.Replace(Constants.Space, string.Empty), StringComparison.Ordinal)) {
                        radioButton.Checked = true;
                        break;
                    }
                }
            }
            EnableControls();
            CheckDefaultRadioButtons();
            if (settings.MainFormCenterScreen) {
                StartPosition = FormStartPosition.CenterScreen;
            }
            fileExtensionFilter.SetFilterIndex(settings.ExtensionFilterIndex);
        }

        private void CheckDefaultRadioButtons() {
            foreach (Control control in Controls) {
                if (control is GroupBox) {
                    GroupBox groupBox = (GroupBox)control;
                    RadioButton button = null;
                    foreach (RadioButton radioButton in groupBox.Controls) {
                        if (radioButton.Checked && radioButton.Enabled) {
                            button = radioButton;
                            break;
                        }
                    }
                    if (button == null) {
                        foreach (RadioButton radioButton in groupBox.Controls) {
                            if (radioButton.Enabled) {
                                button = radioButton;
                            }
                        }
                        if (button != null) {
                            button.Checked = true;
                        }
                    }
                }
            }
        }

        private void BuildMainMenu() {
            MainMenu mainMenu = new MainMenu();
            MenuItem menuItemFile = mainMenu.MenuItems.Add(Properties.Resources.MenuItemFile);
            MenuItem menuItemView = mainMenu.MenuItems.Add(Properties.Resources.MenuItemView);
            MenuItem menuItemOptions = mainMenu.MenuItems.Add(Properties.Resources.MenuItemOptions);
            MenuItem menuItemHelp = mainMenu.MenuItems.Add(Properties.Resources.MenuItemHelp);
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExport, new EventHandler(Export), Shortcut.CtrlE));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExportBasic, new EventHandler(ExportBasicTheme), Shortcut.CtrlI));
            menuItemFile.MenuItems.Add("-");
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintPreview, new EventHandler(PrintPreview)));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrint, new EventHandler(Print), Shortcut.CtrlP));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintPreviewBasic, new EventHandler(PrintPreviewBasicTheme)));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintBasic, new EventHandler(PrintBasicTheme), Shortcut.CtrlJ));
            menuItemFile.MenuItems.Add("-");
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExit, new EventHandler(Close), Shortcut.AltF4));
            menuItemView.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemShowDialog, new EventHandler(ShowDialog), Shortcut.CtrlD));
            menuItemView.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemShowCode, new EventHandler(ShowCode), Shortcut.CtrlL));
            if (settings.DisableThemes || settings.RenderWithVisualStyles) {
                menuItemOptions.MenuItems.Add(new MenuItem(settings.DisableThemes ? Properties.Resources.MenuItemEnableThemes : Properties.Resources.MenuItemDisableThemes, new EventHandler(ToggleThemes), Shortcut.CtrlT));
            }
            menuItemOptions.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemApplicationReset, new EventHandler(ApplicationReset)));
            menuItemOptions.MenuItems.Add("-");
            menuItemOptions.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPreferences, new EventHandler(ShowPreferences), Shortcut.CtrlG));
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemOnlineHelp, new EventHandler(OpenHelp), Shortcut.F1));
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCheckForUpdates, new EventHandler(CheckUpdates)));
            menuItemHelp.MenuItems.Add("-");
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemAbout, new EventHandler(ShowAbout)));
            Menu = mainMenu;
        }

        private void Export(object sender, EventArgs e) {
            saveFileDialog.Filter = fileExtensionFilter.GetFilter();
            saveFileDialog.FilterIndex = fileExtensionFilter.GetFilterIndex();
            MessageForm messageForm = null;
            BackgroundForm backgroundForm = null;
            try {
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    if (checkBox5.Checked) {
                        messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    } else {
                        messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    }
                    backgroundForm = new BackgroundForm();
                    backgroundForm.Show(this);
                    backgroundForm.Location = new Point(SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot, SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
                    backgroundForm.Size = new Size(messageForm.Width + Constants.BackgroundFormOverShoot * 2, messageForm.Height + Constants.BackgroundFormOverShoot * 2);
                    messageForm.Show(this);
                    messageForm.Location = SystemInformation.WorkingArea.Location;
                    Cursor = Cursors.WaitCursor;
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);
                    using (Bitmap bitmap = new Bitmap(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height), PixelFormat.Format32bppArgb)) {
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.CopyFromScreen(SystemInformation.WorkingArea.Location.X, SystemInformation.WorkingArea.Location.Y, 0, 0, new Size(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height)), CopyPixelOperation.SourceCopy);
                        switch (Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant()) {
                            case Constants.ExtensionBmp:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                                break;
                            case Constants.ExtensionGif:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Gif);
                                break;
                            case Constants.ExtensionJpg:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                                break;
                            case Constants.ExtensionTif:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Tiff);
                                break;
                            case Constants.ExtensionWebP:
                                using (WebP webP = new WebP()) {
                                    File.WriteAllBytes(saveFileDialog.FileName, webP.EncodeLossless(bitmap));
                                }
                                break;
                            default:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
                                break;
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessageExportFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessageExportFailed, false);
            } finally {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
                settings.LastExportDirectory = saveFileDialog.InitialDirectory;
                SetStatusBarPanelText(Properties.Resources.MessageExportFinished, false);
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                Cursor = Cursors.Default;
                if (messageForm != null) {
                    messageForm.Close();
                }
                if (backgroundForm != null) {
                    backgroundForm.Close();
                }
                fileExtensionFilter.SetFilterIndex(saveFileDialog.FilterIndex);
                settings.ExtensionFilterIndex = fileExtensionFilter.GetFilterIndex();
            }
        }

        private void ExportBasicTheme(object sender, EventArgs e) {
            saveFileDialog.Filter = fileExtensionFilter.GetFilter();
            saveFileDialog.FilterIndex = fileExtensionFilter.GetFilterIndex();
            MessageForm messageForm = null;
            try {
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    if (checkBox5.Checked) {
                        messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    } else {
                        messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    }
                    messageForm.Show(this);
                    using (Bitmap bitmap = new Bitmap(messageForm.Width, messageForm.Height, PixelFormat.Format32bppArgb)) {
                        messageForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        switch (Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant()) {
                            case Constants.ExtensionBmp:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                                break;
                            case Constants.ExtensionGif:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Gif);
                                break;
                            case Constants.ExtensionJpg:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                                break;
                            case Constants.ExtensionTif:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Tiff);
                                break;
                            case Constants.ExtensionWebP:
                                using (WebP webP = new WebP()) {
                                    File.WriteAllBytes(saveFileDialog.FileName, webP.EncodeLossless(bitmap));
                                }
                                break;
                            default:
                                bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
                                break;
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessageExportFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessageExportFailed, false);
            } finally {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
                settings.LastExportDirectory = saveFileDialog.InitialDirectory;
                SetStatusBarPanelText(Properties.Resources.MessageExportFinished, false);
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                if (messageForm != null) {
                    messageForm.Close();
                }
                fileExtensionFilter.SetFilterIndex(saveFileDialog.FilterIndex);
                settings.ExtensionFilterIndex = fileExtensionFilter.GetFilterIndex();
            }
        }

        private void PrintPreview(object sender, EventArgs e) {
            dialog = printPreviewDialog;
            try {
                bitmap = GetMessageFormScreenshot();
                if (printPreviewDialog.ShowDialog() == DialogResult.OK) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, false);
            }
        }

        private void Print(object sender, EventArgs e) {
            try {
                bitmap = GetMessageFormScreenshot();
                if (printDialog.ShowDialog() == DialogResult.OK) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, false);
            }
        }

        private void PrintPreviewBasicTheme(object sender, EventArgs e) {
            dialog = printPreviewDialog;
            try {
                bitmap = GetMessageForm();
                if (printPreviewDialog.ShowDialog() == DialogResult.OK) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, false);
            }
        }

        private void PrintBasicTheme(object sender, EventArgs e) {
            try {
                bitmap = GetMessageForm();
                if (printDialog.ShowDialog() == DialogResult.OK) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, false);
            }
        }

        private void BeginPrint(object sender, PrintEventArgs e) {
            printAction = e.PrintAction;
            printDocument.OriginAtMargins = settings.PrintSoftMargins;
            SetStatusBarPanelText(e.PrintAction == PrintAction.PrintToPreview ? Properties.Resources.MessageGeneratingPreview : Properties.Resources.MessagePrinting, false);
        }

        private void PrintPage(object sender, PrintPageEventArgs e) {
            if (bitmap == null) {
                return;
            }
            try {
                RectangleF marginBounds = e.MarginBounds;
                RectangleF printableArea = e.PageSettings.PrintableArea;
                if (printAction == PrintAction.PrintToPreview) {
                    e.Graphics.TranslateTransform(printableArea.X, printableArea.Y);
                }
                int availableWidth = (int)Math.Floor(printDocument.OriginAtMargins ? marginBounds.Width : e.PageSettings.Landscape ? printableArea.Height : printableArea.Width);
                int availableHeight = (int)Math.Floor(printDocument.OriginAtMargins ? marginBounds.Height : e.PageSettings.Landscape ? printableArea.Width : printableArea.Height);
                Size availableSize = new Size(availableWidth, availableHeight);
                bool rotate = IsGraphicsRotationNeeded(bitmap.Size, availableSize);
                if (rotate) {
                    e.Graphics.RotateTransform(90, MatrixOrder.Prepend);
                }
                Size size = GetNewGraphicsSize(bitmap.Size, availableSize);
                e.Graphics.DrawImage(bitmap, 0, rotate ? -availableWidth : 0, size.Width, size.Height);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, true);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFailed, false);
            }
        }

        private void EndPrint(object sender, PrintEventArgs e) {
            SetStatusBarPanelText(e.PrintAction == PrintAction.PrintToPreview ? Properties.Resources.MessagePreviewGenerated : Properties.Resources.MessagePrintingFinished, false);
        }

        private void Close(object sender, EventArgs e) {
            Close();
        }

        private void ToggleThemes(object sender, EventArgs e) {
            settings.DisableThemes = !settings.DisableThemes;
            SaveSettings();
            dialog = null;
            Close();
            Application.Restart();
        }

        private void ApplicationReset(object sender, EventArgs e) {
            dialog = new MessageForm(this, Properties.Resources.MessageResetWarningLine1 + Environment.NewLine + Properties.Resources.MessageResetWarningLine2, null, MessageForm.Buttons.YesNo, MessageForm.BoxIcon.Warning, MessageForm.DefaultButton.Button2);
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            if (dialog.ShowDialog() == DialogResult.Yes) {
                settings.Clear();
                cleared = true;
                dialog = null;
                Application.Restart();
            }
        }

        private void ShowPreferences(object sender, EventArgs e) {
            PreferencesForm preferencesForm = new PreferencesForm();
            preferencesForm.HelpButtonClicked += new CancelEventHandler(OpenHelp);
            preferencesForm.HelpRequested += new HelpEventHandler(OpenHelp);
            preferencesForm.Settings = settings;
            dialog = preferencesForm;
            if (preferencesForm.ShowDialog() == DialogResult.OK) {
                SetStatusBarPanelText(Properties.Resources.MessagePreferencesSaved, false);
                if (preferencesForm.RestartRequired) {
                    dialog = null;
                    Close();
                    Application.Restart();
                }
            }
        }

        private void OpenHelp(object sender, EventArgs e) {
            try {
                Process.Start(Properties.Resources.Website.TrimEnd('/').ToLowerInvariant() + '/' + Application.ProductName.ToLowerInvariant() + '/');
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog();
            }
        }

        private void CheckUpdates(object sender, EventArgs e) {
            updateChecker.Check(UpdateChecker.CheckType.User);
        }

        private void ShowAbout(object sender, EventArgs e) {
            dialog = new AboutForm();
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog();
        }

        private void ShowDialog(object sender, EventArgs e) {
            DialogResult dialogResult = DialogResult.None;
            SaveSettings();
            StringBuilder stringBuilder = new StringBuilder(Properties.Resources.LabelDialogResult);
            stringBuilder.Append(' ');
            if (checkBox6.Checked) {
                if (checkBox5.Checked) {
                    dialogResult = MessageBox.Show(this, textBox2.Text, textBox1.Text, MessageBoxButtons(), MessageBoxIcon(), MessageBoxDefaultButton());
                } else {
                    dialogResult = MessageBox.Show(textBox2.Text, textBox1.Text, MessageBoxButtons(), MessageBoxIcon(), MessageBoxDefaultButton());
                }
                stringBuilder.Append(dialogResult.ToString());
                SetStatusBarPanelText(stringBuilder.ToString(), true);
            } else {
                MessageForm messageForm;
                if (checkBox5.Checked) {
                    messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                } else {
                    messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                }
                messageForm.HelpButtonClicked += new CancelEventHandler(OnMessageFormHelpButtonClicked);
                messageForm.HelpRequested += new HelpEventHandler(OnMessageFormHelpRequested);
                dialog = messageForm;
                stringBuilder.Append(messageForm.ShowDialog().ToString());
                stringBuilder.Append(CultureInfo.InvariantCulture.TextInfo.ListSeparator);
                stringBuilder.Append(' ');
                stringBuilder.Append(Properties.Resources.LabelClickedButton);
                stringBuilder.Append(' ');
                stringBuilder.Append(messageForm.MessageBoxClickedButton.ToString());
                SetStatusBarPanelText(stringBuilder.ToString(), true);
            }
        }

        private void ShowCode(object sender, EventArgs e) {
            codeGenerator.SetParent = checkBox5.Checked;
            codeGenerator.Caption = textBox1.Text;
            codeGenerator.Text = textBox2.Text;
            codeGenerator.FormButtons = MessageFormButtons();
            codeGenerator.FormBoxIcon = MessageFormBoxIcon();
            codeGenerator.FormDefaultButton = MessageFormDefaultButton();
            codeGenerator.BoxButtons = MessageBoxButtons();
            codeGenerator.BoxIcon = MessageBoxIcon();
            codeGenerator.BoxDefaultButton = MessageBoxDefaultButton();
            codeGenerator.CenterScreen = checkBox1.Checked;
            codeGenerator.ShowHelpButton = checkBox2.Checked;
            codeGenerator.MaximumWidth = checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth;
            codeGenerator.NoWrap = checkBox4.Checked;
            codeGenerator.ShowMessageBox = checkBox6.Checked;
            codeGenerator.ShowCode();
        }

        private void OnMessageFormHelpButtonClicked(object sender, CancelEventArgs e) {
            SetStatusBarPanelText(Properties.Resources.MessageHelpButtonClicked, true);
        }

        private void OnMessageFormHelpRequested(object sender, HelpEventArgs hlpevent) {
            SetStatusBarPanelText(Properties.Resources.MessageHelpRequested, true);
        }

        private void TextBoxMouseDown(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                textBoxClicks = 0;
                return;
            }
            TextBox textBox = (TextBox)sender;
            textBoxClicksTimer.Stop();
            if (textBox.SelectionLength > 0) {
                textBoxClicks = 2;
            } else if (textBoxClicks == 0 || Math.Abs(e.X - location.X) < 2 && Math.Abs(e.Y - location.Y) < 2) {
                textBoxClicks++;
            } else {
                textBoxClicks = 0;
            }
            location = e.Location;
            if (textBoxClicks == 3) {
                if (textBox.Multiline) {
                    int selectionEnd = Math.Min(textBox.Text.IndexOf('\r', textBox.SelectionStart), textBox.Text.IndexOf('\n', textBox.SelectionStart));
                    if (selectionEnd < 0) {
                        selectionEnd = textBox.TextLength;
                    }
                    selectionEnd = Math.Max(textBox.SelectionStart + textBox.SelectionLength, selectionEnd);
                    int selectionStart = Math.Min(textBox.SelectionStart, selectionEnd);
                    do {
                        selectionStart--;
                    } while (selectionStart > 0 && textBox.Text[selectionStart] != '\n' && textBox.Text[selectionStart] != '\r');
                    textBox.Select(selectionStart, selectionEnd - selectionStart);
                } else {
                    textBox.SelectAll();
                }
                textBoxClicks = 0;
                MouseEvent(MOUSEEVENTF_LEFTUP, Convert.ToUInt32(Cursor.Position.X), Convert.ToUInt32(Cursor.Position.X), 0, 0);
                textBox.Focus();
            } else {
                textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
                textBoxClicksTimer.Start();
                textBoxClicksTimer.Tick += new EventHandler((s, t) => {
                    textBoxClicksTimer.Stop();
                    textBoxClicks = 0;
                });
            }
        }

        private void TextBoxKeyPress(object sender, KeyPressEventArgs e) {
            TextBox textBox = (TextBox)sender;
            if (IsKeyLocked(Keys.Insert) && !char.IsControl(e.KeyChar) && !textBox.ReadOnly && textBox.SelectionLength == 0 && textBox.SelectionStart < textBox.TextLength) {
                int selectionStart = textBox.SelectionStart;
                StringBuilder stringBuilder = new StringBuilder(textBox.Text);
                stringBuilder[textBox.SelectionStart] = e.KeyChar;
                e.Handled = true;
                textBox.Text = stringBuilder.ToString();
                textBox.SelectionStart = selectionStart + 1;
            }
        }

        private void NumericUpDownKeyPress(object sender, KeyPressEventArgs e) {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            if (IsKeyLocked(Keys.Insert) && char.IsDigit(e.KeyChar) && !numericUpDown.ReadOnly) {
                FieldInfo fieldInfo = numericUpDown.GetType().GetField("upDownEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                TextBox textBox = (TextBox)fieldInfo.GetValue(numericUpDown);
                if (textBox.SelectionLength == 0 && textBox.SelectionStart < textBox.TextLength) {
                    int selectionStart = textBox.SelectionStart;
                    StringBuilder stringBuilder = new StringBuilder(numericUpDown.Text);
                    stringBuilder[textBox.SelectionStart] = e.KeyChar;
                    e.Handled = true;
                    textBox.Text = stringBuilder.ToString();
                    textBox.SelectionStart = selectionStart + 1;
                }
            }
        }

        private void OnFormLoad(object sender, EventArgs e) {
            Application.Idle += new EventHandler(ApplicationIdle);
            if (settings.CheckForUpdates) {
                updateChecker.Check(settings.StatusBarNotifOnly ? UpdateChecker.CheckType.Silent : UpdateChecker.CheckType.Auto);
            }
        }

        private void OnStateSet(UpdateChecker.State state, string mesage) {
            SetStatusBarPanelText(mesage, false);
        }

        private void OnDialogCreated(object sender, Form form) {
            if (dialog == null || !dialog.Visible) {
                dialog = form;
            }
        }

        private void ApplicationIdle(object sender, EventArgs e) {
            statusBarPanelCapsLock.Text = IsKeyLocked(Keys.CapsLock) ? Properties.Resources.CaptionCapsLock : string.Empty;
            statusBarPanelNumLock.Text = IsKeyLocked(Keys.NumLock) ? Properties.Resources.CaptionNumLock : string.Empty;
            statusBarPanelInsert.Text = IsKeyLocked(Keys.Insert) ? Properties.Resources.CaptionOverWrite : Properties.Resources.CaptionInsert;
            statusBarPanelScrollLock.Text = IsKeyLocked(Keys.Scroll) ? Properties.Resources.CaptionScrollLock : string.Empty;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                switch (settings.EscapeFunction) {
                    case 1:
                        Close();
                        break;
                    case 2:
                        WindowState = FormWindowState.Minimized;
                        break;
                }
            } else if (e.Control && e.KeyCode == Keys.A) {
                e.SuppressKeyPress = true;
                if (sender is TextBox) {
                    ((TextBox)sender).SelectAll();
                } else if (sender is NumericUpDown) {
                    NumericUpDown numericUpDown = (NumericUpDown)sender;
                    numericUpDown.Select(0, numericUpDown.Text.Length);
                }
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            try {
                if (InvokeRequired) {
                    Invoke(new System.Timers.ElapsedEventHandler(OnTimerElapsed), new object[] { sender, e });
                } else {
                    statusPanelTimer.Stop();
                    statusBarPanel.Text = Properties.Resources.MessageReady;
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void OnFormActivated(object sender, EventArgs e) {
            if (dialog != null) {
                dialog.Activate();
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            if (dialog != null && dialog.Visible) {
                e.Cancel = true;
                return;
            }
            statusPanelTimer.Stop();
            statusPanelTimer.Dispose();
            if (!cleared) {
                SaveSettings();
            }
            persistWindowState.DisableSavePosition = false;
            Application.Idle -= new EventHandler(ApplicationIdle);
        }

        private void SetStatusBarPanelText(string text, bool persistent) {
            statusPanelTimer.Stop();
            statusBarPanel.Text = text.EndsWith("...") ? text : text.Trim('.');
            if (!persistent) {
                statusPanelTimer.Start();
            }
        }

        private void MaximumCheckedChanged(object sender, EventArgs e) {
            CheckBox checkBox = (CheckBox)sender;
            numericUpDown.Enabled = checkBox.Checked;
            if (checkBox.Checked) {
                numericUpDown.Select(0, numericUpDown.Value.ToString().Length);
                numericUpDown.Focus();
            }
        }

        private void ShowMessageBoxCheckedChanged(object sender, EventArgs e) {
            EnableControls();
            CheckDefaultRadioButtons();
        }

        private void EnableControls() {
            radioButton10.Enabled = !checkBox6.Checked;
            radioButton11.Enabled = !checkBox6.Checked;
            radioButton12.Enabled = !checkBox6.Checked;
            radioButton13.Enabled = !checkBox6.Checked;
            radioButton14.Enabled = !checkBox6.Checked;
            radioButton15.Enabled = !checkBox6.Checked;
            radioButton22.Enabled = !checkBox6.Checked;
            radioButton23.Enabled = !checkBox6.Checked;
            radioButton27.Enabled = !checkBox6.Checked;
            radioButton28.Enabled = !checkBox6.Checked;
            radioButton29.Enabled = !checkBox6.Checked;
            radioButton30.Enabled = !checkBox6.Checked;
            checkBox1.Enabled = !checkBox6.Checked;
            checkBox2.Enabled = !checkBox6.Checked;
            checkBox3.Enabled = !checkBox6.Checked;
            checkBox4.Enabled = !checkBox6.Checked;
            numericUpDown.Enabled = checkBox3.Checked && !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[0].Enabled = !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[1].Enabled = !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[3].Enabled = !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[4].Enabled = !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[5].Enabled = !checkBox6.Checked;
            Menu.MenuItems[0].MenuItems[6].Enabled = !checkBox6.Checked;
        }

        private MessageForm.Buttons MessageFormButtons() {
            if (radioButton17.Checked) {
                return MessageForm.Buttons.OKCancel;
            } else if (radioButton18.Checked) {
                return MessageForm.Buttons.AbortRetryIgnore;
            } else if (radioButton19.Checked) {
                return MessageForm.Buttons.YesNoCancel;
            } else if (radioButton20.Checked) {
                return MessageForm.Buttons.YesNo;
            } else if (radioButton21.Checked) {
                return MessageForm.Buttons.RetryCancel;
            } else if (radioButton22.Checked) {
                return MessageForm.Buttons.YesAllNoCancel;
            } else if (radioButton23.Checked) {
                return MessageForm.Buttons.DeleteAllSkipCancel;
            } else if (radioButton31.Checked) {
                return MessageForm.Buttons.YesAllNoAll;
            } else {
                return MessageForm.Buttons.OK;
            }
        }

        private MessageBoxButtons MessageBoxButtons() {
            if (radioButton17.Checked) {
                return System.Windows.Forms.MessageBoxButtons.OKCancel;
            } else if (radioButton18.Checked) {
                return System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore;
            } else if (radioButton19.Checked) {
                return System.Windows.Forms.MessageBoxButtons.YesNoCancel;
            } else if (radioButton20.Checked) {
                return System.Windows.Forms.MessageBoxButtons.YesNo;
            } else if (radioButton21.Checked) {
                return System.Windows.Forms.MessageBoxButtons.RetryCancel;
            } else {
                return System.Windows.Forms.MessageBoxButtons.OK;
            }
        }

        private MessageForm.BoxIcon MessageFormBoxIcon() {
            if (radioButton2.Checked) {
                return MessageForm.BoxIcon.Hand;
            } else if (radioButton3.Checked) {
                return MessageForm.BoxIcon.Stop;
            } else if (radioButton4.Checked) {
                return MessageForm.BoxIcon.Error;
            } else if (radioButton5.Checked) {
                return MessageForm.BoxIcon.Question;
            } else if (radioButton6.Checked) {
                return MessageForm.BoxIcon.Exclamation;
            } else if (radioButton7.Checked) {
                return MessageForm.BoxIcon.Warning;
            } else if (radioButton8.Checked) {
                return MessageForm.BoxIcon.Asterisk;
            } else if (radioButton9.Checked) {
                return MessageForm.BoxIcon.Information;
            } else if (radioButton10.Checked) {
                return MessageForm.BoxIcon.OK;
            } else if (radioButton11.Checked) {
                return MessageForm.BoxIcon.Shield;
            } else if (radioButton12.Checked) {
                return MessageForm.BoxIcon.ShieldError;
            } else if (radioButton13.Checked) {
                return MessageForm.BoxIcon.ShieldQuestion;
            } else if (radioButton14.Checked) {
                return MessageForm.BoxIcon.ShieldQuestionRed;
            } else if (radioButton15.Checked) {
                return MessageForm.BoxIcon.ShieldWarning;
            } else if (radioButton28.Checked) {
                return MessageForm.BoxIcon.ShieldOK;
            } else if (radioButton29.Checked) {
                return MessageForm.BoxIcon.WinLogo;
            } else if (radioButton30.Checked) {
                return MessageForm.BoxIcon.Application;
            } else {
                return MessageForm.BoxIcon.None;
            }
        }

        private MessageBoxIcon MessageBoxIcon() {
            if (radioButton2.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Hand;
            } else if (radioButton3.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Stop;
            } else if (radioButton4.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Error;
            } else if (radioButton5.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Question;
            } else if (radioButton6.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Exclamation;
            } else if (radioButton7.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Warning;
            } else if (radioButton8.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Asterisk;
            } else if (radioButton9.Checked) {
                return System.Windows.Forms.MessageBoxIcon.Information;
            } else {
                return System.Windows.Forms.MessageBoxIcon.None;
            }
        }

        private MessageForm.DefaultButton MessageFormDefaultButton() {
            if (radioButton25.Checked) {
                return MessageForm.DefaultButton.Button2;
            } else if (radioButton26.Checked) {
                return MessageForm.DefaultButton.Button3;
            } else if (radioButton27.Checked) {
                return MessageForm.DefaultButton.Button4;
            } else {
                return MessageForm.DefaultButton.Button1;
            }
        }

        private MessageBoxDefaultButton MessageBoxDefaultButton() {
            if (radioButton25.Checked) {
                return System.Windows.Forms.MessageBoxDefaultButton.Button2;
            } else if (radioButton26.Checked) {
                return System.Windows.Forms.MessageBoxDefaultButton.Button3;
            } else {
                return System.Windows.Forms.MessageBoxDefaultButton.Button1;
            }
        }

        private void SaveSettings() {
            settings.Caption = textBox1.Text;
            settings.Text = textBox2.Text;
            settings.IconIndex = (int)MessageFormBoxIcon();
            settings.ButtonsIndex = (int)MessageFormButtons();
            settings.DefaultButtonIndex = (int)MessageFormDefaultButton();
            settings.BoxCenterScreen = checkBox1.Checked;
            settings.DisplayHelpButton = checkBox2.Checked;
            settings.SetMaximumWidth = checkBox3.Checked;
            settings.MaximumWidth = (int)numericUpDown.Value;
            settings.SetNoWrap = checkBox4.Checked;
            settings.SetParentForm = checkBox5.Checked;
            settings.ShowMessageBox = checkBox6.Checked;
            settings.Save();
        }

        private static ContextMenu BuildRadioButtonContextMenu() {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler((sender, e) => {
                try {
                    Clipboard.SetText(((MenuItem)sender).GetContextMenu().SourceControl.Text.Replace(Constants.Space, string.Empty));
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                }
            })));
            return contextMenu;
        }

        private Bitmap GetMessageForm() {
            MessageForm messageForm = null;
            if (checkBox5.Checked) {
                messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            } else {
                messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            }
            messageForm.Show(this);
            Bitmap bitmap = new Bitmap(messageForm.Width, messageForm.Height, PixelFormat.Format32bppArgb);
            messageForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            messageForm.Close();
            return bitmap;
        }

        private Bitmap GetMessageFormScreenshot() {
            MessageForm messageForm = null;
            BackgroundForm backgroundForm = null;
            if (checkBox5.Checked) {
                messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            } else {
                messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(), MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            }
            backgroundForm = new BackgroundForm();
            backgroundForm.Show(this);
            backgroundForm.Location = new Point(SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot, SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
            backgroundForm.Size = new Size(messageForm.Width + Constants.BackgroundFormOverShoot * 2, messageForm.Height + Constants.BackgroundFormOverShoot * 2);
            messageForm.Show(this);
            messageForm.Location = SystemInformation.WorkingArea.Location;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);
            Bitmap bitmap = new Bitmap(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height), PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(SystemInformation.WorkingArea.Location.X, SystemInformation.WorkingArea.Location.Y, 0, 0, new Size(Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width), Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height)), CopyPixelOperation.SourceCopy);
            Cursor = Cursors.Default;
            messageForm.Close();
            backgroundForm.Close();
            return bitmap;
        }

        private static bool IsGraphicsRotationNeeded(Size graphicSize, Size canvasSize) {
            if (graphicSize.Width <= 0 || graphicSize.Height <= 0 || canvasSize.Width <= 0 || canvasSize.Height <= 0) {
                return false;
            }
            if (graphicSize.Width / (float)graphicSize.Height == 1f || canvasSize.Width / (float)canvasSize.Height == 1f) {
                return false;
            }
            if (graphicSize.Width < canvasSize.Width && graphicSize.Height < canvasSize.Height) {
                return false;
            }
            if (graphicSize.Width / (float)graphicSize.Height < 1f && canvasSize.Width / (float)canvasSize.Height < 1f || graphicSize.Width / (float)graphicSize.Height > 1f && canvasSize.Width / (float)canvasSize.Height > 1f) {
                return false;
            }
            return true;
        }

        private static Size GetNewGraphicsSize(Size graphicSize, Size canvasSize) {
            bool rotate = IsGraphicsRotationNeeded(graphicSize, canvasSize);
            float ratio = 1f;
            float ratioWidth = graphicSize.Width / (float)(rotate ? canvasSize.Height : canvasSize.Width);
            float ratioHeight = graphicSize.Height / (float)(rotate ? canvasSize.Width : canvasSize.Height);
            float ratioMax = Math.Max(ratioWidth, ratioHeight);
            if (ratioMax > ratio) {
                ratio = ratioMax;
            }
            return new Size((int)Math.Floor(graphicSize.Width / ratio), (int)Math.Floor(graphicSize.Height / ratio));
        }

        [DllImport("user32.dll", EntryPoint = "mouse_event", SetLastError = true)]
        private static extern void MouseEvent(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
    }
}

/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2020-2024 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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
 * Version 1.1.1.1
 */

using FortSoft.Tools;
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
using System.Text;
using System.Windows.Forms;

namespace MessageForm {
    public partial class MainForm : Form {
        private Bitmap bitmap;
        private bool restart, suppressSaveData;
        private CodeGenerator codeGenerator;
        private FileExtensionFilter fileExtensionFilter;
        private Form dialog;
        private int textBoxClicks;
        private PersistWindowState persistWindowState;
        private Point location;
        private PrintAction printAction;
        private PrintDialog printDialog;
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private SaveFileDialog saveFileDialog;
        private Settings settings;
        private StatusBarPanel statusBarPanel;
        private StatusBarPanel statusBarPanelCapsLock;
        private StatusBarPanel statusBarPanelInsert;
        private StatusBarPanel statusBarPanelNumLock;
        private StatusBarPanel statusBarPanelScrollLock;
        private System.Timers.Timer statusPanelTimer;
        private Timer textBoxClicksTimer;
        private UpdateChecker updateChecker;

        public MainForm(Settings settings) {
            this.settings = settings;

            Icon = Properties.Resources.Icon;
            Text = Program.GetTitle();

            textBoxClicksTimer = new Timer();
            textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
            textBoxClicksTimer.Tick += new EventHandler((sender, e) => {
                textBoxClicksTimer.Stop();
                textBoxClicks = 0;
            });

            printDialog = new PrintDialog();
            printDocument = new PrintDocument();
            printDocument.DocumentName = new StringBuilder()
                .Append(Program.GetTitle())
                .Append(Constants.Space)
                .Append(Properties.Resources.CaptionPrintOutput)
                .ToString();
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

            fileExtensionFilter = new FileExtensionFilter(settings.ExtensionFilterIndex);

            persistWindowState = new PersistWindowState();
            persistWindowState.Parent = this;
            persistWindowState.DisableSavePosition = settings.MainFormCenterScreen;

            saveFileDialog = new SaveFileDialog() {
                AddExtension = true,
                CheckPathExists = true,
                FileName = Application.ProductName + Properties.Resources.CaptionExport,
                InitialDirectory = string.IsNullOrEmpty(settings.LastExportDirectory)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    : settings.LastExportDirectory,
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
            statusBar.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy,
                new EventHandler((sender, e) => {
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

            statusPanelTimer = new System.Timers.Timer(Constants.StatusLblInterval);
            statusPanelTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);

            updateChecker = new UpdateChecker(settings);
            updateChecker.Parent = this;
            updateChecker.StateChanged += new EventHandler<UpdateCheckEventArgs>(OnUpdateCheckerStateChanged);
            updateChecker.Help += new HelpEventHandler(OpenHelp);

            codeGenerator = new CodeGenerator();
            codeGenerator.Parent = this;
            codeGenerator.DialogCreated += new EventHandler<CodeGeneratorEventArgs>(OnDialogCreated);
            codeGenerator.HelpRequested += new HelpEventHandler(OpenHelp);

            LoadSettings();
            numericUpDown.Select(0, numericUpDown.Text.Length);
        }

        private void ApplicationReset(object sender, EventArgs e) {
            StringBuilder message = new StringBuilder()
                .Append(Properties.Resources.MessageResetWarningLine1)
                .Append(Environment.NewLine)
                .Append(Properties.Resources.MessageResetWarningLine2);
            dialog = new MessageForm(this, message.ToString(), null, MessageForm.Buttons.YesNo, MessageForm.BoxIcon.Warning);
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            if (dialog.ShowDialog(this).Equals(DialogResult.Yes)) {
                settings.Clear();
                suppressSaveData = true;
                RestartApplication();
            }
        }

        private void BeginPrint(object sender, PrintEventArgs e) {
            printAction = e.PrintAction;
            printDocument.OriginAtMargins = settings.PrintSoftMargins;
            if (e.PrintAction.Equals(PrintAction.PrintToPreview)) {
                SetStatusBarPanelText(Properties.Resources.MessageGeneratingPreview, false);
            } else {
                SetStatusBarPanelText(Properties.Resources.MessagePrinting, false);
            }
        }

        private void BuildMainMenu() {
            MainMenu mainMenu = new MainMenu();
            MenuItem menuItemFile = mainMenu.MenuItems.Add(Properties.Resources.MenuItemFile);
            MenuItem menuItemView = mainMenu.MenuItems.Add(Properties.Resources.MenuItemView);
            MenuItem menuItemOptions = mainMenu.MenuItems.Add(Properties.Resources.MenuItemOptions);
            MenuItem menuItemHelp = mainMenu.MenuItems.Add(Properties.Resources.MenuItemHelp);
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExport,
                new EventHandler(Export), Shortcut.CtrlE));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExportBasic,
                new EventHandler(ExportBasicTheme), Shortcut.CtrlShiftE));
            menuItemFile.MenuItems.Add(Constants.Hyphen.ToString());
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintPreview,
                new EventHandler(PrintPreview)));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrint,
                new EventHandler(Print), Shortcut.CtrlP));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintPreviewBasic,
                new EventHandler(PrintPreviewBasicTheme)));
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPrintBasic,
                new EventHandler(PrintBasicTheme), Shortcut.CtrlShiftP));
            menuItemFile.MenuItems.Add(Constants.Hyphen.ToString());
            menuItemFile.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemExit,
                new EventHandler(Close), Shortcut.AltF4));
            menuItemView.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemShowDialog,
                new EventHandler(ShowDialog), Shortcut.F5));
            menuItemView.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemShowCode,
                new EventHandler(ShowCode), Shortcut.F7));
            if (settings.DisableThemes || settings.RenderWithVisualStyles) {
                menuItemOptions.MenuItems.Add(new MenuItem(settings.DisableThemes
                        ? Properties.Resources.MenuItemEnableThemes
                        : Properties.Resources.MenuItemDisableThemes,
                    new EventHandler(ToggleThemes), Shortcut.AltF9));
            }
            menuItemOptions.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemApplicationReset,
                new EventHandler(ApplicationReset)));
            menuItemOptions.MenuItems.Add(Constants.Hyphen.ToString());
            menuItemOptions.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemPreferences,
                new EventHandler(ShowPreferences), Shortcut.CtrlG));
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemOnlineHelp,
                new EventHandler(OpenHelp), Shortcut.F1));
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCheckForUpdates,
                new EventHandler(CheckUpdates)));
            menuItemHelp.MenuItems.Add(Constants.Hyphen.ToString());
            menuItemHelp.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemAbout,
                new EventHandler(ShowAbout)));
            Menu = mainMenu;
        }

        private static ContextMenu BuildRadioButtonContextMenu() {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy,
                new EventHandler((sender, e) => {
                    try {
                        Clipboard.SetText(contextMenu.SourceControl.Text.Replace(Constants.Space.ToString(), string.Empty));
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                    }
                })));
            return contextMenu;
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

        private void CheckUpdates(object sender, EventArgs e) => updateChecker.Check(UpdateChecker.CheckType.User);

        private void Close(object sender, EventArgs e) => Close();

        private void EndPrint(object sender, PrintEventArgs e) {
            if (e.PrintAction.Equals(PrintAction.PrintToPreview)) {
                SetStatusBarPanelText(Properties.Resources.MessagePreviewGenerated, false);
            } else {
                SetStatusBarPanelText(Properties.Resources.MessagePrintingFinished, false);
            }
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
            radioButton31.Enabled = !checkBox6.Checked;
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

        private void Export(object sender, EventArgs e) {
            saveFileDialog.Filter = fileExtensionFilter.GetFilter();
            saveFileDialog.FilterIndex = fileExtensionFilter.GetFilterIndex();
            MessageForm messageForm = null;
            BackgroundForm backgroundForm = null;
            try {
                if (saveFileDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    if (checkBox5.Checked) {
                        messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                            MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                            checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    } else {
                        messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                            MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                            checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    }
                    backgroundForm = new BackgroundForm();
                    backgroundForm.Show(this);
                    backgroundForm.Location = new Point(
                        SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot,
                        SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
                    backgroundForm.Size = new Size(
                        messageForm.Width + Constants.BackgroundFormOverShoot * 2,
                        messageForm.Height + Constants.BackgroundFormOverShoot * 2);
                    messageForm.Show(this);
                    messageForm.Location = SystemInformation.WorkingArea.Location;
                    Cursor = Cursors.WaitCursor;
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);

                    using (Bitmap bitmap = new Bitmap(
                            Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width),
                            Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height),
                            PixelFormat.Format32bppArgb)) {

                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.CopyFromScreen(SystemInformation.WorkingArea.Location, Point.Empty, bitmap.Size,
                            CopyPixelOperation.SourceCopy);
                        StaticMethods.SaveBitmap(bitmap, saveFileDialog.FileName);
                    }
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessageExportFailed);
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
                if (saveFileDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    if (checkBox5.Checked) {
                        messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                            MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                            checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    } else {
                        messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                            MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                            checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                    }
                    messageForm.Show(this);
                    StaticMethods.ExportAsImage(messageForm, saveFileDialog.FileName);
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessageExportFailed);
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

        private Bitmap GetMessageForm() {
            MessageForm messageForm = null;
            if (checkBox5.Checked) {
                messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                    MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                    checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            } else {
                messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                    MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                    checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
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
                messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                    MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                    checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            } else {
                messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                    MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                    checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
            }
            backgroundForm = new BackgroundForm();
            backgroundForm.Show(this);
            backgroundForm.Location = new Point(
                SystemInformation.WorkingArea.Location.X - Constants.BackgroundFormOverShoot,
                SystemInformation.WorkingArea.Location.Y - Constants.BackgroundFormOverShoot);
            backgroundForm.Size = new Size(
                messageForm.Width + Constants.BackgroundFormOverShoot * 2,
                messageForm.Height + Constants.BackgroundFormOverShoot * 2);
            messageForm.Show(this);
            messageForm.Location = SystemInformation.WorkingArea.Location;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            System.Threading.Thread.Sleep(Constants.ScreenFormCaptureDelay);
            Bitmap bitmap = new Bitmap(
                Math.Min(SystemInformation.WorkingArea.Width, messageForm.Width),
                Math.Min(SystemInformation.WorkingArea.Height, messageForm.Height),
                PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(SystemInformation.WorkingArea.Location, Point.Empty, bitmap.Size, CopyPixelOperation.SourceCopy);
            Cursor = Cursors.Default;
            messageForm.Close();
            backgroundForm.Close();
            return bitmap;
        }

        private void LoadSettings() {
            textBox1.Text = settings.Caption;
            textBox2.Text = settings.Text;
            checkBox6.Checked = settings.ShowMessageBox;
            checkBox5.Checked = settings.SetParentForm;
            checkBox4.Checked = settings.SetNoWrap;
            checkBox3.Checked = settings.SetMaximumWidth;
            checkBox2.Checked = settings.DisplayHelpButton;
            checkBox1.Checked = settings.BoxCenterScreen;
            if (settings.MaximumWidth < numericUpDown.Minimum + 1 || settings.MaximumWidth > numericUpDown.Maximum) {
                numericUpDown.Value = MessageForm.defaultWidth;
            } else {
                numericUpDown.Value = settings.MaximumWidth;
            }
            string str = ((MessageForm.BoxIcon)settings.IconIndex).ToString();
            foreach (Control control in groupBox1.Controls) {
                if (control is RadioButton) {
                    RadioButton radioButton = (RadioButton)control;
                    if (string.Equals(str, radioButton.Text, StringComparison.Ordinal)) {
                        radioButton.Checked = true;
                        break;
                    }
                }
            }
            str = ((MessageForm.Buttons)settings.ButtonsIndex).ToString();
            foreach (Control control in groupBox2.Controls) {
                if (control is RadioButton) {
                    RadioButton radioButton = (RadioButton)control;
                    if (string.Equals(str, radioButton.Text, StringComparison.Ordinal)) {
                        radioButton.Checked = true;
                        break;
                    }
                }
            }
            str = ((MessageForm.DefaultButton)settings.DefaultButtonIndex).ToString();
            foreach (Control control in groupBox3.Controls) {
                if (control is RadioButton) {
                    RadioButton radioButton = (RadioButton)control;
                    string s = radioButton.Text.Replace(Constants.Space.ToString(), string.Empty);
                    if (string.Equals(str, s, StringComparison.Ordinal)) {
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

        private void MaximumCheckedChanged(object sender, EventArgs e) {
            CheckBox checkBox = (CheckBox)sender;
            numericUpDown.Enabled = checkBox.Checked;
            if (checkBox.Checked) {
                numericUpDown.Select(0, numericUpDown.Value.ToString().Length);
                numericUpDown.Focus();
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

        private MessageBoxDefaultButton MessageBoxDefaultButton() {
            if (radioButton25.Checked) {
                return System.Windows.Forms.MessageBoxDefaultButton.Button2;
            } else if (radioButton26.Checked) {
                return System.Windows.Forms.MessageBoxDefaultButton.Button3;
            } else {
                return System.Windows.Forms.MessageBoxDefaultButton.Button1;
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

        private void OnApplicationIdle(object sender, EventArgs e) {
            statusBarPanelCapsLock.Text = IsKeyLocked(Keys.CapsLock)
                ? Properties.Resources.CaptionCapsLock
                : string.Empty;
            statusBarPanelNumLock.Text = IsKeyLocked(Keys.NumLock)
                ? Properties.Resources.CaptionNumLock
                : string.Empty;
            statusBarPanelInsert.Text = IsKeyLocked(Keys.Insert)
                ? Properties.Resources.CaptionOverWrite
                : Properties.Resources.CaptionInsert;
            statusBarPanelScrollLock.Text = IsKeyLocked(Keys.Scroll)
                ? Properties.Resources.CaptionScrollLock
                : string.Empty;
        }

        private void OnDialogCreated(object sender, CodeGeneratorEventArgs e) {
            if (dialog == null || !dialog.Visible) {
                dialog = e.Dialog;
            }
        }

        private void OnFormLoad(object sender, EventArgs e) {
            Application.Idle += new EventHandler(OnApplicationIdle);
            if (settings.CheckForUpdates) {
                updateChecker.Check(settings.StatusBarNotifOnly ? UpdateChecker.CheckType.Silent : UpdateChecker.CheckType.Auto);
            }
        }

        private void OnUpdateCheckerStateChanged(object sender, UpdateCheckEventArgs e) {
            SetStatusBarPanelText(e.Message, false);
            if (dialog == null || !dialog.Visible) {
                dialog = e.Dialog;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode.Equals(Keys.Escape)) {
                switch (settings.EscapeFunction) {
                    case 1:
                        Close();
                        break;
                    case 2:
                        WindowState = FormWindowState.Minimized;
                        break;
                }
            } else if (e.Control && e.KeyCode.Equals(Keys.A)) {
                e.SuppressKeyPress = true;
                if (sender is TextBox) {
                    ((TextBox)sender).SelectAll();
                } else if (sender is NumericUpDown) {
                    NumericUpDown numericUpDown = (NumericUpDown)sender;
                    numericUpDown.Select(0, numericUpDown.Text.Length);
                }
            }
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e) {
            if (sender is TextBox) {
                TextBox textBox = (TextBox)sender;
                if (IsKeyLocked(Keys.Insert)
                        && !char.IsControl(e.KeyChar)
                        && !textBox.ReadOnly
                        && textBox.SelectionLength.Equals(0)
                        && textBox.SelectionStart < textBox.TextLength) {

                    int selectionStart = textBox.SelectionStart;
                    StringBuilder stringBuilder = new StringBuilder(textBox.Text);
                    stringBuilder[textBox.SelectionStart] = e.KeyChar;
                    e.Handled = true;
                    textBox.Text = stringBuilder.ToString();
                    textBox.SelectionStart = selectionStart + 1;
                }
            } else if (sender is NumericUpDown) {
                NumericUpDown numericUpDown = (NumericUpDown)sender;
                if (IsKeyLocked(Keys.Insert) && char.IsDigit(e.KeyChar) && !numericUpDown.ReadOnly) {
                    FieldInfo fieldInfo = numericUpDown.GetType().GetField(Constants.NumericUpDownEdit,
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    TextBox textBox = (TextBox)fieldInfo.GetValue(numericUpDown);
                    if (textBox.SelectionLength.Equals(0) && textBox.SelectionStart < textBox.TextLength) {
                        int selectionStart = textBox.SelectionStart;
                        StringBuilder stringBuilder = new StringBuilder(numericUpDown.Text);
                        stringBuilder[textBox.SelectionStart] = e.KeyChar;
                        e.Handled = true;
                        textBox.Text = stringBuilder.ToString();
                        textBox.SelectionStart = selectionStart + 1;
                    }
                }
            }
        }

        private void OnMessageFormHelpButtonClicked(object sender, CancelEventArgs e) {
            SetStatusBarPanelText(Properties.Resources.MessageHelpButtonClicked, true);
        }

        private void OnMessageFormHelpRequested(object sender, HelpEventArgs hlpevent) {
            SetStatusBarPanelText(Properties.Resources.MessageHelpRequested, true);
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
                NativeMethods.MouseEvent(Constants.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
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

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            try {
                if (InvokeRequired) {
                    Invoke(new System.Timers.ElapsedEventHandler(OnTimerElapsed), sender, e);
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
            Application.Idle -= new EventHandler(OnApplicationIdle);
            updateChecker.Dispose();
            textBoxClicksTimer.Dispose();
            statusPanelTimer.Stop();
            statusPanelTimer.Dispose();
            if (suppressSaveData) {
                persistWindowState.SavingOptions = PersistWindowState.PersistWindowStateSavingOptions.None;
            } else {
                SaveSettings();
            }
            persistWindowState.DisableSavePosition = false;
            if (restart) {
                Application.Restart();
            }
        }

        private void OpenHelp(object sender, EventArgs e) {
            try {
                StringBuilder url = new StringBuilder()
                    .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                    .Append(Constants.Slash)
                    .Append(Application.ProductName.ToLowerInvariant())
                    .Append(Constants.Slash);
                Process.Start(url.ToString());
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void Print(object sender, EventArgs e) {
            try {
                bitmap = GetMessageFormScreenshot();
                if (printDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessagePrintingFailed);
            }
        }

        private void PrintBasicTheme(object sender, EventArgs e) {
            try {
                bitmap = GetMessageForm();
                if (printDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessagePrintingFailed);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e) {
            if (bitmap == null) {
                return;
            }
            try {
                RectangleF marginBounds = e.MarginBounds;
                RectangleF printableArea = e.PageSettings.PrintableArea;
                if (printAction.Equals(PrintAction.PrintToPreview)) {
                    e.Graphics.TranslateTransform(printableArea.X, printableArea.Y);
                }
                int availableWidth = (int)Math.Floor(printDocument.OriginAtMargins
                    ? marginBounds.Width
                    : e.PageSettings.Landscape ? printableArea.Height : printableArea.Width);
                int availableHeight = (int)Math.Floor(printDocument.OriginAtMargins
                    ? marginBounds.Height
                    : e.PageSettings.Landscape ? printableArea.Width : printableArea.Height);
                Size availableSize = new Size(availableWidth, availableHeight);
                bool rotate = StaticMethods.IsGraphicsRotationNeeded(bitmap.Size, availableSize);
                if (rotate) {
                    e.Graphics.RotateTransform(90, MatrixOrder.Prepend);
                }
                Size size = StaticMethods.GetNewGraphicsSize(bitmap.Size, availableSize);
                e.Graphics.DrawImage(bitmap, 0, rotate ? -availableWidth : 0, size.Width, size.Height);
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessagePrintingFailed);
            }
        }

        private void PrintPreview(object sender, EventArgs e) {
            dialog = printPreviewDialog;
            try {
                bitmap = GetMessageFormScreenshot();
                if (printPreviewDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessagePrintingFailed);
            }
        }

        private void PrintPreviewBasicTheme(object sender, EventArgs e) {
            dialog = printPreviewDialog;
            try {
                bitmap = GetMessageForm();
                if (printPreviewDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    printDocument.Print();
                }
            } catch (Exception exception) {
                ShowException(exception, Properties.Resources.MessagePrintingFailed);
            }
        }

        private void RestartApplication() {
            if (Program.IsDebugging) {
                Close();
            } else {
                restart = true;
                Close();
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

        private void SetStatusBarPanelText(string text, bool persistent) {
            statusPanelTimer.Stop();
            statusBarPanel.Text = text.EndsWith(Constants.ThreeDots) ? text : text.Trim(Constants.Period);
            if (!persistent) {
                statusPanelTimer.Start();
            }
        }

        private void ShowAbout(object sender, EventArgs e) {
            dialog = new AboutForm();
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog();
        }

        private void ShowCode(object sender, EventArgs e) {
            codeGenerator.BoxButtons = MessageBoxButtons();
            codeGenerator.BoxDefaultButton = MessageBoxDefaultButton();
            codeGenerator.BoxIcon = MessageBoxIcon();
            codeGenerator.Caption = textBox1.Text;
            codeGenerator.CenterScreen = checkBox1.Checked;
            codeGenerator.FormBoxIcon = MessageFormBoxIcon();
            codeGenerator.FormButtons = MessageFormButtons();
            codeGenerator.FormDefaultButton = MessageFormDefaultButton();
            codeGenerator.MaximumWidth = checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth;
            codeGenerator.NoWrap = checkBox4.Checked;
            codeGenerator.SetParent = checkBox5.Checked;
            codeGenerator.ShowHelpButton = checkBox2.Checked;
            codeGenerator.ShowMessageBox = checkBox6.Checked;
            codeGenerator.Text = textBox2.Text;
            codeGenerator.ShowCode();
        }

        private void ShowDialog(object sender, EventArgs e) {
            DialogResult dialogResult = DialogResult.None;
            SaveSettings();
            StringBuilder stringBuilder = new StringBuilder(Properties.Resources.LabelDialogResult);
            stringBuilder.Append(Constants.Space);
            if (checkBox6.Checked) {
                if (checkBox5.Checked) {
                    dialogResult = MessageBox.Show(this, textBox2.Text, textBox1.Text, MessageBoxButtons(), MessageBoxIcon(),
                        MessageBoxDefaultButton());
                } else {
                    dialogResult = MessageBox.Show(textBox2.Text, textBox1.Text, MessageBoxButtons(), MessageBoxIcon(),
                        MessageBoxDefaultButton());
                }
                stringBuilder.Append(dialogResult.ToString());
                SetStatusBarPanelText(stringBuilder.ToString(), true);
            } else {
                MessageForm messageForm;
                if (checkBox5.Checked) {
                    messageForm = new MessageForm(this, textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                        MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                        checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                } else {
                    messageForm = new MessageForm(textBox2.Text, textBox1.Text, MessageFormButtons(), MessageFormBoxIcon(),
                        MessageFormDefaultButton(), checkBox1.Checked, checkBox2.Checked,
                        checkBox3.Checked ? (int)numericUpDown.Value : MessageForm.defaultWidth, checkBox4.Checked);
                }
                messageForm.HelpButtonClicked += new CancelEventHandler(OnMessageFormHelpButtonClicked);
                messageForm.HelpRequested += new HelpEventHandler(OnMessageFormHelpRequested);
                dialog = messageForm;
                stringBuilder.Append(messageForm.ShowDialog().ToString())
                    .Append(CultureInfo.InvariantCulture.TextInfo.ListSeparator)
                    .Append(Constants.Space)
                    .Append(Properties.Resources.LabelClickedButton)
                    .Append(Constants.Space)
                    .Append(messageForm.MessageBoxClickedButton.ToString());
                SetStatusBarPanelText(stringBuilder.ToString(), true);
            }
        }

        private void ShowException(Exception exception) => ShowException(exception, null);

        private void ShowException(Exception exception, string statusMessage) {
            Debug.WriteLine(exception);
            ErrorLog.WriteLine(exception);
            SetStatusBarPanelText(string.IsNullOrEmpty(statusMessage) ? exception.Message : statusMessage, true);
            StringBuilder title = new StringBuilder()
                .Append(Program.GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(Properties.Resources.CaptionError);
            dialog = new MessageForm(this, exception.Message, title.ToString(), MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog(this);
            SetStatusBarPanelText(string.IsNullOrEmpty(statusMessage) ? exception.Message : statusMessage, false);
        }

        private void ShowMessageBoxCheckedChanged(object sender, EventArgs e) {
            EnableControls();
            CheckDefaultRadioButtons();
        }

        private void ShowPreferences(object sender, EventArgs e) {
            PreferencesForm preferencesForm = new PreferencesForm();
            preferencesForm.HelpButtonClicked += new CancelEventHandler(OpenHelp);
            preferencesForm.HelpRequested += new HelpEventHandler(OpenHelp);
            preferencesForm.Settings = settings;
            dialog = preferencesForm;
            if (preferencesForm.ShowDialog(this).Equals(DialogResult.OK)) {
                SetStatusBarPanelText(Properties.Resources.MessagePreferencesSaved, false);
                if (preferencesForm.RestartRequired) {
                    RestartApplication();
                }
            }
        }

        private void ToggleThemes(object sender, EventArgs e) {
            settings.DisableThemes = !settings.DisableThemes;
            RestartApplication();
        }
    }
}

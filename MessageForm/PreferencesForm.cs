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
using System.Windows.Forms;

namespace MessageForm {
    public partial class PreferencesForm : Form {
        private Settings settings;
        private bool disableThemes, mainFormCenterScreen;

        public PreferencesForm() {
            InitializeComponent();
            pictureBox.Image = Properties.Resources.Warning.ToBitmap();
        }

        public bool RestartRequired { get; set; }

        public Settings Settings {
            get {
                return settings;
            }
            set {
                settings = value;
                if (settings.PrintSoftMargins) {
                    radioButton2.Checked = true;
                } else {
                    radioButton1.Checked = true;
                }
                checkBox6.Checked = settings.MainFormCenterScreen;
                checkBox5.Checked = settings.EscapeFunction.Equals(1);
                checkBox1.Checked = settings.EscapeFunction.Equals(2);
                checkBox2.Checked = settings.DisableThemes;
                checkBox2.Visible = settings.DisableThemes || settings.RenderWithVisualStyles;
                checkBox3.Checked = settings.CheckForUpdates;
                checkBox4.Checked = settings.StatusBarNotifOnly;
                checkBox4.Enabled = checkBox3.Checked;
                disableThemes = settings.DisableThemes;
                mainFormCenterScreen = settings.MainFormCenterScreen;
                SetWarning();
            }
        }

        private void OnAutoUpdatesCheckedChanged(object sender, EventArgs e) => checkBox4.Enabled = checkBox3.Checked;

        private void OnExitOnEscCheckedChanged(object sender, EventArgs e) {
            if (checkBox5.Checked) {
                checkBox1.Checked = false;
            }
        }

        private void OnMinimizeOnEscCheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                checkBox5.Checked = false;
            }
        }

        private void Save(object sender, EventArgs e) {
            settings.PrintSoftMargins = radioButton2.Checked;
            settings.MainFormCenterScreen = checkBox6.Checked;
            settings.EscapeFunction = checkBox1.Checked ? 2 : checkBox5.Checked ? 1 : 0;
            settings.DisableThemes = checkBox2.Checked;
            settings.CheckForUpdates = checkBox3.Checked;
            settings.StatusBarNotifOnly = checkBox4.Checked;
            settings.Save();
            RestartRequired = !settings.DisableThemes.Equals(disableThemes)
                || !settings.MainFormCenterScreen.Equals(mainFormCenterScreen) && settings.MainFormCenterScreen;
            DialogResult = DialogResult.OK;
        }

        private void SetWarning() {
            if ((settings.DisableThemes || settings.RenderWithVisualStyles)
                        && (checkBox2.Checked && Application.RenderWithVisualStyles
                            || !checkBox2.Checked && !Application.RenderWithVisualStyles)
                    || !checkBox6.Checked.Equals(settings.MainFormCenterScreen) && checkBox6.Checked) {

                pictureBox.Visible = true;
                label.Visible = true;
            } else {
                pictureBox.Visible = false;
                label.Visible = false;
            }
        }

        private void SetWarning(object sender, EventArgs e) => SetWarning();
    }
}

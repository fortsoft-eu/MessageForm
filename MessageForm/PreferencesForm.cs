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
                checkBox5.Checked = settings.EscapeFunction == 1;
                checkBox1.Checked = settings.EscapeFunction == 2;
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

        private void SetWarning() {
            if ((settings.DisableThemes || settings.RenderWithVisualStyles) && (checkBox2.Checked && Application.RenderWithVisualStyles || !checkBox2.Checked && !Application.RenderWithVisualStyles) || checkBox6.Checked != settings.MainFormCenterScreen && checkBox6.Checked) {
                pictureBox.Visible = true;
                label.Visible = true;
            } else {
                pictureBox.Visible = false;
                label.Visible = false;
            }
        }

        private void SetWarning(object sender, EventArgs e) {
            SetWarning();
        }

        private void Save(object sender, EventArgs e) {
            settings.PrintSoftMargins = radioButton2.Checked;
            settings.MainFormCenterScreen = checkBox6.Checked;
            settings.EscapeFunction = checkBox1.Checked ? 2 : checkBox5.Checked ? 1 : 0;
            settings.DisableThemes = checkBox2.Checked;
            settings.CheckForUpdates = checkBox3.Checked;
            settings.StatusBarNotifOnly = checkBox4.Checked;
            settings.Save();
            RestartRequired = settings.DisableThemes != disableThemes || settings.MainFormCenterScreen != mainFormCenterScreen && settings.MainFormCenterScreen;
            DialogResult = DialogResult.OK;
        }

        private void AutoUpdatesCheckedChanged(object sender, EventArgs e) {
            checkBox4.Enabled = checkBox3.Checked;
        }

        private void ExitOnEscCheckedChanged(object sender, EventArgs e) {
            if (checkBox5.Checked) {
                checkBox1.Checked = false;
            }
        }

        private void MinimizeOnEscCheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                checkBox5.Checked = false;
            }
        }
    }
}

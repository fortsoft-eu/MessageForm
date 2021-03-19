using FSTools;

namespace MessageForm {
    public class Settings {
        private PersistentSettings persistentSettings;

        public Settings() {
            persistentSettings = new PersistentSettings();
            PrintSoftMargins = true;
            CheckForUpdates = true;
            SetParentForm = true;
            EscapeFunction = 1;
            Load();
        }

        public string Caption { get; set; }
        public string Text { get; set; }
        public int IconIndex { get; set; }
        public int ButtonsIndex { get; set; }
        public int DefaultButtonIndex { get; set; }
        public bool BoxCenterScreen { get; set; }
        public bool DisplayHelpButton { get; set; }
        public bool SetMaximumWidth { get; set; }
        public int MaximumWidth { get; set; }
        public bool SetNoWrap { get; set; }
        public bool SetParentForm { get; set; }
        public bool ShowMessageBox { get; set; }
        public bool PrintSoftMargins { get; set; }
        public bool MainFormCenterScreen { get; set; }
        public int EscapeFunction { get; set; }
        public bool DisableThemes { get; set; }
        public string LastExportDirectory { get; set; }
        public int ExtensionFilterIndex { get; set; }
        public bool CheckForUpdates { get; set; }
        public bool StatusBarNotifOnly { get; set; }

        private void Load() {
            Caption = persistentSettings.Load("Caption", Caption);
            Text = persistentSettings.Load("Text", Text);
            IconIndex = persistentSettings.Load("IconIndex", IconIndex);
            ButtonsIndex = persistentSettings.Load("ButtonsIndex", ButtonsIndex);
            DefaultButtonIndex = persistentSettings.Load("DefaultButtonIndex", DefaultButtonIndex);
            BoxCenterScreen = persistentSettings.Load("BoxCenterScreen", BoxCenterScreen);
            DisplayHelpButton = persistentSettings.Load("DisplayHelpButton", DisplayHelpButton);
            SetMaximumWidth = persistentSettings.Load("SetMaximumWidth", SetMaximumWidth);
            MaximumWidth = persistentSettings.Load("MaximumWidth", MaximumWidth);
            SetNoWrap = persistentSettings.Load("SetNoWrap", SetNoWrap);
            SetParentForm = persistentSettings.Load("SetParentForm", SetParentForm);
            ShowMessageBox = persistentSettings.Load("ShowMessageBox", ShowMessageBox);
            PrintSoftMargins = persistentSettings.Load("PrintSoftMargins", PrintSoftMargins);
            MainFormCenterScreen = persistentSettings.Load("MainFormCenterScreen", MainFormCenterScreen);
            EscapeFunction = persistentSettings.Load("EscapeFunction", EscapeFunction);
            DisableThemes = persistentSettings.Load("DisableThemes", DisableThemes);
            LastExportDirectory = persistentSettings.Load("LastExportDir", LastExportDirectory);
            ExtensionFilterIndex = persistentSettings.Load("ExtFilterIndex", ExtensionFilterIndex);
            CheckForUpdates = persistentSettings.Load("CheckForUpdates", CheckForUpdates);
            StatusBarNotifOnly = persistentSettings.Load("StatusBarNotifOnly", StatusBarNotifOnly);
        }

        public void Save() {
            persistentSettings.Save("Caption", Caption);
            persistentSettings.Save("Text", Text);
            persistentSettings.Save("IconIndex", IconIndex);
            persistentSettings.Save("ButtonsIndex", ButtonsIndex);
            persistentSettings.Save("DefaultButtonIndex", DefaultButtonIndex);
            persistentSettings.Save("BoxCenterScreen", BoxCenterScreen);
            persistentSettings.Save("DisplayHelpButton", DisplayHelpButton);
            persistentSettings.Save("SetMaximumWidth", SetMaximumWidth);
            persistentSettings.Save("MaximumWidth", MaximumWidth);
            persistentSettings.Save("SetNoWrap", SetNoWrap);
            persistentSettings.Save("SetParentForm", SetParentForm);
            persistentSettings.Save("ShowMessageBox", ShowMessageBox);
            persistentSettings.Save("PrintSoftMargins", PrintSoftMargins);
            persistentSettings.Save("MainFormCenterScreen", MainFormCenterScreen);
            persistentSettings.Save("EscapeFunction", EscapeFunction);
            persistentSettings.Save("DisableThemes", DisableThemes);
            persistentSettings.Save("LastExportDir", LastExportDirectory);
            persistentSettings.Save("ExtFilterIndex", ExtensionFilterIndex);
            persistentSettings.Save("CheckForUpdates", CheckForUpdates);
            persistentSettings.Save("StatusBarNotifOnly", StatusBarNotifOnly);
        }

        public bool RenderWithVisualStyles { get; set; }

        public void Clear() {
            persistentSettings.Clear();
        }
    }
}

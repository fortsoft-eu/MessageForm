namespace MessageForm {
    public static class Constants {
        public const int DefaultExtensionFilterIndex = 4;   //PNG

        public const int BackgroundFormOverShoot = 25;      //Pixels
        public const int ScreenFormCaptureDelay = 500;      //Millisecond

        public const string ExtensionPng = ".png";
        public const string ExtensionBmp = ".bmp";
        public const string ExtensionGif = ".gif";
        public const string ExtensionJpg = ".jpg";
        public const string ExtensionTif = ".tif";
        public const string ExtensionWebP = ".webp";

        public const string CodeGeneratorMsgFormInst = "MessageForm messageForm = new MessageForm(";
        public const string CodeGeneratorMsgFormShowDlg = "messageForm.ShowDialog(";
        public const string CodeGeneratorMsgBoxInst = "MessageBox.Show(";
        public const string CodeGeneratorThis = "this";
        public const string CodeGeneratorNull = "null";
        public const string CodeGeneratorCommaAndSpace = ", ";
        public const string CodeGeneratorEndBracketAndSemicolon = ");";

        public const string ErrorLog = "error.log";
        public const string ErrorLogTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public const string EnDash = "–";
        public const string Space = " ";

        public const string VersionRegexPattern = "^\\d+\\.\\d+\\.\\d+\\.\\d$";
        public const string VersionUrlAppend = "version";

        public const string ExampleCaption = "\"Example \"\"Quotes\"\" Caption\"";
        public const string ExampleText = "\"Embedded \"\"Quotes\"\" White Space\"";
        public const string ExampleOutputFilePath = "C:\\Program Files\\Example Application\\example.png";
    }
}

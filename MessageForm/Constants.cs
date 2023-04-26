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

namespace MessageForm {

    /// <summary>
    /// Constants used in many places in the application.
    /// </summary>
    public static class Constants {

        /// <summary>
        /// Over shoot of the BackgroundForm for capturing the window area of the
        /// MessageForm.
        /// </summary>
        public const int BackgroundFormOverShoot = 25;

        /// <summary>
        /// The default width of the old AboutForm.
        /// </summary>
        public const int DefaultAboutFormWidth = 420;

        /// <summary>
        ///The number of spaces used for indentation.
        /// </summary>
        public const int Indentation = 4;

        /// <summary>
        /// The time delay in milliseconds after which the window area will be
        /// captured.
        /// </summary>
        public const int ScreenFormCaptureDelay = 500;

        /// <summary>
        /// The time interval in seconds to refresh the StatusBar labels.
        /// </summary>
        public const int StatusLblInterval = 30;

        /// <summary>
        /// Windows API constants.
        /// </summary>
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MONITORPOWER = 0xF170;
        public const int SC_SCREENSAVE = 0xF140;
        public const int SC_TASKLIST = 0xF130;
        public const int WM_CLEAR = 0x0303;
        public const int WM_COPY = 0x0301;
        public const int WM_CUT = 0x0300;
        public const int WM_HSCROLL = 0x114;
        public const int WM_PASTE = 0x0302;
        public const int WM_SYSCOMMAND = 0x112;

        /// <summary>
        /// Characters used in many places in the application code.
        /// </summary>
        public const char BackSlash = '\\';
        public const char CarriageReturn = '\r';
        public const char ClosingParenthesis = ')';
        public const char Colon = ':';
        public const char Comma = ',';
        public const char Eight = '8';
        public const char EnDash = '–';
        public const char EqualSign = '=';
        public const char Hyphen = '-';
        public const char LineFeed = '\n';
        public const char LowerCaseT = 't';
        public const char OpeningParenthesis = '(';
        public const char Period = '.';
        public const char QuestionMark = '?';
        public const char QuotationMark = '"';
        public const char Semicolon = ';';
        public const char Seven = '7';
        public const char Slash = '/';
        public const char Space = ' ';
        public const char Two = '2';
        public const char VerticalBar = '|';
        public const char VerticalTab = '\t';
        public const char Zero = '0';

        /// <summary>
        /// Strings used in many places in the application code.
        /// </summary>
        public const string CodeGeneratorMsgBoxInst = "MessageBox.Show";
        public const string CodeGeneratorMsgFormInst = "MessageForm messageForm = new MessageForm";
        public const string CodeGeneratorMsgFormShowDlg = "messageForm.ShowDialog";
        public const string CodeGeneratorNull = "null";
        public const string CodeGeneratorStringEmpty = "string.Empty";
        public const string CodeGeneratorThis = "this";
        public const string CommandLineSwitchUB = "-b";
        public const string CommandLineSwitchUC = "-c";
        public const string CommandLineSwitchUD = "-d";
        public const string CommandLineSwitchUE = "-e";
        public const string CommandLineSwitchUH = "-h";
        public const string CommandLineSwitchUI = "-i";
        public const string CommandLineSwitchUM = "-m";
        public const string CommandLineSwitchUO = "-o";
        public const string CommandLineSwitchUQ = "-?";
        public const string CommandLineSwitchUS = "-s";
        public const string CommandLineSwitchUT = "-t";
        public const string CommandLineSwitchUU = "-T";
        public const string CommandLineSwitchUW = "-w";
        public const string CommandLineSwitchWB = "/b";
        public const string CommandLineSwitchWC = "/c";
        public const string CommandLineSwitchWD = "/d";
        public const string CommandLineSwitchWE = "/e";
        public const string CommandLineSwitchWH = "/h";
        public const string CommandLineSwitchWI = "/i";
        public const string CommandLineSwitchWM = "/m";
        public const string CommandLineSwitchWO = "/o";
        public const string CommandLineSwitchWQ = "/?";
        public const string CommandLineSwitchWS = "/s";
        public const string CommandLineSwitchWT = "/t";
        public const string CommandLineSwitchWU = "/T";
        public const string CommandLineSwitchWW = "/w";
        public const string ErrorLogEmptyString = "[Empty String]";
        public const string ErrorLogErrorMessage = "ERROR MESSAGE";
        public const string ErrorLogFileName = "Error.log";
        public const string ErrorLogNull = "[null]";
        public const string ErrorLogTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public const string ErrorLogWhiteSpace = "[White Space]";
        public const string ExampleCaption = "\"Example \"\"Quotes\"\" Caption\"";
        public const string ExampleOutputFilePath = "C:\\Program Files\\Example Application\\example.png";
        public const string ExampleText = "\"Embedded \"\"Quotes\"\" White Space\"";
        public const string ExtensionBmp = ".bmp";
        public const string ExtensionFilterBmp = "Windows Bitmap BMP (*.bmp)|*.bmp";
        public const string ExtensionFilterGif = "CompuServe GIF 89a (*.gif)|*.gif";
        public const string ExtensionFilterJpg = "JPEG File Interchange Format (*.jpg)|*.jpg";
        public const string ExtensionFilterPng = "Portable Network Graphics PNG (*.png)|*.png";
        public const string ExtensionFilterTif = "Tagged Image File Format TIFF (*.tif)|*.tif";
        public const string ExtensionFilterWebP = "Google WebP (*.webp)|*.webp";
        public const string ExtensionGif = ".gif";
        public const string ExtensionJpg = ".jpg";
        public const string ExtensionPng = ".png";
        public const string ExtensionTif = ".tif";
        public const string ExtensionWebP = ".webp";
        public const string LibWebPX64FileName = "libwebp_x64.dll";
        public const string LibWebPX86FileName = "libwebp_x86.dll";
        public const string MonospaceFontName = "Courier New";
        public const string NumericUpDownEdit = "upDownEdit";
        public const string RemoteApiScriptName = "api.php";
        public const string RemoteApiSubdomain = "api";
        public const string RemoteApplicationConfig = "ApplicationConfig";
        public const string RemoteClientRemoteAddress = "ClientRemoteAddress";
        public const string RemoteProductLatestVersion = "ProductLatestVersion";
        public const string RemoteVariableNameGet = "get";
        public const string RemoteVariableNameSet = "set";
        public const string SchemeHttp = "http";
        public const string ThreeDots = "...";
        public const string VersionRegexPattern = "^\\d+\\.\\d+\\.\\d+\\.\\d$";
        public const string VersionUrlAppend = "version";
        public const string WordWrap = "WordWrap";
        public const string XmlElementVersion = "Version";
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MessageForm {
    public class ArgumentParser {
        private List<string> arguments;
        private string argumentString, caption, text, outputFilePath;
        private MessageForm.Buttons buttons;
        private MessageForm.BoxIcon boxIcon;
        private MessageForm.DefaultButton defaultButton;
        private int maximumWidth;
        private bool expectingCaption, expectingText, expectingOutputFilePath, expectingBoxIcon, expectingButtons, expectingDefaultButton, expectingMaximumWidth, captionSet, textSet, outputFilePathSet, boxIconSet, buttonsSet, defaultButtonSet, maximumWidthSet, basicThemeSet, helpSet, helpButtonSet, noWrapSet, hasArguments, thisTestSet;

        public ArgumentParser() {
            Reset();
        }

        private void Reset() {
            caption = string.Empty;
            text = string.Empty;
            outputFilePath = string.Empty;
            buttons = 0;
            boxIcon = 0;
            defaultButton = 0;
            expectingCaption = false;
            expectingText = false;
            expectingOutputFilePath = false;
            expectingBoxIcon = false;
            expectingButtons = false;
            expectingDefaultButton = false;
            expectingMaximumWidth = false;
            captionSet = false;
            textSet = false;
            outputFilePathSet = false;
            boxIconSet = false;
            buttonsSet = false;
            defaultButtonSet = false;
            maximumWidthSet = false;
            basicThemeSet = false;
            helpSet = false;
            helpButtonSet = false;
            noWrapSet = false;
            hasArguments = false;
            thisTestSet = false;
        }

        private void Evaluate() {
            foreach (string arg in arguments) {
                string argument = arg;
                hasArguments = true;
                if (argument == "-c" || argument == "/c") {
                    if (captionSet || expectingCaption) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageC);
                    }
                    if (expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingCaption = true;
                } else if (argument == "-t" || argument == "/t") {
                    if (textSet || expectingText) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    if (expectingCaption || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingText = true;
                } else if (argument == "-o" || argument == "/o") {
                    if (outputFilePathSet || expectingOutputFilePath) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageO);
                    }
                    if (expectingCaption || expectingText || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingOutputFilePath = true;
                } else if (argument == "-b" || argument == "/b") {
                    if (buttonsSet || expectingButtons) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageB);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingDefaultButton || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingButtons = true;
                } else if (argument == "-i" || argument == "/i") {
                    if (boxIconSet || expectingBoxIcon) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageI);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingButtons || expectingDefaultButton || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingBoxIcon = true;
                } else if (argument == "-d" || argument == "/d") {
                    if (defaultButtonSet || expectingDefaultButton) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageD);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingMaximumWidth || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingDefaultButton = true;
                } else if (argument == "-m" || argument == "/m") {
                    if (maximumWidthSet || expectingMaximumWidth) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageW);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || helpSet || thisTestSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingMaximumWidth = true;
                } else if (argument == "-e" || argument == "/e") {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    helpButtonSet = true;
                } else if (argument == "-w" || argument == "/w") {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    noWrapSet = true;
                } else if (argument == "-s" || argument == "/s") {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    basicThemeSet = true;
                } else if (argument == "-h" || argument == "/h" || argument == "-?" || argument == "/?") {
                    if (captionSet || textSet || outputFilePathSet || boxIconSet || buttonsSet || defaultButtonSet || maximumWidthSet || helpSet || thisTestSet || helpButtonSet || noWrapSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth || basicThemeSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    helpSet = true;
                } else if (argument == "-T" || argument == "/T") {
                    if (captionSet || textSet || outputFilePathSet || boxIconSet || buttonsSet || defaultButtonSet || maximumWidthSet || helpSet || thisTestSet || helpButtonSet || noWrapSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth || basicThemeSet) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    thisTestSet = true;
                } else if (expectingCaption) {
                    caption = argument;
                    expectingCaption = false;
                    captionSet = true;
                } else if (expectingText) {
                    text = argument;
                    expectingText = false;
                    textSet = true;
                } else if (expectingOutputFilePath) {
                    outputFilePath = argument;
                    expectingOutputFilePath = false;
                    outputFilePathSet = true;
                } else if (expectingButtons) {
                    buttons = (MessageForm.Buttons)int.Parse(argument);
                    expectingButtons = false;
                    buttonsSet = true;
                } else if (expectingBoxIcon) {
                    boxIcon = (MessageForm.BoxIcon)int.Parse(argument);
                    expectingBoxIcon = false;
                    boxIconSet = true;
                } else if (expectingDefaultButton) {
                    defaultButton = (MessageForm.DefaultButton)int.Parse(argument);
                    expectingDefaultButton = false;
                    defaultButtonSet = true;
                } else if (expectingMaximumWidth) {
                    maximumWidth = int.Parse(argument);
                    expectingMaximumWidth = false;
                    maximumWidthSet = true;
                } else if (argument.StartsWith("-") || argument.StartsWith("/")) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageU);
                } else {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                }
            }
            if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {
                throw new ApplicationException(Properties.Resources.ExceptionMessageM);
            }
            if (hasArguments && !textSet && !helpSet && !thisTestSet) {
                throw new ApplicationException(Properties.Resources.ExceptionMessageS);
            }
        }

        public static string EscapeArgument(string argument) {
            argument = Regex.Replace(argument, @"(\\*)" + "\"", @"$1$1\" + "\"");
            return "\"" + Regex.Replace(argument, @"(\\+)$", @"$1$1") + "\"";
        }

        public bool HasArguments {
            get {
                return hasArguments;
            }
        }

        public bool DisplayHelpButton {
            get {
                return helpButtonSet;
            }
        }

        public bool NoWrap {
            get {
                return noWrapSet;
            }
        }

        public bool BasicTheme {
            get {
                return basicThemeSet;
            }
        }

        public bool IsHelp {
            get {
                return helpSet;
            }
        }

        public bool IsThisTest {
            get {
                return thisTestSet;
            }
        }

        public string Caption {
            get {
                return caption;
            }
        }

        public string Text {
            get {
                return text;
            }
        }

        public string OutputFilePath {
            get {
                return outputFilePath;
            }
        }

        public MessageForm.Buttons Buttons {
            get {
                return buttons;
            }
        }

        public MessageForm.BoxIcon BoxIcon {
            get {
                return boxIcon;
            }
        }

        public MessageForm.DefaultButton DefaultButton {
            get {
                return defaultButton;
            }
        }

        public int MaximumWidth {
            get {
                return maximumWidth;
            }
        }

        public string[] Arguments {
            get {
                return arguments.ToArray();
            }
            set {
                Reset();
                arguments = new List<string>(value.Length);
                arguments.AddRange(value);
                try {
                    Evaluate();
                } catch (Exception exception) {
                    Reset();
                    throw exception;
                }
            }
        }

        public string ArgumentString {
            get {
                if (string.IsNullOrEmpty(argumentString) && arguments.Count > 0) {
                    return string.Join(Constants.Space, arguments);
                }
                return argumentString;
            }
            set {
                Reset();
                argumentString = value;
                arguments = Parse(argumentString);
                try {
                    Evaluate();
                } catch (Exception exception) {
                    Reset();
                    throw exception;
                }
            }
        }

        private static List<string> Parse(string str) {
            List<string> arguments = new List<string>();
            StringBuilder c = new StringBuilder();
            bool e = false, d = false, s = false;
            for (int i = 0; i < str.Length; i++) {
                if (!s) {
                    if (str[i] == ' ') {
                        continue;
                    }
                    d = str[i] == '"';
                    s = true;
                    e = false;
                    if (d) {
                        continue;
                    }
                }
                if (d) {
                    if (str[i] == '\\') {
                        if (i + 1 < str.Length && str[i + 1] == '"') {
                            c.Append(str[++i]);
                        } else {
                            c.Append(str[i]);
                        }
                    } else if (str[i] == '"') {
                        if (i + 1 < str.Length && str[i + 1] == '"') {
                            c.Append(str[++i]);
                        } else {
                            d = false;
                            e = true;
                        }
                    } else {
                        c.Append(str[i]);
                    }
                } else if (s) {
                    if (str[i] == ' ') {
                        s = false;
                        arguments.Add(e ? c.ToString() : c.ToString().TrimEnd(' '));
                        c = new StringBuilder();
                    } else if (!e) {
                        c.Append(str[i]);
                    }
                }
            }
            if (c.Length > 0) {
                arguments.Add(c.ToString());
            }
            return arguments;
        }
    }
}

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
using System.Collections.Generic;
using System.Text;

namespace MessageForm {
    public class ArgumentParser {
        private bool basicThemeSet;
        private bool boxIconSet;
        private bool buttonsSet;
        private bool captionSet;
        private bool defaultButtonSet;
        private bool expectingBoxIcon;
        private bool expectingButtons;
        private bool expectingCaption;
        private bool expectingDefaultButton;
        private bool expectingMaximumWidth;
        private bool expectingOutputFilePath;
        private bool expectingText;
        private bool hasArguments;
        private bool helpButtonSet;
        private bool helpSet;
        private bool maximumWidthSet;
        private bool noWrapSet;
        private bool outputFilePathSet;
        private bool textSet;
        private bool thisTestSet;
        private int maximumWidth;
        private List<string> arguments;
        private MessageForm.BoxIcon boxIcon;
        private MessageForm.Buttons buttons;
        private MessageForm.DefaultButton defaultButton;
        private string argumentString;
        private string caption;
        private string outputFilePath;
        private string text;

        public ArgumentParser() {
            Reset();
        }

        public bool BasicTheme => basicThemeSet;

        public bool DisplayHelpButton => helpButtonSet;

        public bool HasArguments => hasArguments;

        public bool IsHelp => helpSet;

        public bool IsThisTest => thisTestSet;

        public bool NoWrap => noWrapSet;

        public int MaximumWidth => maximumWidth;

        public MessageForm.BoxIcon BoxIcon => boxIcon;

        public MessageForm.Buttons Buttons => buttons;

        public MessageForm.DefaultButton DefaultButton => defaultButton;

        public string ArgumentString {
            get {
                if (string.IsNullOrEmpty(argumentString) && arguments.Count > 0) {
                    return string.Join(Constants.Space.ToString(), arguments);
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

        public string Caption => caption;

        public string OutputFilePath => outputFilePath;

        public string Text => text;

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

        private void Evaluate() {
            foreach (string arg in arguments) {
                string argument = arg;
                hasArguments = true;
                if (argument.Equals(Constants.CommandLineSwitchUC) || argument.Equals(Constants.CommandLineSwitchWC)) {
                    if (captionSet || expectingCaption) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageC);
                    }
                    if (expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingCaption = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUT) || argument.Equals(Constants.CommandLineSwitchWT)) {
                    if (textSet || expectingText) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    if (expectingCaption || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingText = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUO) || argument.Equals(Constants.CommandLineSwitchWO)) {
                    if (outputFilePathSet || expectingOutputFilePath) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageO);
                    }
                    if (expectingCaption || expectingText || expectingBoxIcon || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingOutputFilePath = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUB) || argument.Equals(Constants.CommandLineSwitchWB)) {
                    if (buttonsSet || expectingButtons) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageB);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingDefaultButton
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingButtons = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUI) || argument.Equals(Constants.CommandLineSwitchWI)) {
                    if (boxIconSet || expectingBoxIcon) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageI);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingBoxIcon = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUD) || argument.Equals(Constants.CommandLineSwitchWD)) {
                    if (defaultButtonSet || expectingDefaultButton) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageD);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons
                            || expectingMaximumWidth || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingDefaultButton = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUM) || argument.Equals(Constants.CommandLineSwitchWM)) {
                    if (maximumWidthSet || expectingMaximumWidth) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageW);
                    }
                    if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons
                            || expectingDefaultButton || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingMaximumWidth = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUE) || argument.Equals(Constants.CommandLineSwitchWE)) {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon
                            || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    helpButtonSet = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUW) || argument.Equals(Constants.CommandLineSwitchWW)) {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon
                            || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    noWrapSet = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUS) || argument.Equals(Constants.CommandLineSwitchWS)) {
                    if (helpSet || thisTestSet || expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon
                            || expectingButtons || expectingDefaultButton || expectingMaximumWidth) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    basicThemeSet = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUH) || argument.Equals(Constants.CommandLineSwitchWH)
                        || argument.Equals(Constants.CommandLineSwitchUQ) || argument.Equals(Constants.CommandLineSwitchWQ)) {

                    if (captionSet || textSet || outputFilePathSet || boxIconSet || buttonsSet || defaultButtonSet || maximumWidthSet
                            || helpSet || thisTestSet || helpButtonSet || noWrapSet || expectingCaption || expectingText
                            || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || basicThemeSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    helpSet = true;
                } else if (argument.Equals(Constants.CommandLineSwitchUU) || argument.Equals(Constants.CommandLineSwitchWU)) {
                    if (captionSet || textSet || outputFilePathSet || boxIconSet || buttonsSet || defaultButtonSet || maximumWidthSet
                            || helpSet || thisTestSet || helpButtonSet || noWrapSet || expectingCaption || expectingText
                            || expectingOutputFilePath || expectingBoxIcon || expectingButtons || expectingDefaultButton
                            || expectingMaximumWidth || basicThemeSet) {

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
                } else if (argument.StartsWith(Constants.Hyphen.ToString()) || argument.StartsWith(Constants.Slash.ToString())) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageU);
                } else {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                }
            }
            if (expectingCaption || expectingText || expectingOutputFilePath || expectingBoxIcon || expectingButtons
                    || expectingDefaultButton || expectingMaximumWidth) {

                throw new ApplicationException(Properties.Resources.ExceptionMessageM);
            }
            if (hasArguments && !textSet && !helpSet && !thisTestSet) {
                throw new ApplicationException(Properties.Resources.ExceptionMessageS);
            }
        }

        private void Reset() {
            basicThemeSet = false;
            boxIcon = 0;
            boxIconSet = false;
            buttons = 0;
            buttonsSet = false;
            caption = string.Empty;
            captionSet = false;
            defaultButton = 0;
            defaultButtonSet = false;
            expectingBoxIcon = false;
            expectingButtons = false;
            expectingCaption = false;
            expectingDefaultButton = false;
            expectingMaximumWidth = false;
            expectingOutputFilePath = false;
            expectingText = false;
            hasArguments = false;
            helpButtonSet = false;
            helpSet = false;
            maximumWidthSet = false;
            noWrapSet = false;
            outputFilePath = string.Empty;
            outputFilePathSet = false;
            text = string.Empty;
            textSet = false;
            thisTestSet = false;
        }

        private static List<string> Parse(string str) {
            char[] c = str.ToCharArray();
            List<string> arguments = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            bool e = false, d = false, s = false;
            for (int i = 0; i < c.Length; i++) {
                if (!s) {
                    if (c[i].Equals(Constants.Space)) {
                        continue;
                    }
                    d = c[i].Equals(Constants.QuotationMark);
                    s = true;
                    e = false;
                    if (d) {
                        continue;
                    }
                }
                if (d) {
                    if (c[i].Equals(Constants.BackSlash)) {
                        if (i + 1 < c.Length && c[i + 1].Equals(Constants.QuotationMark)) {
                            stringBuilder.Append(c[++i]);
                        } else {
                            stringBuilder.Append(c[i]);
                        }
                    } else if (c[i].Equals(Constants.QuotationMark)) {
                        if (i + 1 < c.Length && c[i + 1].Equals(Constants.QuotationMark)) {
                            stringBuilder.Append(c[++i]);
                        } else {
                            d = false;
                            e = true;
                        }
                    } else {
                        stringBuilder.Append(c[i]);
                    }
                } else if (s) {
                    if (c[i].Equals(Constants.Space)) {
                        s = false;
                        arguments.Add(e ? stringBuilder.ToString() : stringBuilder.ToString().TrimEnd(Constants.Space));
                        stringBuilder = new StringBuilder();
                    } else if (!e) {
                        stringBuilder.Append(c[i]);
                    }
                }
            }
            if (stringBuilder.Length > 0) {
                arguments.Add(stringBuilder.ToString());
            }
            return arguments;
        }
    }
}

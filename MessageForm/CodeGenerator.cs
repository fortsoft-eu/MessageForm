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
using System.Text;
using System.Windows.Forms;

namespace MessageForm {
    public class CodeGenerator {
        private Form dialog;

        public event EventHandler<CodeGeneratorEventArgs> DialogCreated;
        public event HelpEventHandler HelpRequested;

        public bool CenterScreen { get; set; }

        public bool NoWrap { get; set; }

        public bool SetParent { get; set; }

        public bool ShowHelpButton { get; set; }

        public bool ShowMessageBox { get; set; }

        public Form Parent { get; set; }

        public int MaximumWidth { get; set; }

        public MessageBoxButtons BoxButtons { get; set; }

        public MessageBoxDefaultButton BoxDefaultButton { get; set; }

        public MessageBoxIcon BoxIcon { get; set; }

        public MessageForm.BoxIcon FormBoxIcon { get; set; }

        public MessageForm.Buttons FormButtons { get; set; }

        public MessageForm.DefaultButton FormDefaultButton { get; set; }

        public string Caption { get; set; }

        public string Text { get; set; }

        private string GenerateCode() {
            StringBuilder stringBuilder = new StringBuilder();
            if (ShowMessageBox) {
                stringBuilder.Append(Constants.CodeGeneratorMsgBoxInst)
                    .Append(Constants.OpeningParenthesis);
                if (SetParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis)
                        .Append(Constants.Comma)
                        .Append(Constants.Space);
                }
                if (string.IsNullOrEmpty(Text)) {
                    stringBuilder.Append(Constants.CodeGeneratorStringEmpty);
                } else {
                    stringBuilder.Append(StaticMethods.EscapeArgument(Text));
                }
                if (!string.IsNullOrEmpty(Caption) || BoxButtons > 0 || BoxIcon > 0 || BoxDefaultButton > 0) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space);
                    if (string.IsNullOrEmpty(Caption)) {
                        stringBuilder.Append(Constants.CodeGeneratorNull);
                    } else {
                        stringBuilder.Append(StaticMethods.EscapeArgument(Caption));
                    }
                }
                if (BoxButtons > 0 || BoxIcon > 0 || FormDefaultButton > 0) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageBoxButtons).Name,
                            BoxButtons.ToString()
                        }));
                }
                if (BoxIcon > 0 || FormDefaultButton > 0) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageBoxIcon).Name,
                            BoxIcon.ToString()
                        }));
                }
                if (BoxDefaultButton > 0) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageBoxDefaultButton).Name,
                            BoxDefaultButton.ToString()
                        }));
                }
                stringBuilder.Append(Constants.ClosingParenthesis)
                    .Append(Constants.Semicolon);
            } else {
                stringBuilder.Append(Constants.CodeGeneratorMsgFormInst)
                    .Append(Constants.OpeningParenthesis);
                if (SetParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis)
                        .Append(Constants.Comma)
                        .Append(Constants.Space);
                }
                if (string.IsNullOrEmpty(Text)) {
                    stringBuilder.Append(Constants.CodeGeneratorStringEmpty);
                } else {
                    stringBuilder.Append(StaticMethods.EscapeArgument(Text));
                }
                if (!string.IsNullOrEmpty(Caption) || FormButtons > 0 || FormBoxIcon > 0 || FormDefaultButton > 0 || CenterScreen
                        || ShowHelpButton || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {

                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space);
                    if (string.IsNullOrEmpty(Caption)) {
                        stringBuilder.Append(Constants.CodeGeneratorNull);
                    } else {
                        stringBuilder.Append(StaticMethods.EscapeArgument(Caption));
                    }
                }
                if (FormButtons > 0 || FormBoxIcon > 0 || FormDefaultButton > 0 || CenterScreen || ShowHelpButton
                        || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {

                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageForm.Buttons).Namespace,
                            typeof(MessageForm.Buttons).Name,
                            FormButtons.ToString()
                        }));
                }
                if (FormBoxIcon > 0 || FormDefaultButton > 0 || CenterScreen || ShowHelpButton
                        || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {

                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageForm.BoxIcon).Namespace,
                            typeof(MessageForm.BoxIcon).Name,
                            FormBoxIcon.ToString()
                        }));
                }
                if (FormDefaultButton > 0 || CenterScreen || ShowHelpButton
                        || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {

                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(string.Join(Constants.Period.ToString(), new string[] {
                            typeof(MessageForm.DefaultButton).Namespace,
                            typeof(MessageForm.DefaultButton).Name,
                            FormDefaultButton.ToString()
                        }));
                }
                if (CenterScreen || ShowHelpButton || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(CenterScreen.ToString().ToLowerInvariant());
                }
                if (ShowHelpButton || MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(ShowHelpButton.ToString().ToLowerInvariant());
                }
                if (MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth) || NoWrap) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space);
                    if (MaximumWidth > 0 && !MaximumWidth.Equals(MessageForm.defaultWidth)) {
                        stringBuilder.Append(MaximumWidth);
                    } else {
                        stringBuilder.Append(0);
                    }
                }
                if (NoWrap) {
                    stringBuilder.Append(Constants.Comma)
                        .Append(Constants.Space)
                        .Append(NoWrap.ToString().ToLowerInvariant());
                }
                stringBuilder.Append(Constants.ClosingParenthesis)
                    .Append(Constants.Semicolon)
                    .AppendLine()
                    .Append(Constants.CodeGeneratorMsgFormShowDlg)
                    .Append(Constants.OpeningParenthesis);
                if (SetParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis);
                }
                stringBuilder.Append(Constants.ClosingParenthesis)
                    .Append(Constants.Semicolon);
            }
            return stringBuilder.ToString();
        }

        private void OnHelpRequested(object sender, HelpEventArgs hlpevent) => HelpRequested?.Invoke(sender, hlpevent);

        public void ShowCode() {
            if (dialog == null || !dialog.Visible) {
                CodeForm codeForm = new CodeForm(GenerateCode());
                codeForm.HelpRequested += new HelpEventHandler(OnHelpRequested);
                dialog = codeForm;
                DialogCreated?.Invoke(this, new CodeGeneratorEventArgs(codeForm));
                codeForm.ShowDialog(Parent);
            }
        }
    }
}

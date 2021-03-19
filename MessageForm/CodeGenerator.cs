using System.Text;
using System.Windows.Forms;

namespace MessageForm {
    public class CodeGenerator {
        private string caption, text;
        private bool setParent, centerScreen, showHelpButton, noWrap, showMessageBox;
        private int maximumWidth;
        private MessageForm.Buttons formButtons;
        private MessageForm.BoxIcon formBoxIcon;
        private MessageForm.DefaultButton formDefaultButton;
        private MessageBoxButtons boxButtons;
        private MessageBoxIcon boxIcon;
        private MessageBoxDefaultButton boxDefaultButton;
        private Form parent, dialog;

        public delegate void CodeGeneratorEventHandler(object sender, Form dialog);

        public event CodeGeneratorEventHandler DialogCreated;
        public event HelpEventHandler HelpRequested;

        public Form Parent {
            get {
                return parent;
            }
            set {
                parent = value;
            }
        }

        public bool SetParent {
            get {
                return setParent;
            }
            set {
                setParent = value;
            }
        }

        public string Caption {
            get {
                return caption;
            }
            set {
                caption = value;
            }
        }

        public string Text {
            get {
                return text;
            }
            set {
                text = value;
            }
        }

        public MessageForm.Buttons FormButtons {
            get {
                return formButtons;
            }
            set {
                formButtons = value;
            }
        }

        public MessageForm.BoxIcon FormBoxIcon {
            get {
                return formBoxIcon;
            }
            set {
                formBoxIcon = value;
            }
        }

        public MessageForm.DefaultButton FormDefaultButton {
            get {
                return formDefaultButton;
            }
            set {
                formDefaultButton = value;
            }
        }

        public MessageBoxButtons BoxButtons {
            get {
                return boxButtons;
            }
            set {
                boxButtons = value;
            }
        }

        public MessageBoxIcon BoxIcon {
            get {
                return boxIcon;
            }
            set {
                boxIcon = value;
            }
        }

        public MessageBoxDefaultButton BoxDefaultButton {
            get {
                return boxDefaultButton;
            }
            set {
                boxDefaultButton = value;
            }
        }

        public bool CenterScreen {
            get {
                return centerScreen;
            }
            set {
                centerScreen = value;
            }
        }

        public bool ShowHelpButton {
            get {
                return showHelpButton;
            }
            set {
                showHelpButton = value;
            }
        }

        public int MaximumWidth {
            get {
                return maximumWidth;
            }
            set {
                maximumWidth = value;
            }
        }

        public bool NoWrap {
            get {
                return noWrap;
            }
            set {
                noWrap = value;
            }
        }

        public bool ShowMessageBox {
            get {
                return showMessageBox;
            }
            set {
                showMessageBox = value;
            }
        }

        public void ShowCode() {
            if (dialog == null || !dialog.Visible) {
                CodeForm showCodeForm = new CodeForm(GenerateCode());
                showCodeForm.HelpRequested += new HelpEventHandler(OnHelpRequested);
                dialog = showCodeForm;
                DialogCreated?.Invoke(this, showCodeForm);
                showCodeForm.ShowDialog(parent);
            }
        }

        private string GenerateCode() {
            StringBuilder stringBuilder = new StringBuilder();
            if (showMessageBox) {
                stringBuilder.Append(Constants.CodeGeneratorMsgBoxInst);
                if (setParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis);
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                }
                stringBuilder.Append(string.IsNullOrEmpty(text) ? Constants.CodeGeneratorNull : ArgumentParser.EscapeArgument(text));
                if (!string.IsNullOrEmpty(caption) || boxButtons > 0 || boxIcon > 0 || boxDefaultButton > 0) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.IsNullOrEmpty(caption) ? Constants.CodeGeneratorNull : ArgumentParser.EscapeArgument(caption));
                }
                if (boxButtons > 0 || boxIcon > 0 || formDefaultButton > 0) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageBoxButtons).Name, boxButtons.ToString() }));
                }
                if (boxIcon > 0 || formDefaultButton > 0) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageBoxIcon).Name, boxIcon.ToString() }));
                }
                if (boxDefaultButton > 0) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageBoxDefaultButton).Name, boxDefaultButton.ToString() }));
                }
                stringBuilder.Append(Constants.CodeGeneratorEndBracketAndSemicolon);
            } else {
                stringBuilder.Append(Constants.CodeGeneratorMsgFormInst);
                if (setParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis);
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                }
                stringBuilder.Append(string.IsNullOrEmpty(text) ? Constants.CodeGeneratorNull : ArgumentParser.EscapeArgument(text));
                if (!string.IsNullOrEmpty(caption) || formButtons > 0 || formBoxIcon > 0 || formDefaultButton > 0 || centerScreen || showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.IsNullOrEmpty(caption) ? Constants.CodeGeneratorNull : ArgumentParser.EscapeArgument(caption));
                }
                if (formButtons > 0 || formBoxIcon > 0 || formDefaultButton > 0 || centerScreen || showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageForm.Buttons).Namespace, typeof(MessageForm.Buttons).Name, formButtons.ToString() }));
                }
                if (formBoxIcon > 0 || formDefaultButton > 0 || centerScreen || showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageForm.BoxIcon).Namespace, typeof(MessageForm.BoxIcon).Name, formBoxIcon.ToString() }));
                }
                if (formDefaultButton > 0 || centerScreen || showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(string.Join(".", new string[] { typeof(MessageForm.DefaultButton).Namespace, typeof(MessageForm.DefaultButton).Name, formDefaultButton.ToString() }));
                }
                if (centerScreen || showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(centerScreen.ToString().ToLowerInvariant());
                }
                if (showHelpButton || maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(showHelpButton.ToString().ToLowerInvariant());
                }
                if (maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth || noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(maximumWidth > 0 && maximumWidth != MessageForm.defaultWidth ? maximumWidth.ToString() : 0.ToString());
                }
                if (noWrap) {
                    stringBuilder.Append(Constants.CodeGeneratorCommaAndSpace);
                    stringBuilder.Append(noWrap.ToString().ToLowerInvariant());
                }
                stringBuilder.AppendLine(Constants.CodeGeneratorEndBracketAndSemicolon);
                stringBuilder.Append(Constants.CodeGeneratorMsgFormShowDlg);
                if (setParent) {
                    stringBuilder.Append(Constants.CodeGeneratorThis);
                }
                stringBuilder.Append(Constants.CodeGeneratorEndBracketAndSemicolon);
            }
            return stringBuilder.ToString();
        }

        private void OnHelpRequested(object sender, HelpEventArgs hlpevent) {
            HelpRequested?.Invoke(sender, hlpevent);
        }
    }
}

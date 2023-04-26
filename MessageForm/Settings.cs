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

using FortSoft.Tools;
using System;
using System.Collections;
using System.Text;

namespace MessageForm {

    /// <summary>
    /// This is an implementation of using the PersistentSettings class for
    /// Message Form.
    /// </summary>
    public sealed class Settings : IDisposable {

        /// <summary>
        /// Field.
        /// </summary>
        private PersistentSettings persistentSettings;

        /// <summary>
        /// Occurs on successful saving all application settings into the Windows
        /// registry.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings() {
            persistentSettings = new PersistentSettings();
            Load();
        }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool BoxCenterScreen { get; set; }

        /// <summary>
        /// Represents the setting if the application should check for updates.
        /// The default value is true.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// Represents whether visual styles will be used when rendering
        /// application windows. The default value is false.
        /// </summary>
        public bool DisableThemes { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool DisplayHelpButton { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool MainFormCenterScreen { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool PrintSoftMargins { get; set; } = true;

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool SetMaximumWidth { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool SetNoWrap { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool SetParentForm { get; set; } = true;

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool ShowMessageBox { get; set; }

        /// <summary>
        /// Represents the setting if the application should inform the user
        /// about available updates in the status bar only. If not, a pop-up
        /// window will appear. The default value is false.
        /// </summary>
        public bool StatusBarNotifOnly { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int ButtonsIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int DefaultButtonIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int EscapeFunction { get; set; } = 1;

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int ExtensionFilterIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int IconIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int MaximumWidth { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string Caption { get; set; } = Properties.Resources.DummyCaption;

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string LastExportDirectory { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string Text { get; set; } = Properties.Resources.DummyText;

        /// <summary>
        /// Loads the software application settings from the Windows registry.
        /// </summary>
        private void Load() {
            IntToBitSettings(persistentSettings.Load("BitSettings", BitSettingsToInt()));
            IntToByteSettings(persistentSettings.Load("ByteSettings", ByteSettingsToInt()));
            IntToWordSettings(persistentSettings.Load("WordSettings", WordSettingsToInt()));

            Caption = persistentSettings.Load("Caption", Caption);
            LastExportDirectory = persistentSettings.Load("LastExportDir", LastExportDirectory);
            Text = persistentSettings.Load("Text", Text);
        }

        /// <summary>
        /// Saves the software application settings into the Windows registry.
        /// </summary>
        public void Save() {
            persistentSettings.Save("BitSettings", BitSettingsToInt());
            persistentSettings.Save("ByteSettings", ByteSettingsToInt());
            persistentSettings.Save("WordSettings", WordSettingsToInt());

            persistentSettings.Save("Caption", Caption);
            persistentSettings.Save("LastExportDir", LastExportDirectory);
            persistentSettings.Save("Text", Text);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Expands an integer value into some boolean settings.
        /// </summary>
        private void IntToBitSettings(int i) {
            BitArray bitArray = new BitArray(new int[] { i });
            bool[] bitSettings = new bool[bitArray.Count];
            bitArray.CopyTo(bitSettings, 0);
            i = bitSettings.Length - 21;

            ShowMessageBox = bitSettings[--i];
            SetParentForm = bitSettings[--i];
            SetNoWrap = bitSettings[--i];
            SetMaximumWidth = bitSettings[--i];
            DisplayHelpButton = bitSettings[--i];
            BoxCenterScreen = bitSettings[--i];
            MainFormCenterScreen = bitSettings[--i];
            DisableThemes = bitSettings[--i];
            PrintSoftMargins = bitSettings[--i];
            StatusBarNotifOnly = bitSettings[--i];
            CheckForUpdates = bitSettings[--i];
        }

        /// <summary>
        /// Compacts some boolean settings into an integer value.
        /// </summary>
        private int BitSettingsToInt() {
            StringBuilder stringBuilder = new StringBuilder(string.Empty.PadRight(21, Constants.Zero))
                .Append(ShowMessageBox ? 1 : 0)
                .Append(SetParentForm ? 1 : 0)
                .Append(SetNoWrap ? 1 : 0)
                .Append(SetMaximumWidth ? 1 : 0)
                .Append(DisplayHelpButton ? 1 : 0)
                .Append(BoxCenterScreen ? 1 : 0)
                .Append(MainFormCenterScreen ? 1 : 0)
                .Append(DisableThemes ? 1 : 0)
                .Append(PrintSoftMargins ? 1 : 0)
                .Append(StatusBarNotifOnly ? 1 : 0)
                .Append(CheckForUpdates ? 1 : 0);
            return Convert.ToInt32(stringBuilder.ToString(), 2);
        }

        /// <summary>
        /// Expands an integer value into byte values.
        /// </summary>
        private void IntToByteSettings(int i) {
            byte[] bytes = IntToByteArray(i);
            IconIndex = bytes[0];
            ButtonsIndex = bytes[1];
            DefaultButtonIndex = bytes[2];
            EscapeFunction = bytes[3];
        }

        /// <summary>
        /// Compacts some byte values into an integer value.
        /// </summary>
        private int ByteSettingsToInt() {
            byte[] bytes = new byte[] {
                (byte)IconIndex,
                (byte)ButtonsIndex,
                (byte)DefaultButtonIndex,
                (byte)EscapeFunction
            };
            return ByteArrayToInt(bytes);
        }

        /// <summary>
        /// Expands an integer value into Border3DStyle and InspectOverlayOpacity.
        /// </summary>
        private void IntToWordSettings(int i) {
            ushort[] values = IntToUShortArray(i);
            ExtensionFilterIndex = values[0];
            MaximumWidth = values[1];
        }

        /// <summary>
        /// Compacts Border3DStyle and InspectOverlayOpacity into an integer value.
        /// </summary>
        private int WordSettingsToInt() {
            ushort[] values = new ushort[] {
                (ushort)ExtensionFilterIndex,
                (ushort)MaximumWidth
            };
            return UShortArrayToInt(values);
        }

        /// <summary>
        /// This setting will not be directly stored in the Windows registry.
        /// </summary>
        public bool RenderWithVisualStyles { get; set; }

        /// <summary>
        /// Clears the software application values from the Windows registry.
        /// </summary>
        public void Clear() => persistentSettings.Clear();

        /// <summary>Clean up any resources being used.</summary>
        public void Dispose() => persistentSettings.Dispose();

        /// <summary>
        /// Hardware-independent static method for conversion of byte array into
        /// an integer value.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>An integer value to store in the Windows registry.</returns>
        public static int ByteArrayToInt(byte[] bytes) {
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Hardware-independent static method for conversion of an integer value
        /// into a byte array.
        /// </summary>
        /// <param name="val">An integer value stored in the registry.</param>
        public static byte[] IntToByteArray(int val) {
            byte[] bytes = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Hardware-independent static method for conversion of two ushort
        /// values into an integer value.
        /// </summary>
        /// <param name="values">An array of ushort values.</param>
        /// <returns>An integer value to store in the Windows registry.</returns>
        public static int UShortArrayToInt(ushort[] values) {
            byte[] bytes = new byte[4];
            Array.Copy(
                BitConverter.GetBytes(values[0]),
                0,
                bytes,
                BitConverter.IsLittleEndian ? 0 : 2,
                2);
            Array.Copy(
                BitConverter.GetBytes(values[1]),
                0,
                bytes,
                BitConverter.IsLittleEndian ? 2 : 0,
                2);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Hardware-independent static method for conversion of an integer value
        /// into two ushort values.
        /// </summary>
        /// <param name="val">An integer value stored in the registry.</param>
        public static ushort[] IntToUShortArray(int val) {
            byte[] bytes = BitConverter.GetBytes(val);
            return new ushort[] {
                BitConverter.ToUInt16(bytes, BitConverter.IsLittleEndian ? 0 : 2),
                BitConverter.ToUInt16(bytes, BitConverter.IsLittleEndian ? 2 : 0)
            };
        }
    }
}

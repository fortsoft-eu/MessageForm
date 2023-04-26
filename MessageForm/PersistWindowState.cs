/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2009-2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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
 * Version 2.3.2.2
 */

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace FortSoft.Tools {

    /// <summary>
    /// Implements saving of the state of the window (Windows Form) in the
    /// Windows registry and its resetting according to previously saved values.
    /// Uses Windows registry and Windows API.
    /// </summary>
    public sealed class PersistWindowState : Component {

        /// <summary>
        /// Windows API constants.
        /// </summary>
        private const int GW_HWNDNEXT = 2;
        private const int SW_RESTORE = 9;

        /// <summary>
        /// Constants.
        /// </summary>
        private const string Location = "Location";
        private const string Size = "Size";
        private const string Software = "Software";
        private const string State = "State";

        /// <summary>
        /// Imports.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        /// <summary>
        /// Fields.
        /// </summary>
        private bool topMost;
        private Form parent;
        private FormWindowState windowState;
        private IntPtr parentHandle;
        private Point nLocation;
        private Size nSize;
        private string initialTitle;

        /// <summary>
        /// Occurs on error accessing the Windows registry.
        /// </summary>
        public event EventHandler<PersistWindowStateEventArgs> Error;

        /// <summary>
        /// Occurs on successful reset the parent form to its previous state
        /// according to the values stored in the Windows registry or this class.
        /// </summary>
        public event EventHandler<PersistWindowStateEventArgs> Loaded;

        /// <summary>
        /// Occurs on successful saving the values into the Windows registry.
        /// </summary>
        public event EventHandler<PersistWindowStateEventArgs> Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistWindowState"/>
        /// class.
        /// </summary>
        public PersistWindowState() {
            SavingOptions = PersistWindowStateSavingOptions.Registry;
            RegistryPath = Path.Combine(Software, Application.CompanyName, Application.ProductName);
        }

        /// <summary>
        /// Allows saving minimized parent form.
        /// </summary>
        public bool AllowSaveMinimized { get; set; }

        /// <summary>
        /// Allows saving TopMost property of the parent form.
        /// </summary>
        public bool AllowSaveTopMost { get; set; }

        /// <summary>
        /// Other opened windows detection options.
        /// </summary>
        public WindowDetectionOptions DetectionOptions { get; set; }

        /// <summary>
        /// Disables saving height od the parent form.
        /// </summary>
        public bool DisableSaveHeight { get; set; }

        /// <summary>
        /// Disables saving position od the parent form.
        /// </summary>
        public bool DisableSavePosition { get; set; }

        /// <summary>
        /// Disables saving size od the parent form.
        /// </summary>
        public bool DisableSaveSize { get; set; }

        /// <summary>
        /// Disables saving width od the parent form.
        /// </summary>
        public bool DisableSaveWidth { get; set; }

        /// <summary>
        /// Disables saving WindowState property od the parent form.
        /// </summary>
        public bool DisableSaveWindowState { get; set; }

        /// <summary>
        /// Ensures a fixed height of the parent form.
        /// </summary>
        public bool FixHeight { get; set; }

        /// <summary>
        /// Ensures a fixed width of the parent form.
        /// </summary>
        public bool FixWidth { get; set; }

        /// <summary>
        /// Gets / sets parent form whose state will be saved.
        /// </summary>
        public Form Parent {
            get {
                return parent;
            }
            set {
                parent = value;
                parent.Load += new EventHandler(OnLoad);
                parent.FormClosed += new FormClosedEventHandler((sender, e) => Save());
                initialTitle = parent.Text;
            }
        }

        /// <summary>
        /// Ensures the placement of the parent form on the screen after load.
        /// </summary>
        public bool PlaceOnScreenAfterLoad { get; set; } = true;

        /// <summary>
        /// Gets / sets Windows registry subtree path. Default value is set after
        /// instantiation.
        /// </summary>
        public string RegistryPath { get; set; }

        /// <summary>
        /// Gets / sets saving options. (That means if the state of the parent
        /// form will be saved only within the scope of the running instance of
        /// the software application or if it will be saved permanently in the
        /// Windows registry.)
        /// </summary>
        public PersistWindowStateSavingOptions SavingOptions { get; set; }

        /// <summary>
        /// Checks if another same window of the application is already running
        /// to prevent multiple windows from having the same initial position.
        /// </summary>
        /// <returns>
        /// True if another same window is already running; otherwise false.
        /// </returns>
        private bool AlreadyRunning() {
            if (DetectionOptions.Equals(WindowDetectionOptions.NoDetection)) {
                return false;
            }
            Process process = Process.GetCurrentProcess();
            FileSystemInfo processFileInfo = new FileInfo(process.MainModule.FileName);
            foreach (Process p in Process.GetProcessesByName(process.ProcessName)
                    .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                    .ToArray()) {

                if (!p.Id.Equals(process.Id)
                        && !p.MainWindowHandle.Equals(IntPtr.Zero)
                        && processFileInfo.Name.Equals(new FileInfo(p.MainModule.FileName).Name)) {

                    switch (DetectionOptions) {
                        case WindowDetectionOptions.TitleContains:
                            if (p.MainWindowTitle.Contains(initialTitle)) {
                                return true;
                            }
                            break;
                        case WindowDetectionOptions.TitleStartsWith:
                            if (p.MainWindowTitle.StartsWith(initialTitle)) {
                                return true;
                            }
                            break;
                        case WindowDetectionOptions.TitleEndsWith:
                            if (p.MainWindowTitle.EndsWith(initialTitle)) {
                                return true;
                            }
                            break;
                        case WindowDetectionOptions.TitleEquals:
                            if (p.MainWindowTitle.Equals(initialTitle)) {
                                return true;
                            }
                            break;
                        default:
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Thread-safe brings the parent form to the front of the z-order. This
        /// method is the replacement of unreliable Control.BringToFront().
        /// </summary>
        public void BringToFront() {
            if (!parentHandle.Equals(IntPtr.Zero)) {
                SetForegroundWindow(parentHandle);
            }
        }

        /// <summary>
        /// Checks if the parent form immediately follows the reference form in z-order.
        /// </summary>
        /// <param name="hWnd">Reference form window handle.</param>
        private bool IsNextWindow(IntPtr hWnd) {
            IntPtr window = GetTopWindow(GetDesktopWindow());
            int thisWindow = -1, i = 0;

            do {
                if (thisWindow > -1 && thisWindow + 1 < i) {
                    break;
                }
                if (!IsWindowVisible(window)) {
                    continue;
                }
                if (thisWindow > 0 && window.Equals(parentHandle)) {
                    return true;
                }
                if (window.Equals(hWnd)) {
                    thisWindow = i;
                }
                i++;
            } while (!(window = GetWindow(window, GW_HWNDNEXT)).Equals(IntPtr.Zero));

            return false;
        }

        /// <summary>
        /// Handles the Load event of the parent form.
        /// </summary>
        private void OnLoad(object sender, EventArgs e) {
            parentHandle = parent.Handle;

            if (SavingOptions.Equals(PersistWindowStateSavingOptions.None) || string.IsNullOrEmpty(RegistryPath)) {
                if (!nLocation.IsEmpty || !nSize.IsEmpty) {
                    if (AllowSaveTopMost) {
                        parent.TopMost = topMost;
                    }
                    if (!DisableSaveSize && (parent.FormBorderStyle.Equals(FormBorderStyle.Sizable)
                            || parent.FormBorderStyle.Equals(FormBorderStyle.SizableToolWindow))) {

                        if (!FixWidth && !DisableSaveWidth && nSize.Width > 0) {
                            parent.Width = nSize.Width;
                        }
                        if (!FixHeight && !DisableSaveHeight && nSize.Height > 0) {
                            parent.Height = nSize.Height;
                        }
                    }
                    if (!AlreadyRunning()) {
                        if (PlaceOnScreenAfterLoad && !DisableSavePosition) {
                            parent.Location = AdjustLocation(nLocation, parent.Size);
                        } else if (!DisableSavePosition) {
                            parent.Location = nLocation;
                        }
                    }
                    if (parent.WindowState.Equals(FormWindowState.Normal)) {
                        nSize = parent.Size;
                    }
                    if (!DisableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)) {
                        parent.WindowState = AllowSaveMinimized || !parent.WindowState.Equals(FormWindowState.Minimized)
                            ? windowState
                            : FormWindowState.Normal;
                    }
                    Loaded?.Invoke(this, new PersistWindowStateEventArgs());
                } else {
                    if (AllowSaveTopMost) {
                        topMost = parent.TopMost;
                    }
                    if (parent.WindowState.Equals(FormWindowState.Normal)) {
                        nLocation = parent.Location;
                    }
                    if (!AlreadyRunning()) {
                        if (PlaceOnScreenAfterLoad && !DisableSavePosition) {
                            parent.Location = AdjustLocation(nLocation, parent.Size);
                        } else if (!DisableSavePosition) {
                            parent.Location = nLocation;
                        }
                    }
                }
            }

            if (SavingOptions.Equals(PersistWindowStateSavingOptions.Registry) && !string.IsNullOrEmpty(RegistryPath)) {
                RegistryKey registryKey = null;
                try {
                    registryKey = Registry.CurrentUser.OpenSubKey(RegistryPath);
                } catch (IOException exception) {
                    Debug.WriteLine(exception);
                    Error?.Invoke(this, new PersistWindowStateEventArgs(registryKey, exception));
                } catch (SecurityException exception) {
                    Debug.WriteLine(exception);
                    Error?.Invoke(this, new PersistWindowStateEventArgs(registryKey, exception));
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                }
                if (registryKey == null) {
                    if (parent.WindowState.Equals(FormWindowState.Normal)) {
                        nLocation = parent.Location;
                    }
                    if (!AlreadyRunning()) {
                        if (PlaceOnScreenAfterLoad && !DisableSavePosition) {
                            parent.Location = AdjustLocation(nLocation, parent.Size);
                        } else if (!DisableSavePosition) {
                            parent.Location = nLocation;
                        }
                    }
                } else {
                    nLocation = IntToPoint((int)registryKey.GetValue(parent.Name + Location, PointToInt(parent.Location)));
                    if (!DisableSaveSize && (parent.FormBorderStyle.Equals(FormBorderStyle.Sizable)
                            || parent.FormBorderStyle.Equals(FormBorderStyle.SizableToolWindow))) {

                        Size size = IntToSize((int)registryKey.GetValue(parent.Name + Size, SizeToInt(parent.Size)));
                        if (!FixWidth && !DisableSaveWidth && size.Width > 0) {
                            parent.Width = size.Width;
                        }
                        if (!FixHeight && !DisableSaveHeight && size.Height > 0) {
                            parent.Height = size.Height;
                        }
                    }
                    if (!AlreadyRunning()) {
                        if (PlaceOnScreenAfterLoad && !DisableSavePosition) {
                            parent.Location = AdjustLocation(nLocation, parent.Size);
                        } else if (!DisableSavePosition) {
                            parent.Location = nLocation;
                        }
                    }
                    if (parent.WindowState.Equals(FormWindowState.Normal)) {
                        nSize = parent.Size;
                    }
                    if (!DisableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)
                            || AllowSaveTopMost) {

                        windowState = IntToWindowState((int)registryKey.GetValue(parent.Name
                            + State, WindowStateToInt(parent.WindowState, parent.TopMost)), out topMost);
                        if (!AllowSaveMinimized && windowState.Equals(FormWindowState.Minimized)) {
                            windowState = FormWindowState.Normal;
                        }
                        parent.WindowState = windowState;
                        if (AllowSaveTopMost) {
                            parent.TopMost = topMost;
                        }
                    }
                    Loaded?.Invoke(this, new PersistWindowStateEventArgs(registryKey));
                }
            }

            if (FixWidth && FixHeight) {
                parent.MaximumSize = parent.Size;
                parent.MinimumSize = parent.Size;
            } else if (FixWidth) {
                parent.MaximumSize = new Size(
                    parent.Width,
                    parent.MaximumSize.Height > 0 ? parent.MaximumSize.Height : int.MaxValue);
                parent.MinimumSize = new Size(
                    parent.Width,
                    parent.MinimumSize.Height);
            } else if (FixHeight) {
                parent.MaximumSize = new Size(
                    parent.MaximumSize.Width > 0 ? parent.MaximumSize.Width : int.MaxValue,
                    parent.Height);
                parent.MinimumSize = new Size(
                    parent.MinimumSize.Width,
                    parent.Height);
            }

            parent.Resize += new EventHandler(OnResize);
            parent.Move += new EventHandler(OnMove);
        }

        /// <summary>
        /// Handles the Move event of the parent form.
        /// </summary>
        private void OnMove(object sender, EventArgs e) {
            if (parent.WindowState.Equals(FormWindowState.Normal)) {
                nLocation = parent.Location;
            }
            if (AllowSaveMinimized || !parent.WindowState.Equals(FormWindowState.Minimized)) {
                windowState = parent.WindowState;
            }
        }

        /// <summary>
        /// Handles the Resize event of the parent form.
        /// </summary>
        private void OnResize(object sender, EventArgs e) {
            if (parent.WindowState.Equals(FormWindowState.Normal)) {
                nSize = parent.Size;
            }
            if (AllowSaveMinimized || !parent.WindowState.Equals(FormWindowState.Minimized)) {
                windowState = parent.WindowState;
            }
        }

        /// <summary>
        /// Thread-safe restores the parent form to its the previous WindowState.
        /// </summary>
        public void Restore() {
            if (!parentHandle.Equals(IntPtr.Zero) && !IsIconic(parentHandle).Equals(0)) {
                ShowWindowAsync(parentHandle, SW_RESTORE);
            }
        }

        /// <summary>
        /// Saves the state of the parent form into the Windows registry.
        /// </summary>
        public void Save() {
            if (SavingOptions.Equals(PersistWindowStateSavingOptions.Registry) && !string.IsNullOrEmpty(RegistryPath)) {
                RegistryKey registryKey = null;
                try {
                    registryKey = Registry.CurrentUser.CreateSubKey(RegistryPath);
                } catch (IOException exception) {
                    Debug.WriteLine(exception);
                    Error?.Invoke(this, new PersistWindowStateEventArgs(registryKey, exception));
                } catch (SecurityException exception) {
                    Debug.WriteLine(exception);
                    Error?.Invoke(this, new PersistWindowStateEventArgs(registryKey, exception));
                } catch (UnauthorizedAccessException exception) {
                    Debug.WriteLine(exception);
                    Error?.Invoke(this, new PersistWindowStateEventArgs(registryKey, exception));
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                }
                if (registryKey == null) {
                    return;
                }
                if (!DisableSavePosition) {
                    registryKey.SetValue(parent.Name + Location, PointToInt(nLocation));
                }
                if (!DisableSaveSize
                        && !(DisableSaveWidth && DisableSaveHeight)
                        && (parent.FormBorderStyle.Equals(FormBorderStyle.Sizable)
                            || parent.FormBorderStyle.Equals(FormBorderStyle.SizableToolWindow))) {

                    registryKey.SetValue(parent.Name + Size, SizeToInt(nSize));
                }
                if (!DisableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)
                        || AllowSaveTopMost) {

                    if (AllowSaveMinimized || !parent.WindowState.Equals(FormWindowState.Minimized)) {
                        registryKey.SetValue(parent.Name + State, WindowStateToInt(parent.WindowState, parent.TopMost));
                    } else {
                        registryKey.SetValue(parent.Name + State, WindowStateToInt(windowState, parent.TopMost));
                    }
                }
                Saved?.Invoke(this, new PersistWindowStateEventArgs(registryKey));
            }
        }

        /// <summary>
        /// Thread-safe sets the parent form visible for the user and brings it
        /// to the front before other windows on the user's desktop except the
        /// reference form.
        /// </summary>
        /// <param name="hWnd">Reference form window handle.</param>
        public void SetVisible(IntPtr hWnd) {
            if (!parentHandle.Equals(IntPtr.Zero)) {
                if (!IsIconic(parentHandle).Equals(0)) {
                    ShowWindow(parentHandle, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                } else if (!IsNextWindow(hWnd)) {
                    SetForegroundWindow(parentHandle);
                    SetForegroundWindow(hWnd);
                }
            }
        }

        /// <summary>
        /// Calculates the new location for the parent form to be visible on the
        /// screen.
        /// </summary>
        /// <param name="location">Current location of the parent form.</param>
        /// <param name="size">Current size of the parent form.</param>
        /// <returns>Adjusted location for the parent form.</returns>
        private static Point AdjustLocation(Point location, Size size) {
            Point point = new Point(location.X, location.Y);
            if (point.X < SystemInformation.VirtualScreen.Left) {
                point.X = SystemInformation.VirtualScreen.Left;
            } else if (point.X + size.Width > SystemInformation.VirtualScreen.Width) {
                point.X = SystemInformation.VirtualScreen.Width - size.Width;
            }
            if (point.Y < SystemInformation.VirtualScreen.Top) {
                point.Y = SystemInformation.VirtualScreen.Top;
            } else if (point.Y + size.Height > SystemInformation.VirtualScreen.Height) {
                point.Y = SystemInformation.VirtualScreen.Height - size.Height;
            }
            return point;
        }

        /// <summary>
        /// Converts System.Int32 to System.Drawing.Point.
        /// </summary>
        /// <param name="val">An integer value stored in the registry.</param>
        private static Point IntToPoint(int val) {
            byte[] bytes = BitConverter.GetBytes(val);
            return new Point(
                BitConverter.ToInt16(bytes, BitConverter.IsLittleEndian ? 0 : 2),
                BitConverter.ToInt16(bytes, BitConverter.IsLittleEndian ? 2 : 0));
        }

        /// <summary>
        /// Converts System.Int32 to System.Drawing.Size.
        /// </summary>
        /// <param name="val">An integer value stored in the registry.</param>
        private static Size IntToSize(int val) {
            byte[] bytes = BitConverter.GetBytes(val);
            return new Size(
                BitConverter.ToUInt16(bytes, BitConverter.IsLittleEndian ? 0 : 2),
                BitConverter.ToUInt16(bytes, BitConverter.IsLittleEndian ? 2 : 0));
        }

        /// <summary>
        /// Converts System.Int32 to System.Windows.Forms.FormWindowState and to
        /// System.Boolean.
        /// </summary>
        /// <param name="val">An integer value stored in the registry.</param>
        private static FormWindowState IntToWindowState(int val, out bool topMost) {
            byte[] bytes = BitConverter.GetBytes(val);
            topMost = BitConverter.ToInt16(bytes, BitConverter.IsLittleEndian ? 2 : 0) > 0;
            short windowState = BitConverter.ToInt16(bytes, BitConverter.IsLittleEndian ? 0 : 2);
            return windowState.Equals(1) || windowState.Equals(2) ? (FormWindowState)windowState : FormWindowState.Normal;
        }

        /// <summary>
        /// Converts System.Drawing.Point to System.Int32.
        /// </summary>
        /// <param name="point">Location of the form to be saved.</param>
        /// <returns>An integer value to store in the Windows registry.</returns>
        private static int PointToInt(Point point) {
            byte[] bytes = new byte[4];
            Array.Copy(
                BitConverter.GetBytes(point.X),
                BitConverter.IsLittleEndian ? 0 : 2,
                bytes,
                BitConverter.IsLittleEndian ? 0 : 2,
                2);
            Array.Copy(
                BitConverter.GetBytes(point.Y),
                BitConverter.IsLittleEndian ? 0 : 2,
                bytes,
                BitConverter.IsLittleEndian ? 2 : 0,
                2);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Converts System.Drawing.Size to System.Int32.
        /// </summary>
        /// <param name="size">Size of the form to be saved.</param>
        /// <returns>An integer value to store in the Windows registry.</returns>
        private static int SizeToInt(Size size) {
            byte[] bytes = new byte[4];
            Array.Copy(
                BitConverter.GetBytes(size.Width),
                BitConverter.IsLittleEndian ? 0 : 2,
                bytes,
                BitConverter.IsLittleEndian ? 0 : 2,
                2);
            Array.Copy(
                BitConverter.GetBytes(size.Height),
                BitConverter.IsLittleEndian ? 0 : 2,
                bytes,
                BitConverter.IsLittleEndian ? 2 : 0,
                2);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Converts System.Windows.Forms.FormWindowState and System.Boolean to
        /// System.Int32.
        /// </summary>
        /// <param name="formWindowState">FormWindowState to be saved.</param>
        /// <param name="topMost">A boolean value to be saved.</param>
        /// <returns>An integer value to store in the Windows registry.</returns>
        private static int WindowStateToInt(FormWindowState formWindowState, bool topMost) {
            byte[] bytes = new byte[4];
            Array.Copy(
                BitConverter.GetBytes((short)formWindowState),
                0,
                bytes,
                BitConverter.IsLittleEndian ? 0 : 2,
                2);
            Array.Copy(
                BitConverter.GetBytes(Convert.ToInt16(topMost)),
                0,
                bytes,
                BitConverter.IsLittleEndian ? 2 : 0,
                2);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Other opened windows detection options.
        /// </summary>
        public enum WindowDetectionOptions {
            /// <summary>
            /// The default detection method by process file name.
            /// </summary>
            Default,
            /// <summary>
            /// The already opened window will be determined by title comparison.
            /// </summary>
            TitleContains,
            /// <summary>
            /// The already opened window will be determined by title comparison.
            /// </summary>
            TitleStartsWith,
            /// <summary>
            /// The already opened window will be determined by title comparison.
            /// </summary>
            TitleEndsWith,
            /// <summary>
            /// The already opened window will be determined by title comparison.
            /// </summary>
            TitleEquals,
            /// <summary>
            /// No detection of other form instances will be performed. Use this
            /// option only for one instance forms.
            /// </summary>
            NoDetection
        }

        /// <summary>
        /// PersistWindowState saving options.
        /// </summary>
        public enum PersistWindowStateSavingOptions {
            /// <summary>
            /// The state of the parent form will be saved only within the scope
            /// of the running instance of the software application.
            /// </summary>
            None,
            /// <summary>
            /// The state of the parent form will be saved permanently in the
            /// Windows registry.
            /// </summary>
            Registry
        }
    }

    /// <summary>
    /// Implements custom event args used by PersistWindowState class.
    /// </summary>
    public sealed class PersistWindowStateEventArgs : EventArgs {

        /// <summary>
        /// The Exception property.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The RegistryKey property.
        /// </summary>
        public RegistryKey RegistryKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PersistWindowStateEventArgs"/> class.
        /// </summary>
        public PersistWindowStateEventArgs(RegistryKey registryKey = null, Exception exception = null) {
            Exception = exception;
            RegistryKey = registryKey;
        }
    }
}

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace FSTools {
    public class PersistWindowState : Component {
        private Form parent;
        private FormWindowState windowState;
        private PersistWindowStateSavingOptions savingOptions;
        private string registryPath;
        private int normalLeft, normalTop, normalWidth, normalHeight;
        private bool allowSaveTopMost, allowSaveMinimized, disableSavePosition, disableSaveSize, disableSaveWindowState, disableSaveWidth, disableSaveHeight, placeOnScreenAfterLoad;
        private bool topMost, fixWidth, fixHeight, firstLoad, moved;

        public delegate void WindowStateEventHandler(object sender, RegistryKey registryKey);
        public delegate void WindowStateErrorEventHandler(object sender, RegistryKey registryKey, Exception exception);

        public event WindowStateEventHandler WindowStateLoaded;
        public event WindowStateEventHandler WindowStateSaved;
        public event WindowStateErrorEventHandler RegistryAccessFailed;

        public PersistWindowState() {
            firstLoad = true;
            placeOnScreenAfterLoad = true;
            savingOptions = PersistWindowStateSavingOptions.Registry;
            registryPath = Path.Combine("Software", Application.CompanyName, Application.ProductName);
        }

        public Form Parent {
            set {
                parent = value;
                parent.Load += new EventHandler(OnLoad);
                parent.FormClosed += new FormClosedEventHandler(OnClosed);
            }
            get {
                return parent;
            }
        }

        public PersistWindowStateSavingOptions SavingOptions {
            get {
                return savingOptions;
            }
            set {
                savingOptions = value;
            }
        }

        public string RegistryPath {
            set {
                registryPath = value;
            }
            get {
                return registryPath;
            }
        }

        public bool AllowSaveTopMost {
            set {
                allowSaveTopMost = value;
            }
            get {
                return allowSaveTopMost;
            }
        }

        public bool AllowSaveMinimized {
            set {
                allowSaveMinimized = value;
            }
            get {
                return allowSaveMinimized;
            }
        }

        public bool PlaceOnScreenAfterLoad {
            set {
                placeOnScreenAfterLoad = value;
            }
            get {
                return placeOnScreenAfterLoad;
            }
        }

        public bool DisableSavePosition {
            set {
                disableSavePosition = value;
            }
            get {
                return disableSavePosition;
            }
        }

        public bool DisableSaveSize {
            set {
                disableSaveSize = value;
            }
            get {
                return disableSaveSize;
            }
        }

        public bool DisableSaveWindowState {
            set {
                disableSaveWindowState = value;
            }
            get {
                return disableSaveWindowState;
            }
        }

        public bool DisableSaveWidth {
            set {
                disableSaveWidth = value;
            }
            get {
                return disableSaveWidth;
            }
        }

        public bool DisableSaveHeight {
            set {
                disableSaveHeight = value;
            }
            get {
                return disableSaveHeight;
            }
        }

        public bool FixWidth {
            set {
                fixWidth = value;
            }
            get {
                return fixWidth;
            }
        }

        public bool FixHeight {
            set {
                fixHeight = value;
            }
            get {
                return fixHeight;
            }
        }

        private void OnResize(object sender, EventArgs e) {
            if (parent.WindowState == FormWindowState.Normal) {
                if (fixWidth && normalWidth != 0) {
                    parent.Width = normalWidth;
                } else {
                    normalWidth = parent.Width;
                }
                if (fixHeight && normalHeight != 0) {
                    parent.Height = normalHeight;
                } else {
                    normalHeight = parent.Height;
                }
            }
            windowState = parent.WindowState;
        }

        private void OnMove(object sender, EventArgs e) {
            if (parent.WindowState == FormWindowState.Normal) {
                normalLeft = parent.Location.X;
                normalTop = parent.Location.Y;
            }
            windowState = parent.WindowState;
            moved = true;
        }

        private void OnClosed(object sender, FormClosedEventArgs e) {
            Save();
        }

        public void Save() {
            if (savingOptions == PersistWindowStateSavingOptions.Registry) {
                RegistryKey registryKey = null;
                try {
                    registryKey = Registry.CurrentUser.CreateSubKey(registryPath);
                } catch (SecurityException exception) {
                    Debug.WriteLine(exception);
                    RegistryAccessFailed?.Invoke(this, registryKey, exception);
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                }
                if (registryKey == null) {
                    return;
                }
                if (allowSaveTopMost) {
                    registryKey.SetValue(parent.Name + "TopMost", parent.TopMost ? 1 : 0);
                }
                if (!disableSavePosition && moved) {
                    registryKey.SetValue(parent.Name + "Left", normalLeft);
                    registryKey.SetValue(parent.Name + "Top", normalTop);
                }
                if (!disableSaveSize && (parent.FormBorderStyle == FormBorderStyle.Sizable || parent.FormBorderStyle == FormBorderStyle.SizableToolWindow)) {
                    if (!disableSaveWidth && normalWidth > 0) {
                        registryKey.SetValue(parent.Name + "Width", normalWidth);
                    }
                    if (!disableSaveHeight && normalHeight > 0) {
                        registryKey.SetValue(parent.Name + "Height", normalHeight);
                    }
                }
                if (!disableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)) {
                    if (allowSaveMinimized || parent.WindowState != FormWindowState.Minimized) {
                        registryKey.SetValue(parent.Name + "WindowState", (int)parent.WindowState);
                    } else {
                        registryKey.SetValue(parent.Name + "WindowState", (int)FormWindowState.Normal);
                    }
                }
                WindowStateSaved?.Invoke(this, registryKey);
            }
        }

        private void OnLoad(object sender, EventArgs e) {
            if (savingOptions == PersistWindowStateSavingOptions.None || string.IsNullOrEmpty(registryPath)) {
                if (firstLoad) {
                    OnResize(sender, e);
                    OnMove(sender, e);
                    if (placeOnScreenAfterLoad) {
                        parent.Location = AdjustLocation(new Point(normalLeft, normalTop), parent.Size);
                    }
                    firstLoad = false;
                } else {
                    if (allowSaveTopMost) {
                        parent.TopMost = topMost;
                    }
                    if (!disableSaveSize && (parent.FormBorderStyle == FormBorderStyle.Sizable || parent.FormBorderStyle == FormBorderStyle.SizableToolWindow)) {
                        if (!disableSaveWidth) {
                            parent.Width = normalWidth;
                        }
                        if (!disableSaveHeight) {
                            parent.Height = normalHeight;
                        }
                    }
                    if (placeOnScreenAfterLoad && !disableSavePosition) {
                        parent.Location = AdjustLocation(new Point(normalLeft, normalTop), parent.Size);
                    } else if (!disableSavePosition) {
                        parent.Location = new Point(normalLeft, normalTop);
                    }
                    if (!disableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)) {
                        parent.WindowState = allowSaveMinimized || parent.WindowState != FormWindowState.Minimized ? windowState : FormWindowState.Normal;
                    }
                }
            } else if (savingOptions == PersistWindowStateSavingOptions.Registry) {
                RegistryKey registryKey = null;
                try {
                    registryKey = Registry.CurrentUser.OpenSubKey(registryPath);
                } catch (SecurityException exception) {
                    Debug.WriteLine(exception);
                    RegistryAccessFailed?.Invoke(this, registryKey, exception);
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                }
                if (registryKey != null && !IsAlreadyRunning()) {
                    if (allowSaveTopMost) {
                        topMost = parent.TopMost = (int)registryKey.GetValue(parent.Name + "TopMost", parent.TopMost) > 0;
                    }
                    normalLeft = (int)registryKey.GetValue(parent.Name + "Left", parent.Location.X);
                    normalTop = (int)registryKey.GetValue(parent.Name + "Top", parent.Location.Y);
                    if (!disableSaveSize && (parent.FormBorderStyle == FormBorderStyle.Sizable || parent.FormBorderStyle == FormBorderStyle.SizableToolWindow)) {
                        if (!disableSaveWidth) {
                            int width = (int)registryKey.GetValue(parent.Name + "Width", parent.Width);
                            if (width > 0) {
                                parent.Width = width;
                                firstLoad = false;
                            }
                        }
                        if (!disableSaveHeight) {
                            int height = (int)registryKey.GetValue(parent.Name + "Height", parent.Height);
                            if (height > 0) {
                                parent.Height = height;
                                firstLoad = false;
                            }
                        }
                    }
                    if (placeOnScreenAfterLoad && !disableSavePosition) {
                        parent.Location = AdjustLocation(new Point(normalLeft, normalTop), parent.Size);
                    } else if (!disableSavePosition) {
                        parent.Location = new Point(normalLeft, normalTop);
                    }
                    normalWidth = parent.Width;
                    normalHeight = parent.Height;
                    if (!disableSaveWindowState && parent.ControlBox && (parent.MinimizeBox || parent.MaximizeBox)) {
                        windowState = (FormWindowState)registryKey.GetValue(parent.Name + "WindowState", (int)parent.WindowState);
                        if (!allowSaveMinimized && windowState == FormWindowState.Minimized) {
                            windowState = FormWindowState.Normal;
                        }
                        parent.WindowState = windowState;
                    }
                    WindowStateLoaded?.Invoke(this, registryKey);
                }
            }
            parent.Resize += new EventHandler(OnResize);
            parent.Move += new EventHandler(OnMove);
        }

        private bool IsAlreadyRunning() {
            Process currentProcess = Process.GetCurrentProcess();
            FileSystemInfo fileSystemInfo1 = new FileInfo(currentProcess.MainModule.FileName);
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName)) {
                if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero) {
                    FileSystemInfo fileSystemInfo2 = new FileInfo(process.MainModule.FileName);
                    if (fileSystemInfo1.Name == fileSystemInfo2.Name) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Point AdjustLocation(Point location, Size size) {
            Point point = new Point(location.X, location.Y);
            if (point.X + size.Width > SystemInformation.VirtualScreen.Width) {
                point.X = SystemInformation.VirtualScreen.Width - size.Width;
            } else if (point.X < SystemInformation.VirtualScreen.Left) {
                point.X = SystemInformation.VirtualScreen.Left;
            }
            if (point.Y + size.Height > SystemInformation.VirtualScreen.Height) {
                point.Y = SystemInformation.VirtualScreen.Height - size.Height;
            } else if (point.Y < SystemInformation.VirtualScreen.Top) {
                point.Y = SystemInformation.VirtualScreen.Top;
            }
            return point;
        }

        public enum PersistWindowStateSavingOptions {
            None, Registry
        }
    }
}

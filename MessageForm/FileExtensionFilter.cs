using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MessageForm {
    public class FileExtensionFilter {
        private Dictionary<string, string> filters;
        private int filterIndex;
        private string[] imageTypeFilter;

        public FileExtensionFilter(int defaultFilterIndex) {
            filters = new Dictionary<string, string>(6);
            filters.Add(Constants.ExtensionBmp, "Windows Bitmap BMP (*" + Constants.ExtensionBmp + ")|*" + Constants.ExtensionBmp);
            filters.Add(Constants.ExtensionGif, "CompuServe GIF 89a (*" + Constants.ExtensionGif + ")|*" + Constants.ExtensionGif);
            filters.Add(Constants.ExtensionJpg, "JPEG File Interchange Format (*" + Constants.ExtensionJpg + ")|*" + Constants.ExtensionJpg);
            filters.Add(Constants.ExtensionPng, "Portable Network Graphics PNG (*" + Constants.ExtensionPng + ")|*" + Constants.ExtensionPng);
            filters.Add(Constants.ExtensionTif, "Tagged Image File Format TIFF (*" + Constants.ExtensionTif + ")|*" + Constants.ExtensionTif);
            filters.Add(Constants.ExtensionWebP, "Google WebP (*" + Constants.ExtensionWebP + ")|*" + Constants.ExtensionWebP);

            string[] imageBasicTypeFilter = new string[] { filters[Constants.ExtensionBmp], filters[Constants.ExtensionGif], filters[Constants.ExtensionJpg], filters[Constants.ExtensionPng], filters[Constants.ExtensionTif] };
            string[] imageWebPTypeFilter = new string[] { filters[Constants.ExtensionWebP] };
            try {
                if (File.Exists(Path.Combine(Application.StartupPath, "libwebp_x86.dll")) && File.Exists(Path.Combine(Application.StartupPath, "libwebp_x64.dll"))) {
                    imageTypeFilter = new string[imageBasicTypeFilter.Length + imageWebPTypeFilter.Length];
                    Array.Copy(imageBasicTypeFilter, imageTypeFilter, imageBasicTypeFilter.Length);
                    Array.Copy(imageWebPTypeFilter, 0, imageTypeFilter, imageBasicTypeFilter.Length, imageWebPTypeFilter.Length);
                } else {
                    imageTypeFilter = imageBasicTypeFilter;
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                imageTypeFilter = imageBasicTypeFilter;
            }
            filterIndex = defaultFilterIndex;
        }

        public string GetFilter() {
            return string.Join("|", imageTypeFilter);
        }

        public string GetFilter(string extension) {
            return filters[extension];
        }

        public int GetFilterIndex() {
            return filterIndex;
        }

        public void SetFilterIndex(int filterIndex) {
            if (filterIndex > 0 && filterIndex <= imageTypeFilter.Length) {
                this.filterIndex = filterIndex;
            }
        }
    }
}

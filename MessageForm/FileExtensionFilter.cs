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
            filters.Add(Constants.ExtensionBmp, Constants.ExtensionFilterBmp);
            filters.Add(Constants.ExtensionGif, Constants.ExtensionFilterGif);
            filters.Add(Constants.ExtensionJpg, Constants.ExtensionFilterJpg);
            filters.Add(Constants.ExtensionPng, Constants.ExtensionFilterPng);
            filters.Add(Constants.ExtensionTif, Constants.ExtensionFilterTif);
            filters.Add(Constants.ExtensionWebP, Constants.ExtensionFilterWebP);

            string[] imageBasicTypeFilter = new string[] {
                filters[Constants.ExtensionBmp],
                filters[Constants.ExtensionGif],
                filters[Constants.ExtensionJpg],
                filters[Constants.ExtensionPng],
                filters[Constants.ExtensionTif]
            };
            string[] imageWebPTypeFilter = new string[] {
                filters[Constants.ExtensionWebP]
            };
            try {
                if (File.Exists(Path.Combine(Application.StartupPath, Constants.LibWebPX86FileName))
                        && File.Exists(Path.Combine(Application.StartupPath, Constants.LibWebPX64FileName))) {

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

        public string GetFilter() => string.Join(Constants.VerticalBar.ToString(), imageTypeFilter);

        public string GetFilter(string extension) => filters[extension];

        public int GetFilterIndex() => filterIndex;

        public void SetFilterIndex(int filterIndex) {
            if (filterIndex > 0 && filterIndex <= imageTypeFilter.Length) {
                this.filterIndex = filterIndex;
            }
        }
    }
}

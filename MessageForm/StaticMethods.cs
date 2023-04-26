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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WebPWrapper;

namespace MessageForm {
    public static class StaticMethods {
        public static StringBuilder AppendIndent(this StringBuilder stringBuilder) {
            for (int i = 0; i < Constants.Indentation; i++) {
                stringBuilder.Append(Constants.Space);
            }
            return stringBuilder;
        }

        public static string EscapeArgument(string argument) {
            argument = Regex.Replace(argument, @"(\\*)" + Constants.QuotationMark, @"$1$1\" + Constants.QuotationMark);
            return Constants.QuotationMark + Regex.Replace(argument, @"(\\+)$", @"$1$1") + Constants.QuotationMark;
        }

        public static void ExportAsImage(Control control, string filePath) {
            using (Bitmap bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppArgb)) {
                control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                SaveBitmap(bitmap, filePath);
            }
        }

        public static Size GetNewGraphicsSize(Size graphicSize, Size canvasSize) {
            bool rotate = IsGraphicsRotationNeeded(graphicSize, canvasSize);
            float ratio = 1f;
            float ratioWidth = graphicSize.Width / (float)(rotate ? canvasSize.Height : canvasSize.Width);
            float ratioHeight = graphicSize.Height / (float)(rotate ? canvasSize.Width : canvasSize.Height);
            float ratioMax = Math.Max(ratioWidth, ratioHeight);
            if (ratioMax > ratio) {
                ratio = ratioMax;
            }
            return new Size((int)Math.Floor(graphicSize.Width / ratio), (int)Math.Floor(graphicSize.Height / ratio));
        }

        public static bool IsGraphicsRotationNeeded(Size graphicSize, Size canvasSize) {
            if (graphicSize.Width <= 0 || graphicSize.Height <= 0 || canvasSize.Width <= 0 || canvasSize.Height <= 0) {
                return false;
            }
            if (graphicSize.Width / (float)graphicSize.Height == 1f || canvasSize.Width / (float)canvasSize.Height == 1f) {
                return false;
            }
            if (graphicSize.Width < canvasSize.Width && graphicSize.Height < canvasSize.Height) {
                return false;
            }
            if (graphicSize.Width / (float)graphicSize.Height < 1f && canvasSize.Width / (float)canvasSize.Height < 1f ||
                graphicSize.Width / (float)graphicSize.Height > 1f && canvasSize.Width / (float)canvasSize.Height > 1f) {
                return false;
            }
            return true;
        }

        public static void SaveBitmap(Bitmap bitmap, string finePath) {
            switch (Path.GetExtension(finePath).ToLowerInvariant()) {
                case Constants.ExtensionBmp:
                    bitmap.Save(finePath, ImageFormat.Bmp);
                    break;
                case Constants.ExtensionGif:
                    bitmap.Save(finePath, ImageFormat.Gif);
                    break;
                case Constants.ExtensionJpg:
                    bitmap.Save(finePath, ImageFormat.Jpeg);
                    break;
                case Constants.ExtensionTif:
                    bitmap.Save(finePath, ImageFormat.Tiff);
                    break;
                case Constants.ExtensionWebP:
                    using (WebP webP = new WebP()) {
                        File.WriteAllBytes(finePath, webP.EncodeLossless(bitmap));
                    }
                    break;
                default:
                    bitmap.Save(finePath, ImageFormat.Png);
                    break;
            }
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace GearGenerator
{
    /// <summary>
    /// A print mechanism that prints a large visual as bitmaps across
    /// multiple pages. Adapted from https://www.codeproject.com/Articles/339416/Printing-large-WPF-UserControls
    /// under the Code Project Open License (CPOL): https://www.codeproject.com/info/cpol10.aspx
    /// </summary>
    public class VisualPrinter
    {
        #region Members
        /// <summary>
        /// The left and right-hand margins in pixels.
        /// </summary>
        public static int HorizontalBorder = 5;

        /// <summary>
        /// The top and bottom margins in pixels.
        /// </summary>
        public static int VerticalBorder = 5;

        /// <summary>
        /// The expected horizontal DPI.
        /// </summary>
        private static readonly double HorizontalDpi = 600;

        /// <summary>
        /// The expected vertical DPI.
        /// </summary>
        private static readonly double VerticalDpi = 600;
        #endregion


        #region Public Methods
        /// <summary>
        /// Prints a visual, breaking across pages. The user should've already
        /// accepted the print job. Returns success.
        /// </summary>
        public static bool PrintAcrossPages(PrintDialog dlg, FrameworkElement element)
        {
            var printable = element;

            if (dlg != null && printable != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var capabilities = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
                dlg.PrintTicket.PageBorderless = PageBorderless.None;

                var dpiScale = VerticalDpi / 96.0;
                var document = new FixedDocument();

                try
                {
                    //Sets width and waits for changes to settle.
                    printable.Width = capabilities.PageImageableArea.ExtentWidth;
                    printable.UpdateLayout();

                    //Recomputes the desired height.
                    printable.Measure(new Size( double.PositiveInfinity, double.PositiveInfinity));

                    //Sets the new desired size.
                    var size = new Size( capabilities.PageImageableArea.ExtentWidth, printable.DesiredSize.Height);

                    //Measures and arranges to the desired size.
                    printable.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    printable.Arrange(new Rect(0, 0, printable.DesiredSize.Width, printable.DesiredSize.Height));

                    //printable.Measure(size);
                    //printable.Arrange(new Rect(size));

                    printable.UpdateLayout();

                    //Converts GUI to bitmap at 300 DPI
                    var bmpTarget = new RenderTargetBitmap(
                        (int)(capabilities.PageImageableArea.ExtentWidth * dpiScale),
                        (int)(capabilities.PageImageableArea.ExtentHeight * dpiScale),
                        HorizontalDpi, VerticalDpi, PixelFormats.Pbgra32);
                    bmpTarget.Render(printable);

                    //Converts RenderTargetBitmap to bitmap.
                    var png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(bmpTarget));

                    Bitmap bmp;
                    using (var memStream = new MemoryStream())
                    {
                        png.Save(memStream);
                        bmp = new Bitmap(memStream);
                    }

                    using (bmp)
                    {
                        document.DocumentPaginator.PageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);

                        //Breaks bitmap to fit across pages.
                        var pageBreak = 0;
                        var lastPageBreak = 0;
                        var pageHeight = (int) (capabilities.PageImageableArea.ExtentHeight * dpiScale);

                        //Adds each full page.
                        while (pageBreak < bmp.Height - pageHeight)
                        {
                            pageBreak += pageHeight;

                            //Finds a page breakpoint from bottom, up.
                            pageBreak = FindRowBreakpoint(bmp, lastPageBreak, pageBreak);

                            //Adds the image segment to its own page.
                            var pageContent = GeneratePageContent(
                                bmp, lastPageBreak, pageBreak,
                                document.DocumentPaginator.PageSize.Width,
                                document.DocumentPaginator.PageSize.Height,
                                capabilities);

                            document.Pages.Add(pageContent);
                            lastPageBreak = pageBreak;
                        }

                        //Adds remaining page contents.
                        var lastPageContent = GeneratePageContent(
                            bmp, lastPageBreak,
                            bmp.Height, document.DocumentPaginator.PageSize.Width,
                            document.DocumentPaginator.PageSize.Height, capabilities);

                        document.Pages.Add(lastPageContent);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show( $"An error occurred while trying to print. {e.Message}");
                }

                //Drops visual size adjustments.
                finally
                {
                    //Unsets width and waits for changes to settle.
                    printable.Width = double.NaN;
                    printable.UpdateLayout();

                    printable.LayoutTransform = new ScaleTransform(1, 1);

                    //Recomputes the desired height.
                    var size = new Size(
                        capabilities.PageImageableArea.ExtentWidth,
                        capabilities.PageImageableArea.ExtentHeight);

                    //Measures and arranges to the desired size.
                    printable.Measure(size);
                    printable.Arrange(new Rect(new Point(
                        capabilities.PageImageableArea.OriginWidth,
                        capabilities.PageImageableArea.OriginHeight), size));

                    Mouse.OverrideCursor = null;
                }

                dlg.PrintDocument(document.DocumentPaginator, "Print Document Name");
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Iterates from the bottom line upwards to the top (so as to trim as
        /// little as possible from the complete page) to determine where to
        /// separate a page. Returns the row to break, or last if none found.
        /// </summary>
        private static unsafe int FindRowBreakpoint(
            Bitmap bmp,
            int topLine,
            int bottomLine)
        {
            //Any computed deviation above the threshold
            //is considered too detailed to break on.
            double deviationThreshold = 1627500;

            //Locks to read data.
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, bmp.PixelFormat);

            //Sets the initial row and position.
            var stride = bmpData.Stride;
            var topLeftPixel = bmpData.Scan0;
            var p = (byte*)(void*)topLeftPixel;

            //Iterates from bottom to top to find a breakable row.
            for (var i = bottomLine; i > topLine; i--)
            {
                var count = 0;
                double total = 0;
                double totalVariance = 0;

                //Sets pointer to this row.
                p = (byte*)(void*)topLeftPixel + stride * i;

                //Iterates through each consecutive pixel in the given row.
                for (var column = 0; column < bmp.Width; column++)
                {
                    count++;

                    var red = p[1];
                    var green = p[2];
                    var blue = p[3];

                    //Faster than System.Drawing.Color.FromArgb(0, red, green, blue).ToArgb().
                    var pixelValue = (red << 16) | (green << 8) | blue;

                    total += pixelValue;
                    var average = total / count;
                    totalVariance += (pixelValue - average) * (pixelValue - average);

                    //Skips to next pixel.
                    p += 4;
                }

                //Breaks on this line if possible.
                var standardDeviation = Math.Sqrt(totalVariance / count);
                if (Math.Sqrt(totalVariance / count) < deviationThreshold)
                {
                    bmp.UnlockBits(bmpData);
                    return i;
                }
            }

            //Breaks on the last line given if no break row is found.
            bmp.UnlockBits(bmpData);
            return bottomLine;
        }

        /// <summary>
        /// Sizes the given bitmap to the page size and returns it as part
        /// of a printable page.
        /// </summary>
        private static PageContent GeneratePageContent(
            Bitmap bmp,
            int top,
            int bottom,
            double pageWidth,
            double pageHeight,
            PrintCapabilities capabilities)
        {
            Image pageImage;
            BitmapSource bmpSource;

            //Creates a page with a specific width/height.
            var printPage = new FixedPage();
            printPage.Width = pageWidth;
            printPage.Height = pageHeight;

            //Cuts the given image at a reasonable boundary.
            var newImageHeight = bottom - top;

            //Creates a clone of the image.
            using (var bmpCut = bmp.Clone(new Rectangle(0, top, bmp.Width, newImageHeight), bmp.PixelFormat))
            {
                //Prepares the bitmap source.
                pageImage = new Image();
                bmpSource =
                    Imaging.CreateBitmapSourceFromHBitmap(
                        bmpCut.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bmpCut.Width, bmpCut.Height));
            }

            //Adds the bitmap to the page.
            pageImage.Source = bmpSource;
            pageImage.VerticalAlignment = VerticalAlignment.Top;
            printPage.Children.Add(pageImage);

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(printPage);

            //Adds a margin.
            printPage.Margin = new Thickness(
                HorizontalBorder, VerticalBorder,
                HorizontalBorder, VerticalBorder);

            FixedPage.SetLeft(pageImage, capabilities.PageImageableArea.OriginWidth);
            FixedPage.SetTop(pageImage, capabilities.PageImageableArea.OriginHeight);

            //Adjusts for the margins and to fit the page.
            pageImage.Width = capabilities.PageImageableArea.ExtentWidth - HorizontalBorder * 2;
            pageImage.Height = capabilities.PageImageableArea.ExtentHeight - VerticalBorder * 2;
            return pageContent;
        }
        #endregion
    }
}
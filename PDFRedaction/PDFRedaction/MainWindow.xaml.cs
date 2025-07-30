using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Exporting;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace PDFRedaction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NRAiBiAaIQQuGjN/VkZ+XU9FfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5Vd0ViUH5XdXxdT2dYWkd2");
        }

        private void btnRedactPDFwithoutColor(object sender, RoutedEventArgs e)
        {
            //Load a PDF document for redaction
            PdfLoadedDocument ldoc = new PdfLoadedDocument("../../Input/RedactPDFwithEmail.pdf");

            //Get first page from document
            PdfLoadedPage lpage = ldoc.Pages[0] as PdfLoadedPage;

            //Create PDF redaction for the page
            PdfRedaction redaction = new PdfRedaction(new RectangleF(340, 120, 140, 20));

            //Adds the redaction to loaded 1st page
            lpage.Redactions.Add(redaction);

            //Save the redacted PDF document to disk
            ldoc.Save("RedactedPDF.pdf");

            //Close the document instance
            ldoc.Close(true);
            Process.Start("RedactedPDF.pdf");
        }

        private void btnRedactPDFwithColor_Click(object sender, RoutedEventArgs e)
        {
            //Load a PDF document for redaction
            PdfLoadedDocument ldoc = new PdfLoadedDocument("../../Input/RedactPDFwithEmail.pdf");

            //Get first page from document
            PdfLoadedPage lpage = ldoc.Pages[0] as PdfLoadedPage;

            //Create PDF redaction for the page
            PdfRedaction redaction = new PdfRedaction(new RectangleF(340, 120, 140, 20), System.Drawing.Color.Red);

            //Adds the redaction to loaded page
            lpage.Redactions.Add(redaction);

            //Save the redacted PDF document to disk
            ldoc.Save("RedactedPDF.pdf");

            //Close the document instance
            ldoc.Close(true);
            Process.Start("RedactedPDF.pdf");
        }

        private void btnRedactPDFwithCodes_Click(object sender, RoutedEventArgs e)
        {
            //Load a PDF document for redaction
            PdfLoadedDocument ldoc = new PdfLoadedDocument("../../Input/W4-tax-form.pdf");

            //Get first page from document to redact informations.
            PdfLoadedPage lpage = ldoc.Pages[0] as PdfLoadedPage;

            //Create redaction area for redacting telephone number with code set.
            RectangleF redactionBound = new RectangleF(50, 568, 120, 13);

            PdfRedaction redaction = new PdfRedaction(redactionBound);
            redaction.Appearance.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(0, 0, redactionBound.Width, redactionBound.Height));
            redaction.Appearance.Graphics.DrawString("(b) (6)", new PdfStandardFont(PdfFontFamily.Helvetica, 11), PdfBrushes.White, new PointF(0, 0));

            //Adds the redaction to loaded page
            lpage.Redactions.Add(redaction);

            //Create redaction area for redacting address with code set.
            RectangleF addressRedaction = new RectangleF(50, 592, 75, 13);
            redaction = new PdfRedaction(addressRedaction);
            redaction.Appearance.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(0, 0, addressRedaction.Width, addressRedaction.Height));
            redaction.Appearance.Graphics.DrawString("(b) (6)", new PdfStandardFont(PdfFontFamily.Helvetica, 11), PdfBrushes.White, new PointF(0, 0));
            lpage.Redactions.Add(redaction);

            //Save the redacted PDF document to disk
            ldoc.Save("RedactedPDF.pdf");

            //Close the document instance
            ldoc.Close(true);
            Process.Start("RedactedPDF.pdf");
        }

        private void btnRedactPDFImage_Click(object sender, RoutedEventArgs e)
        {
            using (OCRProcessor processor = new OCRProcessor())
            {
                //Load the PDF document 
                PdfLoadedDocument lDoc = new PdfLoadedDocument(@"../../Input/FormWithSSN.pdf");

                //Load the PDF page
                PdfLoadedPage loadedPage = lDoc.Pages[0] as PdfLoadedPage;

                //Language to process the OCR
                processor.Settings.Language = Languages.English;

                //Extract image and information from the PDF for processing OCR
                PdfImageInfo[] imageInfoCollection = loadedPage.ImagesInfo;

                foreach (PdfImageInfo imgInfo in imageInfoCollection)
                {
                    Bitmap ocrImage = imgInfo.Image as Bitmap;
                    OCRLayoutResult result = null;
                    float scaleX = 0, scaleY = 0;
                    if (ocrImage != null)
                    {
                        //Process OCR by providing loaded PDF document, Data dictionary and language
                        string text = processor.PerformOCR(ocrImage, @"../../LanguagePack/", out result);

                        //Calculate the scale factor for the image used in the PDF
                        scaleX = imgInfo.Bounds.Height / ocrImage.Height;
                        scaleY = imgInfo.Bounds.Width / ocrImage.Width;
                    }

                    //Get the text from page and lines.
                    foreach (var page in result.Pages)
                    {
                        foreach (var line in page.Lines)
                        {
                            if (line.Text != null)
                            {
                                //Regular expression for social security number
                                var ssnMatches = Regex.Matches(line.Text, @"(\d{3})+[ -]*(\d{2})+[ -]*\d{4}", RegexOptions.IgnorePatternWhitespace);
                                if (ssnMatches.Count >= 1)
                                {
                                    RectangleF redactionBound = new RectangleF(line.Rectangle.X * scaleX, line.Rectangle.Y * scaleY,
                                        (line.Rectangle.Width - line.Rectangle.X) * scaleX, (line.Rectangle.Height - line.Rectangle.Y) * scaleY);

                                    //Create PDF redaction for the found SSN location
                                    PdfRedaction redaction = new PdfRedaction(redactionBound);

                                    //Adds the redaction to loaded page
                                    loadedPage.Redactions.Add(redaction);


                                }
                            }
                        }
                    }
                }

                //Save the OCR processed PDF document in the disk
                lDoc.Save("RedactedPDF.pdf");
                lDoc.Close(true);

                Process.Start("RedactedPDF.pdf");
            }
        }

        private void btnAddVisualElements_Click(object sender, RoutedEventArgs e)
        {
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument("../../Input/RedactPDFwithEmail.pdf"))
            {
                //Get the first page from the document 
                PdfLoadedPage page = loadedDocument.Pages[0] as PdfLoadedPage;

                //Create a PDF redaction for the page 
                PdfRedaction redaction = new PdfRedaction(new RectangleF(341, 149, 64, 14));

                //Draw the patten on the redaction area
                PdfHatchBrush pdfHatchBrush = new PdfHatchBrush(PdfHatchStyle.BackwardDiagonal, Color.Red, Color.Transparent);

                redaction.Appearance.Graphics.DrawRectangle(pdfHatchBrush, new RectangleF(0, 0, 64, 14));

                //Add the redaction to the loaded page
                page.Redactions.Add(redaction);

                //Save the redacted PDF document to disk
                loadedDocument.Save("RedactedPDFWithVisualAppearance.pdf");
                //Close the document instance
                loadedDocument.Close(true);

                Process.Start("RedactedPDFWithVisualAppearance.pdf");
            }

        }

        private void btnTextOnly_Click(object sender, RoutedEventArgs e)
        {
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument("../../Input/RedactPDFwithEmail.pdf"))
            {
                //Get the first page from the document 
                PdfLoadedPage page = loadedDocument.Pages[0] as PdfLoadedPage;

                //Create a PDF redaction for the page 
                PdfRedaction redaction = new PdfRedaction(new RectangleF(343, 280, 100, 16));

                //Set text only redaction
                redaction.TextOnly = true;

                //Add the redaction to the loaded page
                page.Redactions.Add(redaction);

                //Save the redacted PDF document to disk
                loadedDocument.Save("RedactedPDFTextOnly.pdf");
                //Close the document instance
                loadedDocument.Close(true);

                Process.Start("RedactedPDFTextOnly.pdf");
            }
        }

        private void btnFindAndRedact_Click(object sender, RoutedEventArgs e)
        {
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument("../../Input/RedactPDFwithEmail.pdf"))
            {
                //Get the first page from the document 
                PdfLoadedPage page = loadedDocument.Pages[0] as PdfLoadedPage;

                //Extract text from the page
                page.ExtractText(out TextLines textLines);

                if (textLines != null && textLines.Count > 0)
                {
                    //Define regular expression pattern to search for dates in the format MM/DD/YYYY
                    string datePattern = @"\b\d{1,2}\/\d{1,2}\/\d{4}\b";

                    //Find the text to redact
                    foreach (TextLine line in textLines)
                    {
                        foreach (TextWord word in line.WordCollection)
                        {
                            //Match the text against the date pattern
                            MatchCollection dateMatches = Regex.Matches(word.Text, datePattern);
                            //Add redaction if the match found
                            foreach (Match dateMatch in dateMatches)
                            {
                                string textToFindAndRedact = dateMatch.Value;
                                if (textToFindAndRedact == word.Text)
                                {
                                    //Create a redaction object.
                                    PdfRedaction redaction = new PdfRedaction(word.Bounds, Color.Black);
                                    //Add a redaction object into the redaction collection of loaded page.
                                    page.AddRedaction(redaction);
                                }
                            }
                        }

                    }
                }
                //Save the redacted PDF document to disk
                loadedDocument.Save("RedactedPDFRegex.pdf");
                //Close the document instance
                loadedDocument.Close(true);

                Process.Start("RedactedPDFRegex.pdf");
            }
        }

        private void btnRedactionProgress_Click(object sender, RoutedEventArgs e)
        {
            //Load the PDF document for redaction
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument("../../Input/input.pdf"))
            {
                ConsolePopup consolePopup = new ConsolePopup();
                consolePopup.Show();

                //Set the redaction progress handler to track the progress of redaction
                loadedDocument.RedactionProgress += (redactSender, args) =>
                {
                    //Display the progress in the console
                    consolePopup.WriteLine($"Redaction Progress: {args.Progress}%");
                };

                //Iterate through each page in the document
                foreach (PdfLoadedPage page in loadedDocument.Pages)
                {
                    //Create a redaction area for the page
                    PdfRedaction redaction = new PdfRedaction(new RectangleF(120, 200, 140, 20));
                    redaction.Appearance.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(0, 0, 140, 20));
                    //Add the redaction to the loaded page
                    page.Redactions.Add(redaction);
                }
                //Save the redacted PDF document to disk
                loadedDocument.Save("RedactedPDFWithProgress.pdf");
                //Close the document instance
                loadedDocument.Close(true);
                //Open the redacted PDF document
                Process.Start("RedactedPDFWithProgress.pdf");
            }
        }
    }
}

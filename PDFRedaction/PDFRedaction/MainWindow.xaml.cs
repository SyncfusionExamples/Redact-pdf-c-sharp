using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Exporting;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;

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

        private void btnRedactPDFwithCodes_Click(object sender, RoutedEventArgs     e)
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
            using (OCRProcessor processor = new OCRProcessor(@"../../TesseractBinaries/3.02"))
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
    }
}

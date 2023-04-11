# Easy ways to Redact PDFs using C# 

Redacting a PDF is the process of permanently removing sensitive or confidential information from PDF documents. The [Syncfusion .NET PDF library](https://www.syncfusion.com/pdf-framework/net/pdf-library) provides an easy way to redact PDF documents. This repository contains the example to redact PDF document in below ways using C#. 
* Redact PDF without color
* Redact PDF with fill color 
* Redact PDF with redaction code 
* Redact from PDF image 

Sample name | Description
--- | ---
PDFRedaction | Sample to redact PDF document in multiple ways. 

## Redact PDF without color 

Here we load the PDF document and just remove the email address from it (leave the area blank). You can find the code example in the method named *btnRedactPDFwithoutColor* in [MainWindow.xaml.cs]() file.

```csharp

//Load a PDF document for redaction
PdfLoadedDocument ldoc = new PdfLoadedDocument("../../Input/RedactPDF.pdf");
//Get first page from document
PdfLoadedPage lpage = ldoc.Pages[0] as PdfLoadedPage;
//Create PDF redaction for the page
PdfRedaction redaction = new PdfRedaction(new RectangleF(340,120,140,20));
//Adds the redaction to loaded 1st page
lpage.Redactions.Add(redaction);
//Save the redacted PDF document to disk
ldoc.Save("RedactedPDF.pdf");
//Close the document instance
ldoc.Close(true);

``` 

As you can see in the screenshot, the email address in the PDF file is completely removed without any trace and you cannot find or select the redacted content.

![Redact without color](Screenshots/RedactPDFWithNoColor.png)

## Redact PDF with fill color

Now we redact the PDF document with fill color. This will completely remove the content from the PDF and apply red color over the redacted area. You can find the code example in the method named *btnRedactPDFwithColor_Click* in [MainWindow.xaml.cs]() file.

```csharp

//Create PDF redaction for the page
PdfRedaction redaction = new PdfRedaction(new RectangleF(340,120,140,20), System.Drawing.Color.Red);

//Adds the redaction to loaded page
lpage.Redactions.Add(redaction);

```

![Redact with color](Screenshots/RedactedPDFWithColor.png)

## Redact PDF with code sets and entries

Certain PDF files, such as invoice, government official forms, contains text or images that are positioned at the fixed position in the PDF page. For example, employee addresses in W-4 tax forms will always be in the same place and can be redacted under the exemption code of US FOIA (b) (6). You can find the code example in the method named *btnRedactPDFwithCodes_Click* in [MainWindow.xaml.cs]() file.   

```csharp

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

```

![Redact with color](Screenshots/RedactedPDFWithCodeSet.png)

## Redact image in PDF - OCR

Sometimes, we may have social security numbers (SSN), employee identification numbers, addresses, email IDs, in a scanned PDF file. In those cases, it is very hard to search manually for a specific pattern to redact it. Syncfusion offers an efficient way to find sensitive information in a PDF image using [OCR library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library/ocr-process) and redact it from the PDF file. 

To do this, copy the Tesseract binaries and language data from the NuGet package location to your application and refer the path to your OCR processor. You can find the code example in the method named *btnRedactPDFImage_Click* in [MainWindow.xaml.cs]() file.   

```csharp

//Initialize the OCR processor
using (OCRProcessor processor = new OCRProcessor(@"../../TesseractBinaries/3.02/"))
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

    //Save the redacted PDF document in the disk
    lDoc.Save("RedactedPDF.pdf");
    lDoc.Close(true);

    Process.Start("RedactedPDF.pdf");
}

```

![Redact with color](Screenshots/AfterRedactingPDF.png)

# How to run the examples
* Download this project to a location in your disk. 
* Open the solution file using Visual Studio. 
* Rebuild the solution to install the required NuGet package. 
* Run the application.

# Resources
*   **Product page:** [Syncfusion PDF Framework](https://www.syncfusion.com/document-processing/pdf-framework/net)
*   **Documentation page:** [Syncfusion .NET PDF library](https://help.syncfusion.com/file-formats/pdf/overview)
*   **Online demo:** [Syncfusion .NET PDF library - Online demos](https://ej2.syncfusion.com/aspnetcore/PDF/CompressExistingPDF#/bootstrap5)
*   **Blog:** [Syncfusion .NET PDF library - Blog](https://www.syncfusion.com/blogs/category/pdf)
*   **Knowledge Base:** [Syncfusion .NET PDF library - Knowledge Base](https://www.syncfusion.com/kb/windowsforms/pdf)
*   **EBooks:** [Syncfusion .NET PDF library - EBooks](https://www.syncfusion.com/succinctly-free-ebooks)
*   **FAQ:** [Syncfusion .NET PDF library - FAQ](https://www.syncfusion.com/faq/)

# Support and feedback
*   For any other queries, reach our [Syncfusion support team](https://www.syncfusion.com/support/directtrac/incidents/newincident?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) or post the queries through the [community forums](https://www.syncfusion.com/forums?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).
*   Request new feature through [Syncfusion feedback portal](https://www.syncfusion.com/feedback?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).

# License
This is a commercial product and requires a paid license for possession or use. Syncfusion’s licensed software, including this component, is subject to the terms and conditions of [Syncfusion's EULA](https://www.syncfusion.com/eula/es/?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples). You can purchase a licnense [here](https://www.syncfusion.com/sales/products?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) or start a free 30-day trial [here](https://www.syncfusion.com/account/manage-trials/start-trials?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).

# About Syncfusion
Founded in 2001 and headquartered in Research Triangle Park, N.C., Syncfusion has more than 26,000+ customers and more than 1 million users, including large financial institutions, Fortune 500 companies, and global IT consultancies.

Today, we provide 1600+ components and frameworks for web ([Blazor](https://www.syncfusion.com/blazor-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET Core](https://www.syncfusion.com/aspnet-core-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET MVC](https://www.syncfusion.com/aspnet-mvc-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET WebForms](https://www.syncfusion.com/jquery/aspnet-webforms-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Angular](https://www.syncfusion.com/angular-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [React](https://www.syncfusion.com/react-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Vue](https://www.syncfusion.com/vue-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), and [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)), mobile ([Xamarin](https://www.syncfusion.com/xamarin-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), and [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)), and desktop development ([WinForms](https://www.syncfusion.com/winforms-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [WPF](https://www.syncfusion.com/wpf-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [WinUI(Preview)](https://www.syncfusion.com/winui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) and [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)). We provide ready-to-deploy enterprise software for dashboards, reports, data integration, and big data processing. Many customers have saved millions in licensing fees by deploying our software.

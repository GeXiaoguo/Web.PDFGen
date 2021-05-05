using PuppeteerSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PDFGen
{
    public static class PDFStreamConverter
    {
        public static string ExtractChromium()
        {
            var runningDirectory = Environment.CurrentDirectory;
            var targetFolder = Path.Combine(runningDirectory, ".local-chromium");
            var zipFileSource = Path.Combine(runningDirectory, "chromium.zip");
            if (!Directory.Exists(targetFolder))
            {
                ZipFile.ExtractToDirectory(zipFileSource, targetFolder);
            }
            return targetFolder;
        }
        public static async Task<Stream> ConvertToPDF(this string html)
        {
            var chromiumPath = ExtractChromium();
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromiumPath,
            };

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(html);
                var printOptions = new PdfOptions
                {
                    Scale = 0.98m,
                    PrintBackground = true,
                    PreferCSSPageSize = true,
                };
                var stream = await page.PdfStreamAsync(printOptions);
                return stream;
            }
        }
    }
}

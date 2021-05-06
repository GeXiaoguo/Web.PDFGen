using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace PDFGen
{
    public static class PDFStreamConverter
    {
        public static async Task<Stream> ConvertToPDF(this string html)
        {
            var launchOptions = new LaunchOptions { Headless = true };

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

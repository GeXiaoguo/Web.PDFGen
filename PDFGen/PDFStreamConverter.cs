using PuppeteerSharp;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PDFGen
{
    public static class PDFStreamConverter
    {
        public static async Task<Stream> ConvertToPDF(this string html)
        {
            // Puppeteer by default using the current directory of the running process to search for Chrome.exe
            // This does not work for WindowsServices(curDir set to system32)
            // This does not work for running the app with Visual Studio(curDir set to the project folder)
            var runningDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var launchOptions = new LaunchOptions 
            {
                Headless = true,
                ExecutablePath =Path.Combine(runningDir, @".local-chromium\Win64-706915\chrome-win\Chrome.exe")
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

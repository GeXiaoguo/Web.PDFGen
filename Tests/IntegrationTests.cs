using NUnit.Framework;
using System.IO;
using System.Net.Http;

namespace Tests
{
    public class Tests
    {
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task ConsumingAPI_HtmlAsPDAsync()
        {
            var htmlText = @"<!DOCTYPE html>
<html>
<body>
<p>This text is normal.</p>
<p><b>This text is bold.</b></p>
</body>
</html>";

            var client = new HttpClient();
            var content = new StringContent(htmlText, System.Text.Encoding.UTF8, "text/html") ;
            var response = await client.PostAsync("http://localhost:5000/api/HtmlToPDF", content);

            using (var fs = new FileStream(@"C:\temp\test.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                stream.Seek(0, SeekOrigin.Begin);
                CopyStream(stream, fs);
            }
            Assert.Pass();
        }
    }
}
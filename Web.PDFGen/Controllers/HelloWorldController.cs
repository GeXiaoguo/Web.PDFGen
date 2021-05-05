using Microsoft.AspNetCore.Mvc;
using PDFGen;
using System.IO;
using System.Text;

namespace Web.PDFGen.Controllers
{
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        [Route("helloworld")]
        public string HelloWorld() =>  "Hello World";

        [HttpPost]
        [Route("api/PrintAsPDF")]
        public async System.Threading.Tasks.Task<IActionResult> PrintAsPDF([FromBody] string htmlText)
        {
            var stream = await PDFStreamConverter.ConvertToPDF(htmlText);
            return new FileStreamResult(stream, "application/pdf");
        }


        [HttpPost]
        [Route("api/HtmlToPDF")]
        public async System.Threading.Tasks.Task<IActionResult> HtmlToPDF()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var htmlText = await reader.ReadToEndAsync();
                var stream = await PDFStreamConverter.ConvertToPDF(htmlText);
                return new FileStreamResult(stream, "application/pdf");
            }
        }
    }
}

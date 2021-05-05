# ASP.NET Core + Windows Service Hosting + PDF printing

## Use raw HostBuilder instead of the default HostBuilder

Prefer `new HostBuilder()` instead of `Host.CreateDefaultBuilder` because it makes all settings exeplicit and the logger provided by `Host.CreateDefaultBuilder` is not suitable for production use.

## Specify the app configuration
```
.ConfigureAppConfiguration((hostContext, configBuilder) =>
{
    configBuilder
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
        .Build();
})
```

## Configure the Web host
```
.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<Startup>();
})
```

## Configure the WidnowsService
```
.ConfigureServices((hostContext, services) => // allow hosting windows services
{
    //services.AddHostedService<>();
})
```

## Add an api end points
The url of the api is to be configured on spot for the function rather than allow the implilict default url
```
[HttpGet]
[Route("helloworld")]
public string HelloWorld() =>  "Hello World";
```

## Add an api end points which takes text input and returns FileResult
```
[HttpPost]
[Route("api/converttopdf")]
public async System.Threading.Tasks.Task<IActionResult> PrintAsPDF([FromBody] string htmlText)
{
    var stream = await PDFStreamConverter.ConvertToPDF(htmlText);
    return new FileStreamResult(stream, "application/zip");
}
```

## Specify the http+https port
`launchsettings.json` is not to be used. Use `UserUrls("http://localhost:5100")` instead

because:
 - `launchsettings.json` is only used on the local development machine
 - `launchsettings.json` is not deployed

## Test 
Run the console app from command line `Web.PDFGen.exe`
get from `http://localhost:5100/helloworld`
post to application/json text to `http://localhost:5100/api/converttopdf`

## Allow posting plain html
```
 [HttpPost]
[Route("api/HtmlToPDF")]
public async System.Threading.Tasks.Task<IActionResult> HtmlToPDF()
{
    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
    {
        var htmlText = await reader.ReadToEndAsync();
        var stream = await PDFStreamConverter.ConvertToPDF(htmlText);
        return new FileStreamResult(stream, "application/zip");
    }
}
```
html text can be posted to the api as text/html intead of applicaiton/json

## Failed to launch Chromium when debuging with Visual Studio
Depending how the app is run the CurrentDirectory of the running process could be different.
https://github.com/dotnet/project-system/issues/5053
When running as a WebApp with VisualStudio the current directory is the project directory. When running as a console app with Visual Studio, the current directory is the bin folder.

This creates an interesting problem for self hosting application. It is both a WebApp and a Console app.

The problem is with Visual Studio. Setting the current directory to the project directory is a shortcut to save the effort of copying the content files to the bin folder. But this breaks all the libraries that has it own content. Our PDFGen library is an example. It carries its own conent(Chromium)

## Consuming the API with HttpClient
```
var htmlText = @"<!DOCTYPE html>
<html>
<body>
<p>This text is normal.</p>
<p><b>This text is bold.</b></p>
</body>
</html>";

var client = new HttpClient();
var content = new StringContent(htmlText, System.Text.Encoding.UTF8, "text/html") ;
var result = await client.PostAsync("http://localhost:5000/api/HtmlToPDF", content);
```

## Get the stream from HttpResponse
`var stream = await response.Content.ReadAsStreamAsync();`

## Write the stream to a file
```
public static void CopyStream(Stream input, Stream output)
{
    byte[] buffer = new byte[8 * 1024];
    int len;
    while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
    {
        output.Write(buffer, 0, len);
    }
}

stream.Seek(0, SeekOrigin.Begin);
CopyStream(stream, fs);

```

## Publishing
publish all files to a folder
`dotnet publish the_csproj_file -pPublishProfile=the_folderprofile.pubxml`

## Packaging
Create a nuget pakcage out of files in the published folder
`nuget.exe pack -Version 1.0.0 the_published_folder -OutputDirectory output_folder`
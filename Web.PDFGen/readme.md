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
add reference to Windows Service host
```
<PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="3.1.4" />
```
use the windowds serivce host
```
.UserWindowsService()
```
## Test the Window Service
```
sc create service_name binPath=full_path_to_exe
sc delete service_name
sc query  service_name
sc start  service_name
sc stop   service_name_
```
## Configure controllers and exception handling
```
services.AddControllers(optoins=>optoins.Filters.Add(typeof(UnhandledExceptionEventHandler)));
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

get from `http://localhost:5100/HeartBeat`

post to application/json text to `http://localhost:5100/api/HtmlToPDF`

## Allow posting plain html
`test/html` is not support by default by ASP.Net Core prjoects. This means `[FromBody] string htmlText` would not work. Instead of adding plain text support to the project, we can also read the raw text from the `Request.Body`
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
Depending how the app is run, the CurrentDirectory of the running process could be different according to https://github.com/dotnet/project-system/issues/5053

The CurrentDirectory of the running process is set to the project directory when running as a WebApp with VisualStudio, and the bin folder when running as a console app. This creates an interesting problem for self hosted ASP.NET apps, because then are both WebApp and Console apps at the same time.

Visual Studio does this as shortcut for saving the effort of copying the content files to the bin folder. This breaks all the libraries that carry content. Our PDFGen library is an example. It carries Chromium as content.

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
var stream = await response.Content.ReadAsStreamAsync();
```

## Publish
To make the exe run without dependency to .net core runtime, it needs to be published as a self contained exe. First time trying to publish from Visual Studio will trigger creating of the publishing profile settings(`FolderProfile.pubxml`). Select `Self-Contained` deployment mode the published files fill contain the .net runtime.

From the Web.PDFGen Asp.net project foler, run `dotnet publish -p:PublishProfile=Properties\PublishProfiles\FolderProfile.pubxml`

## Packaging
To allow nuget.exe to pack every thing in the published folder, add this nuspec file to project and copy it to the build target folder
```
  <files>
    <file src="**\*.*" target="content" />
  </files>
```

In the Web.PDFGen project folder, run `c:\tools\nuget.exe pack  xxxPusblishFolder\Web.PDFGen.nuspec`
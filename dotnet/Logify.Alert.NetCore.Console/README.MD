# Logify Alert for .NET Core console applications
A console client to report exceptions to [Logify Alert](https://logify.devexpress.com). 

Starting with the package version v1.0.37, the .Net Standard v2.0 target framework is only supported.

## Install <a href="https://www.nuget.org/packages/Logify.Alert.Console/"><img alt="Nuget Version" src="https://img.shields.io/nuget/v/Logify.Alert.Console.svg" data-canonical-src="https://img.shields.io/nuget/v/Logify.Alert.Console.svg" style="max-width:100%;" /></a>
```sh
$ Install-Package Logify.Alert.Console
```

## Quick Start

### Automatic error reporting
Add the following code at the beginning of the **Main()** method in the **Program.cs** file:
```csharp
using DevExpress.Logify.Console;
// ...

LogifyAlert client = LogifyAlert.Instance;
client.ApiKey = "SPECIFY_YOUR_API_KEY_HERE";
client.StartExceptionsHandling();
```
That's it. Now, your application will report unhandled exceptions to the Logify Alert service. To manage and view generated reports, use the [Logify Alert](https://logify.devexpress.com) link.

### Manual error reporting
```csharp
using DevExpress.Logify.Console;
// ...

try {
    LogifyAlert.Instance.ApiKey = "SPECIFY_YOUR_API_KEY_HERE";
    RunYourCode();
}
catch (Exception e) {
    LogifyAlert.Instance.Send(e);
}
```

## Configuration
You can set up the Logify Alert client using your own config file in JSON format.

1. Create a new JSON file in your project (for example, *appsettings.json*).

2. Add the Logify Alert configuration settings to this file:

```json
{
  "LogifyAlert": {
    "apiKey": "SPECIFY_YOUR_API_KEY_HERE",
    "appName": "Your application name",
    "appVersion": "1.0.2",
    "customData": {
      "MACHINE_NAME": "My Server"
    }
  }
}
```
3. Set the configuration file's **Copy to Output Directory** property to **Copy Always**.

4. Install the following NuGet packages:
* Microsoft.Extensions.Configuration
* Microsoft.Extensions.Configuration.FileExtensions
* Microsoft.Extensions.Configuration.Json

5. Add the following code at the beginning of the **Main()** method in the **Program.cs** file:
```csharp
using System.IO;
using Microsoft.Extensions.Configuration;
using DevExpress.Logify.Console;
// ...

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

IConfigurationRoot configuration = builder.Build();
LogifyAlert client = LogifyAlert.Instance;
client.Configure(configuration.GetSection("LogifyAlert"));
```


## API
### Properties
#### ApiKey
String. Specifies an [API Key](https://logify.devexpress.com/Alert/Documentation/CreateApp) used to register the applications within the Logify service.
```csharp
client.ApiKey = "My Api Key";
```

#### AppName
String. Specifies the application name.
```csharp
client.AppName = "My Application";
```
#### AppVersion
String. Specifies the application version.
```csharp
client.AppVersion = "1.0.2";
```

#### CollectMiniDump
Boolean. Specifies whether automatic minidumps collecting is enabled. The default value is **false**.
```csharp
client.CollectMiniDump = true;  
```

#### CustomData
IDictionary<String, String>. Gets the collection of custom data sent with generated reports.
Use the **CustomData** property to attach additional information to the generated report. For instance, you can use this property to track additional metrics that are important in terms of your application: CPU usage, environment parameters, and so on. The field name can only consists of a-z, A-Z, 0-9, and _ characters.

```csharp
client.CustomData["CustomerName"] = "Mary";
```

#### Tags
IDictionary<String, String>. Gets the collection of tags specifying additional fields from a raw report, which will be used in auto ignoring, filtering or detecting duplicates. A key is a tag name (a string that consists of a-z, A-Z, 0-9, and _ characters), and a value is a tag value that is saved to a report. A new tag is added with **Allow search** enabled.

```csharp
client.Tags["OS"] = "Win8";
```

#### Instance
Singleton. Returns the single instance of the LogifyAlert class.
```csharp
LogifyAlert client = LogifyAlert.Instance;
```

#### OfflineReportsCount
Integer. Specifies the number of last reports to be kept once an Internet connection is lost. Reports are only saved when the *OfflineReportsEnabled* property is set to **true**.
```csharp
client.OfflineReportsEnabled = true;
client.OfflineReportsCount = 20; // Keeps the last 20 reports
```

#### OfflineReportsDirectory
String. Specifies a directory path that will be used to store reports once an Internet connection is lost. Reports are only saved when the *OfflineReportsEnabled* property is set to **true**.
```csharp
client.OfflineReportsEnabled = true;
client.OfflineReportsDirectory = "<directory-for-offline-reports>";
```

#### OfflineReportsEnabled
Boolean. Default value is **false**. Specifies whether or not Logify should store the last *OfflineReportsCount* reports once an Internet connection is lost. To send the kept reports once an Internet connection is available, call the *SendOfflineReports* method.
```csharp
client.OfflineReportsEnabled = true;
client.OfflineReportsCount = 20; // Keeps the last 20 reports
client.OfflineReportsDirectory = "<directory-for-offline-reports>";
```

#### ProxyCredentials
ICredentials. Specifies proxy credentials (a user name and a password) to be used by Logify Alert to authenticate within your system proxy server. The use of this property resolves the "*407 Proxy Authentication Required*" proxy error.
```csharp
client.ProxyCredentials = new NetworkCredential("MyProxyUserName", "MyProxyPassword");
```

#### UserId
String. Specifies a unique user identifier that corresponds to the sent report.
```csharp
client.UserId = "user@myapp.com";
```


### Methods for manual reporting
Alternatively, Logify Alert allows you to catch required exceptions manually, generate reports based on caught exceptions and send these reports only. For this purpose, use the methods below.

#### Send(Exception e)
Generates a crash report based on the caught exception and sends this report to the Logify Alert service.
```csharp
try {
    RunCode();
}
catch (Exception e) {
    client.Send(e);
}
```

#### Send(Exception e, IDictionary<String, String> additionalCustomData)
Sends the caught exception with specified custom data to the Logify Alert service.
```csharp
try {
    RunCode();
}
catch (Exception e) {
    var data = new Dictionary<String, String>();
    data["FailedOperation"] = "RunCode";
    client.Send(e, data);
}
```

#### SendOfflineReports
Sends all reports saved in the *OfflineReportsDirectory* folder to the Logify Alert service.

### Events

#### AfterReportException
Occurs after Logify Alert sends a new crash report to the service.

```csharp
LogifyAlert.Instance.AfterReportException += OnAfterReportException;

void OnAfterReportException(object sender, AfterReportExceptionEventArgs e) {
    System.Console.WriteLine("A new crash report has been sent to Logify Alert");
}
```

#### BeforeReportException
Occurs before Logify Alert sends a new crash report to the service.

The **BeforeReportException** event occurs between the [CanReportException](#canreportexception) event and sending a new report to the Logify Alert service. If report send is canceled in the [CanReportException](#canreportexception) event's handler, the report is not sent and the **BeforeReportException** event isn't raised.

Handle the **BeforeReportException event** to add custom data to the sent report. To do this, assign the required data to the [CustomData](#customdata) property.
```csharp
LogifyAlert.Instance.BeforeReportException += OnBeforeReportException;

void OnBeforeReportException(object sender, BeforeReportExceptionEventArgs e) {
    LogifyAlert.Instance.CustomData["LoggedInUser"] = "Mary";
}
```


#### CanReportException
Occurs between generating a new crash report and raising the [BeforeReportException](#beforereportexception) event.

The **CanReportException** event occurs right after a new report is generated and prepared to be sent to the Logify Alert service. Handle the **CanReportException** event to cancel sending the report. To do this, assign **true** to the appropriate CanReportExceptionEventArgs's **Cancel** property. Thus, the generated report is not posted to the service and the [BeforeReportException](#beforereportexception) isn't raised.
```csharp
LogifyAlert.Instance.CanReportException += OnCanReportException;

void OnCanReportException(object sender, CanReportExceptionEventArgs args) {
    if (args.Exception is MyCustomException)
        args.Cancel = true;
}
```

### Attributes
#### LogifyIgnoreAttribute
Indicates that exceptions thrown from a specific method should not be handled and sent to Logify Alert.

```csharp
[LogifyIgnore(true)]
void ThisMethodShouldNotReportAnyExceptions() {
    RunSomeCode();
}
```

## Custom Clients
If the described client is not suitable for you, you can create a custom one. For more information, refer to the [Custom Clients](https://github.com/DevExpress/Logify.Alert.Clients/blob/develop/CustomClients.md) document.

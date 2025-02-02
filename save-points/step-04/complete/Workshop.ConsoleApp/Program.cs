using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

using OpenAI;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Workshop.ConsoleApp.Services;

var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                 .Build();

var dashboardEndpoint = config["Aspire:Dashboard:Endpoint"]!;
var resourceBuilder = ResourceBuilder.CreateDefault()
                                     .AddService("SKOpenTelemetry");

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
                             .SetResourceBuilder(resourceBuilder)
                             .AddSource("Microsoft.SemanticKernel*")
                             .AddConsoleExporter()
                             .AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint))
                             .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
                             .SetResourceBuilder(resourceBuilder)
                             .AddMeter("Microsoft.SemanticKernel*")
                             .AddConsoleExporter()
                             .AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint))
                             .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    // Add OpenTelemetry as a logging provider
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddConsoleExporter();
        options.AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint));
        // Format log messages. This is default to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});

var builder = Kernel.CreateBuilder();
builder.Services.AddSingleton(loggerFactory);
if (string.IsNullOrWhiteSpace(config["Azure:OpenAI:Endpoint"]!) == false)
{
    var client = new AzureOpenAIClient(
        new Uri(config["Azure:OpenAI:Endpoint"]!),
        new AzureKeyCredential(config["Azure:OpenAI:ApiKey"]!));

    builder.AddAzureOpenAIChatCompletion(
                deploymentName: config["Azure:OpenAI:DeploymentNames:ChatCompletion"]!,
                azureOpenAIClient: client);
}
else
{
    var client = new OpenAIClient(
        credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
        options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });

    builder.AddOpenAIChatCompletion(
                modelId: config["GitHub:Models:ModelIds:ChatCompletion"]!,
                openAIClient: client);
}
var kernel = builder.Build();

var service = new TextSearchService(config);
var collection = await service.GetVectorStoreRecordCollectionAsync("records");
var search = await service.GetVectorStoreTextSearchAsync(collection);

var plugin = search.CreateWithGetTextSearchResults("SearchPlugin");
kernel.Plugins.Add(plugin);

var promptTemplate = """
    {{#with (SearchPlugin-GetTextSearchResults query)}}  
        {{#each this}}  
        Name: {{Name}}
        Value: {{Value}}
        Link: {{Link}}
        -----------------
        {{/each}}  
    {{/with}}  

    {{query}}

    Include citations to the relevant information where it is referenced in the response.
    """;

var settings = new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

var input = default(string);
var message = default(string);
while (true)
{
    Console.WriteLine("Ask a question about semantic kernel.");
    Console.Write("User: ");
    input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    Console.Write("Assistant: ");

    var searchResponse = await search.GetTextSearchResultsAsync(input, new TextSearchOptions() { Top = 2, Skip = 0 });
    Console.WriteLine("\n--- Text Search Results ---\n");
    await foreach (var result in searchResponse.Results)
    {
        Console.WriteLine($"Name:  {result.Name}");
        Console.WriteLine($"Value: {result.Value}");
        Console.WriteLine($"Link:  {result.Link}");
        Console.WriteLine();
    }

    var promptArguments = new KernelArguments(settings)
    {
        { "query", input }
    };

    var promptResponse = kernel.InvokePromptStreamingAsync(
        promptTemplate: promptTemplate,
        arguments: promptArguments,
        templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
        promptTemplateFactory: new HandlebarsPromptTemplateFactory());

    Console.WriteLine("\n--- Text Search Results from Chat Completions ---\n");
    await foreach (var content in promptResponse)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();

    var functionCallingArguments = new KernelArguments(settings);

    var functionCalingResponse = kernel.InvokePromptStreamingAsync(
        promptTemplate: input,
        arguments: functionCallingArguments);

    Console.WriteLine("\n--- Text Search Results from Chat Completions with Auto Function Calling ---\n");
    await foreach (var content in functionCalingResponse)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();

    Console.WriteLine();
}

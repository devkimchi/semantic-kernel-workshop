using System.ClientModel;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

using OpenAI;

using Workshop.ConsoleApp;

var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                 .Build();

var client = new OpenAIClient(
    credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
    options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });

var kernel = Kernel.CreateBuilder()
                   .AddOpenAIChatCompletion(
                        modelId: config["GitHub:Models:ModelId"]!,
                        openAIClient: client)
                   .Build();

// Uncomment each line to run a plugin
await PluginAction.InvokeInlinePromptAsync(kernel);
// await PluginAction.InvokeImportedPromptAsync(kernel);
// await PluginAction.InvokeTrainBookingPluginAsync(kernel);
// await PluginAction.InvokeWeatherPluginAsync(kernel);

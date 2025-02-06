using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

using OpenAI;

var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                 .Build();

var builder = Kernel.CreateBuilder();
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

var input = default(string);
var message = default(string);
while (true)
{
    Console.Write("User: ");
    input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    Console.Write("Assistant: ");

    var response = kernel.InvokePromptStreamingAsync(input);
    await foreach (var content in response)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();

    Console.WriteLine();
}

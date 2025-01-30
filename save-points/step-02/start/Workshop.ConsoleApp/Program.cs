using System.ClientModel;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

using OpenAI;

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

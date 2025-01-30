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
                   .AddGoogleAIGeminiChatCompletion(
                        modelId: config["Google:Gemini:ModelName"]!,
                        apiKey: config["Google:Gemini:ApiKey"]!,
                        serviceId: "google")
                   .AddOpenAIChatCompletion(
                        modelId: config["GitHub:Models:ModelIds:0"]!,
                        openAIClient: client,
                        serviceId: "github")
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

    Console.WriteLine();
    Console.WriteLine("--- Response from Google Gemini ---");
    var responseGoogle = kernel.InvokePromptStreamingAsync(
            promptTemplate: input,
            arguments: new KernelArguments(new PromptExecutionSettings() { ServiceId = "google" }));
    await foreach (var content in responseGoogle)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();

    Console.WriteLine();
    Console.WriteLine("--- Response from GitHub Models ---");
    var responseGH = kernel.InvokePromptStreamingAsync(
            promptTemplate: input,
            arguments: new KernelArguments(new PromptExecutionSettings() { ServiceId = "github" }));
    await foreach (var content in responseGH)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();

    Console.WriteLine();
}

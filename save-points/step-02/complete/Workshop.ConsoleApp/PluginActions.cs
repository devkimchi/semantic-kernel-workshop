using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using Workshop.ConsoleApp.Plugins.BookingAgent;
using Workshop.ConsoleApp.Plugins.WeatherBot;

namespace Workshop.ConsoleApp;

public static class PluginActions
{
    public static async Task InvokeInlinePromptAsync(Kernel kernel)
    {
        var background = "I really enjoy food and outdoor activities.";
        var prompt = """
            You are a helpful travel guide. 
            I'm visiting {{$city}}. {{$background}}. What are some activities I should do today?
            """;

        var input = default(string);
        var message = default(string);
        while (true)
        {
            Console.WriteLine("Tell me about a city you are visiting.");
            Console.Write("User: ");
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            Console.Write("Assistant: ");

            var function = kernel.CreateFunctionFromPrompt(prompt);
            var arguments = new KernelArguments()
            {
                { "city", input },
                { "background", background }
            };

            var response = kernel.InvokeStreamingAsync(function, arguments);
            await foreach (var content in response)
            {
                await Task.Delay(20);
                message += content;
                Console.Write(content);
            }
            Console.WriteLine();

            Console.WriteLine();
        }
    }

    public static async Task InvokeImportedPromptAsync(Kernel kernel)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins");

        var background = "I really enjoy food and outdoor activities.";
        var plugins = kernel.CreatePluginFromPromptDirectory(path);

        var input = default(string);
        var message = default(string);
        while (true)
        {
            Console.WriteLine("Tell me about a city you are visiting.");
            Console.Write("User: ");
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            Console.Write("Assistant: ");

            // var function = kernel.CreateFunctionFromPrompt(prompt);
            var arguments = new KernelArguments()
            {
                { "city", input },
                { "background", background }
            };

            // var response = kernel.InvokeStreamingAsync(function, arguments);
            var response = kernel.InvokeStreamingAsync(plugins["TravelAgent"], arguments);
            await foreach (var content in response)
            {
                await Task.Delay(20);
                message += content;
                Console.Write(content);
            }
            Console.WriteLine();

            Console.WriteLine();
        }
    }

    public static async Task InvokeTrainBookingPluginAsync(Kernel kernel)
    {
        kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");

        var settings = new PromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var history = new ChatHistory();
        history.AddSystemMessage("The year is 2025 and the current month is February.");

        var service = kernel.GetRequiredService<IChatCompletionService>();

        Console.WriteLine("I'm a train booking assistant. How can I help you today?");
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

            history.AddUserMessage(input);
            var response = service.GetStreamingChatMessageContentsAsync(history, settings, kernel);
            await foreach (var content in response)
            {
                await Task.Delay(20);
                message += content;
                Console.Write(content);
            }
            history.AddAssistantMessage(message!);
            Console.WriteLine();

            Console.WriteLine();
        }
    }

    public static async Task InvokeWeatherPluginAsync(Kernel kernel)
    {
        kernel.Plugins.AddFromType<WeatherPlugin>("WeatherBot");

        var settings = new PromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var history = new ChatHistory();
        history.AddSystemMessage("You're a friendly weather bot.");

        var service = kernel.GetRequiredService<IChatCompletionService>();

        Console.WriteLine("I'm a weather bot. How can I help you today?");
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

            history.AddUserMessage(input);
            var response = service.GetStreamingChatMessageContentsAsync(history, settings, kernel);
            await foreach (var content in response)
            {
                await Task.Delay(20);
                message += content;
                Console.Write(content);
            }
            history.AddAssistantMessage(message!);
            Console.WriteLine();

            Console.WriteLine();
        }
    }
}

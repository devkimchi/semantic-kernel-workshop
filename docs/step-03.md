# STEP 03: Semantic Kernel 에이전트 만들기

이 단계에서는 Semantic Kernel로 에이전트를 만들고 에이전트끼리 작업을 하게 만드는 실습을 합니다.

## 세션 목표

- Semantic Kernel을 활용한 간단한 콘솔 앱을 개발할 수 있습니다.
- Semantic Kernel에 필요한 프롬프트 플러그인을 작성할 수 있습니다.
- Semantic Kernel에 필요한 네이티브 코드 플러그인을 작성할 수 있습니다.
- Semantic Kernel에서 Chat History를 활용해 챗봇을 만들 수 있습니다.
- Semantic Kernel에서 작동하는 AI 에이전트를 만들 수 있습니다.

## 사전 준비 사항

이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 개발 환경을 모두 설정한 상태라고 가정합니다.

## 리포지토리 루트 설정

1. 아래 명령어를 실행시켜 `$REPOSITORY_ROOT` 환경 변수를 설정합니다.

    ```bash
    # Bash/Zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    ```

<a name="시작-프로젝트-복사"></a>
## 시작 프로젝트 복사

이 워크샵을 위해 필요한 시작 프로젝트를 준비해 뒀습니다. 이 프로젝트가 제대로 작동하는지 확인합니다. 시작 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
Workshop
└── Workshop.ConsoleApp
```

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-02`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-03/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-03/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

## 시작 프로젝트 빌드 및 실행

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 생성한 API 액세스 토큰을 콘솔 앱에 등록합니다.

    ```bash
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set GitHub:Models:AccessToken {{GitHub Models Access Token}}
    ```

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 문장이나 입력합니다. 예) `Who are you?` 또는 `하늘은 왜 파랄까?`
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 단일 에이전트 만들기 &ndash; 프롬프트 플러그인

프롬프트 템플릿을 이용해서 에이전트를 만들어 봅니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 콘솔 앱 프로젝트에 에이전트용 패키지 라이브러리를 추가합니다.

    ```bash
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Agents.Core --prerelease
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Yaml
    ```

1. 아래 명령어를 실행시켜 `Workshop.ConsoleApp/Plugins/StoryTellerAgent/StoryTeller.yaml` 파일을 생성합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/StoryTellerAgent && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/StoryTellerAgent/StoryTeller.yaml
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/StoryTellerAgent && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/StoryTellerAgent/StoryTeller.yaml -Force
    ```

1. `Workshop.ConsoleApp/Plugins/StoryTellerAgent/StoryTeller.yaml` 파일을 열어 아래 내용을 입력합니다.

    ```yml
    name: StoryTeller
    template: |
      Tell a story about {{$topic}} that is {{$length}} sentences long.
    template_format: semantic-kernel
    description: A function that generates a story about a topic.
    input_variables:
      - name: topic
        description: The topic of the story.
        is_required: true
      - name: length
        description: The number of sentences in the story.
        is_required: true
    output_variable:
      description: The generated story.
    execution_settings:
      default:
        temperature: 0.6
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `using Microsoft.SemanticKernel;` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    using System.ClientModel;
    
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel;
    
    // 👇👇👇 아래 코드를 입력하세요
    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.ChatCompletion;
    // 👆👆👆 위 코드를 입력하세요
    
    using OpenAI;
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var definition = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "StoryTellerAgent", "StoryTeller.yaml"));
    var template = KernelFunctionYaml.ToPromptTemplateConfig(definition);
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var definition = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "StoryTellerAgent", "StoryTeller.yaml"));
    var template = KernelFunctionYaml.ToPromptTemplateConfig(definition);

    // 👇👇👇 아래 코드를 입력하세요
    var agent = new ChatCompletionAgent(template, new KernelPromptTemplateFactory())
                {
                    Kernel = kernel
                };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var definition = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "StoryTellerAgent", "StoryTeller.yaml"));
    var template = KernelFunctionYaml.ToPromptTemplateConfig(definition);
    var agent = new ChatCompletionAgent(template, new KernelPromptTemplateFactory())
                {
                    Kernel = kernel
                };

    // 👇👇👇 아래 코드를 입력하세요
    var history = new ChatHistory();
    history.AddSystemMessage("You're a very good storyteller agent. Always answer in Korean.");
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("User: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    Console.WriteLine("Hi, I'm your friendly storyteller. What story would you like me to tell you about?");
    // 👆👆👆 위 코드를 입력하세요
    
    Console.Write("User: ");
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 입력하세요
    history.AddUserMessage(input);
    var arguments = new KernelArguments()
    {
        { "topic", input },
        { "length", 3 }
    };
    // 👆👆👆 위 코드를 입력하세요
    ```

   그리고 아래와 같이 코드를 수정합니다.

    ```csharp
    // 👇👇👇 아래 코드를 삭제하세요
    var response = kernel.InvokePromptStreamingAsync(input);
    // 👆👆👆 위 코드를 삭제하세요
    
    // 👇👇👇 아래 코드를 추가하세요
    var response = agent.InvokeStreamingAsync(history, arguments);
    // 👆👆👆 위 코드를 추가하세요

    await foreach (var content in response)
    ```

    그리고 아래 코드를 추가하세요.

    ```csharp
    await foreach (var content in response)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }

    // 👇👇👇 아래 코드를 입력하세요
    history.AddAssistantMessage(message!);
    // 👆👆👆 위 코드를 입력하세요

    Console.WriteLine();
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 주제나 입력합니다. 예) `고양이`, `Trauma Center`
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 다른 주제를 입력해 보거나 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 단일 에이전트 만들기 &ndash; 네이티브 플러그인

네이티브 코드를 이용해서 에이전트를 만들어 봅니다.

1. 앞서 [시작 프로젝트 복사](#시작-프로젝트-복사) 섹션을 따라 새로 `workshop` 디렉토리를 생성합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 콘솔 앱 프로젝트에 에이전트용 패키지 라이브러리를 추가합니다.

    ```bash
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Agents.Core --prerelease
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Yaml
    ```

1. 아래 명령어를 실행시켜 `Workshop.ConsoleApp/Plugins/RestaurantAgent/MenuPlugin.cs` 파일을 생성합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/RestaurantAgent && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/RestaurantAgent/MenuPlugin.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/RestaurantAgent && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/RestaurantAgent/MenuPlugin.cs -Force
    ```

1. `Workshop.ConsoleApp/Plugins/RestaurantAgent/MenuPlugin.cs` 파일을 열어 아래 내용을 입력합니다.

    ```csharp
    using System.ComponentModel;
    
    using Microsoft.SemanticKernel;
    
    namespace Workshop.ConsoleApp.Plugins.RestaurantAgent;
    
    public class MenuPlugin
    {
        [KernelFunction]
        [Description("Provides a list of specials from the menu.")]
        public string GetSpecials() =>
            """
            Special Soup: Clam Chowder
            Special Salad: Cobb Salad
            Special Drink: Chai Tea
            """;
    
        [KernelFunction]
        [Description("Provides the price of the requested menu item.")]
        public string GetItemPrice(
            [Description("The name of the menu item.")]
            string menuItem) =>
            "$9.99";
    }
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `using Microsoft.SemanticKernel;` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    using System.ClientModel;
    
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel;
    
    // 👇👇👇 아래 코드를 입력하세요
    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Workshop.ConsoleApp.Plugins.RestaurantAgent;
    // 👆👆👆 위 코드를 입력하세요
    
    using OpenAI;
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    kernel.Plugins.AddFromType<MenuPlugin>();
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    kernel.Plugins.AddFromType<MenuPlugin>();

    // 👇👇👇 아래 코드를 입력하세요
    var settings = new PromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    kernel.Plugins.AddFromType<MenuPlugin>();
    var settings = new PromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    
    // 👇👇👇 아래 코드를 입력하세요
    var agent = new ChatCompletionAgent()
    {
        Kernel = kernel,
        Arguments = new KernelArguments(settings),
        Instructions = "Answer questions about the menu.",
        Name = "Host",
    };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    kernel.Plugins.AddFromType<MenuPlugin>();
    var settings = new PromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    var agent = new ChatCompletionAgent()
    {
        Kernel = kernel,
        Arguments = new KernelArguments(settings),
        Instructions = "Answer questions about the menu.",
        Name = "Host",
    };

    // 👇👇👇 아래 코드를 입력하세요
    var history = new ChatHistory();
    history.AddSystemMessage("You're a friendly host at a restaurant. Always answer in Korean.");
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("User: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    Console.WriteLine("Hi, I'm your host today. How can I help you today?");
    // 👆👆👆 위 코드를 입력하세요
    
    Console.Write("User: ");
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 입력하세요
    history.AddUserMessage(input);
    // 👆👆👆 위 코드를 입력하세요
    ```

   그리고 아래와 같이 코드를 수정합니다.

    ```csharp
    // 👇👇👇 아래 코드를 삭제하세요
    var response = kernel.InvokePromptStreamingAsync(input);
    // 👆👆👆 위 코드를 삭제하세요
    
    // 👇👇👇 아래 코드를 추가하세요
    var response = agent.InvokeStreamingAsync(history);
    // 👆👆👆 위 코드를 추가하세요

    await foreach (var content in response)
    ```

    그리고 아래 코드를 추가하세요.

    ```csharp
    await foreach (var content in response)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }

    // 👇👇👇 아래 코드를 입력하세요
    history.AddAssistantMessage(message!);
    // 👆👆👆 위 코드를 입력하세요

    Console.WriteLine();
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 스페셜 메뉴 또는 가격을 물어보세요.
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 다중 에이전트 협업하기

여러 개의 에이전트가 협업을 하는 시나리오를 구현해 봅니다.

1. 앞서 [시작 프로젝트 복사](#시작-프로젝트-복사) 섹션을 따라 새로 `workshop` 디렉토리를 생성합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 콘솔 앱 프로젝트에 에이전트용 패키지 라이브러리를 추가합니다.

    ```bash
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Agents.Core --prerelease
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `using Microsoft.SemanticKernel;` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    using System.ClientModel;
    
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel;
    
    // 👇👇👇 아래 코드를 입력하세요
    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.Agents.Chat;
    using Microsoft.SemanticKernel.ChatCompletion;
    // 👆👆👆 위 코드를 입력하세요
    
    using OpenAI;
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var reviewerName = "ProjectManager";
    var reviewerInstructions =
        """
        You are a project manager who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without examples.
        """;
    
    var agentReviewer = new ChatCompletionAgent()
    {
        Name = reviewerName,
        Instructions = reviewerInstructions,
        Kernel = kernel
    };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var agentReviewer = new ChatCompletionAgent()
    {
        ...
    };

    // 👇👇👇 아래 코드를 입력하세요
    var copywriterName = "Copywriter";
    var copywriterInstructions =
        """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        Never delimit the response with quotation marks.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;
    
    var agentWriter = new ChatCompletionAgent()
    {
        Name = copywriterName,
        Instructions = copywriterInstructions,
        Kernel = kernel
    };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var agentWriter = new ChatCompletionAgent()
    {
        ...
    };

    // 👇👇👇 아래 코드를 입력하세요
    var terminationFunction =
        AgentGroupChat.CreatePromptFunctionForStrategy(
            """
            Determine if the copy has been approved. If so, respond with a single word: yes
    
            History:
            {{$history}}
            """,
            safeParameterNames: "history");
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var terminationFunction =
        ...

    // 👇👇👇 아래 코드를 입력하세요
    var selectionFunction =
        AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            Determine which participant takes the next turn in a conversation based on the the most recent participant.
            State only the name of the participant to take the next turn.
            No participant should take more than one turn in a row.
            
            Choose only from these participants:
            - {{{reviewerName}}}
            - {{{copywriterName}}}
            
            Always follow these rules when selecting the next participant:
            - After {{{copywriterName}}}, it is {{{reviewerName}}}'s turn.
            - After {{{reviewerName}}}, it is {{{copywriterName}}}'s turn.
    
            History:
            {{$history}}
            """,
            safeParameterNames: "history");
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var selectionFunction =
        ...
    
    // 👇👇👇 아래 코드를 입력하세요
    var strategyReducer = new ChatHistoryTruncationReducer(1);
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    var strategyReducer = new ChatHistoryTruncationReducer(1);
    
    // 👇👇👇 아래 코드를 입력하세요
    var chat = new AgentGroupChat(agentWriter, agentReviewer)
    {
        ExecutionSettings = new AgentGroupChatSettings()
        {
            SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
            {
                InitialAgent = agentWriter,
                ResultParser = (result) => result.GetValue<string>() ?? copywriterName,
                HistoryVariableName = "history",
                HistoryReducer = strategyReducer,
                EvaluateNameOnly = true,
            },
            TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
            {
                Agents = [ agentReviewer ],
                ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.InvariantCultureIgnoreCase) ?? false,
                HistoryVariableName = "history",
                MaximumIterations = 10,
                HistoryReducer = strategyReducer,
                AutomaticReset = true,
            },
        }
    };
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("User: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    Console.WriteLine("Hi, I'm your project manager today. What product do you have in mind advertising?");
    // 👆👆👆 위 코드를 입력하세요
    
    Console.Write("User: ");
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래와 같이 수정합니다.

    ```csharp
    // 👇👇👇 아래 코드를 삭제하세요
    Console.Write("Assistant: ");

    var response = kernel.InvokePromptStreamingAsync(input);
    // 👆👆👆 위 코드를 삭제하세요
    
    // 👇👇👇 아래 코드를 추가하세요
    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

    var agentName = default(string);
    var isAgentChanged = false;
    var response = chat.InvokeStreamingAsync();
    // 👆👆👆 위 코드를 추가하세요

    await foreach (var content in response)
    ```

    그리고 아래와 같이 `foreach`문을 수정하세요.

    ```csharp
    await foreach (var content in response)
    {
        await Task.Delay(20);

        // 👇👇👇 아래 코드를 삭제하세요
        message += content;
        Console.Write(content);
        // 👆👆👆 위 코드를 삭제하세요

        // 👇👇👇 아래 코드를 추가하세요
        if (content.AuthorName?.Equals(agentName, StringComparison.InvariantCultureIgnoreCase) == false)
        {
            isAgentChanged = true;
            agentName = content.AuthorName;
            Console.WriteLine();
        }
        else
        {
            isAgentChanged = false;
        }

        message += isAgentChanged ? $"{content.AuthorName}: {content.Content}" : content.Content;
        Console.Write(isAgentChanged ? $"{content.AuthorName}: {content.Content}" : content.Content);
        // 👆👆👆 위 코드를 추가하세요
    }
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 광고를 위한 제품, 상품 등을 입력합니다.
1. `ProjectManager: `, `Copywriter: ` 프롬프트가 서로 대화를 하면서 결과를 도출해 내는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-03/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-03`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-03/complete/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-03/complete/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

1. 워크샵 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 생성한 API 액세스 토큰을 콘솔 앱에 등록합니다.

    ```bash
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set GitHub:Models:AccessToken {{GitHub Models Access Token}}
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `// Uncomment each line to run a plugin` 라인을 찾아 아래와 같이 주석을 제거합니다.

    ```csharp
    // Uncomment each line to run a plugin
    
    // 👇👇👇 스토리텔러 에이전트 실행하고 싶으면 주석 제거
    // await AgentActions.InvokeStoryTellerAgentAsync(kernel);
    // 👆👆👆 스토리텔러 에이전트 실행하고 싶으면 주석 제거
    
    // 👇👇👇 식당 호스트 에이전트 실행하고 싶으면 주석 제거
    // await AgentActions.InvokeRestaurantAgentAsync(kernel);
    // 👆👆👆 식당 호스트 에이전트 실행하고 싶으면 주석 제거
    
    // 👇👇👇 에이전트간 협업 과정 실행하고 싶으면 주석 제거
    // await AgentActions.InvokeAgentCollaborationsAsync(kernel);
    // 👆👆👆 에이전트간 협업 과정 실행하고 싶으면 주석 제거
    ```

1. 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 원하는 내용을 입력합니다.
1. `Assistant: ` 프롬프트 혹은 `ProjectManager: `, `Copywriter: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 추가 학습 자료

- [챗 에이전트](https://learn.microsoft.com/semantic-kernel/frameworks/agent/chat-completion-agent)
- [프롬프트 플러그인으로 에이전트 만들기](https://learn.microsoft.com/semantic-kernel/frameworks/agent/agent-templates)
- [네이티브 코드 플러그인으로 에이전트 만들기](https://learn.microsoft.com/semantic-kernel/frameworks/agent/agent-functions)
- [에이전트간 협업하기](https://learn.microsoft.com/semantic-kernel/frameworks/agent/agent-chat)

---

축하합니다! **Semantic Kernel 에이전트 만들기** 실습이 끝났습니다. 이제 [STEP 04: Semantic Kernel RAG 및 모니터링](./step-04.md) 단계로 넘어가세요.

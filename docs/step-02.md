# STEP 02: Semantic Kernel 플러그인 만들기

이 단계에서는 Semantic Kernel에 플러그인을 적용시켜보는 실습을 합니다.

## 세션 목표

- Semantic Kernel을 활용한 간단한 콘솔 앱을 개발할 수 있습니다.
- Semantic Kernel에 필요한 프롬프트 플러그인을 작성할 수 있습니다.
- Semantic Kernel에 필요한 네이티브 코드 플러그인을 작성할 수 있습니다.
- Semantic Kernel에서 Chat History를 활용해 챗봇을 만들 수 있습니다.

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

<a name="시작-프로젝트-복사">
## 시작 프로젝트 복사

이 워크샵을 위해 필요한 시작 프로젝트를 준비해 뒀습니다. 이 프로젝트가 제대로 작동하는지 확인합니다. 시작 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
Workshop
└── Workshop.ConsoleApp
```

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-01`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-02/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-02/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
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
1. `Assistant: ` 프롬프트에 GitHub Models의 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 인라인 프롬프트 플러그인 추가하기

프롬프트 플러그인을 코드 형태로 직접 주입합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var background = "I really enjoy food and outdoor activities.";
    var prompt = """
        You are a helpful travel guide. 
        I'm visiting {{$city}}. {{$background}}. What are some activities I should do today?
        """;
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("User: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    Console.WriteLine("Tell me about a city you are visiting.");
    // 👆👆👆 위 코드를 입력하세요
    
    Console.Write("User: ");
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 입력하세요
    var function = kernel.CreateFunctionFromPrompt(prompt);
    var arguments = new KernelArguments()
    {
        { "city", input },
        { "background", background }
    };
    // 👆👆👆 위 코드를 입력하세요
    ```

   그리고 아래와 같이 코드를 수정합니다.

    ```csharp
    // 👇👇👇 아래 코드를 삭제하세요
    var response = kernel.InvokePromptStreamingAsync(input);
    // 👆👆👆 위 코드를 삭제하세요
    
    // 👇👇👇 아래 코드를 추가하세요
    var response = kernel.InvokeStreamingAsync(function, arguments);
    // 👆👆👆 위 코드를 추가하세요

    await foreach (var content in response)
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 도시 이름을 입력합니다. 예) `Daegu` 또는 `대구`
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 다른 도시 이름을 입력하거나 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 프롬프트 플러그인 외부에서 주입하기

프롬프트 플러그인을 외부에서 읽어들여 주입합니다.

1. 앞서 [시작 프로젝트 복사](#시작-프로젝트-복사) 섹션을 따라 새로 `workshop` 디렉토리를 생성합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 아래 명령어를 실행시켜 `Workshop.ConsoleApp/Plugins/TravelAgent/config.json` 파일과 `Workshop.ConsoleApp/Plugins/TravelAgent/skprompt.txt` 파일을 생성합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent/config.json && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent/skprompt.txt
    ```
    
    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent/config.json -Force && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/TravelAgent/skprompt.txt -Force
    ```

1. `Workshop.ConsoleApp/Plugins/TravelAgent/config.json` 파일을 열어 아래 내용을 입력합니다.

    ```jsonc
    {
      "schema": 1,
      "type": "completion",
      "description": "Recommend various food and outdoor activities in the given city",
      "execution_settings": {
        "default": {
          "max_tokens": 1000,
          "temperature": 0
        }
      },
      "input_variables": [
        {
          "name": "city",
          "description": "Name of the city provided by the user",
          "required": true
        },
        {
          "name": "background",
          "description": "The user's background",
          "required": true
        }
      ]
    }
    ```

1. `Workshop.ConsoleApp/Plugins/TravelAgent/skprompt.txt` 파일을 열어 아래 내용을 입력합니다.

    ```text
    You are a helpful travel guide. 
    I'm visiting {{$city}}. {{$background}}. What are some activities I should do today?
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var background = "I really enjoy food and outdoor activities.";
    var plugins = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins"));
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("User: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    Console.WriteLine("Tell me about a city you are visiting.");
    // 👆👆👆 위 코드를 입력하세요
    
    Console.Write("User: ");
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 입력하세요
    var arguments = new KernelArguments()
    {
        { "city", input },
        { "background", background }
    };
    // 👆👆👆 위 코드를 입력하세요
    ```

   그리고 아래와 같이 코드를 수정합니다.

    ```csharp
    // 👇👇👇 아래 코드를 삭제하세요
    var response = kernel.InvokePromptStreamingAsync(input);
    // 👆👆👆 위 코드를 삭제하세요
    
    // 👇👇👇 아래 코드를 추가하세요
    var response = kernel.InvokeStreamingAsync(plugins["TravelAgent"], arguments);
    // 👆👆👆 위 코드를 추가하세요

    await foreach (var content in response)
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 도시 이름을 입력합니다. 예) `Daegu` 또는 `대구`
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 다른 도시 이름을 입력하거나 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 도전 과제 #1

- 앞서 작성한 `TravelAgent` 플러그인은 현재 영어로 답변을 합니다. 이 플러그인의 답변을 모두 한국어로 바꿔서 출력할 수 있도록 프롬프트를 조정해 보세요.

## 네이티브 코드 플러그인 추가하기

네이티브 코드 기반의 플러그인을 추가합니다. 동시에 채팅 히스토리와 플러그인 자동 호출 기능도 함께 다룹니다.

1. 앞서 [시작 프로젝트 복사](#시작-프로젝트-복사) 섹션을 따라 새로 `workshop` 디렉토리를 생성합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 아래 명령어를 실행시켜 `Workshop.ConsoleApp/Plugins/BookingAgent/trains.json` 파일과 `Workshop.ConsoleApp/Plugins/BookingAgent/TrainBookingPlugin.cs` 파일을 생성합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent/trains.json && \
        touch $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent/TrainBookingPlugin.cs
    ```
    
    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent/trains.json -Force && `
        New-Item -Type File -Path $REPOSITORY_ROOT/workshop/Workshop.ConsoleApp/Plugins/BookingAgent/TrainBookingPlugin.cs -Force
    ```

1. `Workshop.ConsoleApp/Plugins/BookingAgent/trains.json` 파일을 열어 아래 내용을 입력합니다.

    ```jsonc
    [
      {
        "Id": 1,
        "TrainName": "KTX",
        "Destination": "Seoul",
        "DepartureDate": "2025-02-20",
        "Price": 60000,
        "IsBooked": true
      },
      {
        "Id": 2,
        "TrainName": "SRT",
        "Destination": "Suseo",
        "DepartureDate": "2025-02-23",
        "Price": 55000,
        "IsBooked": false
      },
      {
        "Id": 3,
        "TrainName": "ITX",
        "Destination": "Seoul",
        "DepartureDate": "2025-02-25",
        "Price": 45000,
        "IsBooked": false
      }
    ]
    ```

1. `Workshop.ConsoleApp/Plugins/BookingAgent/TrainBookingPlugin.cs` 파일을 열어 아래 내용을 입력합니다.

    ```csharp
    using System.ComponentModel;
    using System.Text.Json;
    
    using Microsoft.SemanticKernel;
    
    namespace Workshop.ConsoleApp.Plugins.BookingAgent;
    
    public class TrainBookingPlugin
    {
        private const string Database = "trains.json";
    
        private static string filepath = Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "BookingAgent", Database);
        private static JsonSerializerOptions options = new() { WriteIndented = true };
    
        private readonly List<TrainModel> _trains;
    
        public TrainBookingPlugin()
        {
            this._trains = this.LoadTrainsFromFile();
        }
    
        [KernelFunction("search_trains")]
        [Description("Searches for available trains based on the destination and departure date in the format YYYY-MM-DD")]
        [return: Description("A list of available trains")]
        public List<TrainModel> SearchTrains(string destination, string departureDate)
        {
            return this._trains.Where(train => train.Destination.Equals(destination, StringComparison.InvariantCultureIgnoreCase) &&
                                               train.DepartureDate.Equals(departureDate))
                               .ToList();
        }
    
        [KernelFunction("book_train")]
        [Description("Books a train based on the train ID provided")]
        [return: Description("Booking confirmation message")]
        public string BookTrain(int trainId)
        {
            var train = this._trains.SingleOrDefault(train => train.Id == trainId);
            if (train == null)
            {
                return "Train not found. Please provide a valid train ID.";
            }
    
            if (train.IsBooked == true)
            {
                return $"You've already booked this train.";
            }
    
            train.IsBooked = true;
            this.SaveTrainsToFile();
            
            return $"Train booked successfully! Train name: {train.TrainName}, Destination: {train.Destination}, Departure: {train.DepartureDate}, Price: ${train.Price}.";
        }
    
        private void SaveTrainsToFile()
        {
            var json = JsonSerializer.Serialize(this._trains, options);
            File.WriteAllText(filepath, json);
        }
    
        private List<TrainModel> LoadTrainsFromFile()
        {
            if (File.Exists(filepath))
            {
                var json = File.ReadAllText(filepath);
                return JsonSerializer.Deserialize<List<TrainModel>>(json)!;
            }
    
            throw new FileNotFoundException($"The file '{Database}' was not found. Please provide a valid {Database} file.");
        }
    }
    
    public class TrainModel
    {
        public int Id { get; set; }
        public required string TrainName { get; set; }
        public required string Destination { get; set; }
        public required string DepartureDate { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; } = false;
    }
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");
    // 👆👆👆 위 코드를 입력하세요

    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");

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
    kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");
    var settings = new PromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    // 👇👇👇 아래 코드를 입력하세요
    var history = new ChatHistory();
    history.AddSystemMessage("The year is 2025 and the current month is February");
    // 👆👆👆 위 코드를 입력하세요

    var input = default(string);
    ```

1. 앞서 입력한 코드에 이어 아래 코드를 입력합니다.

    ```csharp
    kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");
    var settings = new PromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    var history = new ChatHistory();
    history.AddSystemMessage("The year is 2025 and the current month is February.");

    // 👇👇👇 아래 코드를 입력하세요
    var service = kernel.GetRequiredService<IChatCompletionService>();
    
    Console.WriteLine("I'm a train booking assistant. How can I help you today?");
    // 👆👆👆 위 코드를 입력하세요

    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.Write("Assistant: ");` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 입력하세요
    history.AddUserMessage(input);
    var response = service.GetStreamingChatMessageContentsAsync(history, settings, kernel);
    // 👆👆👆 위 코드를 입력하세요

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

1. `User:` 라는 프롬프트가 보이면 "기차 예약" 또는 "book for train"이라고 입력합니다. 비슷한 의미의 다른 표현으로도 입력해 보세요.
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 목적지와 날짜를 입력합니다. `trains.json` 데이터에 입력한 값을 바탕으로 현재 가능한 목적지는 `Seoul`, `Suseo`이고, 날짜는 `2025-02-20`, `2025-02-23`, `2025-02-25`입니다.
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 다른 예약을 해 보거나 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 도전 과제 #2

- `Workshop.ConsoleApp/Plugins/WeatherBot` 디렉토리에 있는 `WeatherPlugin.cs` 파일을 사용해서 특정 도시 &ndash; London, New York, Tokyo, Seoul, Paris, Sydney &dash; 의 날씨를 확인해 봅니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-02/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-02`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-02/complete/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-02/complete/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
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

// 👇👇👇 인라인 프롬프트 실행하고 싶으면 주석 제거
// await PluginAction.InvokeInlinePromptAsync(kernel);
// 👆👆👆 인라인 프롬프트 실행하고 싶으면 주석 제거

// 👇👇👇 외부 프롬프트 실행하고 싶으면 주석 제거
// await PluginAction.InvokeImportedPromptAsync(kernel);
// 👆👆👆 외부 프롬프트 실행하고 싶으면 주석 제거

// 👇👇👇 기차 예약 에이전트 실행하고 싶으면 주석 제거
// await PluginAction.InvokeTrainBookingPluginAsync(kernel);
// 👆👆👆 기차 예약 에이전트 실행하고 싶으면 주석 제거

// 👇👇👇 날씨 봇 실행하고 싶으면 주석 제거
// await PluginAction.InvokeWeatherPluginAsync(kernel);
// 👆👆👆 날씨 봇 실행하고 싶으면 주석 제거
```

1. 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 원하는 내용을 입력합니다.
1. `Assistant: ` 프롬프트에 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 추가 학습 자료

- [프롬프트 템플릿](https://learn.microsoft.com/semantic-kernel/concepts/prompts/prompt-template-syntax)
- [네이티브 플러그인 추가](https://learn.microsoft.com/semantic-kernel/concepts/plugins/adding-native-plugins)
- [챗 히스토리](https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/chat-history)
- [자동 펑션 호출](https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors)

---

축하합니다! **Semantic Kernel 플러그인 만들기** 실습이 끝났습니다. 이제 [STEP 03: Semantic Kernel 에이전트 만들기](./step-03.md) 단계로 넘어가세요.

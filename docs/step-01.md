# STEP 01: Semantic Kernel 기본 작동법

이 단계에서는 Semantic Kernel을 활용해서 다양한 LLM에 연결해 보는 실습을 합니다.

## 세션 목표

- Semantic Kernel에 OpenAI를 비롯한 다양한 LLM을 연결할 수 있습니다.
- Semantic Kernel을 활용해서 프롬프트를 실행시킬 수 있는 간단한 콘솔 앱을 개발할 수 있습니다.


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

## 시작 프로젝트 복사

이 워크샵을 위해 필요한 시작 프로젝트를 준비해 뒀습니다. 이 프로젝트가 제대로 작동하는지 확인합니다. 시작 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
Workshop
└── Workshop.ConsoleApp
```

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-01/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-01/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
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

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 문장이나 입력합니다. 예) `Who are you?` 또는 `하늘은 왜 파랄까?`
1. `Assistant: ` 프롬프트에 입력한 문장이 그대로 출력되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## Semantic Kernel SDK 추가하기

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 콘솔 앱 프로젝트에 Semantic Kernel SDK를 추가합니다.

    ```bash
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel
    ```

## Semantic Kernel에 Google &ndash; Gemini 연결하기

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 콘솔 앱 프로젝트에 Google 커넥터를 추가합니다.

    ```bash
    dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Connectors.Google --prerelease
    ```

1. 이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 생성한 API 액세스 토큰을 콘솔 앱에 등록합니다.

    ```bash
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set Google:Gemini:ApiKey {{Google Gemini API Key}}
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `using Microsoft.Extensions.Configuration;` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    using Microsoft.Extensions.Configuration;
    
    // 👇👇👇 아래 코드를 입력하세요
    using Microsoft.SemanticKernel;
    // 👆👆👆 위 코드를 입력하세요
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `var input = default(string);` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var kernel = Kernel.CreateBuilder()
                       .AddGoogleAIGeminiChatCompletion(
                            modelId: config["Google:Gemini:ModelName"]!,
                            apiKey: config["Google:Gemini:ApiKey"]!,
                            serviceId: "google")
                       .Build();
    // 👆👆👆 위 코드를 입력하세요
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.WriteLine(input);` 라인을 찾아 아래와 같이 수정합니다.

    ```csharp
    Console.Write("Assistant: ");

    // 👇👇👇 아래 코드를 삭제하세요
    Console.WriteLine(input);
    // 👆👆👆 위 코드를 삭제하세요

    // 👇👇👇 아래 코드를 추가하세요
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
    // 👆👆👆 위 코드를 추가하세요

    Console.WriteLine();
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 문장이나 입력합니다. 예) `Who are you?` 또는 `하늘은 왜 파랄까?`
1. `Assistant: ` 프롬프트에 Google Gemini의 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## Semantic Kernel에 GitHub Model &ndash; Azure OpenAI 연결하기

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 생성한 API 액세스 토큰을 콘솔 앱에 등록합니다.

    ```bash
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set GitHub:Models:AccessToken {{GitHub Models Access Token}}
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일을 열고 `using Microsoft.SemanticKernel;` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel;
    
    // 👇👇👇 아래 코드를 입력하세요
    using OpenAI;
    using System.ClientModel;
    // 👆👆👆 위 코드를 입력하세요
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `var kernel = Kernel.CreateBuilder()` 라인을 찾아 아래 코드를 입력합니다.

    ```csharp
    // 👇👇👇 아래 코드를 입력하세요
    var client = new OpenAIClient(
        credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
        options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });
    // 👆👆👆 위 코드를 입력하세요

    var kernel = Kernel.CreateBuilder()
                       .AddGoogleAIGeminiChatCompletion(
                            modelId: config["Google:Gemini:ModelName"]!,
                            apiKey: config["Google:Gemini:ApiKey"]!,
                            serviceId: "google")
                       // 👇👇👇 아래 코드를 입력하세요
                       .AddOpenAIChatCompletion(
                            modelId: config["GitHub:Models:ModelIds:0"]!,
                            openAIClient: client,
                            serviceId: "github")
                       // 👆👆👆 위 코드를 입력하세요
                       .Build();
    
    var input = default(string);
    ```

1. `Workshop.ConsoleApp/Program.cs` 파일에서 `Console.WriteLine("--- Response from Google Gemini ---");` 라인을 찾아 아래와 같이 수정합니다.

    ```csharp
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

    // 👇👇👇 아래 코드를 추가하세요
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
    // 👆👆👆 위 코드를 추가하세요

    Console.WriteLine();
    ```

1. 파일을 저장한 후 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 문장이나 입력합니다. 예) `Who are you?` 또는 `하늘은 왜 파랄까?`
1. `Assistant: ` 프롬프트에 Google Gemini와 GitHub Models의 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-01/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-01`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-01/complete/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-01/complete/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

1. 워크샵 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 이전 [STEP 00: 개발 환경 설정하기](./step-00.md)에서 생성한 API 액세스 토큰을 콘솔 앱에 등록합니다.

    ```bash
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set Google:Gemini:ApiKey {{Google Gemini API Key}}
    dotnet user-secrets --project ./Workshop.ConsoleApp/ set GitHub:Models:AccessToken {{GitHub Models Access Token}}
    ```

1. 콘솔 앱을 실행시킵니다.

    ```bash
    dotnet run --project ./Workshop.ConsoleApp
    ```

1. `User:` 라는 프롬프트가 보이면 아무 문장이나 입력합니다. 예) `Who are you?` 또는 `하늘은 왜 파랄까?`
1. `Assistant: ` 프롬프트에 Google Gemini와 GitHub Models의 응답이 표시되는 것을 확인합니다.
1. 다시 `User: ` 프롬프트가 보이면 아무것도 입력하지 않고 엔터키를 눌러 콘솔 앱을 종료합니다.

## 도전 과제

- 이번 실습에서는 [GitHub Models Marketplace](https://github.com/marketplace?type=models)에서 제공하는 [`gpt-4o`](https://github.com/marketplace/models/azure-openai/gpt-4o) 모델을 사용했습니다. 이번에는 DeepSeek의 R1 모델을 추가해서 다른 답변과 비교해 보세요.

---

축하합니다! **Semantic Kernel 기본 작동법** 실습이 끝났습니다. 이제 [STEP 02: Semantic Kernel 플러그인 만들기](./step-02.md) 단계로 넘어가세요.

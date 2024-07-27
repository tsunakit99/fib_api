### 概要
Azure Functions を利用して、フィボナッチ数列を計算するHTTPトリガー関数を実装。また、MoQライブラリを利用して、ユニットテストを作成。HTTPリクエストで正の整数 `n` を渡すことで、その `n` 番目のフィボナッチ数を計算し、JSON形式で返す。誤った入力に対しては、適切なエラーメッセージを返す。

### プロジェクト構成
- `sp_test` フォルダには、フィボナッチ数列を計算する関数が含まれている。
- `Tests` フォルダには、MoQ を利用したユニットテストが含まれている。

### ソースコードの概要

#### `sp_test/fibFunction.cs`

このファイルには、フィボナッチ数列を計算する Azure Functions HTTP トリガー関数が定義されている。
```csharp
using System.Numerics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace sp_test
{
    public class fibFunction
    {
        private readonly ILogger<fibFunction> _logger;

        public fibFunction(ILogger<fibFunction> logger)
        {
            _logger = logger;
        }

        [Function("fib")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP トリガー関数がリクエストを処理しました。");

            // クエリパラメータから 'n' を取得
            string? nStr = req.Query["n"];

            // 'n' が空の場合はバッドリクエストを返す
            if (string.IsNullOrEmpty(nStr))
            {
                return new BadRequestObjectResult(new { status = 400, message = "リクエストに正の整数を含めて下さい。" });
            }

            // 'n' が正の整数でない場合はバッドリクエストを返す
            if (!BigInteger.TryParse(nStr, out BigInteger n))
            {
                return new BadRequestObjectResult(new { status = 400, message = "文字列または小数は無効な入力です。正の整数を入力してください。" });
            }

            // 'n' が負の数の場合はバッドリクエストを返す
            if (n < 0)
            {
                return new BadRequestObjectResult(new { status = 400, message = "負の数は無効な入力です。正の整数を入力してください。" });
            }

            // フィボナッチ数列の計算結果を取得
            BigInteger result = Fibonacci(n);

            // 結果を JSON 形式で返す
            return new OkObjectResult(new { result = result.ToString() });
        }

        // フィボナッチ数列を計算するメソッド
        private static BigInteger Fibonacci(BigInteger n)
        {
            if (n == 0) return 0;
            if (n == 1) return 1;

            BigInteger[] fib = new BigInteger[(int)n + 1];
            fib[0] = 0;
            fib[1] = 1;

            for (BigInteger i = 2; i <= n; i++)
            {
                fib[(int)i] = fib[(int)i - 1] + fib[(int)i - 2];
            }

            return fib[(int)n];
        }
    }
}
```

#### `Tests/Test_fibFunction.cs`

このファイルには、MoQ を利用したユニットテストが定義されている。テストは、HTTPリクエストのクエリパラメータに基づいて、関数のレスポンスを検証する。
```csharp
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using sp_test;

namespace Tests
{
    public class Test_fibFunction
    {
        private readonly Mock<ILogger<fibFunction>> _mockLogger;
        private readonly fibFunction _function;

        // コンストラクタでモックのロガーと関数を初期化
        public Test_fibFunction()
        {
            _mockLogger = new Mock<ILogger<fibFunction>>();
            _function = new fibFunction(_mockLogger.Object);
        }

        // テストケースを定義するTheory属性とInlineData属性を使用
        [Theory]
        [InlineData("5", HttpStatusCode.OK, "{\"result\":\"5\"}")]
        [InlineData("100", HttpStatusCode.OK, "{\"result\":\"354224848179261915075\"}")]
        [InlineData("-5", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"負の数は無効な入力です。正の整数を入力してください。\"}")]
        [InlineData("invalid", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"文字列または小数は無効な入力です。正の整数を入力してください。\"}")]
        [InlineData("1.2", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"文字列または小数は無効な入力です。正の整数を入力してください。\"}")]
        [InlineData(null, HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"リクエストに正の整数を含めて下さい。\"}")]
        public void TestFunction(string n, HttpStatusCode expectedStatusCode, string expectedResponse)
        {
            // Arrange（準備）
            var context = new DefaultHttpContext();
            var request = context.Request;
           
            // クエリパラメータを設定
            request.QueryString = new QueryString($"?n={n}");

            // Act（実行）
            var result = _function.Run(request) as ObjectResult;

            // Assert（検証）
            Assert.NotNull(result); // 結果がnullでないことを確認
            Assert.Equal((int)expectedStatusCode, result.StatusCode); // ステータスコードを検証

            // 期待されるレスポンスと実際のレスポンスを比較
            var expectedResponseObject = JsonConvert.DeserializeObject(expectedResponse);
            var actualResponseObject = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(expectedResponseObject, actualResponseObject);
        }
    }
}
```


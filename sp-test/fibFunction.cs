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

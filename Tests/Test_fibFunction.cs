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

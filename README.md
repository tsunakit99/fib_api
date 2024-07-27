### �T�v
Azure Functions �𗘗p���āA�t�B�{�i�b�`������v�Z����HTTP�g���K�[�֐��������B�܂��AMoQ���C�u�����𗘗p���āA���j�b�g�e�X�g���쐬�BHTTP���N�G�X�g�Ő��̐��� `n` ��n�����ƂŁA���� `n` �Ԗڂ̃t�B�{�i�b�`�����v�Z���AJSON�`���ŕԂ��B��������͂ɑ΂��ẮA�K�؂ȃG���[���b�Z�[�W��Ԃ��B

### �v���W�F�N�g�\��
- `sp_test` �t�H���_�ɂ́A�t�B�{�i�b�`������v�Z����֐����܂܂�Ă���B
- `Tests` �t�H���_�ɂ́AMoQ �𗘗p�������j�b�g�e�X�g���܂܂�Ă���B

### �\�[�X�R�[�h�̊T�v

#### `sp_test/fibFunction.cs`

���̃t�@�C���ɂ́A�t�B�{�i�b�`������v�Z���� Azure Functions HTTP �g���K�[�֐�����`����Ă���B
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
            _logger.LogInformation("C# HTTP �g���K�[�֐������N�G�X�g���������܂����B");

            // �N�G���p�����[�^���� 'n' ���擾
            string? nStr = req.Query["n"];

            // 'n' ����̏ꍇ�̓o�b�h���N�G�X�g��Ԃ�
            if (string.IsNullOrEmpty(nStr))
            {
                return new BadRequestObjectResult(new { status = 400, message = "���N�G�X�g�ɐ��̐������܂߂ĉ������B" });
            }

            // 'n' �����̐����łȂ��ꍇ�̓o�b�h���N�G�X�g��Ԃ�
            if (!BigInteger.TryParse(nStr, out BigInteger n))
            {
                return new BadRequestObjectResult(new { status = 400, message = "������܂��͏����͖����ȓ��͂ł��B���̐�������͂��Ă��������B" });
            }

            // 'n' �����̐��̏ꍇ�̓o�b�h���N�G�X�g��Ԃ�
            if (n < 0)
            {
                return new BadRequestObjectResult(new { status = 400, message = "���̐��͖����ȓ��͂ł��B���̐�������͂��Ă��������B" });
            }

            // �t�B�{�i�b�`����̌v�Z���ʂ��擾
            BigInteger result = Fibonacci(n);

            // ���ʂ� JSON �`���ŕԂ�
            return new OkObjectResult(new { result = result.ToString() });
        }

        // �t�B�{�i�b�`������v�Z���郁�\�b�h
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

���̃t�@�C���ɂ́AMoQ �𗘗p�������j�b�g�e�X�g����`����Ă���B�e�X�g�́AHTTP���N�G�X�g�̃N�G���p�����[�^�Ɋ�Â��āA�֐��̃��X�|���X�����؂���B
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

        // �R���X�g���N�^�Ń��b�N�̃��K�[�Ɗ֐���������
        public Test_fibFunction()
        {
            _mockLogger = new Mock<ILogger<fibFunction>>();
            _function = new fibFunction(_mockLogger.Object);
        }

        // �e�X�g�P�[�X���`����Theory������InlineData�������g�p
        [Theory]
        [InlineData("5", HttpStatusCode.OK, "{\"result\":\"5\"}")]
        [InlineData("100", HttpStatusCode.OK, "{\"result\":\"354224848179261915075\"}")]
        [InlineData("-5", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"���̐��͖����ȓ��͂ł��B���̐�������͂��Ă��������B\"}")]
        [InlineData("invalid", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"������܂��͏����͖����ȓ��͂ł��B���̐�������͂��Ă��������B\"}")]
        [InlineData("1.2", HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"������܂��͏����͖����ȓ��͂ł��B���̐�������͂��Ă��������B\"}")]
        [InlineData(null, HttpStatusCode.BadRequest, "{\"status\":400,\"message\":\"���N�G�X�g�ɐ��̐������܂߂ĉ������B\"}")]
        public void TestFunction(string n, HttpStatusCode expectedStatusCode, string expectedResponse)
        {
            // Arrange�i�����j
            var context = new DefaultHttpContext();
            var request = context.Request;
           
            // �N�G���p�����[�^��ݒ�
            request.QueryString = new QueryString($"?n={n}");

            // Act�i���s�j
            var result = _function.Run(request) as ObjectResult;

            // Assert�i���؁j
            Assert.NotNull(result); // ���ʂ�null�łȂ����Ƃ��m�F
            Assert.Equal((int)expectedStatusCode, result.StatusCode); // �X�e�[�^�X�R�[�h������

            // ���҂���郌�X�|���X�Ǝ��ۂ̃��X�|���X���r
            var expectedResponseObject = JsonConvert.DeserializeObject(expectedResponse);
            var actualResponseObject = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(expectedResponseObject, actualResponseObject);
        }
    }
}
```


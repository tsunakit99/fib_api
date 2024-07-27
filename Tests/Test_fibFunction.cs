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

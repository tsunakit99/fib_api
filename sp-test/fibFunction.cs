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

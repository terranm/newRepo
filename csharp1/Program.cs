using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StompRelayClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. 설정 로드 및 객체 매핑 (Options Pattern)
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .GetSection("StompConfig")
                .Get<StompConfig>() ?? new StompConfig(); // null 방지용 기본값

            // 2. STOMP 릴레이 서비스 실행
            using var stompService = new StompRelayService(config);
            await stompService.StartAsync();

            Console.WriteLine("프로그램이 실행 중입니다. (종료하려면 'exit' 입력)");
            Console.WriteLine($"[{config.PublishChannel}] 로 보낼 원문 JSON 데이터를 입력하고 엔터를 치세요.\n");

            // 3. 사용자 입력 발신 무한 루프
            while (true)
            {
                string? input = Console.ReadLine(); 
                if (input?.ToLower() == "exit") break;

                if (!string.IsNullOrWhiteSpace(input))
                {
                    stompService.SendRawMessage(input);
                    Console.WriteLine($"[발신 완료]");
                }
            }
        }
    }
}
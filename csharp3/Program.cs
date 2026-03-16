using System;
using System.Threading.Tasks;
using Ubisam.Stomp;

namespace MomaTestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. 통신 환경 설정
            var options = new StompOptions {
                WsUrl = "ws://192.168.0.23:9030/stomp/websocket",
                EnableAutoReconnect = true,    // 자동 재접속 켜기
                UnwrapPayload = true,          // Payload 알맹이만 받기
                EnableDebugLog = true          // 내부 동작 로그 켜기
            };

            // 2. 하위 통신 부품(웹소켓) 생성
            using ITransportHandler transport = new WebSocketTransportHandler(options);
            
            // 3. STOMP 클라이언트 생성 및 부품 주입 (Dependency Injection)
            using IStompClient client = new StompClient(options, transport);

            // 4. 이벤트 구독 설정
            client.OnConnected += () => 
            {
                Console.WriteLine("서버 연결 완료! 토픽을 구독합니다.");
                client.Subscribe("/topic/ubisam"); // 연결 성공 시 구독
            };
            
            client.OnMessageReceived += (destination, payload) => 
                Console.WriteLine($"수신 [토픽: {destination}]: {payload}");
                
            client.OnDisconnected += (reason) => 
                Console.WriteLine($"서버 연결 종료 (사유: {reason})");

            // 5. 서버 접속 및 데이터 송신
            await client.ConnectAsync();
            // Console.ReadLine();
            client.SendMessage("Ready"); // payload만 보내는 간단한 예시
            client.SendMessage("/app/ubisam", "{\"status\": \"ready\"}", "application/json"); // destination과 content-type을 명시하는 예시

            Console.ReadLine(); // 프로그램 종료 대기
            Console.WriteLine($"프로그램 종료");
            // using 블록이 끝나면 client와 transport가 안전하게 Dispose 됩니다.
        }
    }
}
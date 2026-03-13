using System;
using System.Threading.Tasks;
// ⭐️ 우리가 만든 라이브러리를 호출합니다!
using UbisamCSharpStompLibrary; 

namespace STOMPTestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MOMA STOMP 통신 테스트 클라이언트 ===\n");

            // 1. 통신 환경 설정 (DLL 사용자 입맛에 맞게 세팅)
            var config = new StompConfig
            {
                WsUrl = "ws://192.168.0.23:9030/stomp/websocket",
                SubscribeChannel = "/topic/ubisam",     // 관제 서버로부터 명령을 받을 채널
                PublishChannel = "/app/ubisam",         // 관제 서버로 상태를 보낼 채널
                EnableAutoReconnect = true,             // 끊기면 자동 재접속 켜기
                UnwrapPayload = true,                   // 알맹이만 까서 받기
                EnableDebugLog = true                   // 내부 동작 로그 켜기 (테스트니까 켭니다!)
            };

            // 2. 인터페이스를 통해 클라이언트 객체 생성
            using IStompClient stompClient = new StompClient(config);

            // 3. ⭐️ [가장 중요] 이벤트 구독: 서버에서 무슨 일이 생기면 이 함수들을 실행해라!
            stompClient.OnConnected += () => 
            {
                Console.WriteLine("\n[이벤트] 🟢 서버에 정상적으로 연결되었습니다!");
            };

            stompClient.OnDisconnected += (reason) => 
            {
                Console.WriteLine($"\n[이벤트] 🔴 서버와 연결이 끊어졌습니다. (사유: {reason})");
            };

            stompClient.OnMessageReceived += (payload) => 
            {
                Console.WriteLine($"\n[이벤트] 📥 관제 서버에서 데이터가 도착했습니다:\n {payload}");
                // TODO: 여기서 수신된 payload를 분석하여 MOMA 로봇을 움직이거나 상태를 갱신합니다.
            };

            // 4. 서버 연결 시작 (비동기)
            Console.WriteLine("서버에 접속을 시도합니다...");
            await stompClient.ConnectAsync();

            // 5. 사용자 입력 루프 (원하는 데이터를 직접 타이핑해서 서버로 쏴볼 수 있습니다)
            Console.WriteLine("\n[안내] 전송할 JSON 데이터를 입력하고 엔터를 누르세요. (종료하려면 'exit' 입력)");
            
            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                if (input.ToLower() == "exit") break;

                // 입력받은 데이터를 관제 서버로 전송
                stompClient.SendMessage(input);
                Console.WriteLine($"[발신] 📤 데이터를 전송했습니다: {input}");
            }

            // 6. 안전한 종료
            stompClient.Disconnect();
            Console.WriteLine("테스트 프로그램을 종료합니다.");
        }
    }
}
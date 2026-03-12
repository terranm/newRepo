using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Websocket.Client;
using System.Text;

namespace StompRelayClient
{
    public class StompRelayService : IDisposable
    {
        private readonly WebsocketClient _client;
        private readonly StompConfig _config;
        private readonly System.Timers.Timer _heartbeatTimer;

        public StompRelayService(StompConfig config)
        {
            _config = config;
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket());
            
            _client = new WebsocketClient(new Uri(_config.WsUrl), factory)
            {
                //ReconnectTimeout = TimeSpan.FromSeconds(_config.ReconnectIntervalSeconds),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(_config.ReconnectIntervalSeconds)
            };

            _heartbeatTimer = new System.Timers.Timer(8000);
            _heartbeatTimer.Elapsed += (sender, e) =>
            {
                if (_client.IsRunning)
                {
                    _client.Send("\n"); // STOMP 규격의 Heartbeat 전송
                }
            };

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _client.MessageReceived.Subscribe(msg =>
            {
                Console.WriteLine($"[수신] {msg.Text}");
                if (msg.Text != null && msg.Text.StartsWith("CONNECTED"))
                {
                    Console.WriteLine("서버와 STOMP 연결 성공! 토픽을 구독합니다.");
                    SendFrame("SUBSCRIBE", $"id:sub-0\ndestination:{_config.SubscribeChannel}");

                    _heartbeatTimer.Start();
                }
            });

            _client.ReconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"[연결됨] 타입: {info.Type}");
                SendFrame("CONNECT", "accept-version:1.1,1.2\nheart-beat:10000,10000");
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                _heartbeatTimer.Stop();
                Console.WriteLine($"[연결 끊김] 다시 연결 시도 중... (이유: {info.CloseStatus})");
            });
        }

        public async Task StartAsync()
        {
            await _client.Start();
        }

        // 제약조건: 내용물을 깊게 해석하지 않고, 껍데기(첫 글자)만 보고 타입을 결정하여 원문 발신
        public void SendRawMessage(string payload)
        {
            string trimmedPayload = payload.Trim();
            string finalPayload = payload;

            // 만약 입력값이 '{' 나 '[' 로 시작하지 않는 순수 텍스트라면?
            if (!trimmedPayload.StartsWith("{") && !trimmedPayload.StartsWith("["))
            {
                // 객체 {"message":"asd"} 가 아니라, 순수 문자열 "asd" 형태로 묶어줍니다.
                // (내부에 있는 쌍따옴표가 깨지지 않도록 이스케이프 처리 포함)
                finalPayload = $"\"{payload.Replace("\"", "\\\"")}\"";
            }

            int contentLength = Encoding.UTF8.GetByteCount(finalPayload);

            // 서버가 무조건 JSON 컨버터를 쓰므로 content-type은 application/json으로 고정합니다.
            SendFrame("SEND", $"destination:{_config.PublishChannel}\ncontent-type:application/json\ncontent-length:{contentLength}", finalPayload);
        }

        private void SendFrame(string command, string headers, string body = "")
        {
            string frame = $"{command}\n{headers}\n\n{body}\0";
            _client.Send(frame);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Websocket.Client;
using System.Text.Json;

namespace StompRelayClient
{
    public class StompRelayService : IDisposable
    {
        private readonly WebsocketClient _client;
        private readonly StompConfig _config;
        private readonly System.Timers.Timer _heartbeatTimer;
        
        // 여러 컨트롤러의 통신 파이프를 담아두는 스레드 안전 보관함
        private readonly ConcurrentDictionary<string, NetworkStream> _activeControllers = new();

        public StompRelayService(StompConfig config)
        {
            _config = config;
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket());
            
            _client = new WebsocketClient(new Uri(_config.WsUrl), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(_config.ReconnectIntervalSeconds)
            };

            _heartbeatTimer = new System.Timers.Timer(8000);
            _heartbeatTimer.Elapsed += (sender, e) =>
            {
                if (_client.IsRunning) _client.Send("\n");
            };

            SetupEventHandlers();
        }

        // ⭐️ [신규 추가] 현재 중계기의 토픽 및 클라이언트 연결 상태를 예쁘게 출력하는 전광판 함수
        private void PrintSystemStatus()
        {
            Console.WriteLine("\n=================================================");
            Console.WriteLine($"[📡 관제 서버 토픽 상태]");
            Console.WriteLine($"  - 구독(수신) 채널 : {_config.SubscribeChannel}");
            Console.WriteLine($"  - 발행(송신) 채널 : {_config.PublishChannel}");
            Console.WriteLine($"\n[🔌 바이패스 클라이언트 연결 상태 (총 {_activeControllers.Count}개)]");
            
            if (_activeControllers.IsEmpty)
            {
                Console.WriteLine("  - 현재 연결된 하위 제어 프로그램이 없습니다.");
            }
            else
            {
                foreach (var ctrl in _activeControllers.Keys)
                {
                    Console.WriteLine($"  - 🟢 {ctrl} (정상 연결됨)");
                }
            }
            Console.WriteLine("=================================================\n");
        }

        private void StartControllers()
        {
            foreach (var ctrl in _config.Controllers)
            {
                _ = Task.Run(() => MaintainControllerConnectionAsync(ctrl));
            }
        }

        private async Task MaintainControllerConnectionAsync(ControllerConfig ctrl)
        {
            while (true)
            {
                using TcpClient tcpClient = new TcpClient();
                try
                {
                    await tcpClient.ConnectAsync(ctrl.Ip, ctrl.Port);
                    
                    using NetworkStream stream = tcpClient.GetStream();
                    
                    // 연결 성공 시 명단에 추가하고 상태 전광판 출력
                    if (_activeControllers.TryAdd(ctrl.Name, stream))
                    {
                        PrintSystemStatus();
                    }

                    byte[] inBuffer = new byte[4096];
                    while (tcpClient.Connected)
                    {
                        int bytesRead = await stream.ReadAsync(inBuffer, 0, inBuffer.Length);
                        if (bytesRead == 0) break; 

                        string response = Encoding.UTF8.GetString(inBuffer, 0, bytesRead).Trim();
                        if (!string.IsNullOrWhiteSpace(response))
                        {
                            Console.WriteLine($"[{ctrl.Name} ➡️ 관제 서버] {response}");
                            SendRawMessage(response); 
                        }
                    }
                }
                catch (Exception)
                {
                    // 연결 대기 중에는 조용히 무시
                }
                finally
                {
                    // 연결이 끊어지면 명단에서 제거하고 상태 전광판 갱신
                    if (_activeControllers.TryRemove(ctrl.Name, out _))
                    {
                        Console.WriteLine($"[TCP 끊김] 🔴 {ctrl.Name} 연결 해제됨. 재연결 대기 중...");
                        PrintSystemStatus();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_config.ReconnectIntervalSeconds));
            }
        }

        private void SetupEventHandlers()
        {
            _client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text == null) return;

                if (msg.Text.StartsWith("MESSAGE"))
                {
                    int bodyIndex = msg.Text.IndexOf("\n\n");
                    if (bodyIndex != -1)
                    {
                        string body = msg.Text.Substring(bodyIndex + 2).TrimEnd('\0');
                        Console.WriteLine($"[관제 서버 수신] {body}");

                        // JSON 에서 payload 추출
                        string targetData = body;
                        try
                        {
                            using (JsonDocument doc = JsonDocument.Parse(body))
                            {
                                if (doc.RootElement.TryGetProperty("payload", out JsonElement payloadElement))
                                {
                                    // payload가 문자열로 한 번 더 감싸져 있으면 따옴표를 벗기고, 객체면 그대로 추출
                                    targetData = payloadElement.ValueKind == JsonValueKind.String 
                                        ? payloadElement.GetString() ?? body 
                                        : payloadElement.GetRawText();
                                        
                                    Console.WriteLine($"[📦 Payload 추출 완료 ➡️ 컨트롤러 전달] {targetData}");
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // JSON이 아니거나 에러가 나면 튕기지 않고 원문을 그대로 던짐 (방어적 코딩)
                            Console.WriteLine($"[경고] JSON 파싱 실패. 원문을 전달합니다: {body}");
                        }

                        byte[] outBytes = Encoding.UTF8.GetBytes(targetData);
                        
                        foreach (var kvp in _activeControllers)
                        {
                            try
                            {
                                await kvp.Value.WriteAsync(outBytes, 0, outBytes.Length);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[경고] '{kvp.Key}' 컨트롤러로 메시지 전송 실패: {ex.Message}");
                            }
                        }
                    }
                }
                else if (msg.Text.StartsWith("CONNECTED"))
                {
                    Console.WriteLine("관제 서버와 STOMP 연결 성공! 토픽을 구독합니다.");
                    SendFrame("SUBSCRIBE", $"id:sub-0\ndestination:{_config.SubscribeChannel}");
                    
                    // 관제 서버와 STOMP 구독이 완료된 시점에 상태 전광판 출력
                    PrintSystemStatus();
                }
            });

            _client.ReconnectionHappened.Subscribe(info =>
            {
                SendFrame("CONNECT", "accept-version:1.1,1.2\nheart-beat:10000,10000");
                _heartbeatTimer.Start();
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                _heartbeatTimer.Stop();
                Console.WriteLine($"[STOMP 연결 끊김] 다시 연결 시도 중... (이유: {info.CloseStatus})");
            });
        }

        public async Task StartAsync()
        {
            StartControllers(); 
            await _client.Start(); 
        }

        public void SendRawMessage(string payload)
        {
            string trimmedPayload = payload.Trim();
            string finalPayload = payload;

            if (!trimmedPayload.StartsWith("{") && !trimmedPayload.StartsWith("["))
            {
                finalPayload = $"\"{payload.Replace("\"", "\\\"")}\"";
            }

            int contentLength = Encoding.UTF8.GetByteCount(finalPayload);
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
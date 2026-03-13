import socket
import json
import os
import threading
import time

# C# 중계기가 사용하는 설정 파일의 경로 (파이썬 스크립트 위치 기준)
# 프로젝트 폴더가 다르다면 절대 경로(예: 'C:/Work/csharp1/appsettings.json')로 수정해 주세요.
CONFIG_FILE = 'C:\\Users\\Ihe box\\Desktop\\Work\\asdf\\csharp1\\appsettings.json' 

# appsettings.json의 Controllers 배열 안에서 찾을 내 이름
MY_CONTROLLER_NAME = 'Python_CobotController'

def load_network_config():
    """
    외부 JSON 설정 파일에서 현재 컨트롤러의 IP와 Port를 동적으로 읽어옵니다.
    """
    host = '127.0.0.1' # 실패 시 사용할 기본값
    port = 9999
    
    try:
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, 'r', encoding='utf-8') as f:
                config_data = json.load(f)
                
                # StompConfig -> Controllers 배열 순회
                controllers = config_data.get("StompConfig", {}).get("Controllers", [])
                for ctrl in controllers:
                    if ctrl.get("Name") == MY_CONTROLLER_NAME:
                        host = ctrl.get("Ip", host)
                        port = ctrl.get("Port", port)
                        print(f"[설정 완료] '{CONFIG_FILE}'에서 '{MY_CONTROLLER_NAME}'의 네트워크 설정을 성공적으로 불러왔습니다.")
                        return host, port
                        
            print(f"[설정 경고] 파일에 '{MY_CONTROLLER_NAME}' 항목이 없습니다. 기본값({host}:{port})을 사용합니다.")
        else:
            print(f"[설정 경고] '{CONFIG_FILE}' 파일을 찾을 수 없습니다. 기본값({host}:{port})을 사용합니다.")
            
    except Exception as e:
        print(f"[설정 오류] 설정 파일 로드 중 예외 발생: {e}. 기본값을 사용합니다.")
        
    return host, port

# 서버 시작 시 동적으로 IP와 포트를 할당받습니다.
HOST, PORT = load_network_config()

def process_message(raw_message: str):
    """
    관제 서버에서 온 메시지를 분석하고, 회신할 데이터를 생성하는 로직입니다.
    - 대답해야 할 메시지: 유효한 JSON 문자열 반환
    - 무시해야 할 메시지(에코 루프 방지): None 반환
    """
    try:
        # 1. JSON 형태의 데이터인지 확인
        if raw_message.startswith('{') or raw_message.startswith('['):
            data = json.loads(raw_message)
                                    
            # 2. 정확한 명령어 확인 및 회신 데이터 조립
            if isinstance(data, dict) and data.get("command") == "req_status":
                print("\n[분석] 상태 요청 감지. 로봇 상태 데이터를 조립하여 회신합니다.")
                # 실제 MOMA 로봇의 센서/상태 값을 읽어와서 조립하는 로직이 들어갈 자리입니다.
                return json.dumps({"response": "status_ok", "battery": 85, "status": "moving"})
            
            # 3. 그 외의 모든 JSON은 관심 없는 데이터이므로 무시 (None 반환)
            print(f"[무시] 관심 없는 JSON 데이터입니다: {raw_message}") # 콘솔이 너무 혼잡해지면 주석
            return None
            
        # 일반 텍스트가 들어온 경우도 무시
        print(f"[무시] 일반 텍스트 데이터입니다: {raw_message}") # 콘솔이 너무 혼잡해지면 주석
        return None

    except json.JSONDecodeError:
        print("[오류] JSON 파싱에 실패했습니다.")
        return None
    except Exception as e:
        print(f"[오류] 데이터 처리 중 예외 발생: {e}")
        return None

def handle_client(conn: socket.socket, addr: tuple):
    """
    연결된 C# 중계기와의 실질적인 데이터 송수신(I/O) 흐름을 제어합니다.
    """
    print(f"\n[TCP 연결됨] C# 중계기 접속: {addr}")
    with conn:
        while True:
            try:
                # C# 중계기로부터 원문 데이터 수신 대기
                data = conn.recv(4096)
                if not data:
                    print("[TCP 알림] C# 중계기가 정상적으로 연결을 종료했습니다.")
                    break

                # 바이트를 문자열로 디코딩
                raw_message = data.decode('utf-8').strip()
                print(f"\n[수신] {raw_message}")
                
                # 분석 함수 호출 - 반환값이 None이면 무시, 문자열이면 C#으로 회신
                response_text = process_message(raw_message)

                # 반환값이 있을 때만 C#으로 전송
                if response_text:
                    print(f"[회신 전송] {response_text}")
                    conn.sendall((response_text + "\n").encode('utf-8'))
                    
            except ConnectionResetError:
                # 상대방(C#) 프로그램이 비정상적으로 강제 종료되었을 때 발생하는 에러 방어
                print("[TCP 경고] C# 중계기가 비정상적으로 강제 종료되어 연결이 끊어졌습니다.")
                break
            except Exception as e:
                print(f"[TCP 오류] 통신 중 예외 발생: {e}")
                break

def server_loop():
    """
    TCP 소켓 서버를 열고, 클라이언트가 끊어지더라도 절대 죽지 않고 다시 대기합니다.
    """
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        
        try:
            s.bind((HOST, PORT))
            s.listen()
            s.settimeout(1.0)
        except Exception as e:
            print(f"[치명적 오류] 서버를 시작할 수 없습니다. 포트({PORT})가 이미 사용 중인지 확인하세요: {e}")
            return

        print(f"파이썬 분석 서버가 {HOST}:{PORT} 에서 대기 중입니다...")
        print("(종료하려면 아무 때나 Ctrl+C를 누르세요)")

        # 이 무한 루프가 파이썬 서버를 '불사조'로 만들어 줍니다.
        while True:
            try:
                # 새로운 연결이 들어올 때까지 대기
                conn, addr = s.accept()
                
                # 클라이언트와 통신 시작 (통신 중 에러가 나도 handle_client 내부에서 방어됨)
                handle_client(conn, addr)
                
                # 통신이 정상/비정상 종료된 후 다시 루프의 처음으로 돌아가 대기
                print(f"파이썬 분석 서버가 {HOST}:{PORT} 에서 새로운 접속을 다시 대기합니다...")
                        
            except Exception as e:
                # 그 외의 알 수 없는 서버 에러 발생 시에도 죽지 않고 버팀
                print(f"[서버 오류] 연결 수락 중 문제 발생: {e}")

if __name__ == "__main__":
    # 1. 네트워크 통신 로직을 '데몬 스레드(daemon=True)'라는 백그라운드 일꾼에게 던져버립니다.
    server_thread = threading.Thread(target=server_loop, daemon=True)
    server_thread.start()

    # 2. 메인 스레드(본체)는 오직 키보드 입력(Ctrl+C)만 0.5초 단위로 감시합니다.
    try:
        while server_thread.is_alive():
            time.sleep(0.5)
    except KeyboardInterrupt:
        # 사용자가 Ctrl+C를 누르는 즉시 메인 스레드가 끝나고, 백그라운드 일꾼도 즉각 참수(강제 종료)됩니다!
        print("\n[서버 종료] 관리자의 명령으로 즉시 모든 통신을 끊고 종료합니다.")

        
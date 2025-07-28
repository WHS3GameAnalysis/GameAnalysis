from fastapi import FastAPI, Request, HTTPException
from fastapi.responses import PlainTextResponse, FileResponse, RedirectResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional
import os
import json
import time
import hashlib
import secrets
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import base64

app = FastAPI()

origins = ["*"]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 로그 파일 기본 경로 정의
LOGS_BASE_PATH = "/home/whitehat/logs/"

# RSA 키 페어 (파일에서 로드)
def load_key_from_file(file_path: str) -> str:
    """키 파일 읽기"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        print(f"키 파일 읽기 실패 {file_path}: {e}")
        return ""

# 키 파일 경로
CHALLENGE_PRIVATE_KEY_PATH = "chal_priv.pem"
CHALLENGE_PUBLIC_KEY_PATH = "chal_pub.pem"
RESPONSE_PRIVATE_KEY_PATH = "resp_priv.pem"
RESPONSE_PUBLIC_KEY_PATH = "resp_pub.pem"

# 키 파일에서 로드
CHALLENGE_PRIVATE_KEY = load_key_from_file(CHALLENGE_PRIVATE_KEY_PATH)
CHALLENGE_PUBLIC_KEY = load_key_from_file(CHALLENGE_PUBLIC_KEY_PATH)
RESPONSE_PRIVATE_KEY = load_key_from_file(RESPONSE_PRIVATE_KEY_PATH)
RESPONSE_PUBLIC_KEY = load_key_from_file(RESPONSE_PUBLIC_KEY_PATH)

# 키 로드 상태 확인
print(f"CHALLENGE_PRIVATE_KEY 로드: {'성공' if CHALLENGE_PRIVATE_KEY else '실패'}")
print(f"CHALLENGE_PUBLIC_KEY 로드: {'성공' if CHALLENGE_PUBLIC_KEY else '실패'}")
print(f"RESPONSE_PRIVATE_KEY 로드: {'성공' if RESPONSE_PRIVATE_KEY else '실패'}")
print(f"RESPONSE_PUBLIC_KEY 로드: {'성공' if RESPONSE_PUBLIC_KEY else '실패'}")

# 활성 챌린지 저장소 (실제로는 Redis 등 사용)
active_challenges = {}

class Heartbeat(BaseModel):
    client_id: Optional[str] = None
    timestamp: str
    version: str

class EncryptedResponse(BaseModel):
    encrypted_data: str

# 무결성 검사 로깅 함수
def log_integrity(message: str):
    """무결성 검사 로그 기록"""
    try:
        timestamp = time.strftime("%Y-%m-%d %H:%M:%S")
        log_entry = f"[{timestamp}] [INTEGRITY] {message}\n"
        log_file_path = os.path.join(LOGS_BASE_PATH, "integrity.log")
        
        with open(log_file_path, "a", encoding="utf-8") as f:
            f.write(log_entry)
    except Exception as e:
        print(f"무결성 로그 작성 실패: {e}")

def load_private_key(key_pem: str):
    """PEM 형식 개인키 로드"""
    try:
        return serialization.load_pem_private_key(
            key_pem.encode('utf-8'),
            password=None,
            backend=default_backend()
        )
    except Exception as e:
        print(f"개인키 로드 실패: {e}")
        return None

def load_public_key(key_pem: str):
    """PEM 형식 공개키 로드"""
    try:
        return serialization.load_pem_public_key(
            key_pem.encode('utf-8'),
            backend=default_backend()
        )
    except Exception as e:
        print(f"공개키 로드 실패: {e}")
        return None

def encrypt_with_rsa(data: str, public_key) -> str:
    """RSA 공개키로 암호화"""
    try:
        encrypted = public_key.encrypt(
            data.encode('utf-8'),
            padding.OAEP(
                mgf=padding.MGF1(algorithm=hashes.SHA256()),
                algorithm=hashes.SHA256(),
                label=None
            )
        )
        return base64.b64encode(encrypted).decode('utf-8')
    except Exception as e:
        print(f"RSA 암호화 실패: {e}")
        return ""

def encrypt_hybrid_challenge(data: str, public_key) -> str:
    """하이브리드 암호화로 챌린지 암호화 (AES + RSA)"""
    try:
        # 1. AES 키와 IV 생성
        aes_key = secrets.token_bytes(32)  # 256비트 키
        iv = secrets.token_bytes(16)       # 128비트 IV
        
        # 2. PKCS7 패딩 추가
        data_bytes = data.encode('utf-8')
        padding_length = 16 - (len(data_bytes) % 16)
        padded_data = data_bytes + bytes([padding_length] * padding_length)
        
        # 3. AES로 데이터 암호화
        cipher = Cipher(
            algorithms.AES(aes_key),
            modes.CBC(iv),
            backend=default_backend()
        )
        encryptor = cipher.encryptor()
        encrypted_data = encryptor.update(padded_data) + encryptor.finalize()
        
        # 4. RSA로 AES 키 암호화
        encrypted_key = public_key.encrypt(
            aes_key,
            padding.OAEP(
                mgf=padding.MGF1(algorithm=hashes.SHA256()),
                algorithm=hashes.SHA256(),
                label=None
            )
        )
        
        # 5. 결과를 JSON으로 조합
        hybrid_result = {
            "encrypted_key": base64.b64encode(encrypted_key).decode('utf-8'),
            "iv": base64.b64encode(iv).decode('utf-8'),
            "encrypted_data": base64.b64encode(encrypted_data).decode('utf-8')
        }
        
        return json.dumps(hybrid_result)
        
    except Exception as e:
        print(f"하이브리드 암호화 실패: {e}")
        return ""

def decrypt_with_rsa(encrypted_data: str, private_key) -> str:
    """RSA 개인키로 복호화"""
    try:
        encrypted_bytes = base64.b64decode(encrypted_data)
        decrypted = private_key.decrypt(
            encrypted_bytes,
            padding.OAEP(
                mgf=padding.MGF1(algorithm=hashes.SHA256()),
                algorithm=hashes.SHA256(),
                label=None
            )
        )
        return decrypted.decode('utf-8')
    except Exception as e:
        print(f"RSA 복호화 실패: {e}")
        return ""

def decrypt_hybrid_response(encrypted_data: str, private_key) -> str:
    """하이브리드 암호화된 응답 복호화 (AES + RSA)"""
    try:
        # 1. JSON 데이터 파싱
        hybrid_data = json.loads(encrypted_data)
        encrypted_key = base64.b64decode(hybrid_data["encrypted_key"])
        iv = base64.b64decode(hybrid_data["iv"])
        encrypted_data_bytes = base64.b64decode(hybrid_data["encrypted_data"])
        
        # 2. RSA로 AES 키 복호화
        aes_key = private_key.decrypt(
            encrypted_key,
            padding.OAEP(
                mgf=padding.MGF1(algorithm=hashes.SHA256()),
                algorithm=hashes.SHA256(),
                label=None
            )
        )
        
        # 3. AES로 실제 데이터 복호화
        cipher = Cipher(
            algorithms.AES(aes_key),
            modes.CBC(iv),
            backend=default_backend()
        )
        decryptor = cipher.decryptor()
        decrypted_data = decryptor.update(encrypted_data_bytes) + decryptor.finalize()
        
        # 4. PKCS7 패딩 제거
        padding_length = decrypted_data[-1]
        decrypted_data = decrypted_data[:-padding_length]
        
        return decrypted_data.decode('utf-8')
        
    except Exception as e:
        print(f"하이브리드 복호화 실패: {e}")
        return ""

def generate_challenge() -> dict:
    """새로운 챌린지 생성"""
    challenge_id = secrets.token_hex(16)
    challenge_data = secrets.token_hex(32)
    current_time = int(time.time())
    expires_at = current_time + 300  # 5분 후 만료
    
    # SHA512 알고리즘 사용
    algorithm = "sha512"
    
    challenge = {
        "challenge_id": challenge_id,
        "challenge_data": challenge_data,
        "algorithm": algorithm,
        "expires_at": expires_at
    }
    
    # 활성 챌린지에 저장
    active_challenges[challenge_id] = {
        "data": challenge_data,
        "algorithm": algorithm,
        "expires_at": expires_at,
        "created_at": current_time
    }
    
    return challenge

def calculate_expected_response(challenge_data: str, algorithm: str, client_id: str, system_fingerprint: str, timestamp: int = None) -> str:
    """예상 응답값 계산"""
    combined_data = f"{challenge_data}:{system_fingerprint}:{client_id}"
    return hashlib.sha512(combined_data.encode()).hexdigest().upper()

def cleanup_expired_challenges():
    """만료된 챌린지 정리"""
    current_time = int(time.time())
    expired_challenges = [
        challenge_id for challenge_id, challenge in active_challenges.items()
        if challenge["expires_at"] < current_time
    ]
    
    for challenge_id in expired_challenges:
        del active_challenges[challenge_id]
    
    if expired_challenges:
        print(f"만료된 챌린지 {len(expired_challenges)}개 정리됨")

def write_log(log_type: str, message: str):
    """로그 파일에 메시지 작성"""
    try:
        log_file_path = os.path.join(LOGS_BASE_PATH, f"{log_type}.log")
        os.makedirs(os.path.dirname(log_file_path), exist_ok=True)
        
        with open(log_file_path, "a", encoding="utf-8") as f:
            timestamp = time.strftime("%Y-%m-%d %H:%M:%S")
            f.write(f"[{timestamp}] {message}\n")
    except Exception as e:
        print(f"로그 작성 실패: {e}")

# 무결성 검사 관련 함수들
def decrypt_integrity_request(encrypted_data: str) -> str:
    """무결성 검사 요청 복호화 - 기존 하트비트 복호화 방식 활용"""
    try:
        # 응답 개인키로 복호화 (클라이언트가 응답 공개키로 암호화했으므로)
        response_private_key = load_private_key(RESPONSE_PRIVATE_KEY)
        if not response_private_key:
            raise Exception("서버 키 로드 실패")
        
        decrypted_data = decrypt_hybrid_response(encrypted_data, response_private_key)
        if not decrypted_data:
            raise Exception("요청 복호화 실패")
        
        return decrypted_data
        
    except Exception as e:
        print(f"무결성 요청 복호화 실패: {e}")
        raise

def encrypt_integrity_response(plain_text: str) -> str:
    """무결성 검사 응답 암호화 - 기존 하트비트 암호화 방식 활용"""
    try:
        # 챌린지 공개키로 암호화 (클라이언트가 챌린지 개인키로 복호화)
        challenge_public_key = load_public_key(CHALLENGE_PUBLIC_KEY)
        if not challenge_public_key:
            raise Exception("서버 키 로드 실패")
        
        encrypted_response = encrypt_hybrid_challenge(plain_text, challenge_public_key)
        if not encrypted_response:
            raise Exception("응답 암호화 실패")
        
        return encrypted_response
        
    except Exception as e:
        print(f"무결성 응답 암호화 실패: {e}")
        raise

@app.get("/heartbeat_init")
async def heartbeat_init(request: Request):
    """암호화된 챌린지 생성 및 반환"""
    try:
        client_ip = request.client.host
        
        # 만료된 챌린지 정리
        cleanup_expired_challenges()
        
        # 새 챌린지 생성
        challenge = generate_challenge()
        
        # 챌린지를 JSON으로 직렬화
        challenge_json = json.dumps(challenge)
        
        # 클라이언트 공개키로 암호화 (실제로는 클라이언트 공개키 사용)
        # 여기서는 예시로 challenge_public_key 사용
        challenge_public_key = load_public_key(CHALLENGE_PUBLIC_KEY)
        if not challenge_public_key:
            raise HTTPException(status_code=500, detail="서버 키 로드 실패")
        
        encrypted_challenge = encrypt_hybrid_challenge(challenge_json, challenge_public_key)
        
        if not encrypted_challenge:
            raise HTTPException(status_code=500, detail="챌린지 암호화 실패")
        
        # 로그 기록
        log_message = f"IP={client_ip}, ChallengeID={challenge['challenge_id']}, Algorithm={challenge['algorithm']}"
        write_log("heartbeat", f"챌린지 생성: {log_message}")
        
        print(f"챌린지 생성: {log_message}")
        
        return {"encrypted_data": encrypted_challenge}
        
    except Exception as e:
        print(f"챌린지 생성 오류: {e}")
        raise HTTPException(status_code=500, detail="챌린지 생성 실패")

@app.post("/heartbeat_send")
async def heartbeat_send(data: EncryptedResponse, request: Request):
    """암호화된 응답 수신 및 검증"""
    try:
        client_ip = request.client.host
        
        # 응답 개인키로 복호화
        response_private_key = load_private_key(RESPONSE_PRIVATE_KEY)
        if not response_private_key:
            raise HTTPException(status_code=500, detail="서버 키 로드 실패")
        
        decrypted_response = decrypt_hybrid_response(data.encrypted_data, response_private_key)
        if not decrypted_response:
            raise HTTPException(status_code=400, detail="응답 복호화 실패")

        print(f"복호화된 응답: {decrypted_response}")
        
        # JSON 파싱
        try:
            response_data = json.loads(decrypted_response)
        except json.JSONDecodeError:
            raise HTTPException(status_code=400, detail="잘못된 JSON 형식")
        
        challenge_id = response_data.get("challenge_id")
        client_id = response_data.get("client_id")
        response_value = response_data.get("response_value")
        timestamp = response_data.get("timestamp")
        version = response_data.get("version")
        system_fingerprint = response_data.get("system_fingerprint")
        
        if not all([challenge_id, client_id, response_value, system_fingerprint]):
            raise HTTPException(status_code=400, detail="필수 필드 누락")
        
        # 챌린지 존재 확인
        if challenge_id not in active_challenges:
            raise HTTPException(status_code=400, detail="유효하지 않은 챌린지 ID")
        
        challenge_info = active_challenges[challenge_id]
        
        # 만료 확인
        current_time = int(time.time())
        if current_time > challenge_info["expires_at"]:
            del active_challenges[challenge_id]
            raise HTTPException(status_code=400, detail="만료된 챌린지")
        
        # 응답값 검증 (간단한 예시)
        expected_response = calculate_expected_response(
            challenge_info["data"],
            challenge_info["algorithm"],
            client_id,
            system_fingerprint,
            timestamp
        )
        
        is_valid = response_value.upper() == expected_response.upper()
        
        # 사용된 챌린지 제거
        del active_challenges[challenge_id]
        
        # 로그 기록
        log_message = f"IP={client_ip}, ClientID={client_id}, ChallengeID={challenge_id}, Valid={is_valid}, Version={version}"
        write_log("heartbeat", f"응답 검증: {log_message}")
        
        if is_valid:
            print(f"하트비트 검증 성공: {log_message}")
            return {"status": "success", "message": "하트비트 검증 성공"}
        else:
            print(f"하트비트 검증 실패: {log_message}")
            return {"status": "failure", "message": "하트비트 검증 실패"}
        
    except HTTPException:
        raise
    except Exception as e:
        print(f"하트비트 처리 오류: {e}")
        raise HTTPException(status_code=500, detail="하트비트 처리 실패")

# 무결성 검사 API 엔드포인트
@app.post("/api/integrity/check")
async def check_integrity(request: Request):
    """무결성 검사 API - 기존 하트비트 보안 방식 활용"""
    try:
        # 1. 클라이언트 요청 수신 및 복호화
        encrypted_data = await request.json()
        encrypted_request = encrypted_data.get("encrypted_data", "")
        
        if not encrypted_request:
            log_integrity("오류: 암호화된 데이터 없음")
            raise HTTPException(status_code=400, detail="암호화된 데이터 없음")
        
        log_integrity(f"암호화된 요청 수신: {len(encrypted_request)} 문자")
        
        try:
            decrypted_data = decrypt_integrity_request(encrypted_request)
            log_integrity(f"복호화 성공: {len(decrypted_data)} 문자")
        except Exception as e:
            log_integrity(f"복호화 실패: {str(e)}")
            raise HTTPException(status_code=400, detail=f"복호화 실패: {str(e)}")
        
        try:
            client_data = json.loads(decrypted_data)
            log_integrity(f"JSON 파싱 성공: {len(client_data)} 필드")
            # 실제 받은 데이터 내용 로그
            log_integrity(f"받은 데이터 키들: {list(client_data.keys())}")
            log_integrity(f"받은 데이터 내용: {json.dumps(client_data, ensure_ascii=False)}")
        except Exception as e:
            log_integrity(f"JSON 파싱 실패: {str(e)}")
            raise HTTPException(status_code=400, detail=f"JSON 파싱 실패: {str(e)}")
        
        client_id = client_data.get('client_id', 'UNKNOWN')
        game_version = client_data.get('game_version', 'UNKNOWN')
        client_hashes = client_data.get('hashes', [])
        
        # 2. 로그 기록 시작
        log_integrity(f"클라이언트 접속: {client_id}")
        log_integrity(f"버전 확인: {game_version}")
        log_integrity(f"해시값 검사 시작: {len(client_hashes)}개 파일")
        
        # 3. 서버 해시값 로드
        current_version = "v72"  # 나중에 파일에서 읽기
        server_hashes_file = f"/home/whitehat/hashes/{current_version}/integrity.json"
        
        if not os.path.exists(server_hashes_file):
            log_integrity(f"서버 해시값 파일 없음: {server_hashes_file}")
            raise HTTPException(status_code=500, detail="서버 해시값 파일 없음")
        
        with open(server_hashes_file, 'r') as f:
            server_data = json.load(f)
        
        # 4. 해시값 비교
        passed_count = 0
        failed_files = []
        
        for client_file in client_hashes:
            filename = client_file['filename']
            client_hash = client_file['hash']
            
            # 서버 해시값 찾기
            server_file = next((f for f in server_data['files'] if f['filename'] == filename), None)
            
            if server_file and server_file['hash'] == client_hash:
                passed_count += 1
            else:
                failed_files.append(filename)
        
        # 5. 결과 로깅
        if passed_count == len(client_hashes):
            log_integrity(f"검사 결과: 성공 ({passed_count}/{len(client_hashes)} 통과)")
            log_integrity(f"클라이언트 승인: {client_id}")
            result = {"status": "success", "message": "무결성 검사 통과"}
        else:
            log_integrity(f"검사 결과: 실패 ({passed_count}/{len(client_hashes)} 통과, {', '.join(failed_files)} 불일치)")
            log_integrity(f"클라이언트 거부: {client_id} (무결성 검사 실패)")
            result = {"status": "failed", "message": f"무결성 검사 실패: {', '.join(failed_files)}"}
        
        # 6. 암호화된 응답 반환
        encrypted_response = encrypt_integrity_response(json.dumps(result))
        log_integrity(f"응답 암호화 완료: {len(encrypted_response)} 문자")
        log_integrity(f"응답 내용: {json.dumps(result)}")
        return {"encrypted_data": encrypted_response}
        
    except HTTPException:
        raise
    except Exception as e:
        log_integrity(f"무결성 검사 오류: {str(e)}")
        raise HTTPException(status_code=500, detail="무결성 검사 실패")

@app.post("/heartbeat")
async def receive_heartbeat(data: Heartbeat, request: Request):
    """기존 하트비트 엔드포인트 (호환성용)"""
    client_ip = request.client.host
    log_line = f"IP={client_ip}, ClientID={data.client_id}, Timestamp={data.timestamp}, Version={data.version}"
    
    heartbeat_log_file_path = os.path.join(LOGS_BASE_PATH, "heartbeat.log")
    os.makedirs(os.path.dirname(heartbeat_log_file_path), exist_ok=True)
    
    with open(heartbeat_log_file_path, "a", encoding="utf-8") as f:
        f.write(log_line + "\n")
        
    print(f"레거시 하트비트 수신: {log_line}")
    return {"status": "ok"}

def read_log_file(log_type: str):
    """로그 파일 읽기"""
    log_file_path = os.path.join(LOGS_BASE_PATH, f"{log_type}.log")
    
    if not os.path.exists(log_file_path):
        raise HTTPException(status_code=404, detail=f"Log file for {log_type} not found at {log_file_path}")
    
    try:
        with open(log_file_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
            return "".join(lines[-500:]) 
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error reading log file: {e}")

@app.get("/logs/heartbeat", response_class=PlainTextResponse)
async def get_heartbeat_logs():
    """하트비트 로그 반환"""
    return read_log_file("heartbeat")

@app.get("/logs/behavior", response_class=PlainTextResponse)
async def get_behavior_logs():
    """행위기반 로그 반환"""
    return read_log_file("behavior")

@app.get("/logs/integrity", response_class=PlainTextResponse)
async def get_integrity_logs():
    """무결성검사 로그 반환"""
    return read_log_file("integrity")

@app.get("/stats")
async def get_stats():
    """서버 통계 정보"""
    return {
        "active_challenges": len(active_challenges),
        "server_time": int(time.time()),
        "challenge_algorithms": ["sha512"]
    }

@app.get("/admin")
async def admin_page(request: Request):
    """관리자 페이지 - 쿠키 기반 인증"""
    auth_cookie = request.cookies.get("admin_auth")
    
    if auth_cookie != "admin1234":
        return RedirectResponse(url="/", status_code=302)
    
    return FileResponse("/var/www/html/admin.html")

@app.post("/login")
async def login(request: Request):
    """로그인 API - 쿠키 설정"""
    try:
        form_data = await request.form()
        username = form_data.get("username")
        password = form_data.get("password")
        
        if username == "admin" and password == "1234":
            response = RedirectResponse(url="/admin", status_code=302)
            response.set_cookie(key="admin_auth", value="admin1234", max_age=86400, httponly=True, secure=True)
            return response
        else:
            return RedirectResponse(url="/?error=invalid", status_code=302)
    except Exception as e:
        return RedirectResponse(url="/?error=server", status_code=302)

# 실행 예시: uvicorn server:app --host 0.0.0.0 --port 8000 
# 🎮 GameAnalysis

## 📋 프로젝트 개요

**게임해체분석기**는 화이트햇 스쿨 3기 팀 프로젝트에서 게임보안을 주제로 결성된 팀입니다.


### 🎯 분석 대상 게임
**Lethal Company** - Unity 기반 멀티플레이어 서바이벌 호러 게임
<img width="600" height="256" alt="image" src="https://github.com/user-attachments/assets/4d488c9a-0f81-4b98-b459-3b90ca921a10" />


### 📅 프로젝트 기간
**2024년 5월 1일(목) ~ 8월 2일(토)**


### 🎯 프로젝트 목표
- **리버스엔지니어링**을 통한 게임 구조 및 메모리 분석
- 게임 핵의 작동 원리 파악 및 실제 핵 제작을 통한 학습
- 분석 결과를 바탕으로 한 **안티치트 시스템** 개발 및 검증
- 

### 🌐 Live Dashboard
**[대시보드 바로가기](https://ghb.r-e.kr)**


*관리자 계정: admin / 1234*


## 👥 팀 구성

**8명의 팀원으로 구성된 게임보안 팀**


### 🤝 협업 방식
- **GitHub**: 코드 버전 관리 및 협업
- **Discord**: 실시간 커뮤니케이션
- **Offline/Online**: 하이브리드 미팅


### 👨‍💻 팀원
| Name | Role |
|------|------|
| **이지훈** | PM, Anti-Debugging, Process Watcher, GUI Client |
| **임준서** | Anti-Cheat Server, Integrity Check, Dashboard Logging |
| **이재은** | Behavior-based Detection |
| **소예나** | DLL Injection Detection, Anti-Debugging |
| **장준우** | Reflection-based IL Code Verification, AppDomain Integrity Check |
| **남기찬** | Behavior-based Detection |
| **이서준** | DLL Injection Detection, Harmony Patch Detection |
| **황성하** | YARA-based Process Watcher, Heartbeat Monitoring |





## 🏗️ 프로젝트 구조

### 🔄 시스템 아키텍처

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Game Client   │    │  Anti-Cheat     │    │   Ubuntu        │
│                 │    │  DLL            │    │   Server        │
│  MainForm.cs    │◄──►│  (Injected)     │◄──►│   FastAPI       │
│  Console        │    │  Pipe Listener  │    │   + Uvicorn     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                       │
                                ▼                       ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │  Log Collection │    │   Nginx         │
                       │  & Processing   │    │   (TLS/443)     │
                       └─────────────────┘    └─────────────────┘
```

### 📁 폴더 구조
```
GameAnalysis/
├── AntiCheat/
│   ├── Client_Lethal_Anti_Cheat/     # GUI Client (.exe)
│   │   ├── MainForm.cs               # 메인 콘솔 창
│   │   ├── Integrity/                # 무결성 검사
│   │   └── Util/                     # 유틸리티
│   ├── Lethal_Anti_Cheat/            # DLL (.dll)
│   │   ├── BehaviourAntiCheats/      # 행동 기반 탐지
│   │   ├── DebugDetector/            # 디버거 탐지
│   │   └── ProcessWatcher/           # 프로세스 감시
│   └── Lethal_Anti_Debugging/        # 디버깅 방지 (.exe)
├── hack/                             # 해킹 도구
└── references/                       # 참고 자료
```

## 🔧 주요 기능

### 🛡️ Anti-Cheat Core Features
- **Anti-Debugging**: 디버거 탐지 및 방지
- **Process Watcher**: 프로세스 모니터링 및 감시
- **Behavior-based Detection**: 행동 기반 탐지 시스템
- **Client-Server Heartbeat**: 실시간 클라이언트 상태 모니터링
- **Integrity Check**: 핵심 DLL,exe 파일들 해시기반 무결성 검사
- **Admin Dashboard Monitoring**: 관리자 웹 대시보드를 통한 실시간 로그 모니터링

### 🔍 Advanced Detection
- **DLL Injection Detection**: DLL 인젝션 탐지
- **Reflection-based IL Code Verification**: IL 코드 검증
- **AppDomain Integrity Check**: AppDomain 무결성 검사
- **Harmony Patch Detection**: Harmony 패치 탐지
- **YARA-based Process Watcher**: YARA 규칙 기반 프로세스 감시

## 🚀 설치 및 실행

## 📊 결과

## 🛠️ 기술 스택

## 📄 라이선스

---

### 🙏 Special Thanks
- **PM, PL**: 프로젝트 지도 및 멘토링
- **소연님**: 아낌없는 지원

**화이트햇 스쿨 3기 (WHS3) 팀프로젝트**

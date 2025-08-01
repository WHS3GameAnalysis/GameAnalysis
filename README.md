<div align="center">
  <h1>🎮 GameAnalysis</h1>
  <p1>게임과 핵을 분석하여 안티치트를 제작하는 TEAM 게임해체분석기</p1>
</div>

<br/>

<div align="center">
  <img width="500" height="456" alt="logo4" src="https://github.com/user-attachments/assets/ff5b76af-2c8c-4489-a15f-e4447f41aca6" />
</div>

<br/>



## 📌 프로젝트 개요

- **프로젝트명**: GameAnalysis (게임해체분석기)
- **프로젝트 기간**: 2025.05.01 ~ 2025.08.02
- **프로젝트 형태**: 화이트햇 스쿨 3기 팀 프로젝트
- **주요 타겟**: <img src="https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white" height="14"/> Lethal Company - 1인칭 4인 협동 공포 서바이벌 게임
- **목표**: 
  1. **게임 구조 분석**: Lethal Company 게임의 리버스엔지니어링을 통한 게임 구조 분석
  2. **핵 분석 및 제작**: 실제 쓰이고 있는 핵을 분석하고 학습 목적으로 핵 제작
  3. **안티치트 시스템 개발**: 제작을 통해 학습한 지식을 기반으로 종합적인 안티치트 시스템 개발



## 👥 팀 소개

### 팀 구성원 및 역할
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

### 🤝 협업 방식
- **GitHub**: 코드 버전 관리 및 협업
- **Discord**: 실시간 커뮤니케이션
- **Notion**: 분석 결과 공유 및 문서 관리
- **Offline/Online**: 하이브리드 미팅
- **매주 정기적인 스크럼 미팅**: 분석 결과 공유, 진행상황 점검 및 멘토링

## 📁 프로젝트 구조

### ⚙️ 안티치트 시스템 아키텍처
<img width="1811" height="716" alt="image" src="https://github.com/user-attachments/assets/f6735d5a-a779-43ea-b9e5-80496ea12444" />

### 📁 폴더 구조
```
GameAnalysis/
├── AntiCheat/
│
│   ├── Client_Lethal_Anti_Cheat/          # 클라이언트 GUI (윈도우 폼 기반 실행 파일)
│   │   ├── app.manifest                   # 관리자 권한 실행 설정
│   │   ├── Program.cs                     # 프로그램 진입점
│   │   ├── MainForm.cs                    # 메인 폼
│   │   │   ├── MainForm.Designer.cs
│   │   │   └── MainForm.resx
│   │   ├── Resources/                     # 외부 라이브러리
│   │   │   └── SharpMonoInjector.dll      # DLL 인젝션 도구
│   │   ├── Integrity/                     # 무결성 검사 모듈
│   │   │   ├── FileHashUtil.cs
│   │   │   ├── IntegrityChecker.cs
│   │   │   ├── IntegrityResult.cs
│   │   │   └── ServerHashManager.cs
│   │   └── Util/                          # 유틸 및 보조 관리
│   │       ├── BehaviorLogManager.cs      # 서버와 통신 (로그 수집)
│   │       ├── HeartbeatManager.cs        # 서버와 통신 (하트비트)
│   │       ├── HwidUtil.cs
│   │       ├── InjectorManager.cs
│   │       ├── LogManager.cs
│   │       ├── PipeListener.cs
│   │       ├── RsaKey.cs                  # 서버와 통신 (암호화)
│   │       ├── SecurityUtil.cs            # 서버와 통신 (보안)
│   │       └── SimpleACManager.cs
│
│   ├── Lethal_Anti_Cheat/                 # DLL 기반 백그라운드 안티치트 (Injection DLL)
│   │   ├── BehaviourAntiCheats/          # 게임 내 치트행위 탐지
│   │   │   ├── DamageHack.cs
│   │   │   ├── GodMode.cs
│   │   │   ├── InfinityStamina.cs
│   │   │   ├── SuperJumpAndFastClimb.cs
│   │   │   └── TeleportCheck.cs
│   │   ├── BehaviourCore/                # 안티치트 핵심 로직
│   │   │   ├── AntiCheatUtils.cs
│   │   │   ├── AntiManager.cs
│   │   │   └── MessageUtils.cs
│   │   ├── DebugDetector/                # 디버거 탐지 기능
│   │   │   ├── DebugDetector.cs
│   │   │   ├── IDebugCheck.cs
│   │   │   ├── MonoDebuggerAttachCheck.cs
│   │   │   ├── MonoPortScanCheck.cs
│   │   │   └── RemoteDebuggerCheck.cs
│   │   ├── DLLDetector/                  # 외부 DLL 감지
│   │   │   ├── AllowList.cs
│   │   │   ├── CheckDLL.cs
│   │   │   └── CheckSignature.cs
│   │   ├── HarmonyPatchDetector/         # Harmony 패치 기반 훅 탐지
│   │   │   └── HarmonyPatchDetector.cs
│   │   ├── ProcessWatcher/               # 프로세스 기반 탐지
│   │   │   ├── NtProcessScanner.cs
│   │   │   └── ProcessWatcher.cs
│   │   ├── Reflection/                   # 리플렉션/도메인 우회 탐지
│   │   │   ├── AppDomainModuleScanner.cs
│   │   │   ├── HashDumper.cs
│   │   │   ├── ReflectionDetector.cs
│   │   │   └── SandboxAppDomain.cs
│   │   ├── Util/                         # 보조 유틸리티
│   │   │   ├── ConsoleManager.cs
│   │   │   ├── NativeMethods.cs
│   │   │   ├── PipeLogger.cs
│   │   │   └── UnifiedScanner.cs
│   │   └── Loader.cs                    # 진입 로직 (DLL EntryPoint)
│
│   └── Lethal_Anti_Debugging/            # 디버깅 방지용 별도 프로그램 (.exe)
│       ├── dllmain.cpp
│       ├── Logger.cpp
│       ├── ProcessScanner.cpp
│       ├── Logger.h
│       └── ProcessScanner.h
│
├── hack/                                 # 해킹 도구 샘플 혹은 테스트용
│
└── references/                           # 문서, YARA 규칙, 샘플, 서명 정보 등 외부 참고 자료

```

## ⚡ 주요 기능
<img width="900" height="611" alt="image" src="https://github.com/user-attachments/assets/692714e1-fa12-4eb6-9fce-9d86ebf67b70" />

### 🛡️ Anti-Cheat Core Features
- **Anti-Debugging**: 디버거 탐지 및 방지
- **Process Watcher**: 프로세스 모니터링 및 감시
- **Behavior-based Detection**: 행동 기반 탐지 시스템
- **Client-Server Heartbeat**: 실시간 클라이언트 상태 모니터링
- **Integrity Check**: 핵심 DLL,exe 파일들 해시기반 서버사이드 무결성 검사
- **Admin Dashboard Monitoring**: 관리자 웹 대시보드를 통한 실시간 로그 모니터링

### 🛡️ Advanced Anti-Cheat Detection
- **DLL Injection Detection**: DLL 인젝션 탐지
- **Reflection-based IL Code Verification**: IL 코드 검증
- **AppDomain Integrity Check**: AppDomain 무결성 검사
- **Harmony Patch Detection**: Harmony 패치 탐지
- **YARA-based Process Watcher**: YARA 규칙 기반 프로세스 감시

## ⚙️ 기술 스택

<table>
  <thead>
    <tr>
      <th>분류</th>
      <th>기술 스택</th>
    </tr>
  </thead>
  <tbody>
         <tr>
       <td>Analysis Tools</td>
       <td>
         <img src="https://img.shields.io/badge/dnSpy-000000?style=flat&logo=dnspy&logoColor=white"/>
         <img src="https://img.shields.io/badge/Cheat%20Engine-000000?style=flat&logo=cheatengine&logoColor=white"/>
         <img src="https://img.shields.io/badge/x64dbg-000000?style=flat&logo=x64dbg&logoColor=white"/>
         <img src="https://img.shields.io/badge/IDA-000000?style=flat&logo=ida&logoColor=white"/>
       </td>
     </tr>
    <tr>
      <td>Anti-Cheat Client</td>
      <td>
        <img src="https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp&logoColor=white"/>
        <img src="https://img.shields.io/badge/.NET%208-512BD4?style=flat&logo=.net&logoColor=white"/>
        <img src="https://img.shields.io/badge/C%2B%2B-00599C?style=flat&logo=c%2B%2B&logoColor=white"/>
        <img src="https://img.shields.io/badge/Windows%20Forms-0078D4?style=flat&logo=windows&logoColor=white"/>
                 <img src="https://img.shields.io/badge/SharpMonoInjector-8A2BE2?style=flat&logo=sharpmonoinjector&logoColor=white"/>
      </td>
    </tr>
    <tr>
      <td>Anti-Cheat Server</td>
      <td>
        <img src="https://img.shields.io/badge/Python-3776AB?style=flat&logo=python&logoColor=white"/>
        <img src="https://img.shields.io/badge/FastAPI-009688?style=flat&logo=fastapi&logoColor=white"/>
        <img src="https://img.shields.io/badge/Uvicorn-059669?style=flat&logo=uvicorn&logoColor=white"/>
        <img src="https://img.shields.io/badge/Nginx-009639?style=flat&logo=nginx&logoColor=white"/>
        <img src="https://img.shields.io/badge/Pydantic-920000?style=flat&logo=pydantic&logoColor=white"/>
                 <img src="https://img.shields.io/badge/Cryptography-2F4F4F?style=flat&logo=cryptography&logoColor=white"/>
      </td>
    </tr>
                   <tr>
        <td>Dashboard</td>
        <td>
          <img src="https://img.shields.io/badge/HTML5-E34F26?style=flat&logo=html5&logoColor=white"/>
          <img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=flat&logo=javascript&logoColor=black"/>
          <img src="https://img.shields.io/badge/Tailwind%20CSS-06B6D4?style=flat&logo=tailwindcss&logoColor=white"/>
        </td>
      </tr>
         <tr>
       <td>Infrastructure & Cloud</td>
       <td>
         <img src="https://img.shields.io/badge/Google%20Cloud-4285F4?style=flat&logo=googlecloud&logoColor=white"/>
         <img src="https://img.shields.io/badge/Ubuntu-E95420?style=flat&logo=ubuntu&logoColor=white"/>
                   <img src="https://img.shields.io/badge/HTTPS-009639?style=flat&logo=https&logoColor=white"/>
         <img src="https://img.shields.io/badge/Let's%20Encrypt-003A70?style=flat&logo=letsencrypt&logoColor=white"/>
       </td>
     </tr>
    <tr>
      <td>Security & Libraries</td>
      <td>
        <img src="https://img.shields.io/badge/RSA-000000?style=flat&logo=rsa&logoColor=white"/>
        <img src="https://img.shields.io/badge/AES-000000?style=flat&logo=aes&logoColor=white"/>
        <img src="https://img.shields.io/badge/SHA256-000000?style=flat&logo=sha256&logoColor=white"/>
        <img src="https://img.shields.io/badge/YARA-000000?style=flat&logo=yara&logoColor=white"/>
        <img src="https://img.shields.io/badge/Harmony-000000?style=flat&logo=harmony&logoColor=white"/>
      </td>
    </tr>
  </tbody>
</table>

## 🌐 대시보드
**[대시보드 바로가기](https://ghb.r-e.kr)**

*관리자 계정: admin / 1234*

---

### 🙏 Support
- **PM, PL**: 프로젝트 지도 및 멘토링
- **MAXMINY**

**화이트햇 스쿨 3기 (WHS3) 팀프로젝트**

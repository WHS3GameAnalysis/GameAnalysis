# GameAnalysis - 게임 해체 분석기 (WHS3)

화이트햇 스쿨 3기 팀프로젝트 - 게임 해체 분석기

## 📁 프로젝트 구조

```
GameAnalysis/
├── README.md                   # 프로젝트 README
├── docs/                       # 프로젝트 문서화
│   ├── .../              
├── references/                 # 핵/치트 레퍼런스 및 연구 자료
│   ├── .../
├── analysis-tools/             # 분석 도구 및 스크립트
│   ├── .../
├── reports/                    # 분석 보고서 및 결과
│   ├── .../
```

## 📋 다른 repo를 레퍼런스로 추가 하는 방법

### 1. Git Submodule 추가하기

```bash
# references 폴더 안에 submodule 추가
git submodule add <레포지토리_URL> references/<폴더명>

# 현재 추가된 예시: LethalMenu (게임 치트 메뉴)
# git submodule add https://github.com/IcyRelic/LethalMenu references/LethalMenu

# 추가 예시들:
# 치트엔진 관련 레포지토리 추가
git submodule add https://github.com/cheat-engine/cheat-engine.git references/cheat-engine

# 게임 해킹 도구 레포지토리 추가  
git submodule add https://github.com/example/game-hacking-tools.git references/game-hacking-tools

# 안티치트 우회 연구 레포지토리 추가
git submodule add https://github.com/example/anti-cheat-bypass.git references/anti-cheat-bypass
```

### 2. Submodule 초기화 및 업데이트

```bash
# 레퍼런스 서브모듈 초기화 (references 폴더 내 repo들 다운로드 받아짐)
git submodule update --init --recursive

# 모든 submodule을 최신 버전으로 업데이트
git submodule update --remote

# 특정 submodule만 업데이트
git submodule update --remote references/<폴더명>
```

### 3. Submodule 제거하기

```bash
# submodule 제거 (필요시)
git submodule deinit references/<폴더명>
git rm references/<폴더명>
rm -rf .git/modules/references/<폴더명>
```

### 4. 팀원들이 프로젝트를 클론할 때

```bash
# 프로젝트를 클론하면서 submodule도 함께 받기
git clone --recursive <이_프로젝트_URL>

# 또는 이미 클론한 후에 submodule 받기
git clone <이_프로젝트_URL>
cd GameAnalysis
git submodule update --init --recursive
```

## 👥 팀 정보

화이트햇 스쿨 3기 (WHS3) 팀프로젝트

## 📄 라이선스

이 프로젝트는 교육 및 연구 목적으로만 사용됩니다.
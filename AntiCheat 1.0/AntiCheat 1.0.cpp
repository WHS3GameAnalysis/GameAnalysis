// AntiCheat 1.0.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "AntiCheat 1.0.h"
#include "src/integrity_checker.h"
#include <string>
#include <vector>
#include <thread>

#define MAX_LOADSTRING 100
#define WM_INTEGRITY_CHECK_COMPLETE (WM_USER + 1)

// 전역 변수:
HINSTANCE hInst;                                // 현재 인스턴스입니다.
WCHAR szTitle[MAX_LOADSTRING];                  // 제목 표시줄 텍스트입니다.
WCHAR szWindowClass[MAX_LOADSTRING];            // 기본 창 클래스 이름입니다.
HWND g_hLogEdit = NULL;                         // 로그 표시용 에디트 컨트롤
HWND g_hMainWnd = NULL;                         // 메인 윈도우 핸들
IntegrityChecker g_integrityChecker;            // 무결성 검사기
std::vector<std::string> g_logMessages;         // 로그 메시지들
bool g_isChecking = false;                      // 검사 중인지 여부

// 이 코드 모듈에 포함된 함수의 선언을 전달합니다
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);

// 무결성 검사 관련 함수들
void                AddLogMessage(const std::string& message);
void                StartIntegrityCheck();
DWORD WINAPI        IntegrityCheckThread(LPVOID lpParam);
void                OnIntegrityCheckComplete(bool success, const std::string& message);
void                OnFileCheckProgress(int current, int total, const std::string& filename, const std::string& status);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // 전역 문자열을 초기화합니다.
    LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_ANTICHEAT10, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // 애플리케이션 초기화를 수행합니다
    if (!InitInstance (hInstance, nCmdShow))
    {
        return FALSE;
    }

    MSG msg;

    // 기본 메시지 루프
    while (GetMessage(&msg, nullptr, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return (int) msg.wParam;
}



//  함수: MyRegisterClass()
//  창 클래스를 등록
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style          = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc    = WndProc;
    wcex.cbClsExtra     = 0;
    wcex.cbWndExtra     = 0;
    wcex.hInstance      = hInstance;
    wcex.hIcon          = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_ANTICHEAT10));
    wcex.hCursor        = LoadCursor(nullptr, IDC_ARROW);
    wcex.hbrBackground  = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName   = NULL;
    wcex.lpszClassName  = szWindowClass;
    wcex.hIconSm        = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

//
//   함수: InitInstance(HINSTANCE, int)
//
//   용도: 인스턴스 핸들을 저장하고 주 창을 생성
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   hInst = hInstance; // 인스턴스 핸들을 전역 변수에 저장합니다.

   HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
      800, 600, 800, 600, nullptr, nullptr, hInstance, nullptr);

   if (!hWnd)
   {
      return FALSE;
   }

   g_hMainWnd = hWnd;
   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   // 윈도우 생성 후 자동으로 무결성 검사 시작
   StartIntegrityCheck();

   return TRUE;
}

//
//  함수: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  용도: 주 창의 메시지를 처리
//
//  WM_COMMAND  - 애플리케이션 메뉴를 처리
//  WM_PAINT    - 주 창을 그립니다.
//  WM_DESTROY  - 종료 메시지를 게시하고 반환합니다.
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_CREATE:
        {
            // 로그 표시용 에디트 컨트롤 생성
            g_hLogEdit = CreateWindowW(L"EDIT", L"",
                WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | ES_READONLY,
                10, 10, 760, 540, hWnd, NULL, hInst, NULL);
            
            // 폰트 설정
            HFONT hFont = CreateFontW(16, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE,
                DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, L"Consolas");
            SendMessage(g_hLogEdit, WM_SETFONT, (WPARAM)hFont, TRUE);
        }
        break;
    case WM_COMMAND:
        {
            int wmId = LOWORD(wParam);
            // 메뉴 선택을 구문 분석합니다
            switch (wmId)
            {
            default:
                return DefWindowProc(hWnd, message, wParam, lParam);
            }
        }
        break;
    case WM_INTEGRITY_CHECK_COMPLETE:
        {
            bool success = (wParam != 0);
            std::string* message = reinterpret_cast<std::string*>(lParam);
            OnIntegrityCheckComplete(success, *message);
            delete message;
        }
        break;

    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}



// 현재 시간을 문자열로 가져오기
std::string GetCurrentTimeString() {
    SYSTEMTIME st;
    GetLocalTime(&st);
    
    char timeStr[20];
    sprintf_s(timeStr, "%02d:%02d:%02d", st.wHour, st.wMinute, st.wSecond);
    return std::string(timeStr);
}

// 로그 메시지 추가
void AddLogMessage(const std::string& message) {
    std::string timeMessage = GetCurrentTimeString() + ": " + message;
    g_logMessages.push_back(timeMessage);
    
    // 에디트 컨트롤에 메시지 추가
    if (g_hLogEdit) {
        // 현재 텍스트 길이 구하기
        int length = GetWindowTextLengthA(g_hLogEdit);
        
        // 새 메시지 추가
        SendMessageA(g_hLogEdit, EM_SETSEL, length, length);
        SendMessageA(g_hLogEdit, EM_REPLACESEL, FALSE, (LPARAM)(timeMessage + "\r\n").c_str());
        
        // 스크롤을 맨 아래로
        SendMessage(g_hLogEdit, EM_SCROLLCARET, 0, 0);
    }
}



// 파일 검사 진행률 콜백 함수
void OnFileCheckProgress(int current, int total, const std::string& filename, const std::string& status) {
    std::string progress = "[" + std::to_string(current) + "/" + std::to_string(total) + "] ";
    
    // 검사 완료 결과만 로그 출력
    AddLogMessage(progress + filename + " - " + status);
}

// 무결성 검사 시작
void StartIntegrityCheck() {
    if (g_isChecking) return;
    
    g_isChecking = true;
    AddLogMessage("=== Lethal Company AntiCheat v1.0 ===");
    AddLogMessage("게임 버전: 리썰컴퍼니 v72 기준");
    
    // 콜백 함수 설정
    g_integrityChecker.SetProgressCallback(OnFileCheckProgress);
    
    // 별도 스레드에서 검사 실행
    CreateThread(NULL, 0, IntegrityCheckThread, NULL, 0, NULL);
}

// 무결성 검사 스레드
DWORD WINAPI IntegrityCheckThread(LPVOID lpParam) {
    AddLogMessage("안티치트 시작...");
    // 게임 경로 확인
    std::string gamePath = g_integrityChecker.GetGamePath();
    if (gamePath.empty()) {
        AddLogMessage("✗ 오류: 리썰 컴퍼니 경로를 찾을 수 없습니다.");
        std::string* message = new std::string("게임 경로를 찾을 수 없음");
        PostMessage(g_hMainWnd, WM_INTEGRITY_CHECK_COMPLETE, 0, (LPARAM)message);
        g_isChecking = false;
        return 0;
    }
    
    AddLogMessage("게임 경로: " + gamePath);
    AddLogMessage("무결성 검사 시작...");
    
    // 무결성 검사 수행
    IntegrityResult result = g_integrityChecker.CheckIntegrity();
    
    // 상세 결과 로그 추가
    AddLogMessage("=== 검사 완료 ===");
    AddLogMessage("검사 파일: " + std::to_string(result.total_files) + "개");
    AddLogMessage("정상 파일: " + std::to_string(result.passed_files) + "개");
    AddLogMessage("변조 파일: " + std::to_string(result.failed_files_count) + "개");
    
    if (result.is_valid) {
        AddLogMessage("무결성 검사 통과");
        
        // 게임 실행
        if (g_integrityChecker.LaunchGame()) {
            AddLogMessage("게임 실행 중...");
            Sleep(3000);
            PostMessage(g_hMainWnd, WM_CLOSE, 0, 0);
        } else {
            AddLogMessage("게임 실행 실패");
        }
    } else {
        AddLogMessage("무결성 검사 실패");
        AddLogMessage("=== 변조된 파일 ===");
        for (const auto& failed_file : result.failed_files) {
            AddLogMessage("  " + failed_file);
        }
    }
    
    // 결과를 메인 스레드로 전송
    std::string* message = new std::string(result.message);
    PostMessage(g_hMainWnd, WM_INTEGRITY_CHECK_COMPLETE, result.is_valid ? 1 : 0, (LPARAM)message);
    
    g_isChecking = false;
    return 0;
}

// 무결성 검사 완료 처리
void OnIntegrityCheckComplete(bool success, const std::string& message) {
    if (success) {
        AddLogMessage("=== 최종 결과 ===");
        AddLogMessage("무결성 검사 통과");
        AddLogMessage("게임 실행 완료");
        AddLogMessage("런처 종료...");
    } else {
        AddLogMessage("=== 최종 결과 ===");
        AddLogMessage("무결성 검사 실패");
        AddLogMessage("게임 실행 차단");
        AddLogMessage(message);
    }
}

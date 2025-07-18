#include <Windows.h>
#include <iostream>
#include <string>

// 메시지박스를 표시하는 쓰레드 함수
// 메시지박스를 표시하고 메모리 할당을 수행하는 쓰레드 함수
DWORD WINAPI MessageThreadProc(LPVOID param) {
    // 힙 메모리 할당
    HANDLE hHeap = GetProcessHeap();
    void* heap_mem = HeapAlloc(hHeap, 0, 1024);
    
    // VirtualAlloc 메모리 할당
    void* virtual_mem = VirtualAlloc(
        NULL,
        4096,
        MEM_COMMIT | MEM_RESERVE,
        PAGE_READWRITE
    );

    MessageBoxW(NULL, L"SimpleAC 테스트 메시지", L"알림", MB_OK | MB_ICONINFORMATION);

    // 메모리 해제
    if (heap_mem) {
        HeapFree(hHeap, 0, heap_mem);
    }
    if (virtual_mem) {
        VirtualFree(virtual_mem, 0, MEM_RELEASE);
    }

    return 0;
}

// 5초마다 새로운 쓰레드 생성하는 쓰레드 함수
DWORD WINAPI ThreadCreatorProc(LPVOID param) {
    while (true) {
        HANDLE hThread = CreateThread(
            NULL,                   // 보안 속성
            0,                      // 스택 크기
            MessageThreadProc,      // 쓰레드 함수
            NULL,                   // 쓰레드 파라미터
            0,                      // 생성 플래그
            NULL                    // 쓰레드 ID
        );

        if (hThread) {
            CloseHandle(hThread);
        }
        else {
            std::wcout << L"메시지 쓰레드 생성 실패" << std::endl;
        }

        Sleep(5000); // 5초 대기
    }
    return 0;
}

int main()
{
    // 콘솔 설정
    SetConsoleOutputCP(CP_UTF8);
    setlocale(LC_ALL, "");
    
    std::wcout << L"=== SimpleAC 테스트 애플리케이션 ===" << std::endl;
    std::wcout << L"DLL 로딩 중..." << std::endl;
    
    // DLL 로드
    HMODULE hDll = LoadLibraryW(L"SimpleAC.dll");
    if (hDll == NULL)
    {
        DWORD error = GetLastError();
        std::wcout << L"DLL 로드 실패. 오류 코드: " << error << std::endl;
        std::wcout << L"SimpleAC.dll 파일이 같은 폴더에 있는지 확인하세요." << std::endl;
        
        std::wcout << L"아무 키나 누르면 종료됩니다..." << std::endl;
        std::cin.get();
        return 1;
    }
    
    std::wcout << L"DLL 로드 성공!" << std::endl;

    // 쓰레드 생성기 쓰레드 시작
    HANDLE hCreatorThread = CreateThread(
        NULL,
        0,
        ThreadCreatorProc,
        NULL,
        0,
        NULL
    );

    if (hCreatorThread) {
        CloseHandle(hCreatorThread);
    }
    else {
        std::wcout << L"쓰레드 생성기 쓰레드 생성 실패" << std::endl;
    }
  
	std::cin.get(); // 사용자 입력 대기

    FreeLibrary(hDll);
    
    std::wcout << L"프로그램이 종료됩니다." << std::endl;
    Sleep(2000);
    
    return 0;
} 
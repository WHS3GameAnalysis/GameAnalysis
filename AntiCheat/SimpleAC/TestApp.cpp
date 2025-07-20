#include <Windows.h>
#include <iostream>
#include <string>

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
  
	std::cin.get(); // 사용자 입력 대기

    FreeLibrary(hDll);
    
    std::wcout << L"프로그램이 종료됩니다." << std::endl;
    Sleep(2000);
    
    return 0;
} 
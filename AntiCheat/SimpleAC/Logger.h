#pragma once

#include <Windows.h>
#include <string>
#include <iostream>
#include <sstream>
#include <ctime>
#include <iomanip>
#include <mutex>

class Logger
{
public:
    // 로그 출력 함수
    static void Log(const std::wstring& message);
    static void Log(const std::string& message);
    
    // 포맷팅된 로그 출력
    static void LogF(const wchar_t* format, ...);
    static void LogF(const char* format, ...);
    
    // 에러 로그 출력
    static void LogError(const std::wstring& message);
    static void LogError(const std::string& message);
    
    // 경고 로그 출력
    static void LogWarning(const std::wstring& message);
    static void LogWarning(const std::string& message);
    
private:
    // 멀티스레드 동기화
    static std::mutex s_logMutex;
    
    // 현재 시간을 문자열로 변환
    static std::wstring GetCurrentTimeString();
    
    // 메시지 포맷팅
    static std::wstring FormatMessage(const std::wstring& level, const std::wstring& message);
}; 
#include "Logger.h"

// static 멤버 정의
std::mutex Logger::s_logMutex;

void Logger::Log(const std::wstring& message)
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    std::wstring formattedMessage = FormatMessage(L"INFO", message);
    std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
}

void Logger::Log(const std::string& message)
{
    std::wstring wideMessage(message.begin(), message.end());
    Log(wideMessage);
}

void Logger::LogF(const wchar_t* format, ...)
{
    va_list args;
    va_start(args, format);
    
    wchar_t buffer[1024];
    vswprintf_s(buffer, format, args);
    
    va_end(args);
    
    Log(std::wstring(buffer));
}

void Logger::LogF(const char* format, ...)
{
    va_list args;
    va_start(args, format);
    
    char buffer[1024];
    vsprintf_s(buffer, format, args);
    
    va_end(args);
    
    Log(std::string(buffer));
}

void Logger::LogError(const std::wstring& message)
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    std::wstring formattedMessage = FormatMessage(L"ERROR", message);
    std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
}

void Logger::LogError(const std::string& message)
{
    std::wstring wideMessage(message.begin(), message.end());
    LogError(wideMessage);
}

void Logger::LogWarning(const std::wstring& message)
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    std::wstring formattedMessage = FormatMessage(L"WARNING", message);
    std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
}

void Logger::LogWarning(const std::string& message)
{
    std::wstring wideMessage(message.begin(), message.end());
    LogWarning(wideMessage);
}

std::wstring Logger::GetCurrentTimeString()
{
    auto now = std::time(nullptr);
    std::tm localTime;
    localtime_s(&localTime, &now);
    
    std::wstringstream ss;
    ss << std::put_time(&localTime, L"%Y-%m-%d %H:%M:%S");
    return ss.str();
}

std::wstring Logger::FormatMessage(const std::wstring& level, const std::wstring& message)
{
    std::wstringstream ss;
    ss << L"[" << GetCurrentTimeString() << L"] [" << level << L"] " << message;
    return ss.str();
} 
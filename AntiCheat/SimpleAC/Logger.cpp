#include "Logger.h"

// static 멤버 정의
std::mutex Logger::s_logMutex;
HANDLE Logger::s_pipeHandle = INVALID_HANDLE_VALUE;
bool Logger::s_pipeInitialized = false;

void Logger::InitializePipe()
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    if (s_pipeInitialized)
        return;
    
    // 파이프 클라이언트로 연결
    s_pipeHandle = CreateFileW(
        L"\\\\.\\pipe\\AntiCheatPipe",
        GENERIC_WRITE,
        0,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr
    );
    
    if (s_pipeHandle != INVALID_HANDLE_VALUE)
    {
        s_pipeInitialized = true;
        OutputDebugStringW(L"[Logger] Pipe connection established\n");
    }
    else
    {
        OutputDebugStringW(L"[Logger] Failed to connect to pipe\n");
    }
}

void Logger::CleanupPipe()
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    if (s_pipeHandle != INVALID_HANDLE_VALUE)
    {
        CloseHandle(s_pipeHandle);
        s_pipeHandle = INVALID_HANDLE_VALUE;
    }
    s_pipeInitialized = false;
}

void Logger::SendToPipe(const std::wstring& message)
{
    if (!s_pipeInitialized || s_pipeHandle == INVALID_HANDLE_VALUE)
        return;
    
    // UTF-8로 변환
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, message.c_str(), -1, nullptr, 0, nullptr, nullptr);
    if (size_needed <= 0)
        return;
    
    std::string utf8_message(size_needed - 1, 0);
    WideCharToMultiByte(CP_UTF8, 0, message.c_str(), -1, &utf8_message[0], size_needed - 1, nullptr, nullptr);
    utf8_message += "\n";
    
    DWORD bytesWritten;
    WriteFile(s_pipeHandle, utf8_message.c_str(), static_cast<DWORD>(utf8_message.length()), &bytesWritten, nullptr);
}

void Logger::Log(const std::wstring& message)
{
    std::lock_guard<std::mutex> lock(s_logMutex);
    
    std::wstring formattedMessage = FormatMessage(L"INFO", message);
    //std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
    
    // 파이프로 전송
    SendToPipe(formattedMessage);
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
    //std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
    
    // 파이프로 전송
    SendToPipe(formattedMessage);
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
    //std::wcout << formattedMessage << std::endl;
    
    // 디버거 출력
    OutputDebugStringW((formattedMessage + L"\n").c_str());
    
    // 파이프로 전송
    SendToPipe(formattedMessage);
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
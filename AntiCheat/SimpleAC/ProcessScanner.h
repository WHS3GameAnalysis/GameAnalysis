#pragma once

#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <Windows.h>
#include <TlHelp32.h>
#include <Psapi.h>
#include <string>
#include <vector>
#include <memory>
#include <set>
#include <future>
#include <algorithm>
#include <functional>

// YARA 라이브러리 포함
#include <yara.h>

// 프로세스 정보 구조체 (멀티스레드 안전)
struct ProcessInfo
{
    DWORD processId;
    std::wstring processName;
    std::wstring executablePath;
    
    ProcessInfo() : processId(0) {}
    ProcessInfo(DWORD pid, const std::wstring& name, const std::wstring& path)
        : processId(pid), processName(name), executablePath(path) {}
};

class ProcessScanner
{
public:
    ProcessScanner();
    ~ProcessScanner();
    
    // 초기화 및 정리
    bool Initialize();
    void Cleanup();
    
    // 프로세스 스캔
    void ScanAllProcesses();
    
    // 특정 프로세스 스캔
    bool ScanProcess(const ProcessInfo& processInfo);
    
private:
    // YARA 관련 변수들
    YR_RULES* m_yaraRules;
    YR_COMPILER* m_yaraCompiler;
    
    bool m_initialized;
    std::wstring m_rulesPath;
    
    // 스캔한 프로세스 추적
    std::set<DWORD> m_scannedProcesses;
    
    // 프로세스 열거
    std::vector<ProcessInfo> EnumerateProcesses();
    
    // 프로세스 메모리 읽기
    bool ReadProcessMemory(HANDLE processHandle, LPCVOID address, SIZE_T size, std::vector<BYTE>& buffer);
    
    // 전체 프로세스 메모리 스캔
    bool ScanProcessMemory(HANDLE processHandle, const ProcessInfo& processInfo);
    
    // 단일 메모리 영역 스캔 (병렬 처리용)
    bool ScanMemoryRegion(HANDLE processHandle, const MEMORY_BASIC_INFORMATION& region, const ProcessInfo& processInfo);
    
    // YARA 규칙 로드
    bool LoadYaraRules();
    
    // YARA 콜백 함수
    static int YaraCallback(YR_SCAN_CONTEXT* context, int message, void* message_data, void* user_data);
    
    // 권한 상승
    bool EnableDebugPrivilege();
    
    // 프로세스 정보 가져오기
    bool GetProcessInfo(DWORD processId, ProcessInfo& processInfo);
    
    // 메모리 영역 정보 가져오기
    std::vector<MEMORY_BASIC_INFORMATION> GetMemoryRegions(HANDLE processHandle);
}; 
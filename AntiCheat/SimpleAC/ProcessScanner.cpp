#include "ProcessScanner.h"
#include "Logger.h"

ProcessScanner::ProcessScanner()
    : m_initialized(false), m_yaraRules(nullptr), m_yaraCompiler(nullptr)
{
    // YARA 규칙 파일 경로 설정
    m_rulesPath = L"malware_rules.yar";
}

ProcessScanner::~ProcessScanner()
{
    Cleanup();
}

bool ProcessScanner::Initialize()
{
    if (m_initialized)
        return true;
    
    Logger::Log(L"[ProcessScanner] Initialization started");
    
    // 디버그 권한 활성화
    if (!EnableDebugPrivilege())
    {
        Logger::LogWarning(L"[ProcessScanner] Failed to enable debug privilege - some processes may not be accessible");
    }
    
    // YARA 라이브러리 초기화
    int result = yr_initialize();
    if (result != ERROR_SUCCESS)
    {
        Logger::LogF(L"[ProcessScanner] YARA library initialization failed with error: %d", result);
        return false;
    }
    
    // YARA 규칙 로드
    if (!LoadYaraRules())
    {
        Logger::LogError(L"[ProcessScanner] YARA rules loading failed");
        yr_finalize();
        return false;
    }
    
    m_initialized = true;
    Logger::Log(L"[ProcessScanner] Initialization completed with real YARA engine");
    
    return true;
}

void ProcessScanner::Cleanup()
{
    if (!m_initialized)
        return;
    
    Logger::Log(L"[ProcessScanner] Cleanup started");
    
    // YARA 리소스 정리
    if (m_yaraRules != nullptr)
    {
        yr_rules_destroy(m_yaraRules);
        m_yaraRules = nullptr;
    }
    
    if (m_yaraCompiler != nullptr)
    {
        yr_compiler_destroy(m_yaraCompiler);
        m_yaraCompiler = nullptr;
    }
    
    yr_finalize();
    
    m_initialized = false;
    Logger::Log(L"[ProcessScanner] Cleanup completed");
}

void ProcessScanner::ScanAllProcesses()
{
    if (!m_initialized)
    {
        Logger::LogError(L"[ProcessScanner] Not Initialized");
        return;
    }
    
    Logger::Log(L"[ProcessScanner] Starting priority process scan...");
    
    int scannedCount = 0;
    int newProcessCount = 0;
    int existingProcessCount = 0;
    
    // 현재 실행 중인 모든 프로세스 열거
    std::vector<ProcessInfo> allProcesses = EnumerateProcesses();
    std::vector<ProcessInfo> newProcesses;
    std::vector<ProcessInfo> existingProcesses;
    
    // 새로운 프로세스와 기존 프로세스 분리
    for (const auto& processInfo : allProcesses)
    {
        // 시스템 프로세스들 제외
        if (processInfo.processId == 0 || processInfo.processId == 4)
            continue;
            
        if (m_scannedProcesses.find(processInfo.processId) == m_scannedProcesses.end())
        {
            newProcesses.push_back(processInfo);
        }
        else
        {
            existingProcesses.push_back(processInfo);
        }
    }
    
    // 1. 새로운 프로세스 우선 스캔
    for (const auto& processInfo : newProcesses)
    {
        if (ScanProcess(processInfo))
        {
            scannedCount++;
            newProcessCount++;
        }
        // 스캔 완료된 프로세스 추가
        m_scannedProcesses.insert(processInfo.processId);
    }
    
    // 2. 기존 프로세스 후순위 스캔
    for (const auto& processInfo : existingProcesses)
    {
        if (ScanProcess(processInfo))
        {
            scannedCount++;
            existingProcessCount++;
        }
    }
    
    // 더 이상 실행되지 않는 프로세스들을 추적 목록에서 제거
    std::set<DWORD> currentProcessIds;
    for (const auto& processInfo : allProcesses)
    {
        currentProcessIds.insert(processInfo.processId);
    }
    
    auto it = m_scannedProcesses.begin();
    while (it != m_scannedProcesses.end())
    {
        if (currentProcessIds.find(*it) == currentProcessIds.end())
        {
            it = m_scannedProcesses.erase(it);
        }
        else
        {
            ++it;
        }
    }
    
    Logger::LogF(L"[ProcessScanner] Priority scan completed: %d total scanned (New: %d, Existing: %d)", 
                 scannedCount, newProcessCount, existingProcessCount);
}

bool ProcessScanner::ScanProcess(const ProcessInfo& processInfo)
{
    // 프로세스 핸들 열기
    HANDLE processHandle = OpenProcess(
        PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
        FALSE,
        processInfo.processId
    );
    
    if (processHandle == NULL)
    {
        return false;
    }
    
    // Logger::LogF(L"[ProcessScanner] Process scan started: %s (PID: %d)", 
    //             processInfo.processName.c_str(), processInfo.processId);
    
    bool result = false;
    
    try
    {
        // 프로세스 메모리 스캔 (핸들을 전달)
        result = ScanProcessMemory(processHandle, processInfo);
        //Logger::LogF(L"[ProcessScanner] Process scan completed: %s", processInfo.processName.c_str());
    }
    catch (...)
    {
        // 예외 발생 시에도 핸들 정리
        CloseHandle(processHandle);
        throw;
    }
    
    // 핸들 닫기
    CloseHandle(processHandle);
    
    return result;
}

std::vector<ProcessInfo> ProcessScanner::EnumerateProcesses()
{
    std::vector<ProcessInfo> processes;
    
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE)
    {
        Logger::LogError(L"[ProcessScanner] Failed to create process snapshot");
        return processes;
    }
    
    PROCESSENTRY32W pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32W);
    
    if (Process32FirstW(hSnapshot, &pe32))
    {
        do
        {
            ProcessInfo processInfo;
            if (GetProcessInfo(pe32.th32ProcessID, processInfo))
            {
                processes.push_back(std::move(processInfo));
            }
        } while (Process32NextW(hSnapshot, &pe32));
    }
    
    CloseHandle(hSnapshot);
    return processes;
}

bool ProcessScanner::GetProcessInfo(DWORD processId, ProcessInfo& processInfo)
{
    processInfo.processId = processId;
    
    // 프로세스 핸들 임시로 열기 (정보만 가져오기 위해)
    HANDLE tempHandle = OpenProcess(
        PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
        FALSE,
        processId
    );
    
    if (tempHandle == NULL)
    {
        // 권한 부족으로 열 수 없는 경우
        return false;
    }
    
    // 프로세스 이름 가져오기
    wchar_t processName[MAX_PATH] = { 0 };
    if (GetProcessImageFileNameW(tempHandle, processName, MAX_PATH) > 0)
    {
        std::wstring fullPath(processName);
        size_t pos = fullPath.find_last_of(L"\\");
        if (pos != std::wstring::npos)
        {
            processInfo.processName = fullPath.substr(pos + 1);
        }
        else
        {
            processInfo.processName = fullPath;
        }
        processInfo.executablePath = fullPath;
    }
    else
    {
        processInfo.processName = L"Unknown";
        processInfo.executablePath = L"Unknown";
    }
    
    // 임시 핸들 즉시 닫기
    CloseHandle(tempHandle);
    
    return true;
}

bool ProcessScanner::ScanMemoryRegion(HANDLE processHandle, const MEMORY_BASIC_INFORMATION& region, const ProcessInfo& processInfo)
{
    std::vector<BYTE> buffer;
    if (!ReadProcessMemory(processHandle, region.BaseAddress, region.RegionSize, buffer))
    {
        return false;
    }
    
    // 실제 YARA 스캔 실행
    int result = yr_rules_scan_mem(
        m_yaraRules,
        buffer.data(),
        buffer.size(),
        SCAN_FLAGS_REPORT_RULES_MATCHING,
        YaraCallback,
        (void*)&processInfo,
        0
    );
    
    if (result != ERROR_SUCCESS)
    {
        Logger::LogF(L"[ProcessScanner] YARA scan error: %d for process %s", 
                    result, processInfo.processName.c_str());
        return false;
    }
    
    return true;
}

bool ProcessScanner::ScanProcessMemory(HANDLE processHandle, const ProcessInfo& processInfo)
{
    try
    {
        // 메모리 영역들 가져오기
        std::vector<MEMORY_BASIC_INFORMATION> memoryRegions = GetMemoryRegions(processHandle);
        
        // 스캔 가능한 영역 필터링
        std::vector<MEMORY_BASIC_INFORMATION> validRegions;
        for (const auto& region : memoryRegions)
        {
            if (region.State == MEM_COMMIT && 
                (region.Protect & PAGE_READONLY || 
                 region.Protect & PAGE_READWRITE ||
                 region.Protect & PAGE_EXECUTE_READ ||
                 region.Protect & PAGE_EXECUTE_READWRITE))
            {
                validRegions.push_back(region);
            }
        }
        
        if (validRegions.empty())
        {
            return true;
        }
        
        // 병렬 처리를 위한 배치 크기 (동시에 실행할 최대 스레드 수)
        const size_t maxConcurrentThreads = std::min(validRegions.size(), static_cast<size_t>(128));
        int scannedRegions = 0;
        
        // 배치 단위로 병렬 스캔 실행
        for (size_t i = 0; i < validRegions.size(); i += maxConcurrentThreads)
        {
            std::vector<std::future<bool>> futures;
            size_t batchEnd = std::min(i + maxConcurrentThreads, validRegions.size());
            
            // 현재 배치의 모든 영역을 비동기로 스캔 시작
            for (size_t j = i; j < batchEnd; j++)
            {
                futures.push_back(
                    std::async(std::launch::async, 
                              &ProcessScanner::ScanMemoryRegion, 
                              this, 
                              processHandle, 
                              validRegions[j], 
                              processInfo)
                );
            }
            
            // 현재 배치의 모든 작업 완료 대기
            for (auto& future : futures)
            {
                try
                {
                    if (future.get())
                    {
                        scannedRegions++;
                    }
                }
                catch (...)
                {
                    Logger::LogError(L"[ProcessScanner] Exception in parallel memory scan");
                }
            }
        }
        
        // Logger::LogF(L"[ProcessScanner] Parallel memory scan completed: %s - %d regions scanned", 
        //             processInfo.processName.c_str(), scannedRegions);
        
        return true;
    }
    catch (...)
    {
        Logger::LogError(L"[ProcessScanner] Exception occurred during parallel memory scan");
        return false;
    }
}

bool ProcessScanner::ReadProcessMemory(HANDLE processHandle, LPCVOID address, SIZE_T size, std::vector<BYTE>& buffer)
{
    if (size == 0 || size > 100 * 1024 * 1024) // 100MB 제한
        return false;
    
    buffer.resize(size);
    SIZE_T bytesRead = 0;
    
    BOOL result = ::ReadProcessMemory(processHandle, address, buffer.data(), size, &bytesRead);
    if (!result || bytesRead != size)
    {
        return false;
    }
    
    return true;
}

std::vector<MEMORY_BASIC_INFORMATION> ProcessScanner::GetMemoryRegions(HANDLE processHandle)
{
    std::vector<MEMORY_BASIC_INFORMATION> regions;
    
    MEMORY_BASIC_INFORMATION mbi;
    SIZE_T address = 0;
    
    while (VirtualQueryEx(processHandle, (LPCVOID)address, &mbi, sizeof(mbi)) == sizeof(mbi))
    {
        regions.push_back(mbi);
        address += mbi.RegionSize;
        
        // 너무 많은 영역을 방지하기 위한 제한
        if (regions.size() > 10000)
            break;
    }
    
    return regions;
}

bool ProcessScanner::LoadYaraRules()
{
    // YARA 컴파일러 생성
    int result = yr_compiler_create(&m_yaraCompiler);
    if (result != ERROR_SUCCESS)
    {
        Logger::LogF(L"[ProcessScanner] YARA compiler creation failed with error: %d", result);
        return false;
    }
    
    // YARA 규칙 파일 열기
    FILE* ruleFile = nullptr;
    errno_t err = _wfopen_s(&ruleFile, m_rulesPath.c_str(), L"r");
    if (err != 0 || ruleFile == nullptr)
    {
        Logger::LogF(L"[ProcessScanner] Failed to open YARA rules file: %s (error: %d)", 
                    m_rulesPath.c_str(), err);
        return false;
    }
    
    // YARA 규칙 컴파일
    result = yr_compiler_add_file(m_yaraCompiler, ruleFile, nullptr, "malware_rules.yar");
    fclose(ruleFile);
    
    if (result != ERROR_SUCCESS)
    {
        Logger::LogF(L"[ProcessScanner] YARA rules compilation failed with error: %d", result);
        return false;
    }
    
    // 컴파일된 규칙 추출
    result = yr_compiler_get_rules(m_yaraCompiler, &m_yaraRules);
    if (result != ERROR_SUCCESS)
    {
        Logger::LogF(L"[ProcessScanner] YARA rules extraction failed with error: %d", result);
        return false;
    }
    
    Logger::Log(L"[ProcessScanner] YARA rules loaded successfully");
    return true;
}

// YARA 콜백 함수 - 매치가 발견되었을 때 호출됨
int ProcessScanner::YaraCallback(YR_SCAN_CONTEXT* context, int message, void* message_data, void* user_data)
{
    if (message == CALLBACK_MSG_RULE_MATCHING)
    {
        YR_RULE* rule = (YR_RULE*)message_data;
        ProcessInfo* processInfo = (ProcessInfo*)user_data;
        
        if (rule && processInfo)
        {
            // 규칙 이름을 UTF-8에서 UTF-16으로 변환
            std::wstring ruleName = L"Unknown";
            if (rule->identifier)
            {
                int len = MultiByteToWideChar(CP_UTF8, 0, rule->identifier, -1, nullptr, 0);
                if (len > 0)
                {
                    ruleName.resize(len - 1);
                    MultiByteToWideChar(CP_UTF8, 0, rule->identifier, -1, &ruleName[0], len);
                }
            }
            
            Logger::LogF(L"[ProcessScanner] *** MALWARE DETECTED! ***");
            Logger::LogF(L"[ProcessScanner] Rule: %s", ruleName.c_str());
            Logger::LogF(L"[ProcessScanner] Process: %s (PID: %d)", 
                        processInfo->processName.c_str(), 
                        processInfo->processId);
            
            // 규칙 태그들 출력
            YR_STRING* string;
            if (rule->strings)
            {
                Logger::LogF(L"[ProcessScanner] Matched conditions:");
                
                yr_rule_strings_foreach(rule, string)
                {
                    // 문자열 식별자 변환
                    std::wstring stringId = L"Unknown";
                    if (string->identifier)
                    {
                        int len = MultiByteToWideChar(CP_UTF8, 0, string->identifier, -1, nullptr, 0);
                        if (len > 0)
                        {
                            stringId.resize(len - 1);
                            MultiByteToWideChar(CP_UTF8, 0, string->identifier, -1, &stringId[0], len);
                        }
                    }
                    
                    // 각 매치 위치와 내용 출력
                    YR_MATCH* match;
                    bool hasMatches = false;
                    yr_string_matches_foreach(context, string, match)
                    {
                        if (!hasMatches)
                        {
                            Logger::LogF(L"[ProcessScanner]   - String: %s", stringId.c_str());
                            hasMatches = true;
                        }
                        
                        Logger::LogF(L"[ProcessScanner]     * Offset: 0x%08X, Length: %d", 
                                    (unsigned int)match->offset, 
                                    (int)match->match_length);
                        
                        // 매칭된 내용의 일부 출력 (최대 32바이트)
                        if (match->data && match->match_length > 0)
                        {
                            int displayLen = (match->match_length < 32) ? match->match_length : 32;
                            std::wstring hexData;
                            std::wstring asciiData;
                            
                            for (int i = 0; i < displayLen; i++)
                            {
                                wchar_t hex[4];
                                swprintf_s(hex, L"%02X ", (unsigned char)match->data[i]);
                                hexData += hex;
                                
                                // 출력 가능한 ASCII 문자만 표시
                                if (match->data[i] >= 32 && match->data[i] < 127)
                                {
                                    asciiData += (wchar_t)match->data[i];
                                }
                                else
                                {
                                    asciiData += L'.';
                                }
                            }
                            
                            Logger::LogF(L"[ProcessScanner]     * Content (Hex): %s", hexData.c_str());
                            Logger::LogF(L"[ProcessScanner]     * Content (ASCII): %s", asciiData.c_str());
                            
                            if (match->match_length > 32)
                            {
                                Logger::LogF(L"[ProcessScanner]     * ... (%d more bytes)", 
                                            match->match_length - 32);
                            }
                        }
                    }
                }
            }
            
            // 구분선 출력
            Logger::LogF(L"[ProcessScanner] =====================================");
        }
    }
    
    return CALLBACK_CONTINUE;
}

bool ProcessScanner::EnableDebugPrivilege()
{
    HANDLE hToken;
    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
    {
        return false;
    }
    
    TOKEN_PRIVILEGES tkp;
    if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tkp.Privileges[0].Luid))
    {
        CloseHandle(hToken);
        return false;
    }
    
    tkp.PrivilegeCount = 1;
    tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
    
    BOOL result = AdjustTokenPrivileges(hToken, FALSE, &tkp, 0, NULL, NULL);
    CloseHandle(hToken);
    
    return result && GetLastError() == ERROR_SUCCESS;
} 
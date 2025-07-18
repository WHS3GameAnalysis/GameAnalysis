#include "InjectionDetector.h"
#include "Logger.h"
#include <tlhelp32.h>
#include <psapi.h>
#include <sstream>
#include <iomanip>

// 전역 인스턴스
InjectionDetector* g_injection_detector = nullptr;

// 후킹된 NT 함수들의 구현
NTSTATUS NTAPI HookedNtAllocateVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, ULONG_PTR ZeroBits, PSIZE_T RegionSize, ULONG AllocationType, ULONG Protect) {
    NTSTATUS status = STATUS_SUCCESS;
    
    if (g_injection_detector && g_injection_detector->original_nt_allocate_virtual_memory_) {
        status = g_injection_detector->original_nt_allocate_virtual_memory_(ProcessHandle, BaseAddress, ZeroBits, RegionSize, AllocationType, Protect);
        
        if (NT_SUCCESS(status) && ProcessHandle == GetCurrentProcess()) {
            g_injection_detector->OnMemoryAllocated(*BaseAddress, *RegionSize, Protect, AllocationType);
        }
    }
    
    return status;
}

NTSTATUS NTAPI HookedNtFreeVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, PSIZE_T RegionSize, ULONG FreeType) {
    NTSTATUS status = STATUS_SUCCESS;
    
    if (g_injection_detector && g_injection_detector->original_nt_free_virtual_memory_) {
        if (ProcessHandle == GetCurrentProcess()) {
            g_injection_detector->OnMemoryFreed(*BaseAddress);
        }
        status = g_injection_detector->original_nt_free_virtual_memory_(ProcessHandle, BaseAddress, RegionSize, FreeType);
    }
    
    return status;
}

NTSTATUS NTAPI HookedLdrLoadDll(PWCHAR PathToFile, ULONG Flags, PUNICODE_STRING ModuleFileName, PHANDLE ModuleHandle) {
    NTSTATUS status = STATUS_SUCCESS;
    
    if (g_injection_detector && g_injection_detector->original_ldr_load_dll_) {
        status = g_injection_detector->original_ldr_load_dll_(PathToFile, Flags, ModuleFileName, ModuleHandle);
        
        if (NT_SUCCESS(status) && ModuleHandle && *ModuleHandle) {
            std::wstring library_name;
            if (ModuleFileName && ModuleFileName->Buffer) {
                library_name = std::wstring(ModuleFileName->Buffer, ModuleFileName->Length / sizeof(WCHAR));
            } else if (PathToFile) {
                library_name = std::wstring(PathToFile);
            }
            
            g_injection_detector->OnLibraryLoaded((HMODULE)*ModuleHandle, library_name);
        }
    }
    
    return status;
}

PVOID NTAPI HookedRtlAllocateHeap(PVOID HeapHandle, ULONG Flags, SIZE_T Size) {
    PVOID result = nullptr;
    
    if (g_injection_detector && g_injection_detector->original_rtl_allocate_heap_) {
        result = g_injection_detector->original_rtl_allocate_heap_(HeapHandle, Flags, Size);
        
        if (result) {
            g_injection_detector->OnMemoryAllocated(result, Size, PAGE_READWRITE, MEM_COMMIT);
        }
    }
    
    return result;
}

BOOLEAN NTAPI HookedRtlFreeHeap(PVOID HeapHandle, ULONG Flags, PVOID BaseAddress) {
    BOOLEAN result = FALSE;
    
    if (g_injection_detector && g_injection_detector->original_rtl_free_heap_) {
        if (BaseAddress) {
            g_injection_detector->OnMemoryFreed(BaseAddress);
        }
        result = g_injection_detector->original_rtl_free_heap_(HeapHandle, Flags, BaseAddress);
    }
    
    return result;
}

HANDLE WINAPI HookedCreateThread(LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId) {
    HANDLE thread_handle = NULL;
    
    if (g_injection_detector && g_injection_detector->original_create_thread_) {
        thread_handle = g_injection_detector->original_create_thread_(lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
        
        if (thread_handle != NULL && thread_handle != INVALID_HANDLE_VALUE) {
            DWORD thread_id = 0;
            
            // lpThreadId가 제공된 경우 사용, 그렇지 않으면 핸들로부터 ID 획득
            if (lpThreadId && *lpThreadId != 0) {
                thread_id = *lpThreadId;
            } else {
                thread_id = GetThreadId(thread_handle);
            }
            
            if (thread_id != 0) {
                DWORD parent_thread_id = GetCurrentThreadId();
                g_injection_detector->OnThreadCreated(thread_id, parent_thread_id);
            }
        }
    }
    
    return thread_handle;
}

HANDLE WINAPI HookedCreateRemoteThread(HANDLE hProcess, LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId) {
    HANDLE thread_handle = NULL;
    
    if (g_injection_detector && g_injection_detector->original_create_remote_thread_) {
        thread_handle = g_injection_detector->original_create_remote_thread_(hProcess, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
        
        if (thread_handle != NULL && thread_handle != INVALID_HANDLE_VALUE) {
            DWORD thread_id = 0;
            
            // lpThreadId가 제공된 경우 사용, 그렇지 않으면 핸들로부터 ID 획득
            if (lpThreadId && *lpThreadId != 0) {
                thread_id = *lpThreadId;
            } else {
                thread_id = GetThreadId(thread_handle);
            }
            
            if (thread_id != 0) {
                DWORD parent_thread_id = GetCurrentThreadId();
                g_injection_detector->OnThreadCreated(thread_id, parent_thread_id);
                
                // CreateRemoteThread는 특히 의심스러우므로 추가 로깅
                OutputDebugStringA("[SimpleAC] REMOTE THREAD CREATED - HIGHLY SUSPICIOUS!\n");
            }
        }
    }
    
    return thread_handle;
}

// InjectionDetector 클래스 구현
InjectionDetector::InjectionDetector() 
    : hooks_installed_(false), is_initialized_(false), main_process_id_(0),
      original_nt_allocate_virtual_memory_(nullptr), original_nt_free_virtual_memory_(nullptr),
      original_ldr_load_dll_(nullptr), original_rtl_allocate_heap_(nullptr),
      original_rtl_free_heap_(nullptr), original_create_thread_(nullptr),
      original_create_remote_thread_(nullptr), suspicious_event_index_(0), whitelisted_thread_count_(0) {
    main_process_id_ = GetCurrentProcessId();
    
    // 정적 배열 초기화
    memset(suspicious_events_, 0, sizeof(suspicious_events_));
    memset(whitelisted_threads_array_, 0, sizeof(whitelisted_threads_array_));
}

InjectionDetector::~InjectionDetector() {
    if (is_initialized_) {
        Cleanup();
    }
}

bool InjectionDetector::Initialize() {
    if (is_initialized_) {
        return true;
    }
    
    Logger::Log(L"[InjectionDetector] Initializing NT Function Hooking System");
    
    // ntdll.dll 모듈 핸들 얻기
    HMODULE ntdll_module = GetModuleHandleW(L"ntdll.dll");
    if (!ntdll_module) {
        Logger::Log(L"[InjectionDetector] Failed to get ntdll.dll handle");
        return false;
    }
    
    // 원본 NT 함수 포인터 설정
    original_nt_allocate_virtual_memory_ = (NtAllocateVirtualMemoryFunc)GetProcAddress(ntdll_module, "NtAllocateVirtualMemory");
    if (!original_nt_allocate_virtual_memory_) {
        Logger::Log(L"[InjectionDetector] Failed to get NtAllocateVirtualMemory address");
        return false;
    }
    
    original_nt_free_virtual_memory_ = (NtFreeVirtualMemoryFunc)GetProcAddress(ntdll_module, "NtFreeVirtualMemory");
    if (!original_nt_free_virtual_memory_) {
        Logger::Log(L"[InjectionDetector] Failed to get NtFreeVirtualMemory address");
        return false;
    }
    
    original_ldr_load_dll_ = (LdrLoadDllFunc)GetProcAddress(ntdll_module, "LdrLoadDll");
    if (!original_ldr_load_dll_) {
        Logger::Log(L"[InjectionDetector] Failed to get LdrLoadDll address");
        return false;
    }
    
    original_rtl_allocate_heap_ = (RtlAllocateHeapFunc)GetProcAddress(ntdll_module, "RtlAllocateHeap");
    if (!original_rtl_allocate_heap_) {
        Logger::Log(L"[InjectionDetector] Failed to get RtlAllocateHeap address");
        return false;
    }
    
    original_rtl_free_heap_ = (RtlFreeHeapFunc)GetProcAddress(ntdll_module, "RtlFreeHeap");
    if (!original_rtl_free_heap_) {
        Logger::Log(L"[InjectionDetector] Failed to get RtlFreeHeap address");
        return false;
    }
    
    // kernel32.dll 모듈 핸들 얻기
    HMODULE kernel32_module = GetModuleHandleW(L"kernel32.dll");
    if (!kernel32_module) {
        Logger::Log(L"[InjectionDetector] Failed to get kernel32.dll handle");
        return false;
    }
    
    original_create_thread_ = (CreateThreadFunc)GetProcAddress(kernel32_module, "CreateThread");
    if (!original_create_thread_) {
        Logger::Log(L"[InjectionDetector] Failed to get CreateThread address");
        return false;
    }
    
    original_create_remote_thread_ = (CreateRemoteThreadFunc)GetProcAddress(kernel32_module, "CreateRemoteThread");
    if (!original_create_remote_thread_) {
        Logger::Log(L"[InjectionDetector] Failed to get CreateRemoteThread address");
        return false;
    }

    // 화이트리스트 초기화 (현재 프로세스의 모든 메모리/스레드)
    InitializeWhitelist();
    
    // Detours를 사용한 NT 함수 inline 후킹 설치
    if (!InstallNTHooks()) {
        Logger::Log(L"[InjectionDetector] Failed to install NT hooks");
        return false;
    }
    
    is_initialized_ = true;
    Logger::Log(L"[InjectionDetector] NT Function Hooking System Initialized Successfully");
    
    return true;
}

void InjectionDetector::Cleanup() {
    if (!is_initialized_) {
        return;
    }
    
    Logger::Log(L"[InjectionDetector] Cleaning up NT Function Hooking System");
    
    // 후킹 해제
    UninstallNTHooks();
    
    // 메모리 정리
    {
        std::lock_guard<std::mutex> lock(memory_mutex_);
        tracked_memory_regions_.clear();
        whitelisted_memory_addresses_.clear();
    }
    
    {
        std::lock_guard<std::mutex> lock(thread_mutex_);
        tracked_threads_.clear();
        whitelisted_thread_ids_.clear();
    }
    
    is_initialized_ = false;
    Logger::Log(L"[InjectionDetector] NT Function Hooking System Cleanup Complete");
}

void InjectionDetector::ScanMemoryRegions() {
    if (!is_initialized_) {
        return;
    }
    
    // 먼저 suspicious_events 배열 처리
    ProcessSuspiciousEvents();
    
    // 스캔 중에는 후킹을 일시적으로 비활성화하여 재귀 호출 방지
    static bool scanning_in_progress = false;
    if (scanning_in_progress) {
        return;
    }
    
    scanning_in_progress = true;
    
    MEMORY_BASIC_INFORMATION mbi;
    void* current_address = nullptr;
    
    while (VirtualQuery(current_address, &mbi, sizeof(mbi))) {
        if (mbi.State == MEM_COMMIT && mbi.Type == MEM_PRIVATE) {
            // 실행 가능한 메모리 영역인지 확인
            if (mbi.Protect & (PAGE_EXECUTE | PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) {
                // 화이트리스트에 없는 실행 가능한 메모리 영역 발견
                if (!IsMemoryWhitelisted(mbi.BaseAddress)) {
                    // 의심스러운 메모리 영역 분석
                    //if (AnalyzeMemoryCharacteristics(mbi.BaseAddress, mbi.RegionSize)) {
                        std::wstringstream log_message;
                        log_message << L"[InjectionDetector] SUSPICIOUS MEMORY REGION DETECTED! "
                                   << L"Address: 0x" << std::hex << std::uppercase << (uintptr_t)mbi.BaseAddress
                                   << L", Size: " << std::dec << mbi.RegionSize
                                   << L", Protection: 0x" << std::hex << mbi.Protect;
                        Logger::LogWarning(log_message.str());
                    //}
                }
            }
        }
        current_address = (void*)((uintptr_t)mbi.BaseAddress + mbi.RegionSize);
    }
    
    scanning_in_progress = false;
}

void InjectionDetector::InitializeWhitelist() {
    Logger::Log(L"[InjectionDetector] Initializing whitelist with current process state");
    
    // 현재 프로세스의 모든 스레드를 화이트리스트에 추가
    InitializeThreadWhitelist();
    
    // 현재 프로세스의 모든 메모리 영역을 화이트리스트에 추가
    InitializeMemoryWhitelist();
    
    Logger::Log(L"[InjectionDetector] Whitelist initialization complete");
}

void InjectionDetector::InitializeThreadWhitelist() {
    // 정적 배열에 현재 프로세스의 스레드들 추가
    HANDLE thread_snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
    if (thread_snapshot == INVALID_HANDLE_VALUE) {
        Logger::Log(L"[InjectionDetector] Failed to create thread snapshot");
        return;
    }
    
    THREADENTRY32 thread_entry;
    thread_entry.dwSize = sizeof(THREADENTRY32);
    
    whitelisted_thread_count_ = 0; // 리셋
    
    if (Thread32First(thread_snapshot, &thread_entry)) {
        do {
            if (thread_entry.th32OwnerProcessID == main_process_id_ && 
                whitelisted_thread_count_ < MAX_WHITELISTED_THREADS) {
                whitelisted_threads_array_[whitelisted_thread_count_] = thread_entry.th32ThreadID;
                whitelisted_thread_count_++;
                
                // 동적 컨테이너에도 추가 (스캔용)
                std::lock_guard<std::mutex> lock(thread_mutex_);
                whitelisted_thread_ids_.insert(thread_entry.th32ThreadID);
                auto thread_info = std::make_unique<ThreadInfo>(
                    thread_entry.th32ThreadID, 0, main_process_id_
                );
                thread_info->is_whitelisted = true;
                tracked_threads_.push_back(std::move(thread_info));
            }
        } while (Thread32Next(thread_snapshot, &thread_entry));
    }
    
    CloseHandle(thread_snapshot);
}

void InjectionDetector::InitializeMemoryWhitelist() {
    std::lock_guard<std::mutex> lock(memory_mutex_);
    
    MEMORY_BASIC_INFORMATION mbi;
    void* current_address = nullptr;
    
    while (VirtualQuery(current_address, &mbi, sizeof(mbi))) {
        if (mbi.State == MEM_COMMIT) {
            whitelisted_memory_addresses_.insert(mbi.BaseAddress);
            auto memory_region = std::make_unique<MemoryRegion>(
                mbi.BaseAddress, mbi.RegionSize, mbi.Protect, mbi.Type,
                GetCurrentThreadId(), main_process_id_
            );
            memory_region->is_whitelisted = true;
            tracked_memory_regions_.push_back(std::move(memory_region));
        }
        current_address = (void*)((uintptr_t)mbi.BaseAddress + mbi.RegionSize);
    }
}

bool InjectionDetector::InstallNTHooks() {
    Logger::Log(L"[InjectionDetector] Installing NT function hooks using Detours");
    
    // Detours 트랜잭션 시작
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    
    // 각 NT 함수에 대한 후킹 설치
    LONG result = DetourAttach(&(PVOID&)original_nt_allocate_virtual_memory_, HookedNtAllocateVirtualMemory);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach NtAllocateVirtualMemory hook");
        DetourTransactionAbort();
        return false;
    }
    
    result = DetourAttach(&(PVOID&)original_nt_free_virtual_memory_, HookedNtFreeVirtualMemory);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach NtFreeVirtualMemory hook");
        DetourTransactionAbort();
        return false;
    }
    
    /*result = DetourAttach(&(PVOID&)original_ldr_load_dll_, HookedLdrLoadDll);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach LdrLoadDll hook");
        DetourTransactionAbort();
        return false;
    }
    
    result = DetourAttach(&(PVOID&)original_rtl_allocate_heap_, HookedRtlAllocateHeap);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach RtlAllocateHeap hook");
        DetourTransactionAbort();
        return false;
    }
    
    result = DetourAttach(&(PVOID&)original_rtl_free_heap_, HookedRtlFreeHeap);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach RtlFreeHeap hook");
        DetourTransactionAbort();
        return false;
    }*/
    
    // 스레드 생성 함수 후킹
    result = DetourAttach(&(PVOID&)original_create_thread_, HookedCreateThread);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach CreateThread hook");
        DetourTransactionAbort();
        return false;
    }
    
    result = DetourAttach(&(PVOID&)original_create_remote_thread_, HookedCreateRemoteThread);
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to attach CreateRemoteThread hook");
        DetourTransactionAbort();
        return false;
    }
    
    // 트랜잭션 커밋
    result = DetourTransactionCommit();
    if (result != NO_ERROR) {
        Logger::Log(L"[InjectionDetector] Failed to commit NT detour transaction");
        return false;
    }
    
    hooks_installed_ = true;
    Logger::Log(L"[InjectionDetector] NT function hooks installed successfully");
    return true;
}

void InjectionDetector::UninstallNTHooks() {
    if (!hooks_installed_) {
        return;
    }
    
    Logger::Log(L"[InjectionDetector] Uninstalling NT function hooks");
    
    // Detours 트랜잭션 시작
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    
    // 각 NT 함수에 대한 후킹 해제
    DetourDetach(&(PVOID&)original_nt_allocate_virtual_memory_, HookedNtAllocateVirtualMemory);
    DetourDetach(&(PVOID&)original_nt_free_virtual_memory_, HookedNtFreeVirtualMemory);
    DetourDetach(&(PVOID&)original_ldr_load_dll_, HookedLdrLoadDll);
    DetourDetach(&(PVOID&)original_rtl_allocate_heap_, HookedRtlAllocateHeap);
    DetourDetach(&(PVOID&)original_rtl_free_heap_, HookedRtlFreeHeap);
    DetourDetach(&(PVOID&)original_create_thread_, HookedCreateThread);
    DetourDetach(&(PVOID&)original_create_remote_thread_, HookedCreateRemoteThread);
    
    // 트랜잭션 커밋
    DetourTransactionCommit();
    
    hooks_installed_ = false;
    Logger::Log(L"[InjectionDetector] NT function hooks uninstalled successfully");
}

bool InjectionDetector::IsMemoryWhitelisted(void* address) {
    std::lock_guard<std::mutex> lock(memory_mutex_);
    return whitelisted_memory_addresses_.find(address) != whitelisted_memory_addresses_.end();
}

bool InjectionDetector::IsThreadWhitelisted(DWORD thread_id) {
    // 정적 배열에서 선형 검색 (뮤텍스 없음)
    for (long i = 0; i < whitelisted_thread_count_; i++) {
        if (whitelisted_threads_array_[i] == thread_id) {
            return true;
        }
    }
    return false;
}

void InjectionDetector::OnMemoryAllocated(void* address, size_t size, DWORD protection, DWORD allocation_type) {
    // 콜백에서는 절대 동적 메모리 할당 금지!
    DWORD current_thread_id = GetCurrentThreadId();
    
    // 화이트리스트 체크
    if (!IsThreadWhitelisted(current_thread_id)) {
        // 의심스러운 이벤트를 정적 배열에 기록
        long index = InterlockedIncrement(&suspicious_event_index_) - 1;
        if (index < MAX_SUSPICIOUS_EVENTS) {
            SuspiciousEvent* event = &suspicious_events_[index % MAX_SUSPICIOUS_EVENTS];
            event->type = SuspiciousEvent::MEMORY_ALLOC;
            event->address = address;
            event->size = size;
            event->thread_id = current_thread_id;
            event->protection = protection;
            event->library_name[0] = L'\0'; // 사용하지 않음
            event->is_used = true;
        }
        
        // 간단한 디버그 출력만
        OutputDebugStringA("[SimpleAC] SUSPICIOUS MEMORY ALLOCATION!\n");
    }
}

void InjectionDetector::OnMemoryFreed(void* address) {
    // 메모리 해제는 특별히 기록하지 않음 (재귀 방지)
    // 필요시 간단한 디버그 출력만
    // OutputDebugStringA("[SimpleAC] Memory freed\n");
}

void InjectionDetector::OnLibraryLoaded(HMODULE module, const std::wstring& library_name) {
    DWORD current_thread_id = GetCurrentThreadId();
    
    // 화이트리스트 체크
    if (!IsThreadWhitelisted(current_thread_id)) {
        // 의심스러운 이벤트를 정적 배열에 기록
        long index = InterlockedIncrement(&suspicious_event_index_) - 1;
        if (index < MAX_SUSPICIOUS_EVENTS) {
            SuspiciousEvent* event = &suspicious_events_[index % MAX_SUSPICIOUS_EVENTS];
            event->type = SuspiciousEvent::LIBRARY_LOAD;
            event->address = (void*)module;
            event->size = 0;
            event->thread_id = current_thread_id;
            event->protection = 0;
            
            // 라이브러리 이름 복사 (안전하게)
            size_t copy_len = min(library_name.length(), 259);
            wcsncpy_s(event->library_name, library_name.c_str(), copy_len);
            event->library_name[copy_len] = L'\0';
            
            event->is_used = true;
        }
        
        // 간단한 디버그 출력만
        OutputDebugStringA("[SimpleAC] SUSPICIOUS LIBRARY LOAD!\n");
    }
}

void InjectionDetector::OnThreadCreated(DWORD thread_id, DWORD parent_thread_id) {
    // 화이트리스트 체크
    if (IsThreadWhitelisted(parent_thread_id)) {
        // 화이트리스트에 추가 (원자적 연산)
        long current_count = InterlockedIncrement(&whitelisted_thread_count_) - 1;
        if (current_count < MAX_WHITELISTED_THREADS) {
            whitelisted_threads_array_[current_count] = thread_id;
        }
        
        OutputDebugStringA("[SimpleAC] New whitelisted thread created\n");
    } else {
        // 의심스러운 이벤트를 정적 배열에 기록
        long index = InterlockedIncrement(&suspicious_event_index_) - 1;
        if (index < MAX_SUSPICIOUS_EVENTS) {
            SuspiciousEvent* event = &suspicious_events_[index % MAX_SUSPICIOUS_EVENTS];
            event->type = SuspiciousEvent::THREAD_CREATE;
            event->address = nullptr;
            event->size = 0;
            event->thread_id = thread_id;
            event->protection = parent_thread_id; // parent_thread_id를 protection 필드에 저장
            event->library_name[0] = L'\0';
            event->is_used = true;
        }
        
        OutputDebugStringA("[SimpleAC] SUSPICIOUS THREAD CREATION!\n");
    }
}

bool InjectionDetector::AnalyzeMemoryCharacteristics(void* address, size_t size) {
    // 실제 메모리 내용을 분석하여 의심스러운 패턴 감지
    __try {
        // PE 헤더 시그니처 검사
        if (size >= sizeof(IMAGE_DOS_HEADER)) {
            PIMAGE_DOS_HEADER dos_header = (PIMAGE_DOS_HEADER)address;
            if (dos_header->e_magic == IMAGE_DOS_SIGNATURE) {
                // PE 파일 구조로 보이는 메모리 영역 - 의심스러움
                return true;
            }
        }
        
        // 셸코드 패턴 검사 (간단한 예시)
        BYTE* memory_bytes = (BYTE*)address;
        for (size_t i = 0; i < size - 4; i++) {
            // 일반적인 셸코드 패턴들
            if (memory_bytes[i] == 0x60 && memory_bytes[i+1] == 0x61) { // PUSHAD, POPAD
                return true;
            }
            if (memory_bytes[i] == 0xEB && memory_bytes[i+2] == 0x5D) { // JMP +2, POP EBP
                return true;
            }
        }
        
        return false;
    }
    __except(EXCEPTION_EXECUTE_HANDLER) {
        // 메모리 접근 오류 - 의심스러움
        return true;
    }
}

void InjectionDetector::AddToWhitelist(void* base_address, size_t size) {
    std::lock_guard<std::mutex> lock(memory_mutex_);
    
    whitelisted_memory_addresses_.insert(base_address);
    
    auto memory_region = std::make_unique<MemoryRegion>(
        base_address, size, PAGE_READWRITE, MEM_COMMIT,
        GetCurrentThreadId(), main_process_id_
    );
    memory_region->is_whitelisted = true;
    tracked_memory_regions_.push_back(std::move(memory_region));
    
    std::wstringstream log_message;
    log_message << L"[InjectionDetector] Memory region added to whitelist: 0x" 
               << std::hex << std::uppercase << (uintptr_t)base_address
               << L", Size: " << std::dec << size;
    Logger::Log(log_message.str());
}

void InjectionDetector::AddThreadToWhitelist(DWORD thread_id) {
    std::lock_guard<std::mutex> lock(thread_mutex_);
    
    whitelisted_thread_ids_.insert(thread_id);
    
    auto thread_info = std::make_unique<ThreadInfo>(
        thread_id, GetCurrentThreadId(), main_process_id_
    );
    thread_info->is_whitelisted = true;
    tracked_threads_.push_back(std::move(thread_info));
    
    std::wstringstream log_message;
    log_message << L"[InjectionDetector] Thread added to whitelist: " << thread_id;
    Logger::Log(log_message.str());
}

bool InjectionDetector::IsSuspiciousMemoryRegion(const MemoryRegion& region) {
    // 실행 가능한 메모리 영역인지 확인
    if (region.protection & (PAGE_EXECUTE | PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) {
        // 화이트리스트에 없는 경우 의심스러움
        if (!region.is_whitelisted) {
            return true;
        }
    }
    
    // 비정상적으로 큰 메모리 영역
    if (region.size > 100 * 1024 * 1024) { // 100MB 이상
        return true;
    }
    
    // 실행 가능하면서 쓰기 가능한 메모리
    if ((region.protection & PAGE_EXECUTE_READWRITE) || 
        (region.protection & PAGE_EXECUTE_WRITECOPY)) {
        return true;
    }
    
    return false;
} 

void InjectionDetector::ProcessSuspiciousEvents() {
    // suspicious_events 배열에서 새로운 이벤트들을 처리
    static long last_processed_index = 0;
    
    long current_index = suspicious_event_index_;
    for (long i = last_processed_index; i < current_index && i < MAX_SUSPICIOUS_EVENTS; i++) {
        SuspiciousEvent* event = &suspicious_events_[i % MAX_SUSPICIOUS_EVENTS];
        if (event->is_used) {
            ProcessSuspiciousEvent(event);
            event->is_used = false; // 처리 완료 표시
        }
    }
    
    last_processed_index = current_index;
}

void InjectionDetector::ProcessSuspiciousEvent(SuspiciousEvent* event) {
    std::wstringstream log_message;
    
    switch (event->type) {
    case SuspiciousEvent::MEMORY_ALLOC:
        log_message << L"[InjectionDetector] SUSPICIOUS MEMORY ALLOCATION! "
                   << L"Address: 0x" << std::hex << std::uppercase << (uintptr_t)event->address
                   << L", Size: " << std::dec << event->size
                   << L", Thread: " << event->thread_id
                   << L", Protection: 0x" << std::hex << event->protection;
        break;
        
    case SuspiciousEvent::LIBRARY_LOAD:
        log_message << L"[InjectionDetector] SUSPICIOUS LIBRARY LOAD! "
                   << L"Library: " << event->library_name
                   << L", Module: 0x" << std::hex << std::uppercase << (uintptr_t)event->address
                   << L", Thread: " << std::dec << event->thread_id;
        break;
        
    case SuspiciousEvent::THREAD_CREATE:
        log_message << L"[InjectionDetector] SUSPICIOUS THREAD CREATION! "
                   << L"Thread: " << event->thread_id
                   << L", Parent: " << event->protection; // protection 필드에 parent_thread_id 저장됨
        break;
    }
    
    Logger::LogWarning(log_message.str());
} 
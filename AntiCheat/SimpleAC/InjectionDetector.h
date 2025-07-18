#pragma once
#include <Windows.h>
#include <detours/detours.h>
#include <winternl.h>
#include <vector>
#include <unordered_set>
#include <unordered_map>
#include <memory>
#include <mutex>

// NT 상수 정의
#ifndef STATUS_SUCCESS
#define STATUS_SUCCESS ((NTSTATUS)0x00000000L)
#endif

#ifndef NT_SUCCESS
#define NT_SUCCESS(Status) (((NTSTATUS)(Status)) >= 0)
#endif

// NT 함수들의 시그니처 정의
typedef NTSTATUS (NTAPI* NtAllocateVirtualMemoryFunc)(
    HANDLE ProcessHandle,
    PVOID* BaseAddress,
    ULONG_PTR ZeroBits,
    PSIZE_T RegionSize,
    ULONG AllocationType,
    ULONG Protect
);

typedef NTSTATUS (NTAPI* NtFreeVirtualMemoryFunc)(
    HANDLE ProcessHandle,
    PVOID* BaseAddress,
    PSIZE_T RegionSize,
    ULONG FreeType
);

typedef NTSTATUS (NTAPI* LdrLoadDllFunc)(
    PWCHAR PathToFile,
    ULONG Flags,
    PUNICODE_STRING ModuleFileName,
    PHANDLE ModuleHandle
);

typedef PVOID (NTAPI* RtlAllocateHeapFunc)(
    PVOID HeapHandle,
    ULONG Flags,
    SIZE_T Size
);

typedef BOOLEAN (NTAPI* RtlFreeHeapFunc)(
    PVOID HeapHandle,
    ULONG Flags,
    PVOID BaseAddress
);

typedef HANDLE (WINAPI* CreateThreadFunc)(
    LPSECURITY_ATTRIBUTES lpThreadAttributes,
    SIZE_T dwStackSize,
    LPTHREAD_START_ROUTINE lpStartAddress,
    LPVOID lpParameter,
    DWORD dwCreationFlags,
    LPDWORD lpThreadId
);

typedef HANDLE (WINAPI* CreateRemoteThreadFunc)(
    HANDLE hProcess,
    LPSECURITY_ATTRIBUTES lpThreadAttributes,
    SIZE_T dwStackSize,
    LPTHREAD_START_ROUTINE lpStartAddress,
    LPVOID lpParameter,
    DWORD dwCreationFlags,
    LPDWORD lpThreadId
);

/**
 * @brief 메모리 영역 정보를 저장하는 구조체
 */
struct MemoryRegion {
    void* base_address;
    size_t size;
    DWORD protection;
    DWORD allocation_type;
    DWORD thread_id;  // 할당한 스레드 ID
    DWORD process_id;
    bool is_whitelisted;
    SYSTEMTIME allocation_time;
    
    MemoryRegion(void* addr, size_t sz, DWORD prot, DWORD alloc_type, DWORD tid, DWORD pid)
        : base_address(addr), size(sz), protection(prot), allocation_type(alloc_type), 
          thread_id(tid), process_id(pid), is_whitelisted(false) {
        GetSystemTime(&allocation_time);
    }
};

/**
 * @brief 스레드 정보를 저장하는 구조체
 */
struct ThreadInfo {
    DWORD thread_id;
    DWORD parent_thread_id;
    DWORD process_id;
    bool is_whitelisted;
    SYSTEMTIME creation_time;
    
    ThreadInfo(DWORD tid, DWORD parent_tid, DWORD pid)
        : thread_id(tid), parent_thread_id(parent_tid), process_id(pid), is_whitelisted(false) {
        GetSystemTime(&creation_time);
    }
};

/**
 * @brief DLL 인젝션 탐지를 위한 메인 클래스
 */
class InjectionDetector {
    // 후킹된 NT 함수들을 friend로 선언
    friend NTSTATUS NTAPI HookedNtAllocateVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, ULONG_PTR ZeroBits, PSIZE_T RegionSize, ULONG AllocationType, ULONG Protect);
    friend NTSTATUS NTAPI HookedNtFreeVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, PSIZE_T RegionSize, ULONG FreeType);
    friend NTSTATUS NTAPI HookedLdrLoadDll(PWCHAR PathToFile, ULONG Flags, PUNICODE_STRING ModuleFileName, PHANDLE ModuleHandle);
    friend PVOID NTAPI HookedRtlAllocateHeap(PVOID HeapHandle, ULONG Flags, SIZE_T Size);
    friend BOOLEAN NTAPI HookedRtlFreeHeap(PVOID HeapHandle, ULONG Flags, PVOID BaseAddress);
    friend HANDLE WINAPI HookedCreateThread(LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId);
    friend HANDLE WINAPI HookedCreateRemoteThread(HANDLE hProcess, LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId);
    
private:
    // 메모리 영역 추적 (동적 할당 사용 - 스캔에서만 사용)
    std::vector<std::unique_ptr<MemoryRegion>> tracked_memory_regions_;
    std::unordered_set<void*> whitelisted_memory_addresses_;
    
    // 스레드 추적 (동적 할당 사용 - 스캔에서만 사용)
    std::vector<std::unique_ptr<ThreadInfo>> tracked_threads_;
    std::unordered_set<DWORD> whitelisted_thread_ids_;
    
    // 콜백용 정적 데이터 (메모리 할당 없음)
    static const int MAX_SUSPICIOUS_EVENTS = 1000;
    
    struct SuspiciousEvent {
        enum Type { MEMORY_ALLOC, LIBRARY_LOAD, THREAD_CREATE } type;
        void* address;
        size_t size;
        DWORD thread_id;
        DWORD protection;
        wchar_t library_name[260]; // MAX_PATH
        volatile bool is_used;
    };
    
    SuspiciousEvent suspicious_events_[MAX_SUSPICIOUS_EVENTS];
    volatile long suspicious_event_index_;
    
    // 화이트리스트용 정적 배열 (콜백에서 안전하게 사용)
    static const int MAX_WHITELISTED_THREADS = 100;
    DWORD whitelisted_threads_array_[MAX_WHITELISTED_THREADS];
    volatile long whitelisted_thread_count_;
    
    // 동기화 (스캔 함수에서만 사용)
    std::mutex memory_mutex_;
    std::mutex thread_mutex_;
    
    // 후킹 관련 (Detours용)
    bool hooks_installed_;
    
    // 원본 NT 함수 포인터들
    NtAllocateVirtualMemoryFunc original_nt_allocate_virtual_memory_;
    NtFreeVirtualMemoryFunc original_nt_free_virtual_memory_;
    LdrLoadDllFunc original_ldr_load_dll_;
    RtlAllocateHeapFunc original_rtl_allocate_heap_;
    RtlFreeHeapFunc original_rtl_free_heap_;
    CreateThreadFunc original_create_thread_;
    CreateRemoteThreadFunc original_create_remote_thread_;
    
    // 초기화 관련
    bool is_initialized_;
    DWORD main_process_id_;
    
public:
    InjectionDetector();
    ~InjectionDetector();
    
    /**
     * @brief 인젝션 탐지 시스템 초기화
     * @return 성공 시 true, 실패 시 false
     */
    bool Initialize();
    
    /**
     * @brief 시스템 정리 및 후킹 해제
     */
    void Cleanup();
    
    /**
     * @brief 메모리 영역 순찰 및 탐지
     */
    void ScanMemoryRegions();
    
    /**
     * @brief 현재 프로세스의 모든 메모리 영역을 화이트리스트에 추가
     */
    void InitializeWhitelist();
    
    /**
     * @brief 특정 메모리 영역을 화이트리스트에 추가
     * @param base_address 메모리 영역의 시작 주소
     * @param size 메모리 영역의 크기
     */
    void AddToWhitelist(void* base_address, size_t size);
    
    /**
     * @brief 특정 스레드를 화이트리스트에 추가
     * @param thread_id 스레드 ID
     */
    void AddThreadToWhitelist(DWORD thread_id);
    
    /**
     * @brief 메모리 영역이 화이트리스트에 있는지 확인
     * @param address 확인할 메모리 주소
     * @return 화이트리스트에 있으면 true, 없으면 false
     */
    bool IsMemoryWhitelisted(void* address);
    
    /**
     * @brief 스레드가 화이트리스트에 있는지 확인
     * @param thread_id 확인할 스레드 ID
     * @return 화이트리스트에 있으면 true, 없으면 false
     */
    bool IsThreadWhitelisted(DWORD thread_id);
    
    // 후킹된 NT 함수들에서 호출할 콜백 함수들
    void OnMemoryAllocated(void* address, size_t size, DWORD protection, DWORD allocation_type);
    void OnMemoryFreed(void* address);
    void OnLibraryLoaded(HMODULE module, const std::wstring& library_name);
    void OnThreadCreated(DWORD thread_id, DWORD parent_thread_id);
    
private:
    /**
     * @brief Detours를 사용한 NT 함수 inline 후킹 설치
     * @return 성공 시 true, 실패 시 false
     */
    bool InstallNTHooks();
    
    /**
     * @brief Detours를 사용한 NT 함수 inline 후킹 해제
     */
    void UninstallNTHooks();
    
    /**
     * @brief 현재 프로세스의 모든 스레드를 화이트리스트에 추가
     */
    void InitializeThreadWhitelist();
    
    /**
     * @brief 현재 프로세스의 모든 메모리 영역을 화이트리스트에 추가
     */
    void InitializeMemoryWhitelist();
    
    /**
     * @brief 특정 메모리 영역이 의심스러운지 분석
     * @param region 분석할 메모리 영역
     * @return 의심스러우면 true, 아니면 false
     */
    bool IsSuspiciousMemoryRegion(const MemoryRegion& region);
    
    /**
     * @brief 메모리 영역의 특징을 분석하여 인젝션 여부 판단
     * @param address 분석할 메모리 주소
     * @param size 메모리 크기
     * @return 인젝션으로 의심되면 true, 아니면 false
     */
    bool AnalyzeMemoryCharacteristics(void* address, size_t size);
    
    /**
     * @brief 의심스러운 이벤트들을 처리
     */
    void ProcessSuspiciousEvents();
    
    /**
     * @brief 개별 의심스러운 이벤트를 처리
     * @param event 처리할 이벤트
     */
    void ProcessSuspiciousEvent(SuspiciousEvent* event);
};

// 전역 인스턴스
extern InjectionDetector* g_injection_detector;

// 후킹된 NT 함수들 (전역 함수로 정의)
NTSTATUS NTAPI HookedNtAllocateVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, ULONG_PTR ZeroBits, PSIZE_T RegionSize, ULONG AllocationType, ULONG Protect);
NTSTATUS NTAPI HookedNtFreeVirtualMemory(HANDLE ProcessHandle, PVOID* BaseAddress, PSIZE_T RegionSize, ULONG FreeType);
NTSTATUS NTAPI HookedLdrLoadDll(PWCHAR PathToFile, ULONG Flags, PUNICODE_STRING ModuleFileName, PHANDLE ModuleHandle);
PVOID NTAPI HookedRtlAllocateHeap(PVOID HeapHandle, ULONG Flags, SIZE_T Size);
BOOLEAN NTAPI HookedRtlFreeHeap(PVOID HeapHandle, ULONG Flags, PVOID BaseAddress);
HANDLE WINAPI HookedCreateThread(LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId);
HANDLE WINAPI HookedCreateRemoteThread(HANDLE hProcess, LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId); 
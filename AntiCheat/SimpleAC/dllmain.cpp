﻿#include <Windows.h>
#include <iostream>
#include <thread>
#include "ProcessScanner.h"
#include "Logger.h"

// DLL 인스턴스 핸들
HINSTANCE g_hInstance = NULL;

// 프로세스 스캐너 스레드 핸들
HANDLE g_hProcessScannerThread = NULL;
DWORD g_dwProcessScannerThreadId = 0;

// 스캐너 종료 이벤트
HANDLE g_hStopEvent = NULL;

// 프로세스 스캐너 스레드 함수
DWORD WINAPI ProcessScannerThreadProc(LPVOID lpParam)
{
    Logger::Log(L"[SimpleAC] Process Scanner Thread Started");
    
    ProcessScanner scanner;
    
    // YARA 엔진 초기화
    if (!scanner.Initialize())
    {
        Logger::Log(L"[SimpleAC] YARA Engine Initialization Failed");
        return 1;
    }
    
    Logger::Log(L"[SimpleAC] YARA Engine Initialized Successfully");
    
    // 프로세스 스캔 루프 (10초 주기)
    while (WaitForSingleObject(g_hStopEvent, 10000) == WAIT_TIMEOUT)
    {
        Logger::Log(L"[SimpleAC] Process Scan Started...");
        scanner.ScanAllProcesses();
        Logger::Log(L"[SimpleAC] Process Scan Completed");
    }
    
    scanner.Cleanup();
    Logger::Log(L"[SimpleAC] Process Scanner Thread Terminated");
    
    return 0;
}

// DLL 진입점
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hInstance = hModule;
        
        // Logger 파이프 초기화
        Logger::InitializePipe();
        Logger::Log(L"[SimpleAC] DLL Loaded");
        
        // 종료 이벤트 생성
        g_hStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        if (g_hStopEvent == NULL)
        {
            Logger::Log(L"[SimpleAC] Failed to Create Stop Event");
            return FALSE;
        }
        
        // 프로세스 스캐너 스레드 시작
        g_hProcessScannerThread = CreateThread(
            NULL,
            0,
            ProcessScannerThreadProc,
            NULL,
            0,
            &g_dwProcessScannerThreadId
        );
        
        if (g_hProcessScannerThread == NULL)
        {
            Logger::Log(L"[SimpleAC] Failed to Create Process Scanner Thread");
            CloseHandle(g_hStopEvent);
            return FALSE;
        }
        
        break;
        
    case DLL_THREAD_ATTACH:
        break;
        
    case DLL_THREAD_DETACH:
        break;
        
    case DLL_PROCESS_DETACH:
        Logger::Log(L"[SimpleAC] DLL Unloading Started");
        
        // 모든 스레드 종료 신호
        if (g_hStopEvent != NULL)
        {
            SetEvent(g_hStopEvent);
        }
        
        // 프로세스 스캐너 스레드 종료 대기
        if (g_hProcessScannerThread != NULL)
        {
            WaitForSingleObject(g_hProcessScannerThread, 10000); // 10초 대기
            CloseHandle(g_hProcessScannerThread);
            g_hProcessScannerThread = NULL;
        }
        
        // 이벤트 핸들 정리
        if (g_hStopEvent != NULL)
        {
            CloseHandle(g_hStopEvent);
            g_hStopEvent = NULL;
        }
        
        Logger::Log(L"[SimpleAC] All threads terminated successfully");
        Logger::Log(L"[SimpleAC] DLL Unloaded Successfully");
        
        // Logger 파이프 정리
        Logger::CleanupPipe();
        
        // 콘솔 해제
        //FreeConsole();
        
        break;
    }
    
    return TRUE;
} 
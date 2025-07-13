#include "integrity_checker.h"
#include <fstream>
#include <sstream>
#include <iomanip>
#include <wincrypt.h>
#include <iostream>
#include <filesystem>

#pragma comment(lib, "crypt32.lib")
#pragma comment(lib, "shell32.lib")

namespace fs = std::filesystem;

IntegrityChecker::IntegrityChecker() {
    InitializeFileList();
    InitializeEmbeddedHashes();
    game_path_ = FindLethalCompanyPath();
    progress_callback_ = nullptr;
}

IntegrityChecker::~IntegrityChecker() {
}

void IntegrityChecker::InitializeFileList() {
    // 10개 핵심 파일 목록 초기화 (실제 게임 구조 기반)
    file_list_.clear();
    file_list_.push_back({"Lethal Company.exe", "", "", "메인 실행 파일"});
    file_list_.push_back({"Lethal Company_Data/Managed/Assembly-CSharp.dll", "", "", "게임 로직 (핵심)"});
    file_list_.push_back({"UnityPlayer.dll", "", "", "Unity 엔진"});
    file_list_.push_back({"Lethal Company_Data/globalgamemanagers", "", "", "유니티 게임 매니저"});
    file_list_.push_back({"Lethal Company_Data/Managed/UnityEngine.CoreModule.dll", "", "", "Unity 핵심 시스템"});
    file_list_.push_back({"Lethal Company_Data/Managed/Unity.Netcode.Runtime.dll", "", "", "네트워킹 (멀티플레이어 치트)"});
    file_list_.push_back({"Lethal Company_Data/Managed/Unity.InputSystem.dll", "", "", "입력 시스템 (봇, 매크로)"});
    file_list_.push_back({"MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll", "", "", "Mono 런타임 (스크립트 실행)"});
    file_list_.push_back({"Lethal Company_Data/Plugins/x86_64/steam_api64.dll", "", "", "Steam API (서버 연결)"});
    file_list_.push_back({"Lethal Company_Data/Managed/UnityEngine.dll", "", "", "Unity 기본 기능"});
}

std::string IntegrityChecker::FindLethalCompanyPath() {
    std::vector<std::string> possiblePaths;
    
    // 일반적인 스팀 라이브러리 경로들
    possiblePaths.push_back("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company");
    possiblePaths.push_back("C:\\Steam\\steamapps\\common\\Lethal Company");
    possiblePaths.push_back("D:\\Steam\\steamapps\\common\\Lethal Company");
    possiblePaths.push_back("E:\\Steam\\steamapps\\common\\Lethal Company");
    
    // 사용자 문서 폴더
    char documentsPath[MAX_PATH];
    if (SUCCEEDED(SHGetFolderPathA(NULL, CSIDL_MYDOCUMENTS, NULL, 0, documentsPath))) {
        std::string docsPath = std::string(documentsPath) + "\\Steam\\steamapps\\common\\Lethal Company";
        possiblePaths.push_back(docsPath);
    }
    
    // 각 경로 확인
    for (const auto& path : possiblePaths) {
        if (fs::exists(path) && fs::exists(path + "\\Lethal Company.exe")) {
            return path;
        }
    }
    
    return "";
}

std::string IntegrityChecker::CalculateFileHash(const std::string& filepath) {
    return CalculateSHA256(filepath);
}

std::string IntegrityChecker::CalculateSHA256(const std::string& filepath) {
    HCRYPTPROV hProv = 0;
    HCRYPTHASH hHash = 0;
    BYTE rgbHash[32];
    DWORD cbHash = 0;
    std::string hashString = "";

    // 파일 열기
    std::ifstream file(filepath, std::ios::binary);
    if (!file.is_open()) {
        return "";
    }

    // CryptoAPI 초기화
    if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        file.close();
        return "";
    }

    // 해시 객체 생성
    if (!CryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0);
        file.close();
        return "";
    }

    // 파일 데이터 읽어서 해시 계산
    char buffer[1024];
            while (file.read(buffer, sizeof(buffer))) {
            if (!CryptHashData(hHash, (BYTE*)buffer, static_cast<DWORD>(file.gcount()), 0)) {
            CryptDestroyHash(hHash);
            CryptReleaseContext(hProv, 0);
            file.close();
            return "";
        }
    }

    // 마지막 청크 처리
            if (file.gcount() > 0) {
            if (!CryptHashData(hHash, (BYTE*)buffer, static_cast<DWORD>(file.gcount()), 0)) {
            CryptDestroyHash(hHash);
            CryptReleaseContext(hProv, 0);
            file.close();
            return "";
        }
    }

    // 해시 값 가져오기
    cbHash = 32;
    if (CryptGetHashParam(hHash, HP_HASHVAL, rgbHash, &cbHash, 0)) {
        // 16진수 문자열로 변환
        std::stringstream ss;
        for (DWORD i = 0; i < cbHash; i++) {
            ss << std::hex << std::setw(2) << std::setfill('0') << (int)rgbHash[i];
        }
        hashString = ss.str();
    }

    // 정리
    CryptDestroyHash(hHash);
    CryptReleaseContext(hProv, 0);
    file.close();

    return hashString;
}

void IntegrityChecker::InitializeEmbeddedHashes() {
    // 10개 테스트 exe,dll 파일의 해시값을 직접 내장
    std::vector<std::string> embedded_hashes = {
        "8046d0f3d3c7c72e9198732df5bf4b38c100b6ab5f8a0ce6bb6b01879eec1cbd",  // Lethal Company.exe
        "486013ae3c5092f424a36690d4e5590d0abd392c602d3e659788b47c64b5c2fa",  // Assembly-CSharp.dll
        "cbfa9dc252c1c11e4b00c7fbbc25cb6a26a35d0e06610090b0dab46a2bd7e776",  // UnityPlayer.dll
        "2accb4dab209c207f90ffed5252942907bbf7b8c508b72ca607e7003e94b41ca",  // globalgamemanagers
        "82765c5207acd23082d69e8ee787408097eab07cb992b07462fcc4bcf4a47f97",  // UnityEngine.CoreModule.dll
        "f5e986b88680085e91a3a1fcc8dc00e70a4da5a000284f7f198d79490e757e6a",  // Unity.Netcode.Runtime.dll
        "0374d20edd614615546b70ab974472f3f64fc21dcafda44d35ed8d9c5677f580",  // Unity.InputSystem.dll
        "d3bdcf2c0029c4109d02fd746d1a2124b24bf44aeb64f90493b6f0f89f434488",  // mono-2.0-bdwgc.dll
        "473f5a312b56519f347741b63f3dea590946b96ea40ef3803d5f452c39af2f1e",  // steam_api64.dll
        "a7cdf1998c2282742e8d56061f11a138950b42ac5457055863262e41b21ea88d"   // UnityEngine.dll
    };
    
    // 파일 목록에 해시값 할당
    for (size_t i = 0; i < file_list_.size() && i < embedded_hashes.size(); ++i) {
        file_list_[i].expected_hash = embedded_hashes[i];
    }
}

IntegrityResult IntegrityChecker::CheckIntegrity() {
    IntegrityResult result = {};    
    result.is_valid = true;
    result.total_files = static_cast<int>(file_list_.size());
    result.passed_files = 0;
    result.failed_files_count = 0;
    
    if (game_path_.empty()) {
        result.is_valid = false;
        result.message = "리썰 컴퍼니 경로를 찾을 수 없습니다.";
        return result;
    }
    
    // 경로 끝에 백슬래시가 없으면 추가
    std::string base_path = game_path_;
    if (!base_path.empty() && base_path.back() != '\\') {
        base_path += "\\";
    }
    
    for (size_t i = 0; i < file_list_.size(); ++i) {
        auto& file_info = file_list_[i];
        

        
        // 파일 경로 구성
        file_info.filepath = base_path + file_info.filename;
        
        // 파일 존재 확인
        if (!fs::exists(file_info.filepath)) {
            if (progress_callback_) {
                progress_callback_(static_cast<int>(i + 1), static_cast<int>(file_list_.size()), file_info.filename, "✗ 파일을 찾을 수 없음");
            }
            result.failed_files.push_back(file_info.filename + " (파일을 찾을 수 없음)");
            result.failed_files_count++;
            result.is_valid = false;
            continue;
        }
        
        // 해시 계산
        std::string current_hash = CalculateSHA256(file_info.filepath);
        if (current_hash.empty()) {
            if (progress_callback_) {
                progress_callback_(static_cast<int>(i + 1), static_cast<int>(file_list_.size()), file_info.filename, "✗ 해시 계산 실패");
            }
            result.failed_files.push_back(file_info.filename + " (해시 계산 실패)");
            result.failed_files_count++;
            result.is_valid = false;
        }
        else if (current_hash != file_info.expected_hash) {
            if (progress_callback_) {
                progress_callback_(static_cast<int>(i + 1), static_cast<int>(file_list_.size()), file_info.filename, "✗ 해시 불일치");
            }
            result.failed_files.push_back(file_info.filename);
            result.failed_files_count++;
            result.is_valid = false;
        }
        else {
            if (progress_callback_) {
                progress_callback_(static_cast<int>(i + 1), static_cast<int>(file_list_.size()), file_info.filename, "✓");
            }
            result.success_files.push_back(file_info.filename + " ✓");
            result.passed_files++;
        }
    }
    
    if (result.is_valid) {
        result.message = "무결성 검사 완료: 모든 파일이 정상입니다.";
    } else {
        result.message = "무결성 검사 실패: " + std::to_string(result.failed_files_count) + "개 파일이 변조되었습니다.";
    }
    
    return result;
}

bool IntegrityChecker::LaunchGame() {
    if (game_path_.empty()) {
        return false;
    }
    
    std::string game_exe_path = game_path_;
    if (game_exe_path.back() != '\\') {
        game_exe_path += "\\";
    }
    game_exe_path += "Lethal Company.exe";
    
    // 게임 실행
    STARTUPINFOA si = {0};
    PROCESS_INFORMATION pi = {0};
    si.cb = sizeof(si);
    
    bool success = CreateProcessA(
        game_exe_path.c_str(),  // 실행 파일 경로
        NULL,                   // 명령줄 인수
        NULL,                   // 프로세스 보안 속성
        NULL,                   // 스레드 보안 속성
        FALSE,                  // 핸들 상속
        0,                      // 생성 플래그
        NULL,                   // 환경 변수
        game_path_.c_str(),     // 현재 디렉토리
        &si,                    // 시작 정보
        &pi                     // 프로세스 정보
    );
    
    if (success) {
        // 프로세스 핸들 정리
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        return true;
    }
    
    return false;
} 
#pragma once
#include <string>
#include <vector>
#include <windows.h>
#include <shlobj.h>
#include <filesystem>

namespace fs = std::filesystem;

// 파일 정보 구조체
struct FileInfo {
    std::string filename;
    std::string filepath;
    std::string expected_hash;
    std::string description;
};

// 무결성 검사 결과 구조체
struct IntegrityResult {
    bool is_valid;
    std::string message;
    std::vector<std::string> failed_files;
    std::vector<std::string> success_files;
    int total_files;
    int passed_files;
    int failed_files_count;
};

class IntegrityChecker {
public:
    IntegrityChecker();
    ~IntegrityChecker();

    // SHA-256 해시 계산
    std::string CalculateFileHash(const std::string& filepath);
    
    // 내장된 해시 데이터 초기화
    void InitializeEmbeddedHashes();
    
    // 무결성 검사 실행
    IntegrityResult CheckIntegrity();
    
    // 게임 실행
    bool LaunchGame();
    
    // 리썰 컴퍼니 경로 찾기
    std::string FindLethalCompanyPath();
    
    // 게임 경로 설정
    void SetGamePath(const std::string& path) { game_path_ = path; }
    std::string GetGamePath() const { return game_path_; }
    
    // 파일 검사 진행률 콜백 함수 타입
    typedef void (*FileCheckProgressCallback)(int current, int total, const std::string& filename, const std::string& status);
    
    // 콜백 함수 설정
    void SetProgressCallback(FileCheckProgressCallback callback) { progress_callback_ = callback; }

private:
    std::vector<FileInfo> file_list_;
    std::string game_path_;
    FileCheckProgressCallback progress_callback_;
    
    // Windows CryptoAPI를 사용한 해시 계산
    std::string CalculateSHA256(const std::string& filepath);
    
    // 10개 핵심 파일 목록 초기화
    void InitializeFileList();
}; 
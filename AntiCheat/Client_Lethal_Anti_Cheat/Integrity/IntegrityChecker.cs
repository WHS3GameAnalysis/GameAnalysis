using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LethalAntiCheatLauncher.Integrity
{
    public class IntegrityChecker
    {
        private readonly List<FileIntegrityInfo> _fileList = new();
        private readonly string _gamePath;
        private readonly List<string> _embeddedHashes = new()
        {
            "8046d0f3d3c7c72e9198732df5bf4b38c100b6ab5f8a0ce6bb6b01879eec1cbd",
            "486013ae3c5092f424a36690d4e5590d0abd392c602d3e659788b47c64b5c2fa",
            "cbfa9dc252c1c11e4b00c7fbbc25cb6a26a35d0e06610090b0dab46a2bd7e776",
            "2accb4dab209c207f90ffed5252942907bbf7b8c508b72ca607e7003e94b41ca",
            "82765c5207acd23082d69e8ee787408097eab07cb992b07462fcc4bcf4a47f97",
            "f5e986b88680085e91a3a1fcc8dc00e70a4da5a000284f7f198d79490e757e6a",
            "0374d20edd614615546b70ab974472f3f64fc21dcafda44d35ed8d9c5677f580",
            "d3bdcf2c0029c4109d02fd746d1a2124b24bf44aeb64f90493b6f0f89f434488",
            "473f5a312b56519f347741b63f3dea590946b96ea40ef3803d5f452c39af2f1e",
            "a7cdf1998c2282742e8d56061f11a138950b42ac5457055863262e41b21ea88d"
        };

        public IntegrityChecker()
        {
            _gamePath = FindLethalCompanyPath();
            InitializeFileList();
        }

        private string FindLethalCompanyPath()
        {
            string[] paths = {
                @"C:\Program Files (x86)\Steam\steamapps\common\Lethal Company",
                @"C:\Steam\steamapps\common\Lethal Company",
                @"D:\Steam\steamapps\common\Lethal Company",
                @"D:\Games\Steam\steamapps\common\Lethal Company",
                @"E:\Steam\steamapps\common\Lethal Company",
                @"C:\SteamLibrary\steamapps\common\Lethal Company"
            };

            return paths.FirstOrDefault(p => File.Exists(Path.Combine(p, "Lethal Company.exe"))) ?? "";
        }

        private void InitializeFileList()
        {
            string[] filenames = {
                "Lethal Company.exe",
                "Lethal Company_Data/Managed/Assembly-CSharp.dll",
                "UnityPlayer.dll",
                "Lethal Company_Data/globalgamemanagers",
                "Lethal Company_Data/Managed/UnityEngine.CoreModule.dll",
                "Lethal Company_Data/Managed/Unity.Netcode.Runtime.dll",
                "Lethal Company_Data/Managed/Unity.InputSystem.dll",
                "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll",
                "Lethal Company_Data/Plugins/x86_64/steam_api64.dll",
                "Lethal Company_Data/Managed/UnityEngine.dll"
            };

            string[] descs = {
                "메인 실행 파일", "게임 로직", "Unity 엔진", "유니티 매니저", "Unity 핵심 시스템",
                "멀티플레이어", "입력 시스템", "Mono 런타임", "Steam API", "Unity 기능"
            };

            for (int i = 0; i < filenames.Length; i++)
            {
                _fileList.Add(new FileIntegrityInfo
                {
                    Filename = filenames[i],
                    Filepath = Path.Combine(_gamePath, filenames[i].Replace("/", "\\")),
                    ExpectedHash = _embeddedHashes[i],
                    Description = descs[i]
                });
            }
        }

        public IntegrityResult CheckIntegrity(Action<int, int, string, string>? callback = null)
        {
            IntegrityResult result = new()
            {
                IsValid = true,
                TotalFiles = _fileList.Count
            };

            foreach (var file in _fileList.Select((file, index) => (file, index)))
            {
                if (!File.Exists(file.file.Filepath))
                {
                    callback?.Invoke(file.index + 1, _fileList.Count, file.file.Filename, "✗ 파일 없음");
                    result.FailedFiles.Add(file.file.Filename);
                    result.FailedFilesCount++;
                    result.IsValid = false;
                    continue;
                }

                string hash = FileHashUtil.CalculateSHA256(file.file.Filepath);
                if (string.IsNullOrEmpty(hash))
                {
                    callback?.Invoke(file.index + 1, _fileList.Count, file.file.Filename, "✗ 해시 실패");
                    result.FailedFiles.Add(file.file.Filename);
                    result.FailedFilesCount++;
                    result.IsValid = false;
                    continue;
                }

                if (hash != file.file.ExpectedHash)
                {
                    callback?.Invoke(file.index + 1, _fileList.Count, file.file.Filename, "✗ 해시 불일치");
                    result.FailedFiles.Add(file.file.Filename);
                    result.FailedFilesCount++;
                    result.IsValid = false;
                }
                else
                {
                    callback?.Invoke(file.index + 1, _fileList.Count, file.file.Filename, "✓");
                    result.SuccessFiles.Add(file.file.Filename);
                    result.PassedFiles++;
                }
            }

            result.Message = result.IsValid ? "모든 파일 무결성 확인됨." : $"{result.FailedFilesCount}개 파일에서 문제가 발견됨.";
            return result;
        }

        public bool LaunchGame()
        {
            if (string.IsNullOrEmpty(_gamePath)) return false;

            string exePath = Path.Combine(_gamePath, "Lethal Company.exe");

            if (!File.Exists(exePath)) return false;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = _gamePath,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}


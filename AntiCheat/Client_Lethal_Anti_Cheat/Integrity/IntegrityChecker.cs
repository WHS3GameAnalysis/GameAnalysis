using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher.Integrity
{
    public class IntegrityChecker
    {
        private readonly string _gamePath;

        public IntegrityChecker()
        {
            _gamePath = FindLethalCompanyPath();
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





        // 서버 기반 무결성 검사 메서드 추가
        public async Task<IntegrityResult> CheckIntegrityWithServerAsync()
        {
            try
            {
                var result = await ServerHashManager.CheckIntegrityWithServerAsync();
                
                if (result.IsValid)
                {
                    LogManager.Log(LogSource.Integrity, "게임 실행 승인됨", Color.Green);
                }
                else
                {
                    LogManager.Log(LogSource.Integrity, $"게임 실행 거부됨: {result.Message}", Color.Red);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.Integrity, $"검사 중 오류 발생: {ex.Message}", Color.Red);
                return new IntegrityResult { IsValid = false, Message = $"오류: {ex.Message}" };
            }
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.DLLDetector
{
    public static class CheckDLL
    {
        private static Timer timer;

        public static void Start()
        {
            timer = new Timer(5000); // 5초마다 체크
            timer.Elapsed += CheckModules;
            timer.AutoReset = true;
            timer.Start();
        }

        public static void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        private static void CheckModules(object sender, ElapsedEventArgs e)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    string moduleName = module.ModuleName;

                    if (!AllowList.moduleList.Contains(moduleName))
                    {
                        //Console.WriteLine("\n[!] Detected Unknown Module:");
                        //Console.WriteLine(" - " + moduleName);
                        PipeLogger.Log($"[DLLDetector] [!] Detected Unknown Module: {moduleName}");
                        if (!CheckSignature.IsFileSigned(module.FileName))
                        {
                            //Console.WriteLine("[!] Detected Unsigned Module:");
                            //Console.WriteLine("- " + module.ModuleName);
                            PipeLogger.Log($"[DLLDetector] [!] Detected Unsigned Module: {module.ModuleName}");
                        }

                        try
                        {
                            string[] sections = PEParser.GetSectionNames(module.FileName);
                            //Console.WriteLine("  [+] Sections:");
                            PipeLogger.Log($"[DLLDetector] [+] Sections:");
                            foreach (var section in sections)
                            {
                                //Console.WriteLine("   - " + section);
                                PipeLogger.Log($"[DLLDetector] - {section}");
                            }

                            if (PEParser.HasSuspiciousSections(sections))
                            {
                                //Console.WriteLine("  [!] Suspicious packed sections detected!");
                                PipeLogger.Log($"[DLLDetector] [!] Suspicious packed sections detected!");
                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine("  [X] Failed to parse PE: " + ex.Message);
                            PipeLogger.Log($"[DLLDetector] [X] Failed to parse PE: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("[X] Error while checking modules: " + ex.Message);
                PipeLogger.Log($"[DLLDetector] [X] Error while checking modules: {ex.Message}");
            }
        }
    }

    public static class PEParser
    {
        private static readonly string[] SuspiciousSectionNames = {
                ".aspack", ".upx", "UPX0", "UPX1", "UPX2",
                ".themida",
                ".vmp0", ".vmp1", ".vmp2", ".vmprotect", "VMProtect",
                ".enigma1", ".enigma2", ".edata",
                ".pec", ".pec1", ".pec2",
                ".mpress1", ".mpress2",
                ".nsp0", ".nsp1",
                ".exedata", ".writable",
                ".obsidium", ".obs1", ".obs2",
                ".dotfuscator", ".xfg", ".xyz",
                ".confuser", ".cfr", ".cfx"
            };


        public static string[] GetSectionNames(string dllPath)
        {
            using (var stream = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(0x3C, SeekOrigin.Begin);
                int peHeaderOffset = reader.ReadInt32();

                stream.Seek(peHeaderOffset, SeekOrigin.Begin);
                uint signature = reader.ReadUInt32();
                if (signature != 0x00004550)
                    throw new InvalidDataException("Invalid PE signature");

                stream.Seek(2, SeekOrigin.Current);
                short numberOfSections = reader.ReadInt16();

                stream.Seek(12, SeekOrigin.Current);
                short sizeOfOptionalHeader = reader.ReadInt16();
                stream.Seek(2, SeekOrigin.Current);
                stream.Seek(sizeOfOptionalHeader, SeekOrigin.Current);

                var sections = new string[numberOfSections];
                for (int i = 0; i < numberOfSections; i++)
                {
                    byte[] nameBytes = reader.ReadBytes(8);
                    string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
                    sections[i] = name;

                    stream.Seek(32, SeekOrigin.Current);
                }

                return sections;
            }
        }

        public static bool HasSuspiciousSections(string[] sections)
        {
            foreach (var section in sections)
            {
                foreach (var suspicious in SuspiciousSectionNames)
                {
                    if (section.Equals(suspicious, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace Anticheat
{
    public static class CheckDLL
    {
        private static Timer timer;

        private static readonly HashSet<string> exceptList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // 기본 제외 모듈
            "mono-2.0-bdwgc",
            "monomod.utils",
            "mono.cecil",
            "mono.cecil.mdb",
            "mono.cecil.pdb",
            "mono.cecil.rocks",
            "monomod.runtimedetour",
            "monomod.utils.cil.ilgeneratorproxy",
            "mono.security",

            // 필터 추가 목록
            "lethal company.exe",
            "ntdll",
            "kernel32",
            "kernelbase",
            "unityplayer",
            "user32",
            "win32u",
            "gdi32",
            "gdi32full",
            "msvcp_win",
            "ucrtbase",
            "ole32",
            "combase",
            "rpcrt4",
            "version",
            "msvcrt",
            "shlwapi",
            "setupapi",
            "advapi32",
            "sechost",
            "shell32",
            "wintypes",
            "oleaut32",
            "imm32",
            "winmm",
            "crypt32",
            "ws2_32",
            "opengl32",
            "bcrypt",
            "winhttp",
            "hid",
            "dwmapi",
            "glu32",
            "dxcore",
            "shcore",
            "gameoverlayrenderer64",
            "psapi",
            "cfgmgr32",
            "kernel.appcore",
            "bcryptprimitives",
            "uxtheme",
            "windows.storage",
            "profapi",
            "iphlpapi",
            "nvunityplugin",
            "audioplugindissonance",
            "msctf",
            "d3d11",
            "dxgi",
            "directxdatabasehelper",
            "igd10iumd64",
            "apphelp",
            "igd10um64xe",
            "intelcontrollib",
            "igdgmm64",
            "igc64",
            "clbcatq",
            "wbemprox",
            "wbemcomn",
            "wbemsvc",
            "fastprox",
            "amsi",
            "userenv",
            "mpoav",
            "mmdevapi",
            "devobj",
            "audioses",
            "resourcepolicyclient",
            "powrprof",
            "umpdc",
            "windows.gaming.input",
            "inputhost",
            "coremessaging",
            "twinapi.appcore",
            "cryptbase",
            "wintrust",
            "msasn1",
            "dcomp",
            "microsoft.internal.warppal",
            "textinputframework",
            "windows.ui",
            "opus",
            "vcruntime140",
            "mscorlib",
            "bepinex.preloader",
            "bepinex",
            "system.core",
            "monomod.utils",
            "mono.cecil",
            "mono.cecil.mdb",
            "mono.cecil.pdb",
            "mono.cecil.rocks",
            "monomod.runtimedetour",
            "0harmony",
            "system",
            "monomod.utils.cil.ilgeneratorproxy",
            "harmonyxinterop",
            "mono.security",
            "system.configuration",
            "system.xml",
            "unityengine.coremodule",
            "unityengine",
            "unityengine.uimodule",
            "unityengine.xrmodule",
            "netstandard",
            "assembly-csharp-firstpass",
            "assembly-csharp",
            "unity.visualeffectgraph.runtime",
            "unity.jobs",
            "unity.renderpipelines.core.runtime",
            "unity.ai.navigation",
            "unity.services.core.environments.internal",
            "dissonancevoip",
            "unity.services.core",
            "unity.multiplayer.tools.networksolutioninterface",
            "unity.networking.transport",
            "unity.services.core.networking",
            "unity.probuilder.csg",
            "unity.netcode.components",
            "unity.multiplayer.tools.netstatsmonitor.configuration",
            "unity.services.core.scheduler",
            "unity.services.core.environments",
            "unity.services.relay",
            "unity.services.core.configuration",
            "unity.renderpipelines.highdefinition.config.runtime",
            "unity.multiplayer.tools.netstats",
            "unity.renderpipelines.core.shaderlibrary",
            "unity.collections",
            "unity.netcode.runtime",
            "unity.probuilder.poly2tri",
            "unity.textmeshpro",
            "unity.services.core.analytics",
            "unity.multiplayer.tools.common",
            "unity.profiling.core",
            "unity.renderpipelines.shadergraph.shadergraphlibrary",
            "unity.animation.rigging",
            "unity.burst",
            "clientnetworktransform",
            "unity.probuilder",
            "unity.services.core.internal",
            "unity.services.core.device",
            "unity.services.core.threading",
            "facepunch transport for netcode for gameobjects",
            "unity.services.qos",
            "unity.multiplayer.tools.networkprofiler.runtime",
            "unityengine.ui",
            "unity.services.core.telemetry",
            "unity.timeline",
            "unity.multiplayer.tools.netstatsmonitor.implementation",
            "unity.inputsystem",
            "unity.inputsystem.forui",
            "easytexteffects",
            "unity.animation.rigging.doccodeexamples",
            "unity.services.authentication",
            "unity.probuilder.kdtree",
            "unity.mathematics",
            "com.olegknyazev.softmask",
            "unity.multiplayer.tools.metrictypes",
            "unity.services.core.registration",
            "unity.multiplayer.tools.netstatsreporting",
            "unity.renderpipelines.highdefinition.runtime",
            "unity.multiplayer.tools.netstatsmonitor.component",
            "unity.collections.lowlevel.ilsupport",
            "facepunch.steamworks.win64",
            "unity.burst.unsafe",
            "newtonsoft.json",
            "amazingassets.terraintomesh",
            "lckr",
            "system.runtime",
            "system.xml.linq",
            "system.numerics",
            "system.runtime.serialization",
            "anticheat"
        };

        // 이미 감지된 DLL 저장
        private static readonly HashSet<string> alreadyDetected = new HashSet<string>();

        public static void Start()
        {
            timer = new Timer(3000); // 3초마다 체크
            timer.Elapsed += CheckModules;
            timer.AutoReset = true;
            timer.Start();
        }

        // ExitProcess
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void ExitProcess(uint uExitCode);

        private static void CheckModules(object sender, ElapsedEventArgs e)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var newDetectedModules = new List<string>();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    string rawName = module.ModuleName;
                    string cleanName = rawName.ToLowerInvariant().Replace(".dll", "").Replace(".exe", "");

                    // 필터링: .dll, .exe 전부 제거된 상태로 exceptList랑 비교
                    bool isExcepted = exceptList.Any(x =>
                        x.ToLowerInvariant().Replace(".dll", "").Replace(".exe", "") == cleanName);

                    if (!isExcepted && !alreadyDetected.Contains(cleanName))
                    {
                        newDetectedModules.Add(rawName); // 출력은 원본 이름으로
                        alreadyDetected.Add(cleanName);
                    }
                }

                if (newDetectedModules.Count > 0)
                {
                    Console.WriteLine("[!] Detected suspicious DLL(s):");
                    foreach (var module in newDetectedModules.OrderBy(x => x)) // 보기 좋게 정렬
                    {
                        Console.WriteLine("    - " + module);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Error: " + ex.Message);
            }
        }
    }
}

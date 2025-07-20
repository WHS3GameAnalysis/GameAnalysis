//using Lethal_Anti_Cheat.DebugDetector;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Net.Sockets;

//namespace Lethal_Anti_Cheat.DebugDetector
//{
//    public class MonoPortScanCheck : IDebugCheck
//    {
//        public string MethodName => "Mono Debug Port Scan (55000~57000)";
//        private readonly string host = "127.0.0.1";
//        private readonly int startPort = 55000;
//        private readonly int endPort = 55001;

//        public bool IsDebugged(Process _)
//        {
//            foreach (int port in PortRange())
//            {
//                //Console.WriteLine($"[DebugDetector] Scanning port {port} on {host}...");
//                if (IsPortOpen(host, port))
//                {
//                    //Console.WriteLine($"[DebugDetector] Port {port} is open, indicating a potential Mono debugger is attached.");
//                    return true;
//                }
//            }
//            return false;
//        }

//        private IEnumerable<int> PortRange()
//        {
//            for (int port = startPort; port <= endPort; port++)
//            {
//                yield return port;
//            }
//        }

//        private bool IsPortOpen(string host, int port)
//        {
//            try
//            {
//                using var client = new TcpClient();
//                var result = client.BeginConnect(host, port, null, null);
//                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
//                client.EndConnect(result);
//                return true;

//            }
//            catch
//            {
//                return false;
//                // Port is closed or unreachable
//            }

//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.DebugDetector
{
    public class MonoPortScanCheck : IDebugCheck
    {
        public string MethodName => "Mono Debug Port Scan (PID-based: 55000~57000)";

        private const int AF_INET = 2; // IPv4
        private readonly int startPort = 55000;
        private readonly int endPort = 57000;

        public bool IsDebugged(Process gameProcess)
        {
            int gamePid = gameProcess.Id;
            bool detected = false;

            foreach (var entry in GetAllTcpConnections())
            {
                if (entry.State != MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                    continue;

                if (entry.LocalPort < startPort || entry.LocalPort > endPort)
                    continue;

                if (entry.ProcessId == gamePid)
                {
                    PipeLogger.Log($"[DebugDetector] 게임 프로세스가 포트 {entry.LocalPort} 을 열고 있음 (PID={entry.ProcessId})");
                    detected = true;
                }
                else
                {
                    try
                    {
                        Process other = Process.GetProcessById(entry.ProcessId);
                        if (!other.HasExited)
                        {
                            PipeLogger.Log($"[DebugDetector] 포트 {entry.LocalPort} 을 연 다른 프로세스 감지됨: PID={entry.ProcessId}, Name={other.ProcessName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        PipeLogger.Log($"[DebugDetector] 포트 {entry.LocalPort} 확인 중 예외 발생: {ex.Message}");
                    }
                }
            }

            return detected;
        }

        private IEnumerable<TcpRow> GetAllTcpConnections()
        {
            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                if (GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) != 0)
                    yield break;

                int rowStructSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                int rowCount = Marshal.ReadInt32(tcpTablePtr);
                IntPtr rowPtr = IntPtr.Add(tcpTablePtr, 4);

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);

                    var localPort = BitConverter.ToUInt16(new byte[2] {
                        (byte)(row.LocalPort >> 8),
                        (byte)(row.LocalPort & 0xFF)
                    }, 0);

                    yield return new TcpRow
                    {
                        State = row.State,
                        LocalPort = localPort,
                        ProcessId = (int)row.OwningPid
                    };

                    rowPtr = IntPtr.Add(rowPtr, rowStructSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            TCP_TABLE_CLASS tblClass,
            uint reserved);

        private enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL
        }

        private enum MIB_TCP_STATE
        {
            MIB_TCP_STATE_CLOSED = 1,
            MIB_TCP_STATE_LISTEN = 2,
            MIB_TCP_STATE_SYN_SENT = 3,
            MIB_TCP_STATE_SYN_RCVD = 4,
            MIB_TCP_STATE_ESTAB = 5,
            MIB_TCP_STATE_FIN_WAIT1 = 6,
            MIB_TCP_STATE_FIN_WAIT2 = 7,
            MIB_TCP_STATE_CLOSE_WAIT = 8,
            MIB_TCP_STATE_CLOSING = 9,
            MIB_TCP_STATE_LAST_ACK = 10,
            MIB_TCP_STATE_TIME_WAIT = 11,
            MIB_TCP_STATE_DELETE_TCB = 12
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public MIB_TCP_STATE State;
            public uint LocalAddr;
            public uint LocalPort;
            public uint RemoteAddr;
            public uint RemotePort;
            public uint OwningPid;
        }

        private class TcpRow
        {
            public MIB_TCP_STATE State { get; set; }
            public int LocalPort { get; set; }
            public int ProcessId { get; set; }
        }
    }
}

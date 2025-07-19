using Lethal_Anti_Cheat.DebugDetector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace Lethal_Anti_Cheat.DebugDetector
{
    public class MonoPortScanCheck : IDebugCheck
    {
        public string MethodName => "Mono Debug Port Scan (55000~57000)";
        private readonly string host = "127.0.0.1";
        private readonly int startPort = 55000;
        private readonly int endPort = 55001;

        public bool IsDebugged(Process _)
        {
            foreach (int port in PortRange())
            {
                //Console.WriteLine($"[DebugDetector] Scanning port {port} on {host}...");
                if (IsPortOpen(host, port))
                {
                    //Console.WriteLine($"[DebugDetector] Port {port} is open, indicating a potential Mono debugger is attached.");
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<int> PortRange()
        {
            for (int port = startPort; port <= endPort; port++)
            {
                yield return port;
            }
        }

        private bool IsPortOpen(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                client.EndConnect(result);
                return true;

            }
            catch
            {
                return false;
                // Port is closed or unreachable
            }

        }
    }
}

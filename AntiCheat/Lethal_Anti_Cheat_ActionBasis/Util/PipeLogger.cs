using System.IO;
using System.IO.Pipes;

namespace Lethal_Anti_Cheat.Util
{
    public static class PipeLogger
    {
        //private const string PipeName = "AntiCheatPipe";
        // DLL 안에 static 필드로 유지
        private static NamedPipeClientStream _pipe;
        private static StreamWriter _writer;

        public static void Log(string message)
        {
            try
            {
                if (_pipe == null || !_pipe.IsConnected)
                {
                    _pipe = new NamedPipeClientStream(".", "AntiCheatPipe", PipeDirection.Out);
                    _pipe.Connect(500);
                    _writer = new StreamWriter(_pipe) { AutoFlush = true };
                }

                _writer.WriteLine(message);
            }
            catch
            {
                // 무시
            }
        }

    }
}

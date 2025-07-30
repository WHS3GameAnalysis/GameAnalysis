using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks; // Task 사용을 위해 추가
using System.Drawing;
using LethalAntiCheatLauncher.Util;


namespace LethalAntiCheatLauncher
{
    public static class PipeListener
    {
        private const string PipeName = "AntiCheatPipe";

        // 파이프 서버가 성공적으로 초기화되었음을 알리는 TaskCompletionSource
        private static TaskCompletionSource<bool> _pipeServerReadyTcs = new TaskCompletionSource<bool>();

        // Start 메서드를 async Task로 변경하여 외부에서 await 할 수 있도록 합니다.
        public static async Task Start()
        {
            // 이 Task.Run은 파이프 서버가 클라이언트 연결을 지속적으로 기다리는 루프를
            // 백그라운드 스레드에서 비동기적으로 실행합니다.
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    NamedPipeServerStream server = null;
                    try
                    {
                        server = new NamedPipeServerStream(
                            PipeName,
                            PipeDirection.In,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Message, // 이전에 수정된 부분, 그대로 유지
                            PipeOptions.Asynchronous);

                        LogManager.Log(LogSource.AntiCheat, "Pipe server created. Waiting for connection...", Color.Gray);

                        // 파이프 서버가 생성되고 연결 대기 상태가 되면 _pipeServerReadyTcs를 완료시킵니다.
                        // 이전에 완료되지 않은 경우에만 설정합니다.
                        if (!_pipeServerReadyTcs.Task.IsCompleted)
                        {
                            _pipeServerReadyTcs.TrySetResult(true);
                        }

                        await server.WaitForConnectionAsync(); // 비동기적으로 클라이언트 연결을 기다립니다.
                        LogManager.Log(LogSource.AntiCheat, "Pipe client connected.", Color.Green);

                        // 연결된 클라이언트와의 통신을 별도의 Task에서 처리합니다.
                        _ = Task.Run(() => HandleClientConnection(server));
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(LogSource.AntiCheat, $"PipeListener Error: {ex.Message}", Color.Red);
                        // 초기 파이프 서버 생성 실패 시, 대기 중인 Task에 예외를 전달합니다.
                        if (!_pipeServerReadyTcs.Task.IsCompleted)
                        {
                            _pipeServerReadyTcs.TrySetException(ex);
                        }
                        await Task.Delay(1000); // Thread.Sleep 대신 await Task.Delay 사용
                    }
                }
            });

            // _pipeServerReadyTcs.Task를 await 하여 파이프 서버가 준비될 때까지 기다립니다.
            // 이 Task가 완료되어야 Start() 메서드가 완료됩니다.
            await _pipeServerReadyTcs.Task;
        }

        private static async Task HandleClientConnection(NamedPipeServerStream server)
        {
            try
            {
                using (var reader = new StreamReader(server))
                {
                    while (server.IsConnected)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                // 기존 로그 분류 및 처리 로직 유지
                                if (line.Contains("[Behaviour]"))
                                {
                                    LogManager.Log(LogSource.Behavior, line, Color.Plum);
                                    BehaviorLogManager.SendLog(line);
                                }
                                else if (line.Contains("[DebugDetector]"))
                                {
                                    LogManager.Log(LogSource.Debug, line, Color.LightSkyBlue);
                                }
                                else if (line.Contains("[HarmonyPatchDetector]"))
                                {
                                    LogManager.Log(LogSource.Harmony, line, Color.MediumPurple);
                                }
                                else if (line.Contains("[ProcessWatcher]") || line.Contains("[NtProcessScanner]"))
                                {
                                    LogManager.Log(LogSource.Process, line, Color.Orange);
                                }
                                else if (line.Contains("[Reflection]"))
                                {
                                    LogManager.Log(LogSource.Reflection, line, Color.LightCoral);
                                }
                                else if (line.Contains("[DLLDetector]"))
                                {
                                    LogManager.Log(LogSource.DLL, line, Color.Yellow);
                                }
                                else if (line.Contains("[SimpleAC]") || line.Contains("[ProcessScanner]"))
                                {
                                    LogManager.Log(LogSource.SimpleAC, line, Color.Tomato);
                                }
                                else
                                {
                                    LogManager.Log(LogSource.AntiCheat, line, Color.Cyan);
                                }
                            }
                        }
                        else
                        {
                            // 클라이언트 연결 끊김
                            break;
                        }
                    }
                }
                LogManager.Log(LogSource.AntiCheat, "Pipe client disconnected.", Color.Yellow);
            }
            catch (IOException ex)
            {
                // 클라이언트가 강제로 연결을 끊었을 때 발생하는 일반적인 오류 (예: 파이프가 닫힘)
                LogManager.Log(LogSource.AntiCheat, $"Pipe client connection lost: {ex.Message}", Color.OrangeRed);
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.AntiCheat, $"Error handling client connection: {ex.Message}", Color.Red);
            }
            finally
            {
                // 클라이언트 연결 처리가 끝나면 해당 서버 스트림을 닫습니다.
                server?.Dispose();
            }
        }
    }
}
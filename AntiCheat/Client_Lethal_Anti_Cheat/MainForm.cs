using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LethalAntiCheatLauncher.Integrity;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher
{
    public partial class MainForm : Form
    {
        private readonly List<LogEntry> _allLogs = new List<LogEntry>();
        private readonly object _logLock = new object();
        private LogSource _currentFilter = LogSource.All;

        public MainForm()
        {
            InitializeComponent();
            SubscribeToLogEvents();
            SetupFilterButtons();
        }

        private void SubscribeToLogEvents()
        {
            LogManager.OnLogReceived += (logEntry) =>
            {
                lock (_logLock)
                {
                    _allLogs.Add(logEntry);
                }

                if (_currentFilter == LogSource.All || _currentFilter == logEntry.Source)
                {
                    AppendLogToUI(logEntry);
                }
            };
        }

        private void SetupFilterButtons()
        {
            btnFilterAll.Click += (s, e) => SetFilter(LogSource.All);
            btnFilterAntiCheat.Click += (s, e) => SetFilter(LogSource.AntiCheat);
            btnFilterIntegrity.Click += (s, e) => SetFilter(LogSource.Integrity);
            btnFilterHeartbeat.Click += (s, e) => SetFilter(LogSource.Heartbeat);
            btnFilterSimpleAC.Click += (s, e) => SetFilter(LogSource.SimpleAC);
            btnFilterDll.Click += (s, e) => SetFilter(LogSource.DLL);
            btnFilterBehavior.Click += (s, e) => SetFilter(LogSource.Behavior);
            btnFilterDebug.Click += (s, e) => SetFilter(LogSource.Debug);
            btnFilterHarmony.Click += (s, e) => SetFilter(LogSource.Harmony);
            btnFilterProcess.Click += (s, e) => SetFilter(LogSource.Process);
            btnFilterReflection.Click += (s, e) => SetFilter(LogSource.Reflection);
        }

        private void SetFilter(LogSource filter)
        {
            _currentFilter = filter;
            RefreshLogView();
        }

        private void RefreshLogView()
        {
            if (logRichTextBox.InvokeRequired)
            {
                logRichTextBox.Invoke(new Action(RefreshLogView));
                return;
            }

            logRichTextBox.Clear();
            List<LogEntry> filteredLogs;
            lock (_logLock)
            {
                filteredLogs = (_currentFilter == LogSource.All)
                    ? new List<LogEntry>(_allLogs) // Create a copy for safe iteration
                    : _allLogs.Where(log => log.Source == _currentFilter).ToList();
            }

            foreach (var logEntry in filteredLogs)
            {
                AppendLog(logEntry.Message, logEntry.Color);
            }
        }

        private void AppendLogToUI(LogEntry logEntry)
        {
            if (logRichTextBox.InvokeRequired)
            {
                logRichTextBox.Invoke(new Action(() => AppendLog(logEntry.Message, logEntry.Color)));
            }
            else
            {
                AppendLog(logEntry.Message, logEntry.Color);
            }
        }

        private void AppendLog(string message, Color color)
        {
            logRichTextBox.SelectionStart = logRichTextBox.TextLength;
            logRichTextBox.SelectionLength = 0;
            logRichTextBox.SelectionColor = color;
            logRichTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            logRichTextBox.ScrollToCaret();
        }

        private async void launchButton_Click(object sender, EventArgs e)
        {
            launchButton.Enabled = false;
            await RunAntiCheatProcess();
        }

        private async Task RunAntiCheatProcess()
        {
            try
            {
                var checker = new IntegrityChecker();

                // 진행률 표시 시작
                this.Invoke(new Action(() =>
                {
                    statusLabel.Text = "서버 무결성 검사 중...";
                    progressBar.Value = 25;
                    progressBar.Maximum = 100;
                }));

                var result = await checker.CheckIntegrityWithServerAsync();

                // 진행률 완료
                this.Invoke(new Action(() =>
                {
                    progressBar.Value = 100;
                }));

                if (!result.IsValid)
                {
                    LogManager.Log(LogSource.Integrity, "게임 실행 차단됨", Color.Red);
                    statusLabel.Text = "무결성 검사 실패";
                    MessageBox.Show("무결성 검사에 실패했습니다. 게임을 실행할 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    launchButton.Enabled = true;
                    return;
                }

                HeartbeatManager.Start();

                LogManager.Log(LogSource.AntiCheat, "Launching game...", Color.Cyan);
                statusLabel.Text = "Launching game...";

                if (!await Task.Run(() => checker.LaunchGame()))
                {
                    LogManager.Log(LogSource.AntiCheat, "Failed to launch the game. Administrator rights or path issue.", Color.Red);
                    statusLabel.Text = "Failed to launch game.";
                    MessageBox.Show("Failed to launch the game. It might be a path or permission issue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    launchButton.Enabled = true;
                    return;
                }

                statusLabel.Text = "Game running. Initializing anti-cheat...";

                // 파이프 서버가 완전히 준비될 때까지 기다립니다.
                await PipeListener.Start(); // <-- 이 부분을 수정했습니다.
                await Task.Run(() => SimpleACManager.LoadSimpleAC());
                InjectorManager.InjectWhenGameStarts();

                this.FormClosing += (s, ev) =>
                {
                    SimpleACManager.UnloadSimpleAC();
                    InjectorManager.UnloadAntiCheat();
                };

                statusLabel.Text = "Anti-Cheat is active.";
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.AntiCheat, $"An unexpected error occurred: {ex.Message}", Color.Red);
                statusLabel.Text = "An error occurred.";
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                launchButton.Enabled = true;
            }
        }
    }
}
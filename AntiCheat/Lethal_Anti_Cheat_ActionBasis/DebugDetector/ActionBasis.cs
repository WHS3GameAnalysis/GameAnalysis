using System;
using System.Collections.Generic;

namespace ActionBasis
{
    public struct Player
    {
        public string PlayerId;
        public Vectors Position;
        public Vectors LastPosition;
        public DateTime LastUpdateTime;
        public bool IsMoving;
        public bool IsRunning;

        public Player(string playerId)
        {
            PlayerId = playerId;
            Position = new Vectors(0, 0, 0);
            LastPosition = new Vectors(0, 0, 0);
            LastUpdateTime = DateTime.Now;
            IsMoving = false;
            IsRunning = false;
        }
    }

    public struct Vectors
    {
        public float X, Y, Z;

        public Vectors(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Distance(Vectors other)
        {
            return (float)Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2) + Math.Pow(Z - other.Z, 2));
        }
    }

    public enum DetectionLevel
    {
        Normal,      // 정상
        Suspicious,  // 의심
        Detected,    // 탐지
        Critical     // 심각 (즉시 조치)
    }

    public class SpeedHackDetection
    {
        private readonly float _maxWalkSpeed = 5.0f;     // 걷기 최대 속도
        private readonly float _maxRunSpeed = 12.0f;     // 뛰기 최대 속도
        private readonly float _suspiciousMultiplier = 1.5f;  // 의심 배수
        private readonly float _detectedMultiplier = 2.0f;    // 탐지 배수
        private readonly float _criticalMultiplier = 3.0f;    // 심각 배수

        private readonly Dictionary<string, Player> _players;
        private readonly Dictionary<string, List<float>> _speedHistory;
        private readonly int _maxHistorySize = 10;

        public event Action<string, DetectionLevel, string> OnSpeedHackDetected;

        public SpeedHackDetection()
        {
            _players = new Dictionary<string, Player>();
            _speedHistory = new Dictionary<string, List<float>>();
        }

        public void UpdatePlayerPosition(string playerId, Vectors newPosition, bool isRunning = false)
        {
            DateTime currentTime = DateTime.Now;

            // 플레이어 초기화
            if (!_players.ContainsKey(playerId))
            {
                _players[playerId] = new Player(playerId);
                _speedHistory[playerId] = new List<float>();
            }

            Player player = _players[playerId];

            // 이전 위치가 있으면 속도 계산
            if (player.LastUpdateTime != default(DateTime))
            {
                float timeDiff = (float)(currentTime - player.LastUpdateTime).TotalSeconds;

                if (timeDiff > 0)
                {
                    float distance = newPosition.Distance(player.Position);
                    float speed = distance / timeDiff;

                    // 속도 히스토리 업데이트
                    _speedHistory[playerId].Add(speed);
                    if (_speedHistory[playerId].Count > _maxHistorySize)
                    {
                        _speedHistory[playerId].RemoveAt(0);
                    }

                    // 스피드 핵 검사
                    DetectionLevel detection = AnalyzeSpeed(playerId, speed, isRunning);

                    if (detection != DetectionLevel.Normal)
                    {
                        string message = GenerateDetectionMessage(playerId, speed, isRunning, detection);
                        OnSpeedHackDetected?.Invoke(playerId, detection, message);
                    }
                }
            }

            // 플레이어 정보 업데이트
            player.LastPosition = player.Position;
            player.Position = newPosition;
            player.LastUpdateTime = currentTime;
            player.IsRunning = isRunning;
            player.IsMoving = newPosition.Distance(player.LastPosition) > 0.1f;

            _players[playerId] = player;
        }

        private DetectionLevel AnalyzeSpeed(string playerId, float speed, bool isRunning)
        {
            float maxAllowedSpeed = isRunning ? _maxRunSpeed : _maxWalkSpeed;

            // 즉시 임계값 검사
            if (speed > maxAllowedSpeed * _criticalMultiplier)
            {
                return DetectionLevel.Critical;
            }
            else if (speed > maxAllowedSpeed * _detectedMultiplier)
            {
                return DetectionLevel.Detected;
            }
            else if (speed > maxAllowedSpeed * _suspiciousMultiplier)
            {
                return DetectionLevel.Suspicious;
            }

            // 패턴 분석 (연속적으로 빠른 속도 유지)
            if (_speedHistory[playerId].Count >= 5)
            {
                var recentSpeeds = _speedHistory[playerId].GetRange(_speedHistory[playerId].Count - 5, 5);
                float avgSpeed = 0;
                foreach (var s in recentSpeeds)
                {
                    avgSpeed += s;
                }
                avgSpeed /= recentSpeeds.Count;

                if (avgSpeed > maxAllowedSpeed * _suspiciousMultiplier)
                {
                    return DetectionLevel.Suspicious;
                }
            }

            return DetectionLevel.Normal;
        }

        private string GenerateDetectionMessage(string playerId, float speed, bool isRunning, DetectionLevel level)
        {
            float maxAllowedSpeed = isRunning ? _maxRunSpeed : _maxWalkSpeed;
            string action = isRunning ? "뛰기" : "걷기";

            return $"[{level}] 플레이어 {playerId}: {action} 중 비정상 속도 {speed:F2} units/s (최대: {maxAllowedSpeed:F2})";
        }

        public void HandleDetection(string playerId, DetectionLevel level, string message)
        {
            Console.WriteLine($"🚨 스피드 핵 탐지: {message}");

            switch (level)
            {
                case DetectionLevel.Suspicious:
                    Console.WriteLine($"👀 플레이어 {playerId} 모니터링 강화");
                    // 로그만 남기고 계속 관찰
                    break;

                case DetectionLevel.Detected:
                    Console.WriteLine($"⚠️ 플레이어 {playerId} 경고 메시지 발송");
                    // 경고 메시지 또는 속도 제한
                    break;

                case DetectionLevel.Critical:
                    Console.WriteLine($"🔴 플레이어 {playerId} 즉시 강퇴 처리");
                    // 즉시 강퇴 또는 계정 정지
                    KickPlayer(playerId);
                    break;
            }
        }

        private void KickPlayer(string playerId)
        {
            Console.WriteLine($"플레이어 {playerId}가 강퇴되었습니다.");
            // 실제 게임에서는 여기서 강퇴 로직 구현
            _players.Remove(playerId);
            _speedHistory.Remove(playerId);
        }

        public void GetPlayerStats(string playerId)
        {
            if (_players.ContainsKey(playerId) && _speedHistory.ContainsKey(playerId))
            {
                var player = _players[playerId];
                var speeds = _speedHistory[playerId];

                Console.WriteLine($"=== 플레이어 {playerId} 통계 ===");
                Console.WriteLine($"현재 위치: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})");
                Console.WriteLine($"이동 상태: {(player.IsRunning ? "뛰기" : "걷기")}");

                if (speeds.Count > 0)
                {
                    float avgSpeed = 0;
                    float maxSpeed = 0;
                    foreach (var speed in speeds)
                    {
                        avgSpeed += speed;
                        if (speed > maxSpeed) maxSpeed = speed;
                    }
                    avgSpeed /= speeds.Count;

                    Console.WriteLine($"평균 속도: {avgSpeed:F2} units/s");
                    Console.WriteLine($"최대 속도: {maxSpeed:F2} units/s");
                }
            }
        }
    }

    // 사용 예시
    public class Program
    {
        public static void Main()
        {
            var speedDetector = new SpeedHackDetection();

            // 이벤트 핸들러 등록
            speedDetector.OnSpeedHackDetected += (playerId, level, message) =>
            {
                speedDetector.HandleDetection(playerId, level, message);
            };

            string testPlayer = "player_001";

            Console.WriteLine("=== 스피드 핵 탐지 테스트 ===\n");

            // 정상적인 이동
            Console.WriteLine("1. 정상적인 걷기 테스트");
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(0, 0, 0));
            System.Threading.Thread.Sleep(1000);
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(4, 0, 0)); // 4 units/s

            System.Threading.Thread.Sleep(500);

            // 정상적인 뛰기
            Console.WriteLine("\n2. 정상적인 뛰기 테스트");
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(4, 0, 0), true);
            System.Threading.Thread.Sleep(1000);
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(15, 0, 0), true); // 11 units/s

            System.Threading.Thread.Sleep(500);

            // 스피드 핵 시뮬레이션
            Console.WriteLine("\n3. 스피드 핵 시뮬레이션");
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(15, 0, 0));
            System.Threading.Thread.Sleep(1000);
            speedDetector.UpdatePlayerPosition(testPlayer, new Vectors(50, 0, 0)); // 35 units/s - 매우 빠름!

            System.Threading.Thread.Sleep(500);

            // 플레이어 통계 출력
            Console.WriteLine();
            speedDetector.GetPlayerStats(testPlayer);
        }
    }
}
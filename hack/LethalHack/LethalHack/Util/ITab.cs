using UnityEngine;

namespace LethalHack.Util
{
    /// <summary>
    /// 모든 탭이 구현해야 하는 기본 인터페이스
    /// </summary>
    public interface ITab
    {
        string TabName { get; }
        void DrawTab();
        void DrawWindows();
    }
} 
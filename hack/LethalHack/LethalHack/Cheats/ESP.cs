using UnityEngine;

namespace LethalHack.Cheats
{
    public class ESP : Cheat
    {
        private Camera mainCam;

        public override void Trigger()
        {
            if (mainCam == null)
                mainCam = Camera.main;

            EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.gameObject.activeSelf)
                    continue;

                Renderer rend = enemy.GetComponentInChildren<Renderer>();
                if (rend == null) continue;

                Bounds bounds = rend.bounds;

                Vector3 top = bounds.center + Vector3.up * bounds.extents.y;
                Vector3 bottom = bounds.center - Vector3.up * bounds.extents.y;

                Vector3 screenTop = mainCam.WorldToScreenPoint(top);
                Vector3 screenBottom = mainCam.WorldToScreenPoint(bottom);

                if (screenTop.z < 0 || screenBottom.z < 0)
                    continue;

                float height = Mathf.Abs(screenTop.y - screenBottom.y);
                float width = height / 2f;
                float x = screenBottom.x - width / 2f;
                float y = Screen.height - screenTop.y;

                // Box 그리기
                DrawBox(new Rect(x, y, width, height), Color.red);
            }
        }

        private void DrawBox(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;

            // 얇은 라인으로 사각형 테두리 그리기
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), Texture2D.whiteTexture); // top
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height, rect.width, 1), Texture2D.whiteTexture); // bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), Texture2D.whiteTexture); // left
            GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, 1, rect.height), Texture2D.whiteTexture); // right

            GUI.color = oldColor;
        }
    }
}

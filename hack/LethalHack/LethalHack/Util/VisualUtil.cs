using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static IngamePlayerSettings;

namespace LethalHack.Util
{
    public static class VisualUtil
    {
        public static GUIStyle StringStyle { get; set; } = new GUIStyle(GUI.skin.label);
        public static void DrawString(Vector2 position, string label, bool centered = true, bool alignMiddle = false, bool bold = false, bool forceOnScreen = false, int fontSize = -1)
        {
            var content = new GUIContent(label);

            StringStyle.fontSize = 14;
            StringStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

            var size = StringStyle.CalcSize(content);
            var upperLeft = centered ? position - size / 2f : position;
            var style = new GUIStyle(GUI.skin.label);

            if (alignMiddle) style.alignment = TextAnchor.MiddleCenter;
            else style.alignment = TextAnchor.MiddleLeft;

            if (bold) style.fontStyle = FontStyle.Bold;

            if (fontSize > 0) style.fontSize = fontSize;

            Rect pos = new Rect(upperLeft, size);

            if (forceOnScreen)
            {
                if (pos.x < 0) pos.x = 10;
                if (pos.y < 0) pos.y = 10;
                if (pos.x + pos.width > Screen.width) pos.x = Screen.width - pos.width - 10;
                if (pos.y + pos.height > Screen.height) pos.y = Screen.height - pos.height - 10;
            }

            GUI.Label(pos, content, style);
        }

        public static void DrawString(Vector2 position, string label, Color color, bool centered = true, bool alignMiddle = false, bool bold = false, bool forceOnScreen = false, int fontSize = -1)
        {
            Color color2 = GUI.color;
            DrawString(position, label, centered, alignMiddle, bold, forceOnScreen, fontSize);
            GUI.color = color2;
        }

        public static void DrawDistanceString(Vector2 position, string label, float distance, bool showDistance = true)
        {
            if (showDistance) label += "\n" + distance.ToString() + "m";
            DrawString(position, label, Color.red, true, true);
        }

        public static Bounds GetBounds(this GameObject gameObject)
        {
            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer != null) return renderer.bounds;
            Collider collider = gameObject.GetComponentInChildren<Collider>();
            if (collider != null) return collider.bounds;
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        public static Texture2D lineTex;
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            Matrix4x4 matrix = GUI.matrix;
            if (!lineTex) lineTex = new Texture2D(1, 1);
            Color color2 = GUI.color;
            GUI.color = color;
            float num = Vector3.Angle(pointB - pointA, Vector2.right);
            if (pointA.y > pointB.y) num = -num;
            Vector2 scale = new Vector2((pointB - pointA).magnitude, width);
            if (scale.x == 0 || scale.y == 0) return;
            GUIUtility.ScaleAroundPivot(scale, new Vector2(pointA.x, pointA.y + 0.5f));
            GUIUtility.RotateAroundPivot(num, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1f, 1f), lineTex);
            GUI.matrix = matrix;
            GUI.color = color2;
        }

        public static void DrawBoxOutline(GameObject GameObject, Color color, float thickness)
        {
            Bounds bounds = GameObject.GetBounds();
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[1] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
            corners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            corners[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            corners[6] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            corners[7] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            if (CameraUtil.WorldToScreen(corners[0], out var topL) && CameraUtil.WorldToScreen(corners[1], out var topR) && CameraUtil.WorldToScreen(corners[2], out var topRN) && CameraUtil.WorldToScreen(corners[3], out var topLN) && CameraUtil.WorldToScreen(corners[4], out var bottomLF) && CameraUtil.WorldToScreen(corners[5], out var bottomRF) && CameraUtil.WorldToScreen(corners[6], out var bottomRN) && CameraUtil.WorldToScreen(corners[7], out var bottomLN))
            {
                DrawLine(topL, topR, color, thickness);
                DrawLine(topR, topRN, color, thickness);
                DrawLine(topRN, topLN, color, thickness);
                DrawLine(topLN, topL, color, thickness);
                DrawLine(bottomLF, bottomRF, color, thickness);
                DrawLine(bottomRF, bottomRN, color, thickness);
                DrawLine(bottomRN, bottomLN, color, thickness);
                DrawLine(bottomLN, bottomLF, color, thickness);
                DrawLine(topL, bottomLF, color, thickness);
                DrawLine(topR, bottomRF, color, thickness);
                DrawLine(topRN, bottomRN, color, thickness);
                DrawLine(topLN, bottomLN, color, thickness);
            }
        }
    }
}

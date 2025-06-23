using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalHack.Util
{
    public static class VisualUtil
    {
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

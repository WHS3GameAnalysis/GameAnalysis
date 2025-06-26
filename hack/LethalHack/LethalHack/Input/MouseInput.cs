using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalHack.Input
{
    public class MouseInput : MonoBehaviour
    {
        private float Yaw = 0f;
        private float Pitch = 0f;

        private void Update()
        {
            if (Cursor.visible) return;

            Yaw += Mouse.current.delta.x.ReadValue() * 0.15f;
            Yaw = (Yaw + 360) % 360;

            Pitch -= Mouse.current.delta.y.ReadValue() * 0.15f;
            Pitch = Mathf.Clamp(Pitch, -90, 90);

            transform.eulerAngles = new Vector3(Pitch, Yaw, 0f);
        }
    }
}
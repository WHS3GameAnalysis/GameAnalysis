using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAnalysis.LethalHack
{
    internal class KBInput : MonoBehaviour
    {
        private float sprintMultiplier = 1f;

        private void Update()
        {
            if (Cursor.visible) return;

            Vector3 input = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) input += transform.forward;
            if (Keyboard.current.sKey.isPressed) input -= transform.forward;
            if (Keyboard.current.aKey.isPressed) input -= transform.right;
            if (Keyboard.current.dKey.isPressed) input += transform.right;
            if (Keyboard.current.spaceKey.isPressed) input += transform.up;
            if (Keyboard.current.leftCtrlKey.isPressed) input -= transform.up;

            if (input == Vector3.zero) return;

            sprintMultiplier = Keyboard.current.leftShiftKey.isPressed
                ? Mathf.Min(sprintMultiplier + (5f * Time.deltaTime), 5f)
                : 1f;

            float speed = 5f;
            transform.position += input * Time.deltaTime * speed * sprintMultiplier;
        }

    }
}

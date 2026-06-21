using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Assets
{
    public class InputController : MonoBehaviour
    {
        private readonly Dictionary<Key, Action> KEYBOARD_INPUTS = new Dictionary<Key, Action>() {
            { Key.Escape, HandlePauseTab },
            { Key.M, HandleMovesHistoryTab },
            { Key.A, HandlePositionAssessmentTab }
        };

        private readonly Dictionary<MouseButton, Action> MOUSE_INPUTS = new Dictionary<MouseButton, Action>() {
            { MouseButton.Left, HandleClick }
        };

        void Update()
        {
            foreach (var k in KEYBOARD_INPUTS)
            {
                if (Keyboard.current[k.Key].wasPressedThisFrame)
                {
                    k.Value.Invoke();
                }
            }

            foreach (var kvp in MOUSE_INPUTS)
            {
                if (IsMouseButtonPressed(kvp.Key))
                {
                    kvp.Value.Invoke();
                }
            }
        }

        private bool IsMouseButtonPressed(MouseButton button)
        {
            if (Mouse.current == null) return false;

            switch (button)
            {
                case MouseButton.Left:
                    return Mouse.current.leftButton.wasPressedThisFrame;
                case MouseButton.Right:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case MouseButton.Middle:
                    return Mouse.current.middleButton.wasPressedThisFrame;
                default:
                    return false;
            }
        }

        private static void HandleClick()
        {
            if (Game.IsPaused) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x >= Camera.main.pixelRect.width || mousePos.y >= Camera.main.pixelRect.height)
                return;

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit[] hit = Physics.RaycastAll(ray)
                                    .OrderBy(h => h.transform.position.y)
                                    .ToArray();

            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform.gameObject.TryGetComponent<BoardCell>(out BoardCell cell) && cell.IsHighlighted)
                {
                    BoardEntities.Instance.TryMakeMoveSelectedChecker(new Vector2Int(cell.X, cell.Y), OpponentType.Player);
                    return;
                }
                else if (hit[i].transform.gameObject.TryGetComponent<Checker>(out Checker checker))
                {
                    BoardEntities.Instance.TrySelectChecker(checker.Data, OpponentType.Player);
                    return;
                }
            }
        }

        private static void HandlePauseTab()
        {
            if (Game.IsPaused)
                Game.UnPause();
            else Game.Pause();
        }

        private static void HandlePositionAssessmentTab()
        {
            UIManager.Instance.SwitchTabState(UIManager.Tab.PositionAssessment);
        }

        private static void HandleMovesHistoryTab()
        {
            UIManager.Instance.SwitchTabState(UIManager.Tab.MovesHistory);
        }

    }
}
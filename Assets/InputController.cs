using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    public class InputController : MonoBehaviour
    {
        [SerializeField]
        private readonly Dictionary<KeyCode, Action> INPUT_EVENTS = new Dictionary<KeyCode, Action>() {
            { KeyCode.Mouse0, HandleClick },
            { KeyCode.Escape, HandlePauseTab },
            { KeyCode.M, HandleMovesHistoryTab }
        };

        [SerializeField] private static PauseTab _pauseTabObj;
        [SerializeField] private static MovesHistoryTab _movesHistoryTab;


        void Start()
        {

        }

        void Update()
        {
            if (Game.IsPaused) return;

            if (Input.anyKeyDown)
            {
                KeyCode[] keys = INPUT_EVENTS.Keys.ToArray();

                for (int i = 0; i < keys.Length; i++)
                {
                    if (Input.GetKeyDown(keys[i]))
                    {
                        INPUT_EVENTS[keys[i]]();
                    }
                }
            }
        }

        private static void HandleClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hit = Physics.RaycastAll(ray)
                                    .OrderBy(h => h.transform.position.y)
                                    .ToArray();

            for (int i = 0; i < hit.Length; i++)
            {
                if (Game.IsCheckerSelected)
                {
                    if (hit[i].transform.gameObject.TryGetComponent<BoardCell>(out BoardCell cell))
                    {
                        BoardEntities.Instance.TryMakeMoveSelectedChecker(cell, OpponentType.Player);
                        return;
                    }
                }

                if (hit[i].transform.gameObject.TryGetComponent<Checker>(out Checker checker))
                {
                    BoardEntities.Instance.TrySelectChecker(checker, OpponentType.Player);
                    return;
                }
            }
        }

        private static void HandlePauseTab()
        {
            if (Game.IsPaused)
            {
                _pauseTabObj.Hide();
                Game.UnPause();
            }
            else
            {
                _pauseTabObj.Show();
                Game.Pause();
            }
        }

        private static void HandleMovesHistoryTab()
        {
            if (_movesHistoryTab.IsVisible)
                _movesHistoryTab.Hide();
            else _movesHistoryTab.Show();
        }

    }
}
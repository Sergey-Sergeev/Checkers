using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class BoardEntities : MonoBehaviour
    {
        [SerializeField] private Vector3 _spawnPosition;
        [SerializeField] private Checker _checkerObj;
        [SerializeField] public static readonly Color CHECKER_COLOR_1 = GameSettings.CHECKER_COLOR_1;
        [SerializeField] public static readonly Color CHECKER_COLOR_2 = GameSettings.CHECKER_COLOR_2;

        public const int COMMAND_CHECKERS_COUNT = 24;
        public const float CHECKER_MOVE_SPEED = 5.0f;

        public Checker? Selected { get; private set; } = null;
        public BoardPosition CurrentPosition { get; private set; }
        public static BoardEntities Instance { get; private set; }

        private bool _isNeedContinueBeatCheckers = false;
        private List<Checker> _beatenCheckersInRow = new List<Checker>();

        void Start()
        {
            Instance = this;
            SetCheckers();
        }

        void Update()
        {

        }

        private void SetCheckers()
        {
            int count = 0;

            List<Checker> checkers = new List<Checker>();

            Color playerColor = CHECKER_COLOR_1;
            Color aiColor = CHECKER_COLOR_2;

            if (GameSettings.FirstMoveTurn == OpponentType.AI)
            {
                playerColor = CHECKER_COLOR_2;
                aiColor = CHECKER_COLOR_1;
            }

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
            {
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                {
                    if (count == COMMAND_CHECKERS_COUNT)
                    {
                        CurrentPosition = new BoardPosition(checkers);
                        return;
                    }

                    if ((y % 2 == 1 && x % 2 == 0) || (y % 2 == 0 && x % 2 == 1)) continue;

                    count++;

                    Checker checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(playerColor, x, y, CheckerType.USUAL, OpponentType.Player);
                    MoveChecker(checker, x, y);
                    checkers.Add(checker);

                    checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(aiColor, CheckersBoard.WIDTH - x, CheckersBoard.HEIGHT - y, CheckerType.USUAL, OpponentType.AI);
                    MoveChecker(checker, CheckersBoard.WIDTH - x, CheckersBoard.HEIGHT - y);
                    checkers.Add(checker);
                }
            }
        }

        public void TrySelectChecker(Checker checker, OpponentType opponent)
        {
            if (!(Game.CurrentMoveTurn == opponent &&
                !_isNeedContinueBeatCheckers &&
                checker != Selected &&
                checker.Opponent == opponent &&
                CurrentPosition.IsCheckerExist(checker) &&
                !checker.IsBeaten)) return;

            List<Vector2Int> moves = CurrentPosition.GetAllMoves(checker, out _);

            if (moves.Count == 0) return;

            if (Selected != null) UnSelectChecker();
            SelectChecker(checker, moves);
        }

        public void TryMakeMoveSelectedChecker(BoardCell cell, OpponentType opponent)
        {
            if (Game.CurrentMoveTurn != opponent ||
                Selected == null ||
                Selected.IsMoving) return;

            List<Vector2Int> moves = CurrentPosition.GetAllMoves(Selected, out bool isPrevMoveBeatChecker);

            if (moves.Count == 0 || !moves.Any(c => c.x == cell.X && c.y == cell.Y)) return;

            CheckersBoard.Instance.UnHighlightCells();
            MoveChecker(Selected, cell.X, cell.Y);
            CurrentPosition.MakeMove(Selected, new Vector2Int(cell.X, cell.Y), out List<Checker> beatenCheckers);

            _beatenCheckersInRow.AddRange(beatenCheckers);

            moves = CurrentPosition.GetAllMoves(Selected, out bool isNextMoveBeatChecker);

            if (isPrevMoveBeatChecker && isNextMoveBeatChecker)
            {
                _isNeedContinueBeatCheckers = true;
                SelectChecker(Selected, moves);
            }
            else
            {
                _isNeedContinueBeatCheckers = false;
                Selected = null;
                Game.SetOpponentTurn();
                RemoveBeatenCheckers();
            }
        }

        private void RemoveBeatenCheckers()
        {
            for (int i = 0; i < _beatenCheckersInRow.Count; i++)
                _beatenCheckersInRow[i].Destroy();
            _beatenCheckersInRow.Clear();
        }

        private void SelectChecker(Checker checker, List<Vector2Int> moves = null)
        {
            Selected = checker;
            CheckersBoard.Instance.HighlightCells(moves == null ? CurrentPosition.GetAllMoves(checker, out _) : moves);
        }

        private void UnSelectChecker()
        {
            Selected = null;
            CheckersBoard.Instance.UnHighlightCells();
        }

        private void MoveChecker(Checker checker, int x, int y)
        {
            Vector3 pos = CheckersBoard.Instance.GetCellWorldPosition(x, y);
            Vector3 targetPosition = new Vector3(pos.x, checker.transform.position.y, pos.z);
            StartCoroutine(MoveCheckerCoroutine(checker, targetPosition));
        }

        private IEnumerator MoveCheckerCoroutine(Checker checker, Vector3 targetPosition)
        {
            checker.IsMoving = true;
            Vector3 startPosition = checker.transform.position;

            float progress = 0f;
            while (progress < 1f)
            {
                progress += CHECKER_MOVE_SPEED * Time.deltaTime;
                checker.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                yield return null;
            }

            checker.transform.position = targetPosition;
            checker.IsMoving = false;
        }

    }
}
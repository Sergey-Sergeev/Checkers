using Assets.scripts.Core;
using Assets.scripts.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
    internal class BoardManager : MonoBehaviour
    {
        [SerializeField] private Vector3 _spawnPosition;
        [SerializeField] private Checker _checkerObj;

        public const float CHECKER_MOVE_SPEED = 2.0f;

        public bool IsAllCheckersMoved { get => _movingCheckersCount == 0; }
        public BoardPosition CurrentPosition { get; private set; }
        public Checker? Selected { get; private set; } = null;
        public static BoardManager Instance { get; private set; }


        public delegate void PositionChangedDelegate(CheckerData checker, CheckerMove move, bool isContinueBeating);
        public event PositionChangedDelegate PositionChangedEvent;

        private Vector3 _checkerSize;

        private static Color _aiColor;
        private static Color _playerColor;

        private List<CheckerData> _beatenCheckersInRow = new List<CheckerData>();
        private List<Checker> _checkers;
        private int _movingCheckersCount = 0;

        public BoardManager()
        {
            Instance = this;
        }

        void Awake()
        {
            _aiColor = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? GameSettings.Instance.CheckerColor2 : GameSettings.Instance.CheckerColor1;
            _playerColor = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? GameSettings.Instance.CheckerColor1 : GameSettings.Instance.CheckerColor2;
            _checkerSize = _checkerObj.GetComponent<Renderer>().bounds.size;
        }

        void Start()
        {
            if (GameSettings.Instance.IsCustomBoard)
                SetCustomBoard();
            else SetCheckers();
        }

        private void SetCustomBoard()
        {
            _checkers = new List<Checker>();

            for (int i = 0; i < GameSettings.Instance.CustomBoardPosition.Count; i++)
            {
                CheckerData checkerData = GameSettings.Instance.CustomBoardPosition[i];

                Checker checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                checker.Set(checkerData.Opponent == OpponentType.Player ? _playerColor : _aiColor, checkerData.X, checkerData.Y, checkerData.Type, checkerData.Opponent);
                _checkers.Add(checker);
                MoveChecker(checker, checkerData.X, checkerData.Y);
            }

            CurrentPosition = new BoardPosition(_checkers.Select(c => c.Data).ToList(), GameSettings.Instance.BoardWidth, GameSettings.Instance.BoardHeight);
        }

        private void SetCheckers()
        {
            int count = 0;

            _checkers = new List<Checker>();

            for (int y = 0; y < GameSettings.Instance.BoardHeight; y++)
            {
                for (int x = 0; x < GameSettings.Instance.BoardWidth; x++)
                {
                    if (count == GameSettings.Instance.OpponentCountOfChechers) break;

                    if ((x + y) % 2 == 1) continue;

                    count++;

                    Checker checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(_playerColor, x, y, CheckerType.Usual, OpponentType.Player);
                    _checkers.Add(checker);
                }
            }

            count = 0;

            for (int y = GameSettings.Instance.BoardHeight - 1; y >= 0; y--)
            {
                for (int x = GameSettings.Instance.BoardWidth - 1; x >= 0; x--)
                {
                    if (count == GameSettings.Instance.OpponentCountOfChechers) break;

                    if ((x + y) % 2 == 1) continue;

                    count++;

                    Checker checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(_aiColor, x, y, CheckerType.Usual, OpponentType.AI);
                    _checkers.Add(checker);
                }
            }

            CurrentPosition = new BoardPosition(_checkers.Select(c => c.Data).ToList(), GameSettings.Instance.BoardWidth, GameSettings.Instance.BoardHeight);

            for (int i = 0; i < _checkers.Count; i++)
                MoveChecker(_checkers[i], _checkers[i].Data.X, _checkers[i].Data.Y);
        }

        public bool TrySelectChecker(CheckerData checkerData, OpponentType opponent)
        {
            if (Selected != null && checkerData == Selected.Data)
                return true;

            if (GameManager.Instance.CurrentMoveTurn != opponent ||
                CurrentPosition.IsOpponentContinueBeating ||
                checkerData == null ||
                checkerData.Opponent != opponent ||
                !CurrentPosition.IsCheckerExist(checkerData)) return false;

            IReadOnlyList<CheckerMove> moves = checkerData.GetAllMovesForChecker(CurrentPosition);

            if (moves.Count == 0 || (!moves.First().IsBeatOpponentChecker && CurrentPosition.IsOpponentNeedBeatChecker(opponent))) return false;

            if (Selected != null)
                UnSelectChecker();

            SelectChecker(checkerData);
            return true;
        }

        public bool TryMakeMoveSelectedChecker(Vector2Int to, OpponentType opponent)
        {
            if (GameManager.Instance.CurrentMoveTurn != opponent ||
                Selected == null ||
                Selected.IsMoving ||
                !CurrentPosition.IsCheckerCanMoveAt(Selected.Data, to)) return false;

            Vector2Int from = new Vector2Int(Selected.Data.X, Selected.Data.Y);

            CheckersBoard.Instance.UnHighlightCells();
            MoveChecker(Selected, to.x, to.y);

            CheckerData updatedState = CurrentPosition.MakeMove(from, to, out CheckerData beatenCheckerData, out bool isCheckerTransformd);

            PositionChangedEvent?.Invoke(Selected.Data,
                new CheckerMove(from, to, Selected.Data.Opponent, beatenCheckerData != null),
                CurrentPosition.IsOpponentContinueBeating);

            Selected.Data = updatedState;

            if (isCheckerTransformd)
                Selected.TransformInKing();

            if (beatenCheckerData != null)
                _beatenCheckersInRow.Add(beatenCheckerData);

            if (CurrentPosition.IsOpponentContinueBeating)
            {
                SelectChecker(Selected.Data);
            }
            else
            {
                Selected = null;
                GameManager.Instance.SwitchOpponentTurn();
                RemoveBeatenCheckers();
            }

            return true;
        }

        private void RemoveBeatenCheckers()
        {
            for (int i = 0; i < _beatenCheckersInRow.Count; i++)
            {
                int index = _checkers.FindIndex(c => c.Data == _beatenCheckersInRow[i]);
                _checkers[index].Destroy();
                _checkers.RemoveAt(index);
            }
            _beatenCheckersInRow.Clear();
        }

        private void SelectChecker(CheckerData checkerData)
        {
            Selected = _checkers.Find(c => c.Data == checkerData);
            IReadOnlyList<CheckerMove> moves = checkerData.GetAllMovesForChecker(CurrentPosition);
            CheckersBoard.Instance.HighlightCells(moves.Select(m => new Vector2Int(m.To.x, m.To.y)).ToList());
        }

        private void UnSelectChecker()
        {
            Selected = null;
            CheckersBoard.Instance.UnHighlightCells();
        }

        private void MoveChecker(Checker checker, int x, int y)
        {
            _movingCheckersCount++;
            Vector3 cellPos = CheckersBoard.Instance.GetCellWorldPosition(x, y);
            Vector3 targetPosition = new Vector3(cellPos.x, cellPos.y + _checkerSize.y / 2f + CheckersBoard.Instance.CellSize.y / 2f, cellPos.z);
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

                Vector3 newPos = Vector3.Lerp(startPosition, targetPosition, progress);

                if (checker == null || checker.gameObject == null)
                    break;

                checker.transform.position = newPos;

                yield return null;
            }

            if (!(checker == null || checker.gameObject == null))
            {
                checker.transform.position = targetPosition;
                checker.IsMoving = false;
            }

            _movingCheckersCount--;
        }

    }
}
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
        [SerializeField] public const int OPPONENT_CHECKERS_COUNT = 12;
        [SerializeField] public const float CHECKER_MOVE_SPEED = 5.0f;

        public bool IsAllCheckersMoved { get => _movingCheckersCount == 0; }
        public Checker? Selected { get; private set; } = null;
        public BoardPosition CurrentPosition { get; private set; }
        public static BoardEntities Instance { get; private set; }
        public MovesHistory MovesHistory { get; private set; }
        public Vector3 CheckerSize { get; private set; }

        private bool _isNeedContinueBeatCheckers = false;
        private List<CheckerData> _beatenCheckersInRow = new List<CheckerData>();
        private List<Checker> _checkers;
        private int _movingCheckersCount = 0;

        void Awake()
        {
            Instance = this;
            MovesHistory = new MovesHistory();
            CheckerSize = _checkerObj.GetComponent<Renderer>().bounds.size;
        }

        void Start()
        {
            SetCheckers();
        }

        private void SetCheckers()
        {
            int count = 0;

            _checkers = new List<Checker>();

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
                    if (count == OPPONENT_CHECKERS_COUNT)
                    {
                        CurrentPosition = new BoardPosition(_checkers);
                        for (int i = 0; i < _checkers.Count; i++)
                        {
                            MoveChecker(_checkers[i], _checkers[i].Data.X, _checkers[i].Data.Y);
                        }
                        return;
                    }

                    if ((y % 2 == 1 && x % 2 == 0) || (y % 2 == 0 && x % 2 == 1)) continue;

                    count++;

                    Checker checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(playerColor, x, y, CheckerType.USUAL, OpponentType.Player);
                    _checkers.Add(checker);

                    checker = Instantiate<Checker>(_checkerObj, _spawnPosition, Quaternion.identity);
                    checker.Set(aiColor, CheckersBoard.WIDTH - 1 - x, CheckersBoard.HEIGHT - 1 - y, CheckerType.USUAL, OpponentType.AI);
                    _checkers.Add(checker);
                }
            }
        }

        public bool TrySelectChecker(CheckerData checkerData, OpponentType opponent)
        {
            if (Selected != null && checkerData == Selected.Data)
                return true;

            if (Game.CurrentMoveTurn != opponent ||
                _isNeedContinueBeatCheckers ||
                checkerData == null ||
                checkerData.Opponent != opponent ||
                !CurrentPosition.IsCheckerExist(checkerData)) return false;

            List<CheckerMove> moves = checkerData.GetAllMovesForChecker(CurrentPosition);

            if (moves.Count == 0 || (!moves.First().IsBeatOpponentChecker && CurrentPosition.IsOpponentNeedBeatChecker(opponent))) return false;

            if (Selected != null)
                UnSelectChecker();

            SelectChecker(checkerData, moves);
            return true;
        }

        public bool TryMakeMoveSelectedChecker(Vector2Int move, OpponentType opponent)
        {
            if (Game.CurrentMoveTurn != opponent ||
                Selected == null ||
                Selected.IsMoving ||
                !CurrentPosition.IsCheckerCanMoveAt(Selected.Data, move)) return false;

            CheckersBoard.Instance.UnHighlightCells();
            MoveChecker(Selected, move.x, move.y);

            CurrentPosition.MakeMove(Selected.Data, move, out _isNeedContinueBeatCheckers, out CheckerData beatenCheckerData, out bool isCheckerTransformd);
            MovesHistory.Add(Selected.Data, move, beatenCheckerData != null, _isNeedContinueBeatCheckers, isCheckerTransformd);

            if (isCheckerTransformd)
                Selected.TransformInKing();

            if (beatenCheckerData != null)
                _beatenCheckersInRow.Add(beatenCheckerData);

            if (_isNeedContinueBeatCheckers)
            {
                SelectChecker(Selected.Data);
            }
            else
            {
                Selected = null;
                Game.SwitchOpponentTurn();
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

        private void SelectChecker(CheckerData checkerData, List<CheckerMove> moves = null)
        {
            Selected = _checkers.Find(c => c.Data == checkerData);
            moves = moves == null ? checkerData.GetAllMovesForChecker(CurrentPosition) : moves;
            CheckersBoard.Instance.HighlightCells(moves.Select(m => new Vector2Int(m.To.x, m.To.y)).ToList());
        }

        private void UnSelectChecker()
        {
            Selected = null;
            CheckersBoard.Instance.UnHighlightCells();
        }

        private void MoveChecker(Checker checker, int x, int y)
        {
            Vector3 cellPos = CheckersBoard.Instance.GetCellWorldPosition(x, y);
            Vector3 targetPosition = new Vector3(cellPos.x, cellPos.y + CheckerSize.y / 2f + CheckersBoard.Instance.CellSize.y / 2f, cellPos.z);
            StartCoroutine(MoveCheckerCoroutine(checker, targetPosition));
        }

        private IEnumerator MoveCheckerCoroutine(Checker checker, Vector3 targetPosition)
        {
            _movingCheckersCount++;
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
            _movingCheckersCount--;
        }

    }
}
using Assets.scripts.Infrastructure;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.scripts.GameLogic
{
    internal class CheckersBoard : MonoBehaviour
    {
        public readonly Vector3 CENTRE_COORDS = Vector3.zero;

        [SerializeField] private BoardCell _boardCellObj;
        [SerializeField] private GameObject _lateralObj;
        [SerializeField] private GameObject _cornerObj;
        [SerializeField] private Material _cellMaterial_1;
        [SerializeField] private Material _cellMaterial_2;
        [SerializeField] private Material _boardCellSelectedMaterial;

        public static CheckersBoard Instance;

        public Vector3 FirstCellPosition { get; private set; }
        public Vector3 CellSize { get; private set; }


        private BoardCell[,] _cells;

        private void Awake()
        {
            Instance = this;
            CalculateConsts();
            CreateBoard();
        }

        public Vector3 GetCellWorldPosition(int x, int y)
        {
            float worldX = FirstCellPosition.x + x * CellSize.x;
            float worldZ = FirstCellPosition.z + y * CellSize.z;
            return new Vector3(worldX, FirstCellPosition.y, worldZ);
        }

        public void HighlightCells(List<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                _cells[cells[i].x, cells[i].y].IsHighlighted = true;
                _cells[cells[i].x, cells[i].y].GetComponent<Renderer>().material = _boardCellSelectedMaterial;
            }
        }

        public void UnHighlightCells()
        {
            for (int y = 0; y < GameSettings.Instance.BoardHeight; y++)
            {
                for (int x = 0; x < GameSettings.Instance.BoardWidth; x++)
                {
                    if (_cells[x, y].IsHighlighted)
                    {
                        _cells[x, y].IsHighlighted = false;
                        _cells[x, y].GetComponent<Renderer>().material = _cells[x, y].DefaultMaterial;
                    }
                }
            }
        }

        private void CalculateConsts()
        {
            CellSize = _boardCellObj.GetComponent<Renderer>().bounds.size;

            float leftSideX = CENTRE_COORDS.x - (CellSize.x * GameSettings.Instance.BoardWidth) / 2;
            float downSideZ = CENTRE_COORDS.z - (CellSize.z * GameSettings.Instance.BoardHeight) / 2;
            FirstCellPosition = new Vector3(leftSideX + CellSize.x / 2, CENTRE_COORDS.y, downSideZ + CellSize.z / 2);
        }

        private void CreateBoard()
        {
            _cells = new BoardCell[GameSettings.Instance.BoardWidth, GameSettings.Instance.BoardHeight];

            for (int y = 0; y < GameSettings.Instance.BoardHeight; y++)
            {
                for (int x = 0; x < GameSettings.Instance.BoardWidth; x++)
                {
                    if (x == 0 || y == 0 || x == GameSettings.Instance.BoardWidth - 1 || y == GameSettings.Instance.BoardHeight - 1)
                    {
                        if (x == 0) Instantiate(_lateralObj, GetCellWorldPosition(x, y), Quaternion.Euler(0, 0, 0));
                        if (y == GameSettings.Instance.BoardHeight - 1) Instantiate(_lateralObj, GetCellWorldPosition(x, y), Quaternion.Euler(0, 90, 0));
                        if (x == GameSettings.Instance.BoardWidth - 1) Instantiate(_lateralObj, GetCellWorldPosition(x, y), Quaternion.Euler(0, 180, 0));
                        if (y == 0) Instantiate(_lateralObj, GetCellWorldPosition(x, y), Quaternion.Euler(0, 270, 0));
                    }

                    BoardCell cell = Instantiate(_boardCellObj);
                    Material material = (y + (x % 2)) % 2 == 0 ? _cellMaterial_2 : _cellMaterial_1;

                    cell.Set(x, y, false, material);

                    float nextX = FirstCellPosition.x + CellSize.x * x;
                    float nextZ = FirstCellPosition.z + CellSize.z * y;
                    cell.transform.position = new Vector3(nextX, FirstCellPosition.y, nextZ);

                    cell.GetComponent<Renderer>().material = material;

                    _cells[x, y] = cell;
                }
            }

            Instantiate(_cornerObj, GetCellWorldPosition(0, 0), Quaternion.Euler(0, 0, 0));
            Instantiate(_cornerObj, GetCellWorldPosition(0, GameSettings.Instance.BoardHeight - 1), Quaternion.Euler(0, 90, 0));
            Instantiate(_cornerObj, GetCellWorldPosition(GameSettings.Instance.BoardWidth - 1, GameSettings.Instance.BoardHeight - 1), Quaternion.Euler(0, 180, 0));
            Instantiate(_cornerObj, GetCellWorldPosition(GameSettings.Instance.BoardWidth - 1, 0), Quaternion.Euler(0, 270, 0));
        }
    }
}
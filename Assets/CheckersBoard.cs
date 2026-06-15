using Assets;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
    [SerializeField] public readonly Vector3 CENTRE_COORDS = Vector3.zero;
    [SerializeField] public static readonly int HEIGHT = GameSettings.BoardHeight;
    [SerializeField] public static readonly int WIDTH = GameSettings.BoardWidth;
    [SerializeField] public readonly Color CELL_COLOR_1 = GameSettings.CELL_COLOR_1;
    [SerializeField] public readonly Color CELL_COLOR_2 = GameSettings.CELL_COLOR_2;

    [SerializeField] private static BoardCell _boardCellObj;
    [SerializeField] private static Material _boardCellDefaultMaterial;
    [SerializeField] private static Material _boardCellSelectedMaterial;
    [SerializeField] private static GameObject _lateralObj;
    [SerializeField] private static GameObject _cornerObj;

    private BoardCell[,] _cells;

    public Vector3 FIRST_CELL_POSITION;
    public float CELL_HEIGHT;
    public float CELL_WIDTH;


    public static CheckersBoard Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CalculateConsts();
        CreateBoard();
    }

    void Update()
    {

    }

    public Vector3 GetCellWorldPosition(int x, int y)
    {
        float worldX = FIRST_CELL_POSITION.x + x * CELL_WIDTH;
        float worldZ = FIRST_CELL_POSITION.z + y * CELL_HEIGHT;
        return new Vector3(worldX, FIRST_CELL_POSITION.y, worldZ);
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
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (_cells[x, y].IsHighlighted)
                {
                    _cells[x, y].IsHighlighted = false;
                    _cells[x, y].GetComponent<Renderer>().material = _boardCellDefaultMaterial;
                }                 
            }
        }
    }

    private void CalculateConsts()
    {
        CELL_WIDTH = _boardCellObj.transform.lossyScale.x;
        CELL_HEIGHT = _boardCellObj.transform.lossyScale.z;

        float leftSideX = CENTRE_COORDS.x - (CELL_WIDTH * WIDTH) / 2;
        float downSideZ = CENTRE_COORDS.z - (CELL_HEIGHT * HEIGHT) / 2;
        FIRST_CELL_POSITION = new Vector3(leftSideX + CELL_WIDTH / 2, CENTRE_COORDS.y, downSideZ + CELL_HEIGHT / 2);
    }

    private void CreateBoard()
    {
        _cells = new BoardCell[WIDTH, HEIGHT];

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (x == 0 || y == 0 || x == WIDTH - 1 || y == HEIGHT - 1)
                {
                    float deg = 0;

                    if (x == 0) deg = 90;
                    else if (x == WIDTH - 1) deg = 270;
                    else if (y == HEIGHT - 1) deg = 180;

                    Instantiate(_lateralObj, GetCellWorldPosition(x, y), Quaternion.Euler(0, deg, 0));
                }

                BoardCell cell = Instantiate(_boardCellObj);

                cell.X = x;
                cell.Y = y;
                cell.IsHighlighted = false;

                float nextX = FIRST_CELL_POSITION.x + CELL_WIDTH * x;
                float nextZ = FIRST_CELL_POSITION.z + CELL_HEIGHT * y;
                cell.transform.position = new Vector3(nextX, FIRST_CELL_POSITION.y, nextZ);

                Color color = (y + (x % 2)) % 2 == 0 ? CELL_COLOR_1 : CELL_COLOR_2;
                cell.GetComponent<Material>().color = color;

                _cells[x, y] = cell;
            }
        }

        Instantiate(_cornerObj, GetCellWorldPosition(0, 0), Quaternion.Euler(0, 0, 0));
        Instantiate(_cornerObj, GetCellWorldPosition(0, HEIGHT - 1), Quaternion.Euler(0, 90, 0));
        Instantiate(_cornerObj, GetCellWorldPosition(WIDTH - 1, HEIGHT - 1), Quaternion.Euler(0, 180, 0));
        Instantiate(_cornerObj, GetCellWorldPosition(WIDTH - 1, 0), Quaternion.Euler(0, 270, 0));
    }



}


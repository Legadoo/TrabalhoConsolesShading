using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLifeSimulation : MonoBehaviour
{
    public int gridSize = 50;               // Tamanho da grade
    public float cellSize = 1f;             // Tamanho de cada célula
    public GameObject cellPrefab;           // Prefab da célula

    private int[,] grid;                    // Grade de células
    private GameObject[,] cells;            // Array de objetos de célula

    private void Start()
    {
        CreateGrid();                       // Cria a grade
        InitializeGrid();                   // Inicializa o estado da grade
    }

    // Cria a grade de células
    private void CreateGrid()
    {
        grid = new int[gridSize, gridSize];
        cells = new GameObject[gridSize, gridSize];

        Vector3 startPosition = new Vector3(-(gridSize - 1) * cellSize / 2f, -(gridSize - 1) * cellSize / 2f, 0f);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 cellPosition = startPosition + new Vector3(x * cellSize, y * cellSize, 0f);
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                cells[x, y] = cell;
            }
        }
    }

    // Inicializa o estado da grade
    private void InitializeGrid()
    {
        // Código para definir o estado inicial da grade vai aqui
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseInput();              // Trata a entrada do mouse
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateGrid();                    // Atualiza o estado da grade
        }
    }

    // Lida com a entrada do mouse para alterar o estado das células
    private void HandleMouseInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedCell = hit.collider.gameObject;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (cells[x, y] == clickedCell)
                    {
                        grid[x, y] = 1 - grid[x, y];          // Alterna o estado da célula
                        UpdateCellColor(cells[x, y], grid[x, y]);
                        return;
                    }
                }
            }
        }
    }

    // Atualiza o estado da grade
    private void UpdateGrid()
    {
        int[,] newGrid = new int[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int neighbors = GetAliveNeighbors(x, y);

                if (grid[x, y] == 1)
                {
                    if (neighbors < 2 || neighbors > 3)
                    {
                        newGrid[x, y] = 0;              // A célula morre
                    }
                    else
                    {
                        newGrid[x, y] = 1;              // A célula sobrevive
                    }
                }
                else
                {
                    if (neighbors == 3)
                    {
                        newGrid[x, y] = 1;              // A célula nasce
                    }
                    else
                    {
                        newGrid[x, y] = 0;              // A célula permanece morta
                    }
                }
            }
        }

        grid = newGrid;
        UpdateCellColors();
    }

    // Obtém o número de vizinhos vivos de uma célula
    private int GetAliveNeighbors(int x, int y)
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                int neighborX = x + i;
                int neighborY = y + j;

                if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
                {
                    count += grid[neighborX, neighborY];
                }
            }
        }

        return count;
    }

    // Atualiza as cores das células com base no estado da grade
    private void UpdateCellColors()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                UpdateCellColor(cells[x, y], grid[x, y]);
            }
        }
    }

    // Atualiza a cor de uma célula com base em seu estado
    private void UpdateCellColor(GameObject cell, int state)
    {
        Renderer cellRenderer = cell.GetComponent<Renderer>();
        cellRenderer.material.color = state == 1 ? Color.white : Color.black;
    }
}
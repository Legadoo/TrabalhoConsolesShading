using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLifeSimulation1 : MonoBehaviour
{
    public int gridSize = 50;               // Tamanho da grade
    public float cellSize = 1f;             // Tamanho de cada célula
    public GameObject cellPrefab;           // Prefab da célula

    private int[,] grid;                    // Grade de células
    private GameObject[,] cells;            // Array de objetos de célula

    private bool isRunning = false;          // Indica se a simulação está em execução
    private bool useGPU = false;             // Indica se a simulação deve ser executada na GPU

    public ComputeShader computeShader;
    private int kernelIndex;
    private RenderTexture gameOfLifeTexture;

    private void Start()
    {
        CreateGrid();                       // Cria a grade
        InitializeGrid();                   // Inicializa o estado da grade
        InitializeGameOfLifeTexture();
    }

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

    private void InitializeGrid()
    {
      for (int x = 0; x < gridSize; x++)
    {
        for (int y = 0; y < gridSize; y++)
        {
            // Código para definir o estado inicial das células
             if (Random.value < 0.5f)
             {
                 grid[x, y] = 1; // Célula viva
                 UpdateCellColor(cells[x, y], 1); // Atualiza a cor da célula para indicar que ela está viva
             }
             else
             {
                 grid[x, y] = 0; // Célula morta
                 UpdateCellColor(cells[x, y], 0); // Atualiza a cor da célula para indicar que ela está morta
             }
        }
    }
    }

    private void Update()
    {   
        if(isRunning == true){
        UpdateGameOfLifeGPU();
        UpdateGrid();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseInput();              // Trata a entrada do mouse
        }
    
    }

    private void FixedUpdate(){
        // if(isRunning == true){
            
        //     UpdateGameOfLifeGPU();
        // }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0)) // Verifica se o botão esquerdo do mouse está pressionado
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
                        grid[x, y] = 1 - grid[x, y]; // Alterna o estado da célula
                        UpdateCellColor(cells[x, y], grid[x, y]); // Atualiza a cor da célula para refletir o novo estado
                        return;
                    }
                }
            }
        }
    }
    }

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
                    newGrid[x, y] = 0; // Célula morre
                }
                else
                {
                    newGrid[x, y] = 1; // Célula sobrevive
                }
            }
            else
            {
                if (neighbors == 3)
                {
                    newGrid[x, y] = 1; // Célula nasce
                }
                else
                {
                    newGrid[x, y] = 0; // Célula permanece morta
                }
            }
        }
    }

    grid = newGrid;
    UpdateCellColors();
    }

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

private void UpdateCellColor(GameObject cell, int state)
{
    Renderer cellRenderer = cell.GetComponent<Renderer>();
    cellRenderer.material.color = state == 1 ? Color.white : Color.black;
}

// private void InitializeGameOfLifeTexture()
// {
//     kernelIndex = computeShader.FindKernel("UpdateGameOfLife");
//     if(kernelIndex < 0)
//     {
//         Debug.LogError("KernelIndex Invalida");
//         return;
//     }



//     gameOfLifeTexture = new RenderTexture(gridSize, gridSize, 0, RenderTextureFormat.RInt);
//     gameOfLifeTexture.enableRandomWrite = true;
//     gameOfLifeTexture.filterMode = FilterMode.Point;
//     gameOfLifeTexture.Create();

// }


private void InitializeGameOfLifeTexture()
{
    gameOfLifeTexture = new RenderTexture(gridSize, gridSize, 0);
    gameOfLifeTexture.enableRandomWrite = true;
    gameOfLifeTexture.Create();

}

public void UpdateGameOfLifeGPU()
{
    kernelIndex = 0; //computeShader.FindKernel("UpdateGameOfLife");

    if (gameOfLifeTexture != null)
    {
        // Defina a textura no shader de computação
        computeShader.SetTexture(kernelIndex, "Result", gameOfLifeTexture);
        computeShader.Dispatch(kernelIndex, gridSize / 8, gridSize / 8, 1);
    }
    else
    {
        Debug.LogError("gameOfLifeTexture is null");
    }
    
}





    private void OnGUI()
    {
        float buttonWidth = 100f;
        float buttonHeight = 30f;
        float buttonSpacing = 10f;
        float startY = 10f;

        // Botão para iniciar/parar a simulação
        if (GUI.Button(new Rect(10f, startY, buttonWidth, buttonHeight), isRunning ? "Parar" : "Iniciar"))
        {
            isRunning = !isRunning;

            if (isRunning)
            {
                if (useGPU)
                {
                    // Iniciar a simulação na GPU
                    UpdateGameOfLifeGPU();
                }
                // else
                // {
                //     // Iniciar a simulação na CPU                  
                //     UpdateGrid();                  
                // }
            }
        }

        startY += buttonHeight + buttonSpacing;

        // Botão para alternar entre CPU e GPU
        if (GUI.Button(new Rect(10f, startY, buttonWidth, buttonHeight), useGPU ? "CPU" : "GPU"))
        {
            useGPU = !useGPU;
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SnakeGame
{
    private TMP_Text display;

    private int width = 30;
    private int height = 15;

    private Vector2Int direction = Vector2Int.right;
    private float moveTimer = 0f;
    private float moveInterval = 0.15f;

    private List<Vector2Int> snake = new List<Vector2Int>();
    private Vector2Int food;

    private bool isGameOver = false;
    private int score = 0;

    public SnakeGame(TMP_Text display)
    {
        this.display = display;
    }

    public void Init()
    {
        snake.Clear();
        snake.Add(new Vector2Int(5, 5));
        snake.Add(new Vector2Int(4, 5));
        snake.Add(new Vector2Int(3, 5));

        SpawnFood();
        Render();
    }

    public void Update()
    {
        if (isGameOver) return;

        ReadInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    private void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;
    }

    private void Move()
    {
        Vector2Int newHead = snake[0] + direction;

        // 벽 충돌
        if (newHead.x <= 0 || newHead.x >= width - 1 ||
            newHead.y <= 0 || newHead.y >= height - 1)
        {
            GameOver();
            return;
        }

        // 자기 몸 충돌
        for (int i = 0; i < snake.Count; i++)
        {
            if (snake[i] == newHead)
            {
                GameOver();
                return;
            }
        }

        // 이동
        snake.Insert(0, newHead);

        // 먹이
        if (newHead == food)
        {
            score++;
            SpawnFood();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }

        Render();
    }

    private void SpawnFood()
    {
        food = new Vector2Int(
            Random.Range(1, width - 2),
            Random.Range(1, height - 2)
        );
    }

    private void Render()
    {
        char[,] buffer = new char[width, height];

        // 빈 화면
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                buffer[x, y] = ' ';

        // 벽
        for (int x = 0; x < width; x++)
        {
            buffer[x, 0] = '#';
            buffer[x, height - 1] = '#';
        }

        for (int y = 0; y < height; y++)
        {
            buffer[0, y] = '#';
            buffer[width - 1, y] = '#';
        }

        // 뱀
        buffer[snake[0].x, snake[0].y] = 'O'; // 머리
        for (int i = 1; i < snake.Count; i++)
            buffer[snake[i].x, snake[i].y] = '@';

        // 먹이
        buffer[food.x, food.y] = '*';

        // 출력 문자열 조립
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(" SCORE: " + score);

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
                sb.Append(buffer[x, y]);

            sb.AppendLine();
        }

        display.text = sb.ToString();
    }

    private void GameOver()
    {
        isGameOver = true;
        display.text += "\n   GAME OVER!\n   Press close.";
    }
}

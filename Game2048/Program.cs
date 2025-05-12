using System;
using System.Collections.Generic;

class Game2048
{
    private int[,] board;
    private readonly int size = 4;
    public int Size => size;
    private readonly Random random = new Random();
    private int score = 0;
    private bool gameOver = false;

    public Game2048()
    {
        board = new int[size, size];
        AddRandomTile();
        AddRandomTile();
    }

    public void PrintBoard()
    {
        Console.Clear();
        Console.WriteLine($"Score: {score}\n");
        
        for (int i = 0; i < size; i++)
        {
            Console.WriteLine("+------+------+------+------+");
            for (int j = 0; j < size; j++)
            {
                Console.Write("| ");
                if (board[i, j] != 0)
                    Console.Write($"{board[i, j],4} ");
                else
                    Console.Write("    ");
            }
            Console.WriteLine("|");
        }
        Console.WriteLine("+------+------+------+------+");
        Console.WriteLine("\nUse arrow keys to move. Press 'q' to quit.");
    }

    public bool Move(Direction direction)
    {
        bool moved = false;
        int[,] oldBoard = (int[,])board.Clone();

        switch (direction)
        {
            case Direction.Up:
                moved = MoveUp();
                break;
            case Direction.Down:
                moved = MoveDown();
                break;
            case Direction.Left:
                moved = MoveLeft();
                break;
            case Direction.Right:
                moved = MoveRight();
                break;
        }

        if (moved)
        {
            AddRandomTile();
            if (IsGameOver())
            {
                gameOver = true;
                return true;
            }
        }
        return moved;
    }


    private bool MoveUp()
    {
        bool moved = false;
        for (int j = 0; j < size; j++)
        {
            int[] column = new int[size];
            for (int i = 0; i < size; i++)
                column[i] = board[i, j];

            bool columnMoved = MoveAndMerge(column);
            moved = moved || columnMoved;

            for (int i = 0; i < size; i++)
                board[i, j] = column[i];
        }
        return moved;
    }

    private bool MoveDown()
    {
        bool moved = false;
        for (int j = 0; j < size; j++)
        {
            int[] column = new int[size];
            for (int i = 0; i < size; i++)
                column[i] = board[size - 1 - i, j];

            bool columnMoved = MoveAndMerge(column);
            moved = moved || columnMoved;

            for (int i = 0; i < size; i++)
                board[size - 1 - i, j] = column[i];
        }
        return moved;
    }


    private bool MoveLeft()
    {
        bool moved = false;
        for (int i = 0; i < size; i++)
        {
            int[] row = new int[size];
            for (int j = 0; j < size; j++)
                row[j] = board[i, j];

            bool rowMoved = MoveAndMerge(row);
            moved = moved || rowMoved;

            for (int j = 0; j < size; j++)
                board[i, j] = row[j];
        }
        return moved;
    }

    private bool MoveRight()
    {
        bool moved = false;
        for (int i = 0; i < size; i++)
        {
            int[] row = new int[size];
            for (int j = 0; j < size; j++)
                row[j] = board[i, size - 1 - j];

            bool rowMoved = MoveAndMerge(row);
            moved = moved || rowMoved;

            for (int j = 0; j < size; j++)
                board[i, size - 1 - j] = row[j];
        }
        return moved;
    }


    private bool MoveAndMerge(int[] line)
    {
        int[] original = (int[])line.Clone();
        int writeIndex = 0;
        
        // Move all non-zero elements to the beginning
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] != 0)
            {
                line[writeIndex] = line[i];
                writeIndex++;
            }
        }
        
        // Fill the rest with zeros
        for (int i = writeIndex; i < line.Length; i++)
        {
            line[i] = 0;
        }
        
        // Merge adjacent equal numbers
        for (int i = 0; i < line.Length - 1; i++)
        {
            if (line[i] != 0 && line[i] == line[i + 1])
            {
                line[i] *= 2;
                score += line[i];
                
                // Shift remaining elements
                for (int j = i + 1; j < line.Length - 1; j++)
                {
                    line[j] = line[j + 1];
                }
                line[line.Length - 1] = 0;
            }
        }
        
        // Check if the line has changed
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] != original[i]) return true;
        }
        return false;
    }

    private void AddRandomTile()
    {
        var emptyCells = new List<Tuple<int, int>>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] == 0)
                {
                    emptyCells.Add(Tuple.Create(i, j));
                }
            }
        }

        if (emptyCells.Count > 0)
        {
            var cell = emptyCells[random.Next(emptyCells.Count)];
            board[cell.Item1, cell.Item2] = (random.Next(10) == 0) ? 4 : 2;
        }
    }

    private bool IsGameOver()
    {
        // Check for any empty cells
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] == 0)
                    return false;
            }
        }

        // Check for possible merges
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int current = board[i, j];
                // Check right
                if (j < size - 1 && board[i, j + 1] == current)
                    return false;
                // Check down
                if (i < size - 1 && board[i + 1, j] == current)
                    return false;
            }
        }

        return true;
    }

    public bool IsGameOverStatus() => gameOver;
    public int GetScore() => score;
    
    public Game2048 DeepCopy()
    {
        var copy = new Game2048
        {
            board = (int[,])board.Clone(),
            score = score,
            gameOver = gameOver
        };
        return copy;
    }
    
    public int GetCell(int i, int j) => board[i, j];
    
    public void SetCell(int i, int j, int value) => board[i, j] = value;
}

enum Direction
{
    Up,
    Right,
    Down,
    Left
}

class Program
{
    private static Direction FindBestMove(Game2048 game)
    {
        double bestScore = double.NegativeInfinity;
        Direction bestMove = Direction.Up;
        
        // Try all possible moves
        foreach (Direction move in Enum.GetValues(typeof(Direction)))
        {
            Game2048 gameCopy = game.DeepCopy();
            bool moved = gameCopy.Move(move);
            
            if (moved)
            {
                double score = Expectimax(gameCopy, 2, false);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }
        
        return bestMove;
    }
    
    private static double Expectimax(Game2048 game, int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || game.IsGameOverStatus())
        {
            return EvaluateBoard(game);
        }
        
        if (isMaximizingPlayer)
        {
            double maxEval = double.NegativeInfinity;
            
            foreach (Direction move in Enum.GetValues(typeof(Direction)))
            {
                Game2048 gameCopy = game.DeepCopy();
                bool moved = gameCopy.Move(move);
                
                if (moved)
                {
                    double eval = Expectimax(gameCopy, depth - 1, false);
                    maxEval = Math.Max(maxEval, eval);
                }
            }
            
            return maxEval != double.NegativeInfinity ? maxEval : 0;
        }
        else
        {
            // Chance node (tile placement)
            double sumEval = 0;
            int emptyCells = 0;
            
            // Count empty cells
            for (int i = 0; i < game.Size; i++)
            {
                for (int j = 0; j < game.Size; j++)
                {
                    if (game.GetCell(i, j) == 0)
                    {
                        emptyCells++;
                    }
                }
            }
            
            if (emptyCells == 0) return 0;
            
            // For each empty cell, try adding a 2 or 4
            for (int i = 0; i < game.Size; i++)
            {
                for (int j = 0; j < game.Size; j++)
                {
                    if (game.GetCell(i, j) == 0)
                    {
                        // Try adding a 2 (90% chance)
                        Game2048 gameCopy2 = game.DeepCopy();
                        gameCopy2.SetCell(i, j, 2);
                        sumEval += 0.9 * Expectimax(gameCopy2, depth - 1, true) / emptyCells;
                        
                        // Try adding a 4 (10% chance)
                        Game2048 gameCopy4 = game.DeepCopy();
                        gameCopy4.SetCell(i, j, 4);
                        sumEval += 0.1 * Expectimax(gameCopy4, depth - 1, true) / emptyCells;
                    }
                }
            }
            
            return sumEval;
        }
    }
    
    private static double EvaluateBoard(Game2048 game)
    {
        // Simple evaluation function that considers:
        // 1. Empty cells (more is better)
        // 2. Monotonicity (smoothly decreasing values)
        // 3. Smoothness (minimizing differences between adjacent tiles)
        // 4. Maximum tile value
        
        double score = 0;
        int emptyCells = 0;
        double smoothness = 0;
        double monotonicity = 0;
        int maxTile = 0;
        
        // Weights for each factor
        const double EMPTY_WEIGHT = 2.7;
        const double SMOOTHNESS_WEIGHT = 0.1;
        const double MONOTONICITY_WEIGHT = 1.0;
        const double MAX_TILE_WEIGHT = 1.0;
        
        // Check for empty cells and max tile
        for (int i = 0; i < game.Size; i++)
        {
            for (int j = 0; j < game.Size; j++)
            {
                int value = game.GetCell(i, j);
                if (value == 0)
                {
                    emptyCells++;
                }
                else
                {
                    maxTile = Math.Max(maxTile, value);
                }
            }
        }
        
        // Calculate smoothness (sum of absolute differences between adjacent tiles)
        for (int i = 0; i < game.Size; i++)
        {
            for (int j = 0; j < game.Size; j++)
            {
                int value = game.GetCell(i, j);
                if (value != 0)
                {
                    // Check right neighbor
                    if (j < game.Size - 1)
                    {
                        int right = game.GetCell(i, j + 1);
                        if (right != 0)
                        {
                            smoothness -= Math.Abs(value - right);
                        }
                    }
                    // Check bottom neighbor
                    if (i < game.Size - 1)
                    {
                        int bottom = game.GetCell(i + 1, j);
                        if (bottom != 0)
                        {
                            smoothness -= Math.Abs(value - bottom);
                        }
                    }
                }
            }
        }
        
        // Calculate monotonicity (prefer increasing/decreasing sequences)
        for (int i = 0; i < game.Size; i++)
        {
            for (int j = 0; j < game.Size - 1; j++)
            {
                int current = game.GetCell(i, j);
                int next = game.GetCell(i, j + 1);
                if (current > next)
                {
                    monotonicity += Math.Log2(current) - Math.Log2(next);
                }
                else if (next > current)
                {
                    monotonicity += Math.Log2(next) - Math.Log2(current);
                }
            }
        }
        
        // Combine all factors with weights
        score = (emptyCells * EMPTY_WEIGHT) +
               (smoothness * SMOOTHNESS_WEIGHT) +
               (monotonicity * MONOTONICITY_WEIGHT) +
               (Math.Log2(maxTile) * MAX_TILE_WEIGHT);
        
        return score;
    }
    
    private static void AutoPlay(Game2048 game)
    {
        while (!game.IsGameOverStatus())
        {
            Direction bestMove = FindBestMove(game);
            game.Move(bestMove);
            game.PrintBoard();
            System.Threading.Thread.Sleep(1); // Wait for 100ms
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        
        var game = new Game2048();
        
        Console.WriteLine("Press any key to start manual play, or 'A' for auto-play");
        var key = Console.ReadKey(true).Key;
        
        if (key == ConsoleKey.A)
        {
            AutoPlay(game);
        }
        else
        {
            while (true)
            {
                game.PrintBoard();
                if (game.IsGameOverStatus()) break;

                Console.WriteLine("Use W/A/S/D to move, Q to quit, A for auto-play");
                key = Console.ReadKey(true).Key;
                
                if (key == ConsoleKey.Q) break;
                if (key == ConsoleKey.A)
                {
                    AutoPlay(game);
                    break;
                }

                switch (key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        game.Move(Direction.Up);
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        game.Move(Direction.Right);
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        game.Move(Direction.Down);
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        game.Move(Direction.Left);
                        break;
                    default:
                        continue;
                }
            }
        }
        
        game.PrintBoard();
        if (game.IsGameOverStatus())
        {
            Console.WriteLine("\nGame Over! Final score: " + game.GetScore());
        }
    }
}

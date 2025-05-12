using System;
using System.Collections.Generic;
using System.Linq;

namespace Game2048Web.Models
{
    public class Game2048
    {
        // Using a jagged array for better JSON serialization
        private int[][] _board;
        private readonly int size = 4;
        private readonly Random random = new Random();
        
        public int[][] Board 
        { 
            get 
            {
                // Ensure the board is properly initialized
                if (_board == null || _board.Length != size)
                {
                    _board = new int[size][];
                    for (int i = 0; i < size; i++)
                    {
                        _board[i] = new int[size];
                    }
                }
                return _board; 
            }
            set 
            { 
                _board = value ?? new int[size][];
                // Ensure the board has the correct structure
                for (int i = 0; i < size; i++)
                {
                    _board[i] = _board[i] ?? new int[size];
                    if (_board[i].Length != size)
                    {
                        Array.Resize(ref _board[i], size);
                    }
                }
            } 
        }
        
        public int Size => size;
        public int Score { get; set; }
        public bool GameOver { get; set; }
        public bool IsAutoPlaying { get; set; }

        // Required for JSON serialization
        public Game2048()
        {
            // Initialize the board with default values
            Board = new int[size][];
            for (int i = 0; i < size; i++)
            {
                Board[i] = new int[size];
            }
            // Call Reset to set up the initial game state
            Reset();
        }
        

        
        private void InitializeBoard()
        {
            // The Board property getter will handle initialization
            var board = Board; // This ensures the board is initialized
            
            // Clear the board
            for (int i = 0; i < size; i++)
            {
                Array.Clear(board[i], 0, size);
            }
        }
        
        public void Reset()
        {
            InitializeBoard();
            Score = 0;
            GameOver = false;
            IsAutoPlaying = false;
            AddRandomTile();
            AddRandomTile();
        }

        public bool Move(Direction direction)
        {
            try 
            {
                // Create a deep copy of the current board for comparison
                var oldBoard = new int[size][];
                for (int i = 0; i < size; i++)
                {
                    oldBoard[i] = new int[size];
                    Array.Copy(Board[i], oldBoard[i], size);
                }

                bool moved = false;

                // Perform the move based on direction
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
                    // Add a new tile if the board changed
                    AddRandomTile();
                    
                    // Check if game is over after a successful move
                    GameOver = IsGameOver();
                }
                else if (IsGameOver())
                {
                    GameOver = true;
                }

                return moved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Move({direction}): {ex}");
                return false;
            }
        }

        private bool MoveUp()
        {
            try
            {
                bool moved = false;
                for (int j = 0; j < size; j++)
                {
                    int[] column = new int[size];
                    // Copy the column
                    for (int i = 0; i < size; i++)
                    {
                        column[i] = Board[i][j];
                    }

                    // Try to move and merge
                    if (MoveAndMerge(column))
                    {
                        moved = true;
                        // Update the column
                        for (int i = 0; i < size; i++)
                        {
                            Board[i][j] = column[i];
                        }
                    }
                }
                return moved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveUp: {ex}");
                return false;
            }
        }

        private bool MoveDown()
        {
            try
            {
                bool moved = false;
                for (int j = 0; j < size; j++)
                {
                    int[] column = new int[size];
                    // Copy the column in reverse order
                    for (int i = 0; i < size; i++)
                    {
                        column[i] = Board[size - 1 - i][j];
                    }

                    // Try to move and merge
                    if (MoveAndMerge(column))
                    {
                        moved = true;
                        // Update the column in reverse order
                        for (int i = 0; i < size; i++)
                        {
                            Board[size - 1 - i][j] = column[i];
                        }
                    }
                }
                return moved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveDown: {ex}");
                return false;
            }
        }


        private bool MoveLeft()
        {
            try
            {
                bool moved = false;
                for (int i = 0; i < size; i++)
                {
                    // Copy the row
                    int[] row = new int[size];
                    for (int j = 0; j < size; j++)
                    {
                        row[j] = Board[i][j];
                    }

                    // Try to move and merge
                    if (MoveAndMerge(row))
                    {
                        moved = true;
                        // Update the row
                        for (int j = 0; j < size; j++)
                        {
                            Board[i][j] = row[j];
                        }
                    }
                }
                return moved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveLeft: {ex}");
                return false;
            }
        }

        private bool MoveRight()
        {
            try
            {
                bool moved = false;
                for (int i = 0; i < size; i++)
                {
                    // Copy the row in reverse order
                    int[] row = new int[size];
                    for (int j = 0; j < size; j++)
                    {
                        row[j] = Board[i][size - 1 - j];
                    }

                    // Try to move and merge
                    if (MoveAndMerge(row))
                    {
                        moved = true;
                        // Update the row in reverse order
                        for (int j = 0; j < size; j++)
                        {
                            Board[i][size - 1 - j] = row[j];
                        }
                    }
                }
                return moved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveRight: {ex}");
                return false;
            }
        }


        private bool MoveAndMerge(int[] line)
        {
            try
            {
                if (line == null || line.Length == 0)
                    return false;

                // Create a copy of the original line for comparison
                int[] original = new int[line.Length];
                Array.Copy(line, original, line.Length);

                int index = 0;

                // Move all non-zero elements to the front
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != 0)
                    {
                        if (index != i)
                        {
                            line[index] = line[i];
                            line[i] = 0;
                        }
                        index++;
                    }
                }


                // Merge adjacent equal elements
                bool merged = false;
                for (int i = 0; i < line.Length - 1; i++)
                {
                    if (line[i] != 0 && line[i] == line[i + 1])
                    {
                        line[i] *= 2;
                        Score += line[i];
                        line[i + 1] = 0;
                        merged = true;
                        i++; // Skip the next element as it's been merged
                    }
                }


                // Move all non-zero elements to the front again
                index = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != 0)
                    {
                        if (index != i)
                        {
                            line[index] = line[i];
                            line[i] = 0;
                        }
                        index++;
                    }
                }


                // Check if the line has changed
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != original[i])
                    {
                        return true;
                    }
                }
                return merged;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveAndMerge: {ex}");
                return false;
            }
        }

        private void AddRandomTile()
        {
            var emptyCells = new List<Tuple<int, int>>();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (Board[i][j] == 0)
                    {
                        emptyCells.Add(Tuple.Create(i, j));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                var cell = emptyCells[random.Next(emptyCells.Count)];
                Board[cell.Item1][cell.Item2] = (random.Next(10) == 0) ? 4 : 2;
            }
        }


        private bool IsGameOver()
        {
            // Check for any empty cells
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (Board[i][j] == 0)
                        return false;
                }
            }

            // Check for possible merges
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int current = Board[i][j];
                    // Check right
                    if (j < size - 1 && Board[i][j + 1] == current)
                        return false;
                    // Check down
                    if (i < size - 1 && Board[i + 1][j] == current)
                        return false;
                }
            }

            return true;
        }


    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }
}

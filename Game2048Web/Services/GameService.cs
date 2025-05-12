using Game2048Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game2048Web.Services
{
    public class GameService
    {
        private Game2048 _game;
        private bool _isAutoPlaying;
        private readonly object _lock = new object();
        private CancellationTokenSource? _autoPlayCts;
        private Task? _autoPlayTask;

        public GameService()
        {
            _game = new Game2048();
            _isAutoPlaying = false;
        }

        public Game2048 GetGame()
        {
            lock (_lock)
            {
                var game = new Game2048();
                
                try
                {
                    // Ensure the source board is valid
                    if (_game?.Board == null || _game.Board.Length != game.Size)
                    {
                        // If the board is invalid, return a new game
                        return new Game2048();
                    }
                    
                    // Initialize the board with the correct size
                    game.Board = new int[game.Size][];
                    
                    // Copy the board
                    for (int i = 0; i < game.Size; i++)
                    {
                        // Ensure the row exists and has the correct size
                        if (_game.Board[i] == null || _game.Board[i].Length != game.Size)
                        {
                            game.Board[i] = new int[game.Size];
                        }
                        else
                        {
                            game.Board[i] = new int[game.Size];
                            Array.Copy(_game.Board[i], game.Board[i], game.Size);
                        }
                    }
                    
                    // Copy other properties
                    game.Score = _game.Score;
                    game.GameOver = _game.GameOver;
                    game.IsAutoPlaying = _isAutoPlaying; // Use the service's auto-play state
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetGame: {ex}");
                    // Return a new game if there's an error
                    return new Game2048();
                }
                
                return game;
            }
        }

        public void MakeMove(Direction direction)
        {
            lock (_lock)
            {
                _game.Move(direction);
            }
        }

        public void ResetGame()
        {
            lock (_lock)
            {
                _game = new Game2048();
                _isAutoPlaying = false;
                _autoPlayCts?.Cancel();
            }
        }

        public Task StartAutoPlay()
        {
            lock (_lock)
            {
                if (_isAutoPlaying) 
                    return Task.CompletedTask;
                    
                _isAutoPlaying = true;
                _game.IsAutoPlaying = true;
                _autoPlayCts = new CancellationTokenSource();
                _autoPlayTask = AutoPlayLoop(_autoPlayCts.Token);
                
                // Don't await the task here, just return it
                return Task.CompletedTask;
            }
        }

        private async Task AutoPlayLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool gameOver;
                    
                    // Check game over state within the lock
                    lock (_lock)
                    {
                        gameOver = _game.GameOver;
                    }
                    
                    if (gameOver)
                        break;
                    
                    // Find the best move and execute it
                    Direction bestMove = FindBestMove();
                    bool moved;
                    
                    lock (_lock)
                    {
                        if (cancellationToken.IsCancellationRequested) 
                            break;
                            
                        moved = _game.Move(bestMove);
                    }

                    if (moved)
                    {
                        // Add a small delay between moves
                        await Task.Delay(100, cancellationToken);
                    }
                    else
                    {
                        // If no move was made, add a small delay to prevent tight loop
                        await Task.Delay(10, cancellationToken);
                        
                        // If we can't move, the game is over
                        lock (_lock)
                        {
                            bool canMove = false;
                            // Check if any moves are possible
                            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                            {
                                var newBoard = _game.Board.Select(row => row.ToArray()).ToArray();
                                var tempGame = new Game2048 { Board = newBoard };
                                if (tempGame.Move(dir))
                                {
                                    canMove = true;
                                    break;
                                }
                            }
                            
                            if (!canMove)
                            {
                                _game.GameOver = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Auto-play was cancelled - this is expected
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AutoPlayLoop: {ex}");
            }
            finally
            {
                lock (_lock)
                {
                    _isAutoPlaying = false;
                    _game.IsAutoPlaying = false;
                    _autoPlayTask = null;
                    _autoPlayCts = null;
                }
            }
        }

        public async Task StopAutoPlay()
        {
            Task? taskToAwait = null;
            CancellationTokenSource? cts = null;
            
            lock (_lock)
            {
                if (!_isAutoPlaying) 
                    return;
                    
                _isAutoPlaying = false;
                _game.IsAutoPlaying = false;
                
                // Get references to the task and CTS before nulling them
                taskToAwait = _autoPlayTask;
                cts = _autoPlayCts;
                
                // Clear the references
                _autoPlayTask = null;
                _autoPlayCts = null;
            }
            
            // Cancel the CTS outside the lock to avoid deadlocks
            try
            {
                cts?.Cancel();
                
                // Wait for the task to complete if it exists
                if (taskToAwait != null)
                {
                    await taskToAwait;
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling the task
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping auto-play: {ex.Message}");
                throw;
            }
        }

        private Direction FindBestMove()
        {
            Direction bestMove = Direction.Up;
            double bestScore = double.NegativeInfinity;
            bool foundValidMove = false;

            try
            {
                // Try each possible direction
                foreach (Direction move in Enum.GetValues(typeof(Direction)))
                {
                    try
                    {
                        // Create a deep copy of the board
                        var newBoard = new int[_game.Board.Length][];
                        for (int i = 0; i < _game.Board.Length; i++)
                        {
                            if (_game.Board[i] == null)
                            {
                                newBoard[i] = new int[_game.Board.Length];
                                continue;
                            }
                            
                            newBoard[i] = new int[_game.Board[i].Length];
                            Array.Copy(_game.Board[i], newBoard[i], _game.Board[i].Length);
                        }
                        
                        var gameCopy = new Game2048
                        {
                            Board = newBoard,
                            Score = _game.Score,
                            GameOver = _game.GameOver,
                            IsAutoPlaying = _game.IsAutoPlaying
                        };

                        // Try to make the move
                        bool moved = gameCopy.Move(move);

                        if (moved)
                        {
                            foundValidMove = true;
                            
                            // Evaluate the move using the Expectimax algorithm
                            double score = Expectimax(gameCopy.Board, 2, false);
                            
                            // Update the best move if this one is better
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestMove = move;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error evaluating move {move}: {ex.Message}");
                    }
                }
                
                // If no valid moves were found, check if the game is actually over
                if (!foundValidMove)
                {
                    lock (_lock)
                    {
                        _game.GameOver = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindBestMove: {ex}");
            }
            
            return bestMove;
        }

        private double Expectimax(int[][] board, int depth, bool isMaximizing)
        {
            if (depth == 0 || IsTerminalNode(board))
                return EvaluateBoard(board);

            if (isMaximizing)
            {
                double maxEval = double.NegativeInfinity;
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    // Create a deep copy of the board
                    var newBoard = board.Select(row => row.ToArray()).ToArray();
                    var tempGame = new Game2048 { Board = newBoard, Score = 0 };
                    bool moved = tempGame.Move(direction);
                    if (moved)
                    {
                        double eval = Expectimax(tempGame.Board, depth - 1, false);
                        maxEval = Math.Max(maxEval, eval);
                    }
                }
                return maxEval == double.NegativeInfinity ? 0 : maxEval;
            }
            else
            {
                double totalScore = 0;
                int emptyCells = 0;

                // Count empty cells
                for (int i = 0; i < board.Length; i++)
                {
                    for (int j = 0; j < board[i].Length; j++)
                    {
                        if (board[i][j] == 0)
                            emptyCells++;
                    }
                }

                // Calculate the probability of each possible move
                double probability = 1.0 / emptyCells;

                // Calculate the expected score for each possible move
                for (int i = 0; i < board.Length; i++)
                {
                    for (int j = 0; j < board[i].Length; j++)
                    {
                        if (board[i][j] == 0)
                        {
                            // Simulate a 2 being placed in this cell
                            board[i][j] = 2;
                            double score = Expectimax(board, depth - 1, true);
                            totalScore += score * probability;
                            board[i][j] = 0;

                            // Simulate a 4 being placed in this cell
                            board[i][j] = 4;
                            score = Expectimax(board, depth - 1, true);
                            totalScore += score * probability;
                            board[i][j] = 0;
                        }
                    }
                }

                return totalScore;
            }
        }

        private bool IsTerminalNode(int[][] board)
        {
            // Check if there are any empty cells
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] == 0)
                        return false;
                }
            }

            // Check if there are any possible merges
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    int current = board[i][j];
                    // Check right
                    if (j < board[i].Length - 1 && board[i][j + 1] == current)
                        return false;
                    // Check down
                    if (i < board.Length - 1 && board[i + 1][j] == current)
                        return false;
                }
            }

            return true;
        }

        private double EvaluateBoard(int[][] board)
        {
            double score = 0;
            int size = board.Length;
            int emptyCells = 0;
            double smoothness = 0;
            double monotonicity = 0;
            int maxTile = 0;

            // Check for empty cells and max tile
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    int value = board[i][j];
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
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    int value = board[i][j];
                    if (value != 0)
                    {
                        // Check right neighbor
                        // Check right neighbor
                        if (j < board[i].Length - 1)
                        {
                            int right = board[i][j + 1];
                            if (right != 0)
                            {
                                smoothness -= Math.Abs(value - right);
                            }
                        }
                        // Check bottom neighbor
                        if (i < board.Length - 1)
                        {
                            int bottom = board[i + 1][j];
                            if (bottom != 0)
                            {
                                smoothness -= Math.Abs(value - bottom);
                            }
                        }
                    }
                }
            }


            // Calculate monotonicity (prefer increasing/decreasing sequences)
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length - 1; j++)
                {
                    int current = board[i][j];
                    int next = board[i][j + 1];
                    if (current > 0 && next > 0)
                    {
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
            }


            // Define weights for evaluation factors
            const double EMPTY_WEIGHT = 2.7;
            const double SMOOTHNESS_WEIGHT = 0.1;
            const double MONOTONICITY_WEIGHT = 1.0;
            const double MAX_TILE_WEIGHT = 1.0;

            // Combine all factors with weights
            score = (emptyCells * EMPTY_WEIGHT) +
                   (smoothness * SMOOTHNESS_WEIGHT) +
                   (monotonicity * MONOTONICITY_WEIGHT) +
                   (maxTile > 0 ? Math.Log2(maxTile) * MAX_TILE_WEIGHT : 0);

            return score;
        }
    }
}

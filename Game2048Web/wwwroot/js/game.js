document.addEventListener('DOMContentLoaded', () => {
    const gameBoard = document.getElementById('game-board');
    const scoreElement = document.getElementById('score');
    const bestScoreElement = document.getElementById('best-score');
    const newGameButton = document.getElementById('new-game');
    const autoPlayButton = document.getElementById('auto-play');
    const stopAutoPlayButton = document.getElementById('stop-auto-play');
    const colorSchemeSelect = document.getElementById('color-scheme');

    let boardSize = 4;
    let gameState = {
        board: Array(boardSize).fill().map(() => Array(boardSize).fill(0)),
        score: 0,
        gameOver: false,
        isAutoPlaying: false
    };
    
    let pollInterval;

    // Initialize the game
    function initGame() {
        createGrid();
        fetchGameState();
        setupEventListeners();
        initColorScheme();
    }
    
    // Initialize color scheme
    function initColorScheme() {
        // Get saved scheme from localStorage or default to 'default'
        const savedScheme = localStorage.getItem('colorScheme') || 'default';
        
        // Apply the saved scheme
        applyColorScheme(savedScheme);
        
        // Update the select element
        colorSchemeSelect.value = savedScheme;
    }
    
    // Apply color scheme
    function applyColorScheme(scheme) {
        // Remove any existing scheme classes
        document.body.classList.remove('scheme-default', 'scheme-dark', 'scheme-pastel');
        
        // Add the selected scheme class
        document.body.classList.add(`scheme-${scheme}`);
        
        // Save preference to localStorage
        localStorage.setItem('colorScheme', scheme);
    }

    // Create the game grid
    function createGrid() {
        gameBoard.innerHTML = '';
        gameBoard.style.gridTemplateColumns = `repeat(${boardSize}, 1fr)`;
        gameBoard.style.gridTemplateRows = `repeat(${boardSize}, 1fr)`;
        
        for (let i = 0; i < boardSize; i++) {
            for (let j = 0; j < boardSize; j++) {
                const cell = document.createElement('div');
                cell.className = 'grid-cell';
                cell.dataset.row = i;
                cell.dataset.col = j;
                gameBoard.appendChild(cell);
            }
        }
    }

    // Fetch game state from the server
    async function fetchGameState() {
        try {
            const response = await fetch('/api/game');
            if (!response.ok) throw new Error('Failed to fetch game state');
            
            const data = await response.json();
            updateGameState(data);
        } catch (error) {
            console.error('Error fetching game state:', error);
        }
    }

    // Update the game state and UI
    function updateGameState(state) {
        if (!state) {
            console.error('Received null or undefined game state');
            return;
        }
        
        console.log('Received game state:', state);
        
        // Handle both lowercase 'board' and uppercase 'Board' properties
        const boardData = state.board || state.Board || [];
        
        // Safely update the game state
        gameState = { 
            ...state,
            // Normalize property names to lowercase for consistency
            board: Array.isArray(boardData) ? boardData : [],
            score: state.score || state.Score || 0,
            gameOver: state.gameOver || state.GameOver || false,
            isAutoPlaying: state.isAutoPlaying || state.IsAutoPlaying || false
        };
        
        // Update score
        const score = parseInt(gameState.score) || 0;
        scoreElement.textContent = score;
        
        // Update best score
        const currentBestScore = parseInt(localStorage.getItem('bestScore') || '0');
        const bestScore = Math.max(score, currentBestScore);
        if (score > currentBestScore) {
            localStorage.setItem('bestScore', score.toString());
        }
        bestScoreElement.textContent = bestScore;
        
        // Update board
        try {
            updateBoard();
        } catch (error) {
            console.error('Error updating board:', error);
        }
        
        // Update auto-play button visibility
        const isAutoPlaying = !!gameState.isAutoPlaying;
        autoPlayButton.style.display = isAutoPlaying ? 'none' : 'block';
        stopAutoPlayButton.style.display = isAutoPlaying ? 'block' : 'none';
        
        // Show game over message if needed
        if (gameState.gameOver) {
            showGameOver();
        }
    }

    // Update the game board UI
    function updateBoard() {
        try {
            // Remove all tiles
            document.querySelectorAll('.tile').forEach(tile => tile.remove());
            
            // Make sure we have a valid game state
            if (!gameState) {
                console.error('Game state is not defined');
                return;
            }
            
            // Get the board data, handling both lowercase and uppercase property names
            const boardData = gameState.board || gameState.Board;
            
            // Ensure we have a valid board
            if (!boardData) {
                console.error('Board is undefined or null:', gameState);
                return;
            }
            
            if (!Array.isArray(boardData)) {
                console.error('Board is not an array:', boardData);
                return;
            }
            
            // Log the board structure for debugging
            console.log('Board structure:', JSON.stringify(boardData));
            
            // Create a safe board to work with
            const safeBoard = [];
            for (let i = 0; i < boardSize; i++) {
                safeBoard[i] = Array.isArray(boardData[i]) ? 
                    boardData[i].slice(0, boardSize) : 
                    Array(boardSize).fill(0);
                
                // Ensure each row has the correct length
                while (safeBoard[i].length < boardSize) {
                    safeBoard[i].push(0);
                }
            }
            
            // Add new tiles using the safe board
            for (let i = 0; i < boardSize; i++) {
                for (let j = 0; j < boardSize; j++) {
                    const value = safeBoard[i][j];
                    if (value && value !== 0) {
                        addTile(i, j, value);
                    }
                }
            }
        } catch (error) {
            console.error('Error in updateBoard:', error);
            console.error('Game state:', gameState);
        }
    }

    // Add a tile to the board
    function addTile(row, col, value) {
        const tile = document.createElement('div');
        tile.className = `tile tile-${value}`;
        tile.textContent = value;
        tile.dataset.row = row;
        tile.dataset.col = col;
        
        // Position the tile
        const cellSize = gameBoard.offsetWidth / boardSize;
        tile.style.width = `${cellSize - 15}px`;
        tile.style.height = `${cellSize - 15}px`;
        tile.style.left = `${col * cellSize + 7.5}px`;
        tile.style.top = `${row * cellSize + 7.5}px`;
        
        gameBoard.appendChild(tile);
    }

    // Show game over message
    function showGameOver() {
        const gameOverDiv = document.createElement('div');
        gameOverDiv.className = 'game-over';
        
        const message = document.createElement('div');
        message.className = 'game-over-message';
        message.textContent = 'Game Over!';
        
        const button = document.createElement('button');
        button.className = 'game-button';
        button.textContent = 'Try Again';
        button.onclick = resetGame;
        
        gameOverDiv.appendChild(message);
        gameOverDiv.appendChild(button);
        document.querySelector('.game-board-container').appendChild(gameOverDiv);
    }

    // Handle keyboard input
    function handleKeyDown(event) {
        if (gameState.gameOver || gameState.isAutoPlaying) return;
        
        let direction;
        
        switch (event.key) {
            case 'ArrowUp':
                direction = 'Up';
                break;
            case 'ArrowRight':
                direction = 'Right';
                break;
            case 'ArrowDown':
                direction = 'Down';
                break;
            case 'ArrowLeft':
                direction = 'Left';
                break;
            default:
                return; // Ignore other keys
        }
        
        makeMove(direction);
    }

    // Make a move in the game
    async function makeMove(direction) {
        try {
            console.log(`Making move: ${direction}`);
            
            // Convert direction string to number
            let directionValue;
            switch (direction) {
                case 'Up':
                    directionValue = 0;
                    break;
                case 'Right':
                    directionValue = 1;
                    break;
                case 'Down':
                    directionValue = 2;
                    break;
                case 'Left':
                    directionValue = 3;
                    break;
                default:
                    console.error('Invalid direction:', direction);
                    return;
            }
            
            const response = await fetch('/api/game/move', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ Direction: directionValue }) // Send direction as a number
            });
            
            // Check for HTTP errors
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Server error (${response.status}): ${errorText}`);
                throw new Error(`Move failed: ${response.status} ${response.statusText}`);
            }
            
            // Parse the response
            const text = await response.text();
            let data;
            
            try {
                data = JSON.parse(text);
            } catch (parseError) {
                console.error('Error parsing move response:', parseError);
                console.log('Raw response:', text);
                throw new Error('Invalid JSON response from server');
            }
            
            // Update the game state
            console.log('Move successful, received data:', data);
            updateGameState(data);
            
        } catch (error) {
            console.error('Error making move:', error);
            // Don't show an alert for every move error as it would be annoying
        }
    }

    // Reset the game
    async function resetGame() {
        try {
            const response = await fetch('/api/game/reset', {
                method: 'POST'
            });
            
            if (!response.ok) throw new Error('Reset failed');
            
            const data = await response.json();
            updateGameState(data);
            
            // Remove game over message if it exists
            const gameOverDiv = document.querySelector('.game-over');
            if (gameOverDiv) {
                gameOverDiv.remove();
            }
        } catch (error) {
            console.error('Error resetting game:', error);
        }
    }

    // Toggle auto-play
    async function toggleAutoPlay() {
        try {
            if (gameState.isAutoPlaying) {
                await stopAutoPlay();
            } else {
                await startAutoPlay();
            }
        } catch (error) {
            console.error('Error toggling auto-play:', error);
            alert('Error toggling auto-play. Please check console for details.');
            
            // On error, stop polling and re-enable buttons
            if (pollInterval) {
                clearInterval(pollInterval);
                pollInterval = null;
            }
            
            autoPlayButton.disabled = false;
            stopAutoPlayButton.disabled = true;
        }
    }

    // Start auto-play
    async function startAutoPlay() {
        try {
            console.log('Starting auto-play...');
            
            // Disable auto-play button and enable stop button
            autoPlayButton.disabled = true;
            stopAutoPlayButton.disabled = false;
            
            // Update UI state immediately
            gameState.isAutoPlaying = true;
            
            // Start auto-play on the server
            const response = await fetch('/api/game/autoplay/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server error starting auto-play:', errorText);
                throw new Error('Failed to start auto-play');
            }
            
            const data = await response.json();
            console.log('Auto-play started successfully, received data:', data);
            
            // Update the game state
            updateGameState(data);
            
            // Start polling for game state updates
            console.log('Starting polling for game state updates...');
            pollGameState();
            
        } catch (error) {
            console.error('Error starting auto-play:', error);
            
            // Reset the game state
            gameState.isAutoPlaying = false;
            
            // Re-enable buttons
            autoPlayButton.disabled = false;
            stopAutoPlayButton.disabled = true;
            
            // Notify the user
            alert('Failed to start auto-play: ' + error.message);
        }
    }

    // Stop auto-play
    async function stopAutoPlay() {
        // Clear any existing polling interval
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
        }
        
        try {
            // Disable stop button to prevent multiple clicks
            stopAutoPlayButton.disabled = true;
            
            // Send request to stop auto-play
            const response = await fetch('/api/game/autoplay/stop', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (!response.ok) {
                throw new Error('Failed to stop auto-play');
            }
            
            // Update game state with the latest from server
            const data = await response.json();
            updateGameState(data);
            
            // Re-enable auto-play button
            autoPlayButton.disabled = false;
            
        } catch (error) {
            console.error('Error stopping auto-play:', error);
            alert('Failed to stop auto-play. Please try again.');
            
            // Re-enable stop button on error
            stopAutoPlayButton.disabled = false;
        }
    }
    
    // Poll for game state updates (for auto-play)
    function pollGameState() {
        // Clear any existing interval
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
        }
        
        // Set up new polling interval (every 500ms)
        pollInterval = setInterval(async () => {
            try {
                // Fetch the game state from the server
                const response = await fetch('/api/game');
                if (!response.ok) {
                    throw new Error(`Failed to fetch game state: ${response.status} ${response.statusText}`);
                }
                
                // Parse the response
                const text = await response.text();
                let data;
                
                try {
                    data = JSON.parse(text);
                } catch (parseError) {
                    console.error('Error parsing game state JSON:', parseError);
                    console.log('Raw response:', text);
                    throw new Error('Invalid JSON response from server');
                }
                
                // Validate the data
                if (!data) {
                    throw new Error('Empty response from server');
                }
                
                // Handle both lowercase and uppercase property names
                const boardData = data.board || data.Board;
                
                // Ensure the board is properly structured
                if (!boardData || !Array.isArray(boardData)) {
                    console.error('Invalid board data received:', data);
                    // Create a new board with the correct structure
                    const newBoard = Array(boardSize).fill().map(() => Array(boardSize).fill(0));
                    // Set both lowercase and uppercase properties to ensure compatibility
                    data.board = newBoard;
                    data.Board = newBoard;
                } else {
                    // Normalize the board data
                    const normalizedBoard = [];
                    
                    // Ensure all rows exist and have the correct length
                    for (let i = 0; i < boardSize; i++) {
                        if (!Array.isArray(boardData[i])) {
                            normalizedBoard[i] = Array(boardSize).fill(0);
                        } else if (boardData[i].length !== boardSize) {
                            // Fix row length
                            const row = boardData[i].slice(0, boardSize);
                            while (row.length < boardSize) row.push(0);
                            normalizedBoard[i] = row;
                        } else {
                            normalizedBoard[i] = boardData[i];
                        }
                    }
                    
                    // Set both lowercase and uppercase properties to ensure compatibility
                    data.board = normalizedBoard;
                    data.Board = normalizedBoard;
                }
                
                // Normalize other properties
                data.score = data.score || data.Score || 0;
                data.gameOver = data.gameOver || data.GameOver || false;
                data.isAutoPlaying = data.isAutoPlaying || data.IsAutoPlaying || false;
                
                // Update the game state with the validated data
                updateGameState(data);
                
                // If game is over, stop auto-play
                if (data.gameOver) {
                    console.log('Game over detected, stopping auto-play');
                    await stopAutoPlay();
                }
            } catch (error) {
                console.error('Error polling game state:', error);
                
                // On error, stop polling and re-enable buttons
                if (pollInterval) {
                    clearInterval(pollInterval);
                    pollInterval = null;
                }
                
                // Reset the UI
                gameState.isAutoPlaying = false;
                autoPlayButton.disabled = false;
                stopAutoPlayButton.disabled = true;
                
                // Notify the user
                alert('Error checking game state. Auto-play has been stopped.');
            }
        }, 500);
    }
    
    // Set up event listeners
    function setupEventListeners() {
        // Keyboard controls
        document.addEventListener('keydown', handleKeyDown);
        
        // Touch controls
        let touchStartX = 0;
        let touchStartY = 0;
        
        document.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
        }, { passive: true });
        
        document.addEventListener('touchend', (e) => {
            if (gameState.gameOver || gameState.isAutoPlaying) return;
            
            const touchEndX = e.changedTouches[0].clientX;
            const touchEndY = e.changedTouches[0].clientY;
            
            const dx = touchEndX - touchStartX;
            const dy = touchEndY - touchStartY;
            
            // Minimum swipe distance (in pixels)
            const minSwipeDistance = 30;
            
            if (Math.abs(dx) > Math.abs(dy)) {
                // Horizontal swipe
                if (Math.abs(dx) > minSwipeDistance) {
                    if (dx > 0) {
                        makeMove('Right');
                    } else {
                        makeMove('Left');
                    }
                }
            } else {
                // Vertical swipe
                if (Math.abs(dy) > minSwipeDistance) {
                    if (dy > 0) {
                        makeMove('Down');
                    } else {
                        makeMove('Up');
                    }
                }
            }
        }, { passive: true });
        
        // Button events
        newGameButton.addEventListener('click', resetGame);
        autoPlayButton.addEventListener('click', toggleAutoPlay);
        stopAutoPlayButton.addEventListener('click', toggleAutoPlay);
        
        // Color scheme selector
        colorSchemeSelect.addEventListener('change', (e) => {
            const selectedScheme = e.target.value;
            applyColorScheme(selectedScheme);
        });
    }

    // Initialize the game
    initGame();
});

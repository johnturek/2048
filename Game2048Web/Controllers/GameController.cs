using Game2048Web.Models;
using Game2048Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Game2048Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly UserStatsService _userStatsService;
        private readonly UserManager<IdentityUser> _userManager;

        public GameController(GameService gameService, UserStatsService userStatsService, UserManager<IdentityUser> userManager)
        {
            _gameService = gameService;
            _userStatsService = userStatsService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var game = _gameService.GetGame();
            return Ok(game);
        }

        [HttpPost("move")]
        public IActionResult Move([FromBody] MoveRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is empty");
                }
                
                // Make the move
                _gameService.MakeMove(request.Direction);
                
                // Get the updated game state
                var game = _gameService.GetGame();
                return Ok(game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error making move: {ex.Message}");
            }
        }

        [HttpPost("reset")]
        public IActionResult Reset()
        {
            _gameService.ResetGame();
            var game = _gameService.GetGame();
            return Ok(game);
        }
        
        [HttpPost("save-stats")]
        public async Task<IActionResult> SaveStats([FromBody] SaveStatsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is empty");
                }
                
                // Only save stats if user is authenticated
                if (User.Identity.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(User);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await _userManager.GetUserAsync(User);
                        string defaultNickname = user?.UserName ?? "Player";
                        await _userStatsService.UpdateGameStatsAsync(userId, request.Score, request.HighestTile, defaultNickname);
                    }
                }
                
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving stats: {ex.Message}");
            }
        }

        [HttpPost("autoplay/start")]
        public async Task<IActionResult> StartAutoPlay()
        {
            try
            {
                await _gameService.StartAutoPlay();
                var game = _gameService.GetGame();
                return Ok(game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error starting auto-play: {ex.Message}");
            }
        }

        [HttpPost("autoplay/stop")]
        public async Task<IActionResult> StopAutoPlay()
        {
            try
            {
                await _gameService.StopAutoPlay();
                var game = _gameService.GetGame();
                return Ok(game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error stopping auto-play: {ex.Message}");
            }
        }
    }

    public class MoveRequest
    {
        public Direction Direction { get; set; }
    }
    
    public class SaveStatsRequest
    {
        public int Score { get; set; }
        public int HighestTile { get; set; }
    }
    
    // Add a model binder to handle direction values
    public class DirectionJsonConverter : System.Text.Json.Serialization.JsonConverter<Direction>
    {
        public override Direction Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.String)
            {
                // Handle string values
                string directionString = reader.GetString();
                if (Enum.TryParse<Direction>(directionString, true, out Direction direction))
                {
                    return direction;
                }
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.Number)
            {
                // Handle numeric values
                int directionValue = reader.GetInt32();
                if (Enum.IsDefined(typeof(Direction), directionValue))
                {
                    return (Direction)directionValue;
                }
            }
            
            // Default to Up if invalid
            return Direction.Up;
        }
        
        public override void Write(System.Text.Json.Utf8JsonWriter writer, Direction value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}

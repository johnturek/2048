using System.Collections.Generic;
using System.Threading.Tasks;
using Game2048Web.Models;
using Game2048Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Game2048Web.Pages
{
    public class LeaderboardModel : PageModel
    {
        private readonly UserStatsService _statsService;

        public LeaderboardModel(UserStatsService statsService)
        {
            _statsService = statsService;
        }

        public List<UserGameStats> TopPlayers { get; set; } = new List<UserGameStats>();

        public async Task<IActionResult> OnGetAsync()
        {
            TopPlayers = await _statsService.GetLeaderboardAsync(20); // Get top 20 players
            return Page();
        }
    }
}

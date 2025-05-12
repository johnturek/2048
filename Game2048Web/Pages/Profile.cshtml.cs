using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Game2048Web.Models;
using Game2048Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Game2048Web.Pages
{
    [Authorize] // Require authentication to access this page
    public class ProfileModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserStatsService _statsService;

        public ProfileModel(UserManager<IdentityUser> userManager, UserStatsService statsService)
        {
            _userManager = userManager;
            _statsService = statsService;
        }

        public UserGameStats UserStats { get; set; } = new UserGameStats();
        
        [BindProperty]
        public string Nickname { get; set; } = string.Empty;
        
        [BindProperty]
        public bool ShowInLeaderboard { get; set; } = true;
        
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        
        public int UserRank { get; set; } = 0;
        public int TotalPlayers { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            UserStats = await _statsService.GetUserStatsAsync(user.Id, user.UserName);
            Nickname = UserStats.Nickname;
            ShowInLeaderboard = UserStats.ShowInLeaderboard;
            
            // Get user's rank on the leaderboard
            var rankInfo = await _statsService.GetUserRankAsync(user.Id);
            UserRank = rankInfo.Rank;
            TotalPlayers = rankInfo.Total;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            
            // Validate nickname
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                Nickname = user.UserName ?? "Player";
            }
            else if (Nickname.Length > 20)
            {
                Nickname = Nickname.Substring(0, 20);
            }
            
            // Update profile
            await _statsService.UpdateProfileAsync(user.Id, Nickname, ShowInLeaderboard);
            
            // Refresh user stats
            UserStats = await _statsService.GetUserStatsAsync(user.Id);
            
            StatusMessage = "Your profile has been updated";
            return Page();
        }
    }
}

using System;

namespace Game2048Web.Models
{
    public class UserGameStats
    {
        public string UserId { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public int HighScore { get; set; }
        public int GamesPlayed { get; set; }
        public int HighestTile { get; set; }
        public DateTime LastPlayed { get; set; }
        public bool ShowInLeaderboard { get; set; } = true;
        
        // Navigation property to user (if needed)
        // public IdentityUser User { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game2048Web.Data;
using Game2048Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Game2048Web.Services
{
    public class UserStatsService
    {
        private readonly ApplicationDbContext _context;

        public UserStatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserGameStats> GetUserStatsAsync(string userId, string defaultNickname = "")
        {
            var stats = await _context.UserGameStats.FindAsync(userId);
            
            if (stats == null)
            {
                // Create new stats for the user if they don't exist
                stats = new UserGameStats
                {
                    UserId = userId,
                    Nickname = defaultNickname,
                    HighScore = 0,
                    GamesPlayed = 0,
                    HighestTile = 0,
                    LastPlayed = DateTime.UtcNow,
                    ShowInLeaderboard = true
                };
                
                _context.UserGameStats.Add(stats);
                await _context.SaveChangesAsync();
            }
            
            return stats;
        }

        public async Task UpdateGameStatsAsync(string userId, int score, int highestTile, string defaultNickname = "")
        {
            var stats = await _context.UserGameStats.FindAsync(userId);
            
            if (stats == null)
            {
                stats = new UserGameStats
                {
                    UserId = userId,
                    Nickname = defaultNickname,
                    HighScore = score,
                    GamesPlayed = 1,
                    HighestTile = highestTile,
                    LastPlayed = DateTime.UtcNow,
                    ShowInLeaderboard = true
                };
                
                _context.UserGameStats.Add(stats);
            }
            else
            {
                stats.GamesPlayed++;
                stats.LastPlayed = DateTime.UtcNow;
                
                if (score > stats.HighScore)
                {
                    stats.HighScore = score;
                }
                
                if (highestTile > stats.HighestTile)
                {
                    stats.HighestTile = highestTile;
                }
            }
            
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<UserGameStats>> GetLeaderboardAsync(int count = 10)
        {
            return await _context.UserGameStats
                .Where(s => s.ShowInLeaderboard)
                .OrderByDescending(s => s.HighScore)
                .Take(count)
                .ToListAsync();
        }
        
        public async Task<(int Rank, int Total)> GetUserRankAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (0, 0);
                
            var userStats = await _context.UserGameStats.FindAsync(userId);
            if (userStats == null)
                return (0, 0);
                
            // Get total number of players with scores
            int totalPlayers = await _context.UserGameStats
                .Where(s => s.HighScore > 0)
                .CountAsync();
                
            // If user has no score yet, return total players but no rank
            if (userStats.HighScore <= 0)
                return (0, totalPlayers);
                
            // Count how many players have higher scores
            int playersWithHigherScore = await _context.UserGameStats
                .Where(s => s.HighScore > userStats.HighScore)
                .CountAsync();
                
            // Rank is players with higher score + 1
            int rank = playersWithHigherScore + 1;
            
            return (rank, totalPlayers);
        }
        
        public async Task UpdateProfileAsync(string userId, string nickname, bool showInLeaderboard)
        {
            var stats = await _context.UserGameStats.FindAsync(userId);
            
            if (stats != null)
            {
                stats.Nickname = nickname;
                stats.ShowInLeaderboard = showInLeaderboard;
                await _context.SaveChangesAsync();
            }
        }
    }
}

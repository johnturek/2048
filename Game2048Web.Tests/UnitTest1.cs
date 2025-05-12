using System;
using System.Threading.Tasks;
using Game2048Web.Data;
using Game2048Web.Models;
using Game2048Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Game2048Web.Tests;

public class UserStatsServiceTests
{
    private ApplicationDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        var context = new ApplicationDbContext(options);
        return context;
    }
    
    [Fact]
    public async Task GetUserStats_CreatesNewStatsIfNotExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var service = new UserStatsService(context);
        string userId = "test-user-id";
        string nickname = "TestUser";
        
        // Act
        var stats = await service.GetUserStatsAsync(userId, nickname);
        
        // Assert
        Assert.NotNull(stats);
        Assert.Equal(userId, stats.UserId);
        Assert.Equal(nickname, stats.Nickname);
        Assert.Equal(0, stats.HighScore);
        Assert.Equal(0, stats.GamesPlayed);
        Assert.True(stats.ShowInLeaderboard);
    }
    
    [Fact]
    public async Task SavesHigherScore_WhenUpdatingGameStats()
    {
        // Arrange
        var context = CreateTestDbContext();
        var service = new UserStatsService(context);
        string userId = "test-user-id";
        
        // Create initial stats
        var initialStats = new UserGameStats
        {
            UserId = userId,
            HighScore = 1000,
            GamesPlayed = 1,
            HighestTile = 512,
            LastPlayed = DateTime.UtcNow,
            ShowInLeaderboard = true
        };
        
        await context.UserGameStats.AddAsync(initialStats);
        await context.SaveChangesAsync();
        
        // Act - Update with higher score
        await service.UpdateGameStatsAsync(userId, 2000, 1024);
        
        // Assert
        var updatedStats = await context.UserGameStats.FindAsync(userId);
        Assert.NotNull(updatedStats);
        Assert.Equal(2000, updatedStats.HighScore);
        Assert.Equal(2, updatedStats.GamesPlayed);
        Assert.Equal(1024, updatedStats.HighestTile);
    }
    
    [Fact]
    public async Task UpdateProfile_ChangesNicknameAndVisibility()
    {
        // Arrange
        var context = CreateTestDbContext();
        var service = new UserStatsService(context);
        string userId = "test-user-id";
        string initialNickname = "InitialNick";
        string updatedNickname = "UpdatedNick";
        
        // Create initial stats
        await service.GetUserStatsAsync(userId, initialNickname);
        
        // Act
        await service.UpdateProfileAsync(userId, updatedNickname, false);
        var updatedStats = await service.GetUserStatsAsync(userId);
        
        // Assert
        Assert.Equal(updatedNickname, updatedStats.Nickname);
        Assert.False(updatedStats.ShowInLeaderboard);
    }
}

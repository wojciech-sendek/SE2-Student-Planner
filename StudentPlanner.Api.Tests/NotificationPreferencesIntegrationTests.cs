using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;
using Xunit;

namespace StudentPlanner.Api.Tests
{
    public class NotificationPreferencesIntegrationTests : IClassFixture<StudentPlannerApiFactory>
    {
        private readonly HttpClient _client;
        private readonly StudentPlannerApiFactory _factory;

        public NotificationPreferencesIntegrationTests(StudentPlannerApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsNotificationsEnabled()
        {
            // Arrange
            var userId = "pref-user-1";
            var email = "pref1@pw.edu.pl";
            await SeedUserAsync(userId, email);
            Authenticate(userId);
            
            // Act
            var response = await _client.GetAsync("/api/Auth/me");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
            Assert.NotNull(user);
            // Default value should be true as set in ApplicationUser.cs
            Assert.True(user.NotificationsEnabled);
        }

        [Fact]
        public async Task UpdateNotifications_UpdatesPreference()
        {
            // Arrange
            var userId = "pref-user-2";
            var email = "pref2@pw.edu.pl";
            await SeedUserAsync(userId, email);
            Authenticate(userId);
            
            var updateDto = new UpdateNotificationsDto { Enabled = false };
            
            // Act
            var patchResponse = await _client.PatchAsJsonAsync("/api/Auth/notifications", updateDto);
            
            // Assert
            patchResponse.EnsureSuccessStatusCode();
            
            // Verify by getting "me" again
            var meResponse = await _client.GetAsync("/api/Auth/me");
            meResponse.EnsureSuccessStatusCode();
            var user = await meResponse.Content.ReadFromJsonAsync<CurrentUserDto>();
            Assert.NotNull(user);
            Assert.False(user.NotificationsEnabled);
        }

        private async Task SeedUserAsync(string userId, string email)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (await db.Users.AnyAsync(u => u.Id == userId)) return;

            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = email,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                NotificationsEnabled = true
            });
            await db.SaveChangesAsync();
        }

        private void Authenticate(string userId)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        }
    }
}

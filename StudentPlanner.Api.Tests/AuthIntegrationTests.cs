using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Tests;

public class AuthIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenValid()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "newuser@pw.edu.pl",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Registration successful.");
    }

    [Fact]
    public async Task Me_ReturnsUserInfo_WhenAuthenticated()
    {
        // Arrange
        var userId = "me-user-id";
        var email = "me@pw.edu.pl";
        await SeedUserAsync(userId, email);
        Authenticate(userId);

        // Act
        var response = await _client.GetAsync("/api/Auth/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNoContent_WhenAuthenticated()
    {
        // Arrange
        var userId = "delete-user-id";
        await SeedUserAsync(userId, "delete@pw.edu.pl");
        Authenticate(userId);

        // Act
        var response = await _client.DeleteAsync("/api/Auth/delete-account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);
            user.Should().BeNull();
        }
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
            LastName = "User"
        });
        await db.SaveChangesAsync();
    }

    private void Authenticate(string userId)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
    }
}

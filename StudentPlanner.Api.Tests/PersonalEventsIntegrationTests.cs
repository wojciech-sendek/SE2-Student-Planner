using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.PersonalEvents;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Tests;

public class PersonalEventsIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly HttpClient _client;

    public PersonalEventsIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePersonalEvent_ReturnsCreated_WhenValid()
    {
        // Arrange
        var userId = "test-user-id";
        await SeedUserAsync(userId);
        Authenticate(userId);

        var request = new CreatePersonalEventRequestDto
        {
            Title = "Test Event",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Location = "Test Location"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/PersonalEvents", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdEvent = await response.Content.ReadFromJsonAsync<PersonalEventDto>();
        createdEvent.Should().NotBeNull();
        createdEvent!.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task GetMine_ReturnsUserEventsOnly()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        await SeedUserAsync(userId1);
        await SeedUserAsync(userId2);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PersonalEvents.Add(new PersonalEvent { Title = "User 1 Event", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), UserId = userId1 });
            db.PersonalEvents.Add(new PersonalEvent { Title = "User 2 Event", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), UserId = userId2 });
            await db.SaveChangesAsync();
        }

        Authenticate(userId1);

        // Act
        var response = await _client.GetAsync("/api/PersonalEvents");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<IEnumerable<PersonalEventDto>>();
        events.Should().HaveCount(1);
        events!.First().Title.Should().Be("User 1 Event");
    }

    [Fact]
    public async Task UpdatePersonalEvent_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var userId = "test-user-id";
        await SeedUserAsync(userId);
        Authenticate(userId);

        var existingEvent = new PersonalEvent
        {
            Title = "Original Title",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            UserId = userId
        };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PersonalEvents.Add(existingEvent);
            await db.SaveChangesAsync();
        }

        var updateRequest = new UpdatePersonalEventRequestDto
        {
            Title = "Updated Title",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
            Location = "Updated Location"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/PersonalEvents/{existingEvent.Id}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedEvent = await db.PersonalEvents.FindAsync(existingEvent.Id);
            updatedEvent.Should().NotBeNull();
            updatedEvent!.Title.Should().Be(updateRequest.Title);
        }
    }

    [Fact]
    public async Task DeletePersonalEvent_ReturnsNoContent_WhenExists()
    {
        // Arrange
        var userId = "test-user-id";
        await SeedUserAsync(userId);
        Authenticate(userId);

        var existingEvent = new PersonalEvent
        {
            Title = "To Delete",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            UserId = userId
        };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PersonalEvents.Add(existingEvent);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.DeleteAsync($"/api/PersonalEvents/{existingEvent.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedEvent = await db.PersonalEvents.FindAsync(existingEvent.Id);
            deletedEvent.Should().BeNull();
        }
    }

    private async Task SeedUserAsync(string userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await db.Users.AnyAsync(u => u.Id == userId)) return;

        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@pw.edu.pl",
            Email = $"{userId}@pw.edu.pl",
            FirstName = "Test",
            LastName = "User"
        });
        await db.SaveChangesAsync();
    }

    private void Authenticate(string userId)
    {
		// Simple mock auth for testing
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
    }
}

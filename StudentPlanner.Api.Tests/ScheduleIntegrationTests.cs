using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Schedule;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Tests;

public class ScheduleIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly HttpClient _client;
    private readonly StudentPlannerApiFactory _factory;

    public ScheduleIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSchedule_ReturnsMergedEventsInOrder()
    {
        // Arrange
        var userId = "test-user-id";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Clear old entries if necessary
            db.PersonalEvents.RemoveRange(db.PersonalEvents);
            db.UsosEvents.RemoveRange(db.UsosEvents);

            db.PersonalEvents.Add(new PersonalEvent
            {
                UserId = userId,
                Title = "Personal 1",
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
            });

            db.UsosEvents.Add(new UsosEvent
            {
                UserId = userId,
                ExternalId = "usos-1",
                Title = "USOS 1",
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(4),
                Room = "A1",
                Teacher = "John Doe",
                SyncedAtUtc = DateTime.UtcNow
            });

            db.PersonalEvents.Add(new PersonalEvent
            {
                UserId = userId,
                Title = "Personal 2",
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(5),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(6)
            });

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/Schedule");

        // Assert
        response.EnsureSuccessStatusCode();

        var events = await response.Content.ReadFromJsonAsync<IEnumerable<EventDto>>();

        events.Should().NotBeNull();
        var eventList = events!.ToList();
        
        eventList.Should().HaveCount(3);
        
        eventList[0].Title.Should().Be("Personal 1");
        eventList[0].IsPersonal.Should().BeTrue();
        
        eventList[1].Title.Should().Be("USOS 1");
        eventList[1].IsPersonal.Should().BeFalse();
        eventList[1].Room.Should().Be("A1");
        
        eventList[2].Title.Should().Be("Personal 2");
        eventList[2].IsPersonal.Should().BeTrue();
    }

    [Fact]
    public async Task GetSchedule_ReturnsOk_WhenAuthenticatedAsManager()
    {
        // Arrange
        var userId = "manager-schedule-user";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        _client.DefaultRequestHeaders.Remove("X-Test-Roles");
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Manager");

        // Act
        var response = await _client.GetAsync("/api/Schedule");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Tests;

public class EventRequestsIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly HttpClient _client;

    public EventRequestsIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SubmitRequest_Create_SavesPendingRequest_WithoutCreatingAcademicEvent()
    {
        var managerId = "manager-create";
        var facultyId = await SeedManagerAsync(managerId, "frontend-create-faculty");
        AuthenticateManager(managerId);

        var academicEventsBefore = await CountAcademicEventsAsync();

        var payload = new
        {
            requestType = 0,
            facultyId,
            details = new
            {
                title = "Frontend submitted seminar",
                startTime = DateTime.UtcNow.AddDays(3),
                endTime = DateTime.UtcNow.AddDays(3).AddHours(2),
                location = "MATH-301",
                description = "Ignored by the Sprint 4 backend model."
            }
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        created.Should().NotBeNull();
        created!.RequestType.Should().Be("Create");
        created.Status.Should().Be("Pending");
        created.RequestStatus.Should().Be("Pending");
        created.Title.Should().Be("Frontend submitted seminar");
        created.Details.Title.Should().Be("Frontend submitted seminar");
        created.FacultyId.Should().Be(facultyId);
        created.SubmissionDate.Should().Be(created.CreatedAtUtc);

        (await CountAcademicEventsAsync()).Should().Be(academicEventsBefore);
    }

    [Fact]
    public async Task SubmitRequest_Update_SavesPendingRequest_AndDoesNotModifyTargetEvent()
    {
        var managerId = "manager-update";
        var facultyId = await SeedManagerAsync(managerId, "frontend-update-faculty");
        var academicEventId = await SeedAcademicEventAsync(facultyId, "Original title");
        AuthenticateManager(managerId);

        var payload = new
        {
            requestType = 1,
            targetEventId = academicEventId.ToString(),
            details = new
            {
                title = "Requested new title",
                startTime = DateTime.UtcNow.AddDays(5),
                endTime = DateTime.UtcNow.AddDays(5).AddHours(1),
                location = "Updated room"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        created.Should().NotBeNull();
        created!.RequestType.Should().Be("Update");
        created.Status.Should().Be("Pending");
        created.TargetAcademicEventId.Should().Be(academicEventId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var target = await db.AcademicEvents.AsNoTracking().SingleAsync(e => e.Id == academicEventId);
        target.Title.Should().Be("Original title");
    }

    [Fact]
    public async Task SubmitRequest_Delete_SavesPendingRequest_AndDoesNotDeleteTargetEvent()
    {
        var managerId = "manager-delete";
        var facultyId = await SeedManagerAsync(managerId, "frontend-delete-faculty");
        var academicEventId = await SeedAcademicEventAsync(facultyId, "Delete candidate");
        AuthenticateManager(managerId);

        var payload = new
        {
            requestType = 2,
            targetEventId = academicEventId,
            reason = "No longer needed."
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        created.Should().NotBeNull();
        created!.RequestType.Should().Be("Delete");
        created.Status.Should().Be("Pending");
        created.ReviewComment.Should().Be("No longer needed.");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.AcademicEvents.AnyAsync(e => e.Id == academicEventId)).Should().BeTrue();
    }

    [Fact]
    public async Task SubmitRequest_Delete_AllowsVisibleUniversityEvent()
    {
        var managerId = "manager-delete-university";
        await SeedManagerAsync(managerId, "frontend-delete-university-faculty");
        var universityEventId = await SeedUniversityAcademicEventAsync();
        AuthenticateManager(managerId);

        var payload = new
        {
            requestType = 2,
            targetEventId = universityEventId,
            reason = "Shared event should be removable by request."
        };

        var response = await _client.PostAsJsonAsync("/api/event-requests", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        created.Should().NotBeNull();
        created!.RequestType.Should().Be("Delete");
        created.Status.Should().Be("Pending");
        created.TargetAcademicEventId.Should().Be(universityEventId);
    }

    private async Task<int> SeedManagerAsync(string managerId, string facultyName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var faculty = new Faculty
        {
            Name = facultyName,
            DisplayName = facultyName
        };

        var manager = new ApplicationUser
        {
            Id = managerId,
            UserName = $"{managerId}@pw.edu.pl",
            Email = $"{managerId}@pw.edu.pl",
            EmailConfirmed = true,
            Faculties = new List<Faculty> { faculty }
        };

        db.Users.Add(manager);
        await db.SaveChangesAsync();
        return faculty.Id;
    }

    private async Task<int> SeedAcademicEventAsync(int facultyId, string title)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var academicEvent = new AcademicEvent
        {
            Title = title,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            Location = "A-101",
            FacultyId = facultyId
        };

        db.AcademicEvents.Add(academicEvent);
        await db.SaveChangesAsync();
        return academicEvent.Id;
    }

    private async Task<int> SeedUniversityAcademicEventAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var universityFaculty = await db.Faculties.SingleAsync(f => f.Name == "university");

        var academicEvent = new AcademicEvent
        {
            Title = "Shared university event",
            StartTime = DateTime.UtcNow.AddDays(6),
            EndTime = DateTime.UtcNow.AddDays(6).AddHours(1),
            Location = "Main Hall",
            FacultyId = universityFaculty.Id
        };

        db.AcademicEvents.Add(academicEvent);
        await db.SaveChangesAsync();
        return academicEvent.Id;
    }

    private async Task<int> CountAcademicEventsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.AcademicEvents.CountAsync();
    }

    private void AuthenticateManager(string userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        _client.DefaultRequestHeaders.Remove("X-Test-Roles");
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Manager");
    }
}

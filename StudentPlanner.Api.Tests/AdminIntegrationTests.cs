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

public class AdminIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly HttpClient _client;

    public AdminIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_CanListPendingRequests()
    {
        // Arrange
        var adminId = "admin-list";
        await SeedAdminAsync(adminId);
        AuthenticateAdmin(adminId);

        var managerId = "manager-list";
        var facultyId = await SeedManagerAsync(managerId, "Faculty for List");
        await CreatePendingRequest(managerId, facultyId, "Request to list");

        // Act
        var response = await _client.GetAsync("/api/admin/event-requests?status=Pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var requests = await response.Content.ReadFromJsonAsync<IEnumerable<EventRequestDto>>();
        requests.Should().NotBeNull();
        requests!.Any(r => r.Title == "Request to list").Should().BeTrue();
    }

    [Fact]
    public async Task Admin_CanApproveRequest_AndAcademicEventIsCreated()
    {
        // Arrange
        var adminId = "admin-approve";
        await SeedAdminAsync(adminId);
        AuthenticateAdmin(adminId);

        var managerId = "manager-approve";
        var facultyId = await SeedManagerAsync(managerId, "Faculty for Approval");
        var requestId = await CreatePendingRequest(managerId, facultyId, "Request to approve");

        var payload = new { ReviewComment = "Approved by test" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/event-requests/{requestId}/approve", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewed = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        reviewed!.Status.Should().Be("Approved");
        reviewed.ReviewComment.Should().Be("Approved by test");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var academicEvent = await db.AcademicEvents.FirstOrDefaultAsync(e => e.Title == "Request to approve");
        academicEvent.Should().NotBeNull();
        academicEvent!.FacultyId.Should().Be(facultyId);
    }

    [Fact]
    public async Task Admin_CanRejectRequest()
    {
        // Arrange
        var adminId = "admin-reject";
        await SeedAdminAsync(adminId);
        AuthenticateAdmin(adminId);

        var managerId = "manager-reject";
        var facultyId = await SeedManagerAsync(managerId, "Faculty for Rejection");
        var requestId = await CreatePendingRequest(managerId, facultyId, "Request to reject");

        var payload = new { ReviewComment = "Rejected by test" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/admin/event-requests/{requestId}/reject", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewed = await response.Content.ReadFromJsonAsync<EventRequestDto>();
        reviewed!.Status.Should().Be("Rejected");
        reviewed.ReviewComment.Should().Be("Rejected by test");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var academicEvent = await db.AcademicEvents.FirstOrDefaultAsync(e => e.Title == "Request to reject");
        academicEvent.Should().BeNull();
    }

    private async Task SeedAdminAsync(string adminId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await db.Users.AnyAsync(u => u.Id == adminId)) return;

        db.Users.Add(new ApplicationUser
        {
            Id = adminId,
            UserName = $"{adminId}@pw.edu.pl",
            Email = $"{adminId}@pw.edu.pl",
            EmailConfirmed = true
        });
        await db.SaveChangesAsync();
    }

    private async Task<int> SeedManagerAsync(string managerId, string facultyName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var faculty = new Faculty { Name = facultyName, DisplayName = facultyName };
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

    private async Task<int> CreatePendingRequest(string managerId, int facultyId, string title)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var request = new EventRequest
        {
            ManagerId = managerId,
            FacultyId = facultyId,
            RequestType = EventRequestType.Create,
            Status = EventRequestStatus.Pending,
            Title = title,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Location = "Room 1",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.EventRequests.Add(request);
        await db.SaveChangesAsync();
        return request.Id;
    }

    private void AuthenticateAdmin(string userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        _client.DefaultRequestHeaders.Remove("X-Test-Roles");
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Admin");
    }
}

using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Faculty;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Tests;

public class FacultiesIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly HttpClient _client;

    public FacultiesIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsFacultiesFromDatabase()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Faculties.AddRange(new List<Faculty>
            {
                new Faculty { Name = "EITI", DisplayName = "Wydział Elektroniki i Technik Informacyjnych" },
                new Faculty { Name = "MINI", DisplayName = "Wydział Matematyki i Nauk Informacyjnych" }
            });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/Faculties");

        // Assert
        response.EnsureSuccessStatusCode();
        var faculties = await response.Content.ReadFromJsonAsync<IEnumerable<FacultyDto>>();
        faculties.Should().NotBeNull();
        var facultyList = faculties!.ToList();
        facultyList.Count.Should().BeGreaterThanOrEqualTo(2);
        facultyList.Should().Contain(f => f.Name == "EITI");
        facultyList.Should().Contain(f => f.Name == "MINI");
        
        // Check ordering by DisplayName
        var sortedList = facultyList.OrderBy(f => f.DisplayName).ToList();
        facultyList.Should().ContainInOrder(sortedList);
    }
}

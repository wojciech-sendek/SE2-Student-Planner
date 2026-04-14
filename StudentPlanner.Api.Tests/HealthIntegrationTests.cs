using System.Net.Http.Json;
using FluentAssertions;

namespace StudentPlanner.Api.Tests;

public class HealthIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly HttpClient _client;

    public HealthIntegrationTests(StudentPlannerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WithStatusAndTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/api/Health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<HealthResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("OK");
        content.TimestampUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    private class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
    }
}

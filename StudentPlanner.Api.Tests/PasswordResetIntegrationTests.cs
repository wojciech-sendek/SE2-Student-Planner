using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Tests;

public class PasswordResetIntegrationTests : IClassFixture<StudentPlannerApiFactory>
{
    private readonly StudentPlannerApiFactory _factory;
    private readonly Mock<IEmailService> _emailServiceMock = new();

    public PasswordResetIntegrationTests(StudentPlannerApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ForgotPassword_And_ResetPassword_Flow_Succeeds()
    {
        // Arrange
        var email = "reset@pw.edu.pl";
        var password = "OldPassword123!";
        var newPassword = "NewPassword123!";
        string? capturedToken = null;

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((to, subject, body) =>
            {
                // Simple regex or string search to find the token in the body
                // Body: Your password reset token is: <b>{token}</b><br/>...
                var start = body.IndexOf("<b>") + 3;
                var end = body.IndexOf("</b>");
                capturedToken = body.Substring(start, end - start);
            })
            .Returns(Task.CompletedTask);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped(_ => _emailServiceMock.Object));
            });
        }).CreateClient();

        // Seed user
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email };
            await userManager.CreateAsync(user, password);
        }

        // Act - Forgot Password
        var forgotResponse = await client.PostAsJsonAsync("/api/Auth/forgot-password", new ForgotPasswordRequestDto { Email = email });
        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        capturedToken.Should().NotBeNull();

        // Act - Reset Password
        var resetRequest = new ResetPasswordRequestDto
        {
            Email = email,
            Token = capturedToken!,
            NewPassword = newPassword
        };
        var resetResponse = await client.PostAsJsonAsync("/api/Auth/reset-password", resetRequest);
        
        // Assert
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify login with new password
        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto { Email = email, Password = newPassword });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var email = "invalid-token@pw.edu.pl";
        var client = _factory.CreateClient();

        // Seed user
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email };
            await userManager.CreateAsync(user, "Password123!");
        }

        var resetRequest = new ResetPasswordRequestDto
        {
            Email = email,
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        // Act
        var resetResponse = await client.PostAsJsonAsync("/api/Auth/reset-password", resetRequest);

        // Assert
        resetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

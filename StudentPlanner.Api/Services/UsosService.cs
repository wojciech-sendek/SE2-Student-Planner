using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentPlanner.Api.Configurations;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class UsosService : IUsosService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;
        private readonly UsosOptions _options;
        private readonly IDataProtector _protector;
        private readonly ILogger<UsosService> _logger;

        public UsosService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            HttpClient httpClient,
            IOptions<UsosOptions> options,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UsosService> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpClient = httpClient;
            _options = options.Value;
            _protector = dataProtectionProvider.CreateProtector("StudentPlanner.Usos.RefreshTokens.v1");
            _logger = logger;
        }

        public UsosAuthorizationUrlResponseDto CreateAuthorizationUrl(string userId)
        {
            if (!IsOAuthConfigured())
            {
                return new UsosAuthorizationUrlResponseDto
                {
                    AuthorizationUrl = null,
                    State = null,
                    Message = "USOS OAuth is not configured. Fill the Usos section in appsettings before using real USOS."
                };
            }

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(15).Ticks;
            var rawState = $"{userId}|{expiresAtUtc}|{Guid.NewGuid():N}";
            var protectedState = _protector.Protect(rawState);

            var query = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = _options.ClientId,
                ["redirect_uri"] = _options.RedirectUri,
                ["state"] = protectedState
            };

            if (!string.IsNullOrWhiteSpace(_options.Scope))
            {
                query["scope"] = _options.Scope;
            }

            var authorizationUrl = QueryHelpers.AddQueryString(_options.AuthorizationEndpoint, query);

            return new UsosAuthorizationUrlResponseDto
            {
                AuthorizationUrl = authorizationUrl,
                State = protectedState,
                Message = "Redirect the user to this URL to authorize USOS access."
            };
        }

        public async Task CompleteAuthorizationAsync(string code, string state)
        {
            if (!IsOAuthConfigured())
            {
                throw new UsosApiException("USOS OAuth is not configured.");
            }

            var userId = ReadUserIdFromState(state);
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new UsosApiException("User from USOS OAuth state was not found.");
            }

            var tokenSet = await ExchangeAuthorizationCodeAsync(code);

            if (string.IsNullOrWhiteSpace(tokenSet.RefreshToken))
            {
                throw new UsosApiException("USOS did not return a refresh token.");
            }

            user.UsosRefreshTokenProtected = _protector.Protect(tokenSet.RefreshToken);
            user.UsosConnectedAtUtc = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            await SyncScheduleForUserAsync(user);
        }

        public async Task<IReadOnlyList<UsosEventDto>> GetScheduleAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbContext.UsosEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(e => e.EndTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.StartTime <= to.Value);
            }

            return await query
                .OrderBy(e => e.StartTime)
                .Select(e => new UsosEventDto
                {
                    Id = e.Id,
                    ExternalId = e.ExternalId,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    Room = e.Room,
                    Teacher = e.Teacher,
                    Source = "usos",
                    IsReadOnly = true,
                    SyncedAtUtc = e.SyncedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UsosEventDto>> SyncScheduleForUserAsync(ApplicationUser user)
        {
            IReadOnlyList<UsosEventDto> remoteEvents;

            if (!IsRemoteScheduleConfigured() || string.IsNullOrWhiteSpace(user.UsosRefreshTokenProtected))
            {
                if (_options.UseMockScheduleWhenNotConfigured)
                {
                    remoteEvents = BuildMockSchedule();
                }
                else
                {
                    throw new UsosAuthorizationRequiredException("USOS authorization required.");
                }
            }
            else
            {
                try
                {
                    var refreshToken = _protector.Unprotect(user.UsosRefreshTokenProtected);
                    var accessToken = await RefreshAccessTokenAsync(user, refreshToken);
                    remoteEvents = await FetchRemoteScheduleAsync(accessToken);
                }
                catch (UsosAuthorizationRequiredException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is not UsosApiException)
                {
                    _logger.LogError(ex, "Unexpected USOS schedule synchronization error for user {UserId}", user.Id);
                    throw new UsosApiException("USOS API failure.", ex);
                }
            }

            await ReplaceStoredScheduleAsync(user, remoteEvents);
            return await GetScheduleAsync(user.Id);
        }

        private async Task ReplaceStoredScheduleAsync(ApplicationUser user, IReadOnlyList<UsosEventDto> events)
        {
            var existingEvents = await _dbContext.UsosEvents
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            _dbContext.UsosEvents.RemoveRange(existingEvents);

            var syncedAtUtc = DateTime.UtcNow;

            var entities = events.Select(e => new UsosEvent
            {
                ExternalId = e.ExternalId,
                Title = e.Title.Trim(),
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = string.IsNullOrWhiteSpace(e.Location) ? null : e.Location.Trim(),
                Room = string.IsNullOrWhiteSpace(e.Room) ? null : e.Room.Trim(),
                Teacher = string.IsNullOrWhiteSpace(e.Teacher) ? null : e.Teacher.Trim(),
                SyncedAtUtc = syncedAtUtc,
                UserId = user.Id
            }).ToList();

            _dbContext.UsosEvents.AddRange(entities);

            user.UsosScheduleSyncedAtUtc = syncedAtUtc;

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<OAuthTokenSet> ExchangeAuthorizationCodeAsync(string code)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _options.RedirectUri,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            using var response = await _httpClient.PostAsync(_options.TokenEndpoint, new FormUrlEncodedContent(form));
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("USOS token exchange failed. Status: {Status}. Body: {Body}", response.StatusCode, json);
                throw new UsosApiException("USOS API failure.");
            }

            return ParseTokenSet(json);
        }

        private async Task<string> RefreshAccessTokenAsync(ApplicationUser user, string refreshToken)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            using var response = await _httpClient.PostAsync(_options.TokenEndpoint, new FormUrlEncodedContent(form));
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("USOS refresh-token request failed. Status: {Status}. Body: {Body}", response.StatusCode, json);
                throw new UsosApiException("USOS API failure.");
            }

            var tokenSet = ParseTokenSet(json);

            if (string.IsNullOrWhiteSpace(tokenSet.AccessToken))
            {
                throw new UsosApiException("USOS did not return an access token.");
            }

            if (!string.IsNullOrWhiteSpace(tokenSet.RefreshToken) && tokenSet.RefreshToken != refreshToken)
            {
                user.UsosRefreshTokenProtected = _protector.Protect(tokenSet.RefreshToken);
                await _userManager.UpdateAsync(user);
            }

            return tokenSet.AccessToken;
        }

        private async Task<IReadOnlyList<UsosEventDto>> FetchRemoteScheduleAsync(string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, _options.ScheduleEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("USOS schedule request failed. Status: {Status}. Body: {Body}", response.StatusCode, json);
                throw new UsosApiException("USOS API failure.");
            }

            return ParseSchedule(json);
        }

        private string ReadUserIdFromState(string state)
        {
            string rawState;

            try
            {
                rawState = _protector.Unprotect(state);
            }
            catch (Exception ex)
            {
                throw new UsosApiException("Invalid USOS OAuth state.", ex);
            }

            var parts = rawState.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || !long.TryParse(parts[1], out var ticks))
            {
                throw new UsosApiException("Invalid USOS OAuth state.");
            }

            if (DateTime.UtcNow > new DateTime(ticks, DateTimeKind.Utc))
            {
                throw new UsosApiException("Expired USOS OAuth state.");
            }

            return parts[0];
        }

        private OAuthTokenSet ParseTokenSet(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                return new OAuthTokenSet(
                    TryGetString(root, "access_token") ?? string.Empty,
                    TryGetString(root, "refresh_token") ?? string.Empty,
                    TryGetInt(root, "expires_in"));
            }
            catch (JsonException ex)
            {
                throw new UsosApiException("Invalid USOS token response.", ex);
            }
        }

        private static IReadOnlyList<UsosEventDto> ParseSchedule(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            JsonElement eventsElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                eventsElement = root;
            }
            else if (root.TryGetProperty("events", out var eventsProperty))
            {
                eventsElement = eventsProperty;
            }
            else if (root.TryGetProperty("items", out var itemsProperty))
            {
                eventsElement = itemsProperty;
            }
            else
            {
                return Array.Empty<UsosEventDto>();
            }

            if (eventsElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<UsosEventDto>();
            }

            var result = new List<UsosEventDto>();

            foreach (var item in eventsElement.EnumerateArray())
            {
                var startText = TryGetString(item, "start_time", "startTime", "start", "startDate");
                var endText = TryGetString(item, "end_time", "endTime", "end", "endDate");

                if (!TryParseDateTime(startText, out var startTime) || !TryParseDateTime(endText, out var endTime))
                {
                    continue;
                }

                if (endTime <= startTime)
                {
                    continue;
                }

                var externalId = TryGetString(item, "id", "event_id", "classgroup_id", "unit_id")
                    ?? $"{startTime:O}-{endTime:O}-{result.Count}";

                var title = TryGetString(item, "title", "name", "course_name", "courseName", "subject")
                    ?? "USOS class";

                var room = TryGetString(item, "room", "room_number", "roomNumber");
                var location = TryGetString(item, "location", "building", "place") ?? room;

                result.Add(new UsosEventDto
                {
                    ExternalId = externalId,
                    Title = title,
                    StartTime = startTime,
                    EndTime = endTime,
                    Location = location,
                    Room = room,
                    Teacher = TryGetString(item, "teacher", "lecturer", "instructor"),
                    Source = "usos",
                    IsReadOnly = true,
                    SyncedAtUtc = DateTime.UtcNow
                });
            }

            return result;
        }

        private static IReadOnlyList<UsosEventDto> BuildMockSchedule()
        {
            var today = DateTime.Today;

            return new List<UsosEventDto>
            {
                new()
                {
                    ExternalId = $"mock-usos-{today:yyyyMMdd}-1",
                    Title = "USOS: Mathematics lecture",
                    StartTime = today.AddDays(1).AddHours(8),
                    EndTime = today.AddDays(1).AddHours(9).AddMinutes(30),
                    Location = "Main Campus",
                    Room = "Aula 101",
                    Teacher = "USOS Demo Teacher",
                    Source = "usos",
                    IsReadOnly = true,
                    SyncedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    ExternalId = $"mock-usos-{today:yyyyMMdd}-2",
                    Title = "USOS: Programming lab",
                    StartTime = today.AddDays(2).AddHours(12),
                    EndTime = today.AddDays(2).AddHours(13).AddMinutes(30),
                    Location = "Computer Science Building",
                    Room = "Lab 204",
                    Teacher = "USOS Demo Teacher",
                    Source = "usos",
                    IsReadOnly = true,
                    SyncedAtUtc = DateTime.UtcNow
                }
            };
        }

        private bool IsOAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(_options.AuthorizationEndpoint)
                   && !string.IsNullOrWhiteSpace(_options.TokenEndpoint)
                   && !string.IsNullOrWhiteSpace(_options.ClientId)
                   && !string.IsNullOrWhiteSpace(_options.ClientSecret)
                   && !string.IsNullOrWhiteSpace(_options.RedirectUri);
        }

        private bool IsRemoteScheduleConfigured()
        {
            return IsOAuthConfigured()
                   && !string.IsNullOrWhiteSpace(_options.ScheduleEndpoint);
        }

        private static string? TryGetString(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (!element.TryGetProperty(name, out var property))
                {
                    continue;
                }

                if (property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }

                if (property.ValueKind == JsonValueKind.Number
                    || property.ValueKind == JsonValueKind.True
                    || property.ValueKind == JsonValueKind.False)
                {
                    return property.ToString();
                }
            }

            return null;
        }

        private static int? TryGetInt(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            {
                return value;
            }

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
            {
                return value;
            }

            return null;
        }

        private static bool TryParseDateTime(string? value, out DateTime dateTime)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                dateTime = default;
                return false;
            }

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out dateTime);
        }

        private sealed record OAuthTokenSet(string AccessToken, string RefreshToken, int? ExpiresInSeconds);
    }
}
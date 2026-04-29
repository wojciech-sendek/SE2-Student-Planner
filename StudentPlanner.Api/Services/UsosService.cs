using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentPlanner.Api.Configurations;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class UsosApiException : Exception
    {
        public UsosApiException(string message) : base(message)
        {
        }

        public UsosApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class UsosAuthorizationRequiredException : UsosApiException
    {
        public UsosAuthorizationRequiredException()
            : base("USOS authorization required.")
        {
        }
    }

    public class UsosService : IUsosService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly UsosOptions _options;
        private readonly IDataProtector _protector;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsosService(
            ApplicationDbContext dbContext,
            HttpClient httpClient,
            IOptions<UsosOptions> options,
            IDataProtectionProvider dataProtectionProvider)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _options = options.Value;
            _protector = dataProtectionProvider.CreateProtector("StudentPlanner.UsosTokens.v1");
        }

        public async Task<string> BuildAuthorizationUrlAsync(string userId)
        {
            var state = CreateSecureState();

            _dbContext.UsosOAuthStates.Add(new UsosOAuthState
            {
                UserId = userId,
                State = state,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_options.OAuthStateLifetimeMinutes)
            });

            await _dbContext.SaveChangesAsync();

            return BuildUrl(_options.AuthorizationEndpoint, new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = _options.ClientId,
                ["redirect_uri"] = _options.RedirectUri,
                ["scope"] = _options.Scope,
                ["state"] = state
            });
        }

        public async Task CompleteAuthorizationAsync(string code, string state)
        {
            var savedState = await _dbContext.UsosOAuthStates
                .FirstOrDefaultAsync(s => s.State == state);

            if (savedState is null || savedState.ExpiresAtUtc < DateTime.UtcNow)
            {
                throw new UsosApiException("Invalid or expired USOS OAuth state.");
            }

            var tokenResponse = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri,
                ["code"] = code
            });

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) ||
                string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                throw new UsosApiException("USOS token endpoint did not return access_token and refresh_token.");
            }

            await UpsertTokenAsync(savedState.UserId, tokenResponse);

            _dbContext.UsosOAuthStates.Remove(savedState);
            await _dbContext.SaveChangesAsync();

            await SyncAsync(savedState.UserId);
        }

        public async Task EnsureConnectedAndSyncedAsync(string userId)
        {
            var token = await _dbContext.UsosTokens.FirstOrDefaultAsync(t => t.UserId == userId);

            if (token is null)
            {
                throw new UsosAuthorizationRequiredException();
            }

            await SyncAsync(userId);
        }

        public async Task SyncAsync(string userId)
        {
            var token = await GetValidTokenAsync(userId);

            using var request = new HttpRequestMessage(HttpMethod.Get, _options.ScheduleEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                string.IsNullOrWhiteSpace(token.TokenType) ? "Bearer" : token.TokenType,
                _protector.Unprotect(token.AccessTokenEncrypted));

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new UsosApiException($"USOS API failure while fetching schedule. Status: {(int)response.StatusCode}. Body: {body}");
            }

            var parsedEvents = ParseSchedule(body, userId);

            var existing = await _dbContext.UsosEvents
                .Where(e => e.UserId == userId)
                .ToListAsync();

            _dbContext.UsosEvents.RemoveRange(existing);
            _dbContext.UsosEvents.AddRange(parsedEvents);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DisconnectAsync(string userId)
        {
            var tokens = await _dbContext.UsosTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var events = await _dbContext.UsosEvents
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var states = await _dbContext.UsosOAuthStates
                .Where(s => s.UserId == userId)
                .ToListAsync();

            _dbContext.UsosTokens.RemoveRange(tokens);
            _dbContext.UsosEvents.RemoveRange(events);
            _dbContext.UsosOAuthStates.RemoveRange(states);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UsosStatusDto> GetStatusAsync(string userId)
        {
            return new UsosStatusDto
            {
                IsConnected = await _dbContext.UsosTokens.AnyAsync(t => t.UserId == userId),
                SyncedEventsCount = await _dbContext.UsosEvents.CountAsync(e => e.UserId == userId)
            };
        }

        public async Task<IReadOnlyList<AcademicEventDto>> GetAcademicEventsAsync(
            string userId,
            DateTime? from = null,
            DateTime? to = null)
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
                .Select(e => new AcademicEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CourseId = e.CourseId,
                    LecturerName = e.LecturerName,
                    Room = e.Room,
                    EventType = "USOS",
                    IsPersonal = false,
                    IsReadOnly = true
                })
                .ToListAsync();
        }

        private async Task<UsosToken> GetValidTokenAsync(string userId)
        {
            var token = await _dbContext.UsosTokens.FirstOrDefaultAsync(t => t.UserId == userId);

            if (token is null)
            {
                throw new UsosAuthorizationRequiredException();
            }

            if (token.AccessTokenExpiresAtUtc > DateTime.UtcNow.AddMinutes(1))
            {
                return token;
            }

            var refreshToken = _protector.Unprotect(token.RefreshTokenEncrypted);

            var refreshed = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["refresh_token"] = refreshToken
            });

            if (string.IsNullOrWhiteSpace(refreshed.AccessToken))
            {
                throw new UsosApiException("USOS refresh token flow did not return access_token.");
            }

            token.AccessTokenEncrypted = _protector.Protect(refreshed.AccessToken);
            token.RefreshTokenEncrypted = _protector.Protect(
                string.IsNullOrWhiteSpace(refreshed.RefreshToken)
                    ? refreshToken
                    : refreshed.RefreshToken);

            token.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(
                refreshed.ExpiresIn > 0 ? refreshed.ExpiresIn : 3600);

            token.TokenType = refreshed.TokenType ?? token.TokenType;
            token.Scope = refreshed.Scope ?? token.Scope;
            token.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return token;
        }

        private async Task UpsertTokenAsync(string userId, TokenResponse tokenResponse)
        {
            var existing = await _dbContext.UsosTokens.FirstOrDefaultAsync(t => t.UserId == userId);

            if (existing is null)
            {
                _dbContext.UsosTokens.Add(new UsosToken
                {
                    UserId = userId,
                    AccessTokenEncrypted = _protector.Protect(tokenResponse.AccessToken!),
                    RefreshTokenEncrypted = _protector.Protect(tokenResponse.RefreshToken!),
                    AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(
                        tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600),
                    TokenType = tokenResponse.TokenType ?? "Bearer",
                    Scope = tokenResponse.Scope,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });

                return;
            }

            existing.AccessTokenEncrypted = _protector.Protect(tokenResponse.AccessToken!);
            existing.RefreshTokenEncrypted = _protector.Protect(tokenResponse.RefreshToken!);
            existing.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(
                tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600);
            existing.TokenType = tokenResponse.TokenType ?? "Bearer";
            existing.Scope = tokenResponse.Scope;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        private async Task<TokenResponse> RequestTokenAsync(Dictionary<string, string> form)
        {
            using var response = await _httpClient.PostAsync(
                _options.TokenEndpoint,
                new FormUrlEncodedContent(form));

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new UsosApiException($"USOS token endpoint failure. Status: {(int)response.StatusCode}. Body: {body}");
            }

            var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions);

            if (token is null)
            {
                throw new UsosApiException("USOS token endpoint returned invalid JSON.");
            }

            return token;
        }

        private static List<UsosEvent> ParseSchedule(string json, string userId)
        {
            using var document = JsonDocument.Parse(json);

            var result = new List<UsosEvent>();

            foreach (var item in ExtractEventElements(document.RootElement))
            {
                var title =
                    GetString(item, "title", "name", "course_name", "courseName", "subject")
                    ?? "USOS event";

                if (!TryGetDate(item, out var start, "startTime", "start_time", "start", "from"))
                {
                    continue;
                }

                if (!TryGetDate(item, out var end, "endTime", "end_time", "end", "to"))
                {
                    continue;
                }

                var room = GetString(item, "room", "room_number", "roomNumber");
                var location = GetString(item, "location", "building", "place") ?? room;

                result.Add(new UsosEvent
                {
                    UserId = userId,
                    Title = title,
                    StartTime = start,
                    EndTime = end,
                    Location = location,
                    Room = room,
                    CourseId = GetString(item, "courseId", "course_id", "courseCode", "course_code"),
                    LecturerName = GetString(item, "lecturerName", "lecturer_name", "teacher", "teacherName")
                });
            }

            return result;
        }

        private static IEnumerable<JsonElement> ExtractEventElements(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    yield return item;
                }

                yield break;
            }

            foreach (var propertyName in new[] { "events", "items", "classes", "schedule" })
            {
                if (TryGetProperty(root, propertyName, out var array) &&
                    array.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in array.EnumerateArray())
                    {
                        yield return item;
                    }

                    yield break;
                }
            }
        }

        private static bool TryGetDate(JsonElement element, out DateTime value, params string[] names)
        {
            var raw = GetString(element, names);

            if (DateTime.TryParse(
                    raw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out value))
            {
                value = value.ToUniversalTime();
                return true;
            }

            value = default;
            return false;
        }

        private static string? GetString(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (!TryGetProperty(element, name, out var property))
                {
                    continue;
                }

                if (property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }

                if (property.ValueKind == JsonValueKind.Number ||
                    property.ValueKind == JsonValueKind.True ||
                    property.ValueKind == JsonValueKind.False)
                {
                    return property.ToString();
                }
            }

            return null;
        }

        private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string BuildUrl(string baseUrl, IReadOnlyDictionary<string, string> query)
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";

            return baseUrl + separator + string.Join(
                "&",
                query.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        }

        private static string CreateSecureState()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-", StringComparison.Ordinal)
                .Replace("/", "_", StringComparison.Ordinal)
                .TrimEnd('=');
        }

        private sealed class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string? TokenType { get; set; }

            [JsonPropertyName("scope")]
            public string? Scope { get; set; }
        }
    }
}
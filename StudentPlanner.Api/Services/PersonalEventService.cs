using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.PersonalEvents;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class PersonalEventService : IPersonalEventService
    {
        private readonly ApplicationDbContext _dbContext;

        public PersonalEventService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<PersonalEventDto>> GetMineAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbContext.PersonalEvents
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
                .Select(e => new PersonalEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location
                })
                .ToListAsync();
        }

        public async Task<PersonalEventDto?> GetByIdAsync(int id, string userId)
        {
            return await _dbContext.PersonalEvents
                .AsNoTracking()
                .Where(e => e.Id == id && e.UserId == userId)
                .Select(e => new PersonalEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PersonalEventDto> CreateAsync(string userId, CreatePersonalEventRequestDto dto)
        {
            var entity = new PersonalEvent
            {
                Title = dto.Title.Trim(),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
                UserId = userId
            };

            _dbContext.PersonalEvents.Add(entity);
            await _dbContext.SaveChangesAsync();

            return new PersonalEventDto
            {
                Id = entity.Id,
                Title = entity.Title,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Location = entity.Location
            };
        }

        public async Task<PersonalEventDto?> UpdateAsync(int id, string userId, UpdatePersonalEventRequestDto dto)
        {
            var entity = await _dbContext.PersonalEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (entity is null)
            {
                return null;
            }

            entity.Title = dto.Title.Trim();
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();

            await _dbContext.SaveChangesAsync();

            return new PersonalEventDto
            {
                Id = entity.Id,
                Title = entity.Title,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Location = entity.Location
            };
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var entity = await _dbContext.PersonalEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (entity is null)
            {
                return false;
            }

            _dbContext.PersonalEvents.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
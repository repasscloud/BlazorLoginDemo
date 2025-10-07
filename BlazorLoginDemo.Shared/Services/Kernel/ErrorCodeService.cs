using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Kernel;

public sealed class ErrorCodeService : IErrorCodeService
{
    private readonly ApplicationDbContext _db;

    public ErrorCodeService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ---- READ ----
    public async Task<ErrorCodeUnified?> GetErrorAsync(string errorCode)
    {
        return await _db.Set<ErrorCodeUnified>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ErrorCode == errorCode);
    }

    public async Task<ErrorCodeUnified?> GetByIdAsync(long id)
    {
        return await _db.Set<ErrorCodeUnified>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<ErrorCodeUnified>> GetAllAsync()
    {
        return await _db.Set<ErrorCodeUnified>()
            .AsNoTracking()
            .OrderBy(e => e.ErrorCode)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string errorCode)
    {
        return await _db.Set<ErrorCodeUnified>()
            .AsNoTracking()
            .AnyAsync(e => e.ErrorCode == errorCode);
    }

    // ---- CREATE ----
    public async Task<ErrorCodeUnified> CreateAsync(ErrorCodeUnified entity)
    {
        if (await ExistsAsync(entity.ErrorCode))
            throw new InvalidOperationException($"ErrorCode '{entity.ErrorCode}' already exists.");

        _db.Set<ErrorCodeUnified>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    // ---- UPDATE ----
    public async Task<bool> UpdateAsync(ErrorCodeUnified entity)
    {
        var existing = await _db.Set<ErrorCodeUnified>()
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (existing is null)
            return false;

        // Copy only mutable fields. Do NOT touch CreatedOnUtc.
        existing.ErrorCode = entity.ErrorCode;
        existing.Title = entity.Title;
        existing.Message = entity.Message;
        existing.Resolution = entity.Resolution;
        existing.ContactSupportLink = entity.ContactSupportLink;
        existing.IsClientFacing = entity.IsClientFacing;
        existing.IsInternalFacing = entity.IsInternalFacing;

        await _db.SaveChangesAsync();
        return true;
    }

    // ---- DELETE ----
    public async Task<bool> DeleteAsync(long id)
    {
        var existing = await _db.Set<ErrorCodeUnified>().FindAsync(id);
        if (existing == null)
            return false;

        _db.Set<ErrorCodeUnified>().Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}

using BlazorLoginDemo.Shared.Models.Kernel.SysVar;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Kernel;

public interface IErrorCodeService
{
    Task<ErrorCodeUnified?> GetErrorAsync(string errorCode);
    Task<ErrorCodeUnified?> GetByIdAsync(long id);
    Task<IReadOnlyList<ErrorCodeUnified>> GetAllAsync();

    Task<ErrorCodeUnified> CreateAsync(ErrorCodeUnified entity);
    Task<bool> UpdateAsync(ErrorCodeUnified entity);
    Task<bool> DeleteAsync(long id);

    Task<bool> ExistsAsync(string errorCode);
}
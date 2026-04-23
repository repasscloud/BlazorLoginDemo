using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Services.Interfaces.Kernel;

public interface IPolicyPdfService
{
    Task<byte[]> GenerateAsync(TravelPolicy policy);
}
namespace BlazorLoginDemo.Shared.Services.Interfaces.Kernel;

public interface IPolicyPdfService
{
    Task<byte[]> GenerateAsync(TravelPolicy policy);
}
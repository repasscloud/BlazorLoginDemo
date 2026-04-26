using Cinturon360.Contracts.Bookings;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class BookingApiClient : ApiClientBase
{
    public BookingApiClient(HttpClient http, ILogger<BookingApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<BookingResponse>> GetBookingAsync(string bookingId, CancellationToken ct = default)
        => GetAsync<BookingResponse>($"api/v1/bookings/{bookingId}", ct);

    public Task<ApiResult<BookingListResponse>> ListBookingsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => GetAsync<BookingListResponse>($"api/v1/bookings?page={page}&pageSize={pageSize}", ct);

    public Task<ApiResult<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
        => PostAsync<BookingResponse>("api/v1/bookings", request, ct);

    public Task<ApiResult<BookingResponse>> UpdateStatusAsync(string bookingId, UpdateBookingStatusRequest request, CancellationToken ct = default)
        => PutAsync<BookingResponse>($"api/v1/bookings/{bookingId}/status", request, ct);

    public Task<ApiResult<bool>> CancelBookingAsync(string bookingId, CancellationToken ct = default)
        => DeleteAsync($"api/v1/bookings/{bookingId}", ct);
}

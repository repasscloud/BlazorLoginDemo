using BlazorLoginDemo.Shared.Models.Kernel.FX;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using Microsoft.AspNetCore.Mvc;
namespace BlazorLoginDemo.Api.Controllers.Fx
{
    [ApiController]
    [Route("api/fx")]
    public sealed class FxController : ControllerBase
    {
        private readonly IFxRateService _fx;

        public FxController(IFxRateService fx) => _fx = fx;

        // GET /api/fx/rates/USD
        [HttpGet("rates/{baseCode}")]
        public async Task<ActionResult<ExchangeRateResponse>> GetRates([FromRoute] string baseCode, CancellationToken ct)
        {
            var data = await _fx.GetRatesAsync(baseCode, ct);
            return Ok(data);
        }

        // GET /api/fx/convert?from=USD&to=AUD&amount=123.45
        [HttpGet("convert")]
        public async Task<ActionResult<ConvertResult>> Convert([FromQuery] ConvertQuery q, CancellationToken ct)
        {
            if (q.Amount < 0) return BadRequest("Amount must be >= 0.");
            var result = await _fx.ConvertAsync(q.From, q.To, q.Amount, ct);
            return Ok(result);
        }

        // GET /api/fx/rate?from=USD&to=GBP
        [HttpGet("rate")]
        public async Task<ActionResult<decimal>> GetRate([FromQuery] string from, [FromQuery] string to, CancellationToken ct)
        {
            var rate = await _fx.GetRateAsync(from, to, ct);
            return Ok(rate);
        }
    }
}

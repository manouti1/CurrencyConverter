using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public CurrencyController(IExchangeService exchangeService) => _exchangeService = exchangeService;

    /// <summary>Convert an amount from one currency to another.</summary>
    [HttpGet("convert")]
    [Authorize(Policy = "Convert")]
    public async Task<ActionResult<ConvertResponse>> Convert([FromQuery] ConvertRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _exchangeService.ConvertAsync(request.From, request.To, request.Amount, ct);
            return Ok(new ConvertResponse(request.From, request.To, request.Amount, result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Get latest exchange rates for a base currency.</summary>
    [HttpGet("rates/latest")]
    public async Task<IActionResult> LatestRates([FromQuery] string baseCurrency = "USD", CancellationToken ct = default)
    {
        try
        {
            var rates = await _exchangeService.GetLatestRatesAsync(baseCurrency, ct);
            return Ok(rates);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Get historical rates between two dates.</summary>
    [HttpGet("rates/history")]
    [Authorize(Policy = "History")]
    public async Task<IActionResult> History(
    [FromQuery] string baseCurrency,
    [FromQuery] DateTime from,
    [FromQuery] DateTime to,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken ct = default)
    {
        if (from > to)
            return BadRequest("'from' must be earlier than 'to'");

        var pagination = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        try
        {
            var history = await _exchangeService.GetHistoryAsync(baseCurrency, from, to, pagination, ct);
            Response.Headers.Append("X-Total-Count", history.TotalCount.ToString());
            Response.Headers.Append("X-Total-Pages", history.TotalPages.ToString());

            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
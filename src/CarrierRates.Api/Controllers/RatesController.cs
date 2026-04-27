using CarrierRates.Api.Contracts;
using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRates.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/rates")]
public class RatesController(IRateQueryService rateQueryService) : ControllerBase
{
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] RateQueryRequestDto request, CancellationToken cancellationToken)
    {
        var query = new RateQuery(
            new Address(request.Origin.PostalCode, request.Origin.CountryCode),
            new Address(request.Destination.PostalCode, request.Destination.CountryCode),
            new PackageDetails(
                request.Package.Weight,
                new Dimensions(
                    request.Package.Dimensions.Length,
                    request.Package.Dimensions.Width,
                    request.Package.Dimensions.Height
                )
            )
        );

        var result = await rateQueryService.QueryAsync(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "No carrier rate available.",
                carrierErrors = result.CarrierErrors
            });
        }

        var payload = result.Value.Select(MapResponse).ToArray();
        return Ok(new
        {
            rates = payload,
            warnings = result.Warnings
        });
    }

    private static ShippingRateResponseDto MapResponse(ShippingRateResponse response)
    {
        return new ShippingRateResponseDto
        {
            Carrier = response.Carrier,
            RateOptions = response.RateOptions.Select(
                option => new RateOptionDto
                {
                    ServiceName = option.ServiceName,
                    EstimatedDelivery = option.EstimatedDelivery,
                    Price = new MoneyDto(option.Price.Amount, option.Price.Currency)
                }
            )
        };
    }
}

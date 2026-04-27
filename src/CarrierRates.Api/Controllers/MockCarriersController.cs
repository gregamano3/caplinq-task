using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRates.Api.Controllers;

[ApiController]
[AllowAnonymous]
public class MockCarriersController : ControllerBase
{
    [HttpPost("api/fedex/rates")]
    public IActionResult FedExRates([FromBody] object request)
    {
        return Ok(new
        {
            carrier = "FedEx",
            serviceOptions = new[]
            {
                new { serviceName = "FedEx Ground", estimatedDelivery = DateTime.UtcNow.Date.AddDays(6), rate = 12.34m },
                new { serviceName = "FedEx 2Day", estimatedDelivery = DateTime.UtcNow.Date.AddDays(2), rate = 25.67m },
                new { serviceName = "FedEx Overnight", estimatedDelivery = DateTime.UtcNow.Date.AddDays(1), rate = 45.89m }
            }
        });
    }

    [HttpPost("api/dhl/rates")]
    public IActionResult DhlRates([FromBody] object request)
    {
        return Ok(new
        {
            provider = "DHL",
            options = new[]
            {
                new { name = "DHL Economy Select", deliveryDate = DateTime.UtcNow.Date.AddDays(7), price = 11.00m },
                new { name = "DHL Express Worldwide", deliveryDate = DateTime.UtcNow.Date.AddDays(2), price = 22.50m },
                new { name = "DHL Same Day", deliveryDate = DateTime.UtcNow.Date.AddDays(1), price = 35.00m }
            }
        });
    }

    [HttpPost("api/ups/shipping-rates")]
    public IActionResult UpsRates([FromBody] object request)
    {
        return Ok(new
        {
            company = "UPS",
            services = new[]
            {
                new { service = "UPS Ground", eta = DateTime.UtcNow.Date.AddDays(6), cost = 15.20m },
                new { service = "UPS 2nd Day Air", eta = DateTime.UtcNow.Date.AddDays(3), cost = 28.40m },
                new { service = "UPS Next Day Air", eta = DateTime.UtcNow.Date.AddDays(2), cost = 52.75m }
            }
        });
    }
}

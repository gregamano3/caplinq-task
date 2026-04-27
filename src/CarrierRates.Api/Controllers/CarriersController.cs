using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Application.Contracts.Carriers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRates.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/carriers")]
public class CarriersController(ICarrierManagementService carrierManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{carrierId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid carrierId, [FromBody] UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.UpdateAsync(carrierId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{carrierId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid carrierId, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.DeleteAsync(carrierId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{carrierId:guid}/enable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Enable(Guid carrierId, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.EnableAsync(carrierId, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{carrierId:guid}/disable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(
        Guid carrierId,
        [FromBody] DisableCarrierRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await carrierManagementService.DisableAsync(carrierId, request, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{carrierId:guid}/disable-requests")]
    public async Task<IActionResult> RequestDisable(
        Guid carrierId,
        [FromBody] DisableRequestCreate request,
        CancellationToken cancellationToken
    )
    {
        var result = await carrierManagementService.RequestDisableAsync(carrierId, request, cancellationToken);
        return result.IsSuccess ? Ok(new { requestId = result.Value }) : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{carrierId:guid}/disable-requests/{requestId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid carrierId, Guid requestId, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.ApproveDisableRequestAsync(requestId, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(new { errors = result.Errors });
    }

    [HttpPatch("{carrierId:guid}/disable-requests/{requestId:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid carrierId, Guid requestId, CancellationToken cancellationToken)
    {
        var result = await carrierManagementService.RejectDisableRequestAsync(requestId, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(new { errors = result.Errors });
    }
}

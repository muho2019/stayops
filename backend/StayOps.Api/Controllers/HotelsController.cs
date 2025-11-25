using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Api.Contracts;
using StayOps.Api.Contracts.Hotels;
using StayOps.Application.Hotels.Commands;
using StayOps.Application.Hotels.Services;
using StayOps.Application.Users;
using StayOps.Domain.Hotels;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/hotels")]
[Authorize]
public sealed class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));
    }

    [HttpGet]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(PagedResponse<HotelResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHotels(
        [FromQuery] HotelStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _hotelService.GetHotelsAsync(new ListHotelsQuery(status, page, pageSize), cancellationToken);
        var response = new PagedResponse<HotelResponse>(
            result.Items.Select(Map).ToArray(),
            result.Total,
            result.Page,
            result.PageSize);

        return Ok(response);
    }

    [HttpGet("{hotelId:guid}")]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHotel(Guid hotelId, CancellationToken cancellationToken)
    {
        var result = await _hotelService.GetHotelAsync(hotelId, cancellationToken);
        return Ok(Map(result));
    }

    [HttpPost]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateHotelCommand(request.Code, request.Name, request.Timezone, request.Status);
        var result = await _hotelService.CreateHotelAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetHotel), new { hotelId = result.Id }, Map(result));
    }

    [HttpPut("{hotelId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateHotel(Guid hotelId, [FromBody] UpdateHotelRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateHotelCommand(hotelId, request.Code, request.Name, request.Timezone, request.Status);
        await _hotelService.UpdateHotelAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{hotelId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteHotel(Guid hotelId, CancellationToken cancellationToken)
    {
        await _hotelService.DeleteHotelAsync(new DeleteHotelCommand(hotelId), cancellationToken);
        return NoContent();
    }

    private static HotelResponse Map(Application.Hotels.Contracts.HotelResult result)
    {
        return new HotelResponse(
            result.Id,
            result.Code,
            result.Name,
            result.Timezone,
            result.Status,
            result.CreatedAt,
            result.UpdatedAt);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Api.Contracts;
using StayOps.Api.Contracts.RoomTypes;
using StayOps.Application.RoomTypes.Commands;
using StayOps.Application.RoomTypes.Services;
using StayOps.Application.Users;
using StayOps.Domain.RoomTypes;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/room-types")]
[Authorize]
public sealed class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService ?? throw new ArgumentNullException(nameof(roomTypeService));
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(PagedResponse<RoomTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomTypes(
        [FromQuery] Guid hotelId,
        [FromQuery] RoomTypeStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _roomTypeService.GetListAsync(new ListRoomTypesQuery(hotelId, status, page, pageSize), cancellationToken);
        var response = new PagedResponse<RoomTypeResponse>(
            result.Items.Select(Map).ToArray(),
            result.Total,
            result.Page,
            result.PageSize);

        return Ok(response);
    }

    [HttpGet("{roomTypeId:guid}")]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomType(Guid roomTypeId, CancellationToken cancellationToken)
    {
        var result = await _roomTypeService.GetAsync(roomTypeId, cancellationToken);
        return Ok(Map(result));
    }

    [HttpPost]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoomTypeCommand(
            request.HotelId,
            request.Name,
            request.Description,
            request.Capacity,
            request.BaseRate,
            request.Status);

        var result = await _roomTypeService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetRoomType), new { roomTypeId = result.Id }, Map(result));
    }

    [HttpPut("{roomTypeId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRoomType(Guid roomTypeId, [FromBody] UpdateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRoomTypeCommand(
            roomTypeId,
            request.Name,
            request.Description,
            request.Capacity,
            request.BaseRate,
            request.Status);

        await _roomTypeService.UpdateAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{roomTypeId:guid}/status")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeRoomTypeStatus(Guid roomTypeId, [FromBody] ChangeRoomTypeStatusRequest request, CancellationToken cancellationToken)
    {
        await _roomTypeService.ChangeStatusAsync(new ChangeRoomTypeStatusCommand(roomTypeId, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{roomTypeId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRoomType(Guid roomTypeId, CancellationToken cancellationToken)
    {
        await _roomTypeService.DeleteAsync(new DeleteRoomTypeCommand(roomTypeId), cancellationToken);
        return NoContent();
    }

    private static RoomTypeResponse Map(Application.RoomTypes.Contracts.RoomTypeResult result)
    {
        return new RoomTypeResponse(
            result.Id,
            result.HotelId,
            result.Name,
            result.Description,
            result.Capacity,
            result.BaseRate,
            result.Status,
            result.CreatedAt,
            result.UpdatedAt);
    }
}

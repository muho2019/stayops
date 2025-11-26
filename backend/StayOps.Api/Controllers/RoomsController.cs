using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Api.Contracts;
using StayOps.Api.Contracts.Rooms;
using StayOps.Application.Rooms.Commands;
using StayOps.Application.Rooms.Services;
using StayOps.Application.Users;
using StayOps.Domain.Abstractions;
using StayOps.Domain.Rooms;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/rooms")]
[Authorize]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(PagedResponse<RoomResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRooms(
        [FromQuery] Guid hotelId,
        [FromQuery] RoomStatus? status,
        [FromQuery] Guid? roomTypeId,
        [FromQuery] HousekeepingStatus? housekeepingStatus,
        [FromQuery] string? number,
        [FromQuery] int? floor,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _roomService.GetListAsync(
            new ListRoomsQuery(hotelId, status, roomTypeId, housekeepingStatus, number, floor, page, pageSize),
            cancellationToken);

        var response = new PagedResponse<RoomResponse>(
            result.Items.Select(Map).ToArray(),
            result.Total,
            result.Page,
            result.PageSize);

        return Ok(response);
    }

    [HttpGet("{roomId:guid}")]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoom(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _roomService.GetAsync(roomId, cancellationToken);
        return Ok(Map(result));
    }

    [HttpPost]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(
            request.HotelId,
            request.RoomTypeId,
            request.Number,
            request.Floor,
            request.Status,
            request.HousekeepingStatus ?? HousekeepingStatus.Clean);

        var result = await _roomService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetRoom), new { roomId = result.Id }, Map(result));
    }

    [HttpPut("{roomId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRoom(Guid roomId, [FromBody] UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        byte[]? rowVersion = ParseRowVersion(request.RowVersion);
        var command = new UpdateRoomCommand(
            roomId,
            request.RoomTypeId,
            request.Number,
            request.Floor,
            request.Status,
            request.HousekeepingStatus,
            rowVersion);

        await _roomService.UpdateAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{roomId:guid}/status")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeStatus(Guid roomId, [FromBody] ChangeRoomStatusRequest request, CancellationToken cancellationToken)
    {
        await _roomService.ChangeStatusAsync(new ChangeRoomStatusCommand(roomId, request.Status, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{roomId:guid}/housekeeping-status")]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeHousekeepingStatus(Guid roomId, [FromBody] ChangeHousekeepingStatusRequest request, CancellationToken cancellationToken)
    {
        await _roomService.ChangeHousekeepingStatusAsync(new ChangeHousekeepingStatusCommand(roomId, request.HousekeepingStatus, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{roomId:guid}")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRoom(Guid roomId, CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(new DeleteRoomCommand(roomId), cancellationToken);
        return NoContent();
    }

    [HttpGet("summary")]
    [Authorize(Roles = $"{UserRoleDefaults.AdminRoleName},{UserRoleDefaults.StaffRoleName}")]
    [ProducesResponseType(typeof(IEnumerable<RoomSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] Guid hotelId, CancellationToken cancellationToken)
    {
        var summary = await _roomService.GetSummaryAsync(hotelId, cancellationToken);
        var response = summary
            .Select(item => new RoomSummaryResponse(
                item.RoomTypeId,
                item.RoomTypeName,
                item.Total,
                item.Active,
                item.OutOfService,
                item.Dirty))
            .ToArray();

        return Ok(response);
    }

    [HttpGet("{roomId:guid}/history")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(PagedResponse<RoomHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        Guid roomId,
        [FromQuery] Guid hotelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _roomService.GetHistoryAsync(hotelId, roomId, page, pageSize, cancellationToken);
        var response = new PagedResponse<RoomHistoryResponse>(
            result.Items.Select(h => new RoomHistoryResponse(
                h.Id,
                h.RoomId,
                h.Status,
                h.HousekeepingStatus,
                h.Action,
                h.Reason,
                h.CreatedAt)).ToArray(),
            result.Total,
            result.Page,
            result.PageSize);

        return Ok(response);
    }

    private byte[]? ParseRowVersion(string? rowVersion)
    {
        string? value = rowVersion;
        if (string.IsNullOrWhiteSpace(value) && Request.Headers.TryGetValue("If-Match", out var headerValue))
        {
            value = headerValue.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim('"');

        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            throw new DomainException("Invalid row version format.");
        }
    }

    private static RoomResponse Map(Application.Rooms.Contracts.RoomResult result)
    {
        return new RoomResponse(
            result.Id,
            result.HotelId,
            result.RoomTypeId,
            result.Number,
            result.Floor,
            result.Status,
            result.HousekeepingStatus,
            result.CreatedAt,
            result.UpdatedAt,
            result.RowVersion);
    }
}

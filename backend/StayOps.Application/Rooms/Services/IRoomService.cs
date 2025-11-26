using StayOps.Application.Abstractions;
using StayOps.Application.Rooms.Commands;
using StayOps.Application.Rooms.Contracts;

namespace StayOps.Application.Rooms.Services;

public interface IRoomService
{
    Task<RoomResult> CreateAsync(CreateRoomCommand command, CancellationToken cancellationToken);

    Task UpdateAsync(UpdateRoomCommand command, CancellationToken cancellationToken);

    Task ChangeStatusAsync(ChangeRoomStatusCommand command, CancellationToken cancellationToken);

    Task ChangeHousekeepingStatusAsync(ChangeHousekeepingStatusCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(DeleteRoomCommand command, CancellationToken cancellationToken);

    Task<RoomResult> GetAsync(Guid roomId, CancellationToken cancellationToken);

    Task<PagedResult<RoomResult>> GetListAsync(ListRoomsQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoomSummaryItem>> GetSummaryAsync(Guid hotelId, CancellationToken cancellationToken);

    Task<PagedResult<RoomHistoryResult>> GetHistoryAsync(Guid hotelId, Guid roomId, int page, int pageSize, CancellationToken cancellationToken);
}

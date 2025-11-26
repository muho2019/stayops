using StayOps.Application.Abstractions;
using StayOps.Application.RoomTypes.Commands;
using StayOps.Application.RoomTypes.Contracts;

namespace StayOps.Application.RoomTypes.Services;

public interface IRoomTypeService
{
    Task<RoomTypeResult> CreateAsync(CreateRoomTypeCommand command, CancellationToken cancellationToken);

    Task UpdateAsync(UpdateRoomTypeCommand command, CancellationToken cancellationToken);

    Task ChangeStatusAsync(ChangeRoomTypeStatusCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(DeleteRoomTypeCommand command, CancellationToken cancellationToken);

    Task<RoomTypeResult> GetAsync(Guid roomTypeId, CancellationToken cancellationToken);

    Task<PagedResult<RoomTypeResult>> GetListAsync(ListRoomTypesQuery query, CancellationToken cancellationToken);
}

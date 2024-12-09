using BattleShipAPI.Enums;
using BattleShipAPI.Mediator;
using BattleShipAPI.Models;

namespace BattleShipAPI.Services;

public class GameValidationService : BaseComponent
{
    public async Task ValidateAttackRequest(AttackInformation request)
    {
        var cell = request.GameRoom.Board.Cells[request.AttackRequest.X][request.AttackRequest.Y];
            
        if (cell.OwnerId == request.Connection.PlayerId)
        {
            await _mediator.Notify("InformClient", new InformClientRequest()
            {
                Clients = request.Clients,
                ClientId = request.Context.ConnectionId,
                Key = "FailedToAttackCell",
                Values = ["You cannot attack your own territory."]
            });
            return;
        }

        if (cell.State == CellState.DamagedShip || cell.State == CellState.SunkenShip ||
            cell.State == CellState.Missed)
        {
            await _mediator.Notify("InformClient", new InformClientRequest()
            {
                Clients = request.Clients,
                ClientId = request.Context.ConnectionId,
                Key = "FailedToAttackCell",
                Values = ["This territory has already been attacked."]
            });
            return;
        }
        
        await _mediator.Notify("InformAboutAttackCellRequestValidationSuccess", request);
    }
}
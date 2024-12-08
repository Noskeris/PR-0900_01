using BattleShipAPI.Enums;
using BattleShipAPI.Mediator;
using BattleShipAPI.Models;

namespace BattleShipAPI.Services;

public class GameValidationService : BaseComponent
{
    public void ValidateAttackRequest(AttackRequestValidationRequest request)
    {
        if (request.Cell.OwnerId == request.Connection.PlayerId)
        {
            // Call mediator InformAboutAttackCellRequestValidationFailure with message "You cannot attack your own territory."
            await _mediator.Notify();
            return;
        }

        if (request.Cell.State == CellState.DamagedShip || request.Cell.State == CellState.SunkenShip ||
            request.Cell.State == CellState.Missed)
        {
            
            // Call mediator InformAboutAttackCellRequestValidationFailure with message "This territory has already been attacked.");
            return;
        }
        
        // Call mediator InformAboutAttackCellRequestValidationSuccess
    }
}
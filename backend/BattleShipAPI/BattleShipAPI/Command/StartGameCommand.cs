﻿namespace BattleShipAPI.Command;

public class StartGameCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.HandleAction("StartGame", context.CallerContext, context.Clients);
    }
}
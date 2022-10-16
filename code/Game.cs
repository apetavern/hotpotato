global using Sandbox;

global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace Potato;

// hot potato lookin ass game
public partial class Game : Sandbox.Game
{
	[Net] public Player PotatoHolder { get; set; }

	public Game()
	{

	}

	[Event.Tick.Server]
	public void Tick()
	{
		DebugOverlay.ScreenText( "Potato Holder:  " + PotatoHolder?.Client.Name, 0 );
		DebugOverlay.ScreenText( "Active State:  " + ActiveState.ToString(), 1 );
		DebugOverlay.ScreenText( "State Timer:  " + StateTimer.ToString(), 2 );
		DebugOverlay.ScreenText( "Potato Timer:  " + PotatoTimer.ToString(), 3 );
	}

	public override void ClientJoined( Client cl )
	{
		var player = new Player( cl );
		cl.Pawn = player;
		player.Spawn();
	}

	public override void DoPlayerNoclip( Client player )
	{
		base.DoPlayerNoclip( player );
	}
}

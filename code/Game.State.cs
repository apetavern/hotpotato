namespace Potato;

public partial class Game
{
	public enum GameState
	{
		Waiting,
		Countdown,
		Playing,
		End
	}

	[Net]
	public GameState ActiveState { get; set; } = GameState.Waiting;

	[Net]
	public TimeUntil StateTimer { get; set; }


	[Net]
	public TimeUntil PotatoTimer { get; set; }

	[Net]
	public Player WinningPlayer { get; set; }

	public void ChangeState( GameState @new )
	{
		HandleStateEnd( ActiveState );
		ActiveState = @new;
		HandleStateStart( ActiveState );
	}

	[Event.Tick.Server]
	public void TickState()
	{
		var players = All.OfType<Player>().Where( p => p.IsAlive );

		if ( ActiveState is GameState.Waiting )
		{
			if ( Client.All.Count > 2 )
			{
				ChangeState( GameState.Countdown );
			}
		}
		else if ( ActiveState is GameState.Countdown )
		{
			if ( StateTimer <= 0f )
			{
				ChangeState( GameState.Playing );
			}
		}
		else if ( ActiveState is GameState.Playing )
		{
			if ( players.Count() == 1 )
			{
				WinningPlayer = players.First();
				ChangeState( GameState.End );
			}

			if ( PotatoTimer <= 0f )
			{
				PotatoTimer = 10f;
				PotatoHolder.OnKilled();
				SelectNewPotatoHolder();
			}
		}
		else if ( ActiveState is GameState.End )
		{
			if ( StateTimer <= 0f )
			{
				ChangeState( GameState.Waiting );
			}
		}
	}

	private void HandleStateEnd( GameState state )
	{
		Log.Info( $"HandleStateEnd: {state}" );

		if ( state is GameState.Waiting )
		{
			StateTimer = 5f;
		}
		else if ( state is GameState.Countdown )
		{
			StateTimer = 30f + (10f * Client.All.Count);
			PotatoTimer = 10f;
		}
		else if ( state is GameState.Playing )
		{
			StateTimer = 5f;
		}
		else if ( state is GameState.End )
		{
			ResetGame();
		}
	}

	private void HandleStateStart( GameState state )
	{
		Log.Info( $"HandleStateStart: {state}" );

		if ( state is GameState.Waiting )
		{
			
		}
		else if ( state is GameState.Countdown )
		{

		}
		else if ( state is GameState.Playing )
		{
			SelectNewPotatoHolder();
		}
		else if ( state is GameState.End )
		{

		}
	}

	public void SelectNewPotatoHolder()
	{
		var player = All.OfType<Player>().Where( p => p.IsAlive ).OrderBy( p => Guid.NewGuid() ).First();

		Log.Info( player.Client.Name + " is the new potato holder." );
		SetPotatoHolder( player );
	}

	public void SetPotatoHolder( Player player )
	{
		PotatoHolder = player;
		player.EquipPotato();
	}

	public void ResetGame()
	{
		var players = All.OfType<Player>();
		var clients = Client.All;

		foreach (var player in players)
		{
			player.Delete();
		}

		foreach (var cl in clients)
		{
			var player = new Player( cl );
			cl.Pawn = player;
			player.Spawn();
		}

		WinningPlayer = null;
		PotatoHolder = null;
	}

	[ConCmd.Server]
	public static void PlayState()
	{
		var game = Current as Game;
		game.ChangeState( GameState.Playing );
	}
}

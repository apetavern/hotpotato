namespace Potato;

public partial class Player : AnimatedEntity
{
	[Net, Predicted]
	public PawnController Controller { get; set; }

	[Net, Predicted]
	public PawnAnimator Animator { get; set; }

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public ClothingContainer Clothing { get; } = new();

	[Net]
	public bool IsAlive { get; set; } = true;

	[Net]
	public bool HasPotato { get; set; } = false;

	[Net]
	public PotatoCarriable ActiveChild { get; private set; }

	public Particles SweatParticles { get; set; }

	public Player()
	{

	}

	public Player( Client cl )
	{
		Clothing.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Clothing.DressEntity( this );

		CreateHull();

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new ThirdPersonCamera();
	}

	public override void Simulate( Client cl )
	{
		var game = Sandbox.Game.Current as Game;

		Controller?.Simulate( cl, this, Animator );
		ActiveChild?.Simulate( cl );
		ActiveChild?.SimulateAnimator( Animator );

		if ( Input.Pressed( InputButton.PrimaryAttack ) && HasPotato )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 75f )
				.Ignore( this )
				.Run();

			if ( tr.Hit && tr.Entity is Player player && Host.IsServer )
			{
				game.SetPotatoHolder( player );
				ActiveChild.Delete();
				HasPotato = false;
			}
		}

		if ( HasPotato && game.ActiveState is Game.GameState.Playing )
		{
			SweatParticles ??= Particles.Create( "particles/sweat.vpcf" );
		}
		else
		{
			SweatParticles?.Destroy( true );
		}

		SweatParticles?.SetPosition( 0, EyePosition + Vector3.Up * 10f );
	}

	public override void FrameSimulate( Client cl )
	{
		Controller?.Simulate( cl, this, Animator );
	}

	public void EquipPotato()
	{
		HasPotato = true;
		ActiveChild = new PotatoCarriable();
		ActiveChild.ActiveStart( this );

		DoPotatoPickupEffects();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		ActiveChild?.DoExplodeEffects();
		DoDeathEffects();

		IsAlive = false;
	}

	public virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableHitboxes = true;
	}

	[ConCmd.Client]
	public void DoPotatoPickupEffects()
	{
		Sound.FromWorld( "potato_pickup", Position );
	}

	[ConCmd.Client]
	public void DoDeathEffects()
	{
		Sound.FromWorld( "player_explosion", Position );
		Particles.Create( "particles/impact.flesh-big.vpcf", Position );

		string[] gibModels =
		{
			"models/sbox_props/watermelon/watermelon_gib06.vmdl",
			"models/sbox_props/watermelon/watermelon_gib09.vmdl",
			"models/sbox_props/watermelon/watermelon_gib07.vmdl"
		};

		int numOfGibs = 6;

		for ( int i = 0; i < numOfGibs; i++ )
		{
			var gib = new ModelEntity( Rand.FromArray( gibModels ) );
			gib.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			gib.Position = Position;
			gib.PhysicsGroup.ApplyImpulse( Vector3.Random * 100f + Vector3.Up * 100f, false );

			gib.DeleteAsync( 8 );
		}
	}
}

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
	public BaseCarriable ActiveChild { get; private set; }

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
				var game = Sandbox.Game.Current as Game;
				game.SetPotatoHolder( player );
				ActiveChild.Delete();
				HasPotato = false;
			}
		}
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
	}

	public override void OnKilled()
	{
		base.OnKilled();

		IsAlive = false;
	}

	public virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableHitboxes = true;
	}
}

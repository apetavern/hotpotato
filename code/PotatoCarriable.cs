namespace Potato;

public class PotatoCarriable : BaseCarriable
{
	const int HoldType = 4;
	const int Handedness = 0;
	const float Pose = 2.7f;

	Sound TickSound { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Model = Model.Load( "models/potato/potato.vmdl" );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		SetParent( ent, true );

		TickSound = Sound.FromWorld( "timer_tick", Position );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		TickSound.Stop();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		TickSound.SetPosition( Position );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", HoldType );
		anim.SetAnimParameter( "holdtype_handedness", Handedness );
		anim.SetAnimParameter( "holdtype_pose", Pose );
	}

	[ConCmd.Client]
	public void DoExplodeEffects()
	{
		Particles.Create( "particles/explosion.vpcf", Position );
		Sound.FromWorld( "explosion_1", Position );
	}
}

namespace Potato;

public class PotatoCarriable : BaseCarriable
{
	const int HoldType = 4;
	const int Handedness = 0;
	const float Pose = 2.7f;

	public override void Spawn()
	{
		base.Spawn();

		Model = Model.Load( "models/potato/potato.vmdl" );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		SetParent( ent, true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", HoldType );
		anim.SetAnimParameter( "holdtype_handedness", Handedness );
		anim.SetAnimParameter( "holdtype_pose", Pose );
	}
}

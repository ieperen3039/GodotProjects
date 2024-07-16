using Godot;
using System;

public partial class Bolt : CharacterBody2D
{
	[Signal]
	public delegate void BoltHitsEnemyEventHandler(Bolt aBolt, Enemy aTarget);

	private ProjectileElementType mType;
	private ProjectileSize mSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Bolt Ready");
	}

	public override void _PhysicsProcess(double delta)
	{
		KinematicCollision2D lCollision = MoveAndCollide(Velocity);
		if (lCollision != null)
		{
			GodotObject lCollidedObject = lCollision.GetCollider();

			if (lCollidedObject is Enemy lEnemy)
			{
				EmitSignal(SignalName.BoltHitsEnemy, this, lEnemy);
			}
			else
			{
				GD.Print("Collided with non-enemy " + lCollidedObject);
			}
		}
	}

	public class SpawnModifiers {
		public const float BaseBoltSpeed = 3.0f;
		public float mSpeedAdditive = BaseBoltSpeed;
		public float mSpeedMultiplicative = 1;
		public ProjectileElementType mType = ProjectileElementType.Air;
		public ProjectileSize mSize = ProjectileSize.Primary;

		public void Apply(Bolt aTarget)
		{
			float lBoltSpeed = mSpeedAdditive * mSpeedMultiplicative;
			aTarget.Velocity = aTarget.Velocity.LimitLength(lBoltSpeed);
			aTarget.mType = mType;
			aTarget.mSize = mSize;
		}
	}
}

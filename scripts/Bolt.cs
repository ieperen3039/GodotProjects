using Godot;
using System;

public partial class Bolt : CharacterBody2D
{
	[Signal]
	public delegate void BoltHitsEnemyEventHandler(Bolt aBolt, Enemy aTarget);

	private ProjectileElementType Type;
	private ProjectileSize Size;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _PhysicsProcess(double aDelta)
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
		public const float BaseBoltSpeed = 10.0f;
		public float SpeedAdditive = BaseBoltSpeed;
		public float SpeedMultiplicative = 1;
		public ProjectileElementType Type = ProjectileElementType.Air;
		public ProjectileSize Size = ProjectileSize.Primary;

		public void Apply(Bolt aTarget)
		{
			float lBoltSpeed = SpeedAdditive * SpeedMultiplicative;
			aTarget.Velocity = aTarget.Velocity.LimitLength(lBoltSpeed);
			aTarget.Type = Type;
			aTarget.Size = Size;
		}
	}

	public class CollisionModifiers
	{
		public bool DoDespawn = true;

		public void Apply(Bolt aTarget){
			if (DoDespawn) {
				aTarget.QueueFree();
			}
		}
	}

}

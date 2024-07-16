using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int Hitpoints { get; private set; }

	public bool IsDead { get; private set; }

	public Vector2 MovementTarget;
	public float Speed;

	public override void _Ready() {

	}

    public void ApplyDamage(int aTotalDamage)
    {
		Hitpoints -= aTotalDamage;

		if (Hitpoints < 0) 
		{
            CollisionShape2D collider = GetNode<CollisionShape2D>("CollisionShape2D");
            collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		}
    }

    public override void _PhysicsProcess(double aDelta)
	{
		Vector2 lNewVelocity = (MovementTarget - Position).LimitLength(Speed);
		MoveAndCollide(lNewVelocity);
	}

	private void HandleInTowerArea()
	{
	}

	public class CollisionModifiers {
		public const float BaseDamage = 1.0f;
		public float DamageAdditive = BaseDamage;
		public float DamageMultiplicative = 1;

		internal void Apply(Enemy aEnemy)
		{
			int lTotalDamage = (int) (DamageAdditive * DamageMultiplicative);
			aEnemy.ApplyDamage(lTotalDamage);
		}
	}
}

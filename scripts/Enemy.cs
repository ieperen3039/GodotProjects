using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int mHitpoints;

	public Vector2 mMovementTarget;
	public float mSpeed;

	public override void _Ready() {

	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 lNewVelocity = (mMovementTarget - Position).LimitLength(mSpeed);
		MoveAndCollide(lNewVelocity);
	}

	private void HandleInTowerArea()
	{
	}

    public class CollisionModifiers {
		public const float BaseDamage = 1.0f;
		public float mDamageAdditive = BaseDamage;
		public float mDamageMultiplicative = 1;

        internal void Apply(Enemy aEnemy)
        {
			int lTotalDamage = (int) (mDamageAdditive * mDamageMultiplicative);
			aEnemy.mHitpoints -= lTotalDamage;
        }
    }
}

using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int mHitpoints { get; private set; }

	public bool mIsDead { get; private set; }

	public Vector2 mMovementTarget;
	public float mSpeed;

	public override void _Ready() {

	}

    public void ApplyDamage(int aTotalDamage)
    {
		mHitpoints -= aTotalDamage;

		if (mHitpoints < 0) 
		{
            CollisionShape2D collider = GetNode<CollisionShape2D>("CollisionShape2D");
            collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		}
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
			aEnemy.ApplyDamage(lTotalDamage);
		}
	}
}

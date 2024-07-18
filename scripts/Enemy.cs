using Godot;
using System;
using System.Diagnostics;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int MaxHitpoints { get; private set; } = 10;

	[Export]
	public float Speed = 1.0f;

	[Export]
	public int DamagePerAttack = 1;

	[Export]
	public double AttacksPerSecond = 1.0;

	public bool IsDead => state is Dieing;

	public Vector2 MovementTarget;

	private AnimationPlayer animationPlayer;
	private Polygon2D healthbar;

	private int currentHitpoints;
	private EnemyState state;

	public override void _Ready()
	{
		state = new Walking();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		healthbar = GetNode<Polygon2D>("Healthbar/Fill");
		currentHitpoints = MaxHitpoints;

		state.Ready(this);
	}

	public void ApplyDamage(int aTotalDamage)
	{
		SetHealth(currentHitpoints - aTotalDamage);
	}

	public override void _PhysicsProcess(double aDelta)
	{
		state.Process(this, aDelta);
		healthbar.Rotation = -Rotation;
	}

	private void HandleInTowerArea(Tower aTower)
	{
		state = new Attacking(aTower, 1.0 / AttacksPerSecond);
	}

	void SetHealth(int aNewHitpoints)
	{
		if (aNewHitpoints < 0) aNewHitpoints = 0;

		currentHitpoints = aNewHitpoints;
		float fraction = (float)currentHitpoints / MaxHitpoints;
		healthbar.Scale = new Vector2(fraction, 1);

		if (currentHitpoints == 0)
		{
			state = new Dieing();
			CollisionShape2D collider = GetNode<CollisionShape2D>("CollisionShape2D");
			collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		}
	}

	private interface EnemyState
	{
		void Ready(Enemy aThis);
		void Process(Enemy aThis, double aDelta);
	}

	private class Walking : EnemyState
	{
		public void Ready(Enemy aThis)
		{
			aThis.animationPlayer.Queue("walk");
		}

		public void Process(Enemy aThis, double aDelta)
		{
			if (aThis.Speed > 0)
			{
				Vector2 NewVelocity = (aThis.MovementTarget - aThis.Position).LimitLength(aThis.Speed);
				aThis.Rotation = NewVelocity.Angle();
				aThis.MoveAndCollide(NewVelocity);
			}
		}
	}

	private class Attacking : EnemyState
	{
		private double cooldownRemaining;
		public Tower target;

		public Attacking(Tower aTarget, double aCooldown)
		{
			target = aTarget;
			cooldownRemaining = aCooldown;
		}

		public void Ready(Enemy aThis)
		{
			aThis.animationPlayer.Queue("attack");
		}

		public void Process(Enemy aThis, double aDelta)
		{
			cooldownRemaining -= aDelta;

			if (cooldownRemaining <= 0)
			{
				target.ApplyDamage(aThis.DamagePerAttack);
				cooldownRemaining += 1.0 / aThis.AttacksPerSecond;
			}
		}
	}

	private class Dieing : EnemyState
	{
		public void Ready(Enemy aThis)
		{
			aThis.animationPlayer.Queue("die");
			aThis.healthbar.Visible = false;
			// aThis.Healthbar.SetDeferred(CanvasItem.PropertyName.Visible, false);
		}

		public void Process(Enemy aThis, double aDelta) { }
	}

	public class CollisionModifiers
	{
		public const float BaseDamage = 1.0f;
		public float DamageAdditive = BaseDamage;
		public float DamageMultiplicative = 1;

		internal void Apply(Enemy aEnemy)
		{
			int lTotalDamage = (int)(DamageAdditive * DamageMultiplicative);
			aEnemy.ApplyDamage(lTotalDamage);
		}
	}
}

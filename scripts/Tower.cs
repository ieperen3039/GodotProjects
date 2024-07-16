using Godot;
using System;

public partial class Tower : StaticBody2D
{
	[Signal]
	public delegate void TowerShootsBoltEventHandler(Bolt aBolt);

	private double mCooldown = 0.5f;
	private double mCooldownRemaining = 0;

	private PackedScene mBoltBlueprint;

	private Node2D mDirectionIndicator;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mDirectionIndicator = GetNode<Node2D>("DirectionIndicator");
		mBoltBlueprint = GD.Load<PackedScene>("res://scenes/blueprints/bolt.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 lMousePosition = GetGlobalMousePosition();
		Vector2 lMouseDirection = lMousePosition - Position;

		mDirectionIndicator.Rotation = lMouseDirection.Angle();
		mCooldownRemaining -= delta;

		if (Input.IsActionPressed("fire") && mCooldownRemaining <= 0)
		{
			HandleBoltFire(lMouseDirection);
			mCooldownRemaining += mCooldown;
		}

		if (mCooldownRemaining < 0) mCooldownRemaining = 0;
	}

	private void HandleBoltFire(Vector2 lClickDirection)
	{
		GD.Print("HandleBoltFire");

		Bolt lBolt = mBoltBlueprint.Instantiate<Bolt>();
		
		// Velocity magnitude will be overridden
		lBolt.Velocity = lClickDirection;
		lBolt.Position = Position;
		lBolt.Rotation = lClickDirection.Angle();

		EmitSignal(SignalName.TowerShootsBolt, lBolt);
	}

	public class ChangeModifiers
	{
		public float mCooldownReductionAdditive = 0;

		public void Apply(Tower aTower) {
			aTower.mCooldownRemaining -= mCooldownReductionAdditive;
		}
	}
}

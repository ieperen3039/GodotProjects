using Godot;
using System;
using System.Diagnostics;

public partial class Tower : StaticBody2D
{
    [Signal]
    public delegate void TowerShootsBoltEventHandler(Bolt aBolt);

    [Export]
    private Marker2D boltFireStartPosition;

    [Export]
    private PackedScene boltBlueprint;

    [Export]
    private Node2D directionIndicator;

    private double cooldown = 0.5f;
    private double cooldownRemaining = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(directionIndicator.GlobalPosition == boltFireStartPosition.GlobalPosition);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double aDelta)
    {
        Vector2 lMousePosition = GetGlobalMousePosition();
        Vector2 lMouseDirection = lMousePosition - boltFireStartPosition.GlobalPosition;

        directionIndicator.Rotation = lMouseDirection.Angle();
        cooldownRemaining -= aDelta;

        if (Input.IsActionPressed("fire") && cooldownRemaining <= 0)
        {
            HandleBoltFire(lMouseDirection);
            cooldownRemaining += cooldown;
        }

        if (cooldownRemaining < 0) cooldownRemaining = 0;
    }

    private void HandleBoltFire(Vector2 lClickDirection)
    {
        Bolt lBolt = boltBlueprint.Instantiate<Bolt>();

        // Velocity magnitude will be overridden
        lBolt.Position = boltFireStartPosition.GlobalPosition;
        lBolt.Rotation = lClickDirection.Angle();
        lBolt.Velocity = lClickDirection;
        lBolt.Type = ProjectileElementType.Arcane;
        lBolt.Size = ProjectileSize.Primary;

        EmitSignal(SignalName.TowerShootsBolt, lBolt);
    }

    private void HandleBodyEntersHurtArea(Node2D body)
    {
        GD.Print("HandleBodyEntersHurtArea");

        if (body is Enemy enemy)
        {
            enemy.HandleEnterTowerArea(this);
        }
    }

    public void ApplyDamage(int aDamage)
    {
        GD.Print("Ow!");
    }

    public class ChangeModifiers
    {
        public float mCooldownReductionAdditive = 0;

        public void Apply(Tower aTower) {
            aTower.cooldownRemaining -= mCooldownReductionAdditive;
        }
    }
}

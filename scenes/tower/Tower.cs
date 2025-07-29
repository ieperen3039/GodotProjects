using Godot;
using System;
using System.Diagnostics;

public partial class Tower : StaticBody2D
{
    [Signal]
    public delegate void OnTowerShootsBoltEventHandler(Bolt aBolt);

    [Export]
    private Marker2D boltFireStartPosition;

    [Export]
    public PackedScene sceneProjectileSpawner;
    public ProjectileSpawner ProjectileSpawner = null;
    private ProjectileElementType boltType; 

    [Export]
    private Node2D directionIndicator;

    private double cooldown = 0.5f;
    private double cooldownRemaining = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(directionIndicator.GlobalPosition == boltFireStartPosition.GlobalPosition);
        boltType = ProjectileElementType.Arcane;
        ProjectileSpawner = sceneProjectileSpawner.Instantiate<ProjectileSpawner>();
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

    private void HandleBoltFire(Vector2 aClickDirection)
    {
        Bolt lBolt = ProjectileSpawner.SpawnBolt(boltType, ProjectileSize.Primary);
        lBolt.Position = boltFireStartPosition.GlobalPosition;
        lBolt.Rotation = aClickDirection.Angle();
        // Velocity magnitude will be overridden
        lBolt.Velocity = aClickDirection;

        EmitSignal(SignalName.OnTowerShootsBolt, lBolt);
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

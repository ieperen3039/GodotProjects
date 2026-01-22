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
    public PackedScene SceneProjectileSpawner;
    public ProjectileSpawner ProjectileSpawner = null;
    private ProjectileElementType boltType; 

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        boltType = ProjectileElementType.Arcane;
        ProjectileSpawner = sceneProjectileSpawner.Instantiate<ProjectileSpawner>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double aDelta)
    {
    }

    private void HandleBoltFire(Vector2 aDirection)
    {
        Bolt lBolt = ProjectileSpawner.SpawnBolt(boltType, ProjectileSize.Primary);
        lBolt.Position = boltFireStartPosition.GlobalPosition;
        lBolt.Rotation = aDirection.Angle();
        // Velocity magnitude will be overridden
        lBolt.Velocity = aDirection;

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
        }
    }
}

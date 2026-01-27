using Godot;
using System;
using System.Diagnostics;

public partial class Tower : StaticBody2D
{
    [Signal]
    public delegate void OnTowerShootsBoltEventHandler(Bolt aBolt);

    private Node2D boltFireStartPosition;

    public ProjectileSpawner ProjectileSpawner = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        boltFireStartPosition = GetNode<Node2D>("BoltFireStartPosition");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double aDelta)
    {
    }

    private void HandleBoltFire(Vector2 aDirection)
    {
        // TODO parse current runes
        Bolt lBolt = ProjectileSpawner.SpawnBolt(aDirection, ProjectileElementType.Arcane);
        lBolt.Position = boltFireStartPosition.GlobalPosition;

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

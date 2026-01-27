using Godot;
using System;

public class ProjectileSpawner
{
    public Random Rng = new();

    private PackedScene arcaneBoltScene;
    private PackedScene fireBoltScene;
    private PackedScene waterBoltScene;

    public ProjectileSpawner()
    {
        arcaneBoltScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/arcane_bolt.tscn");
        fireBoltScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/fire_bolt.tscn");
        waterBoltScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/water_bolt.tscn");
    }

    public Bolt SpawnBolt(Vector2 aDirection, ProjectileElementType type)
    {
        Bolt lBolt = null;
        switch (type)
        {
            case ProjectileElementType.Arcane:
                lBolt = arcaneBoltScene.Instantiate<Bolt>();
                break;
            case ProjectileElementType.Fire:
                lBolt = fireBoltScene.Instantiate<Bolt>();
                break;
            case ProjectileElementType.Water:
                lBolt = waterBoltScene.Instantiate<Bolt>();
                break;
        }

        lBolt.Velocity = aDirection.Normalized();
        lBolt.Rotation = aDirection.Angle();

        return lBolt;
    }
}

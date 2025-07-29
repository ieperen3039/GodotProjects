using Godot;
using System;

public partial class ProjectileSpawner : Node
{
    public Random Rng = new();

    [Export]
    private PackedScene arcaneBoltScene;
    [Export]
    private PackedScene fireBoltScene;
    [Export]
    private PackedScene waterBoltScene;

    public Bolt SpawnBolt(ProjectileElementType type, ProjectileSize size)
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
        lBolt.Size = size;

        return lBolt;
    }
}

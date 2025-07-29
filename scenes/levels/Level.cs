using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class Level : Node2D
{
    [Signal]
    public delegate void OnLevelFinishEventHandler();
    private bool hasFinished = false;

    public SpellBook SpellBook;

    [Export]
    public int EarnedMana = 50;

    [Export]
    private Marker2D FixedSpawn;

    [Export]
    private Line2D[] SpawnAreas;

    [Export]
    private PackedScene[] Enemies;

    [Export]
    private int[] SpawnOrder;

    [Export]
    private Node playFieldNode;
    [Export]
    private Tower tower;
    
    private int enemyIndex = 0;

    private double spawnCooldown = 2;
    private double spawnCooldownRemaining = 3;

    private SpawnLocations spawnLocations;
    
    public ProjectileSpawner ProjectileSpawner => tower.ProjectileSpawner;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(SpellBook != null);
        spawnLocations = new SpawnLocations(SpawnAreas);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        spawnCooldownRemaining -= delta;

        if (spawnCooldownRemaining < 0)
        {
            HandleEnemySpawn();
            spawnCooldownRemaining += spawnCooldown;
        }

        if (IsLevelComplete() && !hasFinished)
        {
            EmitSignal(SignalName.OnLevelFinish);
            hasFinished = true;
        }
    }

    // true if no enemies left
    private bool IsLevelComplete()
    {
        if (enemyIndex < SpawnOrder.Length) return false;

        foreach (Node node in playFieldNode.GetChildren())
        {
            if (node is Enemy) return false;
        }

        return true;
    }


    public void HandleEnemySpawn()
    {
        if (enemyIndex >= SpawnOrder.Length) return;
        
        int indexToSpawn = SpawnOrder[enemyIndex++];
        Enemy enemy = (Enemy) Enemies[indexToSpawn].Instantiate();

        Vector2 spawnPosition = (FixedSpawn != null) ? FixedSpawn.Position : spawnLocations.GenerateSpawnLocation();

        enemy.Position = spawnPosition;
        enemy.MovementTarget = tower.Position;

        playFieldNode.AddChild(enemy);
    }

    private void HandleTowerShootsBolt(Bolt aBolt) => HandleSpawnBolt(aBolt, true);

    private void HandleSpawnBolt(Bolt aBolt, bool aIsPlayer)
    {
        aBolt.OnBoltHitsEnemy += HandleBoltCollision;

        Bolt.SpawnModifiers mods = new();
        foreach (ICardEffect lEffects in SpellBook.Effects)
        {
            lEffects.OnBoltSpawn(aBolt, mods, aIsPlayer);
        }

        mods.Apply(aBolt);
        playFieldNode.AddChild(aBolt);
    }

    private void HandleBoltCollision(Bolt aBolt, Enemy aEnemy)
    {
        Enemy.CollisionModifiers enemyMod = new();
        Bolt.CollisionModifiers boltMod = new();

        enemyMod.DamageAdditive = aBolt.Damage;

        foreach (ICardEffect lEffect in SpellBook.Effects)
        {
            lEffect.OnEnemyBoltCollision(aBolt, aEnemy, boltMod, enemyMod);
        }

        boltMod.Apply(aBolt);
        enemyMod.Apply(aEnemy);

        // After processing the enemy and bolt, we check for new spawns.
        // Only now we know whether we made a kill (or whether the bolt has despawned)
        
        CollisionModifiers levelMod = new();
        foreach (ICardEffect lEffect in SpellBook.Effects)
        {
            lEffect.AfterEnemyBoltCollision(aBolt, aEnemy, levelMod, ProjectileSpawner);
        }
        levelMod.Apply(this);
    }

    public class CollisionModifiers
    {
        public IList<Node2D> NewEntities = new List<Node2D>();

        public void Apply(Level aLevel)
        {
            foreach (Node2D entity in NewEntities)
            {
                if (entity is Bolt bolt)
                {
                    aLevel.HandleSpawnBolt(bolt, false);
                }
                else
                {
                    aLevel.playFieldNode.AddChild(entity);
                }
            }
        }
    }
}

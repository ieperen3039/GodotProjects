using System.Collections.Generic;
using Godot;

public partial class Level : Node2D
{
    [Export]
    private Marker2D FixedSpawn;

    [Export]
    private Line2D[] SpawnAreas;

    [Export]
    private PackedScene[] Enemies;

    [Export]
    private Node playFieldNode;
    [Export]
    private Tower tower;
    
    private int enemiesIndexToSpawn = 0;

    private IList<IEffectSource> effectSources;

    private double spawnCooldown = 2;
    private double spawnCooldownRemaining = 0;

    private SpawnLocations spawnLocations;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        effectSources = new List<IEffectSource>();
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
    }

    public void HandleEnemySpawn()
    {
        if (enemiesIndexToSpawn >= Enemies.Length) return;
        
        Enemy lEnemy = (Enemy) Enemies[enemiesIndexToSpawn++].Instantiate();

        Vector2 lSpawnPosition = (FixedSpawn != null) ? FixedSpawn.Position : spawnLocations.GenerateSpawnLocation();

        lEnemy.Position = lSpawnPosition;
        lEnemy.MovementTarget = tower.Position;

        playFieldNode.AddChild(lEnemy);
    }

    private void HandleTowerShootsBolt(Bolt aBolt) => HandleSpawnBolt(aBolt, true);

    private void HandleSpawnBolt(Bolt aBolt, bool isPlayer)
    {
        aBolt.BoltHitsEnemy += HandleBoltCollision;

        Bolt.SpawnModifiers mods = new();
        foreach (IEffectSource lEffects in effectSources)
        {
            lEffects.OnBoltSpawn(aBolt, mods, isPlayer);
        }

        mods.Apply(aBolt);
        playFieldNode.AddChild(aBolt);
    }

    private void HandleBoltCollision(Bolt aBolt, Enemy aEnemy)
    {
        Enemy.CollisionModifiers enemyMod = new();
        Bolt.CollisionModifiers boltMod = new();

        foreach (IEffectSource lEffect in effectSources)
        {
            lEffect.OnEnemyBoltCollision(aBolt, aEnemy, boltMod, enemyMod);
        }

        boltMod.Apply(aBolt);
        enemyMod.Apply(aEnemy);

        // After processing the enemy and bolt, we check for new spawns.
        // Only now we know whether we made a kill (or whether the bolt has despawned)
        
        CollisionModifiers levelMod = new();
        foreach (IEffectSource lEffect in effectSources)
        {
            lEffect.AfterEnemyBoltCollision(aBolt, aEnemy, levelMod);
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

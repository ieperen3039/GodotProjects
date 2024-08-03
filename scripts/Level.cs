using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class Level : Node2D
{
	[Export]
	public Marker2D FixedSpawn;

	[Export]
	public Line2D[] SpawnAreas;

	[Export]
	public PackedScene[] Enemies;
	
	private int enemiesIndexToSpawn = 0;

	private IList<IEffectSource> effectSources;

	private Random rng = new();

	private double spawnCooldown = 2;
	private double spawnCooldownRemaining = 0;
	private Node playFieldNode;
	private Tower tower;
	private IList<Vector2[]> spawnLines;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playFieldNode = GetNode("PlayField");
		tower = playFieldNode.GetNode<Tower>("Tower");
		effectSources = new List<IEffectSource>();

		spawnLines = new List<Vector2[]>();
		foreach (Line2D lLine in SpawnAreas)
		{
			Vector2[] lLinePoints = lLine.Points;
			for (int i = 0; i < (lLinePoints.Length - 1); i++)
			{
				spawnLines.Add(new Vector2[] { lLinePoints[i], lLinePoints[i + 1] });
			}
		}
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
		if (enemiesIndexToSpawn > Enemies.Length) return;
		
		Enemy lEnemy = (Enemy) Enemies[enemiesIndexToSpawn++].Instantiate();

		Vector2 lSpawnPosition = GenerateSpawnLocation();

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
		CollisionModifiers levelMod = new();

		foreach (IEffectSource lEffect in effectSources)
		{
			lEffect.OnEnemyBoltCollision(aBolt, aEnemy, boltMod, enemyMod, levelMod);
		}

		boltMod.Apply(aBolt);
		enemyMod.Apply(aEnemy);
		levelMod.Apply(this);
	}

	private Vector2 GenerateSpawnLocation()
	{
		if (FixedSpawn != null) return FixedSpawn.Position;

		float lRandomSpot = rng.NextSingle() * spawnLines.Count;
		int lIndex = (int)lRandomSpot;
		float lPositionOnSpot = lRandomSpot - lIndex;

		Vector2[] lTargetLine = spawnLines[lIndex];
		Vector2 lSpawnPosition = lTargetLine[0].Lerp(lTargetLine[1], lPositionOnSpot);
		return lSpawnPosition;
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

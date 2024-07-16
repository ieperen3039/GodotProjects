using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Level : Node2D
{
	[Export]
	public Line2D[] mSpawnAreas;

	private IList<IEffectSource> mModifiers;

	private PackedScene mEnemyBlueprint = GD.Load<PackedScene>("res://scenes/blueprints/enemies/enemy_farmer.tscn");
	private Random mRng = new Random();

	private double mSpawnCooldown = 2;
	private double mSpawnCooldownRemaining = 0;
	private Node mPlayFieldNode;
	private Tower mTower;
	private IList<Vector2[]> mSpawnLines;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mPlayFieldNode = GetNode("PlayField");
		mTower = mPlayFieldNode.GetNode<Tower>("Tower");
		mModifiers = new List<IEffectSource>();
		
		mSpawnLines = new List<Vector2[]>();
		foreach (Line2D lLine in mSpawnAreas)
		{
			Vector2[] lLinePoints = lLine.Points;
			for (int i = 0; i < (lLinePoints.Length - 1); i++)
			{
				mSpawnLines.Add(new Vector2[] { lLinePoints[i], lLinePoints[i + 1] });
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		mSpawnCooldownRemaining -= delta;

		if (mSpawnCooldownRemaining < 0)
		{
			HandleEnemySpawn();
			mSpawnCooldownRemaining += mSpawnCooldown;
		}
	}

	public void HandleEnemySpawn()
	{
		GD.Print("HandleEnemySpawn");

		Enemy lEnemy = mEnemyBlueprint.Instantiate<Enemy>();

		Vector2 lSpawnPosition = GenerateSpawnLocation();

		lEnemy.Position = lSpawnPosition;
		lEnemy.MovementTarget = mTower.Position;

		AddChild(lEnemy);
	}

	private void HandleTowerShootsBolt(Bolt aBolt)
	{
		GD.Print("HandleTowerShootsBolt");
		
		aBolt.BoltHitsEnemy += HandleBoltCollision;

		Bolt.SpawnModifiers lMods = new();
		foreach (IEffectSource lEffects in mModifiers)
		{
			lEffects.OnBoltSpawn(aBolt, lMods);
		}

		lMods.Apply(aBolt);
		mPlayFieldNode.AddChild(aBolt);
	}

	private void HandleBoltCollision(Bolt aBolt, Enemy aEnemy)
	{
		GD.Print("HandleBoltCollision");

		Enemy.CollisionModifiers lMods = new();
		foreach (IEffectSource lMod in mModifiers)
		{
			lMod.OnEnemyBoltCollision(aBolt, aEnemy, lMods);
		}
		
		lMods.Apply(aEnemy);
	}

	private Vector2 GenerateSpawnLocation()
	{
		float lRandomSpot = mRng.NextSingle() * mSpawnLines.Count;
		int lIndex = (int)lRandomSpot;
		float lPositionOnSpot = lRandomSpot - lIndex;

		Vector2[] lTargetLine = mSpawnLines[lIndex];
		Vector2 lSpawnPosition = lTargetLine[0].Lerp(lTargetLine[1], lPositionOnSpot);
		return lSpawnPosition;
	}
}

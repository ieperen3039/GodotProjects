using System;
using System.Collections.Generic;
using Godot;

public class SpawnLocations {
    
	private IList<Vector2[]> spawnLines;

	private Random rng = new();

    public SpawnLocations(in Line2D[] SpawnAreas) 
    {
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

	public Vector2 GenerateSpawnLocation()
	{
		float lRandomSpot = rng.NextSingle() * spawnLines.Count;
		int lIndex = (int)lRandomSpot;
		float lPositionOnSpot = lRandomSpot - lIndex;

		Vector2[] lTargetLine = spawnLines[lIndex];
		Vector2 lSpawnPosition = lTargetLine[0].Lerp(lTargetLine[1], lPositionOnSpot);
		return lSpawnPosition;
	}
}
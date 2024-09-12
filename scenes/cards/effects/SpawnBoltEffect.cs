
using System;

public class SpawnBoltEffect : ICardEffect
{
    private const float OnEnemyDeathManaMultiplier = 5.0f;

    public ProjectileElementType SourceElement;
    public ProjectileSize SourceSize;

    public ProjectileElementType TargetElement;
    public ProjectileSize TargetSize;
    public int TargetQuantity;
    public bool OnlyOnEnemyDeath;

    public string GetCardTitle() => "Multiply";

    public string GetCardText()
    {
        throw new System.NotImplementedException();
    }

    public int GetManaCost()
    {
        throw new System.NotImplementedException();
    }

    public void OnBoltSpawn(in Bolt aBolt, Bolt.SpawnModifiers aMod, bool aPlayerFire)
    { }

    public void OnEnemyBoltCollision(in Bolt aBolt, in Enemy aEnemy, Bolt.CollisionModifiers aBoltMod, Enemy.CollisionModifiers aEnemyMod)
    { }

    public void AfterEnemyBoltCollision(in Bolt aBolt, in Enemy aEnemy, Level.CollisionModifiers aLevelMod)
    {
        if (aBolt.Size != SourceSize || aBolt.Element != SourceElement) return;

        if (OnlyOnEnemyDeath && !aEnemy.IsDead) return;

        for (int i = 0; i < TargetQuantity; i++)
        {
            Bolt newBolt = new()
            {
                Element = TargetElement,
                Size = TargetSize
            };
            aLevelMod.NewEntities.Add(newBolt);
        }
    }

    public static SpawnBoltEffect CreateWithCost(int targetTotalManaCost)
    {
        SpawnBoltEffect effect = new();
        Random rng = new();

        if (rng.Next() % 20 == 0)
        {
            effect.OnlyOnEnemyDeath = true;
            targetTotalManaCost = (int)(targetTotalManaCost * OnEnemyDeathManaMultiplier);
        };

        WeightTable<ProjectileElementType> elementWeights = new WeightTable<ProjectileElementType>()
            .Add(ProjectileElementType.Arcane, 1)
            .Add(ProjectileElementType.Fire, 1)
            .Add(ProjectileElementType.Nature, 1);

        WeightTable<ProjectileSize> sourceSizesWeights = new WeightTable<ProjectileSize>()
            .Add(ProjectileSize.Primary, 100)
            .Add(ProjectileSize.Secondary, 50)
            .Add(ProjectileSize.Tertiary, 1);

        WeightTable<ProjectileSize> targetSizesWeights = new WeightTable<ProjectileSize>()
            .Add(ProjectileSize.Primary, 1)
            .Add(ProjectileSize.Secondary, 50)
            .Add(ProjectileSize.Tertiary, 100);

        effect.SourceElement = elementWeights.Get(rng);
        effect.SourceSize = sourceSizesWeights.Get(rng);

        effect.TargetElement = elementWeights.Get(rng);
        effect.TargetSize = targetSizesWeights.Get(rng);

        return effect;
    }
}
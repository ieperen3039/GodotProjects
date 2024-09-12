
using System;
using System.Linq;

public class BoltStatEffect : ICardEffect
{
    private const float SpeedMultiplicativeCost = 10;
    private const float SpeedMultiplicativeMax = 4.0f;
    private const float SpeedMultiplicativeStep = 0.5f;
    private const float DamageAdditiveCost = 1;
    private const float DamageAdditiveMax = 20;
    private const float DamageAdditiveStep = 1;
    private const float DamageMultiplicativeCost = 20;
    private const float DamageMultiplicativeMax = 2;
    private const float DamageMultiplicativeStep = 0.2f;
    private const float HomingDegPerSecondCost = 0.2f;
    private const float HomingDegPerSecondMax = 180;
    private const float HomingDegPerSecondStep = 15;
    private const float OnPlayerFireManaMultiplier = 2f;

    private enum Stat { SpeedMultiplicative, DamageAdditive, DamageMultiplicative, HomingDegPerSecond }

    public bool OnlyOnPlayerFire = false;
    public float SpeedAdditive = 0;
    public float SpeedMultiplicative = 1;
    public int DamageAdditive = 0;
    public float DamageMultiplicative = 1;
    public float HomingDegPerSecond = 0;

    public string GetCardTitle() => "Enhance Bolt";

    public int GetManaCost()
    {
        float baseCost = (SpeedMultiplicativeCost * (SpeedMultiplicative - 1))
            + (DamageAdditiveCost * DamageAdditive)
            + (DamageMultiplicativeCost * (DamageMultiplicative - 1))
            + (HomingDegPerSecondCost * HomingDegPerSecond);

        if (OnlyOnPlayerFire) baseCost /= OnPlayerFireManaMultiplier;

        return (int)baseCost;
    }

    public string GetCardText()
    {
        string text = (OnlyOnPlayerFire ? "Player" : "All") + " bolts have:";
        if (SpeedAdditive != 0) text += $"\n+{SpeedAdditive} projectile speed";
        if (SpeedMultiplicative != 1) text += $"\n{SpeedMultiplicative}x projectile speed";
        if (DamageAdditive != 0) text += $"\n+{DamageAdditive} projectile damage";
        if (DamageMultiplicative != 1) text += $"\n{DamageMultiplicative}x projectile damage";

        if (HomingDegPerSecond > 180)
        {
            text += "\nsuperb homing";
        }
        else if (HomingDegPerSecond > 90)
        {
            text += "\nmajor homing";
        }
        else if (HomingDegPerSecond > 45)
        {
            text += "\nmedium homing";
        }
        else if (HomingDegPerSecond > 0)
        {
            text += "\nminor homing";
        }

        return text;
    }

    public void OnBoltSpawn(in Bolt aBolt, Bolt.SpawnModifiers aMod, bool aPlayerFire)
    {
        if (OnlyOnPlayerFire && !aPlayerFire) return;

        aMod.SpeedAdditive += SpeedAdditive;
        aMod.SpeedMultiplicative *= SpeedMultiplicative;
        aMod.DamageAdditive += DamageAdditive;
        aMod.DamageMultiplicative *= DamageMultiplicative;
        aMod.HomingDegPerSecond += HomingDegPerSecond;
    }

    public void OnEnemyBoltCollision(
        in Bolt aBolt, in Enemy aEnemy, Bolt.CollisionModifiers aBoltMod, Enemy.CollisionModifiers aEnemyMod
    )
    { }

    public void AfterEnemyBoltCollision(in Bolt aBolt, in Enemy aEnemy, Level.CollisionModifiers aLevelMod)
    { }

    public static BoltStatEffect CreateWithCost(int targetTotalManaCost)
    {
        BoltStatEffect effect = new();
        Random rng = new();

        if (rng.Next() % 20 == 0)
        {
            effect.OnlyOnPlayerFire = true;
            targetTotalManaCost = (int)(targetTotalManaCost * OnPlayerFireManaMultiplier);
        };

        WeightTable<Stat> effectWeightsTable = new WeightTable<Stat>()
            .Add(Stat.SpeedMultiplicative, 30)
            .Add(Stat.DamageAdditive, 30)
            .Add(Stat.DamageMultiplicative, 10)
            .Add(Stat.HomingDegPerSecond, 10);

        const int NumDraws = 3;
        for (int drawIdx = 0; drawIdx < NumDraws; drawIdx++)
        {
            BoltStatEffect newEffect = (BoltStatEffect)effect.MemberwiseClone();

            float effectStrength;
            switch (effectWeightsTable.Get(rng))
            {
                case Stat.SpeedMultiplicative:
                    effectStrength = RandomWithStep(rng, (int)(SpeedMultiplicativeMax / SpeedMultiplicativeStep));
                    newEffect.SpeedMultiplicative += effectStrength * SpeedMultiplicativeMax;
                    break;
                case Stat.DamageAdditive:
                    effectStrength = RandomWithStep(rng, (int)(DamageAdditiveMax / DamageAdditiveStep));
                    newEffect.DamageAdditive += (int)(effectStrength * DamageAdditiveMax);
                    break;
                case Stat.DamageMultiplicative:
                    effectStrength = RandomWithStep(rng, (int)(DamageMultiplicativeMax / DamageMultiplicativeStep));
                    newEffect.DamageMultiplicative += effectStrength * DamageMultiplicativeMax;
                    break;
                case Stat.HomingDegPerSecond:
                    effectStrength = RandomWithStep(rng, (int)(HomingDegPerSecondMax / HomingDegPerSecondStep));
                    newEffect.HomingDegPerSecond += effectStrength * HomingDegPerSecondMax;
                    break;
            }

            if (newEffect.GetManaCost() <= targetTotalManaCost)
            {
                effect = newEffect;
            }
        }

        return effect;
    }

    private static float RandomWithStep(Random rng, int step)
    {
        return ((rng.Next() % step) + 1.0f) / step;
    }

}

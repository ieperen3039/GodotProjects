
using System;
using System.Linq;

public partial class BoltStatEffect : ICardEffect
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

    public bool OnlyOnPlayerFire = false;
    public float SpeedAdditive = 0;
    public float SpeedMultiplicative = 1;
    public int DamageAdditive = 0;
    public float DamageMultiplicative = 1;
    public float HomingDegPerSecond = 0;

    public string GetCardTitle()
    {
        return "Enhance Bolt";
    }

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

        int[] effectWeights = {
            0,  // unused
            0,  // unused
            30, // 2: SpeedMultiplicative
            30, // 3: DamageAdditive
            10, // 4: DamageMultiplicative
            10, // 5: HomingDegPerSecond
        };

        const int NumDraws = 3;
        for (int drawIdx = 0; drawIdx < NumDraws; drawIdx++)
        {
            int effectIndex = -1;
            int number = rng.Next() % effectWeights.Sum();
            while (number >= 0) number -= effectWeights[++effectIndex];

            BoltStatEffect newEffect = (BoltStatEffect)effect.MemberwiseClone();

            float effectStrength;
            switch (effectIndex)
            {
                case 0:
                    // unused
                    break;
                case 1:
                    // unused
                    break;
                case 2:
                    effectStrength = RandomWithStep(rng, (int)(SpeedMultiplicativeMax / SpeedMultiplicativeStep));
                    newEffect.SpeedMultiplicative += effectStrength * SpeedMultiplicativeMax;
                    break;
                case 3:
                    effectStrength = RandomWithStep(rng, (int)(DamageAdditiveMax / DamageAdditiveStep));
                    newEffect.DamageAdditive += (int)(effectStrength * DamageAdditiveMax);
                    break;
                case 4:
                    effectStrength = RandomWithStep(rng, (int)(DamageMultiplicativeMax / DamageMultiplicativeStep));
                    newEffect.DamageMultiplicative += effectStrength * DamageMultiplicativeMax;
                    break;
                case 5:
                    effectStrength = RandomWithStep(rng, (int)(HomingDegPerSecondMax / HomingDegPerSecondStep));
                    newEffect.HomingDegPerSecond += effectStrength * HomingDegPerSecondMax;
                    break;
                default:
                    throw new Exception("Table index out of bounds");
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

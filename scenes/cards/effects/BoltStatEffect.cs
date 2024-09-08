
using System;
using System.Linq;

public partial class BoltStatEffect : ICardEffect
{
    private const float SpeedMultiplicativeCost = 10;
    private const float DamageAdditiveCost = 1;
    private const float DamageMultiplicativeCost = 20;
    private const float HomingDegPerSecondCost = 0.2f;
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
            int manaLeftToUse = targetTotalManaCost - effect.GetManaCost();
            if (manaLeftToUse <= 0) break;

            float fractionToUse = 1.0f / (NumDraws - drawIdx);

            float offset = (rng.NextSingle() * 3.0f) + 1f;
            int manaToUse = (int)(manaLeftToUse * fractionToUse * offset);

            int effectIndex = -1;
            int number = rng.Next() % effectWeights.Sum();
            while (number >= 0) number -= effectWeights[++effectIndex];

            BoltStatEffect newEffect = (BoltStatEffect)effect.MemberwiseClone();

            switch (effectIndex)
            {
                case 0:
                    // unused
                    break;
                case 1:
                    // unused
                    break;
                case 2:
                    newEffect.SpeedMultiplicative += manaToUse / SpeedMultiplicativeCost;
                    break;
                case 3:
                    newEffect.DamageAdditive += (int)(manaToUse / DamageAdditiveCost);
                    break;
                case 4:
                    newEffect.DamageMultiplicative += manaToUse / DamageMultiplicativeCost;
                    break;
                case 5:
                    newEffect.HomingDegPerSecond += manaToUse / HomingDegPerSecondCost;
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
}

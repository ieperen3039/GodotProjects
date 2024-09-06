
public partial class BoltStatEffect : ICardEffect
{
    public bool OnlyOnPlayerFire = false;
    public float SpeedAdditive = 0;
    public float SpeedMultiplicative = 1;
    public int DamageAdditive = 0;
    public float DamageMultiplicative = 1;
    public float HomingDegPerSecond = 0;

    public string GetCardTitle() 
    {
        return IsPointless() ? "Failure" : "Enhance Bolt";
    }

    public bool IsPointless()
    {
        return SpeedAdditive == 0 && SpeedMultiplicative == 1 && DamageAdditive == 0 && DamageMultiplicative == 1 && HomingDegPerSecond == 0;
    }

    public string GetCardText()
    {
        if (IsPointless())
        {
            return "This card has no effect";
        }

        string text = (OnlyOnPlayerFire ? "Player" : "All") + " bolts have:";
        if (SpeedAdditive != 0) text += string.Format("\n+{0:F0} projectile speed", SpeedAdditive);
        if (SpeedMultiplicative != 1) text += string.Format("\n{0:F2}x projectile speed", SpeedMultiplicative);
        if (DamageAdditive != 0) text += string.Format("\n+{0:D} projectile damage", DamageAdditive);
        if (DamageMultiplicative != 1) text += string.Format("\n{0:F2}x projectile damage", DamageMultiplicative);

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
}

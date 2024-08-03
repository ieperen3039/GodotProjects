

public interface IEffectSource
{
    void OnBoltSpawn(in Bolt aBolt, Bolt.SpawnModifiers aMod, bool aPlayerFire);

    void OnEnemyBoltCollision(in Bolt aBolt, in Enemy aEnemy, Bolt.CollisionModifiers aBoltMod, Enemy.CollisionModifiers aEnemyMod, Level.CollisionModifiers aLevelMod);
}
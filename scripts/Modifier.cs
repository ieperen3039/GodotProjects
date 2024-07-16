

public interface IEffectSource {

    void OnBoltSpawn(in Bolt aBolt, Bolt.SpawnModifiers aMod);

    void OnEnemyBoltCollision(in Bolt aBolt, in Enemy aEnemy, Enemy.CollisionModifiers aMod);
}
using Godot;
using System;
using System.Diagnostics;

public partial class Enemy : CharacterBody2D
{
    [Export]
    public int MaxHitpoints { get; private set; } = 1;

    [Export]
    public float Speed = 1.0f;

    [Export]
    public int DamagePerAttack = 1;

    [Export]
    public float AttacksPerSecond = 1.0f;

    public bool IsDead => state is Dieing;

    public Vector2 MovementTarget;

    private AnimationPlayer animationPlayer;
    private Healthbar healthbar;
    private int currentHitpoints;
    private EnemyState state;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        healthbar = GetNode<Healthbar>("Healthbar");
        currentHitpoints = MaxHitpoints;

        state = new Walking(animationPlayer);
    }

    public void ApplyDamage(int aTotalDamage)
    {
        SetHealth(currentHitpoints - aTotalDamage);
    }

    public override void _PhysicsProcess(double aDelta)
    {
        state.Process(this, aDelta);
        healthbar.GlobalRotation = 0;
    }

    public void HandleEnterTowerArea(Tower aTower)
    {
        if (IsDead) return;

        state = new Attacking(animationPlayer, aTower, 1.0f / AttacksPerSecond);
    }

    void SetHealth(int aNewHitpoints)
    {
        if (aNewHitpoints < 0) aNewHitpoints = 0;

        currentHitpoints = aNewHitpoints;
        healthbar.SetHealth((float)currentHitpoints / MaxHitpoints);

        if (currentHitpoints == 0)
        {
            state = new Dieing(animationPlayer, healthbar);
            // disable collisions
            CollisionShape2D collider = GetNode<CollisionShape2D>("CollisionShape2D");
            collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
    }

    private interface EnemyState
    {
        void Process(Enemy aThis, double aDelta);
    }

    private class Walking : EnemyState
    {
        public Walking(AnimationPlayer animationPlayer)
        {
            animationPlayer.Play("walk");
        }

        public void Process(Enemy aThis, double aDelta)
        {
            if (aThis.Speed > 0)
            {
                aThis.Velocity = (aThis.MovementTarget - aThis.Position).LimitLength(aThis.Speed);
                aThis.MoveAndCollide(aThis.Velocity);
            }

            if (aThis.Velocity.LengthSquared() > 0)
            {
                aThis.Rotation = aThis.Velocity.Angle();
            }
        }
    }

    private class Attacking : EnemyState
    {
        private float cooldown;
        private float cooldownRemaining;
        public Tower target;

        public Attacking(AnimationPlayer animationPlayer, Tower aTarget, float aCooldown)
        {
            target = aTarget;
            cooldownRemaining = aCooldown * 0.5f;
            cooldown = aCooldown;
            animationPlayer.Play("attack", -1, 1.0f / aCooldown);
        }

        public void Process(Enemy aThis, double aDelta)
        {
            cooldownRemaining -= (float) aDelta;

            if (cooldownRemaining <= 0)
            {
                target.ApplyDamage(aThis.DamagePerAttack);
                cooldownRemaining += cooldown;
            }
        }
    }

    private class Dieing : EnemyState
    {
        public Dieing(AnimationPlayer animationPlayer, Healthbar healthbar)
        {
            animationPlayer.Play("die");
            healthbar.Fading = true;
        }

        public void Process(Enemy aThis, double aDelta) { }
    }

    public class CollisionModifiers
    {
        public const float BaseDamage = 1.0f;
        public float DamageAdditive = BaseDamage;
        public float DamageMultiplicative = 1;

        internal void Apply(Enemy aEnemy)
        {
            int lTotalDamage = (int)(DamageAdditive * DamageMultiplicative);
            aEnemy.ApplyDamage(lTotalDamage);
        }
    }
}

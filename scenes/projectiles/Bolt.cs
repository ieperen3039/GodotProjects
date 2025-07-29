using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Bolt : CharacterBody2D
{
    private const int MaxCollisionRecursionDepth = 10;

    [Signal]
    public delegate void OnBoltHitsEnemyEventHandler(Bolt aBolt, Enemy aTarget);

    [Export]
    public ulong TailElementDelayMs;

    public bool IsDead => deathTime < ulong.MaxValue;

    [Export]
    public ProjectileElementType Element { get; private set; }
    public ProjectileSize Size;
    public Enemy EnemyToIgnore = null;

    private Node2D appearance;
    private Area2D enemySearchArea;

    private Queue<Tuple<ulong, Vector2>> positions;
    private ulong maxTimeForPositions;

    public int Damage;
    private float rotationRadPerSecond;
    private ulong deathTime = ulong.MaxValue;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        appearance = GetNode<Node2D>("Appearance");
        enemySearchArea = GetNode<Area2D>("EnemySearchArea");

        positions = new Queue<Tuple<ulong, Vector2>>();
        RecordPosition(Position);
        maxTimeForPositions = TailElementDelayMs * (ulong)(appearance.GetChildren().Count + 1);

        // child nodes are spread for editor visuals. We reset the positions here
        foreach (Node child in appearance.GetChildren())
        {
            if (child is Node2D tailSprite)
            {
                tailSprite.Position = Vector2.Zero;
            }
        }
    }

    void RecordPosition(Vector2 aPosition)
    {
        ulong now = Time.GetTicksMsec();
        positions.Enqueue(Tuple.Create(now, aPosition));

        while (positions.First().Item1 < now - maxTimeForPositions)
        {
            positions.Dequeue();
        }
    }

    Vector2 GetPosition(ulong aTime)
    {
        Tuple<ulong, Vector2> lastPair = positions.First();

        if (aTime <= lastPair.Item1 || positions.Count == 1)
        {
            return lastPair.Item2;
        }

        foreach (Tuple<ulong, Vector2> pair in positions)
        {
            if (pair.Item1 == aTime)
            {
                return pair.Item2;
            }
            else if (pair.Item1 > aTime)
            {

                float fraction = (float)(aTime - lastPair.Item1) / (pair.Item1 - lastPair.Item1);
                return lastPair.Item2.Lerp(pair.Item2, fraction);
            }
            else
            {
                lastPair = pair;
            }

        }

        return lastPair.Item2;
    }

    public override void _Process(double delta)
    {
        ulong now = Time.GetTicksMsec();
        ulong offset = 0;

        foreach (Node child in appearance.GetChildren())
        {
            if (child is Node2D tailSprite)
            {
                ulong visualizedTime = now - offset;
                if (visualizedTime > deathTime)
                {
                    child.QueueFree();
                    continue;
                }

                Vector2 tailGlobalPosition = GetPosition(visualizedTime);
                tailSprite.GlobalPosition = tailGlobalPosition;
                offset += TailElementDelayMs;
            }
        }
    }

    public override void _PhysicsProcess(double aDelta)
    {
        if (IsDead) return;
        RecursiveCollide(Velocity * (float)aDelta, MaxCollisionRecursionDepth);

        if (rotationRadPerSecond > 0)
        {
            Vector2 smallestRelativePosition = Vector2.Inf;
            foreach (Node2D body in enemySearchArea.GetOverlappingBodies())
            {
                if (body is Enemy enemy && enemy != EnemyToIgnore)
                {
                    Vector2 relativePosition = body.Position - Position;
                    if (relativePosition.LengthSquared() < smallestRelativePosition.LengthSquared())
                    {
                        smallestRelativePosition = relativePosition;
                    }
                }
            }

            if (smallestRelativePosition != Vector2.Inf)
            {
                Velocity = HomeTowards(smallestRelativePosition, (float)aDelta);
            }
        }

        RecordPosition(Position);
        Rotation = Velocity.Angle();
    }

    private void RecursiveCollide(Vector2 aMovement, int aRemainingCollisions)
    {
        KinematicCollision2D lCollision = MoveAndCollide(aMovement, true);
        if (lCollision != null)
        {
            if (aRemainingCollisions == 0)
            {
                // too many collisions in a single frame == remove the projectile
                Velocity = Vector2.Zero;
                MyDespawn();
                return;
            }

            GodotObject lCollidedObject = lCollision.GetCollider();

            if (lCollidedObject is Enemy lEnemy)
            {
                if (lEnemy != EnemyToIgnore)
                {
                    Position = lCollision.GetPosition();
                    EmitSignal(SignalName.OnBoltHitsEnemy, this, lEnemy);
                }
                else
                {
                    // ignore the collision
                    Position += aMovement;
                }
            }
            else
            {
                // bounce
                Position = lCollision.GetPosition();
                Velocity.Bounce(lCollision.GetNormal());
                Vector2 lRemainder = lCollision.GetRemainder().Bounce(lCollision.GetNormal());

                GD.Print("Collided with non-enemy " + lCollidedObject);
                RecursiveCollide(lRemainder, aRemainingCollisions - 1);
            }
        }
        else
        {
            Position += aMovement;
        }
    }


    private Vector2 HomeTowards(Vector2 relativePosition, float aDelta)
    {
        float angleRad = Velocity.AngleTo(relativePosition);
        if (Math.Abs(angleRad) < (1f / 1024f))
        {
            return Velocity;
        }

        float maxRotationRad = rotationRadPerSecond * aDelta;
        if (Math.Abs(angleRad) < maxRotationRad)
        {
            return relativePosition.Normalized() * Velocity.Length();
        }
        else if (angleRad > 0)
        {
            return Velocity.Rotated(maxRotationRad);
        }
        else
        {
            return Velocity.Rotated(-maxRotationRad);
        }
    }

    public void MyDespawn()
    {
        deathTime = Time.GetTicksMsec();
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(appearance, "modulate", Colors.Transparent, 0.5f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    public class SpawnModifiers
    {
        public const float BaseBoltSpeed = 100.0f;
        public float BaseDamage;
        public float SpeedAdditive = BaseBoltSpeed;
        public float SpeedMultiplicative = 1;
        public float DamageMultiplicative = 1;
        public float HomingDegPerSecond = 10;

        public SpawnModifiers(Bolt aTarget)
        {
            switch (aTarget.Size)
            {
                case ProjectileSize.Primary:
                    BaseDamage = 5;
                    break;
                case ProjectileSize.Secondary:
                    BaseDamage = 3;
                    break;
                case ProjectileSize.Tertiary:
                    BaseDamage = 2;
                    break;
            }


        }


        public void Apply(Bolt aTarget)
        {
            float lBoltSpeed = SpeedAdditive * SpeedMultiplicative;
            aTarget.Velocity = aTarget.Velocity.Normalized() * lBoltSpeed;
            aTarget.Rotation = aTarget.Velocity.Angle();
            aTarget.Damage = (int)(BaseDamage * DamageMultiplicative);

            aTarget.rotationRadPerSecond = Mathf.DegToRad(HomingDegPerSecond);
        }
    }

    public class CollisionModifiers
    {
        public bool DoDespawn = true;

        public void Apply(Bolt aTarget)
        {
            if (DoDespawn)
            {
                aTarget.MyDespawn();
            }
        }
    }

}

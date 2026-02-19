using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Danmaku;

public class Enemy
{
    public Texture2D Texture;
    public Vector2 Position;
    public Vector2 Velocity;
    public bool IsAlive;
    public int Health;
    public enum PatternType { None, Aim, Ring, Spiral, Wave }

    public PatternType Pattern = PatternType.None;
    public float FireInterval = 1.0f;
    private float _fireTimer = 0f;
    private float _patternAngle = 0f; // stateful for spiral

    // references injected by factory
    public BulletFactory BulletFactory;
    public Player PlayerRef;

    public void Initialize(Texture2D texture, Vector2 position, Vector2 velocity, int health = 1)
    {
        Texture = texture;
        Position = position;
        Velocity = velocity;
        Health = health;
        IsAlive = true;
    }

    public void Draw(SpriteBatch sb)
    {
        if (!IsAlive || Texture == null) return;
        var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        sb.Draw(Texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    public bool Damage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            IsAlive = false;
            return true;
        }
        return false;
    }

    public void Update(GameTime gt)
    {
        if (!IsAlive) return;
        var dt = (float)gt.ElapsedGameTime.TotalSeconds;
        Position += Velocity * dt;

        // firing logic
        _fireTimer -= dt;
        if (_fireTimer <= 0f && BulletFactory != null)
        {
            FirePattern();
            _fireTimer = FireInterval;
        }
    }

    private void FirePattern()
    {
        if (BulletFactory == null) return;

        switch (Pattern)
        {
            case PatternType.Aim:
                if (PlayerRef == null) break;
                var dir = PlayerRef.Position - Position;
                if (dir != Vector2.Zero) dir.Normalize();
                BulletFactory.Spawn(Position, dir, 0f, 180f, 4f, true, 1);
                break;
            case PatternType.Ring:
                int n = 12;
                for (int i = 0; i < n; i++)
                {
                    var angle = (float)(i * Math.PI * 2 / n);
                    var v = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    BulletFactory.Spawn(Position, v, angle, 120f, 4f, true, 1);
                }
                break;
            case PatternType.Spiral:
                // spawn a single bullet advancing the spiral angle
                var v2 = new Vector2((float)Math.Cos(_patternAngle), (float)Math.Sin(_patternAngle));
                BulletFactory.Spawn(Position, v2, _patternAngle, 140f, 4f, true, 1);
                _patternAngle += 0.3f; // step the spiral
                break;
            case PatternType.Wave:
                // create a small fan
                int m = 5;
                var baseAngle = (float)(-Math.PI/2);
                for (int i = 0; i < m; i++)
                {
                    var angle = baseAngle + (i - m/2) * 0.15f;
                    var v3 = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    BulletFactory.Spawn(Position, v3, angle, 160f, 3f, true, 1);
                }
                break;
            case PatternType.None:
            default:
                break;
        }
    }

    public void Reset()
    {
        IsAlive = false;
        Texture = null;
    }
}

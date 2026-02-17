using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Danmaku;

public class Bullet
{
    public Texture2D Texture;
    public Vector2 Position;
    public Vector2 Velocity;
    public bool IsAlive;
    public float Rotation;
    public float Speed;
    public float Radius = 4f;
    public bool IsEnemyBullet = false;
    public int Damage = 1;

    public void Initialize(Texture2D texture, Vector2 position, Vector2 velocity, float rotation = 0f, float speed = 1f, float radius = 4f, bool isEnemyBullet = false, int damage = 1)
    {
        Texture = texture;
        Position = position;
        Velocity = velocity * speed;
        Rotation = rotation;
        Speed = speed;
        Radius = radius;
        IsEnemyBullet = isEnemyBullet;
        Damage = damage;
        IsAlive = true;
    }

    public void Update(GameTime gt)
    {
        if (!IsAlive) return;
        Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;
        if (Position.X < -64 || Position.X > 8192 || Position.Y < -64 || Position.Y > 8192) IsAlive = false;
    }

    public void Draw(SpriteBatch sb)
    {
        if (!IsAlive || Texture == null) return;
        var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        sb.Draw(Texture, Position, null, Color.White, Rotation, origin, 1f, SpriteEffects.None, 0f);
    }

    public void Reset()
    {
        IsAlive = false;
        Texture = null;
    }
}

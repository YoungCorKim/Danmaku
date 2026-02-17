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

    public void Initialize(Texture2D texture, Vector2 position, Vector2 velocity, int health = 1)
    {
        Texture = texture;
        Position = position;
        Velocity = velocity;
        Health = health;
        IsAlive = true;
    }

    public void Update(GameTime gt)
    {
        if (!IsAlive) return;
        Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;
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

    public void Reset()
    {
        IsAlive = false;
        Texture = null;
    }
}

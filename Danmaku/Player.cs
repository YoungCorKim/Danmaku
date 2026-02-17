using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Danmaku;

public class Player
{
    public Texture2D Texture;
    public Vector2 Position;
    public float NormalSpeed = 300f;
    public float SlowSpeed = 120f;
    public float HitboxRadius = 4f;
    public int Lives = 3;
    private Vector2 _startPosition;

    public Player(Texture2D texture, Vector2 startPosition)
    {
        Texture = texture;
        Position = startPosition;
        _startPosition = startPosition;
    }

    public void Update(GameTime gt, KeyboardState ks, Viewport viewport)
    {
        var dt = (float)gt.ElapsedGameTime.TotalSeconds;
        var speed = (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift)) ? SlowSpeed : NormalSpeed;

        Vector2 move = Vector2.Zero;
        if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up)) move.Y -= 1;
        if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down)) move.Y += 1;
        if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left)) move.X -= 1;
        if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right)) move.X += 1;

        if (move != Vector2.Zero)
        {
            move.Normalize();
            Position += move * speed * dt;
        }

        // confine to viewport with a small margin
        var margin = 8;
        Position.X = MathHelper.Clamp(Position.X, margin, viewport.Width - margin);
        Position.Y = MathHelper.Clamp(Position.Y, margin, viewport.Height - margin);
    }

    public void Draw(SpriteBatch sb)
    {
        if (Texture == null) return;
        var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        sb.Draw(Texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    public void Damage(int amount)
    {
        Lives -= amount;
        if (Lives < 0) Lives = 0;
        // simple reset to start position on damage
        Position = _startPosition;
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Danmaku;

public class BulletFactory
{
    private readonly Bullet[] _pool;
    private readonly Stack<int> _freeIndices;
    public Texture2D BulletTexture;

    public BulletFactory(int capacity)
    {
        _pool = new Bullet[capacity];
        _freeIndices = new Stack<int>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            _pool[i] = new Bullet();
            _freeIndices.Push(i);
        }
    }

    public Bullet Spawn(Vector2 position, Vector2 velocity, float rotation = 0f, float speed = 1f, float radius = 4f, bool isEnemyBullet = false, int damage = 1)
    {
        if (_freeIndices.Count == 0) return null;
        int idx = _freeIndices.Pop();
        var b = _pool[idx];
        b.Initialize(BulletTexture, position, velocity, rotation, speed, radius, isEnemyBullet, damage);
        return b;
    }

    public void UpdateAll(GameTime gt)
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            var b = _pool[i];
            if (b.IsAlive)
            {
                b.Update(gt);
                if (!b.IsAlive)
                {
                    b.Reset();
                    _freeIndices.Push(i);
                }
            }
        }
    }

    public void DrawAll(SpriteBatch sb)
    {
        for (int i = 0; i < _pool.Length; i++)
            if (_pool[i].IsAlive)
                _pool[i].Draw(sb);
    }

    public IEnumerable<Bullet> LiveBullets()
    {
        foreach (var b in _pool)
            if (b.IsAlive) yield return b;
    }
}

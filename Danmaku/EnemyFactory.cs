using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Danmaku;

public class EnemyFactory
{
    private readonly Enemy[] _pool;
    private readonly Stack<int> _freeIndices;
    public Texture2D EnemyTexture;
    public BulletFactory BulletFactory;
    public Player PlayerRef;

    public EnemyFactory(int capacity)
    {
        _pool = new Enemy[capacity];
        _freeIndices = new Stack<int>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            _pool[i] = new Enemy();
            _freeIndices.Push(i);
        }
    }

    public Enemy Spawn(Vector2 position, Vector2 velocity, int health = 1, Enemy.PatternType pattern = Enemy.PatternType.None, float fireInterval = 1f)
    {
        if (_freeIndices.Count == 0) return null;
        var idx = _freeIndices.Pop();
        var e = _pool[idx];
        e.Initialize(EnemyTexture, position, velocity, health);
        e.BulletFactory = BulletFactory;
        e.PlayerRef = PlayerRef;
        e.Pattern = pattern;
        e.FireInterval = fireInterval;
        return e;
    }

    public void UpdateAll(GameTime gt)
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            var e = _pool[i];
            if (e.IsAlive)
            {
                e.Update(gt);
                if (!e.IsAlive)
                {
                    e.Reset();
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

    public IEnumerable<Enemy> LiveEnemies()
    {
        foreach (var e in _pool)
            if (e.IsAlive) yield return e;
    }
}

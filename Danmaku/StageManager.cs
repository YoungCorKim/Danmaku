using Microsoft.Xna.Framework;
using System;

namespace Danmaku;

public class StageManager
{
    public enum Stage
    {
        Regular1,
        MidBoss,
        Regular2,
        FinalBoss,
        Finished
    }

    private readonly EnemyFactory _factoryA;
    private readonly EnemyFactory _factoryB;
    private readonly EnemyFactory _midFactory;
    private readonly EnemyFactory _finalFactory;
    private readonly Game _game;

    private Stage _current = Stage.Regular1;
    private float _stageTimer = 0f;
    private float _spawnTimer = 0f;

    // durations in seconds
    private readonly float _regularDuration = 25f;
    private readonly float _midBossDuration = 12f;
    private readonly float _regular2Duration = 25f;
    private readonly float _finalBossDuration = 30f;

    public string CurrentStageName => _current.ToString();

    public StageManager(Game game, EnemyFactory factoryA, EnemyFactory factoryB, EnemyFactory midFactory, EnemyFactory finalFactory)
    {
        _game = game;
        _factoryA = factoryA;
        _factoryB = factoryB;
        _midFactory = midFactory;
        _finalFactory = finalFactory;
        _stageTimer = 0f;
        _spawnTimer = 0f;
    }

    public void Update(GameTime gt)
    {
        var dt = (float)gt.ElapsedGameTime.TotalSeconds;
        _stageTimer += dt;
        _spawnTimer += dt;

        switch (_current)
        {
            case Stage.Regular1:
                RunWave(_factoryA, _factoryB, dt);
                if (_stageTimer >= _regularDuration) Transition(Stage.MidBoss);
                break;
            case Stage.MidBoss:
                if (_spawnTimer >= 0.5f)
                {
                    // spawn a single mid boss near top center
                    var vp = ((Game1)_game).GraphicsDevice.Viewport;
                    var pos = new Microsoft.Xna.Framework.Vector2(vp.Width / 2f, 120f);
                    _midFactory.Spawn(pos, Vector2.Zero, 10);
                    _spawnTimer = 0f;
                }
                if (_stageTimer >= _midBossDuration) Transition(Stage.Regular2);
                break;
            case Stage.Regular2:
                RunWave(_factoryA, _factoryB, dt);
                if (_stageTimer >= _regular2Duration) Transition(Stage.FinalBoss);
                break;
            case Stage.FinalBoss:
                if (_spawnTimer >= 0.75f)
                {
                    var vp = ((Game1)_game).GraphicsDevice.Viewport;
                    var pos = new Microsoft.Xna.Framework.Vector2(vp.Width / 2f, 100f);
                    // spawn final boss with higher health
                    _finalFactory.Spawn(pos, Vector2.Zero, 50);
                    _spawnTimer = 0f;
                }
                if (_stageTimer >= _finalBossDuration) Transition(Stage.Finished);
                break;
            case Stage.Finished:
                // nothing for now
                break;
        }
    }

    private void RunWave(EnemyFactory a, EnemyFactory b, float dt)
    {
        // spawn small grunts periodically
        if (_spawnTimer >= 0.6f)
        {
            var vp = ((Game1)_game).GraphicsDevice.Viewport;
            var x = (float)(50 + (new Random()).NextDouble() * (vp.Width - 100));
            var pos = new Vector2(x, -20f);
            a.Spawn(pos, new Vector2(0, 40f), 1);

            // occasionally spawn B-type moving diagonally
            if (new Random().NextDouble() < 0.35)
            {
                var x2 = (float)(50 + (new Random()).NextDouble() * (vp.Width - 100));
                var dir = new Vector2((float)(new Random().NextDouble() * 2 - 1), 0.5f);
                dir.Normalize();
                b.Spawn(new Vector2(x2, -40f), dir * 80f, 1);
            }

            _spawnTimer = 0f;
        }
    }

    private void Transition(Stage next)
    {
        _current = next;
        _stageTimer = 0f;
        _spawnTimer = 0f;
    }
}

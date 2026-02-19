using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Danmaku;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // Game objects
    private BulletFactory _bulletFactory;
    private EnemyFactory _enemyFactoryA;
    private EnemyFactory _enemyFactoryB;
    private EnemyFactory _midBossFactory;
    private EnemyFactory _finalBossFactory;
    private Player _player;
    private Texture2D _playerTexture;
    private Texture2D _bulletTexture;
    private Texture2D _enemyATexture;
    private Texture2D _enemyBTexture;
    private Texture2D _midBossTexture;
    private Texture2D _finalBossTexture;
    private SpriteFont _font;
    private StageManager _stageManager;
    private int _score = 0;
    private Texture2D _whitePixel;
    
    // simple shooting cooldown
    private float _playerShootTimer = 0f;
    private float _playerShootInterval = 0.12f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        // factories will be created in LoadContent after textures are loaded
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // Load textures, fall back to generated placeholders if content is missing
        // Player placeholder uses a distinct color/size so it's visible against the background
        _playerTexture = TryLoadTexture("player", Color.White, 48);
        _bulletTexture = TryLoadTexture("bullet", Color.Yellow);
        _enemyATexture = TryLoadTexture("enemyA", Color.Red);
        _enemyBTexture = TryLoadTexture("enemyB", Color.Green);
        _midBossTexture = TryLoadTexture("midboss", Color.Purple);
        _finalBossTexture = TryLoadTexture("finalboss", Color.OrangeRed);

        // create pools
        _bulletFactory = new BulletFactory(4096) { BulletTexture = _bulletTexture };
        _enemyFactoryA = new EnemyFactory(64) { EnemyTexture = _enemyATexture };
        _enemyFactoryB = new EnemyFactory(64) { EnemyTexture = _enemyBTexture };
        _midBossFactory = new EnemyFactory(4) { EnemyTexture = _midBossTexture };
        _finalBossFactory = new EnemyFactory(2) { EnemyTexture = _finalBossTexture };

        // wire bullet factory & player reference into enemy factories
        _enemyFactoryA.BulletFactory = _bulletFactory;
        _enemyFactoryB.BulletFactory = _bulletFactory;
        _midBossFactory.BulletFactory = _bulletFactory;
        _finalBossFactory.BulletFactory = _bulletFactory;

        // load font if available (add a SpriteFont named DefaultFont to Content to use)
        try
        {
            _font = Content.Load<SpriteFont>("DefaultFont");
        }
        catch
        {
            _font = null;
        }

        // stage manager
        _stageManager = new StageManager(this, _enemyFactoryA, _enemyFactoryB, _midBossFactory, _finalBossFactory);

        // create player
        _player = new Player(_playerTexture, new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height - 80));

        // give factories access to the player for aimed shots
        _enemyFactoryA.PlayerRef = _player;
        _enemyFactoryB.PlayerRef = _player;
        _midBossFactory.PlayerRef = _player;
        _finalBossFactory.PlayerRef = _player;

        // create a 1x1 white pixel used for hitbox drawing
        _whitePixel = CreatePlaceholderTexture(Color.White, 1);
    }

    private Texture2D TryLoadTexture(string assetName, Color fallbackColor, int size = 32)
    {
        try
        {
            return Content.Load<Texture2D>(assetName);
        }
        catch
        {
            // create a simple square placeholder texture
            return CreatePlaceholderTexture(fallbackColor, size);
        }
    }

    private Texture2D CreatePlaceholderTexture(Color color, int size = 32)
    {
        var tex = new Texture2D(GraphicsDevice, size, size);
        var data = new Color[size * size];
        for (int i = 0; i < data.Length; i++) data[i] = color;
        tex.SetData(data);
        return tex;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var ks = Keyboard.GetState();

        // player
        _player.Update(gameTime, ks, GraphicsDevice.Viewport);

        // shooting
        _playerShootTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (ks.IsKeyDown(Keys.Space) && _playerShootTimer <= 0f)
        {
            _playerShootTimer = _playerShootInterval;
            // spawn a bullet going up from player
            _bulletFactory.Spawn(_player.Position, new Vector2(0, -1), 0f, 600f);
        }

        _bulletFactory.UpdateAll(gameTime);
        _enemyFactoryA.UpdateAll(gameTime);
        _enemyFactoryB.UpdateAll(gameTime);
        _midBossFactory.UpdateAll(gameTime);
        _finalBossFactory.UpdateAll(gameTime);

        // update stage manager (may spawn enemies)
        _stageManager.Update(gameTime);

        // Collision detection: bullets -> enemies, enemy bullets -> player
        var bullets = _bulletFactory.LiveBullets().ToList();
        foreach (var b in bullets)
        {
            if (!b.IsAlive) continue;

            if (!b.IsEnemyBullet)
            {
                // player bullet hits enemies across all factories
                var factories = new[] { _enemyFactoryA, _enemyFactoryB, _midBossFactory, _finalBossFactory };
                bool hit = false;
                foreach (var fac in factories)
                {
                    foreach (var e in fac.LiveEnemies())
                    {
                        var enemyRadius = e.Texture != null ? Math.Max(e.Texture.Width, e.Texture.Height) / 2f : 16f;
                        var sum = enemyRadius + b.Radius;
                        if (Vector2.DistanceSquared(e.Position, b.Position) <= sum * sum)
                        {
                            var died = e.Damage(b.Damage);
                            b.IsAlive = false;
                            if (died) _score += 100;
                            hit = true;
                            break;
                        }
                    }
                    if (hit) break;
                }
            }
            else
            {
                // enemy bullet hits player (use player's small hitbox)
                var sum = _player.HitboxRadius + b.Radius;
                if (Vector2.DistanceSquared(_player.Position, b.Position) <= sum * sum)
                {
                    _player.Damage(b.Damage);
                    b.IsAlive = false;
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _player.Draw(_spriteBatch);
        // draw player hitbox when slow
        if (_player.IsSlow && _whitePixel != null)
        {
            var size = 6;
            _spriteBatch.Draw(_whitePixel, new Rectangle((int)(_player.Position.X - size/2), (int)(_player.Position.Y - size/2), size, size), Color.White);
        }
        _bulletFactory.DrawAll(_spriteBatch);
        _enemyFactoryA.DrawAll(_spriteBatch);
        _enemyFactoryB.DrawAll(_spriteBatch);
        _midBossFactory.DrawAll(_spriteBatch);
        _finalBossFactory.DrawAll(_spriteBatch);

        // UI
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, $"Lives: {_player.Lives}", new Vector2(12, 12), Color.White);
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(12, 40), Color.White);
            _spriteBatch.DrawString(_font, $"Stage: {_stageManager.CurrentStageName}", new Vector2(12, 68), Color.Yellow);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

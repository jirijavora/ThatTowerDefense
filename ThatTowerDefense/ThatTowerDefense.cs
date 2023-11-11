using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThatTowerDefense;

/// <summary>
///     This is the main type for your game.
/// </summary>
public class ThatTowerDefense : Game {
    private SpriteBatch _spriteBatch;
    private SpriteFont _spriteFont;
    private SpriteFont _spriteFont2;
    private List<Balloon> balloonList;

    // The camera 
    private StaticCamera camera;

    private Cannon cannon;
    private GraphicsDeviceManager graphics;
    private bool lost;

    private Texture2D overlayBackground;
    private bool running = true;
    private SpriteBatch spriteBatch;
    private LowPolyTerrain terrain;
    private bool won;


    public ThatTowerDefense() {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Allows the game to perform any initialization it needs to before starting to run.
    ///     This is where it can query for any required services and load any non-graphic
    ///     related content.  Calling base.Initialize will enumerate through any components
    ///     and initialize them as well.
    /// </summary>
    protected override void Initialize() {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    /// <summary>
    ///     LoadContent will be called once per game and is the place to load
    ///     all of your content.
    /// </summary>
    protected override void LoadContent() {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);
        // Create the camera
        camera = new StaticCamera(this, new Vector3(350, 50, -250), new Vector3(0f, -0.3f, -1f));
        var heightmap = Content.Load<Texture2D>("my_heightmap");
        terrain = new LowPolyTerrain(this, heightmap, 15f, Matrix.Identity);

        _spriteFont = Content.Load<SpriteFont>("Arial");
        _spriteFont2 = Content.Load<SpriteFont>("ArialSmall");

        overlayBackground = Content.Load<Texture2D>("Blank");


        balloonList = new List<Balloon> {
            new(this, new Vector3(420, 50, -330), terrain),
            new(this, new Vector3(480, 50, -330), terrain),
            new(this, new Vector3(500, 50, -330), terrain),
            new(this, new Vector3(520, 50, -330), terrain),
            new(this, new Vector3(540, 50, -330), terrain),
            new(this, new Vector3(560, 50, -330), terrain),
            new(this, new Vector3(580, 50, -330), terrain),
            new(this, new Vector3(600, 50, -330), terrain),
            new(this, new Vector3(620, 50, -330), terrain),
            new(this, new Vector3(640, 50, -330), terrain),
            new(this, new Vector3(660, 50, -330), terrain),
            new(this, new Vector3(680, 50, -330), terrain),
            new(this, new Vector3(700, 50, -330), terrain),
            new(this, new Vector3(720, 50, -330), terrain)
        };

        // Create the cannon
        cannon = new Cannon(this, new Vector3(300, 0, -330), -MathHelper.PiOver2, balloonList);
        cannon.HeightMap = terrain;
    }

    /// <summary>
    ///     UnloadContent will be called once per game and is the place to unload
    ///     game-specific content.
    /// </summary>
    protected override void UnloadContent() {
        // TODO: Unload any non ContentManager content here
    }

    /// <summary>
    ///     Allows the game to run logic such as updating the world,
    ///     checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !won && !lost)
            running = true;

        if (!running) return;


        // Update the cannon
        cannon.Update(gameTime);

        foreach (var balloon in balloonList)
            if (balloon.Update(gameTime, 306)) {
                running = false;
                lost = true;
            }

        if (balloonList.Count == 0) won = true;

        base.Update(gameTime);
    }

    /// <summary>
    ///     This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (var balloon in balloonList) balloon.Draw(camera);

        // Draw the cannon
        cannon.Draw(camera);

        terrain.Draw(camera);

        base.Draw(gameTime);

        if (lost) {
            spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

            var backgroundColor = Color.DimGray;
            backgroundColor.A = 100;
            spriteBatch.Draw(overlayBackground,
                new Rectangle(0, 0, 1000, 1000), backgroundColor);


            spriteBatch.DrawString(_spriteFont,
                "You lost",
                new Vector2(320, 160),
                Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            spriteBatch.End();
        }

        if (won) {
            spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

            var backgroundColor = Color.DimGray;
            backgroundColor.A = 100;
            spriteBatch.Draw(overlayBackground,
                new Rectangle(0, 0, 1000, 1000), backgroundColor);


            spriteBatch.DrawString(_spriteFont,
                "You won!",
                new Vector2(320, 160),
                Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

            spriteBatch.End();
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThatTowerDefense;

/// <summary>
///     A class representing a cannon in the game
/// </summary>
public class Cannon {
    // The cannon's rotation speed
    private const float RotateSpeed = 0.35f;

    private const float minBarellRotation = -MathHelper.PiOver4 / 2f;
    private const float maxBarellRotation = MathHelper.PiOver4;

    private const double minShotDelay = 2;

    private const float cannonBallSpeed = 20f;

    // Barrel fields 
    private readonly ModelBone barellBone;
    private readonly Matrix barellTransform;

    // The direction the cannon is facing

    // The game this cannon belongs to 
    private readonly Game game;

    // The cannon's model
    private readonly Model model;

    // The cannon's rotation in the world
    private readonly float rotation;

    private readonly List<CannonBall> shotCannonBalls = new();

    // The bone transformation matrices of the cannon
    private readonly Matrix[] transforms;

    private readonly List<Balloon> balloonList;

    /// <summary>
    ///     The angle the cannon is facing (in radians)
    /// </summary>
    private float barellRotation;

    // The cannon's position in the world 
    private Vector3 position;
    private double timeSinceLastShot = minShotDelay;


    /// <summary>
    ///     Constructs a new Cannon instance
    /// </summary>
    /// <param name="game">The game this cannon belongs to</param>
    public Cannon(Game game, Vector3 position, float rotation, List<Balloon> balloonList) {
        this.game = game;
        this.position = position;
        this.rotation = rotation;
        this.balloonList = balloonList;
        model = game.Content.Load<Model>("cannon");
        transforms = new Matrix[model.Bones.Count];

        // Set the barell fields
        barellBone = model.Bones["barell"];
        barellTransform = barellBone.Transform;
    }

    /// <summary>
    ///     Gets or sets the IHeightMap this cannon is driving upon
    /// </summary>
    public IHeightMap HeightMap { get; set; }

    /// <summary>
    ///     The position of the cannon in the world
    /// </summary>
    public Vector3 Position => position;

    private void shootCannonBall() {
        var world = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(position);

        var barellBoneTransform = Matrix.CreateRotationX(barellRotation) * barellBone.Transform * world;
        var cannonballPosition = Vector3.Transform(new Vector3(0, 0, 1.4f), barellBoneTransform);


        var cannonballXSpeed = cannonBallSpeed * (float)Math.Cos(barellRotation);
        var cannonballYSpeed = cannonBallSpeed * (float)Math.Sin(barellRotation);


        shotCannonBalls.Add(
            new CannonBall(game, cannonballPosition,
                new Vector3(cannonballXSpeed, cannonballYSpeed, 0), HeightMap, balloonList));
    }

    /// <summary>
    ///     Updates the cannons, moving it based on player input
    /// </summary>
    /// <param name="gameTime">The current GameTime</param>
    public void Update(GameTime gameTime) {
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.W)) barellRotation += RotateSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboard.IsKeyDown(Keys.S)) barellRotation -= RotateSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        timeSinceLastShot += gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboard.IsKeyDown(Keys.Space) && timeSinceLastShot >= minShotDelay) {
            timeSinceLastShot = 0;
            shootCannonBall();
        }

        if (barellRotation > maxBarellRotation) barellRotation = maxBarellRotation;
        if (barellRotation < minBarellRotation) barellRotation = minBarellRotation;

        // Set the cannon's height based on the HeightMap
        if (HeightMap != null) position.Y = HeightMap.GetHeightAt(position.X, position.Z);

        foreach (var ball in shotCannonBalls) ball.Update(gameTime);
    }

    /// <summary>
    ///     Draws the cannon in the world
    /// </summary>
    /// <param name="camera">The camera used to render the world</param>
    public void Draw(ICamera camera) {
        foreach (var ball in shotCannonBalls) ball.Draw(camera);

        var world = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(position);

        barellBone.Transform = Matrix.CreateRotationX(barellRotation) * barellTransform;


        model.CopyAbsoluteBoneTransformsTo(transforms);
        // draw the cannon meshes 
        foreach (var mesh in model.Meshes) {
            foreach (BasicEffect effect in mesh.Effects) {
                effect.EnableDefaultLighting();
                effect.World = transforms[mesh.ParentBone.Index] * world;
                effect.View = camera.View;
                effect.Projection = camera.Projection;
            }

            mesh.Draw();
        }
    }
}
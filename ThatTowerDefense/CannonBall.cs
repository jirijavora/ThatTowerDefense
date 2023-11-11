using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThatTowerDefense;

public class CannonBall {
    private const float gravity = 4f;

    private readonly List<Balloon> balloonList;
    private readonly IHeightMap heightMap;

    private readonly Model model;

    private bool disappearing;
    private Game game;
    private Vector3 velocity;

    public CannonBall(Game game, Vector3 position, Vector3 velocity, IHeightMap heightMap, List<Balloon> balloonList) {
        this.game = game;
        this.position = position;
        this.velocity = velocity;
        this.heightMap = heightMap;
        this.balloonList = balloonList;

        model = game.Content.Load<Model>("cannon_ball");
    }

    public Vector3 position { get; private set; }

    public void Update(GameTime gameTime) {
        // Check for ground collision
        if (position.Y - 0.2f <= heightMap.GetHeightAt(position.X, position.Z)) {
            velocity = new Vector3(0, velocity.Y * 0.7f, 0);
            disappearing = true;
        }

        velocity += new Vector3(0, -gravity, 0) * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!disappearing)
            balloonList.RemoveAll(balloon =>
                Math.Abs(balloon.position.X - position.X) < 2f && Math.Abs(balloon.position.Y - position.Y) < 5f);

        position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(ICamera camera) {
        var world = Matrix.CreateTranslation(position);

        var view = camera.View;

        var projection = camera.Projection;

        model.Draw(world, view, projection);
    }
}
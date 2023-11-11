using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThatTowerDefense;

public class Balloon {
    private static readonly Vector3 velocity = new(-7, 0, 0);
    private readonly IHeightMap heightMap;

    private readonly Model model;

    private readonly Matrix[] transforms;

    private Game game;
    public Vector3 position;


    public Balloon(Game game, Vector3 position, IHeightMap heightMap) {
        this.position = position;
        this.game = game;
        this.heightMap = heightMap;

        model = game.Content.Load<Model>("balloon");

        transforms = new Matrix[model.Bones.Count];
    }

    public bool Update(GameTime gameTime, float loosePos) {
        position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        position.Y = heightMap.GetHeightAt(position.X, position.Z);

        if (position.X < loosePos) return true;

        return false;
    }

    public void Draw(ICamera camera) {
        var world = Matrix.CreateTranslation(position);


        model.CopyAbsoluteBoneTransformsTo(transforms);
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
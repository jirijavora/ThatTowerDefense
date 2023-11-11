using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThatTowerDefense;

/// <summary>
///     A class representing terrain
/// </summary>
public class LowPolyTerrain : IHeightMap {
    #region Private Fields

    // The game this terrain belongs to
    private readonly Game game;

    // The terrain hieghts
    private float[,] heights;

    // The width of the terrain (in squares)
    private int width;

    // The height of the terrain (in squares)
    private int height;

    // The number of triangles in the terrain mesh
    private int triangles;

    // The vertices of the terrain mesh
    private VertexBuffer vertices;

    // The indices of the terrain mesh
    private IndexBuffer indices;

    // The effect to render the terrain mesh
    private BasicEffect effect;

    // The texture to apply to the terrain
    private readonly Texture2D texture;

    private const float MapScaleFactor = 10;

    #endregion

    #region Initialization

    /// <summary>
    ///     Converts the supplied Texture2D into height data
    /// </summary>
    /// <param name="heightmap">The heightmap texture</param>
    /// <param name="scale">The difference between the highest and lowest elevation</param>
    private void LoadHeights(Texture2D heightmap, float scale) {
        // Convert the scale factor to work with our color
        scale /= 256;
        width = heightmap.Width;
        height = heightmap.Height;

        heights = new float[width, height];
        var heightmapColors = new Color[width * height];
        heightmap.GetData(heightmapColors);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            heights[x, y] = heightmapColors[x + y * width].R * scale;
    }

    /// <summary>
    ///     Creates the terrain vertex buffer
    /// </summary>
    private void InitializeVertices() {
        var terrainVertices = new VertexPositionNormalTexture[width * height];
        var i = 0;
        for (var z = 0; z < height; z++)
        for (var x = 0; x < width; x++) {
            var currHeight = heights[x, z];

            float deltaX;
            float deltaZ;

            if (x > 0)
                deltaX = heights[x - 1, z] - currHeight;
            else
                deltaX = currHeight - heights[x + 1, z];

            if (z > 0)
                deltaZ = heights[x, z - 1] - currHeight;
            else
                deltaZ = currHeight - heights[x, z + 1];

            terrainVertices[i].Position = new Vector3(x, heights[x, z], -z);
            terrainVertices[i].Normal =
                new Vector3(deltaX, 1, deltaZ); //TODO: Fix this to actually be the normal to the surface
            terrainVertices[i].TextureCoordinate = new Vector2(0, 0);
            i++;
        }

        vertices = new VertexBuffer(game.GraphicsDevice, typeof(VertexPositionNormalTexture), terrainVertices.Length,
            BufferUsage.None);
        vertices.SetData(terrainVertices);
    }

    /// <summary>
    ///     Initializes the indices
    /// </summary>
    private void InitializeIndices() {
        triangles = width * 2 * (height - 1);
        var terrainIndices = new int[triangles];
        var i = 0;
        var z = 0;
        while (z < height - 1) {
            for (var x = 0; x < width; x++) {
                terrainIndices[i++] = x + z * width;
                terrainIndices[i++] = x + (z + 1) * width;
            }

            z++;
            if (z < height - 1)
                for (var x = width - 1; x >= 0; x--) {
                    terrainIndices[i++] = x + (z + 1) * width;
                    terrainIndices[i++] = x + z * width;
                }

            z++;
        }

        var elementSize = IndexElementSize.ThirtyTwoBits;
        indices = new IndexBuffer(game.GraphicsDevice, elementSize, terrainIndices.Length, BufferUsage.None);
        indices.SetData(terrainIndices);
    }

    /// <summary>
    ///     Initialize the effect used to render the terrain
    /// </summary>
    /// <param name="world">The world matrix</param>
    private void InitializeEffect(Matrix world) {
        effect = new BasicEffect(game.GraphicsDevice);
        effect.World = world * Matrix.CreateScale(MapScaleFactor);
        effect.Texture = texture;
        effect.TextureEnabled = true;

        // Turn on the lighting subsystem
        effect.LightingEnabled = true;

        effect.AmbientLightColor = Colors.GrassColor * Colors.AmbientColorMultiplier;
        effect.DirectionalLight0.DiffuseColor = Colors.GrassColor * Colors.DiffuseColorMultiplier;
        effect.DirectionalLight0.Direction = new Vector3(-0.8f, -1, 0.8f);
        effect.DirectionalLight0.SpecularColor = Colors.GrassHighlightColor;
    }

    /// <summary>
    ///     Constructs a new LowPolyTerrain
    /// </summary>
    /// <param name="game">The game this Terrain belongs to</param>
    /// <param name="heightmap">The heightmap used to set heights</param>
    /// <param name="heightRange">The difference between the lowest and highest elevation in the terrain</param>
    /// <param name="world">The terrain's position and orientation in the world</param>
    public LowPolyTerrain(Game game, Texture2D heightmap, float heightRange, Matrix world) {
        this.game = game;
        texture = game.Content.Load<Texture2D>("blank");
        LoadHeights(heightmap, heightRange);
        InitializeVertices();
        InitializeIndices();
        InitializeEffect(world);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Draws the terrain
    /// </summary>
    /// <param name="camera">The camera to use</param>
    public void Draw(ICamera camera) {
        effect.View = camera.View;
        effect.Projection = camera.Projection;

        effect.CurrentTechnique.Passes[0].Apply();
        game.GraphicsDevice.SetVertexBuffer(vertices);
        game.GraphicsDevice.Indices = indices;
        game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, triangles);
    }

    /// <summary>
    ///     Gets the height of the terrain at
    ///     the supplied world coordinates
    /// </summary>
    /// <param name="x">The x world coordinate</param>
    /// <param name="z">The z world coordinate</param>
    /// <returns></returns>
    public float GetHeightAt(float x, float z) {
        var inverseWorld = Matrix.Invert(effect.World);
        var worldCoordinates = new Vector3(x, 0, z);
        var modelCoordinates = Vector3.Transform(worldCoordinates, inverseWorld);

        float tx, ty;
        tx = modelCoordinates.X;
        ty = -modelCoordinates.Z;
        if (tx < 0 || ty < 0 || tx > width - 2 || ty > height - 2)
            return 0;

        // Determine which triangle we're in
        if (tx - (int)tx < 0.5 && ty - (int)ty < 0.5) {
            // In the lower-left triangle
            var xFraction = tx - (int)tx;
            var yFraction = ty - (int)ty;
            var xDifference = heights[(int)tx + 1, (int)ty] - heights[(int)tx, (int)ty];
            var yDifference = heights[(int)tx, (int)ty + 1] - heights[(int)tx, (int)ty];
            return (heights[(int)tx, (int)ty]
                    + xFraction * xDifference
                    + yFraction * yDifference) * MapScaleFactor;
        }
        else {
            // In the upper-right triangle
            var xFraction = (int)tx + 1 - tx;
            var yFraction = (int)ty + 1 - ty;
            var xDifference = heights[(int)tx + 1, (int)ty + 1] - heights[(int)tx, (int)ty + 1];
            var yDifference = heights[(int)tx + 1, (int)ty + 1] - heights[(int)tx + 1, (int)ty];
            return (heights[(int)tx + 1, (int)ty + 1]
                    - xFraction * xDifference
                    - yFraction * yDifference) * MapScaleFactor;
        }
    }

    #endregion
}
using Microsoft.Xna.Framework;

namespace ThatTowerDefense;

/// <summary>
///     A static camera
/// </summary>
public class StaticCamera : ICamera {
    #region Initialization

    /// <summary>
    ///     Constructs a new static camera
    /// </summary>
    /// <param name="game">The game this camera belongs to</param>
    /// <param name="position">The position of the camera</param>
    /// <param name="direction">The direction the camera looks towards</param>
    public StaticCamera(Game game, Vector3 position, Vector3 direction) {
        this.game = game;
        this.position = position;
        this.direction = direction;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            game.GraphicsDevice.Viewport.AspectRatio,
            1,
            10000
        );
        View = Matrix.CreateLookAt(position, position + direction, Vector3.Up);
    }

    #endregion


    #region Fields

    // The camera's position
    private readonly Vector3 position;

    // The camera's direction
    private readonly Vector3 direction;

    // The game this camera belongs to 
    private Game game;

    // The view matrix 

    // The projection matrix 

    #endregion

    #region Properties

    /// <summary>
    ///     The camera's view matrix
    /// </summary>
    public Matrix View { get; }

    /// <summary>
    ///     The camera's projection matrix
    /// </summary>
    public Matrix Projection { get; }

    #endregion
}
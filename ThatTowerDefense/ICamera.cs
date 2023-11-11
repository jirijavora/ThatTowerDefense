﻿using Microsoft.Xna.Framework;

namespace ThatTowerDefense;

/// <summary>
///     An interface defining a camera
/// </summary>
public interface ICamera {
    /// <summary>
    ///     The view matrix
    /// </summary>
    Matrix View { get; }

    /// <summary>
    ///     The projection matrix
    /// </summary>
    Matrix Projection { get; }
}
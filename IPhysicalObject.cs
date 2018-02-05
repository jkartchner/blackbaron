using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BlackBaron
{
    public interface IPhysicalObject
    {
        float BirthTime { get; set; }
        Vector2 OrginalPosition { get; set; }
        Vector2 Acceleration { get; set; }
        Vector2 Direction { get; set; }
        Vector2 Position { get; set; }
        float Scaling { get; set; }
        Color ModColor { get; set; }
        float Rotation { get; set; }
        float RotationFactor { get; set; }
        float Life { get; set; }    // formerly MaxAge
        float Weight { get; set; }
        bool isOnGround { get; set; }
        bool isElemental { get; set; }
    }
}

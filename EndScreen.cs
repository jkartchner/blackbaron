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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class EndScreen : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;
        SpriteBatch spriteBatch;

        Texture2D lensSource;
        Texture2D lensFlare1;
        Texture2D lensFlare2;
        Texture2D lensFlare3;
        Texture2D logoTexture;

        SpriteFont LogoFont;

        Vector2 lightVector;
        Vector2 flareDirection;
        Vector2 centerScreenVector;

        Flare[] flares = new Flare[7];

        double endTimer = 0.0;

        public string[] args;
        public string sysPath = "";

        public EndScreen(Game1 game)
            : base(game)
        {
            this.game = game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
                lightVector = new Vector2(game.m_Width / 2, game.m_Height / 2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            logoTexture = game.Content.Load<Texture2D>("BBLogo");
            if (game.m_Width > 800)
                LogoFont = game.Content.Load<SpriteFont>("LogoFont");
            else
                LogoFont = game.Content.Load<SpriteFont>("LogoFontReduced");

            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            lensSource = game.Content.Load<Texture2D>("LensFlareSource");
            lensFlare1 = game.Content.Load<Texture2D>("LensFlare1");
            lensFlare2 = game.Content.Load<Texture2D>("LensFlare2");
            lensFlare3 = game.Content.Load<Texture2D>("LensFlare3");

            Flare flare = new Flare();
            flare.texture = lensSource;
            flare.Distance = 50;
            flare.ImageScale = 1.0f;
            flare.DirectionScale = 1.0f;
            flares[0] = flare;

            flare = new Flare();
            flare.texture = lensFlare2;
            flare.Distance = 125;
            flare.ImageScale = 0.5f;
            flare.DirectionScale = 0.5f;
            flares[1] = flare;

            flare = new Flare();
            flare.texture = lensFlare3;
            flare.Distance = 150;
            flare.ImageScale = 0.25f;
            flare.DirectionScale = 0.33f;
            flares[2] = flare;

            flare = new Flare();
            flare.texture = lensSource;
            flare.Distance = 210;
            flare.ImageScale = 1.0f;
            flare.DirectionScale = 0.125f;
            flares[3] = flare;

            flare = new Flare();
            flare.texture = lensFlare2;
            flare.Distance = 300;
            flare.ImageScale = 0.5f;
            flare.DirectionScale = -0.5f;
            flares[4] = flare;

            flare = new Flare();
            flare.texture = lensFlare3;
            flare.Distance = 360;
            flare.ImageScale = 0.25f;
            flare.DirectionScale = -0.25f;
            flares[5] = flare;

            flare = new Flare();
            flare.texture = lensFlare1;
            flare.Distance = 400;
            flare.ImageScale = 0.25f;
            flare.DirectionScale = -0.18f;
            flares[6] = flare;

            centerScreenVector = new Vector2(game.m_Width / 2, game.m_Height / 2);
            flareDirection = new Vector2();
            base.LoadContent();
        }

        public struct Flare
        {
            public int Distance;
            public Vector2 Position;
            public Vector2 Direction;
            public float ImageScale;
            public float DirectionScale;
            public Texture2D texture;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            switch (game.State)
            {
                case Game1.GameState.EndScreen:
                    endTimer += gameTime.ElapsedGameTime.TotalMilliseconds / 600;
                    if (endTimer >= 1.0f)
                    {
                        game.activeMenu.isExit = true;
                        if (sysPath != "")
                        {
                            ProcessClass p = new ProcessClass(sysPath, args, game);
                        }
                        game.State = Game1.GameState.MenuScreen;
                    }
                    flareDirection = centerScreenVector - lightVector;
                    float length = flareDirection.Length();
                    flareDirection.Normalize();

                    for(int i = 0; i < 7; i++)
                    {
                        flares[i].Distance = (int)(length * flares[i].DirectionScale);
                        flares[i].Direction = flareDirection;
                        flares[i].Position.X = (flareDirection.X * (float)flares[i].Distance) + lightVector.X;
                        flares[i].Position.Y = flareDirection.Y * (float)flares[i].Distance + lightVector.Y;
                    }
                    //lightVector += new Vector2(12, 3.0f);
                    centerScreenVector += new Vector2(12.0f, 3.0f);
                    break;

                default:
                    break;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            switch (game.State)
            {
                case Game1.GameState.EndScreen:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteBlendMode.Additive);
                    // logo and text
                    if (game.m_Width > 439 && game.m_Height > 439 && LogoFont.MeasureString("Black Baron").X < game.m_Width)
                    {
                        spriteBatch.Draw(logoTexture, new Rectangle((int)((game.m_Width - 439) / 2), (int)((game.m_Height - 439) / 3), 439, 439), null, new Color(Color.White, .75f - (float)endTimer), 0, Vector2.Zero, SpriteEffects.None, 0.5f);
                        spriteBatch.DrawString(LogoFont, "B  L  A  C  K  B  A  R  O  N", new Vector2((float)((game.m_Width - LogoFont.MeasureString("B  L  A  C  K  B  A  R  O  N").X) / 2), (float)((game.m_Height - 439) / 3) + 500), new Color(Color.White, 0.75f - (float)endTimer), 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(logoTexture, new Rectangle(0, 0, game.m_Width, game.m_Height), new Color(Color.White, 1.0f - (float)endTimer));
                        spriteBatch.DrawString(LogoFont, "B  L  A  C  K  B  A  R  O  N", new Vector2((float)((game.m_Width - LogoFont.MeasureString("B  L  A  C  K  B  A  R  O  N").X) / 2), (float)((game.m_Height - 439) / 3) + 500), new Color(Color.White, 1.0f - (float)endTimer), 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    }
                    // the flare
                    foreach (Flare f in flares)
                        spriteBatch.Draw(f.texture, f.Position, null, Color.White, 0, new Vector2(64, 64), f.ImageScale + (game.m_Width * 0.001f), SpriteEffects.None, 0.5f);
                    spriteBatch.End();
                    break;

                default:
                    break;
            }
            base.Draw(gameTime);
        }
    }
}
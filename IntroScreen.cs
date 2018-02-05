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
    public class IntroScreen : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;
        Texture2D fadeToBlack;
        Texture2D introScreen;
        Song introBackGroundMusic;
        SpriteFont LogoFont;
        SpriteBatch spriteBatch;
        public enum ImageState { Deferred, Starting, Active, Ending, Inactive };
        ImageState imagestate = ImageState.Inactive;
        double fadeProgress = 0.0;
        double deferredProgress = 0.0;
        double activeProgress = 0.0;

        public IntroScreen(Game1 game)
            : base(game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            this.game = game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            imagestate = ImageState.Deferred;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            fadeToBlack = game.Content.Load<Texture2D>("FadeToBlackScreen");
            introScreen = game.Content.Load<Texture2D>("BBLogo");
            if (game.m_Width > 800)
                LogoFont = game.Content.Load<SpriteFont>("LogoFont");
            else
                LogoFont = game.Content.Load<SpriteFont>("LogoFontReduced");
            introBackGroundMusic = game.Content.Load<Song>("music_lounge");
            MediaPlayer.IsRepeating = true;
            if(game.isSoundOn)
                MediaPlayer.Play(introBackGroundMusic);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            switch (game.State)
            {
                case Game1.GameState.IntroScreen:
                    /*playBackProgress += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (playBackProgress >= bgDuration.TotalMilliseconds - 330)
                    {
                        playBackProgress = 0.0;
                        MediaPlayer.Play(introBackGroundMusic);
                    }*/
                    if(imagestate == ImageState.Deferred)
                        deferredProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 2000;
                    if (imagestate == ImageState.Active)
                        activeProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
                    fadeProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 4000;
                    if (fadeProgress >= 1.0f)
                    {
                        fadeProgress = 0.0;
                        if (imagestate == ImageState.Starting)
                            imagestate = ImageState.Active;
                        if (imagestate == ImageState.Ending)
                        {
                            game.State = Game1.GameState.MenuScreen;
                            imagestate = ImageState.Inactive;
                            MediaPlayer.Stop();
                        }
                    }
                    if (deferredProgress >= 1.0f)
                    {
                        imagestate = ImageState.Starting;
                        fadeProgress = 0;
                        deferredProgress = 0;
                    }
                    if (activeProgress >= 1.0f)
                    {
                        imagestate = ImageState.Ending;
                        fadeProgress = 0;
                        activeProgress = 0;
                    }
                    break;
                case Game1.GameState.MenuScreen:
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
                case Game1.GameState.IntroScreen:
                    if (imagestate == ImageState.Inactive)
                        return;
                    game.GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    if (game.m_Width > 439 && game.m_Height > 439 && LogoFont.MeasureString("Black Baron").X < game.m_Width)
                    {
                        spriteBatch.Draw(introScreen, new Rectangle((int)((game.m_Width - 439) / 2), (int)((game.m_Height - 439) / 3), 439, 439), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
                        spriteBatch.DrawString(LogoFont, "B  L  A  C  K  B  A  R  O  N", new Vector2((float)((game.m_Width - LogoFont.MeasureString("B  L  A  C  K  B  A  R  O  N").X) / 2), (float)((game.m_Height - 439) / 3) + 500), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(introScreen, new Rectangle(0, 0, game.m_Width, game.m_Height), Color.White);
                        spriteBatch.DrawString(LogoFont, "B  L  A  C  K  B  A  R  O  N", new Vector2((float)((game.m_Width - LogoFont.MeasureString("B  L  A  C  K  B  A  R  O  N").X) / 2), (float)((game.m_Height - 439) / 3) + 500), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                    }
                    switch (imagestate)
                    {
                        case ImageState.Deferred:
                            spriteBatch.Draw(fadeToBlack, new Rectangle(0, 0, game.m_Width, game.m_Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1.0f);
                            break;
                        case ImageState.Starting:
                            spriteBatch.Draw(fadeToBlack, new Rectangle(0, 0, game.m_Width, game.m_Height), null, new Color(Color.White, 1.0f - (float)fadeProgress), 0, Vector2.Zero, SpriteEffects.None, 1.0f);
                            break;

                        case ImageState.Ending:
                            spriteBatch.Draw(fadeToBlack, new Rectangle(0, 0, game.m_Width, game.m_Height), new Color(Color.White, (float)fadeProgress));
                            break;

                        default:
                            break;
                    }
                    spriteBatch.End();
                    break;
                case Game1.GameState.MenuScreen:
                    break;
            }
        }
    }
}
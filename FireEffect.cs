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
    public class FireEffect : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;
        SpriteBatch spriteBatch;
        Texture2D fire1;
        Texture2D fire2;
        Texture2D fire3;
        Texture2D snow;
        Texture2D stress;
        Texture2D smoke;
        List<Texture2D> textureList = new List<Texture2D>();

        public Rectangle sourceEmitter;
        public List<Rectangle> sourceEmitters;
        private Rectangle bounds;
        Vector2 motion;

        public int swt = 0;
        public bool isSmallFileOn = false;
        public bool isEffect = false;
        public bool isTab = false;
        public bool hasPlayed = false;

        public bool isAdding = false;
        public bool isSubtracting = false;

        public FireEffect(Game1 game)
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
            sourceEmitters = new List<Rectangle>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            fire1 = game.Content.Load<Texture2D>("Flame1");
            textureList.Add(fire1);
            fire2 = game.Content.Load<Texture2D>("Flame2");
            textureList.Add(fire2);
            fire3 = game.Content.Load<Texture2D>("Flame3");
            textureList.Add(fire3);
            snow = game.Content.Load<Texture2D>("snow");
            stress = game.Content.Load<Texture2D>("Grain");
            smoke = game.Content.Load<Texture2D>("smoke");
            base.LoadContent();
        }

        public void ChangeParticleColors(ParticleEngine.ParticleEffect p, GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            for(int i = 0; i < p.particleList.Count; i++)
            {
                float timeAlive = (float)(now - p.particleList[i].BirthTime);
                if (timeAlive / p.particleList[i].Life < 0.85f)
                    p.particleList[i].ModColor = Color.Lerp(new Color(255, 120, 40, 255), new Color(255, 120, 40, 125), (timeAlive / p.particleList[i].Life));
                if (timeAlive / p.particleList[i].Life >= 0.85f)
                    p.particleList[i].ModColor = Color.Lerp(new Color(255, 120, 40, 255), new Color(Color.DarkOrange, 0), (timeAlive / p.particleList[i].Life));
            }
        }

        public void ChangeParticleSpeed(ParticleEngine.ParticleEffect p, GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < p.particleList.Count; i++)
            {
                float timeAlive = (float)(now - p.particleList[i].BirthTime);
                if (!p.particleList[i].isModified)
                {
                    p.particleList[i].Direction *= new Vector2(p.particleList[i].Scaling - 0.33f, p.particleList[i].Scaling - 0.33f);
                    p.particleList[i].isModified = true;
                }
            }
        }

        public void ChangeParticleStress(ParticleEngine.ParticleEffect p, GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < p.particleList.Count; i++)
            {
                float timeAlive = (float)(now - p.particleList[i].BirthTime);
                if (!p.particleList[i].isModified)
                {
                    float r = p.VectorToAngle(p.particleList[i].Direction);
                    r += (float)Math.PI;                 // add a bit to the angle for the acceleration so that the particle curves in its trajectory
                    if(r > Math.PI * 2)         // make sure the acceleration doesn't exceed the full range of a circle
                        r -= (float)Math.PI * 2;
                    p.particleList[i].Acceleration = p.AngleToVector(r);
                    p.particleList[i].isModified = true;
                }
                if (timeAlive / p.particleList[i].Life > 0.70f)                 // make the particle phase out in a not so ugly way
                    p.particleList[i].ModColor = Color.Lerp(Color.DarkBlue, new Color(Color.DarkBlue, 0.0f), (timeAlive / p.particleList[i].Life));
            }
            if (sourceEmitter.Left <= bounds.Left || sourceEmitter.Right >= bounds.Right)
                motion.X = -motion.X;
            if (sourceEmitter.Top <= bounds.Top || sourceEmitter.Bottom >= bounds.Bottom)
                motion.Y = -motion.Y;
            sourceEmitter.X += (int)(motion.X * 13);
            sourceEmitter.Y += (int)(motion.Y * 13);
            p.Emitter = sourceEmitter;

            if (isAdding)
                p.NumberOfParticles += 10;
            if (isSubtracting && p.NumberOfParticles > 20)
                p.NumberOfParticles -= 10;

            isSubtracting = false;
            isAdding = false;

        }

        public void ChangeParticleTrail(ParticleEngine.ParticleEffect p, GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < p.particleList.Count; i++)
            {
                float timeAlive = (float)(now - p.particleList[i].BirthTime);
                /*if (!p.particleList[i].isModified)
                {
                    float r = p.VectorToAngle(p.particleList[i].Direction);
                    r += (float)Math.PI;                 // add a bit to the angle for the acceleration so that the particle curves in its trajectory
                    if (r > Math.PI * 2)         // make sure the acceleration doesn't exceed the full range of a circle
                        r -= (float)Math.PI * 2;
                    p.particleList[i].Acceleration = p.AngleToVector(r);
                    p.particleList[i].isModified = true;
                }*/
                if (timeAlive / p.particleList[i].Life > 0.70f)                 // make the particle phase out in a not so ugly way
                    p.particleList[i].ModColor = Color.Lerp(Color.White, new Color(Color.White, 0.0f), (timeAlive / p.particleList[i].Life));
            }
            if (sourceEmitter.Left <= bounds.Left || sourceEmitter.Right >= bounds.Right)
                motion.X = -motion.X;
            if (sourceEmitter.Top <= bounds.Top || sourceEmitter.Bottom >= bounds.Bottom)
                motion.Y = -motion.Y;
            sourceEmitter.X += (int)(motion.X * 33);
            sourceEmitter.Y += (int)(motion.Y * 33);
            p.Emitter = sourceEmitter;

            if (isAdding)
                p.NumberOfParticles += 10;
            if (isSubtracting && p.NumberOfParticles > 20)
                p.NumberOfParticles -= 10;

            isSubtracting = false;
            isAdding = false;

        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            switch (game.State)
            {
                case Game1.GameState.MenuScreen:
                    if (isSmallFileOn)
                    {
                        foreach(Rectangle r in sourceEmitters)
                            game.particleEngine.AddEffect(textureList, r, new ParticleEngine.CustomUpdate(ChangeParticleColors), new Color(255, 120, 40, 255), 0.005f, 1.0f, 670.0f, -0.05f, 270, Vector2.Zero, true, false, true, 2, 180, 0.15f, 0.05f, gameTime);
                        sourceEmitters.Clear();
                        isSmallFileOn = false;
                        if (game.isSoundOn && MediaPlayer.State != MediaState.Playing)
                        {
                            if (hasPlayed)
                                MediaPlayer.Stop();
                            else
                            {
                                MediaPlayer.IsRepeating = false;
                                MediaPlayer.Play(game.fireSoundEffect);
                                hasPlayed = true;
                            }
                        }

                    }
                    break;
                case Game1.GameState.SaveScreen:
                    if (!isEffect)
                    {
                        Random r = new Random();
                        if (!isTab)
                            swt = r.Next(4);
                        isEffect = true;
                        switch (swt)
                        {
                            case 0:     // generate camp fire
                                game.particleEngine.AddEffect(textureList, new Rectangle(game.m_Width / 2 - 150, game.m_Height - 120, 300, 20), new ParticleEngine.CustomUpdate(ChangeParticleColors), new Color(255, 120, 40, 255), 0.05f, 1.0f, 850.0f, -0.80f, 270, Vector2.Zero, true, false, true, 4, 180, 3.0f, 1.0f, gameTime);
                                //game.particleEngine.AddEffect(textureList, new Rectangle(game.m_Width / 4 - 5, game.m_Height - 130, 12, 10), new ParticleEngine.CustomUpdate(ChangeParticleColors), new Color(255, 120, 40, 255), 0.005f, 1.0f, 750.0f, -0.05f, 270, Vector2.Zero, true, false, true, 2, 180, 0.20f, 0.07f, gameTime);
                                break;
                            case 1:     // generate snow
                                //game.particleEngine.AddEffect(snow, new Rectangle(0, -5, game.m_Width, 5), null, Color.White, 0, 0, 5000, 0.01f, 90, Vector2.Zero, true, false, true, 20, 180, 0.01f, 0.75f, gameTime);
                                game.particleEngine.AddEffect(snow, new Rectangle(-700, -5, game.m_Width + 700, 5), new ParticleEngine.CustomUpdate(ChangeParticleSpeed), Color.White, 0, 0, 7000, 0.0001f, 90, new Vector2(5.0f, 5.0f), true, true, true, 1, 180, 4.0f, 1.33f, gameTime);
                                break;
                            case 2:     // generate stress test
                                List<Texture2D> list = new List<Texture2D>();
                                list.Add(stress);
                                sourceEmitter = new Rectangle(game.m_Width / 2, game.m_Height / 2, 50, 50);
                                bounds = new Rectangle((game.m_Width / 2) - 150, (game.m_Height / 2) - 150, 300, 300);

                                // generate a random direction and speed for the emitter
                                int j = r.Next(360);
                                motion = AngleToVector(MathHelper.ToRadians(j));
                                game.particleEngine.AddEffect(list, sourceEmitter, new ParticleEngine.CustomUpdate(ChangeParticleStress), new Color(10, 40, 100, 255), 0, 1.0f, 1500, 1.0f, 0, Vector2.Zero, true, false, true, 20, 360, 10.0f, 2.0f, gameTime);
                                break;
                            case 3:
                                bounds = new Rectangle(0, 0, game.m_Width, game.m_Height);
                                sourceEmitter = new Rectangle(game.m_Width / 2 - 25, game.m_Height - 120, 50, 50);
                                game.particleEngine.AddEffect(smoke, sourceEmitter, new ParticleEngine.CustomUpdate(ChangeParticleTrail), Color.White, 0.02f, 1.0f, 1000f, -0.40f, 0, Vector2.Zero, true, false, false, 3, 360, 0.5f, 1.0f, gameTime);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }

            base.Update(gameTime);
        }

        private Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }
    }
}
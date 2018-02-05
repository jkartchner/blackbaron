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
    public class BackGroundTexture : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;

        SpriteBatch spriteBatch;
        Effect gaussianBlurEffect;
        Effect bloomCombineEffect;

        ResolveTexture2D resolveTarget;
        ResolveTexture2D resolveTarget1;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;

        public bool isOn = false;
        public bool hasResolved = false;

        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

        public BackGroundTexture(Game1 game)
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
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            gaussianBlurEffect = Game.Content.Load<Effect>("GaussianBlur");
            bloomCombineEffect = Game.Content.Load<Effect>("BloomCombine");

            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
            resolveTarget = new ResolveTexture2D(GraphicsDevice, game.m_Width, game.m_Height, 1,
                format);

            resolveTarget1 = new ResolveTexture2D(GraphicsDevice, game.m_Width, game.m_Height, 1, format);

            renderTarget1 = new RenderTarget2D(GraphicsDevice, game.m_Width, game.m_Height, 1,
                format);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, game.m_Width, game.m_Height, 1,
                format);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            int width = game.m_Width / 2;
            int height = game.m_Height / 2;

            renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, 1,
                format);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, 1,
                format);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            switch (game.State)
            {
                case Game1.GameState.MenuScreen:
                    if (isOn)
                    {
                        if (!hasResolved)
                        {
                            // Resolve the scene into a texture, so we can
                            // use it as input data for the bloom processing.
                            GraphicsDevice.ResolveBackBuffer(resolveTarget);
                            hasResolved = true;
                        }
                        
                        GraphicsDevice.ResolveBackBuffer(resolveTarget1);

                        // Pass 1: draw the scene into rendertarget 1, using a
                        // shader that extracts only the brightest parts of the image.

                        DrawFullscreenQuad(resolveTarget, renderTarget1,
                                           gaussianBlurEffect,
                                           IntermediateBuffer.PreBloom);

                        // Pass 2: draw from rendertarget 1 into rendertarget 2,
                        // using a shader to apply a horizontal gaussian blur filter.
                        SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

                        DrawFullscreenQuad(resolveTarget, renderTarget2,
                                           gaussianBlurEffect,
                                           IntermediateBuffer.BlurredHorizontally);

                        // Pass 3: draw from rendertarget 2 back into rendertarget 1,
                        // using a shader to apply a vertical gaussian blur filter.
                        SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

                        DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1,
                                           gaussianBlurEffect,
                                           IntermediateBuffer.BlurredBothWays);
                        // Pass 4: draw both rendertarget 1 and the original scene
                        // image back into the main backbuffer, using a shader that
                        // combines them to produce the final bloomed result.
                        GraphicsDevice.SetRenderTarget(0, null);
                        GraphicsDevice.Textures[1] = resolveTarget1;

                        Viewport viewport = GraphicsDevice.Viewport;

                        DrawFullscreenQuad(renderTarget1.GetTexture(),
                                           viewport.Width, viewport.Height,
                                           bloomCombineEffect,
                                           IntermediateBuffer.FinalResult);

                        /*spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.BackToFront, SaveStateMode.None);
                            spriteBatch.Draw(renderTarget1.GetTexture(), new Rectangle(0, 0, game.m_Width, game.m_Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
                            spriteBatch.Draw(resolveTarget1, new Rectangle(0, 0, game.m_Width, game.m_Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1.0f);
                        spriteBatch.End();*/
                    }
                    break;
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            GraphicsDevice.SetRenderTarget(0, null);
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.FrontToBack,
                              SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();

            // End the custom effect.
            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = 2.0f;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
    }
}
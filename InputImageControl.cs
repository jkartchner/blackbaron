using System;
using System.Collections.Generic;
using System.Linq;
using Soopah.Xna.Input;
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
    public class InputImageControl : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;

        int selectedItem = 0;
        int oldItem = 0;
        enum ImageState { Active, Starting, Ending, Inactive }
        ImageState imageState = ImageState.Starting;
        ImageState oldImageState = ImageState.Inactive;
        SpriteBatch spriteBatch;
        Texture2D NESTexture;
        Texture2D SNESTexture;
        Texture2D genesisTexture;
        Texture2D currentTexture;
        Texture2D oldTexture;

        KeyboardState lastKeyState = new KeyboardState();
        ButtonState[] lastButtonState = new ButtonState[9];
        DirectInputThumbSticks lastSticks = new DirectInputThumbSticks();

        double fadeProgress = 0.0;

        int horPosition1;
        int verPosition1;

        public Keys[] k = new Keys[14];
        public int[] b = new int[14];
        public bool[] f = new bool[14];

        public Keys[] player2k = new Keys[14];
        public int[] player2b = new int[14];
        public bool[] player2f = new bool[14];

        public bool isSetting = false;
        bool hasDirection = false;
        bool hasMoved = false;

        public InputImageControl(Game1 game)
            : base(game)
        {
            this.game = game;
            horPosition1 = (int)(game.m_Width * 0.4f);
            verPosition1 = (int)(game.m_Height * 0.33333f);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            lastKeyState = new KeyboardState();
            lastButtonState = new ButtonState[9];
            //if (game.gamePad[0] != null)
              //  isController = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            NESTexture = game.Content.Load<Texture2D>("NintendoController");
            SNESTexture = game.Content.Load<Texture2D>("SuperNintendoController");
            genesisTexture = game.Content.Load<Texture2D>("GenesisController");
            base.LoadContent();
        }

        private void CheckInput()
        {
            KeyboardState currentKeyState = Keyboard.GetState();
            ButtonState[] buttonState = new ButtonState[13];
            DirectInputThumbSticks thumbState = new DirectInputThumbSticks();
            if (DirectInputGamepad.Gamepads.Count > 0)
            {
                buttonState = game.gamePad[0].Buttons.List.ToArray();
                thumbState = game.gamePad[0].ThumbSticks;
            }
            if ((currentKeyState.IsKeyDown(Keys.Tab) && lastKeyState.IsKeyUp(Keys.Tab)))
            {
                isSetting = !isSetting;
                if (isSetting)
                {
                    game.fireEffect.sourceEmitters.Add(new Rectangle(game.m_Width / 2 + 300, game.m_Height - (80), 12, 10));
                    game.fireEffect.isSmallFileOn = true;

                }
                else
                    game.particleEngine.StopEffects();
                lastKeyState = currentKeyState;
                lastButtonState = buttonState;
                return;
            }

            if (!isSetting)
            {

                if ((currentKeyState.IsKeyDown(Keys.C) && lastKeyState.IsKeyUp(Keys.C)) || (lastButtonState[4] == ButtonState.Released && buttonState[4] == ButtonState.Pressed))
                {
                    oldItem = selectedItem;
                    selectedItem++;
                    if (selectedItem > 2)
                        selectedItem = 0;
                    imageState = ImageState.Starting;
                    oldImageState = ImageState.Ending;
                    fadeProgress = 0;
                }
            }
            else
            {

                Keys[] k2 = currentKeyState.GetPressedKeys();
                if (k2.Length < 2 && k2.Length > 0 && k2[0] != Keys.Escape && k2[0] != Keys.Tab && (currentKeyState.IsKeyDown(k2[0]) && lastKeyState.IsKeyUp(k2[0])))
                {
                    if (game.activeMenu == game.menuList[8])
                    {
                        k[game.activeMenu.selectedItem] = k2[0];
                        f[game.activeMenu.selectedItem] = false;
                    }
                    else if (game.activeMenu == game.menuList[9])
                    {
                        player2k[game.activeMenu.selectedItem] = k2[0];
                        player2f[game.activeMenu.selectedItem] = false;
                    }
                    if (game.activeMenu.selectedItem == 13)
                    {
                        game.particleEngine.StopEffects();
                        isSetting = !isSetting;
                    }
                    else
                        game.activeMenu.selectedItem++;
                }

                List<ButtonState> bState = new List<ButtonState>();
                int j = 0;
                for (int i = 0; i < buttonState.Length; i++)
                {
                    if (buttonState[i] == ButtonState.Pressed)
                    {
                        bState.Add(buttonState[i]);
                        j = i;
                    }
                }
                if (bState.Count < 2 && bState.Count > 0 && (buttonState[j] == ButtonState.Pressed && lastButtonState[j] == ButtonState.Released))
                {
                    if (game.activeMenu == game.menuList[8])
                    {
                        b[game.activeMenu.selectedItem] = j;
                        f[game.activeMenu.selectedItem] = true;
                    }
                    else if (game.activeMenu == game.menuList[9])
                    {
                        player2b[game.activeMenu.selectedItem] = j;
                        player2f[game.activeMenu.selectedItem] = true;
                    }
                    if (game.activeMenu.selectedItem == 13)
                    {
                        game.particleEngine.StopEffects();
                        isSetting = !isSetting;
                    }
                    else
                        game.activeMenu.selectedItem++;
                }
                if (thumbState.HasLeft && (thumbState.Left.X != thumbState.Left.Y))
                {
                    if (game.activeMenu == game.menuList[8])
                    {
                        if (thumbState.Left.X < -0.5f && lastSticks.Left.X > -0.5f)     // 
                        {
                            b[game.activeMenu.selectedItem] = -15;
                            f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.X > 0.5f && lastSticks.Left.X < 0.5f)
                        {
                            b[game.activeMenu.selectedItem] = -16;
                            f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.Y < -0.5f && lastSticks.Left.Y > -0.5f)
                        {
                            b[game.activeMenu.selectedItem] = -13;
                            f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.Y > 0.5f && lastSticks.Left.Y < 0.5f)
                        {
                            b[game.activeMenu.selectedItem] = -14;
                            f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                    }
                    else if (game.activeMenu == game.menuList[9])
                    {
                        if (thumbState.Left.X > -0.5f && lastSticks.Left.X < -0.5f)
                        {
                            player2b[game.activeMenu.selectedItem] = 99;
                            player2f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.X > 0.5f && lastSticks.Left.X < 0.5f)
                        {
                            player2b[game.activeMenu.selectedItem] = 98;
                            player2f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.Y > -0.5f && lastSticks.Left.Y < -0.5f)
                        {
                            player2b[game.activeMenu.selectedItem] = 97;
                            player2f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                        if (thumbState.Left.Y > 0.5f && lastSticks.Left.Y < 0.5f)
                        {
                            player2b[game.activeMenu.selectedItem] = 96;
                            player2f[game.activeMenu.selectedItem] = true;
                            hasMoved = false;
                        }
                    }
                    if (game.activeMenu.selectedItem == 13)
                    {
                        game.particleEngine.StopEffects();
                        isSetting = !isSetting;
                    }
                    else
                    {
                        if (!hasMoved)
                        {
                            game.activeMenu.selectedItem++;
                            hasMoved = true;
                        }
                    }
                }
            }
            lastKeyState = currentKeyState;
            lastButtonState = buttonState;
            lastSticks = thumbState;
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (game.isDrawInput)
            {
                double timePassedSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
                if(imageState == ImageState.Starting || imageState == ImageState.Ending)
                    fadeProgress += timePassedSinceLastFrame / 400;


                CheckInput();

                if (fadeProgress >= 1.0)
                {
                    fadeProgress = 0.0;
                    if (imageState == ImageState.Starting)
                        imageState = ImageState.Active;
                    if (imageState == ImageState.Ending)
                        imageState = ImageState.Inactive;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (game.isDrawInput)
            {
                float smoothedProgress = 0;
                if (imageState != ImageState.Starting && imageState != ImageState.Ending)
                    smoothedProgress = 1.0f;
                else
                    smoothedProgress = MathHelper.SmoothStep(0, 1, (float)fadeProgress);
                float horPosition = (float)horPosition1;
                float selectedVerPosition = RetrievePositions(imageState);
                float oldVerPosition = RetrievePositions(oldImageState);


                currentTexture = RetrieveTexture(selectedItem);
                oldTexture = RetrieveTexture(oldItem);

                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                spriteBatch.Draw(currentTexture, new Rectangle((int)horPosition, (int)selectedVerPosition, (int)(game.m_Width * 0.5f), (int)(game.m_Height / 3)), new Color(Color.White, smoothedProgress));
                spriteBatch.Draw(oldTexture, new Rectangle((int)horPosition, (int)oldVerPosition, (int)(game.m_Width * 0.5f), (int)(game.m_Height / 3)), new Color(Color.White, 1.0f - smoothedProgress));
                spriteBatch.DrawString(game.menuFont, "Press C to see new controller image", new Vector2(game.m_Width - 450, game.m_Height - 100), new Color(Color.White, 0.5f));
                spriteBatch.DrawString(game.menuFont, "Press TAB to change input settings", new Vector2(game.m_Width - 450, game.m_Height - 170), new Color(Color.White, 0.5f));
                spriteBatch.DrawString(game.menuFont, "Change Input Mode", new Vector2(game.m_Width / 2, game.m_Height - 100), new Color(Color.White, 0.5f));
                int incrementer = game.m_Height / 12;
                if (game.activeMenu == game.menuList[8])
                {
                    string s = "";
                    for (int i = 0; i < k.Length; i++)
                    {
                        if (f[i])
                            s = b[i].ToString();
                        else
                            s = k[i].ToString();
                        if (game.activeMenu.selectedItem < 10)
                        {
                            spriteBatch.DrawString(game.menuFont, s, new Vector2(game.m_Width / 4, incrementer * (i + 1)), Color.White);
                            if (i > 8)
                                i = k.Length;
                        }
                        else
                        {
                            if (!hasDirection)
                            {
                                i += 10;
                                hasDirection = true;
                                if (f[i])
                                    s = b[i].ToString();
                                else
                                    s = k[i].ToString();
                            }
                            spriteBatch.DrawString(game.menuFont, s, new Vector2(game.m_Width / 4, incrementer * ((i - 10) + 1)), Color.White);
                        }
                    }
                }
                else if (game.activeMenu == game.menuList[9])
                {
                    string s = "";
                    for (int i = 0; i < player2k.Length; i++)
                    {
                        if (player2f[i])
                            s = player2b[i].ToString();
                        else
                            s = player2k[i].ToString();
                        if (game.activeMenu.selectedItem < 10)
                        {
                            spriteBatch.DrawString(game.menuFont, s, new Vector2(game.m_Width / 4, incrementer * (i + 1)), Color.White);
                            if (i > 8)
                                i = k.Length;           // cuts off the draw so we don't see the list run off the screen
                        }
                        else
                        {
                            if (!hasDirection)
                            {
                                i += 10;
                                hasDirection = true;
                                if (player2f[i])
                                    s = b[i].ToString();
                                else
                                    s = k[i].ToString();
                            }
                            spriteBatch.DrawString(game.menuFont, s, new Vector2(game.m_Width / 4, incrementer * ((i - 10) + 1)), Color.White);
                        }
                    }
                }
                hasDirection = false;
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private Texture2D RetrieveTexture(int imageHash)
        {
            Texture2D hold = NESTexture;
            switch (imageHash)
            {
                case 0:
                    hold = NESTexture;
                    break;
                case 1:
                    hold = SNESTexture;
                    break;
                case 2:
                    hold = genesisTexture;
                    break;
            }
            return hold;
        }

        private float RetrievePositions(ImageState state)
        {
            float smoothedProgress = MathHelper.SmoothStep(0, 1, (float)fadeProgress);
            float verPosition = (float)verPosition1;
            switch (state)
            {
                case ImageState.Starting:
                    verPosition -= 600 * (1.0f - (float)smoothedProgress);

                    break;

                case ImageState.Ending:
                    verPosition += 600 * ((float)smoothedProgress);

                    break;
                case ImageState.Active:
                    verPosition = verPosition1;
                    break;

                case ImageState.Inactive:
                    verPosition = 0;
                    break;
            }
            return verPosition;
        }
    }
}
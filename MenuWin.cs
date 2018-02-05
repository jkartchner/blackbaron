using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class MenuWin: MenuWindow
{
        public enum ImageState { Starting, Active, Ending, Inactive }
        public ImageState imageState;
        public ImageState arrowState;
        public Texture2D screenShot;
        public Texture2D photoFile;
        public Texture2D arrowFile;
        public Texture2D triangleFile;
        public List<Texture2D> screenshotList;
        public List<Texture2D> animationList;
        public List<string> descriptionList;
        public string screenshotPath;
        public string photoPath;
        public ContentManager Content;
        private double changedProgress;
        private double arrowProgress = 0;
        private int selectedSectionChange = 0;
        private int arrowShowInt = 0;
        private bool isUpable = false;
        private bool isDownable = false;
        private bool isArrowable = false;
        public float sprLayerDepth;
        public Color sprColor;

        private int scrnShotWidth = 0;
        private int scrnShotHeight = 0;
        private int scrnShotHorPos = 0;
        private int scrnShotVertPos = 0;


        public MenuWin(SpriteFont spriteFont, int transitionCode, string menuTitle, float horPosition, int verPosition, Game1 game)
        {
            this.horPosition1 = horPosition;
            this.verPosition1 = verPosition;
            itemList = new List<MenuItem>();
            changeSpan = TimeSpan.FromMilliseconds(300);
            itemChangeSpan = TimeSpan.FromMilliseconds(300);
            scrollChangeSpan = TimeSpan.FromMilliseconds(350);
            selectedItem = 0;
            changeProgress = 0;
            itemChangeProgress = 0;
            windowState = WindowState.Inactive;
            itemState = ItemState.Inactive;
            arrowState = ImageState.Starting;
            transCode = transitionCode;
            this.menuTitle = menuTitle;
            this.spriteFont = spriteFont;
            isRomMenu = false;
            if (menuTitle == "Exit")
                this.menuVertOffset = verPosition1 * 3;
            triangleFile = game.Content.Load<Texture2D>("WhiteTriangleArrow");
            arrowFile = game.Content.Load<Texture2D>("Arrow_green");

            this.game = game;
        }

        public MenuWin(SpriteFont spriteFont, int transitionCode, string menuTitle, ContentManager content, float horPosition, int verPosition, Game1 game)
        {
            this.game = game;
            this.horPosition1 = horPosition;
            this.verPosition1 = verPosition;
            itemList = new List<MenuItem>();
            screenshotList = new List<Texture2D>();
            animationList = new List<Texture2D>();
            descriptionList = new List<string>();
            changeSpan = TimeSpan.FromMilliseconds(300);
            itemChangeSpan = TimeSpan.FromMilliseconds(300);
            scrollChangeSpan = TimeSpan.FromMilliseconds(350);
            selectedItem = 0;
            changeProgress = 0;
            itemChangeProgress = 0;
            windowState = WindowState.Inactive;
            itemState = ItemState.Inactive;
            arrowState = ImageState.Starting;
            transCode = transitionCode;
            this.menuTitle = menuTitle;
            this.spriteFont = spriteFont;
            isRomMenu = true;
            changedProgress = 0;
            Content = content;

            arrowFile = Content.Load<Texture2D>("Arrow_green");
            triangleFile = Content.Load<Texture2D>("WhiteTriangleArrow");

            if (menuTitle == "Exit")
                this.menuVertOffset = verPosition1 * 3;
        }

        public MenuWin(SpriteFont spriteFont, int transitionCode, bool isBloom, string menuTitle, float horPosition, int verPosition, Game1 game)
        {
            this.horPosition1 = horPosition;
            this.verPosition1 = verPosition;
            itemList = new List<MenuItem>();
            changeSpan = TimeSpan.FromMilliseconds(300);
            itemChangeSpan = TimeSpan.FromMilliseconds(300);
            scrollChangeSpan = TimeSpan.FromMilliseconds(350);
            selectedItem = 0;
            changeProgress = 0;
            itemChangeProgress = 0;
            windowState = WindowState.Inactive;
            itemState = ItemState.Inactive;
            arrowState = ImageState.Starting;
            transCode = transitionCode;
            this.menuTitle = menuTitle;
            this.spriteFont = spriteFont;
            isRomMenu = false;
            isSysMenu = false;
            isBloom = true;
            if (menuTitle == "Exit")
                this.menuVertOffset = verPosition1 * 3;

            triangleFile = game.Content.Load<Texture2D>("WhiteTriangleArrow");
            arrowFile = game.Content.Load<Texture2D>("Arrow_green");

            this.game = game;
        }

        public MenuWin(SpriteFont spriteFont, int transitionCode, string menuTitle, ContentManager content, float horPosition, int verPosition, bool fIsSystem, Game1 game)
        {
            this.horPosition1 = horPosition;
            this.verPosition1 = verPosition;
            itemList = new List<MenuItem>();
            screenshotList = new List<Texture2D>();
            animationList = new List<Texture2D>();
            descriptionList = new List<string>();
            changeSpan = TimeSpan.FromMilliseconds(300);
            itemChangeSpan = TimeSpan.FromMilliseconds(300);
            scrollChangeSpan = TimeSpan.FromMilliseconds(350);
            selectedItem = 0;
            changeProgress = 0;
            itemChangeProgress = 0;
            windowState = WindowState.Inactive;
            itemState = ItemState.Inactive;
            arrowState = ImageState.Starting;
            transCode = transitionCode;
            this.menuTitle = menuTitle;
            this.spriteFont = spriteFont;
            isRomMenu = false;
            isSysMenu = true;
            changedProgress = 0;
            Content = content;

            arrowFile = Content.Load<Texture2D>("Arrow_green");
            triangleFile = Content.Load<Texture2D>("WhiteTriangleArrow");

            if (menuTitle == "Exit")
                this.menuVertOffset = verPosition1 * 3;

            this.game = game;
        }

        private void CalcArrowHandling()
        {
            // calculate the selected value range for the selectedItem to see when to display the down and up arrows
            if (itemList.Count >= 10)
            {
                int holder = itemList.Count % 10;
                arrowShowInt = itemList.Count - holder;

                isArrowable = true;
            }
        }

        public override void WakeUp()
        {
            imageState = ImageState.Starting;
            base.WakeUp();
        }

        public override void AddMenuItem(string itemText, MenuWindow itemLink)
        {
            if (isSysMenu)
            {
                switch (itemText)
                {
                    case "Super Nintendo":
                        screenShot = Content.Load<Texture2D>("Systems//SuperNintendoLogo");
                        screenshotList.Add(screenShot);
                        break;

                    case "Atari 2600":
                        screenShot = Content.Load<Texture2D>("Systems/Atari");
                        screenshotList.Add(screenShot);
                        break;

                    case "Genesis":
                        screenShot = Content.Load<Texture2D>("Systems/GenesisLogo");
                        screenshotList.Add(screenShot);
                        break;

                    case "Nintendo":
                        screenShot = Content.Load<Texture2D>("Systems/NintendoLogo");
                        screenshotList.Add(screenShot);
                        break;

                    case "MAME":
                        screenShot = Content.Load<Texture2D>("Systems/MAMELogo");
                        screenshotList.Add(screenShot);
                        break;
                }
            }
            // calculate the selected value range for the selectedItem to see when to display the down and up arrows
            CalcArrowHandling();

            base.AddMenuItem(itemText, itemLink);
        }

        public override void AddMenuItem(string itemText, MenuWindow itemLink, string animPhotoPath, string screenPath, string description)
        {
            if (isRomMenu)
            {
                screenshotPath = screenPath;
                photoPath = animPhotoPath;

                    screenShot = Content.Load<Texture2D>(screenshotPath);
                    screenshotList.Add(screenShot);

                    photoFile = Content.Load<Texture2D>(photoPath);
            
                animationList.Add(photoFile);
                    descriptionList.Add(description);
            }
            // calculate the selected value range for the selectedItem to see when to display the down and up arrows
            CalcArrowHandling();
            base.AddMenuItem(itemText, itemLink);
        }

        public override MenuWindow ProcessInput(KeyboardState lastKeybState, KeyboardState currentKeybState,
            ButtonState[] lastButtonState, ButtonState[] currentButtonState, DirectInputThumbSticks lastTSticks, DirectInputThumbSticks currentTSticks, GameTime gameTime)
        {
            if (isArrowable)
            {
                if (selectedItem >= arrowShowInt)
                    isDownable = false;
                else
                    isDownable = true;
                if (selectedItem < 10)
                    isUpable = false;
                else
                    isUpable = true;
            }
            if (isRomMenu || isSysMenu)
            {
                // avoid this one on the system menu
                if (!isSysMenu)
                {
                    if ((lastKeybState.IsKeyUp(Keys.Left) && currentKeybState.IsKeyDown(Keys.Left)) ||
                        (currentTSticks.Left.X < -0.5f && lastTSticks.Left.X > -0.5f) || (lastButtonState[1] == ButtonState.Released && currentButtonState[1] == ButtonState.Pressed))
                    {
                        imageState = ImageState.Ending;
                        return this;
                    }
                }

                if ((lastKeybState.IsKeyUp(Keys.Right) && currentKeybState.IsKeyDown(Keys.Right)) ||
                    (currentTSticks.Left.X > 0.5f && lastTSticks.Left.X < 0.5f))
                {
                    windowState = WindowState.Ending;
                    imageState = ImageState.Ending;

                    return itemList[selectedItem].itemLink;
                }

                if ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)) ||
                    (lastButtonState[0] == ButtonState.Released && currentButtonState[0] == ButtonState.Pressed) && game.activeMenu != game.menuList[4] && game.activeMenu != game.menuList[5]
                     && game.activeMenu != game.menuList[6] && game.activeMenu != game.menuList[7] && game.activeMenu != game.menuList[8] && game.activeMenu != game.menuList[9])
                {
                    windowState = WindowState.Ending;
                    imageState = ImageState.Ending;

                    return itemList[selectedItem].itemLink;
                }
                else if (lastKeybState.IsKeyDown(Keys.Escape) || lastButtonState[8] == ButtonState.Pressed)
                    imageState = ImageState.Ending;
            }
            return base.ProcessInput(lastKeybState, currentKeybState, lastButtonState, currentButtonState, lastTSticks, currentTSticks, gameTime);
        }

        private String parseText(String text)
        {
            String line = String.Empty;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (spriteFont.MeasureString(line + word).Length() > scrnShotWidth)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }

        public override void Update(GameTime gameTime)
        {
            double timePassedSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
            if (game.activeMenu != game.menuList[2])
            {
                    // always update the arrowState
                    arrowProgress += timePassedSinceLastFrame / 1000;
                    if (arrowProgress >= 1.0f)
                    {
                        arrowProgress = 0.0f;
                        if (arrowState == ImageState.Starting)
                            arrowState = ImageState.Ending;
                        else if (arrowState == ImageState.Ending)
                            arrowState = ImageState.Starting;
                    }

                if (selectedItem != selectedSectionChange)
                    imageState = ImageState.Starting;
                if ((imageState == ImageState.Starting) || (imageState == ImageState.Ending))
                    changedProgress += timePassedSinceLastFrame / changeSpan.TotalMilliseconds;

                if (changedProgress >= 1.0f)
                {
                    changedProgress = 0.0f;
                    if (imageState == ImageState.Starting)
                        imageState = ImageState.Active;
                    else if (imageState == ImageState.Ending)
                        imageState = ImageState.Inactive;
                }
                selectedSectionChange = selectedItem;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (windowState == WindowState.Inactive)
                return;
            if (game.activeMenu != game.menuList[2])
            {
                float alphaValue = 0.0f;
                float arrowAlpha = 0.0f;
                float smoothedProgress = MathHelper.SmoothStep(0, 1, (float)changedProgress);
                float arrowSmoothed = MathHelper.SmoothStep(0, 1, (float)arrowProgress);

                switch (imageState)
                {
                    case ImageState.Starting:
                        alphaValue = smoothedProgress;
                        sprLayerDepth = 0.5f;
                        sprColor = new Color(Color.White, alphaValue);
                        break;

                    case ImageState.Ending:
                        alphaValue = 1.0f - smoothedProgress;
                        sprLayerDepth = 1.0f;
                        sprColor = new Color(Color.White, alphaValue);
                        break;

                    default:
                        if (selectedItem != selectedSectionChange)
                        {
                            alphaValue = 0.5f;
                            sprLayerDepth = 0.5f;
                            sprColor = new Color(Color.White, alphaValue);
                        }
                        alphaValue = 1;
                        sprLayerDepth = 1.0f;
                        sprColor = Color.White;
                        break;
                }

                switch (arrowState)
                {
                    case ImageState.Starting:
                        arrowAlpha = arrowSmoothed;
                        break;

                    case ImageState.Ending:
                        arrowAlpha = 1.0f - arrowSmoothed;
                        break;
                }
                if (scrnShotHeight == 0 || scrnShotWidth == 0)
                {
                    scrnShotWidth = (int)((horPosition1 * 8) * 0.525);
                    scrnShotHeight = (int)((verPosition1 * 12) * 0.63333);
                    scrnShotHorPos = (int)(horPosition1 * 3);
                    scrnShotVertPos = verPosition1 / 2;

                }
                if(isRomMenu || isSysMenu)
                    spriteBatch.Draw(screenshotList.ElementAt<Texture2D>(selectedItem), new Rectangle(scrnShotHorPos, scrnShotVertPos, scrnShotWidth, scrnShotHeight), null, sprColor, 0, Vector2.Zero, SpriteEffects.None, sprLayerDepth);
                if (isRomMenu)
                {
                    spriteBatch.Draw(animationList.ElementAt<Texture2D>(selectedItem), new Rectangle((int)((horPosition1 * 8) * 0.85), (int)((verPosition1 * 12) * 0.85), 135, 35), null, sprColor, 0, Vector2.Zero, SpriteEffects.None, sprLayerDepth);
                    string text = parseText(descriptionList.ElementAt<string>(selectedItem));
                    spriteBatch.DrawString(spriteFont, text, new Vector2(scrnShotHorPos, scrnShotVertPos + scrnShotHeight + 25), new Color(Color.White, alphaValue));
                }
                if (windowState == WindowState.Active)
                {
                    int magicNumber = selectedItem % 10;
                    if (magicNumber == 0)
                    {
                        magicNumber = selectedItem;
                        if (selectedItem > 0)
                            magicNumber = 0;
                    }
                    if(!isSysMenu)
                        spriteBatch.Draw(triangleFile, new Rectangle((int)((horPosition1 - 20) + (5 * arrowAlpha)), (magicNumber * verPosition1) + 120, 10, 10), null, new Color(Color.White, arrowAlpha), 3.14159f, Vector2.Zero, SpriteEffects.None, sprLayerDepth);
                    if (game.activeMenu != game.menuList[1] && game.activeMenu != game.menuList[5] && game.activeMenu != game.menuList[8] && game.activeMenu != game.menuList[9])
                    {
                        if (game.activeMenu != game.menuList[4] && game.activeMenu != game.menuList[6])
                            spriteBatch.Draw(triangleFile, new Rectangle((int)((horPosition1 + 40) - (5 * arrowAlpha)) + (int)(spriteFont.MeasureString(itemList[selectedItem].itemText).Length() * 1.2), ((magicNumber % 10) * verPosition1) + 112, 10, 10), null, new Color(Color.White, arrowAlpha), 0, Vector2.Zero, SpriteEffects.None, sprLayerDepth);
                        else if(selectedItem < 1)
                            spriteBatch.Draw(triangleFile, new Rectangle((int)((horPosition1 + 40) - (5 * arrowAlpha)) + (int)(spriteFont.MeasureString(itemList[selectedItem].itemText).Length() * 1.2), ((magicNumber % 10) * verPosition1) + 112, 10, 10), null, new Color(Color.White, arrowAlpha), 0, Vector2.Zero, SpriteEffects.None, sprLayerDepth);
                    }
                }
                if (isDownable)
                    spriteBatch.Draw(arrowFile, new Rectangle((int)horPosition1 + 100, (int)(verPosition1 * 11), 25, 25), null, new Color(Color.White, arrowAlpha), 0, Vector2.Zero, SpriteEffects.None, 1.0f);
                if(isUpable)
                    spriteBatch.Draw(arrowFile, new Rectangle((int)horPosition1 + 100, 30, 25, 25), null, new Color(Color.White, arrowAlpha), 3.14159f, new Vector2(50, 50), SpriteEffects.None, 1.0f);
            }

            base.Draw(spriteBatch, gameTime);
        }
    }
}

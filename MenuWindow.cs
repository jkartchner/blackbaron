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
    public abstract class MenuWindow
    {
        public struct MenuItem
        {
            public string itemText;
            public MenuWindow itemLink;

            public MenuItem(string itemText, MenuWindow itemLink)
            {
                this.itemText = itemText;
                this.itemLink = itemLink;
            }
        }
        public Game1 game;

        public enum WindowState { Starting, Active, Ending, Inactive }
        public enum ItemState { Starting, Active, Ending, Inactive }
        public enum ScrollState { ScrollUp, ScrollDown, Inactive }
        public TimeSpan changeSpan;
        public TimeSpan itemChangeSpan;
        /// <summary>
        /// the speed at which the auto scroll occurs
        /// </summary>
        public TimeSpan scrollChangeSpan;
        public WindowState windowState;
        public ItemState itemState;
        public ScrollState scrollState;
        public List<MenuItem> itemList;
        public int selectedItem;
        public int sysItem = 0;
        public int oldSelectedItem;
        public SpriteFont spriteFont;
        public double changeProgress;
        public double itemChangeProgress;
        public double scrollChangeProgress;
        private double holdKeyProgress = 0;
        private double holdKeySpacingProgress = 0;
        public bool isRomMenu;
        public bool isSysMenu = false;
        public bool isBloom = false;

        public int transCode;
        public string menuTitle;
        public bool isExit = false;
        public int verPosition1 = 100;
        public float horPosition1 = 100;
        public int menuVertOffset = 0;
        public double holdKeySpeed = 90;
        public double holdKeySpacingProgressRamped;
        public double holdKeySpeedRamped = 65;
        public double holdKeyProgressRamped;
        public double horParticle = 0.0;
        public Vector2 menuPosition;



        public virtual void AddMenuItem(string itemText, MenuWindow itemLink)
        {
            MenuItem newItem = new MenuItem(itemText, itemLink);
            itemList.Add(newItem);
        }

        public virtual void AddMenuItem(string itemText, MenuWindow itemLink, string animPhotoPath, string screenPath, string description)
        {
            MenuItem newItem = new MenuItem(itemText, itemLink);
            itemList.Add(newItem);
        }

        public virtual void AddMenuItem(string itemText, MenuWindow itemLink, int actionInteger)
        {
            MenuItem newItem = new MenuItem(itemText, itemLink);
            itemList.Add(newItem);
        }

        public void Clear()
        {
            itemList.Clear();
        }

        public virtual void WakeUp()
        {
            windowState = WindowState.Starting;
            itemState = ItemState.Starting;
            scrollState = ScrollState.Inactive;
        }

        public virtual void WakeUp(int newIndex, string sysPath, string romPath)
        {
            windowState = WindowState.Starting;
            itemState = ItemState.Starting;
            selectedItem = newIndex;
        }

        public virtual MenuWindow ProcessInput(KeyboardState lastKeybState, KeyboardState currentKeybState,
            ButtonState[] lastButtonState, ButtonState[] currentButtonState, DirectInputThumbSticks lastTSticks, DirectInputThumbSticks currentTSticks, GameTime gameTime)
        {
            oldSelectedItem = selectedItem;

            // check keyboard states to see if a key has been held down long enough for autoscroll
            if ((currentTSticks.Left.Y > 0.5f && lastTSticks.Left.Y > 0.5f) || currentKeybState.IsKeyDown(Keys.Down) && 
                lastKeybState.IsKeyDown(Keys.Down))
            {
                if (game.inputControl.isSetting)
                    return this;
                bool holder = true;
                if (holdKeyProgressRamped >= 1.0f && holdKeySpacingProgressRamped >= 1.0f)
                {
                    selectedItem++;
                    holdKeySpacingProgressRamped = 0.0f;
                    holder = false;
                }
                else if (holdKeyProgress >= 1.0f && holdKeySpacingProgress >= 1.0f && holder)
                {
                    selectedItem++;
                    holdKeySpacingProgress = 0.0f;
                }
            }

            // check for someone holding the up direction
            if ((currentTSticks.Left.Y < -0.5f && lastTSticks.Left.Y < -0.5f) || currentKeybState.IsKeyDown(Keys.Up) && lastKeybState.IsKeyDown(Keys.Up))
            {
                if (game.inputControl.isSetting)
                    return this;
                if (holdKeyProgressRamped >= 1.0f && holdKeySpacingProgressRamped >= 1.0f)
                {
                    selectedItem--;
                    holdKeySpacingProgressRamped = 0.0f;
                    
                }
                else if (holdKeyProgress >= 1.0f && holdKeySpacingProgress >= 1.0f)
                {
                    selectedItem--;
                    holdKeySpacingProgress = 0.0f;
                }
            }

            // check for someone pressing (not holding) down
            if ((currentTSticks.Left.Y > 0.5f && lastTSticks.Left.Y < 0.5f) || lastKeybState.IsKeyUp(Keys.Down) &&
                currentKeybState.IsKeyDown(Keys.Down))
            {
                if (game.inputControl.isSetting)
                    return this;
                selectedItem++;
                holdKeyProgress = 0.0f;
                holdKeyProgressRamped = 0.0f;

                if (game.logoState == Game1.LogoState.Active)
                    game.logoState = Game1.LogoState.Ending;
                game.waitProgress = 0;
            }

            // check for someone pressing (not holding) up
            if ((currentTSticks.Left.Y < -0.5f && lastTSticks.Left.Y > -0.5f) || lastKeybState.IsKeyUp(Keys.Up) && 
                currentKeybState.IsKeyDown(Keys.Up))
            {
                if (game.inputControl.isSetting)
                    return this;
                selectedItem--;
                holdKeyProgress = 0.0f;
                holdKeyProgressRamped = 0.0f;

                if (game.logoState == Game1.LogoState.Active)
                    game.logoState = Game1.LogoState.Ending;
                game.waitProgress = 0;
            }

            // handle fast scrolling (10 at a time)
            if ((lastKeybState.IsKeyUp(Keys.PageUp) && currentKeybState.IsKeyDown(Keys.PageUp))
                || currentButtonState[6] == ButtonState.Pressed && lastButtonState[6] == ButtonState.Released)
            {
                if (itemList.Count > 10 && selectedItem > 9)
                {
                    if(game.isSoundOn)
                        game.scrollSoundEffect.Play();
                    scrollState = ScrollState.ScrollDown;
                }
                if (selectedItem > 10)
                    selectedItem -= 10;
                else
                    selectedItem = 0;
            }

            if ((lastKeybState.IsKeyUp(Keys.PageDown) && currentKeybState.IsKeyDown(Keys.PageDown)) ||
                currentButtonState[7] == ButtonState.Pressed && lastButtonState[7] == ButtonState.Released)
            {
                if (itemList.Count > 10 && selectedItem < itemList.Count - (itemList.Count % 10))
                {
                    if (game.isSoundOn)
                        game.scrollSoundEffect.Play();
                    scrollState = ScrollState.ScrollUp;
                }
                if (selectedItem < itemList.Count - 11)
                    selectedItem += 10;
                else
                    selectedItem = itemList.Count - 1;
            }

            if (selectedItem < 0)
                selectedItem = 0;

            if (selectedItem >= itemList.Count)
                selectedItem = itemList.Count - 1;
            sysItem = selectedItem;

            if (((lastButtonState[0] == ButtonState.Released && currentButtonState[0] == ButtonState.Pressed) || ((lastKeybState.IsKeyUp(Keys.Enter) && 
                currentKeybState.IsKeyDown(Keys.Enter))) || (lastKeybState.IsKeyUp(Keys.Right) &&
                currentKeybState.IsKeyDown(Keys.Right) || (currentTSticks.Left.X > 0.5f && lastTSticks.Left.X < 0.5f))))
            {
                if ((game.activeMenu == game.menuList[4] || game.activeMenu == game.menuList[6]) && game.activeMenu.selectedItem > 0 && game.activeMenu.selectedItem != game.activeMenu.itemList.Count - 1)
                {
                    return this;
                }
                else if ((game.activeMenu == game.menuList[5]) && game.activeMenu.selectedItem != game.activeMenu.itemList.Count - 1)
                    return this;
                else if (game.activeMenu == game.menuList[8] || game.activeMenu == game.menuList[9])
                    return this;
                else
                {
                    windowState = WindowState.Ending;
                    return itemList[selectedItem].itemLink;
                }
            }
            else if (lastKeybState.IsKeyUp(Keys.Escape) && currentKeybState.IsKeyDown(Keys.Escape) || lastButtonState[8] == ButtonState.Released && currentButtonState[8] == ButtonState.Pressed)
                return null;

            else
                return this;
        }

        public virtual void Update(GameTime gameTime)
        {
            double timePassedSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
            // update how long a key has been held down
            holdKeyProgress += timePassedSinceLastFrame / 400;
            holdKeyProgressRamped += timePassedSinceLastFrame / 2000;
            holdKeySpacingProgress += timePassedSinceLastFrame / holdKeySpeed;
            holdKeySpacingProgressRamped += timePassedSinceLastFrame / holdKeySpeedRamped;

            // first deal with menu item changes
            if (oldSelectedItem != selectedItem)
            {
                itemState = ItemState.Starting;
                if (selectedItem != 0 && (selectedItem % 10 == 0 && selectedItem - 1 == oldSelectedItem))
                {
                    if (game.isSoundOn)
                        game.scrollSoundEffect.Play();
                    scrollState = ScrollState.ScrollUp;
                }
                else if (oldSelectedItem % 10 == 0 && selectedItem == oldSelectedItem - 1)
                {
                    if (game.isSoundOn)
                        game.scrollSoundEffect.Play();
                    scrollState = ScrollState.ScrollDown;
                }
            }
            // calculate any change progress
            if ((scrollState == ScrollState.ScrollDown) || (scrollState == ScrollState.ScrollUp))            
                scrollChangeProgress += timePassedSinceLastFrame / scrollChangeSpan.TotalMilliseconds;

            if (scrollChangeProgress >= 1.0f)
            {
                scrollChangeProgress = 0.0f;
                if (scrollState == ScrollState.ScrollDown)
                    scrollState = ScrollState.Inactive;
                else if (scrollState == ScrollState.ScrollUp)
                    scrollState = ScrollState.Inactive;
            }
            // calculate item focus progress
            if ((itemState == ItemState.Starting) || (itemState == ItemState.Ending))
                itemChangeProgress += timePassedSinceLastFrame / itemChangeSpan.TotalMilliseconds;

            if (itemChangeProgress >= 1.0f)
            {
                itemChangeProgress = 0.0f;
                if (itemState == ItemState.Starting)
                    itemState = ItemState.Active;
                else if (itemState == ItemState.Ending)
                    itemState = ItemState.Inactive;
            }

            // calculate window change progress
            if ((windowState == WindowState.Starting) || (windowState == WindowState.Ending))
                changeProgress += timePassedSinceLastFrame / changeSpan.TotalMilliseconds;

            if (changeProgress >= 1.0f)
            {
                changeProgress = 0.0f;
                if (windowState == WindowState.Starting)
                    windowState = WindowState.Active;
                else if (windowState == WindowState.Ending)
                    windowState = WindowState.Inactive;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (windowState == WindowState.Inactive)
                return;

            float smoothedProgress = MathHelper.SmoothStep(0, 1, (float)changeProgress);
            float itemSmoothedProgress = MathHelper.SmoothStep(1.0f, 1.2f, (float)itemChangeProgress);
            float scrollSmoothedProgress = MathHelper.SmoothStep((float)verPosition1, verPosition1 * 10, (float)scrollChangeProgress);

            float horPosition = horPosition1;
            int verPosition = verPosition1 + menuVertOffset;
            float alphaValue = 0.0f;
            float itemScaleValue = 1.0f;
            float scrollScalevalue = 0.0f;

            switch (scrollState)
            {
                case ScrollState.ScrollUp:
                    scrollScalevalue = (float)scrollSmoothedProgress;
                    break;

                case ScrollState.ScrollDown:
                    scrollScalevalue = (verPosition1 * 10) - ((float)scrollSmoothedProgress - verPosition1);
                    break;

                default:
                    scrollChangeProgress = 0.0f;
                    break;
            }

            switch (itemState)
            {
                case ItemState.Starting:
                    itemScaleValue = itemSmoothedProgress;
                    break;
                case ItemState.Ending:
                    itemScaleValue = 1.2f - (itemSmoothedProgress - 1.0f);
                    break;
                case ItemState.Active:
                    itemScaleValue = 1.2f;
                    break;
                case ItemState.Inactive:
                    itemScaleValue = 1.0f;
                    break;
            }

            switch (windowState)
            {
                case WindowState.Starting:
                    switch (transCode)
                    {
                        case 0:
                            horPosition -= 200 * (1.0f - (float)smoothedProgress);
                            alphaValue = smoothedProgress;
                            break;

                        case 1:
                            horPosition += 200 * (1.0f - (float)smoothedProgress);
                            alphaValue = smoothedProgress;
                            break;
                    }

                    break;

                case WindowState.Ending:
                    switch (transCode)
                    {
                        case 0:
                            horPosition += 200 * (float)smoothedProgress;
                            alphaValue = 1.0f - smoothedProgress;
                            break;

                        case 1:
                            horPosition -= 200 * (float)smoothedProgress;
                            alphaValue = 1.0f - smoothedProgress;
                            break;
                    }
                    break;

                case WindowState.Inactive:
                    alphaValue = 0;
                    break;

                default:
                    alphaValue = 1;
                    break;
            }

            spriteBatch.DrawString(spriteFont, menuTitle, new Vector2(horPosition - (float)(horPosition1 * 0.25), verPosition1 / 3 + menuVertOffset), Color.Maroon, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);

            switch(scrollState)
            {
                case ScrollState.ScrollUp:
                    // draw 20 items and show only those that are being scrolled
                    verPosition -= (int)scrollScalevalue;
                    int a = (selectedItem - 10) - (selectedItem % 10);
                    for (int itemID = a; itemID < itemList.Count && itemID < a + 20; itemID++)
                    {
                        Vector2 itemPosition = new Vector2(horPosition, verPosition);
                        Color itemColor = Color.White;
                        if (itemID == selectedItem)
                        {
                            itemColor = new Color(new Vector4(1, 0, 0, alphaValue));                            // draw selected item red
                            if (verPosition < (verPosition1 * 10) && verPosition > verPosition1)
                                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText, itemPosition,
                                    itemColor, 0, Vector2.Zero, itemScaleValue, SpriteEffects.None, 0);
                            //game.particleEngine.AddWaterEther(itemPosition + new Vector2(70, 0), 150, 10, 350, gameTime);
                            
                        }

                        else
                        {
                            itemColor = new Color(new Vector4(1, 1, 1, alphaValue));                            // else draw the menu item white
                            if (verPosition < (verPosition1 * 10) && verPosition > verPosition1)
                                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText,
                                    itemPosition, itemColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        }
                        verPosition += verPosition1;                        
                        if (itemID > a + 20)
                            break;
                    }
                    //game.particleEngine.AddWaterEther(itemPosition, 50, 10, 350, gameTime);
                    break;

                case ScrollState.ScrollDown:
                    // draw 20 items and show only those that are being scrolled
                    verPosition -= (int)scrollScalevalue - verPosition1;
                    int b = 0;
                    if (selectedItem >= 10)
                        b = (selectedItem - 10) - (selectedItem % 10) + 1;    // (verPosition - (((selectedItem - (selectedItem % 10)) * 40) - 100));
                    else
                        b = 0;
                    for (int itemID = b; itemID < itemList.Count && itemID < b + 20; itemID++)
                    {
                        Vector2 itemPosition = new Vector2(horPosition, verPosition);
                        Color itemColor = Color.White;
                        if (itemID == selectedItem)
                        {
                            itemColor = new Color(new Vector4(1, 0, 0, alphaValue));
                            if (verPosition < ((10 * verPosition1)) && verPosition > verPosition1)
                                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText, itemPosition,
                                    itemColor, 0, Vector2.Zero, itemScaleValue, SpriteEffects.None, 0);
                            //game.particleEngine.AddWaterEther(itemPosition + new Vector2(70, 0), 150, 10, 350, gameTime);
                        }

                        else
                        {
                            itemColor = new Color(new Vector4(1, 1, 1, alphaValue));
                            if (verPosition < ((10 * verPosition1)) && verPosition > verPosition1)
                                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText,
                                    itemPosition, itemColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        }
                        verPosition += verPosition1;
                        if (itemID > b + 20)
                            break;
                    }
                    break;

                default:
                    // write only 10 items to the screen by always making the for loop run on a number divisible by 10
                    int h = (selectedItem - (selectedItem % 10));  // the last half calculates the row number displayed on the screen (1-10)
                    for (int itemID = h; itemID < itemList.Count && itemID < h + 10; itemID++)
                    {
                        Vector2 itemPosition = new Vector2(horPosition, verPosition);
                        Color itemColor = Color.White;
                        if (itemID == selectedItem)
                        {
                            itemColor = new Color(new Vector4(1, 0, 0, alphaValue));
                            spriteBatch.DrawString(spriteFont, itemList[itemID].itemText, itemPosition,
                                itemColor, 0, Vector2.Zero, itemScaleValue, SpriteEffects.None, 0);
                            menuPosition = itemPosition;
                        }

                        else
                        {
                            itemColor = new Color(new Vector4(1, 1, 1, alphaValue));
                            spriteBatch.DrawString(spriteFont, itemList[itemID].itemText,
                                itemPosition, itemColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        }
                        verPosition += verPosition1;
                        if (itemID > h + 10)
                            break;
                    }
                    break;
            }
        }
    }
}

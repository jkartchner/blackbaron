using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    class GameWin: MenuWindow
    {
        public GameWin(SpriteFont spriteFont, string menuTitle, int transitionCode, Game1 game)
        {
            itemList = new List<MenuItem>();
            changeSpan = TimeSpan.FromMilliseconds(300);
            itemChangeSpan = TimeSpan.FromMilliseconds(300);
            scrollChangeSpan = TimeSpan.FromMilliseconds(350);
            selectedItem = 0;
            changeProgress = 0;
            windowState = WindowState.Inactive;
            transCode = transitionCode;
            this.menuTitle = menuTitle;
            this.spriteFont = spriteFont;

            this.game = game;
        }

        public override void WakeUp(int newIndex, string sysPath, string romPath)
        {
            switch (newIndex)
            {
                case 0:
                    string[] args = { romPath };
                    //ProcessClass p = new ProcessClass(sysPath, args);
                    game.endScreen.args = args;
                    game.endScreen.sysPath = sysPath;
                    if(game.isSoundOn)
                        game.guitarSoundEffect.Play();
                    game.bloomSettingsIndex = 3;
                    game.bloom.Visible = true;
                    game.State = Game1.GameState.EndScreen;
                    break;

                case 1:

                    break;

                case 2:

                    break;

                case 3:
                    if (game.isSoundOn)
                        game.guitarSoundEffect.Play();
                    game.bloomSettingsIndex = 3;
                    game.bloom.Visible = true;
                    game.State = Game1.GameState.EndScreen;
                    break;
                    
                case 4:
                    TextWriter tr = new StreamWriter("Morse.txt");
                    try
                    {
                        // read from file or write to file
                        tr.WriteLine("[Action]");
                        tr.WriteLine("ReEnter Systems");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error writing rom initialization: " + ex.ToString());
                    }
                    finally
                    {
                        tr.Close();
                    }
                    MessageBox.Show("Reopen BlackBaron to reenter system information, Dawg!");
                    this.isExit = true;
                    break;
                case 6:
                    game.State = Game1.GameState.SaveScreen;
                    game.isDisplaySysInfo = true;
                    break;
            }

            base.WakeUp();
        }

    }
}

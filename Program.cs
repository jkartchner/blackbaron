using System;
using System.IO;
using System.Windows.Forms;

namespace BlackBaron
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            TextReader tr = new StreamReader("Morse.txt");
            string s = "";
            try
            {
                // read from file or write to file
                s = tr.ReadLine();
                if (s == "[Action]")
                {
                    s = tr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing rom initialization: " + ex.ToString());
            }
            finally
            {
                tr.Close();
            }
            //try
            {
                if (s == "ReEnter Systems")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
                if(s != "ReEnter Systems")
                {
                    using (Game1 game = new Game1())
                    {
                        game.Run();
                    }
                }
            }
            //catch(Exception ex)
            {
                //MessageBox.Show("Some king of global error, Dawg!: " + ex.ToString());
            }
        }
    }
}


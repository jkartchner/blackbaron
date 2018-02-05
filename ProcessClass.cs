using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlackBaron
{
    /// <summary>
    /// A delegate to handle potential issues with multi-threading; Can take any method with an input string parameter
    /// </summary>
    /// <param name="S"></param>
    public delegate void LoadErrorHandle(string S);

    class ProcessClass
    {
        private Process p;
        private LoadErrorHandle ThreadPush;
        private string execPath;
        private string commandArgs;
        private string[] execArgs;
        private string[] emulatorArgs;

        Game1 game;

        public ProcessClass(string execPath, string[] args, Game1 game)
        {
            this.game = game;

            this.execPath = execPath;
            this.emulatorArgs = args;
            p = new Process();
            // load start info
            p.StartInfo.FileName = execPath;
            p.StartInfo.CreateNoWindow = true;

            // redirect error output and standard response output from console
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += p_DataReceived;
            p.OutputDataReceived += p_DataReceived;

            // load execution args
            LoadArgs();
            p.StartInfo.Arguments = commandArgs;

            // load the delegate to handle error messages from the process
            // a delegate is necessary to avoid threading issues when console output 
            // is redirected and fires the p_DataReceived event; we use this 
            // delegate with the invoke() method to force the UI update onto the
            // main thread——and thus avoid the multi-threaded issue
            ThreadPush = new LoadErrorHandle(PushThread);

            // launch the game
            Launch();
        }

        void p_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // invoking the underlying thread
            //BeginInvoke(ThreadPush, new object[] { e.Data });   // use BeginInvoke because it runs asynchonously and does not wait for UI thread termination;
                                                                // an Invoke here causes a deadlock, waiting for UI return——use Invoke only for multi-threaded
        }                                                       // http://answers.yahoo.com/question/index?qid=20071030093217AAUp8yg

        private void Launch()
        {
            try
            {
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                WriteFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Emulation Error: " + ex);
            }
        }

        private void PushThread(string s)
        {
            // the file write must occur here in the delegate call;
            // because we are forced to use BeginInvoke, the UI thread prioritizes this call until after the 
            // switch button has been completely processed
            MessageBox.Show(s);
        }

        private string ConvertToEncoding(string s)
        {
            // Number Pad
            if (s == "NumPad8")
                return 72.ToString();
            if (s == "NumPad7")
                return 71.ToString();
            if (s == "NumPad9")
                return 73.ToString();
            if (s == "NumPad4")
                return 75.ToString();
            if (s == "NumPad5")
                return 76.ToString();
            if (s == "NumPad6")
                return 77.ToString();
            if (s == "NumPad+")
                return 78.ToString();
            if (s == "NumPad1")
                return 79.ToString();
            if (s == "NumPad2")
                return 80.ToString();
            if (s == "NumPad3")
                return 81.ToString();
            if (s == "NumPad0")
                return 83.ToString();
            if (s == "NumPad.")
                return 84.ToString();
            // Arrow Keys
            if (s == "Up")
                return 200.ToString();
            if (s == "Down")
                return 208.ToString();
            if (s == "Left")
                return 203.ToString();
            if (s == "Right")
                return 205.ToString();
            // Top Row
            if (s == "Q" || s == "q")
                return 16.ToString();
            if (s == "W" || s == "w")
                return 17.ToString();
            if (s == "E" || s == "e")
                return 18.ToString();
            if (s == "R" || s == "r")
                return 19.ToString();
            if (s == "T" || s == "t")
                return 20.ToString();
            if (s == "Y" || s == "y")
                return 21.ToString();
            if (s == "U" || s == "u")
                return 22.ToString();
            if (s == "I" || s == "i")
                return 23.ToString();
            if (s == "O" || s == "o")
                return 24.ToString();
            if (s == "P" || s == "p")
                return 25.ToString();
            if (s == "[")
                return 26.ToString();
            if (s == "]")
                return 27.ToString();
            if (s == @"\")
                return 28.ToString();
            // Home Row
            if (s == "A" || s == "a")
                return 30.ToString();
            if (s == "S" || s == "s")
                return 31.ToString();
            if (s == "D" || s == "d")
                return 32.ToString();
            if (s == "F" || s == "f")
                return 33.ToString();
            if (s == "G" || s == "g")
                return 34.ToString();
            if (s == "H" || s == "h")
                return 35.ToString();
            if (s == "J" || s == "j")
                return 36.ToString();
            if (s == "K" || s == "k")
                return 37.ToString();
            if (s == "L" || s == "l")
                return 38.ToString();
            if (s == ";")
                return 39.ToString();
            if (s == "'")
                return 40.ToString();
            // bottom row
            if (s == "Z" || s == "z")
                return 44.ToString();
            if (s == "X" || s == "x")
                return 45.ToString();
            if (s == "C" || s == "c")
                return 46.ToString();
            if (s == "V" || s == "v")
                return 47.ToString();
            if (s == "B" || s == "b")
                return 48.ToString();
            if (s == "N" || s == "n")
                return 49.ToString();
            if (s == "N" || s == "n")
                return 49.ToString();
            if (s == "M" || s == "m")
                return 50.ToString();
            if (s == ",")
                return 51.ToString();
            if (s == ".")
                return 52.ToString();
            if (s == "/")
                return 53.ToString();


            if (s == "Space")
                return 54.ToString();
            if (s == "Enter")
                return 28.ToString();

            else
                return 0.ToString();

        }

        private void LoadArgs()
        {
            if (execPath.Contains("zsnes") || execPath.Contains("ZSNES"))
            {
                string s = "";
                StreamReader tr = new StreamReader(execPath.Substring(0, execPath.LastIndexOf('\\') + 1) + "zinput.cfg");
                try
                {
                    string s1 = "";
                    // read from file or write to file
                    while (true)
                    {
                        s1 = tr.ReadLine();
                        if (s1 == null)
                            break;

                        // player one buttons
                        if (s1.Contains("pl1startk") )
                        {
                            if(game.inputControl.f[4])
                                s1 = "pl1startk" + "=" + (game.inputControl.b[4] + 272).ToString();
                            else
                                s1 = "pl1startk" + "=" + ConvertToEncoding(game.inputControl.k[4].ToString());
                        }
                        if (s1.Contains("pl1selk") )
                        {
                            if(game.inputControl.f[5])
                                s1 = "pl1selk" + "=" + (game.inputControl.b[5] + 272).ToString();
                            else
                                s1 = "pl1selk" + "=" + ConvertToEncoding(game.inputControl.k[5].ToString());
                        }
                        if (s1.Contains("pl1Xk") )
                        {
                            if(game.inputControl.f[3])
                                s1 = "pl1Xk" + "=" + (game.inputControl.b[3] + 272).ToString();
                            else
                                s1 = "pl1Xk" + "=" + ConvertToEncoding(game.inputControl.k[3].ToString());
                        }
                        if (s1.Contains("pl1Ak") )
                        {
                            if(game.inputControl.f[1])
                                s1 = "pl1Ak" + "=" + (game.inputControl.b[1] + 272).ToString();
                            else
                                s1 = "pl1Ak" + "=" + ConvertToEncoding(game.inputControl.k[1].ToString());
                        }
                        if (s1.Contains("pl1Bk") )
                        {
                            if(game.inputControl.f[0])
                                s1 = "pl1Bk" + "=" + (game.inputControl.b[0] + 272).ToString();
                            else
                                s1 = "pl1Bk" + "=" + ConvertToEncoding(game.inputControl.k[0].ToString());
                        }
                        if (s1.Contains("pl1Yk") )
                        {
                            if(game.inputControl.f[2])
                                s1 = "pl1Yk" + "=" + (game.inputControl.b[2] + 272).ToString();
                            else
                                s1 = "pl1Yk" + "=" + ConvertToEncoding(game.inputControl.k[2].ToString());
                        }
                        if (s1.Contains("pl1Rk") )
                        {
                            if(game.inputControl.f[6])
                                s1 = "pl1Rk" + "=" + (game.inputControl.b[6] + 272).ToString();
                            else
                                s1 = "pl1Rk" + "=" + ConvertToEncoding(game.inputControl.k[6].ToString());
                        }
                        if (s1.Contains("pl1Lk") )
                        {
                            if(game.inputControl.f[7])
                                s1 = "pl1Lk" + "=" + (game.inputControl.b[7] + 272).ToString();
                            else
                                s1 = "pl1Lk" + "=" + ConvertToEncoding(game.inputControl.k[7].ToString());
                        }

                        if (s1.Contains("pl1upk"))
                        {
                            if (game.inputControl.f[11])
                                s1 = "pl1upk" + "=" + (game.inputControl.b[11] + 272).ToString();
                            else
                                s1 = "pl1upk" + "=" + ConvertToEncoding(game.inputControl.k[11].ToString());
                        }
                        if (s1.Contains("pl1downk"))
                        {
                            if (game.inputControl.f[12])
                                s1 = "pl1downk" + "=" + (game.inputControl.b[12] + 272).ToString();
                            else
                                s1 = "pl1downk" + "=" + ConvertToEncoding(game.inputControl.k[12].ToString());
                        }
                        if (s1.Contains("pl1leftk"))
                        {
                            if (game.inputControl.f[13])
                                s1 = "pl1leftk" + "=" + (game.inputControl.b[13] + 272).ToString();
                            else
                                s1 = "pl1leftk" + "=" + ConvertToEncoding(game.inputControl.k[13].ToString());
                        }
                        if (s1.Contains("pl1rightk"))
                        {
                            if (game.inputControl.f[10])
                                s1 = "pl1rightk" + "=" + (game.inputControl.b[10] + 272).ToString();
                            else
                                s1 = "pl1rightk" + "=" + ConvertToEncoding(game.inputControl.k[10].ToString());
                        }

                        // player 2 buttons
                        if (s1.Contains("plstartk"))
                        {
                            if (game.inputControl.player2f[4])
                                s1 = "pl2startk" + "=" + (game.inputControl.player2b[4] + 272).ToString();
                            else
                                s1 = "pl2startk" + "=" + ConvertToEncoding(game.inputControl.player2k[4].ToString());
                        }
                        if (s1.Contains("pl2selk"))
                        {
                            if (game.inputControl.player2f[5])
                                s1 = "pl2selk" + "=" + (game.inputControl.player2b[5] + 272).ToString();
                            else
                                s1 = "pl2selk" + "=" + ConvertToEncoding(game.inputControl.player2k[5].ToString());
                        }
                        if (s1.Contains("pl2Xk"))
                        {
                            if (game.inputControl.player2f[3])
                                s1 = "pl2Xk" + "=" + (game.inputControl.player2b[3] + 272).ToString();
                            else
                                s1 = "pl2Xk" + "=" + ConvertToEncoding(game.inputControl.player2k[3].ToString());
                        }
                        if (s1.Contains("pl2Ak"))
                        {
                            if (game.inputControl.player2f[1])
                                s1 = "pl2Ak" + "=" + (game.inputControl.player2b[1] + 272).ToString();
                            else
                                s1 = "pl2Ak" + "=" + ConvertToEncoding(game.inputControl.player2k[1].ToString());
                        }
                        if (s1.Contains("pl2Bk"))
                        {
                            if (game.inputControl.player2f[0])
                                s1 = "pl2Bk" + "=" + (game.inputControl.player2b[0] + 272).ToString();
                            else
                                s1 = "pl2Bk" + "=" + ConvertToEncoding(game.inputControl.player2k[0].ToString());
                        }
                        if (s1.Contains("pl2Yk"))
                        {
                            if (game.inputControl.player2f[2])
                                s1 = "pl2Yk" + "=" + (game.inputControl.player2b[2] + 272).ToString();
                            else
                                s1 = "pl2Yk" + "=" + ConvertToEncoding(game.inputControl.player2k[2].ToString());
                        }
                        if (s1.Contains("pl2Rk"))
                        {
                            if (game.inputControl.player2f[6])
                                s1 = "pl2Rk" + "=" + (game.inputControl.player2b[6] + 272).ToString();
                            else
                                s1 = "pl2Rk" + "=" + ConvertToEncoding(game.inputControl.player2k[6].ToString());
                        }
                        if (s1.Contains("pl2Lk"))
                        {
                            if (game.inputControl.player2f[7])
                                s1 = "pl2Lk" + "=" + (game.inputControl.player2b[7] + 272).ToString();
                            else
                                s1 = "pl2Lk" + "=" + ConvertToEncoding(game.inputControl.player2k[7].ToString());
                        }

                        if (s1.Contains("pl2upk"))
                        {
                            if (game.inputControl.player2f[11])
                                s1 = "pl2upk" + "=" + (game.inputControl.player2b[11] + 272).ToString();
                            else
                                s1 = "pl2upk" + "=" + ConvertToEncoding(game.inputControl.player2k[11].ToString());
                        }
                        if (s1.Contains("pl2downk"))
                        {
                            if (game.inputControl.player2f[12])
                                s1 = "pl2downk" + "=" + (game.inputControl.player2b[12] + 272).ToString();
                            else
                                s1 = "pl2downk" + "=" + ConvertToEncoding(game.inputControl.player2k[12].ToString());
                        }
                        if (s1.Contains("pl2leftk"))
                        {
                            if (game.inputControl.player2f[13])
                                s1 = "pl2leftk" + "=" + (game.inputControl.player2b[13] + 272).ToString();
                            else
                                s1 = "pl2leftk" + "=" + ConvertToEncoding(game.inputControl.player2k[13].ToString());
                        }
                        if (s1.Contains("pl2rightk"))
                        {
                            if (game.inputControl.player2f[10])
                                s1 = "pl2rightk" + "=" + (game.inputControl.player2b[10] + 272).ToString();
                            else
                                s1 = "pl2rightk" + "=" + ConvertToEncoding(game.inputControl.player2k[10].ToString());
                        }

                        s1 += "\r\n";
                        s += s1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rom initialization: " + ex.ToString());
                }
                finally
                {
                    tr.Close();
                }
                TextWriter tw = new StreamWriter(execPath.Substring(0, execPath.LastIndexOf('\\') + 1) + "zinput.cfg");
                try
                {
                    tw.Write(s);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing rom initialization: " + ex.ToString());
                }
                finally
                {
                    tw.Close();
                }
                execArgs = new string[] { "-m" };

                commandArgs = "-m" + " " + "\"" + emulatorArgs[0] + "\"";
            }
            else if (execPath.Contains("KFusion") || execPath.Contains("kfusion") || execPath.Contains("fusion") || execPath.Contains("Fusion"))
            {
                execArgs = new string[] { "-gen", "-fullscreen" };

                foreach (string arg in execArgs)
                {
                    foreach (string arg2 in emulatorArgs)
                        commandArgs += "\"" + arg2 + "\"";
                    commandArgs += "\"" + arg + "\"";
                }
            }
            else if (execPath.Contains("nestopia") || execPath.Contains("Nestopia"))
            {



                execArgs = new string[] { " -preferences fullscreen on start : yes ", "-view size fullscreen : stretched" };

                /*foreach (string arg in execArgs)
                {
                    foreach (string arg2 in emulatorArgs)
                        commandArgs += arg2;
                    commandArgs += arg;
                }*/

                commandArgs = "\"" + emulatorArgs[0] + "\"" + " " + "" + "-preferences fullscreen on start : yes" + "";
            }
            else if (execPath.Contains("MAME") || execPath.Contains("mame"))
            {
                int offset = emulatorArgs[0].LastIndexOf("\\") + 1;
                commandArgs = emulatorArgs[0].Substring(offset, emulatorArgs[0].Length - offset - 4);// + //"\"" + "-readconfig" + "\"" + "-window" + "\"" + "-norotate" + "\"" + "-joystick" + "\"";
            }
            else
            {
                foreach (string arg in emulatorArgs)
                    commandArgs += "\"" + arg + "\"";
            }
        }

        private void WriteFile()
        {
            TextWriter tr = new StreamWriter("Morse.txt");
            try
            {
                // read from file or write to file
                tr.WriteLine("[Action]");
                tr.WriteLine("Rom Initialized");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing rom initialization: " + ex.ToString());
            }
            finally
            {
                tr.Close();
            }
        }
    }
}

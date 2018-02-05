using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace BlackBaron
{
    public partial class Form1 : Form
    {
        string filename = "//BlackBaronSystem.xml";
        public SystemStruct[] sysstruct;
        private int sysCount;

        private bool fNES = false;
        private bool fSNES = false;
        private bool fGenesis = false;
        private bool fMAME = false;
        private bool fAtari = false;
        private bool fIsRemoving = false;

        private string currentSystem = "";

        public struct SystemStruct
        {
            public string systemname;
            public string filepath;
            public string rompath;
            public Rom[] roms;

            public struct Rom
            {
                public string name;
                public string alias;
                public string description;
                public string photoFile;
                public string screenshotFile;
                public string path;
                public int count;
            }
        }


        public Form1()
        {
            InitializeComponent();
            textBox14.Text = "BlackBaron will now load your roms and match them to its database. This is necessary to make the menus look and feel right.\r\n\r\nTo start, use the dropdown menu to find which systems you own emulators for. Press the Find Paths button. This will bring up one dialog box, followed immediately by another.  First, browse to the executable file for the emulator selected. Next, browse to the folder where all your roms are stored and select one of the roms. Note that in order for BlackBaron to have access to all of your roms, you should keep them all in one place for each system.\r\n\r\nFinally, click through each of your roms at the listbox at the bottom. Each time you select one of these, you will see some possible names for the rom matched in BlackBaron's database. Select the suitable name and press the Select button at the bottom left. If the name is not yet in BlackBaron's database, you will need to make your own, though you should know that by doing this there will not be many options for this rom in BlackBaron when you select it. When finished close the window. Good luck!";
            sysstruct = new SystemStruct[6];
        }

        public void SaveXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode productsNode = doc.CreateElement("BlackBaron");
            doc.AppendChild(productsNode);

            XmlNode sysCountNode = doc.CreateElement("SysCount");
            sysCountNode.AppendChild(doc.CreateTextNode(sysCount.ToString()));
            productsNode.AppendChild(sysCountNode);

            XmlNode sysNode;
            XmlNode filepathNode;
            XmlAttribute sysAttribute;
            XmlAttribute romAttribute;
            XmlNode rompathNode;
            XmlNode romNode;
            XmlNode romCountNode;
            XmlNode nameNode;
            XmlNode aliasNode;
            XmlNode countNode;
            XmlNode descriptionNode;
            XmlNode screenshotNode;
            XmlNode photoNode;
            XmlNode pathNode;
            for(int i = 0; i < sysCount; i++)
            {
                sysNode = doc.CreateElement("System");
                sysAttribute = doc.CreateAttribute("SysName");
                sysAttribute.Value = sysstruct[i].systemname;
                sysNode.Attributes.Append(sysAttribute);
                productsNode.AppendChild(sysNode);

                filepathNode = doc.CreateElement("FilePath");
                filepathNode.AppendChild(doc.CreateTextNode(sysstruct[i].filepath));
                sysNode.AppendChild(filepathNode);

                rompathNode = doc.CreateElement("RomPath");
                rompathNode.AppendChild(doc.CreateTextNode(sysstruct[i].rompath));
                sysNode.AppendChild(rompathNode);

                romCountNode = doc.CreateElement("RomCount");
                romCountNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms.Length.ToString()));
                sysNode.AppendChild(romCountNode);

                for(int l = 0; l < sysstruct[i].roms.Length; l++)
                {
                    romNode = doc.CreateElement("Rom");
                    romAttribute = doc.CreateAttribute("RomName");
                    romAttribute.Value = sysstruct[i].roms[l].name;
                    romNode.Attributes.Append(romAttribute);
                    sysNode.AppendChild(romNode);

                    nameNode = doc.CreateElement("Name");
                    nameNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].name));
                    romNode.AppendChild(nameNode);

                    aliasNode = doc.CreateElement("Alias");
                    aliasNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].alias));
                    romNode.AppendChild(aliasNode);

                    countNode = doc.CreateElement("Count");
                    countNode.AppendChild(doc.CreateTextNode(l.ToString()));
                    romNode.AppendChild(countNode);

                    descriptionNode = doc.CreateElement("Description");
                    descriptionNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].description));
                    romNode.AppendChild(descriptionNode);

                    screenshotNode = doc.CreateElement("ScreenshotFilePath");
                    screenshotNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].screenshotFile));
                    romNode.AppendChild(screenshotNode);

                    photoNode = doc.CreateElement("PhotoFilePath");
                    photoNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].photoFile));
                    romNode.AppendChild(photoNode);

                    pathNode = doc.CreateElement("Path");
                    pathNode.AppendChild(doc.CreateTextNode(sysstruct[i].roms[l].path));
                    romNode.AppendChild(pathNode);
                }
            }

            doc.Save(Application.StartupPath + filename);
        }

        public void ReadXML()
        {
            String endelementfield = "";
            String elementfield = "";
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(Application.StartupPath + filename);
                XmlNodeList NodesCount = xml.GetElementsByTagName("System");
                sysstruct = new SystemStruct[NodesCount.Count];

                XmlTextReader xliff = new XmlTextReader(Application.StartupPath + filename);
                try
                {
                    int count = 0;
                    int romcount = 0;
                    while (xliff.Read())
                    {
                        switch (xliff.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                elementfield = xliff.Name;

                                while (xliff.MoveToNextAttribute()) // Read the attributes.
                                {
                                    SystemStruct.Rom[] newRoms = new SystemStruct.Rom[romcount];    // reset the rom list to the right size
                                    newRoms = sysstruct[count].roms;
                                    sysstruct[count].roms = new SystemStruct.Rom[romcount];
                                    sysstruct[count].roms = newRoms;

                                    sysstruct[count].systemname = xliff.Name;
                                    comboBox1.Items.Remove(sysstruct[count].systemname);
                                    listBox3.Visible = true;
                                    label4.Visible = true;
                                    listBox3.Items.Add(sysstruct[count].systemname);
                                    count++;
                                    romcount = 0;
                                }
                                break;

                            case XmlNodeType.EndElement: //The node is an element's end
                                endelementfield = xliff.Name;
                                break;

                            case XmlNodeType.Text: //Display the text in each element.
                                switch (elementfield)
                                {
                                    case "FilePath":
                                        sysstruct[count].filepath = xliff.Value;
                                        break;

                                    case "RomPath":
                                        sysstruct[count].rompath = xliff.Value;
                                        sysstruct[count].roms = new SystemStruct.Rom[200];
                                        break;

                                    case "Name":
                                        sysstruct[count].roms[romcount].name = xliff.Value;
                                        break;

                                    case "Alias":
                                        sysstruct[count].roms[romcount].alias = xliff.Value;
                                        break;

                                    case "Count":
                                        int result = 0;
                                        if(!Int32.TryParse(xliff.Value, out result))
                                        {
                                            MessageBox.Show("Parse Error, Dawg!");
                                        }
                                        sysstruct[count].roms[romcount].count = result;
                                        break;

                                    case "Path":
                                        sysstruct[count].roms[romcount].path = xliff.Value;
                                        romcount++;
                                        break;
                                }
                                break;
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing inbound XML file:" + ex.Message, "inbound parse error");
                }

                finally
                {
                    xliff.Close();
                }
            }
        }

        public void ReadXMLData(string gameFile, string sysName)
        {
            String endelementfield = "";
            String elementfield = "";
            String attributefield = "";
            bool fIsMatch = false;
            {

                XmlTextReader xliff = new XmlTextReader(Application.StartupPath + "\\BlackBaronData.xml");
                try
                {
                    while (xliff.Read())
                    {
                        switch (xliff.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                elementfield = xliff.Name;

                                while (xliff.MoveToNextAttribute()) // Read the attributes.
                                {
                                    if(elementfield == "System")
                                        attributefield = xliff.Value;
                                }
                                break;

                            case XmlNodeType.EndElement: //The node is an element's end
                                endelementfield = xliff.Name;
                                break;

                            case XmlNodeType.Text: //Display the text in each element.
                                switch (elementfield)
                                {
                                    case "Alias":
                                        if (attributefield != sysName)
                                            break;
                                        string t = xliff.Value;
                                        t = t.Replace("\r", "").Replace("\n", "");
                                        t = t.Trim();
                                        if (gameFile.Contains(t))
                                            fIsMatch = true;
                                        
                                        break;
                                    case "Name":
                                        if (fIsMatch)
                                        {
                                            listBox2.Items.Add(xliff.Value.ToString().Replace("\r", "").Replace("\n", "").Trim());
                                            fIsMatch = false;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                    }
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing inbound XML file:" + ex.Message, "inbound parse error");
                }

                finally
                {
                    xliff.Close();
                }
            }
        }

        public bool ReadXMLData(string sysName)
        {
            String endelementfield = "";
            String elementfield = "";
            String attributefield = "";
            bool fIsMatch = false;
            {

                XmlTextReader xliff = new XmlTextReader(Application.StartupPath + "\\BlackBaronData.xml");
                try
                {
                    while (xliff.Read())
                    {
                        switch (xliff.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                elementfield = xliff.Name;

                                while (xliff.MoveToNextAttribute()) // Read the attributes.
                                {
                                    attributefield = xliff.Value;
                                }
                                break;

                            case XmlNodeType.EndElement: //The node is an element's end
                                endelementfield = xliff.Name;
                                break;

                            case XmlNodeType.Text: //Display the text in each element.
                                switch (elementfield)
                                {
                                    case "Alias":

                                        break;
                                    case "Name":
                                        string t = xliff.Value.ToString().Replace("\r", "").Replace("\n", "").Trim();
                                        if(sysName == t)
                                            fIsMatch = true;

                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                    }
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing inbound XML file:" + ex.Message, "inbound parse error");
                }

                return fIsMatch;
            }
        }

        public void ReadXMLData(string romName, int iCulprit)
        {
            String endelementfield = "";
            String elementfield = "";
            String attributefield = "";
            bool fIsMatch = false;
            {

                XmlTextReader xliff = new XmlTextReader(Application.StartupPath + "\\BlackBaronData.xml");
                try
                {
                    while (xliff.Read())
                    {
                        switch (xliff.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                elementfield = xliff.Name;

                                while (xliff.MoveToNextAttribute()) // Read the attributes.
                                {
                                    if (xliff.Value == romName)
                                        fIsMatch = true;
                                    attributefield = xliff.Value;
                                }
                                break;

                            case XmlNodeType.EndElement: //The node is an element's end
                                endelementfield = xliff.Name;
                                break;

                            case XmlNodeType.Text: //Display the text in each element.
                                switch (elementfield)
                                {
                                    case "Description":
                                        if (fIsMatch)
                                            sysstruct[sysCount].roms[iCulprit].description = xliff.Value;
                                        break;

                                    case "ScreenshotFilePath":
                                        if (fIsMatch)
                                            sysstruct[sysCount].roms[iCulprit].screenshotFile = xliff.Value;
                                        break;

                                    case "PhotoFilePath":
                                        if (fIsMatch)
                                            sysstruct[sysCount].roms[iCulprit].photoFile = xliff.Value;
                                        break;

                                    case "Name":
                                        if (fIsMatch)
                                            fIsMatch = false;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                    }
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing inbound XML file:" + ex.Message, "inbound parse error");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch(comboBox1.Text)
            {
                case "Super Nintendo":
                    if (fSNES)
                        return;
                    sysstruct[sysCount].systemname = "Super Nintendo";
                    SetupNewSystem();

                    fSNES = true;
                    break;
                case "Nintendo":
                    if (fNES)
                        return;
                    sysstruct[sysCount].systemname = "Nintendo";
                    SetupNewSystem();
                    fNES = true;
                    break;

                case "Genesis":
                    if (fGenesis)
                        return;
                    sysstruct[sysCount].systemname = "Genesis";
                    SetupNewSystem();
                    fGenesis = true;
                    break;

                case "MAME":
                    if (fMAME)
                        return;
                    MessageBox.Show("Any MameUI will probably throw some errors. BlackBaron works best if you select a mame version without a frontend (usually named mame.exe or something like that). Dawg.");
                    sysstruct[sysCount].systemname = "MAME";
                    SetupNewSystem();

                    fMAME = true;
                    break;

                case "Atari 2600":
                    if (fAtari)
                        return;
                    sysstruct[sysCount].systemname = "Atari 2600";
                    SetupNewSystem();
                    fAtari = true;
                    break;
                default:
                    break;
            }
        }

        private void SetupNewSystem()
        {
            currentSystem = comboBox1.Text;

            String sFilter = dlgOpen.Filter;
            dlgOpen.Filter = "executable|*.exe";
            dlgOpen.Title = "Select the emulator's executable";
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                sysstruct[sysCount].filepath = dlgOpen.FileName;
            }
            else
            {
                MessageBox.Show("Gotta select which executable file is the emulator, Dawg!");
                return;
            }
            dlgOpen.Filter = "";
            dlgOpen.Title = "Select a rom from the place where you keep your roms";
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                // sysstruct[sysCount].rompath = dlgOpen.FileName;
            }
            else
            {
                MessageBox.Show("Gotta select a rom from your roms folder, Dawg!");
                return;
            }

            // this file write is included here because it wasn't being executed in the FormClosing event for some reason;
            // this is probably because it was being written too soon after the xml write
            // make sure we overwrite the text message
            TextWriter tw = new StreamWriter("Morse.txt");
            try
            {
                tw.WriteLine("[Action]");
                tw.WriteLine("Running App");
            }
            catch
            {
                MessageBox.Show("Rewrite Error, Dawg!");
            }
            finally
            {
                tw.Close();
            }


            // Process the list of files found in the directory. 
            string s = dlgOpen.FileName.Substring(0, dlgOpen.FileName.Length - dlgOpen.SafeFileName.Length);
            sysstruct[sysCount].rompath = s;
            string[] fileEntries = Directory.GetFiles(s);
            sysstruct[sysCount].roms = new SystemStruct.Rom[fileEntries.GetLength(0)];
            int romCount = 0;
            char[] slashes = new char[1];
            slashes[0] = '\\';

            foreach (string fileName in fileEntries)
            {
                sysstruct[sysCount].roms[romCount].path = fileName;
                int holder = fileName.LastIndexOfAny(slashes);
                string t = fileName.Substring(holder + 1);
                sysstruct[sysCount].roms[romCount].name = t;
                listBox1.Items.Add(t);
                sysstruct[sysCount].roms[romCount].count = romCount;
                romCount++;
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fIsRemoving)
            {
                fIsRemoving = false;
                return;
            }
            if (listBox1.Items.Count == 0 || listBox1.SelectedItem.ToString() == string.Empty)
                return;
            listBox2.Items.Clear();
            ReadXMLData(listBox1.SelectedItem.ToString(), currentSystem);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem == null || listBox1.SelectedItem == null)
            {
                MessageBox.Show("You gotta select the name of your rom and the way that you'll see it in BlackBaron, Dawg!");
                return;
            }

            int iCulprit = 0;
            for (int i = 0; i < sysstruct[sysCount].roms.Length; i++)
            {
                if (sysstruct[sysCount].roms[i].name == listBox1.SelectedItem.ToString())
                    iCulprit = i;
            }

            sysstruct[sysCount].roms[iCulprit].alias = listBox2.Text;
            ReadXMLData(listBox2.Text, iCulprit);

            fIsRemoving = true;
            try
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                listBox2.Items.Clear();
                if (listBox1.Items.Count != 0)
                {
                    fIsRemoving = true;
                    listBox1.SelectedIndex = 0;
                    ReadXMLData(listBox1.SelectedItem.ToString(), currentSystem);
                }
            }
            catch
            {
                MessageBox.Show("ItemRemove Parse fail, Dawg!");
                return;
            }

            finally
            {
                if (listBox1.Items.Count == 0 && currentSystem == comboBox1.Text)
                {
                    listBox3.Visible = true;
                    label4.Visible = true;
                    listBox3.Items.Add(comboBox1.Text);
                    comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
                    currentSystem = "";
                    sysCount++;
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0 || listBox2.SelectedItem == null)
                return;
            if (ReadXMLData(listBox2.SelectedItem.ToString()))
                checkBox7.Checked = true;
            else
                checkBox7.Checked = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox13.Text == "")
                return;
            else
                listBox2.Items.Add(textBox13.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("You gotta select the rom you want to delete, Dawg!");
                return;
            }

            SystemStruct.Rom[] roms = new SystemStruct.Rom[sysstruct[sysCount].roms.Length - 1];
            int naughty = 0;
            for (int i = 0; i < sysstruct[sysCount].roms.Length; i++)
            {
                if (listBox1.SelectedItem.ToString() == sysstruct[sysCount].roms[i].name)
                    naughty = sysstruct[sysCount].roms[i].count;
            }
            int orange = 0;
            for (int i = 0; i < sysstruct[sysCount].roms.Length; i++)
            {
                if (sysstruct[sysCount].roms[i].count != naughty)
                {
                    roms[orange] = sysstruct[sysCount].roms[i];
                    orange++;
                }
            }

            sysstruct[sysCount].roms = new SystemStruct.Rom[roms.Length];
            sysstruct[sysCount].roms = roms;

            // and delete the selected index, finally
            fIsRemoving = true;
            try
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                listBox2.Items.Clear();
                if (listBox1.Items.Count != 0)
                {
                    fIsRemoving = true;
                    listBox1.SelectedIndex = 0;
                    ReadXMLData(listBox1.SelectedItem.ToString(), currentSystem);
                }
            }
            catch
            {
                MessageBox.Show("ItemRemove Parse fail, Dawg!");
                return;
            }

            finally
            {
                if (listBox1.Items.Count == 0 && currentSystem == comboBox1.Text)
                {
                    listBox3.Visible = true;
                    label4.Visible = true;
                    listBox3.Items.Add(comboBox1.Text);
                    comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
                    currentSystem = "";
                    sysCount++;
                }
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sysstruct[0].systemname == null || sysstruct[0].systemname == string.Empty)
            {
                TextWriter tw = new StreamWriter("Morse.txt");
                try
                {
                    tw.WriteLine("[Action]");
                    tw.WriteLine("ReEnter Systems");
                }
                catch
                {
                    MessageBox.Show("Rewrite Error, Dawg!");
                }
                finally
                {
                    tw.Close();
                }
                return;
            }
            SaveXML();
        }
    }
}

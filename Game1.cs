using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
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
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Data and Errata
        // graphix tools
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Texture2D logoTexture;
        Texture2D fadeTexture;
        Texture2D hatchTexture;
        Texture2D fancyLogo;
        public Texture2D bloomTexture;
        public Texture2D keyboardTexture;
        public Texture2D controllerTexture;
        public int m_Height;
        public int m_Width;

        // Game States
        

        // sound effects
        SoundEffect itemSoundEffect;
        SoundEffect menuSoundEffect;
        public SoundEffect scrollSoundEffect;
        public SoundEffect guitarSoundEffect;
        public Song fireSoundEffect;

        public List<MenuWindow> menuList;
        public SpriteFont menuFont;
        public MenuWindow activeMenu;
        public KeyboardState lastKeybState;
        public ButtonState[] lastButtonStates;
        DirectInputThumbSticks lastTSticks;
        SystemStruct[] sysstruct;
        GameWin gameMenu;
        GameWin gameMenu2;
        public int sysIndex = 0;
        int romIndex;
        int bloomHolder;
        bool bloomVisHolder = false;
        bool sysFPSSetting = false;
        bool distorHolder = false;
        bool hasCaptured = false;
        bool mustCapture = false;
        bool mustWaitToCapture = false;
        bool hasWaitedToCapture = false;

        // post processing and effects and parameters
        VertexPositionTexture[] ppVertices;
        RenderTarget2D targetRenderedTo;
        ResolveTexture2D resolveTexture;
        Effect postProcessingFXInverse;
        // game components (some post processing effects, some menu system)
        public BloomComponent bloom;
        IntroScreen introScreen;
        public EndScreen endScreen;
        public ParticleEngine particleEngine;
        public PhysicsEngine physicsEngine;
        public FireEffect fireEffect;
        public InputImageControl inputControl;
        public int bloomSettingsIndex = 0;
        float time = 0;
          // for ripples
        EffectParameter waveParam, distortionParam, centerCoordParam, divisorParam;
        Vector2 centerCoord = new Vector2(0.5f);
        float distortion = 1.0f;
        float divisor1 = 1.0f;
        float wave = MathHelper.Pi;
        private void Reset()
        {
            centerCoord = new Vector2(0.5f);
            distortion = 1.0f;
            divisor1 = 0.25f;
            wave = MathHelper.Pi / divisor1;
        }
          // distortion params
        Distortion distorter;

        // system info variables
        double dFPS = 0.0f;
        double dLastUpdate = 0.0f;
        public bool isDisplaySysInfo = false;
        public bool isDrawInput = false;
        double frameCounter = 0;
        double frameTime = 0;
        double currentFrameRate = 0;

        Predicate<MenuWindow> predicate;

        // settings info variables
        bool isInvertedEffect = false;
        bool isTimedColors = false;
        bool isGlow = false;
        bool isDistortion = false;
        bool isBump = false;
        bool isGreenShift = false;
        bool isColorWring = false;
        bool isBloomSoft = false;
        bool isBloomDefault = false;
        bool isFullScreen = false;
        public bool isSoundOn = true;

        // bring in PC gamepads for Saturn Controllers
        public DirectInputGamepad[] gamePad = new DirectInputGamepad[1];
        int noUsers = 0;

        // game state info
        public enum GameState { IntroScreen = 0, MenuScreen, SaveScreen, EndScreen };
        public GameState State = GameState.IntroScreen;
        public bool over = false;

        public enum LogoState { Inactive, Active, SaveScreen, Starting, Ending };
        public LogoState logoState = LogoState.Inactive;
        public double waitProgress = 0.0f;
        public double logoFadeProgress = 0.0f;
        public double screenSaveFadeProgress = 0.0f;

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
                public string screenfile;
                public string description;
                public string photofile;
                public string path;
                public int count;
            }
        }

        #endregion
        #region Initialization
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            m_Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            m_Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

            // load graphic settings
            isFullScreen = Properties.Settings.Default.FullScreen;
            graphics.IsFullScreen = isFullScreen;
            graphics.PreferredBackBufferWidth = m_Width;
            graphics.PreferredBackBufferHeight = m_Height;

            // sound settings
            isSoundOn = Properties.Settings.Default.SoundOn;

            // initialize gamepad info
            DirectInputGamepad.ReloadGamepads();
            noUsers = DirectInputGamepad.Gamepads.Count;
            for(int i = 0; i < DirectInputGamepad.Gamepads.Count; i++)
                gamePad[i] = DirectInputGamepad.Gamepads[i];

            // initialize intro screen component
            introScreen = new IntroScreen(this);
            Components.Add(introScreen);

            // initialize end screen component
            endScreen = new EndScreen(this);
            Components.Add(endScreen);

            // initialize bloom effect
            bloom = new BloomComponent(this);
            Components.Add(bloom);

            // add particle engine
            particleEngine = new ParticleEngine(this);
            Components.Add(particleEngine);

            // add physics engine to affect any particles that need physics updates
            physicsEngine = new PhysicsEngine(this);
            Components.Add(physicsEngine);

            // add fire effect component
            fireEffect = new FireEffect(this);
            Components.Add(fireEffect);

            // add component to handle controller images
            inputControl = new InputImageControl(this);
            Components.Add(inputControl);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // read system info from disk--hand created though serialization should have been used
            ReadXML();
            // set gamepad button configuation
            lastKeybState = new KeyboardState();
            lastButtonStates = new ButtonState[9];

            
            base.Initialize();




            // establish predicate for retrieving a specific menu in the menulist
            predicate = new Predicate<MenuWindow>(delegate(MenuWindow m){ return m == activeMenu; });

            // initialize postprocessing vertices
            InitPostProcessingVertices();
            // load settings**********************************************
            // graphics
            bloomSettingsIndex = Properties.Settings.Default.BloomIndex;
            bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
            bloom.Visible = Properties.Settings.Default.BloomEffect;

            isGlow = Properties.Settings.Default.GlowEffect;
            isColorWring = Properties.Settings.Default.ColorWringEffect;
            isBump = Properties.Settings.Default.BumpEffect;
            isDistortion = Properties.Settings.Default.DistortionEffect;
            isInvertedEffect = Properties.Settings.Default.InvertedEffect;
            isGreenShift = Properties.Settings.Default.GreenShiftEffect;
            isTimedColors = Properties.Settings.Default.TimedColorsEffect;
            // sound
            isSoundOn = Properties.Settings.Default.SoundOn;
            // input player 1 keyboard
            inputControl.k[0] = Properties.Settings.Default.Key0;
            inputControl.k[1] = Properties.Settings.Default.Key1;
            inputControl.k[2] = Properties.Settings.Default.Key2;
            inputControl.k[3] = Properties.Settings.Default.Key3;
            inputControl.k[4] = Properties.Settings.Default.Key4;
            inputControl.k[5] = Properties.Settings.Default.Key5;
            inputControl.k[6] = Properties.Settings.Default.Key6;
            inputControl.k[7] = Properties.Settings.Default.Key7;
            inputControl.k[8] = Properties.Settings.Default.Key8;
            inputControl.k[9] = Properties.Settings.Default.Key9;
            inputControl.k[10] = Properties.Settings.Default.Key10;
            inputControl.k[11] = Properties.Settings.Default.Key11;
            inputControl.k[12] = Properties.Settings.Default.Key12;
            inputControl.k[13] = Properties.Settings.Default.Key13;
            // input player 2 keyboard
            inputControl.player2k[0] = Properties.Settings.Default.PKey0;
            inputControl.player2k[1] = Properties.Settings.Default.PKey1;
            inputControl.player2k[2] = Properties.Settings.Default.PKey2;
            inputControl.player2k[3] = Properties.Settings.Default.PKey3;
            inputControl.player2k[4] = Properties.Settings.Default.PKey4;
            inputControl.player2k[5] = Properties.Settings.Default.PKey5;
            inputControl.player2k[6] = Properties.Settings.Default.PKey6;
            inputControl.player2k[7] = Properties.Settings.Default.PKey7;
            inputControl.player2k[8] = Properties.Settings.Default.PKey8;
            inputControl.player2k[9] = Properties.Settings.Default.PKey9;
            inputControl.player2k[10] = Properties.Settings.Default.PKey10;
            inputControl.player2k[11] = Properties.Settings.Default.PKey11;
            inputControl.player2k[12] = Properties.Settings.Default.PKey12;
            inputControl.player2k[13] = Properties.Settings.Default.PKey13;
            // input player 1 controller
            inputControl.b[0] = Properties.Settings.Default.Button0;
            inputControl.b[1] = Properties.Settings.Default.Button1;
            inputControl.b[2] = Properties.Settings.Default.Button2;
            inputControl.b[3] = Properties.Settings.Default.Button3;
            inputControl.b[4] = Properties.Settings.Default.Button4;
            inputControl.b[5] = Properties.Settings.Default.Button5;
            inputControl.b[6] = Properties.Settings.Default.Button6;
            inputControl.b[7] = Properties.Settings.Default.Button7;
            inputControl.b[8] = Properties.Settings.Default.Button8;
            inputControl.b[9] = Properties.Settings.Default.Button9;
            inputControl.b[10] = Properties.Settings.Default.Button10;
            inputControl.b[11] = Properties.Settings.Default.Button11;
            inputControl.b[12] = Properties.Settings.Default.Button12;
            inputControl.b[13] = Properties.Settings.Default.Button13;
            // input player 2 controller
            inputControl.player2b[0] = Properties.Settings.Default.PButton0;
            inputControl.player2b[1] = Properties.Settings.Default.PButton1;
            inputControl.player2b[2] = Properties.Settings.Default.PButton2;
            inputControl.player2b[3] = Properties.Settings.Default.PButton3;
            inputControl.player2b[4] = Properties.Settings.Default.PButton4;
            inputControl.player2b[5] = Properties.Settings.Default.PButton5;
            inputControl.player2b[6] = Properties.Settings.Default.PButton6;
            inputControl.player2b[7] = Properties.Settings.Default.PButton7;
            inputControl.player2b[8] = Properties.Settings.Default.PButton8;
            inputControl.player2b[9] = Properties.Settings.Default.PButton9;
            inputControl.player2b[10] = Properties.Settings.Default.PButton10;
            inputControl.player2b[11] = Properties.Settings.Default.PButton11;
            inputControl.player2b[12] = Properties.Settings.Default.PButton12;
            inputControl.player2b[13] = Properties.Settings.Default.PButton13;
            // input player 1 combo table
            inputControl.f[0] = Properties.Settings.Default.PFlag0;
            inputControl.f[1] = Properties.Settings.Default.PFlag1;
            inputControl.f[2] = Properties.Settings.Default.PFlag2;
            inputControl.f[3] = Properties.Settings.Default.PFlag3;
            inputControl.f[4] = Properties.Settings.Default.PFlag4;
            inputControl.f[5] = Properties.Settings.Default.PFlag5;
            inputControl.f[6] = Properties.Settings.Default.PFlag6;
            inputControl.f[7] = Properties.Settings.Default.PFlag7;
            inputControl.f[8] = Properties.Settings.Default.PFlag8;
            inputControl.f[9] = Properties.Settings.Default.PFlag9;
            inputControl.f[10] = Properties.Settings.Default.PFlag10;
            inputControl.f[11] = Properties.Settings.Default.PFlag11;
            inputControl.f[12] = Properties.Settings.Default.PFlag12;
            inputControl.f[13] = Properties.Settings.Default.PFlag13;
            // input player 2 combo table
            inputControl.player2f[0] = Properties.Settings.Default.P2Flag0;
            inputControl.player2f[1] = Properties.Settings.Default.P2Flag1;
            inputControl.player2f[2] = Properties.Settings.Default.P2Flag2;
            inputControl.player2f[3] = Properties.Settings.Default.P2Flag3;
            inputControl.player2f[4] = Properties.Settings.Default.P2Flag4;
            inputControl.player2f[5] = Properties.Settings.Default.P2Flag5;
            inputControl.player2f[6] = Properties.Settings.Default.P2Flag6;
            inputControl.player2f[7] = Properties.Settings.Default.P2Flag7;
            inputControl.player2f[8] = Properties.Settings.Default.P2Flag8;
            inputControl.player2f[9] = Properties.Settings.Default.P2Flag9;
            inputControl.player2f[10] = Properties.Settings.Default.P2Flag10;
            inputControl.player2f[11] = Properties.Settings.Default.P2Flag11;
            inputControl.player2f[12] = Properties.Settings.Default.P2Flag12;
            inputControl.player2f[13] = Properties.Settings.Default.P2Flag13;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create sound "engine"
            itemSoundEffect = Content.Load<SoundEffect>("ui_pop");
            menuSoundEffect = Content.Load<SoundEffect>("transition_organic_whoosh");
            scrollSoundEffect = Content.Load<SoundEffect>("airwhoosh");
            guitarSoundEffect = Content.Load<SoundEffect>("guitarchord");
            fireSoundEffect = Content.Load<Song>("FireCrackle");

            // load peripheral images and devices
            logoTexture = Content.Load<Texture2D>("BBLogo");
            fadeTexture = Content.Load<Texture2D>("FadeToBlackScreen");
            bloomTexture = Content.Load<Texture2D>("BloomImage");
            keyboardTexture = Content.Load<Texture2D>("Keyboard");
            controllerTexture = Content.Load<Texture2D>("Controller");
            hatchTexture = Content.Load<Texture2D>("HatchOverlay");
            fancyLogo = Content.Load<Texture2D>("FancyLogo");


            // Load postprocessing engine
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            targetRenderedTo = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            resolveTexture = new ResolveTexture2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            postProcessingFXInverse = Content.Load<Effect>("InverseGlow");

              // load ripple effect engine
            waveParam = postProcessingFXInverse.Parameters["wave"];
            distortionParam = postProcessingFXInverse.Parameters["distortion"];
            centerCoordParam = postProcessingFXInverse.Parameters["centerCoord"];
            divisorParam = postProcessingFXInverse.Parameters["divisor"];

              // load distortion effect engine
            distorter = new Distortion();
                // set the blur parameters for the current viewport
            SetBlurEffectParameters(1f / (float)device.PresentationParameters.BackBufferWidth, 1f / (float)device.PresentationParameters.BackBufferHeight);

            // Begin menu initialization
            if (GraphicsAdapter.DefaultAdapter.IsWideScreen)
                menuFont = Content.Load<SpriteFont>("menuFontEnlarged");
            else
                menuFont = Content.Load<SpriteFont>("menuFont");
            menuList = new List<MenuWindow>();

            MenuWin menuMain = new MenuWin(menuFont, 0, "System", Content, m_Width / 8, m_Height / 12, true, this);
            MenuWin menuSubMenu1 = new MenuWin(menuFont, 1, "Launch", m_Width / 8, m_Height / 12, this);
            MenuWin exitMenu = new MenuWin(menuFont, 0, "Exit", (int)(m_Width * 0.45), (m_Height /12), this);
            MenuWin guiMenu = new MenuWin(menuFont, 1, "Settings", m_Width / 8, m_Height / 12, this);
            MenuWin effectMenu = new MenuWin(menuFont, 0, "Change Effects", m_Width / 8, m_Height / 12, this);
            MenuWin bloomMenu = new MenuWin(menuFont, 1, true, "Change Bloom Effect", m_Width / 8, m_Height / 12, this);
            MenuWin settingsMenu = new MenuWin(menuFont, 0, "System Settings", m_Width / 8, m_Height / 12, this);
            MenuWin inputMenu = new MenuWin(menuFont, 1, "Input Settings", m_Width / 8, m_Height / 12, this);
            MenuWin player1Menu = new MenuWin(menuFont, 0, "Player 1 Input", m_Width / 8, m_Height / 12, this);
            MenuWin player2Menu = new MenuWin(menuFont, 1, "Player 2 Input", m_Width / 8, m_Height / 12, this);
            gameMenu = new GameWin(menuFont, "Bye Bye", 0, this);
            gameMenu2 = new GameWin(menuFont, "Bye Bye", 0, this);
            menuList.Add(menuMain);
            menuList.Add(menuSubMenu1);
            menuList.Add(exitMenu);
            menuList.Add(guiMenu);
            menuList.Add(effectMenu);
            menuList.Add(bloomMenu);
            menuList.Add(settingsMenu);
            menuList.Add(inputMenu);
            menuList.Add(player1Menu);
            menuList.Add(player2Menu);

            foreach(SystemStruct sys in sysstruct)
            {
                menuList.Add(new MenuWin(menuFont, 1, "Roms", Content, m_Width / 8, m_Height / 12, this));
                menuMain.AddMenuItem(sys.systemname, menuList.ElementAt<MenuWindow>(menuList.Count - 1));
                for(int i = 0; i < sys.roms.Length; i++)
                {
                    if (sys.roms[i].photofile == string.Empty || sys.roms[i].photofile == null)
                        sys.roms[i].photofile = "questionmark";
                    if (sys.roms[i].screenfile == string.Empty || sys.roms[i].screenfile == null)
                        sys.roms[i].screenfile = "questionmark";
                    if (sys.roms[i].description == string.Empty || sys.roms[i].description == null)
                        sys.roms[i].description = "No Description";
                    menuList.ElementAt<MenuWindow>(menuList.Count - 1).AddMenuItem(sys.roms[i].alias, menuSubMenu1, sys.roms[i].photofile, sys.roms[i].screenfile, sys.roms[i].description);
                }
            }

            menuSubMenu1.AddMenuItem("Play", gameMenu);
            menuSubMenu1.AddMenuItem("Delete", menuMain);
            menuSubMenu1.AddMenuItem("Back", menuMain);
            exitMenu.AddMenuItem("       Exit", gameMenu2);
            exitMenu.AddMenuItem(" Reenter Systems", gameMenu2);
            exitMenu.AddMenuItem("     Settings", guiMenu);
            exitMenu.AddMenuItem("   Screen Saver", menuMain);
            exitMenu.AddMenuItem("     Main Menu", menuMain);
            guiMenu.AddMenuItem("Graphics Settings", effectMenu);
            guiMenu.AddMenuItem("System Settings", settingsMenu);
            guiMenu.AddMenuItem("  Main Menu", menuMain);
            effectMenu.AddMenuItem("   Bloom", bloomMenu);
            effectMenu.AddMenuItem("Invert Colors", effectMenu);
            effectMenu.AddMenuItem("Timed Colors", effectMenu);
            effectMenu.AddMenuItem(" Weak Glow", effectMenu);
            effectMenu.AddMenuItem("Bump Mapping", effectMenu);
            effectMenu.AddMenuItem(" Color Wring", effectMenu);
            effectMenu.AddMenuItem(" Green Shift", effectMenu);
            effectMenu.AddMenuItem("Distortion Blur", effectMenu);
            effectMenu.AddMenuItem(" Main Menu", menuMain);
            bloomMenu.AddMenuItem("  Default", bloomMenu);
            bloomMenu.AddMenuItem("   Soft", bloomMenu);
            bloomMenu.AddMenuItem(" Saturated", bloomMenu);
            bloomMenu.AddMenuItem("Desaturated", bloomMenu);
            bloomMenu.AddMenuItem("   Blurry", bloomMenu);
            bloomMenu.AddMenuItem("  Subtle", bloomMenu);
            bloomMenu.AddMenuItem("   None", bloomMenu);
            bloomMenu.AddMenuItem(" Main Menu", menuMain);
            settingsMenu.AddMenuItem("   Input", inputMenu);
            settingsMenu.AddMenuItem("Full Screen", settingsMenu);
            settingsMenu.AddMenuItem(" Play Sound", settingsMenu);
            settingsMenu.AddMenuItem("Main Menu", menuMain);
            inputMenu.AddMenuItem("Player One", player1Menu);
            inputMenu.AddMenuItem("Player Two", player2Menu);
            inputMenu.AddMenuItem("Main Menu", menuMain);
            player1Menu.AddMenuItem("Button 0", player1Menu);
            player1Menu.AddMenuItem("Button 1", player1Menu);
            player1Menu.AddMenuItem("Button 2", player1Menu);
            player1Menu.AddMenuItem("Button 3", player1Menu);
            player1Menu.AddMenuItem("Button 4", player1Menu);
            player1Menu.AddMenuItem("Button 5", player1Menu);
            player1Menu.AddMenuItem("Button 6", player1Menu);
            player1Menu.AddMenuItem("Button 7", player1Menu);
            player1Menu.AddMenuItem("Button 8", player1Menu);
            player1Menu.AddMenuItem("Button 9", player1Menu);
            player1Menu.AddMenuItem("Right Direction", player1Menu);
            player1Menu.AddMenuItem("Up Direction", player1Menu);
            player1Menu.AddMenuItem("Down Direction", player1Menu);
            player1Menu.AddMenuItem("Left Direction", player1Menu);
            player2Menu.AddMenuItem("Button 0", player2Menu);
            player2Menu.AddMenuItem("Button 1", player2Menu);
            player2Menu.AddMenuItem("Button 2", player2Menu);
            player2Menu.AddMenuItem("Button 3", player2Menu);
            player2Menu.AddMenuItem("Button 4", player2Menu);
            player2Menu.AddMenuItem("Button 5", player2Menu);
            player2Menu.AddMenuItem("Button 6", player2Menu);
            player2Menu.AddMenuItem("Button 7", player2Menu);
            player2Menu.AddMenuItem("Button 8", player2Menu);
            player2Menu.AddMenuItem("Button 9", player2Menu);
            player2Menu.AddMenuItem("Right Direction", player2Menu);
            player2Menu.AddMenuItem("Up Direction", player2Menu);
            player2Menu.AddMenuItem("Down Direction", player2Menu);
            player2Menu.AddMenuItem("Left Direction", player2Menu);


            activeMenu = menuMain;
            menuMain.WakeUp();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();

            RestoreSettings();

            // save settings**********************************************
            // graphics
            Properties.Settings.Default.BloomIndex = bloomSettingsIndex;
            Properties.Settings.Default.BloomEffect = bloom.Visible;
            Properties.Settings.Default.GlowEffect = isGlow;
            Properties.Settings.Default.ColorWringEffect = isColorWring;
            Properties.Settings.Default.BumpEffect = isBump;
            Properties.Settings.Default.DistortionEffect = isDistortion;
            Properties.Settings.Default.InvertedEffect = isInvertedEffect;
            Properties.Settings.Default.GreenShiftEffect = isGreenShift;
            Properties.Settings.Default.TimedColorsEffect = isTimedColors;
            // sound
            Properties.Settings.Default.SoundOn = isSoundOn;
            // input player 1 keyboard
            Properties.Settings.Default.Key0 = inputControl.k[0];
            Properties.Settings.Default.Key1 = inputControl.k[1];
            Properties.Settings.Default.Key2 = inputControl.k[2];
            Properties.Settings.Default.Key3 = inputControl.k[3];
            Properties.Settings.Default.Key4 = inputControl.k[4];
            Properties.Settings.Default.Key5 = inputControl.k[5];
            Properties.Settings.Default.Key6 = inputControl.k[6];
            Properties.Settings.Default.Key7 = inputControl.k[7];
            Properties.Settings.Default.Key8 = inputControl.k[8];
            Properties.Settings.Default.Key9 = inputControl.k[9];
            Properties.Settings.Default.Key10 = inputControl.k[10];
            Properties.Settings.Default.Key11 = inputControl.k[11];
            Properties.Settings.Default.Key12 = inputControl.k[12];
            Properties.Settings.Default.Key13 = inputControl.k[13];
            // input player 2 keyboard
            Properties.Settings.Default.PKey0 = inputControl.player2k[0];
            Properties.Settings.Default.PKey1 = inputControl.player2k[1];
            Properties.Settings.Default.PKey2 = inputControl.player2k[2];
            Properties.Settings.Default.PKey3 = inputControl.player2k[3];
            Properties.Settings.Default.PKey4 = inputControl.player2k[4];
            Properties.Settings.Default.PKey5 = inputControl.player2k[5];
            Properties.Settings.Default.PKey6 = inputControl.player2k[6];
            Properties.Settings.Default.PKey7 = inputControl.player2k[7];
            Properties.Settings.Default.PKey8 = inputControl.player2k[8];
            Properties.Settings.Default.PKey9 = inputControl.player2k[9];
            Properties.Settings.Default.PKey10 = inputControl.player2k[10];
            Properties.Settings.Default.PKey11 = inputControl.player2k[11];
            Properties.Settings.Default.PKey12 = inputControl.player2k[12];
            Properties.Settings.Default.PKey13 = inputControl.player2k[13];
            // input player 1 controller
            Properties.Settings.Default.Button0 = inputControl.b[0];
            Properties.Settings.Default.Button1 = inputControl.b[1];
            Properties.Settings.Default.Button2 = inputControl.b[2];
            Properties.Settings.Default.Button3 = inputControl.b[3];
            Properties.Settings.Default.Button4 = inputControl.b[4];
            Properties.Settings.Default.Button5 = inputControl.b[5];
            Properties.Settings.Default.Button6 = inputControl.b[6];
            Properties.Settings.Default.Button7 = inputControl.b[7];
            Properties.Settings.Default.Button8 = inputControl.b[8];
            Properties.Settings.Default.Button9 = inputControl.b[9];
            Properties.Settings.Default.Button10 = inputControl.b[10];
            Properties.Settings.Default.Button11 = inputControl.b[11];
            Properties.Settings.Default.Button12 = inputControl.b[12];
            Properties.Settings.Default.Button13 = inputControl.b[13];
            // input player 2 controller
            Properties.Settings.Default.PButton0 = inputControl.player2b[0];
            Properties.Settings.Default.PButton1 = inputControl.player2b[1];
            Properties.Settings.Default.PButton2 = inputControl.player2b[2];
            Properties.Settings.Default.PButton3 = inputControl.player2b[3];
            Properties.Settings.Default.PButton4 = inputControl.player2b[4];
            Properties.Settings.Default.PButton5 = inputControl.player2b[5];
            Properties.Settings.Default.PButton6 = inputControl.player2b[6];
            Properties.Settings.Default.PButton7 = inputControl.player2b[7];
            Properties.Settings.Default.PButton8 = inputControl.player2b[8];
            Properties.Settings.Default.PButton9 = inputControl.player2b[9];
            Properties.Settings.Default.PButton10 = inputControl.player2b[10];
            Properties.Settings.Default.PButton11 = inputControl.player2b[11];
            Properties.Settings.Default.PButton12 = inputControl.player2b[12];
            Properties.Settings.Default.PButton13 = inputControl.player2b[13];
            // input player 1 table
            Properties.Settings.Default.PFlag0 = inputControl.f[0];
            Properties.Settings.Default.PFlag1 = inputControl.f[1];
            Properties.Settings.Default.PFlag2 = inputControl.f[2];
            Properties.Settings.Default.PFlag3 = inputControl.f[3];
            Properties.Settings.Default.PFlag4 = inputControl.f[4];
            Properties.Settings.Default.PFlag5 = inputControl.f[5];
            Properties.Settings.Default.PFlag6 = inputControl.f[6];
            Properties.Settings.Default.PFlag7 = inputControl.f[7];
            Properties.Settings.Default.PFlag8 = inputControl.f[8];
            Properties.Settings.Default.PFlag9 = inputControl.f[9];
            Properties.Settings.Default.PFlag10 = inputControl.f[10];
            Properties.Settings.Default.PFlag11 = inputControl.f[11];
            Properties.Settings.Default.PFlag12 = inputControl.f[12];
            Properties.Settings.Default.PFlag13 = inputControl.f[13];
            // input player 2 table
            Properties.Settings.Default.P2Flag0 = inputControl.player2f[0];
            Properties.Settings.Default.P2Flag1 = inputControl.player2f[1];
            Properties.Settings.Default.P2Flag2 = inputControl.player2f[2];
            Properties.Settings.Default.P2Flag3 = inputControl.player2f[3];
            Properties.Settings.Default.P2Flag4 = inputControl.player2f[4];
            Properties.Settings.Default.P2Flag5 = inputControl.player2f[5];
            Properties.Settings.Default.P2Flag6 = inputControl.player2f[6];
            Properties.Settings.Default.P2Flag7 = inputControl.player2f[7];
            Properties.Settings.Default.P2Flag8 = inputControl.player2f[8];
            Properties.Settings.Default.P2Flag9 = inputControl.player2f[9];
            Properties.Settings.Default.P2Flag10 = inputControl.player2f[10];
            Properties.Settings.Default.P2Flag11 = inputControl.player2f[11];
            Properties.Settings.Default.P2Flag12 = inputControl.player2f[12];
            Properties.Settings.Default.P2Flag13 = inputControl.player2f[13];
            Properties.Settings.Default.Save();
        }
        #endregion
        #region Update Stuff
        private void MenuInput(KeyboardState currentKeybState, ButtonState[] bStates, DirectInputThumbSticks currentTSticks, GameTime gameTime)
        {
            // first, process the systemwide requests like fps switches
            if (currentKeybState.IsKeyDown(Keys.F1) && lastKeybState.IsKeyUp(Keys.F1))
            {
                if (isDisplaySysInfo)
                    isDisplaySysInfo = false;
                else
                    isDisplaySysInfo = true;
            }

            // if we're setting controls, don't do anything
            if (inputControl.isSetting)
                return;

            if ((lastKeybState.IsKeyUp(Keys.Escape) && currentKeybState.IsKeyDown(Keys.Escape)) || (lastButtonStates[8] == ButtonState.Released && bStates[8] == ButtonState.Pressed))
            {
                StoreSettings();
                mustCapture = true;
                mustWaitToCapture = true;
            }

            // make sure we don't do anything if we just barely switched over from the intro screen
            if (over)
            {
                over = false;
                return;
            }
            MenuWindow newActive = activeMenu.ProcessInput(lastKeybState, currentKeybState, lastButtonStates, bStates, lastTSticks, currentTSticks, gameTime);

            if (!over && ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)) ||
                (lastButtonStates[0] == ButtonState.Released && bStates[0] == ButtonState.Pressed)))
            {
                fireEffect.hasPlayed = false;

                AdjustSettings(activeMenu);
                
                if (logoState == LogoState.Active)
                    logoState = LogoState.Ending;
                waitProgress = 0;
                //string s = gamePad[0]
            }


            // press left or press b button to move back once in the menu
            if (((lastButtonStates[1] == ButtonState.Released && bStates[1] == ButtonState.Pressed) || (lastTSticks.Left.X > -0.5f && currentTSticks.Left.X < -0.5f) || lastKeybState.IsKeyUp(Keys.Left) && currentKeybState.IsKeyDown(Keys.Left))
                && activeMenu != menuList.ElementAt<MenuWindow>(2)
                && activeMenu != menuList.ElementAt<MenuWindow>(0))
            {
                activeMenu.windowState = MenuWindow.WindowState.Ending;
                if (activeMenu == menuList.ElementAt<MenuWindow>(1))
                    activeMenu = menuList.ElementAt<MenuWindow>(sysIndex + 10);                  // we add 8 here because the sysIndex is not a 0 based offset and we have 9 menus occupying space in the list before the rom menus start in our menuList
                else if (activeMenu == menuList[4])
                    activeMenu = menuList[3];
                else if (activeMenu == menuList[5])
                    activeMenu = menuList[4];
                                                       else if (activeMenu == menuList[6])
                    activeMenu = menuList[3];
                else if (activeMenu == menuList[7])
                    activeMenu = menuList[6];
                else if ((activeMenu == menuList[8] || activeMenu == menuList[9]) && !inputControl.isSetting)
                    activeMenu = menuList[7];
                else if (inputControl.isSetting)
                    return;
                else
                    activeMenu = menuList.ElementAt<MenuWindow>(0);
                activeMenu.WakeUp();
                if(isSoundOn)
                    menuSoundEffect.Play();
                newActive = activeMenu;
                particleEngine.StopEffects();
                if (logoState == LogoState.Active)
                    logoState = LogoState.Ending;
                waitProgress = 0;

                if (activeMenu != menuList[2] && hasCaptured)
                    hasCaptured = false;
            }

            if (newActive == null || hasWaitedToCapture)
            {
                if (mustWaitToCapture)
                {
                    mustWaitToCapture = false;
                    hasWaitedToCapture = true;
                    newActive = activeMenu;
                }
                else
                {
                    particleEngine.StopEffects();
                    activeMenu.windowState = MenuWindow.WindowState.Ending;
                    activeMenu = menuList.ElementAt<MenuWindow>(2);
                    activeMenu.WakeUp();
                    if(isSoundOn)
                        menuSoundEffect.Play();
                    newActive = activeMenu;

                    //if (isBloomSoft)
                    /*{
                        bloomSettingsIndex = (bloomSettingsIndex + 1) %
                        BloomSettings.PresetSettings.Length;

                        bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                        bloom.Visible = true;
                    }*/
                    hasWaitedToCapture = false;
                }
            }
            else if (newActive != activeMenu)
            {
                particleEngine.StopEffects();
                if (newActive == gameMenu)
                {
                    newActive.WakeUp(activeMenu.selectedItem, sysstruct[sysIndex].filepath, sysstruct[sysIndex].roms[romIndex].path);
                    if(isSoundOn)
                        menuSoundEffect.Play();
                }
                if (newActive == gameMenu2)
                {

                    if (State == GameState.EndScreen)
                        return;
                    newActive.WakeUp(activeMenu.selectedItem + 3, sysstruct[sysIndex].filepath, sysstruct[sysIndex].roms[romIndex].path);
                    if(isSoundOn)
                        menuSoundEffect.Play();
                }
                else if ((activeMenu != menuList[4] && newActive != menuList[4]) && newActive != menuList[5] && activeMenu != menuList[5] && (activeMenu != menuList[6] && newActive != menuList[6]) && (activeMenu != menuList[7] && newActive != menuList[7]))
                {
                    newActive.WakeUp();
                    if(isSoundOn)
                        menuSoundEffect.Play();
                }
                else
                {
                    if(newActive == menuList[5] && bloom.Visible == true)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * ((bloomSettingsIndex + 1) % 10)) + 20, 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if(newActive == menuList[4] && (isInvertedEffect || isTimedColors || isGlow || isDistortion || isBump || isGreenShift || isColorWring))
                    {
                        if(isInvertedEffect)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (2) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isTimedColors)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (3) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isGlow)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (4) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isBump)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (5) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isGreenShift)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (7) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isColorWring)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (6) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isDistortion)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (8) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                    }
                    else if (newActive == menuList[6] && (isSoundOn || isFullScreen))
                    {
                        if (isFullScreen)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (2) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                        if (isSoundOn)
                        {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (3) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                        }
                    }
                    if(isSoundOn)
                        menuSoundEffect.Play();
                }
            }

            if (newActive == activeMenu && (activeMenu == menuList[5] || activeMenu == menuList[4] || activeMenu == menuList[6] || activeMenu == menuList[7]))
            {
                if (activeMenu == menuList[5])
                {
                    for (int i = 0; i < particleEngine.effectList.Count - 1; i++)
                        particleEngine.StopEffects(i);
                }
                else if (activeMenu == menuList[4])
                {
                    particleEngine.StopEffects();
                    if (isInvertedEffect)
                    {
                            fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (2) + 20), 12, 10));
                            fireEffect.isSmallFileOn = true;
                    }
                    if (isTimedColors)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (3) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isGlow)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (4) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isBump)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (5) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isGreenShift)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (7) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isColorWring)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (6) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isDistortion)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (8) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                }
                else if(activeMenu == menuList[6])
                {
                    particleEngine.StopEffects();
                    if (isFullScreen)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (2) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                    if (isSoundOn)
                    {
                        fireEffect.sourceEmitters.Add(new Rectangle((int)newActive.horPosition1 + m_Width / 5, (int)((newActive.verPosition1) * (3) + 20), 12, 10));
                        fireEffect.isSmallFileOn = true;
                    }
                }
                }

            if (newActive.selectedItem != newActive.oldSelectedItem)
            {
                if (isSoundOn)
                    itemSoundEffect.Play();
            }

            if (activeMenu.isRomMenu)
                romIndex = activeMenu.selectedItem;
            if (activeMenu.isSysMenu)
                sysIndex = activeMenu.sysItem;

            activeMenu = newActive;
            if (activeMenu.windowState == MenuWindow.WindowState.Inactive)
                activeMenu.windowState = MenuWindow.WindowState.Starting;

        }

        public void IntroInput(KeyboardState currentKeybState, ButtonState[] bStates, DirectInputThumbSticks currentTSticks)
        {
            if ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)) ||
                (lastButtonStates[0] == ButtonState.Released && bStates[0] == ButtonState.Pressed))
            {
                MediaPlayer.Stop();
                over = true;
                State = GameState.MenuScreen;
            }
        }

        public void ScreenSaverInput(KeyboardState currentKeybState, ButtonState[] bStates, DirectInputThumbSticks currentTSticks)
        {
            if ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)) ||
                (lastButtonStates[0] == ButtonState.Released && bStates[0] == ButtonState.Pressed))
            {
                over = true;
                particleEngine.StopEffects();
                fireEffect.isSmallFileOn = false;
                fireEffect.isEffect = false;
                fireEffect.isTab = false;
                logoState = LogoState.Ending;
                RestoreSettings();
                State = GameState.MenuScreen;
                isDisplaySysInfo = false;
            }
            if ((lastKeybState.IsKeyUp(Keys.Right) && currentKeybState.IsKeyDown(Keys.Right)))
            {
                fireEffect.isAdding = true;
            }
            if ((lastKeybState.IsKeyUp(Keys.Left) && currentKeybState.IsKeyDown(Keys.Left)))
            {
                fireEffect.isSubtracting = true;
            }
            if ((lastKeybState.IsKeyUp(Keys.Tab) && currentKeybState.IsKeyDown(Keys.Tab)))
            {
                particleEngine.StopEffects();
                fireEffect.swt++;
                if (fireEffect.swt > 3)
                    fireEffect.swt = 0;
                fireEffect.isEffect = false;
                fireEffect.isTab = true;
            }
        }

        private void StoreSettings()
        {
            bloomVisHolder = bloom.Visible;
            bloomHolder = bloomSettingsIndex;
            sysFPSSetting = isDisplaySysInfo;
            distorHolder = isDistortion;
        }

        private void RestoreSettings()
        {
            bloom.Visible = bloomVisHolder;
            bloomSettingsIndex = bloomHolder;
            isDisplaySysInfo = sysFPSSetting;
            isDistortion = distorHolder;
        }

        /// <summary>
        /// Runs code within a given menu item. Ridiculous place to put this
        /// </summary>
        private void AdjustSettings(MenuWindow newActive)
        {
            if (newActive.itemList[newActive.selectedItem].itemText == "     Settings")
            {
                hasCaptured = false;
            }
            if (newActive.itemList[newActive.selectedItem].itemText == "   Screen Saver")
            {
                StoreSettings();
                State = Game1.GameState.SaveScreen;
                isDisplaySysInfo = true;
                hasCaptured = false;
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "     Main Menu" && newActive == menuList[2])
            {
                RestoreSettings();
                hasCaptured = false;
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Invert Colors")
            {
                isInvertedEffect = !isInvertedEffect;
                if (isInvertedEffect)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Timed Colors")
            {
                isTimedColors = !isTimedColors;
                if (isTimedColors)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == " Weak Glow")
            {
                isGlow = !isGlow;
                if (isGlow)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Bump Mapping")
            {
                isBump = !isBump;
                if (isBump)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == " Color Wring")
            {
                isColorWring = !isColorWring;
                if (isColorWring)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == " Green Shift")
            {
                isGreenShift = !isGreenShift;
                if (isGreenShift)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Distortion Blur")
            {
                isDistortion = !isDistortion;
                if (isDistortion)
                {
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "  Default")
            {
                isBloomDefault = !isBloomDefault;
                bloomSettingsIndex = 0;

                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                bloom.Visible = true;

                fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                fireEffect.isSmallFileOn = true;
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "   Soft")       // make sure you change the menu item here to reflect what the menu actually looks like
            {
                isBloomSoft = !isBloomSoft;
                //if (isBloomSoft)
                {
                    bloomSettingsIndex = 1;

                    bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                    bloom.Visible = true;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == " Saturated")
            {
                //if (isBloomSoft)
                {
                    bloomSettingsIndex = 2;

                    bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                    bloom.Visible = true;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Desaturated")
            {
                //if (isBloomSoft)
                {
                    bloomSettingsIndex = 3;

                    bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                    bloom.Visible = true;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "   Blurry")
            {
                //if (isBloomSoft)
                {
                    bloomSettingsIndex = 4;

                    bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                    bloom.Visible = true;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "  Subtle")
            {
                {
                    bloomSettingsIndex = 5;

                    bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                    bloom.Visible = true;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "   None")
            {
                bloom.Visible = false;
                particleEngine.StopEffects();
            }

            if (newActive.itemList[newActive.selectedItem].itemText == "Full Screen")
            {
                {
                    isFullScreen = !isFullScreen;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;

                    if (isFullScreen)
                    {
                        m_Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        m_Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

                        // load graphic settings
                        graphics.IsFullScreen = isFullScreen;
                        graphics.PreferredBackBufferWidth = m_Width;
                        graphics.PreferredBackBufferHeight = m_Height;
                        graphics.ApplyChanges();
                    }
                    else
                    {
                        // load graphic settings
                        graphics.IsFullScreen = isFullScreen;
                        graphics.ApplyChanges();
                    }
                }
            }

            if (newActive.itemList[newActive.selectedItem].itemText == " Play Sound")
            {
                {
                    isSoundOn = !isSoundOn;
                    fireEffect.sourceEmitters.Add(new Rectangle((int)activeMenu.horPosition1 + m_Width / 5, (int)((activeMenu.verPosition1) * ((activeMenu.selectedItem + 1) % 10)) + 20, 12, 10));
                    fireEffect.isSmallFileOn = true;
                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState KeybState = Keyboard.GetState();
            ButtonState[] buttonState = new ButtonState[9];
            DirectInputThumbSticks currentTSticks = new DirectInputThumbSticks();
            switch(State)
            {
                case GameState.IntroScreen:
                    KeybState = Keyboard.GetState();
                    buttonState = new ButtonState[9];
                    currentTSticks = new DirectInputThumbSticks();
                    if (DirectInputGamepad.Gamepads.Count > 0)
                    {
                        buttonState = gamePad[0].Buttons.List.ToArray();
                        currentTSticks = gamePad[0].ThumbSticks;
                    }

                    IntroInput(KeybState, buttonState, currentTSticks);
                    break;
                case GameState.MenuScreen:
                        // Allows the game to exit
                        if (activeMenu.isExit)
                        {
                            this.Exit();
                        }

                        waitProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 60000;
                        if (waitProgress >= 1.0f && logoState == LogoState.Inactive)
                        {
                            logoState = LogoState.Starting;
                        }
                    if(logoState == LogoState.Starting || logoState == LogoState.Ending)
                    {
                        logoFadeProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 5000;
                        if (logoFadeProgress >= 1.0f)
                        {
                            logoFadeProgress = 0;
                            if (logoState == LogoState.Ending)
                                logoState = LogoState.Inactive;
                            else
                                logoState = LogoState.Active;
                        }
                    }

                    if (logoState == LogoState.Active)
                    {
                        logoFadeProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 5000;
                        if (logoFadeProgress >= 1.0f)
                        {
                            screenSaveFadeProgress += gameTime.ElapsedGameTime.TotalMilliseconds / 3000;
                            if(screenSaveFadeProgress >= 1.0f)
                            {
                                screenSaveFadeProgress = 0.0f;
                                logoFadeProgress = 0.0f;
                                logoState = LogoState.SaveScreen;
                                StoreSettings();
                                State = GameState.SaveScreen;
                                isDisplaySysInfo = true;
                            }
                        }
                    }
                        KeybState = Keyboard.GetState();
                        buttonState = new ButtonState[9];
                        currentTSticks = new DirectInputThumbSticks();
                        if (DirectInputGamepad.Gamepads.Count > 0)
                        {
                            buttonState = gamePad[0].Buttons.List.ToArray();
                            currentTSticks = gamePad[0].ThumbSticks;
                        }

                        MenuInput(KeybState, buttonState, currentTSticks, gameTime);

                        foreach (MenuWin currentMenu in menuList)
                            currentMenu.Update(gameTime);

                        lastKeybState = KeybState;
                        lastButtonStates = buttonState;
                        lastTSticks = currentTSticks;

                        // and calculate fps
                        dLastUpdate = gameTime.ElapsedGameTime.TotalSeconds;
                        dFPS = (1 / dLastUpdate);

                        // finally update time for postprocessing effect, if used
                        time += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

                        // calculate wave formations and movement if that effect is being used
                        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Reset.
                        if (KeybState.IsKeyDown(Keys.Space))
                        {
                            Reset();
                        }

                        // Move the center.
                        centerCoord.X = MathHelper.Clamp(centerCoord.X +
                            (currentTSticks.Left.X * seconds * 0.5f),
                            0, 1);
                        centerCoord.Y = MathHelper.Clamp(centerCoord.Y -
                            (currentTSticks.Left.Y * seconds * 0.5f),
                            0, 1);

                        // Change the distortion.
                        distortion += currentTSticks.Left.X * seconds * 0.5f;

                        // Change the period.
                        divisor1 += currentTSticks.Left.Y * seconds * 0.5f;

                        divisor1 += 0.005f;
                        //wave = MathHelper.Pi / divisor;
                        wave = MathHelper.Pi / divisor1;
                        waveParam.SetValue(wave);
                        distortionParam.SetValue(distortion);
                        centerCoordParam.SetValue(centerCoord);
                        divisorParam.SetValue(divisor1);
                        // end wave calcing
                        break;
                case GameState.SaveScreen:
                        KeybState = Keyboard.GetState();
                        buttonState = new ButtonState[9];
                        currentTSticks = new DirectInputThumbSticks();
                        if (DirectInputGamepad.Gamepads.Count > 0)
                        {
                            buttonState = gamePad[0].Buttons.List.ToArray();
                            currentTSticks = gamePad[0].ThumbSticks;
                        }
                        ScreenSaverInput(KeybState, buttonState, currentTSticks);
                        lastKeybState = KeybState;
                        break;
                default:
                        break;
            }
            base.Update(gameTime);
        }
        #endregion
        #region Draw Everything
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (State)
            {
                case GameState.MenuScreen:
                    {
                        if (activeMenu.isExit)
                            return;
                        GraphicsDevice.Clear(Color.Black);

                        if (hasCaptured)
                        {
                            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

                            //spriteBatch.End();
                        }

                        spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                        List<string> ppEffectsList = new List<string>();
                        if (hasCaptured)                        // a method developed/removed tweaked with for a background image showing when the exit menu is up
                        {

                            //ppEffectsList.Add("DistortBlur");
                            //distorter.blurAmount = 20.0f;
                            //postProcessingFXInverse.Parameters["DistortionScale"].SetValue(5.25f);
                            //postProcessingFXInverse.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
                            //ppEffectsList.Add("Mosaic");
                            //PostProcess(ppEffectsList);
                            //ppEffectsList.Remove("DistortBlur");
                            //ppEffectsList.Remove("Mosaic");

                            spriteBatch.Draw(resolveTexture, new Rectangle(0, 0, m_Width, m_Height), null, new Color(Color.White, 0.1f), 0, Vector2.Zero, SpriteEffects.None, 0.99f);
                            
                            //spriteBatch.Draw(hatchTexture, new Rectangle(0, 0, m_Width, m_Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
                        }
                        foreach (MenuWin currentMenu in menuList)
                            currentMenu.Draw(spriteBatch, gameTime);
                        DrawSysInfo(spriteBatch, gameTime);
                        DrawLogo(spriteBatch, gameTime);
                        if (activeMenu == menuList[1])
                        {
                            if(noUsers > 0)
                                spriteBatch.Draw(controllerTexture, new Rectangle(m_Width / 3, 0, m_Width, m_Height), null, new Color(Color.White, 0.15f), 0, Vector2.Zero, SpriteEffects.None, 0);
                            else
                                spriteBatch.Draw(keyboardTexture, new Rectangle(m_Width / 3, 0, m_Width, m_Height), null, new Color(Color.White, 0.15f), 0, Vector2.Zero, SpriteEffects.None, 0);
                        }
                        if (activeMenu == menuList[4] || activeMenu == menuList[3] || activeMenu == menuList[6] || activeMenu == menuList[7])
                        {
                            spriteBatch.Draw(fancyLogo, new Rectangle(m_Width / 2 - m_Width / 3, m_Height / 2 - m_Width / 3, (int)(m_Width / 1.5), (int)(m_Width / 1.5)), null, new Color(Color.White, 0.07f), 0, Vector2.Zero, SpriteEffects.None, 1f);
                        }
                        if(activeMenu == menuList[5])           // I couldn't quickly figure out how to get this in in the menus--they're so convoluted now this was easiest
                            spriteBatch.Draw(bloomTexture, new Rectangle(m_Width / 3, 0, m_Width, m_Height), null, new Color(Color.White, 0.15f), 0, Vector2.Zero, SpriteEffects.None, 0);
                        spriteBatch.End();

                        // a collection of possible effects built so far--a system needs to be built to implement at users discretion
                        postProcessingFXInverse.Parameters["xTime"].SetValue(time);
                        if (isInvertedEffect)
                            ppEffectsList.Add("Invert");
                        if (isTimedColors)
                        {
                            ppEffectsList.Add("TimeChange");
                            postProcessingFXInverse.Parameters["xTime"].SetValue(time);
                        }
                        // note that the full blur effect requires both the hor and ver blurs; also note the glow requires blurring first
                        #region Glow
                        if (isGlow)
                        {
                            ppEffectsList.Add("HorBlur");
                            ppEffectsList.Add("VerBlurAndGlow");
                            postProcessingFXInverse.Parameters["xBlurSize"].SetValue(0.5f);
                        }
                        #endregion

                        if(isBump)
                            ppEffectsList.Add("Bump");
                        if(isColorWring)
                            ppEffectsList.Add("ColorWring");
                        if(isGreenShift)
                            ppEffectsList.Add("GreenShift");
                        if (divisor1 < 1.0f)
                            ppEffectsList.Add("Ripple");

                        #region Distortion Effect
                        // none of these work
                        if (isDistortion)
                        {
                            //ppEffectsList.Add("PullIn");
                            //ppEffectsList.Add("ZeroDisplacement");
                            //ppEffectsList.Add("DisplacementMapped");
                            //ppEffectsList.Add("HeatHaze");
                            distorter.blurAmount = 2.0f;
                            ppEffectsList.Add("DistortBlur");
                            postProcessingFXInverse.Parameters["DistortionScale"].SetValue(
                              distorter.DistortionScale);
                            postProcessingFXInverse.Parameters["Time"].SetValue(
                              (float)gameTime.TotalGameTime.TotalSeconds);
                        }
                        #endregion
                        if(!hasCaptured)
                            PostProcess(ppEffectsList);
                        break;
                    }
                case GameState.SaveScreen:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                    int total = 0;
                    int totalparticles = 0;
                    for(int i = 0; i < particleEngine.effectList.Count; i++)
                    {
                        total += particleEngine.effectList[i].particleList.Count;
                        totalparticles += particleEngine.effectList[i].NumberOfParticles;
                    }
                    if (fireEffect.swt == 2)
                    {
                        spriteBatch.DrawString(menuFont, "Total Particles Updated: " + total.ToString(), new Vector2(100, 100), Color.White);
                        spriteBatch.DrawString(menuFont, "Total Particles Generated/Update: " + totalparticles.ToString(), new Vector2(120, 120), Color.White);
                        spriteBatch.DrawString(menuFont, "Use RIGHT or LEFT arrow to stress test the system", new Vector2(120, m_Height - 80), Color.White);
                        DrawSysInfo(spriteBatch, gameTime);
                    }
                    if (fireEffect.swt == 0)
                    {
                        bloomSettingsIndex = 2;
                        bloom.Visible = true;
                    }
                    else
                        bloom.Visible = false;
                    spriteBatch.End();
                    break;
            }

            if (State == GameState.MenuScreen && (activeMenu == menuList[8] || activeMenu == menuList[9]))
                isDrawInput = true;
            else
                isDrawInput = false;

            base.Draw(gameTime);
            if (mustCapture)
            {
                spriteBatch.Begin();
                device.ResolveBackBuffer(resolveTexture, 0);
                mustCapture = false;
                hasCaptured = true;
                device.Textures[0] = resolveTexture;
                device.SetRenderTarget(0, null);
                spriteBatch.Draw(resolveTexture, new Rectangle(0, 0, m_Width, m_Height), Color.White);
                spriteBatch.End();
            }
            if (State == GameState.SaveScreen)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(menuFont, "Press TAB to see a new effect", new Vector2(100, m_Height - 100), Color.White);
                spriteBatch.End();
            }
        }

        private void DrawLogo(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (logoState)
            {
                case LogoState.Active:
                    spriteBatch.Draw(logoTexture, new Rectangle((int)(m_Width * 0.02), (int)(m_Height * 0.88), m_Width / 50, m_Width / 50), Color.White);
                    spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, m_Width, m_Height), new Color(Color.White, (float)screenSaveFadeProgress));
                    break;
                case LogoState.Starting:
                    spriteBatch.Draw(logoTexture, new Rectangle((int)(m_Width * 0.02), (int)(m_Height * 0.88), m_Width / 50, m_Width / 50), new Color(Color.White, (float)logoFadeProgress));
                    break;
                case LogoState.Ending:
                    spriteBatch.Draw(logoTexture, new Rectangle((int)(m_Width * 0.02), (int)(m_Height * 0.88), m_Width / 50, m_Width / 50), new Color(Color.White, 1.0f - (float)logoFadeProgress));
                    break;
                default:
                    break;
            }
        }

        public void DrawSysInfo(SpriteBatch spriteBatch, GameTime gameTime)
        {

            frameCounter++;
            frameTime += gameTime.ElapsedGameTime.Milliseconds;
            if (frameTime >= 1000)
            {
                currentFrameRate = frameCounter;
                frameTime = 0;
                frameCounter = 0;
            }

            if (isDisplaySysInfo)
            {
                spriteBatch.DrawString(menuFont, "FPS:  " + dFPS.ToString(), new Vector2(500, 10), new Color(Color.White, 150));
                spriteBatch.DrawString(menuFont, "DrawFrame:  " + currentFrameRate.ToString(), new Vector2(500, 25), new Color(Color.White, 150));
            }
        }

        /// <summary>
        /// handles all post processing after the draw sequence is finished
        /// </summary>
        private void PostProcess(List<string> ppEffectsList)
        {
            for (int currentTechnique = 0; currentTechnique < ppEffectsList.Count; currentTechnique++)
            {
                device.SetRenderTarget(0, null);
                Texture2D textureRenderedTo;

                if (currentTechnique == 0)
                {
                    device.ResolveBackBuffer(resolveTexture, 0);
                    textureRenderedTo = resolveTexture;
                    // in case a glow filter is called, set the original image so the effect file has it
                    postProcessingFXInverse.Parameters["originalImage"].SetValue(textureRenderedTo);
                }
                else
                {
                    textureRenderedTo = targetRenderedTo.GetTexture();
                }

                if (currentTechnique == ppEffectsList.Count - 1)
                    device.SetRenderTarget(0, null);
                else
                    device.SetRenderTarget(0, targetRenderedTo);

                postProcessingFXInverse.CurrentTechnique = postProcessingFXInverse.Techniques[ppEffectsList[currentTechnique]];
                postProcessingFXInverse.Begin();
                postProcessingFXInverse.Parameters["textureToSampleFrom"].SetValue(textureRenderedTo);
                postProcessingFXInverse.Parameters["backGroundTexture"].SetValue(textureRenderedTo);
                foreach (EffectPass pass in postProcessingFXInverse.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
                    device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, ppVertices, 0, 2);
                    pass.End();
                }
                postProcessingFXInverse.End();
            }
        }

        /// <summary>
        /// initialize postprocessing triangles that the effect classes will use
        /// </summary>
        private void InitPostProcessingVertices()
        {
            ppVertices = new VertexPositionTexture[4];
            int i = 0;
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        private void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = postProcessingFXInverse.Parameters["SampleWeights"];
            offsetsParameter = postProcessingFXInverse.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = distorter.ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = distorter.ComputeGaussian(i + 1);

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
        #endregion
        #region XML Handling
        public void ReadXML()
        {
            String endelementfield = "";
            String elementfield = "";
            {
                XmlDocument xml = new XmlDocument();
                xml.Load("BlackBaronSystem.xml");
                XmlNodeList NodesCount = xml.GetElementsByTagName("System");
                sysstruct = new SystemStruct[NodesCount.Count];

                XmlTextReader xliff = new XmlTextReader("BlackBaronSystem.xml");
                try
                {
                    int count = -1;
                    int romcount = 0;
                    while (xliff.Read())
                    {
                        switch (xliff.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                elementfield = xliff.Name;

                                while (xliff.MoveToNextAttribute()) // Read the attributes.
                                {
                                    if (elementfield == "System")       // right now only reads one attribute: the system name; could be changed with a switch statement later
                                    {
                                        count++;
                                        romcount = 0;
                                        sysstruct[count].systemname = xliff.Value;
                                    }
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
                                        break;

                                    case "RomCount":
                                        int j;
                                        if (Int32.TryParse(xliff.Value.Replace("\r", "").Replace("\n", "").Trim(), out j))
                                            sysstruct[count].roms = new SystemStruct.Rom[j];
                                        else
                                        {
                                            System.Windows.Forms.MessageBox.Show("Parse Error, Dawg!");
                                            this.Exit();
                                        }
                                        break;

                                    case "Name":
                                        sysstruct[count].roms[romcount].name = xliff.Value;
                                        break;

                                    case "Alias":
                                        sysstruct[count].roms[romcount].alias = xliff.Value;
                                        break;

                                    case "ScreenshotFilePath":
                                        sysstruct[count].roms[romcount].screenfile = xliff.Value;
                                        break;

                                    case "PhotoFilePath":
                                        sysstruct[count].roms[romcount].photofile = xliff.Value;
                                        break;

                                    case "Description":
                                        sysstruct[count].roms[romcount].description = xliff.Value;
                                        break;

                                    case "SysCount":
                                        int l;
                                        if (Int32.TryParse(xliff.Value.Replace("\r", "").Replace("\n", "").Trim(), out l))
                                            sysstruct = new SystemStruct[l];
                                        else
                                        {
                                            System.Windows.Forms.MessageBox.Show("Parse Error, Dawg!");
                                            this.Exit();
                                        }                                        
                                        break;

                                    case "Count":
                                        int result = 0;
                                        if (!Int32.TryParse(xliff.Value, out result))
                                        {
                                            System.Windows.Forms.MessageBox.Show("Parse Error, Dawg!");
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
                    System.Windows.Forms.MessageBox.Show("Error parsing inbound XML file:" + ex.Message, "inbound parse error");
                }

                finally
                {
                    xliff.Close();
                }
            }
        }
        #endregion
    }
}

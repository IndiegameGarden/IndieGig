// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TTengine.Core;
using TTengine.Comps;
using TTengine.Systems;
using TTengine.Behaviors;
using TTengine.Modifiers;
using TTengine.Util;
using Artemis;
using Artemis.Interface;
using TreeSharp;
using IndiegameGarden.Base;
using Game1.Comps;
using Game1.Systems;

namespace Game1
{
    /// <summary>
    /// Main game class for IndieGig
    /// </summary>
    public class Game1 : TTGame
    {
        public enum GlobalStateEnum
        {
            STATE_BROWSING,
            STATE_LAUNCHING,
            STATE_CONFIGURING,
            STATE_PLAYING_PHASE1,
            STATE_PLAYING_PHASE2
        }
        public static Game1 InstanceGame1;
        public Game1Factory Factory;
        public GardenConfig Config;
        public IndieGigCollection Collection;
        public Channel MainChannel, WaitChannel;
        public Entity MousePointer;
        public Entity SelectedGame, KeyboardSelectedGame;
        public Entity Music;
        public Entity BackgroundGameIcon, BackgroundGameIconOld, BackgroundRotatingStar;
        public Entity TopLineText, HelpText;
        public List<Entity> CollectionEntities; // all entities in game library
        public GlobalStateEnum GlobalState;
        public GameRunner GameRunner;
        public int  KeyboardSelectedGameIndex = 0;
        public Vector2 LastMousePos;
        public bool IsExiting = false,
                    CanExit = false,
                    IsMouseForSelection = false,
                    IsReadyForKeypress = true;

        // UI constants
        public const double ICON_SIZE = 128, // pixels

                            SCALE_SELECTED = 1.2,
                            SCALE_UNSELECTED = 1.0,
                            SCALE_WHILE_EXITING = 0.0002,
                            SCALE_TO_BACKGROUND = SCALE_UNSELECTED,
                            SCALE_TO_FOREGROUND = 1.42,

                            SPEED_SCALE_TO_SELECTED = 0.1,
                            SPEED_SCALE_TO_UNSELECTED = 0.1,
                            SPEED_SCALE_ICON_TO_FOREGROUND = 0.005,
                            SPEED_SCALE_ICON_TO_BACKGROUND = 0.0025,
                            SPEED_SCALE_ICON_WHILE_EXITING = 0.1,
                            
                            BACKGROUND_STAR_ROTATION_SPEED = 0.05,
                            BACKGROUND_ICON_ROTATION_SPEED = 0.04,
                            BACKGROUND_ROTATION_SLOWDOWN_SPEED = 0.01,
                            BACKGROUND_ROTATION_SPEEDUP_SPEED = 0.01,
                            ROTATION_SPEEDUP_FACTOR_WHILE_LAUNCHING = 2.0,
                            ROTATION_SLOWDOWN_FACTOR_WHILE_PLAYING = 2.0,
                            MOVE_ICONS_ON_EXIT_SPEED = 800.0,
                            MUSIC_FADEOUT_ON_EXIT_SPEED = 0.45;
        public double       SCALE_MAX = Math.Max(SCALE_SELECTED, SCALE_TO_FOREGROUND);
        public const int    ICONCOUNT_HORIZONTAL = 9; // FIXME make adaptive to screen size

        public Game1()
        {
            InstanceGame1 = this;
        }

        /// <summary>
        /// Init of basics: non-graphical, non-XNA, non-channel related 
        /// </summary>
        protected override void Initialize()
        {
            IsAudio = true;
            IsMouseVisible = true;
            Window.AllowAltF4 = false;
            GlobalState = GlobalStateEnum.STATE_BROWSING;
            Factory = Game1Factory.Instance;
            Config = GardenConfig.Instance;
            GameRunner = new GameRunner();
            if (!Config.VerifyStorageFolders())
            {
                MsgBox.Show( Config.ClientName + " ERROR", "Could not create required temp folder:\n" + Config.TempFolder);
                Exit();
            }

            base.Initialize();
        }

        /// <summary>
        /// Load content, levels, all channel creation - most should be in here
        /// </summary>
        protected override void LoadContent()
        {
            // game db
            Collection = new IndieGigCollection();

            // main (menu) channel
            MainChannel = TTFactory.CreateChannel(Color.Black, false);
            ChannelMgr.AddChannel(MainChannel);

            // background icon
            BackgroundGameIcon = Factory.CreateBackgroundGameIcon();
            BackgroundGameIconOld = Factory.CreateBackgroundGameIcon();

            // create collection onto channel
            var iconsLayer = Factory.CreateIconsLayer();
            TTFactory.BuildTo(iconsLayer.GetComponent<ScreenComp>());
            CollectionEntities = Factory.CreateCollection(Collection);
            TTFactory.BuildTo(MainChannel);

            // mouse pointer entity            
            MousePointer = Factory.CreateMousePointer();            

            // text
            TopLineText = TTFactory.CreateTextlet("IndieGig", "m41_lovebit");
            TopLineText.GetComponent<PositionComp>().Position = new Vector2(80f, 20f);
            HelpText = TTFactory.CreateTextlet("");
            HelpText.GetComponent<PositionComp>().Depth = 0f;
            HelpText.GetComponent<PositionComp>().Position = new Vector2(TTFactory.BuildScreen.Width-400f, 20f);

            // music - build it to Root channel so it keeps playing always
            TTFactory.BuildTo(ChannelMgr.Root);
            Music = Factory.CreateMusic();
            TTFactory.BuildTo(MainChannel);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            EscapeKeyProcess(dt);
            
            switch (GlobalState)
            {
                case GlobalStateEnum.STATE_BROWSING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    GameSelectionProcess();
                    BackgroundGameIconFadeTextureProcess();
                    if (!IsExiting)
                    {                        
                        GameLaunchingProcess();                        
                        //BackgroundGameIconNewTextureProcess();
                        //BackgroundSpeedupRotationProcess(dt);
                    }
                    break;

                case GlobalStateEnum.STATE_CONFIGURING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    GameRunProcess(true);
                    IconsShrinkingProcess();
                    //BackgroundGameIconNewTextureProcess();
                    break;

                case GlobalStateEnum.STATE_LAUNCHING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    //TopLineText.GetComponent<TextComp>().Text = "Launching";
                    GameRunProcess();
                    IconsShrinkingProcess();
                    //BackgroundGameIconNewTextureProcess();
                    //BackgroundSpeedupRotationProcess(dt, ROTATION_SPEEDUP_FACTOR_WHILE_LAUNCHING);
                    break;

                case GlobalStateEnum.STATE_PLAYING_PHASE1:
                    //TopLineText.GetComponent<TextComp>().Text = "Playing";
                    //BackgroundSlowdownRotationProcess(dt, ROTATION_SLOWDOWN_FACTOR_WHILE_PLAYING);
                    StatePlayingPhase1Process();
                    break;
                
                case GlobalStateEnum.STATE_PLAYING_PHASE2:
                    MainChannel.IsActive = false; MainChannel.IsVisible = false;
                    break;
            }
            
            base.Update(gameTime);
        }

        void EscapeKeyProcess(double dt)
        {
            CanExit = true; // (BackgroundRotatingStar.GetComponent<RotateComp>().RotateSpeed > 0.02);

            if (CanExit)
            {
                KeyboardState kb = Keyboard.GetState();
                if (kb.IsKeyDown(Keys.Escape) ||
                    ((kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt)) && kb.IsKeyDown(Keys.F4)))
                {
                    var afc = Music.GetComponent<AudioFadingComp>();
                    afc.FadeTarget = 0;
                    afc.FadeSpeed = MUSIC_FADEOUT_ON_EXIT_SPEED;
                    afc.IsFading = true;
                    IsExiting = true;
                    TopLineText.GetComponent<TextComp>().Text = "Bye  :-)";

                    // disable the regular layouter process
                    ChannelMgr.Root.DisableSystem<BasicLayouterSystem>();

                    // icons shrink
                    foreach (Entity e in CollectionEntities)
                    {
                        e.GetComponent<ScaleComp>().ScaleTarget = SCALE_WHILE_EXITING;
                        e.GetComponent<ScaleComp>().ScaleSpeed = SPEED_SCALE_ICON_WHILE_EXITING;
                        //var x = e.GetComponent<PositionComp>().Position.X;
                        //e.GetComponent<TargetMotionComp>().Target = new Vector2(x/10f,-450f-x); // fly away to top
                        //e.GetComponent<TargetMotionComp>().TargetVelocity = MOVE_ICONS_ON_EXIT_SPEED;
                    }
                }
            }
            if (IsExiting) 
            {
                //BackgroundSlowdownRotationProcess(dt,2.0);
                if (Music.GetComponent<AudioComp>().Ampl == 0 /*&&
                    BackgroundRotatingStar.GetComponent<RotateComp>().RotateSpeed == 0 */ )
                    Exit();
            }
        }

        void GameSelectionProcess()
        {
            // keyb input
            KeyboardState kb = Keyboard.GetState();
            int d = 0;
            if (IsReadyForKeypress)
            {
                if (kb.IsKeyDown(Keys.Right))   d = +1;
                if (kb.IsKeyDown(Keys.Left))    d = -1;
                if (kb.IsKeyDown(Keys.Up))      d = -ICONCOUNT_HORIZONTAL;
                if (kb.IsKeyDown(Keys.Down))    d = +ICONCOUNT_HORIZONTAL;
                if (kb.IsKeyDown(Keys.C))   // config
                {
                    if (SelectedGame != null)
                        GlobalState = GlobalStateEnum.STATE_CONFIGURING;
                }
                else if (kb.IsKeyDown(Keys.Enter) ||
                    kb.IsKeyDown(Keys.Z) ||
                    kb.IsKeyDown(Keys.Space) )
                {
                    if (IsMouseForSelection)
                    {
                        IsMouseForSelection = false;
                        IsReadyForKeypress = false;
                    }
                    else
                        GlobalState = GlobalStateEnum.STATE_LAUNCHING;
                }
                if (d != 0)
                {
                    IsReadyForKeypress = false;
                    if ((d + KeyboardSelectedGameIndex) >= 0 && (d + KeyboardSelectedGameIndex) < Collection.Count)
                    {
                        KeyboardSelectedGameIndex += d;
                        KeyboardSelectedGame = CollectionEntities[KeyboardSelectedGameIndex];
                        IsMouseForSelection = false;
                    }
                }
            }
            if (kb.GetPressedKeys().Length == 0)
                IsReadyForKeypress = true;

            // mouse input
            var pos = MousePointer.GetComponent<PositionComp>().Position;
            if (!pos.Equals(LastMousePos) && MousePointer.GetComponent<ScriptComp>().SimTime > 0.3 )
            {
                // only allow selection by mouse if mouse moved and if initial startup period of icons shuffling is over
                IsMouseForSelection = true;
            }
            LastMousePos = pos;

            // use input
            List<Entity> userSelectedItems ;
            if (IsMouseForSelection)
            {
                userSelectedItems = MousePointer.GetComponent<SpriteComp>().Colliders;
                IsMouseVisible = true;
            }
            else
            {
                IsMouseVisible = false;
                userSelectedItems = new List<Entity>();
                userSelectedItems.Add(KeyboardSelectedGame);
            }

            Entity currentSelectedGame = null;
            foreach (Entity e in CollectionEntities)
            {
                var sc = e.GetComponent<ScaleComp>();

                // test if user has made game e as a selection
                if (currentSelectedGame == null && userSelectedItems.Contains(e))
                {
                    currentSelectedGame = e;
                    if (!IsExiting)
                    {
                        sc.ScaleTarget = SCALE_SELECTED;
                        sc.ScaleSpeed = SPEED_SCALE_TO_SELECTED;
                    }

                    // if selection of user changed...
                    if (currentSelectedGame != SelectedGame)
                    {
                        // swap trick
                        Entity tmp;
                        tmp = BackgroundGameIconOld;
                        BackgroundGameIconOld = BackgroundGameIcon;
                        BackgroundGameIcon = tmp;

                        // new icon to be moved to foreground                   
                        BackgroundGameIcon.GetComponent<SpriteComp>().Texture = currentSelectedGame.GetComponent<SpriteComp>().Texture;
                        BackgroundGameIcon.GetComponent<DrawComp>().Alpha = 0f;

                        SelectedGame = currentSelectedGame;
                    }

                    if (IsMouseForSelection)
                    {
                        KeyboardSelectedGame = currentSelectedGame;
                        KeyboardSelectedGameIndex = CollectionEntities.IndexOf(KeyboardSelectedGame);
                    }
                }
                else if (!IsExiting)
                {
                    sc.ScaleTarget = SCALE_UNSELECTED;
                    sc.ScaleSpeed = SPEED_SCALE_TO_UNSELECTED;
                }

                // biggest thing shows on top
                var pc = e.GetComponent<PositionComp>();
                pc.Depth = (float)((SCALE_MAX - sc.Scale) / SCALE_MAX);
            }

            HelpText.GetComponent<TextComp>().Text = "";
            if (SelectedGame != null)
            {
                var gi = SelectedGame.GetComponent<GardenItemComp>();
                if (gi.Item.ExeConfig.Length > 0) {
                    HelpText.GetComponent<TextComp>().Text = "\"C\" to run game config";
                }
            }
        }

        void GameLaunchingProcess()
        {
            if (this.IsActive && !this.IsExiting)
            {
                KeyboardState kb = Keyboard.GetState();
                MouseState ms = Mouse.GetState();

                if (SelectedGame != null && ms.LeftButton == ButtonState.Pressed)
                {
                    GlobalState = GlobalStateEnum.STATE_LAUNCHING;
                }
            }
        }

        void BackgroundGameIconFadeTextureProcess()
        {
            // FIXME more efficient and check why drawcolor doesnt work
            var dc = BackgroundGameIcon.GetComponent<DrawComp>();
            if (dc != null)
            {
                var a = dc.Alpha;
                if (a < 1f)
                {
                    a += (1f / 255f);
                    if (a >= 1f) a = 1f;
                    dc.DrawColor = Color.White * a;
                }
            }

            var dcOld = BackgroundGameIconOld.GetComponent<DrawComp>();
            if (dcOld != null)
            {
                var a = dcOld.Alpha;
                if (a > 0f)
                {
                    a -= (1f / 255f);
                    if (a <= 0f) a = 0f;
                    dcOld.DrawColor = Color.White * a;
                }
            }

        }

        /// <summary>
        /// gradually set the texture of the selected game onto the BackgroundGameIcon
        /// </summary>
        void BackgroundGameIconNewTextureProcess()
        {
            if (SelectedGame != null)
            {
                var sc_dest = BackgroundGameIcon.GetComponent<SpriteComp>();
                var sc_src = SelectedGame.GetComponent<SpriteComp>();
                const float LERP_FACTOR = 0.97f;

                Vector2 dir = RandomMath.RandomDirection();

                const float DSTEP = (1f / (float)ICON_SIZE);
                for (float d = -0.5f; d <= 0.5f; d += DSTEP)
                {
                    int x = (int)(sc_dest.Width * (d * dir.X + 0.5f));
                    int y = (int)(sc_dest.Height * (d * dir.Y + 0.5f));
                    Color px = sc_src.GetPixel(x, y);
                    Color px2 = sc_dest.GetPixel(x, y);
                    sc_dest.SetPixel(x, y, Color.Lerp(px, px2, LERP_FACTOR));
                }
                /*
                if (RandomMath.RandomUnit() < 0.5)
                {
                    int x = RandomMath.RandomIntBetween(0, sc_dest.Width);
                    for (int y = sc_dest.Height; y >= 0; y--)
                    {
                        Color px = sc_src.GetPixel(x, y);
                        Color px2 = sc_dest.GetPixel(x, y);
                        sc_dest.SetPixel(x, y, Color.Lerp(px, px2, LERP_FACTOR));
                    }
                }
                else
                {
                    int y = RandomMath.RandomIntBetween(0, sc_dest.Height);
                    for (int x = sc_dest.Width; x >= 0; x--)
                    {
                        Color px = sc_src.GetPixel(x, y);
                        Color px2 = sc_dest.GetPixel(x, y);
                        sc_dest.SetPixel(x, y, Color.Lerp(px, px2, LERP_FACTOR));
                    }
                }
                */
            }
        }

        /// <summary>
        /// Once a game is selected for launch, this will install/run it
        /// </summary>
        void GameRunProcess(bool isConfigure = false)
        {
            if (SelectedGame != null)
            {
                GardenItem gi = SelectedGame.GetComponent<GardenItemComp>().Item;
                GameRunner.TryRunGame(gi,isConfigure);
            }
        }

        void BackgroundSlowdownRotationProcess(double dt, double multiplier = 1.0 )
        {
            var rc1 = BackgroundGameIcon.GetComponent<RotateComp>();
            var rc2 = BackgroundRotatingStar.GetComponent<RotateComp>();
            double dr = -BACKGROUND_ROTATION_SLOWDOWN_SPEED * multiplier * dt;
            if (rc1.RotateSpeed > 0)
                rc1.RotateSpeed += dr;
            if (rc2.RotateSpeed > 0)
                rc2.RotateSpeed += dr;
            if (rc1.RotateSpeed < 0)
                rc1.RotateSpeed = 0;
            if (rc2.RotateSpeed < 0)
                rc2.RotateSpeed = 0;
        }

        void BackgroundSpeedupRotationProcess(double dt, double multiplier = 1.0)
        {
            var rc1 = BackgroundGameIcon.GetComponent<RotateComp>();
            var rc2 = BackgroundRotatingStar.GetComponent<RotateComp>();
            double dr = multiplier * BACKGROUND_ROTATION_SPEEDUP_SPEED * dt;
            var spd1 = multiplier * BACKGROUND_ICON_ROTATION_SPEED;
            var spd2 = multiplier * BACKGROUND_STAR_ROTATION_SPEED;
            if (rc1.RotateSpeed < spd1)
                rc1.RotateSpeed += dr;
            if (rc2.RotateSpeed < spd2)
                rc2.RotateSpeed += dr;
            if (rc1.RotateSpeed > spd1)
                rc1.RotateSpeed = spd1;
            if (rc2.RotateSpeed > spd2)
                rc2.RotateSpeed = spd2;
        }

        void StatePlayingPhase1Process()
        {
            // when music has stopped - move to next phase (still screen)
            if (Music.GetComponent<AudioComp>().Ampl <= 0)
            {
                this.GlobalState = GlobalStateEnum.STATE_PLAYING_PHASE2;
            }
        }

        void IconsShrinkingProcess()
        {
            
            foreach (Entity e in CollectionEntities)
            {
                var sc = e.GetComponent<ScaleComp>();
                if (SelectedGame != e)
                {
                    sc.ScaleTarget = SCALE_TO_BACKGROUND;
                    sc.ScaleSpeed = SPEED_SCALE_ICON_TO_BACKGROUND;
                }
                else
                {
                    sc.ScaleTarget = SCALE_TO_FOREGROUND;
                    sc.ScaleSpeed = SPEED_SCALE_ICON_TO_FOREGROUND;
                }
                var pc = e.GetComponent<PositionComp>();
                pc.Depth = (float)((SCALE_MAX - sc.Scale) / SCALE_MAX);
            }
             
        }

    }

}

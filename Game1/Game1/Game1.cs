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
        public Entity BackgroundGameIcon, BackgroundGameIconOld;
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
            BackgroundGameIconOld = Factory.CreateBackgroundGameIcon2();

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
                    }
                    break;

                case GlobalStateEnum.STATE_CONFIGURING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    GameRunProcess(true);
                    BackgroundGameIconFadeTextureProcess();
                    IconsShrinkingProcess();
                    break;

                case GlobalStateEnum.STATE_LAUNCHING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    GameRunProcess();
                    BackgroundGameIconFadeTextureProcess();
                    IconsShrinkingProcess();
                    break;

                case GlobalStateEnum.STATE_PLAYING_PHASE1:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    StatePlayingPhase1Process();
                    BackgroundGameIconFadeTextureProcess();
                    break;
                
                case GlobalStateEnum.STATE_PLAYING_PHASE2:
                    MainChannel.IsActive = false; MainChannel.IsVisible = false;
                    break;
            }
            
            base.Update(gameTime);
        }

        void EscapeKeyProcess(double dt)
        {
            CanExit = true; 

            if (CanExit)
            {
                KeyboardState kb = Keyboard.GetState();
                if (kb.IsKeyDown(Keys.Escape) ||
                    ((kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt)) && kb.IsKeyDown(Keys.F4)))
                {
                    var afc = Music.GetComponent<AudioFadingComp>();
                    afc.FadeTarget = 0;
                    afc.FadeSpeed = GUIconstants.MUSIC_FADEOUT_ON_EXIT_SPEED;
                    afc.IsFading = true;
                    IsExiting = true;
                    TopLineText.GetComponent<TextComp>().Text = "Bye  :-)";

                    // disable the regular layouter process
                    ChannelMgr.Root.DisableSystem<BasicLayouterSystem>();

                    // icons shrink
                    foreach (Entity e in CollectionEntities)
                    {
                        e.GetComponent<ScaleComp>().ScaleTarget = GUIconstants.SCALE_WHILE_EXITING;
                        e.GetComponent<ScaleComp>().ScaleSpeed = GUIconstants.SPEED_SCALE_ICON_WHILE_EXITING;
                    }
                }
            }
            if (IsExiting) 
            {
                if (Music.GetComponent<AudioComp>().Ampl == 0 )
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
                if (kb.IsKeyDown(Keys.Up))      d = -GUIconstants.ICONCOUNT_HORIZONTAL;
                if (kb.IsKeyDown(Keys.Down))    d = +GUIconstants.ICONCOUNT_HORIZONTAL;
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
                        sc.ScaleTarget = GUIconstants.SCALE_SELECTED;
                        sc.ScaleSpeed = GUIconstants.SPEED_SCALE_TO_SELECTED;
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
                        (BackgroundGameIcon.GetComponent<ScriptComp>().Parent as ScriptComp).SimTime = 0; // reset flow script to beginning.
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
                    sc.ScaleTarget = GUIconstants.SCALE_UNSELECTED;
                    sc.ScaleSpeed = GUIconstants.SPEED_SCALE_TO_UNSELECTED;
                }

                // biggest thing shows on top
                var pc = e.GetComponent<PositionComp>();
                pc.Depth = (float)((GUIconstants.SCALE_MAX - sc.Scale) / GUIconstants.SCALE_MAX);
            }

            if (currentSelectedGame == null)
                SelectedGame = null;    // no selection is made currently.

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
                    sc.ScaleTarget = GUIconstants.SCALE_TO_BACKGROUND;
                    sc.ScaleSpeed = GUIconstants.SPEED_SCALE_ICON_TO_BACKGROUND;
                }
                else
                {
                    sc.ScaleTarget = GUIconstants.SCALE_TO_FOREGROUND;
                    sc.ScaleSpeed = GUIconstants.SPEED_SCALE_ICON_TO_FOREGROUND;
                }
                var pc = e.GetComponent<PositionComp>();
                pc.Depth = (float)((GUIconstants.SCALE_MAX - sc.Scale) / GUIconstants.SCALE_MAX);
            }
             
        }

    }

}

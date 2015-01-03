// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

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
            STATE_PLAYING
        }
        public static Game1 InstanceGame1;
        public Game1Factory Factory;
        public GardenConfig Config;
        public IndieGigCollection Collection;
        public Channel MainChannel, WaitChannel;
        public Entity MousePointer;
        public Entity SelectedGame, KeyboardSelectedGame;
        public Entity Music;
        public Entity BackgroundGameIcon, BackgroundRotatingStar;
        public List<Entity> CollectionEntities; // all entities in game library
        public GlobalStateEnum GlobalState;
        public GameRunner GameRunner;
        public int  KeyboardSelectedGameIndex = 0;
        public Vector2 LastMousePos;
        public bool IsExiting = false,
                    CanExit = false,
                    IsMouseForSelection = true,
                    IsReadyForKeypress = true;

        // UI constants
        public const double ICON_SIZE = 128, // pixels
                            SCALE_SELECTED = 1.2,
                            SCALE_SPEED_TO_SELECTED = 0.1,
                            SCALE_UNSELECTED = 1.0,
                            SCALE_SPEED_TO_UNSELECTED = 0.1,
                            BACKGROUND_STAR_ROTATION_SPEED = 0.05,
                            BACKGROUND_ICON_ROTATION_SPEED = 0.04,
                            BACKGROUND_ROTATION_SLOWDOWN_SPEED = 0.01,
                            BACKGROUND_ROTATION_SPEEDUP_SPEED = 0.01;
        public const int    ICONCOUNT_HORIZONTAL = 6;

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
            //gameChannel.DisableSystem<SpriteCollisionSystem>();

            // background
            BackgroundGameIcon = Factory.CreateBackgroundGameIcon();
            BackgroundRotatingStar = Factory.CreateBackgroundRotatingStar();

            // create collection onto channel
            var iconsLayer = Factory.CreateIconsLayer();
            TTFactory.BuildTo(iconsLayer.GetComponent<ScreenComp>());
            CollectionEntities = Factory.CreateCollection(Collection);
            TTFactory.BuildTo(MainChannel);

            // mouse pointer entity            
            MousePointer = Factory.CreateMousePointer();

            // text
            var t = TTFactory.CreateTextlet("IndieGig", "m41_lovebit");
            t.GetComponent<PositionComp>().Position2D = new Vector2(80f, 20f);

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
                    GameLaunchingProcess();
                    BackgroundGameIconNewTextureProcess();
                    BackgroundSpeedupRotationProcess(dt);
                    break;

                case GlobalStateEnum.STATE_LAUNCHING:
                    MainChannel.IsActive = true; MainChannel.IsVisible = true;
                    GameRunProcess();
                    BackgroundGameIconNewTextureProcess();
                    BackgroundSlowdownRotationProcess(gameTime.ElapsedGameTime.TotalSeconds);
                    break;

                case GlobalStateEnum.STATE_PLAYING:
                    MainChannel.IsActive = false; MainChannel.IsVisible = false;
                    break;
            }
            
            base.Update(gameTime);
        }

        void EscapeKeyProcess(double dt)
        {
            CanExit = (BackgroundRotatingStar.GetComponent<RotateComp>().RotateSpeed > 0.02) ;

            if (CanExit)
            {
                KeyboardState kb = Keyboard.GetState();
                if (kb.IsKeyDown(Keys.Escape) ||
                    ((kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt)) && kb.IsKeyDown(Keys.F4)))
                {
                    var afc = Music.GetComponent<AudioFadingComp>();
                    afc.FadeTarget = 0;
                    afc.FadeSpeed = 0.9;
                    afc.IsFading = true;
                    IsExiting = true;
                }
            }
            if (IsExiting) 
            {
                BackgroundSlowdownRotationProcess(dt);
                BackgroundSlowdownRotationProcess(dt);
                if (Music.GetComponent<AudioComp>().Ampl == 0)
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
                if (kb.IsKeyDown(Keys.Enter))
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
            Vector2 pos = MousePointer.GetComponent<PositionComp>().Position2D;
            if (!pos.Equals(LastMousePos))
            {
                IsMouseForSelection = true;
            }
            LastMousePos = pos;

            // use input
            List<Entity> coll ;
            if (IsMouseForSelection)
            {
                coll = MousePointer.GetComponent<SpriteComp>().Colliders;
                IsMouseVisible = true;
            }
            else
            {
                IsMouseVisible = false;
                coll = new List<Entity>();
                coll.Add(KeyboardSelectedGame);
            }
            SelectedGame = null;
            foreach (Entity e in CollectionEntities)
            {
                var sc = e.GetComponent<ScaleComp>();
                if (SelectedGame == null && coll.Contains(e))
                {
                    sc.ScaleTarget = SCALE_SELECTED;
                    sc.ScaleSpeed = SCALE_SPEED_TO_SELECTED;
                    SelectedGame = e;
                    BackgroundGameIcon.GetComponent<SpriteComp>().CenterToMiddle();
                    if (IsMouseForSelection)
                    {
                        KeyboardSelectedGame = SelectedGame;
                        KeyboardSelectedGameIndex = CollectionEntities.IndexOf(KeyboardSelectedGame);
                    }
                }
                else
                {
                    sc.ScaleTarget = SCALE_UNSELECTED;
                    sc.ScaleSpeed = SCALE_SPEED_TO_UNSELECTED;
                }

                var dc = e.GetComponent<DrawComp>();
                dc.LayerDepth = (float) ((SCALE_SELECTED-sc.Scale)/SCALE_SELECTED);
            }
        }

        void GameLaunchingProcess()
        {
            if (this.IsActive)
            {
                KeyboardState kb = Keyboard.GetState();
                MouseState ms = Mouse.GetState();

                if (SelectedGame != null && ms.LeftButton == ButtonState.Pressed)
                {
                    GlobalState = GlobalStateEnum.STATE_LAUNCHING;
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
        void GameRunProcess()
        {
            if (SelectedGame != null)
            {
                GardenItem gi = SelectedGame.GetComponent<GardenItemComp>().Item;
                GameRunner.TryRunGame(gi);
            }
        }

        void BackgroundSlowdownRotationProcess(double dt)
        {
            var rc1 = BackgroundGameIcon.GetComponent<RotateComp>();
            var rc2 = BackgroundRotatingStar.GetComponent<RotateComp>();
            double dr = -BACKGROUND_ROTATION_SLOWDOWN_SPEED * dt;
            if (rc1.RotateSpeed > 0)
                rc1.RotateSpeed += dr;
            if (rc2.RotateSpeed > 0)
                rc2.RotateSpeed += dr;
            if (rc1.RotateSpeed < 0)
                rc1.RotateSpeed = 0;
            if (rc2.RotateSpeed < 0)
                rc2.RotateSpeed = 0;
        }

        void BackgroundSpeedupRotationProcess(double dt)
        {
            var rc1 = BackgroundGameIcon.GetComponent<RotateComp>();
            var rc2 = BackgroundRotatingStar.GetComponent<RotateComp>();
            double dr = BACKGROUND_ROTATION_SPEEDUP_SPEED * dt;
            if (rc1.RotateSpeed < BACKGROUND_ICON_ROTATION_SPEED)
                rc1.RotateSpeed += dr;
            if (rc2.RotateSpeed < BACKGROUND_STAR_ROTATION_SPEED)
                rc2.RotateSpeed += dr;
            if (rc1.RotateSpeed > BACKGROUND_ICON_ROTATION_SPEED)
                rc1.RotateSpeed = BACKGROUND_ICON_ROTATION_SPEED;
            if (rc2.RotateSpeed > BACKGROUND_STAR_ROTATION_SPEED)
                rc2.RotateSpeed = BACKGROUND_STAR_ROTATION_SPEED;
        }

    }

}

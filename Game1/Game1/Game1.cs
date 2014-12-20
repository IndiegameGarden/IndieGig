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
            STATE_LAUNCHING
        }
        public Game1Factory Factory;
        public GardenConfig Config;
        public IndieGigCollection Collection;
        public Channel GameChannel;
        public Entity MousePointer;
        public Entity SelectedGame;
        public Entity Music;
        public List<Entity> CollectionEntities; // all entities in game library
        public GlobalStateEnum GlobalState;
        public GameRunner GameRunner;

        public const double SCALE_SELECTED = 1.2,
                            SCALE_SPEED_TO_SELECTED = 0.1,
                            SCALE_UNSELECTED = 1.0,
                            SCALE_SPEED_TO_UNSELECTED = 0.1;

        /// <summary>
        /// Init of basics: non-graphical, non-XNA, non-channel related 
        /// </summary>
        protected override void Initialize()
        {
            IsAudio = true;
            IsMouseVisible = true;
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

            // game channel
            GameChannel = TTFactory.CreateChannel(Color.White, false);
            ChannelMgr.AddChannel(GameChannel);
            GameChannel.ZapTo();
            //gameChannel.DisableSystem<SpriteCollisionSystem>();

            // create collection onto channel
            var iconsLayer = Factory.CreateIconsLayer();
            //
            TTFactory.BuildTo(iconsLayer.GetComponent<ScreenComp>());
            CollectionEntities = Factory.CreateCollection(Collection);
            TTFactory.BuildTo(GameChannel);

            // mouse entity            
            MousePointer = Factory.CreateMousePointer();
            // music
            Music = Factory.CreateMusic();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
                Exit();
            
            switch (GlobalState)
            {
                case GlobalStateEnum.STATE_BROWSING:
                    GameSelectionProcess();
                    GameLaunchingProcess();
                    break;

                case GlobalStateEnum.STATE_LAUNCHING:
                    GameRunProcess();
                    break;
            }
            
            base.Update(gameTime);
        }

        void GameSelectionProcess()
        {
            var coll = MousePointer.GetComponent<SpriteComp>().Colliders;
            SelectedGame = null;
            foreach (Entity e in CollectionEntities)
            {
                var sc = e.GetComponent<ScaleComp>();
                if (SelectedGame == null && coll.Contains(e))
                {
                    sc.ScaleTarget = SCALE_SELECTED;
                    sc.ScaleSpeed = SCALE_SPEED_TO_SELECTED;
                    SelectedGame = e;
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
            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            if (SelectedGame != null && ms.LeftButton == ButtonState.Pressed )
            {
                GlobalState = GlobalStateEnum.STATE_LAUNCHING;
            }
        }

        /// <summary>
        /// Once a game is selected for launch, this will install/run it
        /// </summary>
        void GameRunProcess()
        {
            GardenItem gi = SelectedGame.GetComponent<GardenItemComp>().Item;
            GameRunner.StartInstallRunTask(gi);
            GlobalState = GlobalStateEnum.STATE_BROWSING; // TODO make waiting
        }
    }

}

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

namespace Game1
{
    /// <summary>
    /// Main game class for IndieGig
    /// </summary>
    public class Game1 : TTGame
    {
        public Game1Factory Factory;
        public GardenConfig Config;
        public IndieGigCollection Collection;
        public Channel gameChannel;
        public Entity MousePointer;
        public List<Entity> CollectionEntities;
        public const double SCALE_SELECTED = 1.2,
                            SCALE_SPEED_TO_SELECTED = 0.01,
                            SCALE_UNSELECTED = 1.0,
                            SCALE_SPEED_TO_UNSELECTED = 0.01;

        /// <summary>
        /// Init of basics: non-graphical, non-XNA, non-channel related 
        /// </summary>
        protected override void Initialize()
        {
            IsAudio = false;
            IsMouseVisible = true;
            Factory = Game1Factory.Instance;
            Config = GardenConfig.Instance;
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
            gameChannel = TTFactory.CreateChannel(Color.White, false);
            ChannelMgr.AddChannel(gameChannel);
            gameChannel.ZapTo();
            //gameChannel.DisableSystem<SpriteCollisionSystem>();

            // create collection onto channel
            CollectionEntities = Factory.CreateCollection(Collection);
            // mouse entity
            MousePointer = Factory.CreateMousePointer();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
                Exit();
            GameSelectionProcess();
            base.Update(gameTime);
        }

        void GameSelectionProcess()
        {
            var coll = MousePointer.GetComponent<SpriteComp>().Colliders;
            foreach (Entity e in CollectionEntities)
            {
                var sc = e.GetComponent<ScaleComp>();
                if (coll.Contains(e))
                {
                    sc.ScaleTarget = SCALE_SELECTED;
                    sc.ScaleSpeed = SCALE_SPEED_TO_SELECTED;
                }
                else
                {
                    sc.ScaleTarget = SCALE_UNSELECTED;
                    sc.ScaleSpeed = SCALE_SPEED_TO_UNSELECTED;
                }
            }
        }

    }

}

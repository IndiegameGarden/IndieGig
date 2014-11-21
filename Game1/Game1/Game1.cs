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
            TTFactory.BuildTo(gameChannel);
            gameChannel.World.SystemManager.GetSystem<SpriteCollisionSystem>().IsEnabled = true;

            // create collection onto channel
            Factory.CreateCollection(Collection);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

    }

}

// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TTengine.Core;
using TTengine.Comps;
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
        Channel gameChannel;

        public Game1()
        {
            IsAudio = false;
        }

        protected override void Initialize()
        {
            Factory = Game1Factory.Instance;
            Config = GardenConfig.Instance;
            if (!Config.VerifyStorageFolders())
            {
                MsgBox.Show( Config.ClientName + " ERROR", "Could not create required temp folder:\n" + Config.TempFolder);
                Exit();
            }
            Collection = new IndieGigCollection();

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // game channel
            gameChannel = TTFactory.CreateChannel(Color.White, false);
            ChannelMgr.AddChannel(gameChannel);
            gameChannel.ZapTo();
            TTFactory.BuildTo(gameChannel);

            // create collection onto channel
            Factory.CreateCollection(Collection);

        }       

    }

}

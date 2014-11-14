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

namespace IndiegameGarden
{
    /// <summary>
    /// Main game class for IndieGig
    /// </summary>
    public class Game1 : TTGame
    {
        public Game1Factory Factory;
        public GardenConfig Config;
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

            // add framerate counter
            FrameRateCounter.Create(Color.Black);

            // add several sprites             
            for (float x = 0.1f; x < 1.6f; x += 0.3f)
            {
                for (float y = 0.1f; y < 1f; y += 0.24f)
                {
                    var pos = new Vector2(x * gameChannel.Screen.Width, y * gameChannel.Screen.Height);
                    Factory.CreateHyperActiveBall(pos);
                    Factory.CreateMovingTextlet(pos,"This is the\nTTengine test. !@#$1234");
                    //break;
                }
                //break;
            }
        }       

    }

}

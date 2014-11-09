using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TTengine.Core;
using TTengine.Comps;
using TTengine.Behaviors;
using TTengine.Modifiers;
using TTengine.Util;

using Artemis;
using Artemis.Interface;
using TreeSharp;


namespace TTengineTest
{
    /// <summary>
    /// Visual "unit" tests of various aspects of the TTengine. Press keys to cycle through tests.
    /// </summary>
    public class TestGame : TTGame
    {
        public TestFactory Factory;
        KeyboardState kbOld = Keyboard.GetState();
        int channel = 0;
        List<Channel> testChannels = new List<Channel>();

        public TestGame()
        {
            IsAudio = true;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // Here all the tests are created
            DoTest(new TestPostEffects());
            DoTest(new TestGamepad());
            DoTest(new TestMixedShaders());
            DoTest(new TestTextureSamplingShader());
            DoTest(new TestBasicShader());
            DoTest(new TestModifiers());
            DoTest(new TestSphereCollision());
            DoTest(new TestBTAI());
            DoTest(new TestSpritePixelGetSet());
            DoTest(new TestChannelSpritelets());
            DoTest(new TestZoomedChannel());
            DoTest(new TestAnimatedSprite());
            DoTest(new TestTargetMotion());
            DoTest(new TestScaling());
            DoTest(new TestAudioBasics());
            DoTest(new TestContentLoad());
            DoTest(new TestRelativeMotion());
            DoTest(new TestLinearMotion());
            DoTest(new TestRotation());

            // pick the initial one
            testChannels[channel].ZapTo();

        }

        protected override void Initialize()
        {
            Factory = TestFactory.Instance;
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape) && !kbOld.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            
            if ((kb.IsKeyDown(Keys.Space) && !kbOld.IsKeyDown(Keys.Space)) ||
                (kb.IsKeyDown(Keys.PageDown) && !kbOld.IsKeyDown(Keys.PageDown)) )
            {
                if (channel < testChannels.Count-1)
                {
                    channel++;
                    testChannels[channel].ZapTo();
                }
            }
            else if (kb.IsKeyDown(Keys.PageUp) && !kbOld.IsKeyDown(Keys.PageUp))
            {
                if (channel > 0)
                {
                    channel--;
                    testChannels[channel].ZapTo();
                }
            }
            kbOld = kb;
        }

        private void DoTest(Test test)
        {
            var ch = TTFactory.CreateChannel(test.BackgroundColor,false);
            TTGame.Instance.ChannelMgr.AddChannel(ch);
            testChannels.Add(ch);
            test.Create();

            // add framerate counter
            var col = TTUtil.InvertColor(test.BackgroundColor);
            FrameRateCounter.Create(col);

            // add test info as text
            TestFactory.Instance.CreateTextlet(new Vector2(2f, GraphicsMgr.PreferredBackBufferHeight-20f), test.GetType().Name, col);

        }

    }

}

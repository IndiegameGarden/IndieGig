// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Artemis;
using Artemis.Interface;
using TreeSharp;
using TTengine.Core;
using TTengine.Comps;
using TTengine.Behaviors;
using TTengine.Modifiers;
using TTengine.Util;
using IndiegameGarden.Base;
using Game1.Comps;

namespace Game1
{

    /// <summary>
    /// Factory to create new game-specific entities
    /// </summary>
    public class Game1Factory
    {
        private static Game1Factory _instance = null;
        private Game1 _game;

        private Game1Factory(Game1 game)
        {
            _game = game;
        }

        public static Game1Factory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Game1Factory(TTGame.Instance as Game1);
                return _instance as Game1Factory;
            }
        }

        protected Random rnd = new Random();

        public Entity CreateGardenIcon(double scale, GardenItem gi)
        {
            String fn = Path.Combine ( GardenConfig.Instance.ThumbnailsFolder, gi.ThumbnailFile);
            Entity e = TTFactory.CreateSpritelet( fn );
            e.AddComponent(new ScaleComp(scale));
            e.AddComponent(new TargetMotionComp());
            e.AddComponent(new GardenItemComp(gi));
            e.GetComponent<SpriteComp>().CenterToMiddle();
            return e;
        }

        /// <summary>
        /// Instantiate a whole collection of garden items as Entities in the world
        /// </summary>
        /// <param name="col">the collection to instantiate</param>
        public void CreateCollection(GardenItemCollection col)
        {
            foreach (GardenItem gi in col)
            {
                CreateGardenIcon(1.0,gi);
            }
        }

        /// <summary>
        /// create a ball Spritelet that can be scaled
        /// </summary>
        /// <param name="radius">the relative size scaling, 1 is normal</param>
        /// <returns></returns>
        public Entity CreateBall(double radius)
        {
            Entity e = TTFactory.CreateSpritelet("paul-hardman_circle-four");
            e.AddComponent(new ScaleComp(radius));
            return e;
        }

        /// <summary>
        /// create an active ball with given position and random velocity and some weird (AI) behaviors
        /// </summary>
        /// <returns></returns>
        public Entity CreateHyperActiveBall(Vector2 pos)
        {
            var ball = CreateBall(0.08f + 0.07f * (float)rnd.NextDouble());

            // position and velocity set
            ball.GetComponent<PositionComp>().Position2D = pos;
            ball.GetComponent<VelocityComp>().Velocity = 0.2f * new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, 0f);

            /*
            // duration of entity
            ball.AddComponent(new ExpiresComp(4 + 500 * rnd.NextDouble()));

            // Behavior Tree AI
            BTAIComp ai = new BTAIComp();
            var randomWanderBehavior = new RandomWanderBehavior(1, 6);
            ai.rootNode = new PrioritySelector(randomWanderBehavior);
            ball.AddComponent(ai);

            // Modifier to adapt scale
            TTFactory.AddModifier(ball, ScaleModifierScript);

            // another adapting scale with sine rhythm
            var s = new SineFunction();
            s.Frequency = 0.5;
            s.Amplitude = 0.25;
            s.Offset = 1;
            TTFactory.AddModifier(ball, ScaleModifierScript, s);

            // modifier to adapt rotation
            TTFactory.AddModifier(ball, RotateModifierScript);

            // set different time offset initially, per ball (for the modifiers)
            ball.GetComponent<ScriptComp>().SimTime = 10 * rnd.NextDouble();
            */

            ball.Refresh();
            return ball;

        }

        public Entity CreateMovingTextlet(Vector2 pos, string text)
        {
            var t = TTFactory.CreateTextlet(text);
            t.GetComponent<PositionComp>().Position2D = pos;
            t.GetComponent<DrawComp>().DrawColor = Color.Black;
            t.GetComponent<VelocityComp>().Velocity = 0.2f * new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, 0f);
            t.GetComponent<ScaleComp>().Scale = 0.5;
            return t;
        }

        public void ScaleModifierScript(ScriptContext ctx, double value)
        {
            ctx.Entity.GetComponent<ScaleComp>().ScaleModifier *= 0.5 + ctx.Entity.GetComponent<PositionComp>().Position.X;
        }

        public void ScaleModifierScript2(ScriptContext ctx, double value)
        {
            ctx.Entity.GetComponent<ScaleComp>().ScaleModifier *= value;
        }

        public void RotateModifierScript(ScriptContext ctx, double value)
        {
            ctx.Entity.GetComponent<DrawComp>().DrawRotation = (float)value;
        }
    }

}
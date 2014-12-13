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
using TTMusicEngine.Soundevents;
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
            e.GetComponent<ScaleComp>().Scale = 0.1;
            e.AddComponent(new TargetMotionComp());
            e.AddComponent(new GardenItemComp(gi));
            e.GetComponent<SpriteComp>().CenterToMiddle();
            return e;
        }

        /// <summary>
        /// Create the FxScreenlet (render layer) that holds the game icons
        /// which provides the halo effect for icons, using a pixel shader.
        /// </summary>
        /// <returns></returns>
        public Entity CreateIconsLayer()
        {
            var iconsLayer = TTFactory.CreateFxScreenlet("GameIcon");
            Effect fx = iconsLayer.GetComponent<ScreenComp>().SpriteBatch.effect;
            TTFactory.AddScript(iconsLayer, GameIconFxParametersScript);
            return iconsLayer;
        }

        /// <summary>
        /// Script to update the parameters of the GameIcon.fx each update
        /// </summary>
        /// <param name="ctx"></param>
        void GameIconFxParametersScript(ScriptContext ctx)
        {
            Effect fx = ctx.Entity.GetComponent<ScreenComp>().SpriteBatch.effect;
            //fx.Parameters["Time"].SetValue((float)ctx.SimTime);
        }

        /// <summary>
        /// Instantiate a whole collection of garden items as Entities in the world
        /// </summary>
        /// <param name="col">the collection to instantiate</param>
        public List<Entity> CreateCollection(GardenItemCollection col)
        {
            List<Entity> l = new List<Entity>();
            foreach (GardenItem gi in col)
            {
                l.Add(CreateGardenIcon(1.0,gi));
            }
            return l;
        }

        /// <summary>
        /// An entity that tracks the mouse pointer position, to detect collissions
        /// </summary>
        public Entity CreateMousePointer()
        {
            Entity e = TTFactory.CreateSpritelet("pixie");
            var sc = e.GetComponent<SpriteComp>();
            sc.IsCheckingCollisions = true;
            TTFactory.AddScript(e, MousePointerTrackingScript);
            return e;
        }

        void MousePointerTrackingScript(ScriptContext ctx)
        {
            MouseState ms = Mouse.GetState();
            var pc = ctx.Entity.GetComponent<PositionComp>();
            pc.X = ms.X;
            pc.Y = ms.Y;
        }
    
        void ScaleModifierScript(ScriptContext ctx, double value)
        {
            ctx.Entity.GetComponent<ScaleComp>().ScaleModifier *= 0.5 + ctx.Entity.GetComponent<PositionComp>().Position.X;
        }

        void RotateModifierScript(ScriptContext ctx, double value)
        {
            ctx.Entity.GetComponent<DrawComp>().DrawRotation = (float)value;
        }

        public Entity CreateMusic()
        {
            var soundScript = new SoundEvent("Music");
            var ev = new SampleSoundEvent("star.ogg");
            ev.Amplitude = 0.5;
            soundScript.AddEvent(0.5, ev);
            return TTFactory.CreateAudiolet(soundScript);
        }

    }

}
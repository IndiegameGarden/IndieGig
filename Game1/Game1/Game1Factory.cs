// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

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
        /// <summary>
        /// UI constants
        /// </summary>
        public const double MUSIC_AMPLITUDE = 0.618,
                            BACKGROUND_STAR_SCALE = 2.0;


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
                    _instance = new Game1Factory(Game1.InstanceGame1);
                return _instance ;
            }
        }

        protected Random rnd = new Random();

        public Entity CreateGardenIcon(double scale, GardenItem gi)
        {
            String fn = Path.Combine ( GardenConfig.Instance.ThumbnailsFolder, gi.ThumbnailFile);
            Entity e = TTFactory.CreateSpritelet( fn );
            e.AddComponent(new ScaleComp(scale));
            e.GetComponent<ScaleComp>().Scale = 0.4;
            e.AddComponent(new TargetMotionComp());
            e.AddComponent(new GardenItemComp(gi));
            e.GetComponent<SpriteComp>().CenterToMiddle();
            e.GetComponent<PositionComp>().Position = Game1.InstanceGame1.MainChannel.Screen.Center;
            //e.AddComponent(new TextComp("Test"));
            //TTFactory.AddModifier(e, SetLayerDepthAsText); // DEBUG
            return e;
        }

        /// <summary>
        /// Debug Script to write the layerdepth of parent sprite, as a text value of the textlet
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="val"></param>
        void SetLayerDepthAsText(ScriptContext ctx, double val)
        {
            ctx.Entity.GetComponent<TextComp>().Text = ctx.Entity.GetComponent<PositionComp>().Depth.ToString();
        }

        /// <summary>
        /// Create the FxScreenlet (render layer) that holds the game icons
        /// which provides the halo effect for icons, using a pixel shader.
        /// </summary>
        /// <returns></returns>
        public Entity CreateIconsLayer()
        {
            var iconsLayer = TTFactory.CreateScreenlet(true); // TTFactory.CreateFxScreenlet("GameIcon");
            //Effect fx = iconsLayer.GetComponent<ScreenComp>().SpriteBatch.effect;
            //TTFactory.AddScript(iconsLayer, GameIconFxParametersScript);
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
        /// An entity that tracks the mouse pointer position, to detect collissions with
        /// selectable icons
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
            if (Game1.Instance.IsActive)
            {
                MouseState ms = Mouse.GetState();
                var pc = ctx.Entity.GetComponent<PositionComp>();
                pc.X = ms.X;
                pc.Y = ms.Y;
            }
        }
    
        public Entity CreateMusic()
        {
            var soundScript = new SoundEvent("Music");
            var ev = new SampleSoundEvent("star.ogg");
            ev.Amplitude = MUSIC_AMPLITUDE;
            soundScript.AddEvent(0.5, ev);
            var e = TTFactory.CreateAudiolet(soundScript);
            e.AddComponent(new AudioFadingComp());
            return e;
        }

        public Entity CreateBackgroundRotatingStar()
        {
            var fxLayer = TTFactory.CreateFxScreenlet("Background");
            var sprite = TTFactory.CreateSpritelet("supernova.png");
            sprite.GetComponent<DrawComp>().DrawScreen = fxLayer.GetComponent<ScreenComp>();
            sprite.GetComponent<PositionComp>().Position = TTFactory.BuildScreen.Center;
            sprite.GetComponent<SpriteComp>().CenterToMiddle();
            var rc = new RotateComp();
            rc.RotateSpeed = 0;
            sprite.AddComponent(rc);
            var sc = new ScaleComp();
            sc.ScaleTarget = BACKGROUND_STAR_SCALE;
            sc.ScaleSpeed = 0.1;
            sprite.AddComponent(sc);
            return sprite;
        }

        public Entity CreateBackgroundGameIcon(string spriteName = "supernova128")
        {
            var fxLayer = TTFactory.CreateFxScreenlet("Background");
            var sineFunc = new SineFunction();
            sineFunc.Offset = 0.1;
            sineFunc.Frequency = 0.09283;
            sineFunc.Amplitude = 0.0278323;
            TTFactory.AddModifier(fxLayer, WobbleFlowParameterScript, sineFunc);
            
            var sprite = TTFactory.CreateSpritelet(spriteName);
            sprite.GetComponent<DrawComp>().DrawScreen = fxLayer.GetComponent<ScreenComp>();
            sprite.GetComponent<DrawComp>().LayerDepth = 0.99f;
            sprite.GetComponent<PositionComp>().Position = TTFactory.BuildScreen.Center;
            sprite.GetComponent<SpriteComp>().CenterToMiddle();
            //var rc = new RotateComp();
            //rc.RotateSpeed = 0;
            //sprite.AddComponent(rc);
            var sc = new ScaleComp();
            sc.ScaleTarget = Game1.InstanceGame1.MainChannel.Screen.Width / Game1.ICON_SIZE;
            sc.ScaleSpeed = 10;
            sprite.AddComponent(sc);
            return sprite;
        }

        void WobbleFlowParameterScript(ScriptContext ctx, double val)
        {
            var sc = ctx.Entity.GetComponent<ScreenComp>();
            EffectParameter p = sc.SpriteBatch.effect.Parameters["FlowParameter"];
            p.SetValue((float)(val /*+ 0.1 * sc.SimTime */) );
        }
     }

}
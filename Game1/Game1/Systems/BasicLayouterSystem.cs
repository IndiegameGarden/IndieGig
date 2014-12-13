// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Artemis;
using Artemis.Attributes;
using Artemis.Manager;
using Artemis.System;
using TTengine.Comps;
using Game1.Comps;

namespace Game1.Systems
{
    using Artemis;
    using Artemis.System;

    [ArtemisEntitySystem(GameLoopType = GameLoopType.Update, Layer = 5)]
    class BasicLayouterSystem : EntityComponentProcessingSystem<GardenItemComp,TargetMotionComp>
    {
        public static float Dx = 128f;
        public static float Dy = 128f;

        protected int xPos, yPos, xPosMax = 6;
        protected float haloTime = 0f;

        protected override void Begin()
        {
            xPos = 1;
            yPos = 1;
            double dt = TimeSpan.FromTicks(EntityWorld.Delta).TotalSeconds;
            haloTime += (float)dt; // move the 'halo' of the icon onwards as long as it's visible.
        }

        public override void Process(Entity entity, GardenItemComp gc, TargetMotionComp tmc)
        {
            // set entity to pos
            tmc.Target = calcVector3Pos(xPos,yPos) ;
            tmc.TargetVelocity = 3000.0;

            // next position
            xPos++;
            if (xPos > xPosMax)
            {
                xPos = 0;
                yPos++;
            }

            /*
            // draw color for fx shader
            // this is a conversion from 'halotime' to the time format that can be given to the pixel shader
            // via the 'draw color' parameter
            Color col = entity.GetComponent<DrawComp>().DrawColor;
            int t = (int)(haloTime * 256);
            int c3 = t % 256;
            int c2 = ((t - c3) / 256) % 256;
            int c1 = ((t - c2 - c3) / 65536) % 256;
            entity.GetComponent<DrawComp>().DrawColor = new Color(c1, c2, c3, col.A);
            */
        }

        private Vector3 calcVector3Pos(int x, int y)
        {
            return new Vector3(x * Dx, y * Dy, 0f);
        }
    }
}

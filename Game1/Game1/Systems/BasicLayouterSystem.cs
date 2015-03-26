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

        protected int xPos, yPos, xPosMax = Game1.ICONCOUNT_HORIZONTAL;
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
            tmc.Target = calcVectorPos(xPos,yPos) ;
            tmc.TargetVelocity = 1500.0;

            // next position
            xPos++;
            if (xPos > xPosMax)
            {
                xPos = 1;
                yPos++;
            }
        }

        private Vector2 calcVectorPos(int x, int y)
        {
            return new Vector2(x * Dx, y * Dy);
        }
    }
}

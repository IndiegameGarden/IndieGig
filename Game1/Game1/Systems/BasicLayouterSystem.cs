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
        public static float Dx = 32f;
        public static float Dy = 32f;

        protected int xPos, yPos, xPosMax = 6;

        protected override void Begin()
        {
            xPos = 0;
            yPos = 0;
        }

        public override void Process(Entity entity, GardenItemComp gc, TargetMotionComp tmc)
        {
            // next position
            xPos++;
            if (xPos > xPosMax)
            {
                xPos = 0;
                yPos++;
            }

            // set entity to pos
            tmc.Target = calcVector3Pos(xPos,yPos) ;
            tmc.TargetVelocity = 1.0;
        }

        private Vector3 calcVector3Pos(int x, int y)
        {
            return new Vector3(x * Dx, y * Dy, 0f);
        }
    }
}

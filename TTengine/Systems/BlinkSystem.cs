﻿using System;
using System.Collections.Generic;

using System.Text;

using TTengine.Comps;
using Artemis.System;
using Artemis.Attributes;
using Artemis.Manager;
using Artemis;

namespace TTengine.Systems
{
    // TODO consider a soft (faded) blink as well.
    [ArtemisEntitySystem(GameLoopType = GameLoopType.Update, Layer = SystemsSchedule.BlinkSystem)]
    public class BlinkSystem : EntityComponentProcessingSystem<BlinkComp>
    {
        double dt = 0;

        protected override void Begin()
        {
            dt = TimeSpan.FromTicks(EntityWorld.Delta).TotalSeconds;
        }

        // TODO: check if non-active entities are also called in this process method. For all systems.
        public override void Process(Entity entity, BlinkComp bc)
        {
            bc.Dt = dt;
            bc.SimTime += dt;
            double t = bc.SimTime % bc.TimePeriod;
            bool isVisible;
            if (t <= bc.TimeOn)
                isVisible = true;
            else
                isVisible = false;
            entity.GetComponent<DrawComp>().IsVisible = isVisible;
        }

    }
}

﻿using System;
using System.Collections.Generic;

using System.Text;
using TTengine.Core;
using TTengine.Comps;
using TTMusicEngine;

using Artemis;
using Artemis.Manager;
using Artemis.Attributes;
using Artemis.System;

namespace TTengine.Systems
{
    [ArtemisEntitySystem(GameLoopType = GameLoopType.Draw, Layer = SystemsSchedule.AudioSystem)]
    public class AudioFadingSystem : EntityComponentProcessingSystem<AudioFadingComp,AudioComp>
    {
        double dt = 0;

        protected override void Begin()
        {
            dt = TimeSpan.FromTicks(EntityWorld.Delta).TotalSeconds;
        }

        public override void Process(Entity entity, AudioFadingComp afc, AudioComp ac)
        {
            if (afc.IsFading)
            {
                if (ac.Ampl < afc.FadeTarget)
                {
                    ac.Ampl += afc.FadeSpeed * dt;
                    if (ac.Ampl > afc.FadeTarget)
                    {
                        ac.Ampl = afc.FadeTarget;
                        afc.IsFading = false;
                    }
                }
                else if (ac.Ampl > afc.FadeTarget)
                {
                    ac.Ampl -= afc.FadeSpeed * dt;
                    if (ac.Ampl < afc.FadeTarget)
                    {
                        ac.Ampl = afc.FadeTarget;
                        afc.IsFading = false;
                    }
                }
            }
        }

    }

}

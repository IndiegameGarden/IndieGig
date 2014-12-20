using System;
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
                double da = 0;
                if (ac.Ampl < afc.FadeTarget)
                    da = afc.FadeSpeed * dt;
                else if (ac.Ampl > afc.FadeTarget)
                    da = -afc.FadeSpeed * dt;
                ac.Ampl += da;
            }
        }

    }

}

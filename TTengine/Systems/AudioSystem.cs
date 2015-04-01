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
    public class AudioSystem : EntityComponentProcessingSystem<AudioComp>
    {
        double dt = 0;
        RenderParams rp = new RenderParams();
        MusicEngine audioEngine = null;

        protected override void Begin()
        {
            audioEngine = TTGame.Instance.AudioEngine;
            dt = TimeSpan.FromTicks(EntityWorld.Delta).TotalSeconds;
            audioEngine.Update(); // to be called once every frame
        }

        public override void Process(Entity entity, AudioComp ac)
        {            
            if (!ac.IsPaused)
            {
                ac.SimTime += dt;
            }
            rp.Time = ac.SimTime;
            rp.Ampl = ac.Ampl;
            audioEngine.Render(ac.AudioScript, rp);
        }

    }

}

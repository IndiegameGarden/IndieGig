using System;
using System.Collections.Generic;

using System.Text;
using TTengine.Core;
using TTengine.Comps;

using Artemis;
using Artemis.Manager;
using Artemis.Attributes;
using Artemis.System;

namespace TTengine.Systems
{
    [ArtemisEntitySystem(GameLoopType = GameLoopType.Update, Layer = SystemsSchedule.ScaleSystem)]
    public class ScaleSystem : EntityComponentProcessingSystem<ScaleComp>
    {

        public override void Process(Entity entity, ScaleComp sc)
        {
            sc._isScaleAbsCalculated = false;

            // scaling logic towards target
            if (sc.ScaleSpeed > 0)
            {
                if (sc.Scale < sc.ScaleTarget)
                {
                    sc.Scale += sc.ScaleSpeed * (sc.ScaleTarget - sc.Scale); 
                    if (sc.Scale > sc.ScaleTarget)
                    {
                        sc.Scale = sc.ScaleTarget;
                    }
                }
                else if (sc.Scale > sc.ScaleTarget)
                {
                    sc.Scale += sc.ScaleSpeed * (sc.ScaleTarget - sc.Scale); 
                    if (sc.Scale < sc.ScaleTarget)
                    {
                        sc.Scale = sc.ScaleTarget;
                    }
                }
            }

            // set scale for drawing
            if (entity.HasComponent<DrawComp>())
            {
                // FIXME calculation depends on parents which may have not yet been simulated this round
                entity.GetComponent<DrawComp>().DrawScale = (float) (sc.ScaleAbs * sc.ScaleModifier);
            }

            sc.ScaleModifier = 1; // the ModifierSystem may adapt this one later. Each round reset to 1.

        }

    }
}

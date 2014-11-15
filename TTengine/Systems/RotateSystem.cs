// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

namespace TTengine.Systems
{
    #region Using statements

    using System;
    using Artemis;
    using Artemis.Attributes;
    using Artemis.Manager;
    using Artemis.System;
    using Microsoft.Xna.Framework;
    using TTengine.Comps;

    #endregion

    /// <summary>The movement system.</summary>
    [ArtemisEntitySystem(GameLoopType = GameLoopType.Update, Layer = SystemsSchedule.RotateSystem)]
    public class RotateSystem : EntityComponentProcessingSystem<RotateComp, DrawComp>
    {
        double dt = 0;

        protected override void Begin()
        {
            dt = TimeSpan.FromTicks(EntityWorld.Delta).TotalSeconds;
        }

        /// <summary>Processes the specified entity.</summary>
        /// <param name="entity">The entity.</param>
        public override void Process(Entity entity, RotateComp rotComp, DrawComp drawComp)
        {
            rotComp.Rotate += rotComp.RotateSpeed * dt;
            drawComp.DrawRotation = (float)rotComp.Rotate;
        }
    }

}
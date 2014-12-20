using System;
using System.Collections.Generic;
using System.Text;

using TTengine.Core;
using TTMusicEngine.Soundevents;

namespace TTengine.Comps
{
    /// <summary>
    /// Enables audio fading in and out
    /// </summary>
    public class AudioFadingComp: Comp
    {
        public double FadeTarget = 1.0;
        public double FadeSpeed = 0.1;
        public bool IsFading = false;

        public AudioFadingComp()
        {
        }

    }
}

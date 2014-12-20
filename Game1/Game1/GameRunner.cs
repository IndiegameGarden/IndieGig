using System;
using System.Collections.Generic;
using TTengine.Comps;
using IndiegameGarden.Base;
using IndiegameGarden.Install;

namespace Game1
{
    /// <summary>
    /// Responsible class for entire game-running process at top level
    /// </summary>
    public class GameRunner
    {
        public ITask GameRunTask;

        public bool StartInstallRunTask(GardenItem g)
        {
            // check if there's already an active task 
            if (GameRunTask != null)
            {
                if (!GameRunTask.IsFinished())
                    return false; // stop, only one at a time!
            }

            // fade out music
            var afc = (Game1.Instance as Game1).Music.GetComponent<AudioFadingComp>();
            afc.FadeTarget = 0;
            afc.FadeSpeed = 0.2;
            afc.IsFading = true;

            var installTask = new GameInstallRunTask(g);
            GameRunTask = new ThreadedTask(installTask);
            GameRunTask.Start();
            return true;
        }

    }
}

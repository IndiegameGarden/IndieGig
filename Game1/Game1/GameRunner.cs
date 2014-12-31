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

        public bool TryRunGame(GardenItem g)
        {
            // check if there's already an active task 
            if (GameRunTask != null)
            {
                if (!GameRunTask.IsFinished())
                    return false; // stop, only one at a time!
            }

            ITask t = new GameRunnerTask(g);
            GameRunTask = new ThreadedTask(t);
            GameRunTask.Start();
            return true;
        }
    }

    public class GameRunnerTask : Task
    {
        GardenItem gi;
        ITask installTask;

        public GameRunnerTask(GardenItem g)
        {
            this.gi = g;
        }

        protected override void StartInternal()
        {
            // fade out music
            var afc = Game1.InstanceGame1.Music.GetComponent<AudioFadingComp>();
            afc.FadeTarget = 0;
            afc.FadeSpeed = 0.4;
            afc.IsFading = true;

            // install & run game (via task)
            installTask = new GameInstallRunTask(gi);
            installTask.Start();

            // fade in music again
            afc.FadeTarget = 1;
            afc.FadeSpeed = 0.2;
            afc.IsFading = true;

            // status
            this.CopyStatusFrom(installTask);
            Game1.InstanceGame1.GlobalState = Game1.GlobalStateEnum.STATE_BROWSING; 
        }

        protected override void AbortInternal()
        {
            if (installTask != null)
                installTask.Abort();
            Game1.InstanceGame1.GlobalState = Game1.GlobalStateEnum.STATE_BROWSING; 
        }
    }

}

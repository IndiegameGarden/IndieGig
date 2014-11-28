using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var installTask = new GameInstallRunTask(g);
            GameRunTask = new ThreadedTask(installTask);
            GameRunTask.Start();
            return true;
        }

    }
}

﻿using System;
using System.Threading;
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

        public bool TryRunGame(GardenItem g, bool isConfigure)
        {
            // check if there's already an active task 
            if (GameRunTask != null)
            {
                if (!GameRunTask.IsFinished())
                    return false; // stop, only one at a time!
            }

            ITask t = new GameRunnerTask(g, isConfigure);
            GameRunTask = new ThreadedTask(t);
            GameRunTask.Start();
            return true;
        }
    }

    public class GameRunnerTask : Task
    {
        GardenItem gi;
        bool isConfigure;
        GameInstallRunTask installTask;

        public GameRunnerTask(GardenItem g, bool isConfigure)
        {
            this.gi = g;
            this.isConfigure = isConfigure;
        }

        protected override void StartInternal()
        {
            // install game (via task)
            installTask = new GameInstallRunTask(gi,isConfigure);
            installTask.DoRun = false;
            installTask.Start();

            var afc = Game1.InstanceGame1.Music.GetComponent<AudioFadingComp>();
            var ac = Game1.InstanceGame1.Music.GetComponent<AudioComp>();

            if (!isConfigure)
            {
                // fade out music
                afc.FadeTarget = 0;
                afc.FadeSpeed = 0.4;
                afc.IsFading = true;

                // delayed state change
                int dlyMs = (int)(1000 * (ac.Ampl-afc.FadeTarget)/afc.FadeSpeed) + 150;
                var stateChangeTask = new ThreadedTask(new DelayedStateChanger(dlyMs, Game1.GlobalStateEnum.STATE_PLAYING_PHASE1));
                stateChangeTask.Start();
            }

            // run game
            installTask = new GameInstallRunTask(gi, isConfigure);
            installTask.DoRun = true;
            installTask.Start();

            // fade in music again
            afc.FadeTarget = 1;
            afc.FadeSpeed = 0.2;
            afc.IsFading = true;
            
            // check if music is done playing - if so, reset to begin
            if (ac.SimTime > ac.AudioScript.Duration)
            {
                ac.SimTime = 0;
            }

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

    public class DelayedStateChanger: Task
    {
        Game1.GlobalStateEnum state;
        int delayMs;

        public DelayedStateChanger(int delayMs, Game1.GlobalStateEnum state)
        {
            this.delayMs = delayMs;
            this.state = state;
        }

        protected override void StartInternal()
        {
            Thread.Sleep(delayMs);
            Game1.InstanceGame1.GlobalState = state;
        }

        protected override void AbortInternal()
        {
            throw new NotImplementedException();
        }
    }
}

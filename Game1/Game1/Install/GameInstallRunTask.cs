// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTengine.Comps;
using Game1;
using IndiegameGarden.Download;
using IndiegameGarden.Base;
using MyDownloader.Core;

namespace IndiegameGarden.Install
{

    /// <summary>
    /// a Task to install, then run, a game. 
    /// If game is already installed locally, install is skipped.
    /// </summary>
    public class GameInstallRunTask: Task
    {
        public bool DoRun = true;

        InstallTask installTask;
        GameLauncherTask runTask;
        GardenItem game;

        /// <summary>
        /// create new Install and Run task
        /// </summary>
        /// <param name="game">info of game to install and run</param>
        public GameInstallRunTask(GardenItem game)
        {
            this.game = game;
            status = ITaskStatus.CREATED;
        }

        protected override void StartInternal()
        {
            // do the checking if already installed
            game.Refresh();
            if (!game.IsInstalled)
            {
                // check if folder already there
                // if download ready and OK, start install
                installTask = new InstallTask(game);
                installTask.Start();
                this.CopyStatusFrom(installTask);

                // install failed? remove the game dir
                // FIXME move to a cleanup task
                if (status == ITaskStatus.FAIL)
                {
                    if (!game.IsBundleItem)
                    {
                        string fn = game.ExeFolder; // the game dir
                        if (fn != null && fn.Length > 0)
                        {
                            try
                            {
                                Directory.Delete(fn, true);
                            }
                            catch (Exception)
                            {
                                ; // TODO?
                            }
                        }
                    }
                }
                game.Refresh();
            }

            // ready to run
            if (DoRun && status != ITaskStatus.FAIL)
            {
                runTask = new GameLauncherTask(game);
                runTask.Start();
                this.CopyStatusFrom(runTask);
            }
        }

        protected override void AbortInternal()
        {
            if (IsInstalling())
            {
                installTask.Abort();
                statusMsg = "Install aborted";
            }
            else
            {
                if(installTask != null)
                    installTask.Abort();
                runTask.Abort();
            }
        }

        /// <summary>
        /// check whether currently installing
        /// </summary>
        /// <returns>true if installing, false otherwise</returns>
        public bool IsInstalling()
        {
            return (installTask != null && installTask.IsRunning() );
        }

        public override double Progress()
        {            
            return ProgressInstall();
        }

        /// <summary>
        /// check progress in installing task
        /// </summary>
        /// <returns>progress value 0...1</returns>
        public double ProgressInstall()
        {
            if (installTask == null)
                return 0.0;
            return installTask.Progress();
        }

    }
}

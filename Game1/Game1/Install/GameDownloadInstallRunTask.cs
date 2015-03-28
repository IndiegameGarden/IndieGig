// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IndiegameGarden.Download;
using IndiegameGarden.Base;
using MyDownloader.Core;

namespace IndiegameGarden.Install
{

    /// <summary>
    /// a Task to download, then install, then run, a game. If game file already exists locally, download is skipped.
    /// If game is already installed locally, install is skipped.
    /// </summary>
    public class GameDownloadInstallRunTask: Task
    {
        InstallTask installTask;
        GameDownloader downloadTask;
        GameLauncherTask runTask;
        GardenItem game;

        /// <summary>
        /// create new Download and Install task
        /// </summary>
        /// <param name="game">info of game to download and install</param>
        public GameDownloadInstallRunTask(GardenItem game)
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
                // do the download
                downloadTask = new GameDownloader(game);
                downloadTask.Start();

                if (downloadTask.IsSuccess())
                {
                    Thread.Sleep(100);

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
                }
                else
                {
                    // error in downloading process - no install
                    status = ITaskStatus.FAIL;
                    statusMsg = downloadTask.StatusMsg();
                }
                game.Refresh();

            }

            // ready to run
            if (status != ITaskStatus.FAIL)
            {
                runTask = new GameLauncherTask(game);
                runTask.Start();
                this.CopyStatusFrom(runTask);
            }
        }

        protected override void AbortInternal()
        {
            if (IsDownloading())
            {
                downloadTask.Abort();
                statusMsg = downloadTask.StatusMsg();
            }
            else if (IsInstalling())
            {
                installTask.Abort();
                statusMsg = installTask.StatusMsg();
            }
            else
            {
                if(downloadTask != null)
                    downloadTask.Abort();
                if(installTask != null)
                    installTask.Abort();
            }
        }

        /// <summary>
        /// check whether currently downloading
        /// </summary>
        /// <returns>true if downloading, false otherwise</returns>
        public bool IsDownloading()
        {
            return (downloadTask != null && downloadTask.IsRunning() );
        }

        public virtual double DownloadSpeed()
        {
            if (downloadTask != null)
                return downloadTask.DownloadSpeed();
            return 0;
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
            if (IsDownloading())
                return 0.0; // ProgressDownload() * FRACTION_OF_PROGRESS_FOR_DOWNLOAD;
            if (downloadTask != null && downloadTask.IsFinished())
            {
                if (IsInstalling())
                    return ProgressInstall();
                return 1.0;
            }
            return 0.0;            
        }

        /// <summary>
        /// check progressContributionSingleFile in downloading task
        /// </summary>
        /// <returns>progress value 0...1</returns>
        public double ProgressDownload()
        {
            if (downloadTask == null)
                return 0;
            return downloadTask.Progress();
        }

        /// <summary>
        /// check progress in installing task
        /// </summary>
        /// <returns>progress value 0...1</returns>
        public double ProgressInstall()
        {
            if (installTask == null)
                return 0;
            return installTask.Progress();
        }

    }
}

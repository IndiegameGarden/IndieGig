﻿// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using TTengine.Core;
using TTengine.Util;
using IndiegameGarden.Install;

namespace IndiegameGarden.Base
{
    /**
     * <summary>represents all data and status of a game that a user can select, download and start/play</summary>
     */
    public class GardenItem: IDisposable
    {
        /// <summary>
        /// internally used string ID for game, no whitespace allowed, only alphanumeric and 
        /// _ - special characters allowed.
        /// </summary>
        public string GameID = "";

        /// <summary>
        /// default visibility of item for the user; 1 (yes) or 0 (no)
        /// </summary>
        public byte VisibilityLabel = 1;

        /// <summary>
        /// Name of game
        /// </summary>
        public string Name = "";

        /// <summary>
        /// short game description to show on screen
        /// </summary>
        public string description = "";

        /// <summary>
        /// some hints for the player e.g. what the control keys are.
        /// </summary>
        public string HelpText = "";

        protected string packedFileURL = "";

        protected List<string> packedFileMirrors = new List<string>();

        /// <summary>
        /// URL (optionally without the http:// or www. in front) to game developer's website
        /// </summary>
        public string DeveloperWebsiteURL = "";

        /// <summary>
        /// name of .exe file or .bat to launch to start game
        /// </summary>
        public string ExeFile = "";

        /// <summary>
        /// directory gameDirPath that OS has to 'change directory' to, before launching the game
        /// </summary>
        public string CdPath = "";

        /// <summary>
        /// Latest version of the game packed file which is available
        /// </summary>
        public int Version = 1;

        /// <summary>
        /// scaling factor of game icon when displayed
        /// </summary>
        public float ScaleIcon = 1f;

        /// <summary>
        /// where in 2D coordinates this game is positioned. Zero means non-specified.
        /// </summary>
        public float PositionX = 0;
        public float PositionY = 0;

        /// <summary>
        /// in case a 2D Position is not given, this specifies a wished position delta of game w.r.t. previous game in the library.
        /// </summary>
        public float PositionDeltaX = 0;
        public float PositionDeltaY = 0;

        protected string thumbnailURL = "";

        public bool HasOwnFolder = false;

        // <summary>Displayable status string of the current item, e.g. if downloading or failed to download.</summary>
        public string Status = null;

        /// <summary>
        /// Vector2 version (copy) of PositionX/PositionY
        /// </summary>
        public Vector2 PositionXY
        {
            get
            {
                return new Vector2(PositionX, PositionY);
            }
        }

        public int GridPositionX
        {
            get
            {
                return (int) Math.Round((double)PositionX);
            }
        }

        public int GridPositionY
        {
            get
            {
                return (int)Math.Round((double)PositionY);
            }
        }

        /// <summary>
        /// a set of mirrors for PackedFileURL
        /// </summary>
        public string[] PackedFileMirrors
        {
            get
            {
                return packedFileMirrors.ToArray();
            }
        }

        /// <summary>
        /// where can the packed file (.zip, .rar etc.) be downloaded from.
        /// Optionally without http:// in front. Optionally may set it without
        /// any server URL, then it is prepended with default file server URL
        /// automagically.
        /// </summary>
        public string PackedFileURL
        {
            get
            {
                if (IsBundleItem)
                    return GardenConfig.Instance.BundleFilesServerURL + ExeFile;
                if (!packedFileURL.Contains("/"))
                {
                    if (packedFileURL.Length == 0)
                        return GardenConfig.Instance.PackedFilesServerURL + GameIDwithVersion + "." + PackedFileExtension;
                    else
                        return GardenConfig.Instance.PackedFilesServerURL + packedFileURL;
                }
                else
                    return packedFileURL;
            }
            set
            {
                packedFileURL = value;
            }
        }

        /// <summary>
        /// the name of the packed file (eg .zip or .rar or .exe) once it is downloaded. May differ
        /// from the name of the archive as stored on the web which is included in PackedFileURL.
        /// </summary>
        public string PackedFileName{
            get
            {
                // check for .exe which are stored locally in DataPath
                if (IsBundleItem)
                    return ExeFile;
                else
                    return GameIDwithVersion + "." + PackedFileExtension;
            }
        }

        /// <summary>
        /// get path to a game's packed file (.zip, .rar)
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public string PackedFilePath
        {
            get
            {
                return Path.Combine(PackedFileFolder, PackedFileName );
            }
        }

        public string PackedFileFolder
        {
            get
            {
                if (IsBundleItem)
                    return GardenConfig.Instance.BundleDataPath;
                else
                    return GardenConfig.Instance.PackedFilesFolder;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                lineCount = TTUtil.LineCount(description);
            }
        }


        /// <summary>
        /// returns a human-readable string name for this item
        /// </summary>
        public string ItemName
        {
            get
            {
                if (IsMusic) return "music";
                if (IsSystemPackage) return "item";
                return "game";
            }
        }

        protected int lineCount = -1;

        public int DescriptionLineCount
        {
            get
            {
                if (lineCount < 0)
                    lineCount = TTUtil.LineCount(description);
                return lineCount;
            }
        }

        public string GameIDwithVersion
        {
            get
            {
                if (Version == 1)
                    return GameID;
                else
                    return GameID + "_v" + Version;
            }
        }

        /// <summary>
        /// check whether a 2D coordinate position for game is explicitly given, or not
        /// </summary>
        public bool IsPositionGiven
        {
            get
            {
                return (PositionX != 0f) || (PositionY != 0f);
            }
        }

        /// <summary>
        /// check whether a 2D coordinate position delta for game is explicitly given, or not
        /// </summary>
        public bool IsPositionDeltaGiven
        {
            get
            {
                return (PositionDeltaX != 0f) || (PositionDeltaY != 0f);
            }
        }

        public Vector2 Position
        {
            get
            {
                return new Vector2(PositionX,PositionY);
            }
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }

        public Vector2 PositionDelta
        {
            get
            {
                return new Vector2(PositionDeltaX, PositionDeltaY);
            }
            set
            {
                PositionDeltaX = value.X;
                PositionDeltaY = value.Y;
            }
        }

        /// <summary>
        /// Optionally a download/install task ongoing for this game
        /// </summary>
        public GameDownloadAndInstallTask DlAndInstallTask = null;

        /// <summary>
        /// A ThreadedTask that wraps DlAndInstallTask, so that download/install happens in own thread.
        /// </summary>
        public ThreadedTask ThreadedDlAndInstallTask = null;

        //-- private vars
        private bool isInstalled = false;
        private bool refreshInstallationStatusNeeded = true;

        protected GardenItem()
        {            
        }

        public void Dispose()
        {
            if (ThreadedDlAndInstallTask != null)
            {
                ThreadedDlAndInstallTask.Abort();
            }
        }

        /// <summary>
        /// get full file path from base directory to a game's .exe (to check it's there).
        /// </summary>
        /// <returns></returns>
        public string ExeFilepath
        {
            get {
                if (IsBundleItem)
                    return Path.Combine(GameFolder, ExeFile);
                else
                    return Path.Combine(GameFolder , CdPath , ExeFile);
            }
        }


        /// <summary>
        /// check whether this game is locally installed, if true it is. Use Refresh() to
        /// enforce the installation check again.
        /// </summary>
        public bool IsInstalled
        {
            get
            {
                if (!IsGrowable)            // non-growable items are assumed installed by default
                    isInstalled = true;

                if (refreshInstallationStatusNeeded) // avoid continuous calling of Directory.Exists via this mechanism
                {
                    isInstalled = File.Exists(ExeFilepath) && (DlAndInstallTask == null || DlAndInstallTask.IsFinished());
                    refreshInstallationStatusNeeded = false;
                }
                return isInstalled;
            }
        }

        public float InstallProgress
        {
            get
            {
                if (ThreadedDlAndInstallTask != null)
                {
                    return (float) ThreadedDlAndInstallTask.Progress();
                }
                else
                {
                    if (IsInstalled)
                        return 1f;
                    else
                        return 0f;
                }
            }
        }

        /// <summary>
        /// check whether this game is a web-playable game in the browser
        /// </summary>
        public bool IsWebGame
        {
            get
            {
                return ExeFile.StartsWith("http://") || ExeFile.StartsWith("https://");
            }
        }

        public bool IsInstalling
        {
            get
            {
                return (ThreadedDlAndInstallTask != null) && (!ThreadedDlAndInstallTask.IsFinished());
            }
        }

        /// <summary>
        /// check whether this is a gamelib for igg
        /// </summary>
        public bool IsGameLib
        {
            get
            {
                return GameID.StartsWith("gwg_gamelib");
            }
        }

        /// <summary>
        /// check whether this game can be grown at all (i.e. downloaded and/or unpacked).
        /// Some items may not be growable e.g. display-icon-only games, system items, or coming-soon items.
        /// </summary>
        public bool IsGrowable
        {
            get
            {
                if (IsSystemPackage)
                {
                    if (IsGameLib)
                        return true;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// checks whether this game can be played (i.e. an .exe file can be launched)
        /// </summary>
        public bool IsPlayable
        {
            get
            {
                if (!IsGrowable)
                    return false;
                if(ExeFile.ToLower().EndsWith(".exe"))
                    return true;
                if (IsSectionId || IsMusic )
                    return false;
                return true; // for games that don't specify their .exe file.                
            }
        }

        /// <summary>
        /// checks whether this game item is a music track (.ogg)
        /// </summary>
        public bool IsMusic
        {
            get
            {
                return (ExeFile.ToLower().EndsWith(".ogg"));
            }
        }

        /// <summary>
        /// checks whether this item is a system item, which are handled slightly 
        /// differently. (E.g. unpacked in another place)
        /// </summary>
        protected bool IsSystemPackage
        {
            get
            {
                return GameID.StartsWith("igg");
            }
        }


        /// <summary>
        /// checks for item of type SectionID.
        /// </summary>
        public bool IsSectionId
        {
            get
            {
                return GameID.StartsWith("section_");
            }
        }

        /// <summary>
        /// checks whether this item is visible to the user, depending on a.o. user's 
        /// client version and other properties of the item
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return  (!IsSectionId) && 
                        (VisibilityLabel > 0);
            }
        }

        /// <summary>
        /// checks whether this item is a bundle-item, which means it is a single .exe
        /// and can be stored in another location i.e. the GardenConfig.BundleDataPath
        /// </summary>
        public bool IsBundleItem
        {
            get
            {
                // if (!PackedFileURL.Contains("/") && PackedFileURL.EndsWith(".exe"))
                if (ExeFile.Equals(packedFileURL) && ExeFile.Length > 0 )
                    return true;
                return false;
            }
        }

        public string PackedFileExtension
        {
            get
            {
                string s = ExtractFileExtension(packedFileURL);
                return s;
            }
        }

        /// <summary>
        /// get the folder where a game/item is stored (unpacked)
        /// </summary>
        /// <returns></returns>
        public string GameFolder
        {
            get
            {
                // if bundle, then special path
                if (IsBundleItem)
                    return GardenConfig.Instance.BundleDataPath;
                string folder;
                folder = GardenConfig.Instance.UnpackedFilesFolder;
                // if has own folder, then don't return a folder based on GameID
                if (HasOwnFolder)
                    return folder;
                else
                    return Path.Combine(folder , GameIDwithVersion);
            }
        }

        /// <summary>
        /// get the folder where the .exe file of the game/item is stored
        /// </summary>
        public string ExeFolder
        {
            get
            {
                return Path.Combine(GameFolder, CdPath);
            }
        }

        public string ThumbnailFile
        {
            get
            {
                if (thumbnailURL.Length > 0)
                {
                    // if thumbnail url given, use the file format extension for generating local thumbnail file name.
                    string ext = ExtractFileExtension(thumbnailURL);
                    return GameIDwithVersion + "." + ext;
                }
                else
                {
                    return GameIDwithVersion + ".png";
                }
            }
        }

        // extract an extension e.g. "zip" from a partial or full URL e.g. http://server/test/name.zip 
        // <returns>extension after last dot, or default "zip" if no dot found in 'urlDl'.</returns>
        private string ExtractFileExtension(string url)
        {
            int i = url.LastIndexOf('.');
            if (i == -1)
                return "zip";
            string s = url.Substring(i + 1);
            if (s.Length > 3)
            {
                return "zip"; // HACK - if no ext found, assume zip.
            }
            return s;
        }


        /// <summary>
        /// refresh information by reading from local disk (e.g. installation status etc.)
        /// </summary>
        public void Refresh()
        {
            refreshInstallationStatusNeeded = true;
        }

        public void dummyCodeTODO() {
            // update with default mirror location, only if a main location is defined
            // if no main location is given, use default location as main DL location 
            string defaultDownloadLoc;
            if (!IsBundleItem)
                defaultDownloadLoc = GardenConfig.Instance.PackedFilesServerURL + PackedFileName;
            else
                defaultDownloadLoc = GardenConfig.Instance.BundleFilesServerURL + PackedFileName;
            if (PackedFileURL.Length > 0)
                packedFileMirrors.Add( defaultDownloadLoc );
            else
                PackedFileURL = defaultDownloadLoc;

        }

    }
}

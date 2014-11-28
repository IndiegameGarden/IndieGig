// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using TTengine.Core;
using TTengine.Util;
using IndiegameGarden.Install;
using IndiegameGarden.Util;

namespace IndiegameGarden.Base
{
    /**
     * <summary>represents all data and status of a game that a user can select, download and start/play</summary>
     */
    public class GardenItem: IDisposable
    {
        /// <summary>
        /// internally used string ID for item, no whitespace allowed
        /// </summary>
        public string ID = "";

        /// <summary>
        /// Name of game
        /// </summary>
        public string Name = "";

        protected string description = "";

        protected string thumbnailURL = "";

        protected string packedFileURL = "";

        protected List<string> packedFileMirrors = new List<string>();

        /// <summary>
        /// visibility of item for the user
        /// </summary>
        public bool IsVisible = true;

        /// <summary>
        /// some hints for the player e.g. what the control keys are.
        /// </summary>
        public string HelpText = "";

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

        public bool HasOwnFolder = false;

        /// <summary>
        /// Displayable status string of the current item, e.g. if downloading or failed to download.
        /// </summary>
        public string Status = null;

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
                    return GardenConfig.Instance.BundleExesPath;
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
                lineCount = -1;
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
                    return ID;
                else
                    return ID + "_v" + Version;
            }
        }
        
        //-- private vars
        private bool isInstalled = false;
        private bool refreshInstallationStatusNeeded = true;

        public GardenItem(string id)
        {
            this.ID = id;
        }

        public void Dispose()
        {
            //
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
                    isInstalled = File.Exists(ExeFilepath);
                    refreshInstallationStatusNeeded = false;
                }
                return isInstalled;
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
                if (IsMusic )
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
                return ID.StartsWith("igg");
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
                string s = Util.Util.ExtractFileExtension(packedFileURL);
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
                    return GardenConfig.Instance.BundleExesPath;
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
                    string ext = Util.Util.ExtractFileExtension(thumbnailURL);
                    return GameIDwithVersion + "." + ext;
                }
                else
                {
                    return GameIDwithVersion + ".png";
                }
            }
        }

        /// <summary>
        /// refresh information by reading from local disk (e.g. installation status etc.)
        /// </summary>
        public void Refresh()
        {
            refreshInstallationStatusNeeded = true;
        }

        /*
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
         */

    }
}

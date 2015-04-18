// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace IndiegameGarden.Base
{
    /**
     * configuration data object for the Garden application. Information loaded from JSON file.
     */
    public class GardenConfig
    {
#if DEBUG
        protected const string DEBUG_PATH = "..\\..\\..\\..\\..\\";
#else
        protected const string DEBUG_PATH = ""; // for real release
        //protected const string DEBUG_PATH = "..\\..\\..\\..\\..\\"; // for debugging of release
#endif
        /// <summary>
        /// name of the current client (and current version)
        /// </summary>
        protected const string NAME = "IndieGig";

        /// <summary>
        /// value is constant for a build! update this manually for new version builds. 
        /// </summary>
        protected const int VERSION = 10;

        /// <summary>
        /// specifies relative folder from which to copy files to the DataPath folder during init
        /// </summary>
        protected const string COPY_FILES_SRC_PATH = "."; 
  
        /// <summary>
        /// the instance
        /// </summary>
        protected static GardenConfig instance = null;

        public GardenConfig()
        {
            InitDefaults();                        
        }

        public static GardenConfig Instance
        {
            get
            {
                if (instance == null)
                    instance = new GardenConfig();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// First init with only default values put into field. May be overwritten later
        /// </summary>
        protected void InitDefaults()
        {
            TempFolder = Path.GetFullPath( Path.Combine(Path.GetTempPath(), ClientName) );

            // For bundle single .exe files, save in same folder as Indiegame Garden exe (nice for collecting them)
            BundleExesPath = Path.GetFullPath(".");
            PackedFilesFolder = Path.GetFullPath( DEBUG_PATH + "zips");
            UnpackedFilesFolder = TempFolder;
            ThumbnailsFolder = Path.GetFullPath( DEBUG_PATH + "thumbs");

            PackedFilesServerURL = "http://indie.indiegamegarden.com/zips/";
            BundleFilesServerURL = "http://www.indiegamegarden.com/";

        }

        /// <summary>
        /// verify that the IGG storage folders valid. If needed, create folders 
        /// </summary>
        /// <returns>true if folders' existence was verified, false if not (and could not be created)</returns>
        public bool VerifyStorageFolders()
        {
            // verify and/or create all individual folders
            if (!VerifyFolder(ThumbnailsFolder)) return false;
            if (!VerifyFolder(PackedFilesFolder)) return false;
            if (!VerifyFolder(UnpackedFilesFolder)) return false;

            return true;
        }

        /// <summary>
        /// helper method to verify a single folder existence, creating it if needed
        /// </summary>
        /// <param name="folderName">folder name</param>
        /// <returns>true if folder exists or could be created, false if not exists and could not be created</returns>
        protected bool VerifyFolder(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                try
                {
                    Directory.CreateDirectory(folderName);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            if (Directory.Exists(folderName))
                return true;
            else
                return false;
        }

        /// <summary>
        /// check whether this config is valid e.g. by checking for certain mandatory properties.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// a temp folder that is created by client to store files/folders
        /// </summary>
        public string TempFolder { get; set; }

        /// <summary>
        /// a folder path, abs or rel, pointing to the bundle location where all bundle .exe file downloads go to
        /// </summary>
        public string BundleExesPath { get; set; }

        /// <summary>
        /// abs folder path name where packed files (zip, rar, etc) of games are stored
        /// </summary>
        public string PackedFilesFolder { get; set; }

        /// <summary>
        /// abs folder path where unpacked folders of games reside
        /// </summary>
        public string UnpackedFilesFolder { get; set; }

        /// <summary>
        /// abs folder path where thumbnails are stored
        /// </summary>
        public string ThumbnailsFolder { get; set; } 

        /// <summary>
        /// returns the version of current running IndiegameGarden client
        /// </summary>
        public int ClientVersion { 
            get {
                return VERSION;
            }
        }

        /// <summary>
        /// returns the name of current running IndiegameGarden client
        /// </summary>
        public string ClientName
        {
            get
            {
                return NAME;
            }
        }

        /// <summary>
        /// url of a mirror server storing packed files (incl path) for all or most games
        /// </summary>
        public string PackedFilesServerURL { get; set; }
        public string BundleFilesServerURL { get; set; }

        /// <summary>
        /// get file path to locally stored thumbnail file for game
        /// </summary>
        /// <param name="g"></param>
        /// <returns>by default a .png thumbnail for a game (whether file exists or not)
        /// but if a .jpg thumbnail exists, it is chosen.</returns>
        public string GetThumbnailFilepath(GardenItem g)
        {
            string p1 = Path.Combine(ThumbnailsFolder , g.GameIDwithVersion);
            string p2 = p1;
            p1 += ".jpg";
            p2 += ".png";
            if (File.Exists(p2))
                return p2;
            else if (File.Exists(p1))
                return p1;
            else
                return p2;
        }
               
    }
}

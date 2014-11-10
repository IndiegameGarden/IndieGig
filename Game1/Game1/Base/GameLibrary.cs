// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace IndiegameGarden.Base
{
    /**
     * stores a library of game information, parsed from JSON format files
     */
    public class GameLibrary: IDisposable
    {
        GameCollection gamesCollection;
        int version = 0;

        /// <summary>
        /// create new game library 
        /// </summary>
        public GameLibrary()
        {
        }

        /// <summary>
        /// return version number of this game library
        /// </summary>
        public int Version
        {
            get
            {
                return version;
            }
        }

        /// <summary>
        /// width of garden (number of items horizontally)
        /// </summary>
        public int GardenSizeX
        {
            get
            {
                return 100; // TODO configurable
            }
        }

        /// <summary>
        /// max height of garden (number of items vertically)
        /// </summary>
        public int GardenSizeY
        {
            get
            {
                return 100; // TODO configurable
            }
        }
       
        /// <summary>
        /// last init steps that are taken after loading a gamelib into gamesCollection
        /// </summary>
        void FinalizeGamelibForUse()
        {
        }

        public void Dispose()
        {
            gamesCollection.Dispose();
        }

        /// <summary>
        /// get a GameCollection containing all games in the library
        /// </summary>
        /// <returns>GameCollection containing all games in the library</returns>
        public GameCollection GetList()
        {
            return gamesCollection;
        }
    }
}

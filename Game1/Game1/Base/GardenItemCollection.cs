using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using IndiegameGarden.Util;

namespace IndiegameGarden.Base
{
    /**
     * a collection/list of games
     */
    public class GardenItemCollection: List<GardenItem> , IDisposable
    {

        public GardenItemCollection(): base()
        {
        }

        public void Dispose()
        {
            foreach (GardenItem g in this)
            {
                g.Dispose();
            }
        }

        public GardenItem FindGameNamed(string gameID)
        {
            foreach (GardenItem gi in this)
            {
                if (gi.GameID.Equals(gameID))
                    return gi;
            }
            return null;
        }
    
    }
}

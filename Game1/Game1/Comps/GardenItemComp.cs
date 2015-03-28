// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;
using Artemis.Interface;
using IndiegameGarden.Base;

namespace Game1.Comps
{
    /// <summary>
    /// Comp indicating the GardenItem related to Entity
    /// </summary>
    public class GardenItemComp: IComponent
    {
        public GardenItem Item;

        public GardenItemComp(GardenItem gi)
        {
            this.Item = gi;
        }

    }
}

// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using System.Collections.Generic;
using System.Text;

using IndiegameGarden.Base;

namespace Game1
{
    public class IndieGigCollection: GardenItemCollection
    {
        public IndieGigCollection()
        {
            Create();
        }

        /// <summary>
        /// Fill the collection
        /// </summary>
        protected void Create()
        {
            GardenItem g;

            g = CreateItem("FoxAliens");
            g.Name = "Fox Aliens";
            g.ExeFile = "FOXLZRS.exe";
        }

        private GardenItem CreateItem(string id)
        {
            GardenItem g = new GardenItem(id);
            Add(g);
            return g;
        }
    }
}

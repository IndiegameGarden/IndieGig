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

            g = CreateItem("lisa");
            g.Name = "Lisa";
            g.ExeFile = "lisa.exe";

            g = CreateItem("spelunky");
            g.Name = "Spelunky";
            g.ExeFile = "Spelunky.exe";
            g.ExeConfig = "config.exe";

            g = CreateItem("RussianSubwayDogs");
            g.Name = "Russian Subway Dogs";
            g.ExeFile = "Russian Subway Dogs PKE.exe";

            g = CreateItem("GameTitle");
            g.Name = "Game Title";
            g.ExeFile = "Game Title.exe";

            g = CreateItem("Arvoesine");
            g.Name = "Arvoesine";
            g.ExeFile = "Arvoesine.exe";
            g.ExeConfig = "config.exe";

            g = CreateItem("In60Seconds");
            g.Name = "In 60 Seconds";
            g.ExeFile = "I60Sf.exe";

            g = CreateItem("supercratebox");
            g.Name = "Super Crate Box";
            g.ExeFile = "supercratebox.exe";

            g = CreateItem("maru");
            g.Name = "Maru";
            g.ExeFile = "Maru.exe";

            g = CreateItem("YETI-HUNTER");
            g.Name = "Yeti Hunter";
            g.ExeFile = "YETI HUNTER.exe";

            g = CreateItem("Beacon");
            g.Name = "Beacon";
            g.ExeFile = "Beacon.exe";

            g = CreateItem("blackfoot");
            g.Name = "Blackfoot";
            g.ExeFile = "Blackfoot-v2.exe";

            g = CreateItem("MythicDefence");
            g.Name = "Mythic Defence";
            g.ExeFile = "Mythic Defence - The rise of evil.exe";

            g = CreateItem("Wither");
            g.Name = "Wither";
            g.ExeFile = "RPG_RT.exe";

            g = CreateItem("MagicOwl");
            g.Name = "Magic Owl";
            g.ExeFile = "owl.exe";
            g.ExeConfig = "winsetup.exe";

            g = CreateItem("artibeus");
            g.Name = "Artibeus";
            g.ExeFile = "artibeus.exe";

            g = CreateItem("MadeOfMirrors");
            g.Name = "Made Of Mirrors";
            g.ExeFile = "Made of Mirrors v1.03.exe";

            g = CreateItem("charge");
            g.Name = "Charge";
            g.ExeFile = "charge.exe";

            g = CreateItem("HighRiseRunners");
            g.Name = "High-Rise Runners";
            g.ExeFile = "High-Rise Runners.exe";

            g = CreateItem("spaceWorms");
            g.Name = "Space Worms";
            g.ExeFile = "spaceWorms.exe";

            g = CreateItem("TheBeatTheStepAndTheCowboys");
            g.Name = "The Beat The Step and The Cowboys";
            g.ExeFile = "TheBeatTheStepAndTheCowboys.exe";

            g = CreateItem("Code7");
            g.Name = "Code 7 (prolog)";
            g.ExeFile = "code7.exe";

            g = CreateItem("IAmJason");
            g.Name = "IAMJASON";
            g.ExeFile = "IAMJASON.exe";
            g.ExeConfig = "winsetup.exe";

        }

        private GardenItem CreateItem(string id)
        {
            GardenItem g = new GardenItem(id);
            Add(g);
            return g;
        }
    }
}

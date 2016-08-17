// (c) 2010-2015 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
using System;
using TTengine.Util;

namespace Game1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                using (Game1 game = new Game1())
                {
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                MsgBox.Show("FEIG! (Fatal Error In Game)",
                  "Fatal Error - if you want you can notify the author.\n" + ex.Message + "\n" + ex.ToString());
            }

        }
    }
#endif
}


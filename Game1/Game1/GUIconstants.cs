using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game1
{
    public class GUIconstants
    {
        // UI constants
        public const double ICON_SIZE = 128, // pixels

                            SCALE_SELECTED = 1.2,
                            SCALE_UNSELECTED = 1.0,
                            SCALE_WHILE_EXITING = 0.0002,
                            SCALE_TO_BACKGROUND = SCALE_WHILE_EXITING,
                            SCALE_TO_FOREGROUND = SCALE_UNSELECTED,

                            SPEED_SCALE_TO_SELECTED = 0.1,
                            SPEED_SCALE_TO_UNSELECTED = 0.1,
                            SPEED_SCALE_ICON_WHILE_EXITING = 0.1,
                            SPEED_SCALE_ICON_TO_FOREGROUND = SPEED_SCALE_TO_SELECTED,
                            SPEED_SCALE_ICON_TO_BACKGROUND = SPEED_SCALE_ICON_WHILE_EXITING,

                            MUSIC_FADEOUT_ON_EXIT_SPEED = 0.45;
        public static double SCALE_MAX = Math.Max(SCALE_SELECTED, SCALE_TO_FOREGROUND);
        public const int ICONCOUNT_HORIZONTAL = 9; // FIXME make adaptive to screen size

    }
}

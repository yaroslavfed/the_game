using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace the_game
{
    class resource_paths
    {
        public static readonly string docPath = System.IO.Directory.GetCurrentDirectory();
        public static readonly string userPath = System.IO.Directory.GetCurrentDirectory() + @"\users\";
        public static readonly string iconPath = System.IO.Directory.GetCurrentDirectory() + @"\img\usersIcon\";
        public static readonly string hero_icon_Path = System.IO.Directory.GetCurrentDirectory() + @"\img\heroIcon\";
        public static readonly string weaponPath = System.IO.Directory.GetCurrentDirectory() + @"\img\wIcon\";
        public static readonly string armorPath = System.IO.Directory.GetCurrentDirectory() + @"\img\aIcon\";
        public static readonly string spellsPath = System.IO.Directory.GetCurrentDirectory() + @"\img\spells\";
        public static readonly string inventoryPath = System.IO.Directory.GetCurrentDirectory() + @"\inventory\";
        public static readonly string media_back_path = System.IO.Directory.GetCurrentDirectory() + @"\background_video\";
        public static readonly string user_record_Path = System.IO.Directory.GetCurrentDirectory() + @"\records\";
    }
}

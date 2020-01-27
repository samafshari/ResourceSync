using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceSync
{
    public class Settings
    {
        public static Settings Instance { get; set; }

        public string PathiOS { get; set; }
        public string PathAndroid { get; set; }
        public string Extensions { get; set; } = "png";
        public string Android1x { get; set; } = "drawable-hdpi";
        public string Android2x { get; set; } = "drawable-xhdpi";
        public string Android3x { get; set; } = "drawable-xxhdpi";
    }
}

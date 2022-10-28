using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PlayerIOClient;

namespace Chat_re_trucho
{
    internal static class AppData
    {
        public static readonly Brush[] MessagesColorPallete = new Brush[]
        {
            new SolidColorBrush(Color.FromRgb(255,165,150)),
            new SolidColorBrush(Color.FromRgb(153,255,155)),
            new SolidColorBrush(Color.FromRgb(176,196,255)),
            new SolidColorBrush(Color.FromRgb(247,255,150)),
            new SolidColorBrush(Color.FromRgb(254,150,255)),
        };

        public static string Username { get; set; }
        public static Client Client { get; set; }
        public static Connection ClientConnection { get; set; }
    }
}

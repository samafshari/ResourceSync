using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;

namespace ResourceSync
{
    public class MainViewModel : BindableModel
    {
        public string PathiOS { get; set; }
        public string PathAndroid { get; set; }
        public string Extensions { get; set; } = "jpg,png";
        public string Folder1x { get; set; } = "drawable-xhdpi";
        public string Folder2x { get; set; } = "drawable-xhdpi";
        public string Folder3x { get; set; } = "drawable-xhdpi";
        public new string Log { get; set; }

        public MainViewModel()
        {
            Status = TaskStatuses.Success;
        }

        public Command DoCommand => new Command(() =>
        {
            Task.Run(async () =>
            {
                Log = "";
                var exts = Extensions.ToLower().Split(',');
                var files = Directory.EnumerateFiles(PathiOS)
                    .Where(f => exts.Any(e => f.ToLower().EndsWith($".{e}")))
                    .OrderByDescending(x => x.Length)
                    .ThenByDescending(x => x)
                    .ToArray();

                foreach (var item in files)
                {
                    var ext = exts.First(x => item.ToLower().EndsWith(x));
                    var dest = Path.Combine(PathAndroid, Folder1x);
                    if (item.ToLower().EndsWith($"@2x.{ext}"))
                        dest = Path.Combine(PathAndroid, Folder2x);
                    else if (item.ToLower().EndsWith($"@3x.{ext}"))
                        dest = Path.Combine(PathAndroid, Folder3x);

                    var fi = new FileInfo(item);
                    dest = Path.Combine(dest, fi.Name.Replace("@2x", "").Replace("@3x", ""));
                    
                    File.Copy(item, dest, true);
                    Log += $"{item} -> {dest}\n";
                    UpdateProperties();
                    await Task.Yield();
                }
            });
        });
    }
}

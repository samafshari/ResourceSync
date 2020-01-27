using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using RedCorners;

namespace ResourceSync
{
    public class MainViewModel : BindableModel
    {
        string _pathiOS, _pathAndroid;

        public string PathiOS
        {
            get => _pathiOS;
            set
            {
                SetProperty(ref _pathiOS, value);
                TryGuessAndroidPath();
            }
        }

        public string PathAndroid
        {
            get => _pathAndroid;
            set
            {
                SetProperty(ref _pathAndroid, value);
                TryGuessiOSPath();
            }
        }

        public string Extensions { get; set; } = "jpg,png";
        public string Folder1x { get; set; } = "drawable-xhdpi";
        public string Folder2x { get; set; } = "drawable-xhdpi";
        public string Folder3x { get; set; } = "drawable-xhdpi";
        public new string Log { get; set; }

        public Visibility ControlsVisibility => IsBusy ? Visibility.Collapsed : Visibility.Visible;
        public Visibility WorkingVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;

        public MainViewModel()
        {
            Status = TaskStatuses.Success;
        }

        void Load(Settings model)
        {
            PathiOS = model.PathiOS;
            PathAndroid = model.PathAndroid;
            Extensions = model.Extensions;
            Folder1x = model.Android1x;
            Folder2x = model.Android2x;
            Folder3x = model.Android3x;
            UpdateProperties();
        }

        public Command ClearCommand => new Command(() =>
        {
            Load(new Settings());
        });

        public Command LoadCommand => new Command(() =>
        {
            try
            {
                var fo = new OpenFileDialog();
                fo.DefaultExt = "sync";
                fo.ShowDialog();
                if (File.Exists(fo.FileName))
                {
                    // TODO: Load Model
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        });

        public Command SaveCommand => new Command(() =>
        {
            try
            {
                var fo = new SaveFileDialog();
                fo.DefaultExt = "sync";
                fo.ShowDialog();
                // TODO: Save Model
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        });

        public Command BrowseiOSCommand => new Command(() =>
        {
            try
            {
                // TODO: Pick folder dialog
                string path = "";
                if (Directory.Exists(path))
                    PathiOS = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        });

        public Command BrowseAndroidCommand => new Command(() =>
        {
            try
            {
                // TODO: Pick folder dialog
                string path = "";
                if (Directory.Exists(path))
                    PathAndroid = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        });

        void TryGuessAndroidPath()
        {
            if (PathAndroid.HasValue()) return;
            // TODO: Guess
        }

        void TryGuessiOSPath()
        {
            if (PathiOS.HasValue()) return;
            // TODO: Guess
        }

        public Command DoiOSCommand => new Command(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    Status = TaskStatuses.Busy;
                    Log = "";
                    var exts = Extensions.ToLower().Split(',');
                    var files = Directory.EnumerateFiles(PathiOS)
                        .Where(f => exts.Any(e => f.ToLower().EndsWith($".{e}")))
                        .OrderBy(x => x.Length)
                        .ThenBy(x => x)
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
                }
                finally
                {
                    Status = TaskStatuses.Success;
                }
            });
        });

        public Command DoAndroidCommand => new Command(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    Status = TaskStatuses.Busy;
                    Log = "";
                    var exts = Extensions.ToLower().Split(',');
                    // TODO: Do Android
                }
                finally
                {
                    Status = TaskStatuses.Success;
                }
            });
        });
    }
}

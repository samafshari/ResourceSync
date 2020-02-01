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
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;

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

        public string Extensions { get; set; } 
        public string Folder1x { get; set; } 
        public string Folder2x { get; set; } 
        public string Folder3x { get; set; } 
        public new string Log { get; set; }

        public Visibility ControlsVisibility => IsBusy ? Visibility.Collapsed : Visibility.Visible;
        public Visibility WorkingVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;

        public MainViewModel()
        {
            Status = TaskStatuses.Success;
            ClearCommand.Execute(null);
            if (File.Exists("defaults.sync"))
                Load("defaults.sync");
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

        void Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var model = JsonConvert.DeserializeObject<Settings>(json);
                Load(model);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        }

        void Save(string path)
        {
            Settings.Instance = new Settings
            {
                Android1x = Folder1x,
                Android2x = Folder2x,
                Android3x = Folder3x,
                Extensions = Extensions,
                PathAndroid = PathAndroid,
                PathiOS = PathiOS
            };

            try
            {
                var json = JsonConvert.SerializeObject(Settings.Instance);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        }

        void SaveDefaults()
        {
            Save("defaults.sync");
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
                fo.Filter = "Sync Settings|*.sync";
                fo.ShowDialog();
                if (File.Exists(fo.FileName))
                {
                    Load(fo.FileName);
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
                fo.Filter = "Sync Settings|*.sync";
                fo.CheckFileExists = false;
                fo.ShowDialog();
                Save(fo.FileName);
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
                var path = PickFolder();
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
                var path = PickFolder();
                if (Directory.Exists(path))
                    PathAndroid = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
            }
        });

        string PickFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            dialog.ShowDialog();
            if (Directory.Exists(dialog.FileName))
                return dialog.FileName;
            return null;
        }

        void TryGuessAndroidPath()
        {
            if (PathAndroid.HasValue()) return;
            if (PathiOS.IsNW()) return;
            if (!PathiOS.Contains(".iOS")) return;
            var path = PathiOS.Replace(".iOS", ".Android");

            if (!Directory.Exists(path))
                path = PathiOS.Replace(".iOS", ".Droid");
            if (Directory.Exists(path))
                PathAndroid = path;
        }

        void TryGuessiOSPath()
        {
            if (PathiOS.HasValue()) return;
            if (PathAndroid.IsNW()) return;
            if (!PathAndroid.Contains(".Android") && !PathAndroid.Contains(".Droid")) return;
            var path = PathAndroid.Replace(".Android", ".iOS").Replace(".Droid", ".iOS");
            if (Directory.Exists(path))
                PathiOS = path;
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
                    SaveDefaults();
                }
                finally
                {
                    Status = TaskStatuses.Success;
                }
            });
        });

        public Command DoAndroidCommand => new Command(() =>
        {
            async Task DoAndroidFolderAsync(string directory, string suffix, string ext)
            {
                var files = Directory.EnumerateFiles(directory).Where(x => x.EndsWith(ext));
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    var name = fi.Name;
                    name = name.Substring(0, name.Length - (ext.Length + 1)) + suffix + "." + ext;
                    var dest = Path.Combine(PathiOS, name);
                    File.Copy(file, dest, true);
                    Log += $"{file} -> {dest}\n";
                }
                UpdateProperties();
                await Task.Yield();
            }

            Task.Run(async () =>
            {
                try
                {
                    Status = TaskStatuses.Busy;
                    Log = "";
                    var exts = Extensions.ToLower().Split(',');
                    foreach (var ext in exts)
                    {
                        await DoAndroidFolderAsync(Path.Combine(PathAndroid, Folder1x), "", ext);
                        await DoAndroidFolderAsync(Path.Combine(PathAndroid, Folder2x), "@2x", ext);
                        await DoAndroidFolderAsync(Path.Combine(PathAndroid, Folder3x), "@3x", ext);
                    }
                    SaveDefaults();
                }
                finally
                {
                    Status = TaskStatuses.Success;
                }
            });
        });
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using CmlLib.Core.VersionLoader;
using Windows.System;

namespace UniversalLauncherWinUI.ViewModels;

public partial class LauncherViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _gameVersions;

    [ObservableProperty] private string _selectedGameVersion;

    [ObservableProperty] private string _playerName;

    private readonly MinecraftLauncher _launcher;

    [RelayCommand(CanExecute = nameof(CanLaunchGame))]
    public async void LaunchGame()
    {
        var process = await _launcher.BuildProcessAsync(SelectedGameVersion, new MLaunchOption
        {
            JavaPath = Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin\\java.exe",
            Session = MSession.CreateOfflineSession(PlayerName),
            MaximumRamMb = 4096
        });
        process.Start();
    }

    public bool CanLaunchGame()
    {
        return !string.IsNullOrEmpty(SelectedGameVersion) && !string.IsNullOrEmpty(PlayerName);
    }


    public LauncherViewModel()
    {
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SelectedGameVersion) || args.PropertyName == nameof(PlayerName))
            {
                LaunchGameCommand.NotifyCanExecuteChanged();
            }
        };

        var path = new MinecraftPath();
        var parameters = MinecraftLauncherParameters.CreateDefault(path);
        parameters.VersionLoader = new LocalJsonVersionLoader(path);
        var launcher = new MinecraftLauncher(parameters);

        GameVersions = new ObservableCollection<string>(launcher.GetAllVersionsAsync().Result.Select(v => v.Name));

        _launcher = launcher;
    }
}

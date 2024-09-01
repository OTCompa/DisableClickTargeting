using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SamplePlugin;

internal class Services
{
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider gameInteropProvider { get; set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; set; } = null!;
    [PluginService] internal static ICondition Condition { get; set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    internal static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();
    }
}

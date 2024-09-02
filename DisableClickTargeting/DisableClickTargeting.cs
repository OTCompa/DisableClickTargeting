using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
namespace SamplePlugin
{
    internal unsafe class DisableClickTargeting : IDisposable
    {
        private const int LMB = 11;
        private const int RMB = 4;

        private Configuration Configuration;
        private TargetSystem* targetSystem;

        private delegate char GetInputStatusDelegate(long a1, int a2);
        [Signature("E8 ?? ?? ?? ?? 84 C0 74 6E 48 8B 87 ?? ?? ?? ?? 48 8B F3 48 85 C0 41 0F 95 C6", DetourName = nameof(GetInputStatusDetour))]
        private readonly Hook<GetInputStatusDelegate> _getInputStatusHook;

        public DisableClickTargeting(Plugin plugin)
        {
            Configuration = plugin.Configuration;
            Services.gameInteropProvider.InitializeFromAttributes(this);
            targetSystem = TargetSystem.Instance();
            this._getInputStatusHook?.Enable();
            //this._isInputIDClickedHook?.Enable();
        }

        public void Dispose()
        {
            this._getInputStatusHook.Dispose();
            //this._isInputIDClickedHook?.Dispose();
        }

        private char GetInputStatusDetour(long a1, int a2)
        {
            /*
            if (a2 == LMB)
            {
                Services.PluginLog.Debug(a2.ToString());
            }
            */

            var MoTarget = targetSystem->MouseOverTarget;
            var MoNameplateTarget = targetSystem->MouseOverNameplateTarget;
            if (
                    Configuration.EnablePlugin &&
                    (Configuration.DisableLMB && a2 == LMB) &&
                    Services.Condition[ConditionFlag.InCombat] &&
                    (
                        (MoTarget != null && MoTarget->ObjectKind == ObjectKind.Pc) || 
                        (MoNameplateTarget != null && MoNameplateTarget->ObjectKind == ObjectKind.Pc)
                    )
                )
            {
                return (char)0;
            }

            if (
                    Configuration.EnablePlugin &&
                    (Configuration.DisableRMB && a2 == RMB) &&
                    Services.Condition[ConditionFlag.InCombat] &&
                    (
                        (MoTarget != null && MoTarget->ObjectKind == ObjectKind.Pc) ||
                        (MoNameplateTarget != null && MoNameplateTarget->ObjectKind == ObjectKind.Pc)
                    )
                )
            {
                return (char)0;
            }
            
            return this._getInputStatusHook.Original(a1, a2);
            
        }
    }
}

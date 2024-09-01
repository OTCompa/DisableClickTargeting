using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Conditions;
namespace SamplePlugin
{
    internal unsafe class DisableClickTargeting : IDisposable
    {
        private const int LMB = 11;
        private const int RMB = 106;

        private Configuration Configuration;

        private delegate char GetInputStatusDelegate(long a1, long a2, long a3);
        [Signature("E8 ?? ?? ?? ?? 4C 8B BC 24 ?? ?? ?? ?? 4C 8B B4 24 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 48 8B 9C 24 ?? ?? ?? ??", DetourName = nameof(GetInputStatusDetour))]
        private readonly Hook<GetInputStatusDelegate> _getInputStatusHook;


        private delegate char IsInputIDClickedDelegate(UIInputData* a1, int key);
        [Signature("E8 ?? ?? ?? ?? 84 C0 44 8B C3")]
        private readonly IsInputIDClickedDelegate? _isInputIDClicked = null;

        public DisableClickTargeting(Plugin plugin)
        {
            Configuration = plugin.Configuration;
            Services.gameInteropProvider.InitializeFromAttributes(this);
            this._getInputStatusHook?.Enable();
            //this._isInputIDClickedHook?.Enable();
        }

        public void Dispose()
        {
            this._getInputStatusHook.Dispose();
            //this._isInputIDClickedHook?.Dispose();
        }

        private char GetInputStatusDetour(long a1, long a2, long a3)
        {
            var MoTargetKind = Services.TargetManager.MouseOverTarget?.ObjectKind;
            if (
                Configuration.EnablePlugin &&
                (Configuration.DisableLMB && IsInputIDClicked(LMB)) &&
                MoTargetKind == ObjectKind.Player &&
                Services.Condition[ConditionFlag.InCombat]
                )
            {
                return (char)0;
            }

            if (
                Configuration.EnablePlugin &&
                (Configuration.DisableRMB && IsInputIDClicked(RMB)) &&
                MoTargetKind == ObjectKind.Player &&
                Services.Condition[ConditionFlag.InCombat]
                )
            {
                return (char)0;
            }

            return this._getInputStatusHook.Original(a1, a2, a3);
            
        }


        public unsafe bool IsInputIDClicked(int key)
        {
            if (this._isInputIDClicked == null)
            {
                throw new InvalidOperationException("IsInputIDClicked signature wasn't found!");
            }
            var UIInputData = Framework.Instance()->UIModule->GetUIInputData();
            if (this._isInputIDClicked!(UIInputData, key) == 1)
                return true;
            return false;
        }

        /*
        [Signature("E8 ?? ?? ?? ?? 84 C0 44 8B C3", DetourName = nameof(TestDetour))]
        private readonly Hook<IsInputIDClickedDelegate> _isInputIDClickedHook;
        private char TestDetour(UIInputData* a1, int a2)
        {
            var ret = this._isInputIDClickedHook.Original(a1, a2);
            if (ret != 0)
            {
                Services.PluginLog.Debug(a2.ToString());
            }
            return ret;
        }
        */
    }
}

using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.Game;
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

        private delegate GameObject* GetMouseoverObjectDelegate(TargetSystem* a1, int a2, int a3, GameObjectArray* a4, Camera* camera);
        [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 74 50 48 8B CB", DetourName = nameof(GetMouseOverObjectDetour))]
        private readonly Hook<GetMouseoverObjectDelegate> _getMouseoverObjectHook;

        private string[] strings;

        public DisableClickTargeting(Plugin plugin)
        {
            Configuration = plugin.Configuration;
            Services.gameInteropProvider.InitializeFromAttributes(this);
            targetSystem = TargetSystem.Instance();
            strings = new string[8];
            this._getInputStatusHook?.Enable();
            this._getMouseoverObjectHook?.Enable();
        }

        public void Dispose()
        {
            this._getMouseoverObjectHook?.Dispose();
            this._getInputStatusHook.Dispose();
        }

        private char GetInputStatusDetour(long a1, int a2)
        {
            var moNameplateTarget = targetSystem->MouseOverNameplateTarget;
            if (
                    Configuration.EnablePlugin &&
                    (Configuration.DisableLMB && a2 == LMB) &&
                    Services.Condition[ConditionFlag.InCombat] &&
                    (moNameplateTarget != null && moNameplateTarget->ObjectKind == ObjectKind.Pc)
                )
            {
                return (char)0;
            }

            if (
                    Configuration.EnablePlugin &&
                    (Configuration.DisableRMB && a2 == RMB) &&
                    Services.Condition[ConditionFlag.InCombat] &&
                    (moNameplateTarget != null && moNameplateTarget->ObjectKind == ObjectKind.Pc)
                )
            {
                return (char)0;
            }

            return this._getInputStatusHook.Original(a1, a2);
        }

        private GameObject* GetMouseOverObjectDetour(TargetSystem* a1, int a2, int a3, GameObjectArray* a4, Camera* a5)
        {
            var ret = this._getMouseoverObjectHook.Original(a1, a2, a3, a4, a5);
            bool LmbPressed = GetInputStatusDetour((long)InputManager.Instance(), LMB) == 1;
            bool RmbPressed = GetInputStatusDetour((long)InputManager.Instance(), RMB) == 1;

            if
            (
                !Configuration.EnablePlugin ||
                (
                    (!LmbPressed && !RmbPressed) ||
                    (!Configuration.DisableLMB && LmbPressed) ||
                    (!Configuration.DisableRMB && RmbPressed)
                ) ||
                !Services.Condition[ConditionFlag.InCombat] ||
                ret == null ||
                ret->GetObjectKind() != ObjectKind.Pc
            )
            {
                return ret;
            }
            return null;
        }
    }
}

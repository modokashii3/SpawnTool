using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.IO;
using System.Runtime.CompilerServices;

namespace SpawnTool
{
    public class SpawnTool : IPlugin
    {
        public string Name => "Spawn Tool";
        public string Author => "Seka";

        public void OnLoad()
        {
            KeyBindings.AddKeybind("CreateShell", new Keybind<Key>(Key.P, [Key.LeftShift]));
        }

        private int _shellInt32;
        private Monster? _shellOwner;
        private bool _shellMode = false;
        private bool _wasHiding = false;
        private bool _wasPaused = false;
        private int _shellOwnerId;
        private int _emID;
        private int _subID;
        private NativeFunction<MtObject, int, int, bool, nint> _specialSummon = new(0x141a5a3e0);
        private uint _lastStage;
        public void OnMonsterCreate(Monster monster) 
        {
            _lastStage = (uint)Area.CurrentStage; 
        }
        public void OnUpdate(float dt) 
        { 
            if ((uint)Area.CurrentStage != _lastStage)  { 
                _shellOwner = null; 
            } 
        }
        public void OnQuestLeave(int questId) { _shellOwner = null; }
        public void OnQuestComplete(int questId) { _shellOwner = null; }
        public void OnQuestFail(int questId) { _shellOwner = null; }
        public void OnQuestReturn(int questId) { _shellOwner = null; }
        public void OnQuestAbandon(int questId) { _shellOwner = null; }
        public void OnQuestEnter(int questId) { _shellOwner = null; }
        public unsafe void OnImGuiRender()
        {
            var player = Player.MainPlayer; if (player == null) return;

            int emID = _emID;
            if (ImGui.InputScalar("emID", ImGuiDataType.S32, new IntPtr(Unsafe.AsPointer(ref emID)))) { _emID = emID; }
            int subID = _subID;
            if (ImGui.InputScalar("subID", ImGuiDataType.S32, new IntPtr(Unsafe.AsPointer(ref subID)))) { _subID = subID; }

            if (ImGui.Button("Special Summon"))
            {
                _specialSummon.Invoke(Monster.SingletonInstance, _emID, _subID, true);
            }

            var allMonsters = Monster.GetAllMonsters().ToArray();


            if (ImGui.BeginCombo("Shell Owner", $"{_shellOwner}"))
            {
                foreach (var shellOwner in allMonsters)
                {
                    if (ImGui.Selectable($"{shellOwner}", _shellOwner == shellOwner))
                    {
                        _shellOwner = shellOwner;
                    }
                }
                ImGui.EndCombo();
            }
            if (_shellOwner is null) return;
            if (ImGui.Button("Hide"))
            {
                if (!_wasHiding)
                { _shellOwner.Resize(0.01f); _wasHiding = true; }
                else
                { _shellOwner.Resize(1f); _wasHiding = false; }
            }
            if (ImGui.Button("FreezeIt"))
            {
                if (!_wasPaused)
                { _shellOwner.PauseAnimations(); _wasPaused = true; }
                else
                { _shellOwner.ResumeAnimations(); _wasPaused = false; }
            }

            if (_shellOwner != null)
            {
                int shellOwnerId = (int)_shellOwner.Type; // Convert shellOwner Type to int }

                /*
                if (_shellParamList == null) return;
                ShellParamList shellParamList = _shellParamList;
                if (ImGui.InputScalar("Shell List", ImGuiDataType.S32, new IntPtr(Unsafe.AsPointer(ref shellParamList))))
                {
                    _shellParamList = shellParamList;
                }*/

                int shellInt32 = _shellInt32;
                if (ImGui.InputScalar("Shell Index", ImGuiDataType.S32, new IntPtr(Unsafe.AsPointer(ref shellInt32))))
                {
                    /*if (shellInt32 < 0)
                    {
                        shellInt32 = 0;
                    }
                    else if (shellInt32 >= _shellParamList.ShellCount)
                    {
                        shellInt32 = _shellParamList.ShellCount - 1;
                    }*/
                    _shellInt32 = shellInt32;
                }

                if (ImGui.Button("Toggle ShellKey"))
                {
                    if (!_shellMode) { _shellMode = true; } else { _shellMode = false; }
                }

                /*
                if (_shellParamList == null) return;
                if (_shellInt32 >= 0 && _shellInt32 < _shellParamList.ShellCount)
                {
                    ShellParam shell = _shellParamList.GetShell((uint)_shellInt32);
                    if (shell != null)
                    {
                        ImGui.Text($"Shell Index: {_shellInt32}");
                    }
                    else
                    {
                        ImGui.Text("Shell not found.");
                    }
                }
                else
                {
                    ImGui.Text("Index out of range.");
                }*/

                if (player == null) return;
                if (_shellOwner != null && _shellMode && Input.IsPressed(Key.P))
                {
                    _shellOwner.CreateShell(_shellOwnerId, _shellInt32, player.Position, _shellOwner.Position);
                    Log.Info($"Shell Create");
                }
            }
        }
    }
}
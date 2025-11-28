#if !UNITY_EDITOR
using BepInEx;
using BepInEx.Logging;
using System;
using System.IO;
using HeadVoiceSelector.Patches;

namespace HeadVoiceSelector
{
    [BepInPlugin("com.HeadVoiceSelector.Core", "HeadVoiceSelector Core", "1.0.7")]
    public class HeadVoiceSelector : BaseUnityPlugin
    {
        public static HeadVoiceSelector instance;
        public static ManualLogSource LogSource;

        public static string modPath = Path.Combine(Environment.CurrentDirectory, "user", "mods", "WTT-HeadVoiceSelector");
        public static string pluginPath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins");

        internal void Awake()
        {
            instance = this;
            LogSource = Logger;

            Logger.LogError("[HeadVoiceSelector] ============================================");
            Logger.LogError("[HeadVoiceSelector] Plugin Awake called!");
            Logger.LogError($"[HeadVoiceSelector] Plugin path: {pluginPath}");
            Logger.LogError($"[HeadVoiceSelector] Mod path: {modPath}");
            
            // Manually register patches
            Logger.LogError("[HeadVoiceSelector] Registering patches...");
            try
            {
                new OverallScreenPatch().Enable();
                Logger.LogError("[HeadVoiceSelector] OverallScreenPatch registered successfully!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[HeadVoiceSelector] Failed to register OverallScreenPatch: {ex.Message}");
                Logger.LogError($"[HeadVoiceSelector] Stack trace: {ex.StackTrace}");
            }
            
            Logger.LogError("[HeadVoiceSelector] ============================================");
        }
    }
}
#endif

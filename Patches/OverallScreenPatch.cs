#if !UNITY_EDITOR
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using System.Linq;
using HeadVoiceSelector.Core.UI;
using EFT;

namespace HeadVoiceSelector.Patches
{
    public class OverallScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] ============================================");
            HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] GetTargetMethod() called for OverallScreenPatch!");
            
            var overallScreenType = typeof(EFT.UI.OverallScreen);
            HeadVoiceSelector.LogSource.LogError($"[HeadVoiceSelector] OverallScreen type: {overallScreenType.FullName}");
            
            // Get all methods (public and non-public, instance and static)
            var allMethods = overallScreenType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == "Show" || m.Name.Contains("Show"))
                .ToList();
            
            HeadVoiceSelector.LogSource.LogError($"[HeadVoiceSelector] Found {allMethods.Count} methods with 'Show' in name");
            
            foreach (var method in allMethods)
            {
                var parameters = method.GetParameters();
                HeadVoiceSelector.LogSource.LogError($"[HeadVoiceSelector]   - {method.Name} with {parameters.Length} parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
            }
            
            // Try to find the Show method - prefer one with no parameters
            var targetMethod = allMethods.FirstOrDefault(m => m.Name == "Show" && m.GetParameters().Length == 0);
            
            if (targetMethod == null)
            {
                HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] No parameterless Show() found, trying first Show method...");
                targetMethod = allMethods.FirstOrDefault(m => m.Name == "Show");
            }
            
            if (targetMethod != null)
            {
                var pars = targetMethod.GetParameters();
                HeadVoiceSelector.LogSource.LogError($"[HeadVoiceSelector] Selected method: {targetMethod.Name}({string.Join(", ", pars.Select(p => p.ParameterType.Name))})");
            }
            else
            {
                HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] CRITICAL ERROR: No Show method found at all!");
            }
            
            HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] ============================================");
            return targetMethod;
        }

        [PatchPostfix]
        public static void PatchPostfix(EFT.UI.OverallScreen __instance, Profile currentProfile)
        {
            HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] OverallScreenPatch.PatchPostfix called!");
            HeadVoiceSelector.LogSource.LogError($"[HeadVoiceSelector] Profile: {(currentProfile != null ? currentProfile.Nickname : "NULL")}");
            HeadVoiceSelector.LogSource.LogError("[HeadVoiceSelector] Adding customization drawers to OverallScreen...");
            NewVoiceHeadDrawers.AddCustomizationDrawers(__instance, currentProfile);
        }
    }
}
#endif

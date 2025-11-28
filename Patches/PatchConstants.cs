#if !UNITY_EDITOR
using EFT;
using System.Linq;
using System;
using System.Reflection;

namespace HeadVoiceSelector.Patches
{
    internal static class PatchConstants
    {
        private static object _clientApp;
        
        public static object ClientApp 
        { 
            get => _clientApp;
            set => _clientApp = value;
        }
        
        public static Profile BackEndSession
        {
            get
            {
                if (_clientApp == null) return null;
                
                try
                {
                    var getBackEndSessionMethod = _clientApp.GetType()
                        .GetMethods()
                        .FirstOrDefault(m => m.Name.Contains("GetClientBackEndSession") || 
                                           m.ReturnType.Name.Contains("ClientBackEndSession"));
                    
                    if (getBackEndSessionMethod == null) return null;
                    
                    var backEndSession = getBackEndSessionMethod.Invoke(_clientApp, null);
                    if (backEndSession == null) return null;
                    
                    var profileProperty = backEndSession.GetType()
                        .GetProperties()
                        .FirstOrDefault(p => p.PropertyType == typeof(Profile));
                    
                    if (profileProperty == null) return null;
                    
                    return profileProperty.GetValue(backEndSession) as Profile;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        public static Type FindType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.Contains(typeName) || t.FullName.Contains(typeName));
        }
    }
}
#endif

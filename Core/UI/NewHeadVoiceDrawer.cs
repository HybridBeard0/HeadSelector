#if !UNITY_EDITOR
using EFT.UI;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using EFT;
using SPT.Reflection.Utils;
using System.Threading.Tasks;
using System.Linq;
using Comfort.Common;
using System.Collections.Generic;
using EFT.InventoryLogic;
using HeadVoiceSelector.Utils;

namespace HeadVoiceSelector.Core.UI
{
    internal class NewVoiceHeadDrawers
    {
        private static bool customizationDrawersCloned = false;
        private static readonly List<EquipmentSlot> _hiddenSlots = new List<EquipmentSlot>
        {
            EquipmentSlot.Earpiece,
            EquipmentSlot.Eyewear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Headwear
        };
        protected static readonly CompositeDisposableClass _compositeDisposableClass = new CompositeDisposableClass();
        private static Dictionary<int, TagBank> _voices = new Dictionary<int, TagBank>();
        private static int _selectedHeadIndex;
        private static int _selectedVoiceIndex;
        public static GClass797<EquipmentSlot, PlayerBody.GClass2119> slotViews;
        private static List<KeyValuePair<string, GClass3394>> _headTemplates;
        private static List<KeyValuePair<string, GClass3397>> _voiceTemplates;
        private static GameObject _overallScreen;
        private static Profile _currentProfile;

        public static void WTTChangeHead(string id)
        {
            if (id == null)
            {
                Console.WriteLine("Error: id is null.");
                return;
            }

            var response = WebRequestUtils.Post<string>("/WTT/WTTChangeHead", id);
            if (response != null)
            {
                Console.WriteLine("HeadVoiceSelector: Change Head Route has been requested");
            }
        }

        public static void WTTChangeVoice(string id)
        {
            if (id == null)
            {
                Console.WriteLine("Error: id is null.");
                return;
            }
#if DEBUG
            Console.WriteLine($"WTTChangeVoice: id = {id}");
#endif
            var response = WebRequestUtils.Post<string>("/WTT/WTTChangeVoice", id);
            if (response != null)
            {
                Console.WriteLine("'HeadVoiceSelector': Change Voice Route has been requested");
            }
        }

        public static void AddCustomizationDrawers(OverallScreen overallScreen, Profile profile)
        {
            try
            {
                Console.WriteLine("[HeadVoiceSelector] AddCustomizationDrawers called");
                
                // Store the profile
                _currentProfile = profile;
                Console.WriteLine($"[HeadVoiceSelector] Profile: {(profile != null ? profile.Nickname : "NULL")}");
                
                if (customizationDrawersCloned)
                {
                    Console.WriteLine("[HeadVoiceSelector] Customization drawers already cloned.");
                    return;
                }
                
                GameObject overallScreenGameobject = overallScreen.gameObject;
                _overallScreen = overallScreenGameobject;
                
                Console.WriteLine($"[HeadVoiceSelector] OverallScreen found: {overallScreenGameobject.name}");
                
                Transform leftSide = overallScreenGameobject.transform.Find("LeftSide");
                Console.WriteLine($"[HeadVoiceSelector] LeftSide found: {(leftSide != null ? leftSide.name : "NULL")}");
                
                if (leftSide == null)
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: LeftSide not found. Searching for alternatives...");
                    // List all children to help debug
                    for (int i = 0; i < overallScreenGameobject.transform.childCount; i++)
                    {
                        var child = overallScreenGameobject.transform.GetChild(i);
                        Console.WriteLine($"[HeadVoiceSelector] Child {i}: {child.name}");
                    }
                    return;
                }
                
                Transform clothingPanel = leftSide.transform.Find("ClothingPanel");
                Console.WriteLine($"[HeadVoiceSelector] ClothingPanel found: {(clothingPanel != null ? clothingPanel.name : "NULL")}");
                
                if (clothingPanel == null)
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: ClothingPanel not found. Searching for alternatives...");
                    // List all children of LeftSide
                    for (int i = 0; i < leftSide.childCount; i++)
                    {
                        var child = leftSide.GetChild(i);
                        Console.WriteLine($"[HeadVoiceSelector] LeftSide Child {i}: {child.name}");
                    }
                    return;
                }

                if (clothingPanel != null && leftSide != null)
                {
                    Console.WriteLine("[HeadVoiceSelector] Cloning ClothingPanel...");
                    GameObject clonedCustomizationDrawers = GameObject.Instantiate(clothingPanel.gameObject, leftSide);
                    clonedCustomizationDrawers.gameObject.name = "NewHeadVoiceCustomizationDrawers";

                    Vector3 newPosition = clonedCustomizationDrawers.transform.localPosition;
                    newPosition.y -= 50f;
                    clonedCustomizationDrawers.transform.localPosition = newPosition;

                    customizationDrawersCloned = true;
                    Console.WriteLine("[HeadVoiceSelector] ClothingPanel cloned successfully");

                    if (clonedCustomizationDrawers != null)
                    {
                        Transform headTransform = clonedCustomizationDrawers.transform.Find("Upper");
                        Transform voiceTransform = clonedCustomizationDrawers.transform.Find("Lower");
                        
                        Console.WriteLine($"[HeadVoiceSelector] Upper found: {(headTransform != null ? "YES" : "NO")}");
                        Console.WriteLine($"[HeadVoiceSelector] Lower found: {(voiceTransform != null ? "YES" : "NO")}");
                        
                        if (headTransform != null)
                        {
                            headTransform.gameObject.name = "Head";
                            
                            // Disable the Voice panel
                            if (voiceTransform != null)
                            {
                                voiceTransform.gameObject.SetActive(false);
                                Console.WriteLine("[HeadVoiceSelector] Voice panel disabled");
                            }

                            Transform headIconTransform = clonedCustomizationDrawers.transform.Find("Head/Icon");

                            if (headIconTransform != null)
                            {
                                Image headIcon = headIconTransform.GetComponent<Image>();

                                var headIconPng = Path.Combine(HeadVoiceSelector.pluginPath, "WTT-HeadVoiceSelector", "Icons", "icon_face_selector.png");

                                if (headIconPng != null)
                                {
                                    byte[] headIconByte = File.ReadAllBytes(headIconPng);

                                    Texture2D headIcontexture = new Texture2D(2, 2);

                                    ImageConversion.LoadImage(headIcontexture, headIconByte);

                                    Sprite headIconSprite = Sprite.Create(headIcontexture, new Rect(0, 0, headIcontexture.width, headIcontexture.height), Vector2.zero);

                                    headIcon.sprite = headIconSprite;
                                    
                                    Console.WriteLine("[HeadVoiceSelector] Head icon loaded successfully");
                                }
                                else
                                {
                                    Console.WriteLine("[HeadVoiceSelector] ERROR: Couldn't find icon for head customization dropdown");
                                }
                            }

                            Transform headSelectorTransform = clonedCustomizationDrawers.transform.Find("Head/ClothingSelector");
                            
                            Console.WriteLine($"[HeadVoiceSelector] HeadSelector found: {(headSelectorTransform != null ? "YES" : "NO")}");
                            
                            if (headSelectorTransform != null)
                            {
                                headSelectorTransform.gameObject.name = "HeadSelector";

                                DropDownBox headDropDownBox = headSelectorTransform.GetComponent<DropDownBox>();

                                if (headDropDownBox != null)
                                {
                                    Console.WriteLine("[HeadVoiceSelector] Initializing head dropdown...");
                                    InitCustomizationDropdowns(headDropDownBox, null);
                                    setupCustomizationDrawers(headDropDownBox, null);

                                    clonedCustomizationDrawers.gameObject.SetActive(true);

                                    Console.WriteLine("[HeadVoiceSelector] Successfully cloned and setup head customization dropdown!");
                                }
                                else
                                {
                                    Console.WriteLine("[HeadVoiceSelector] ERROR: headDropdownBox is null");
                                }
                            }
                            else
                            {
                                Console.WriteLine("[HeadVoiceSelector] ERROR: headSelectorTransform is null");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[HeadVoiceSelector] ERROR: headTransform is null");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[HeadVoiceSelector] ERROR: clonedCustomizationDrawers is null");
                    }
                }
                else
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: customizationDrawersPrefab or overallParent not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HeadVoiceSelector] An error occurred: {ex.Message}");
                Console.WriteLine($"[HeadVoiceSelector] Stack trace: {ex.StackTrace}");
            }
        }
        
        public static void InitCustomizationDropdowns(DropDownBox _headSelector, DropDownBox _voiceSelector)
        {
            try
            {
                _compositeDisposableClass.Dispose();

                if (_headSelector != null)
                {
                    _compositeDisposableClass.SubscribeEvent<int>(_headSelector.OnValueChanged, new Action<int>(selectHeadEvent));
                }
                
                // Voice selector is now optional
                if (_voiceSelector != null)
                {
                    _compositeDisposableClass.SubscribeEvent<int>(_voiceSelector.OnValueChanged, new Action<int>(selectVoiceEvent));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
        }
        
        public static void setupCustomizationDrawers(DropDownBox _headSelector, DropDownBox _voiceSelector)
        {
            try
            {
                CustomizationSolverClass instance = Singleton<CustomizationSolverClass>.Instance;

                if (instance == null)
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: CustomizationSolverClass instance is null.");
                    return;
                }

                if (_currentProfile == null)
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: Current profile is null.");
                    return;
                }

                Console.WriteLine($"[HeadVoiceSelector] Getting available heads for side: {_currentProfile.Side}");
                _headTemplates = instance.GetAvailableHeads(_currentProfile.Side)
                    .Select((h) => new KeyValuePair<string, GClass3394>(h.Id, h)).ToList();

                Console.WriteLine($"[HeadVoiceSelector] Added {_headTemplates.Count} head customization templates.");
                
                // Only setup voice templates if voice selector is provided
                if (_voiceSelector != null)
                {
                    Console.WriteLine($"[HeadVoiceSelector] Getting available voices for side: {_currentProfile.Side}");
                    _voiceTemplates = instance.GetAvailableVoices(_currentProfile.Side)
                        .Select((h) => new KeyValuePair<string, GClass3397>(h.Id, h)).ToList();

                    Console.WriteLine($"[HeadVoiceSelector] Added {_voiceTemplates.Count} voice customization templates.");
                    
                    // Log all voice templates
                    foreach (var voice in _voiceTemplates)
                    {
                        Console.WriteLine($"[HeadVoiceSelector] Voice: ID={voice.Key}, Name={voice.Value.Name}");
                    }
                    
                    _voices.Clear();
                }

                if (_headSelector != null)
                {
                    setupHeadDropdownInfo(_headSelector);
                }
                else
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: Head dropdown is null.");
                }

                if (_voiceSelector != null)
                {
                    setupVoiceDropdownInfo(_voiceSelector);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HeadVoiceSelector] ERROR in customization drawers setup: {ex.Message}");
                Console.WriteLine($"[HeadVoiceSelector] Stack trace: {ex.StackTrace}");
            }
        }
        
        public static void setupVoiceDropdownInfo(DropDownBox _voiceSelector)
        {
            try
            {
                if (_currentProfile == null)
                {
                    Console.WriteLine("[HeadVoiceSelector] ERROR: Current profile is null in voice dropdown setup.");
                    return;
                }

                Console.WriteLine($"[HeadVoiceSelector] Current profile voice: {_currentProfile.Info.Voice}");
                string currentVoice = _currentProfile.Info.Voice;

                int selectedIndex = _voiceTemplates.FindIndex(v => v.Value.Id == currentVoice);

                if (selectedIndex == -1)
                {
                    Console.WriteLine($"[HeadVoiceSelector] WARNING: Current voice '{currentVoice}' not found in the voice templates. Using first voice.");
                    selectedIndex = 0;
                }
                else
                {
                    Console.WriteLine($"[HeadVoiceSelector] Found current voice at index: {selectedIndex}");
                }

                Console.WriteLine($"[HeadVoiceSelector] Initializing voice dropdown with {_voiceTemplates.Count} voices");
                _voiceSelector.Show(new Func<IEnumerable<string>>(initializeVoiceDropdown), null);
                _voiceSelector.UpdateValue(selectedIndex, false, null, null);

                _currentProfile.Info.Voice = _voiceTemplates[selectedIndex].Value.Id;
                Console.WriteLine($"[HeadVoiceSelector] Voice dropdown initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HeadVoiceSelector] ERROR in voice dropdown info setup: {ex.Message}");
                Console.WriteLine($"[HeadVoiceSelector] Stack trace: {ex.StackTrace}");
            }
        }

        public static void setupHeadDropdownInfo(DropDownBox _headSelector)
        {
            try
            {
                if (_currentProfile == null)
                {
                    Console.WriteLine("Current profile is null.");
                    return;
                }

                string text = _currentProfile.Customization[EBodyModelPart.Head];

                int num = 0;
                while (num < _headTemplates.Count && !(_headTemplates[num].Key == text))
                {
                    num++;
                }

                _selectedHeadIndex = num;

                _headSelector.Show(new Func<IEnumerable<string>>(initializeHeadDropdown), null);
                _headSelector.UpdateValue(_selectedHeadIndex, false, null, null);

                _currentProfile.Customization[EBodyModelPart.Head] = _headTemplates[_selectedHeadIndex].Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during head dropdown info setup: {ex.Message}");
            }
        }
        
        public static IEnumerable<string> initializeHeadDropdown()
        {
            return _headTemplates.Select(new Func<KeyValuePair<string, GClass3394>, string>(getLocalizedHead)).ToArray<string>();
        }
        
        public static IEnumerable<string> initializeVoiceDropdown()
        {
            return _voiceTemplates.Select(new Func<KeyValuePair<string, GClass3397>, string>(getLocalizedVoice)).ToArray<string>();
        }
        
        public static string getLocalizedHead(KeyValuePair<string, GClass3394> x)
        {
#if DEBUG
            Console.WriteLine($"Localizing head: {x.Key}");
#endif
            return x.Value.NameLocalizationKey.Localized(null);
        }
        
        public static string getLocalizedVoice(KeyValuePair<string, GClass3397> x)
        {
#if DEBUG
            Console.WriteLine($"Localizing voice: {x.Key}");
#endif
            return x.Value.NameLocalizationKey.Localized(null);
        }
        
        public static void selectHeadEvent(int selectedIndex)
        {
            try
            {
#if DEBUG
                Console.WriteLine($"Selecting head event for index: {selectedIndex}");
#endif
                if (selectedIndex == _selectedHeadIndex)
                {
#if DEBUG
                    Console.WriteLine("Selected head index is already set.");
#endif
                    return;
                }

                _selectedHeadIndex = selectedIndex;
                string key = _headTemplates[_selectedHeadIndex].Key;
                
                if (_currentProfile == null)
                {
                    Console.WriteLine("Current profile is null.");
                    return;
                }
                
                _currentProfile.Customization[EBodyModelPart.Head] = key;
#if DEBUG
                Console.WriteLine($"Head customization updated to: {key}");
#endif
                showPlayerPreview().HandleExceptions();

                WTTChangeHead(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select head event: {ex.Message}");
            }
        }
        
        public static async Task showPlayerPreview()
        {
            try
            {
                Transform leftSide = _overallScreen.transform.Find("LeftSide");
                Transform characterPanel = leftSide.transform.Find("CharacterPanel");
                PlayerModelView playerModelViewScript = characterPanel.GetComponentInChildren<PlayerModelView>();

                if (leftSide != null)
                {
                    InventoryPlayerModelWithStatsWindow inventoryPlayerModelWithStatsWindow = leftSide.GetComponent<InventoryPlayerModelWithStatsWindow>();

                    if (inventoryPlayerModelWithStatsWindow != null)
                    {
                        if (_currentProfile == null)
                        {
                            Console.WriteLine("Current profile is null.");
                            return;
                        }
                        
                        await playerModelViewScript.Show(_currentProfile, null, new Action(inventoryPlayerModelWithStatsWindow.method_5), 0f, null, true);

                        changeSelectedHead(false, playerModelViewScript);
                    }
                    else
                    {
                        Console.WriteLine("InventoryPlayerModelWithStatsWindow component not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Overall screen parent not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during player preview: {ex.Message}");
            }
        }
        
        public static void changeSelectedHead(bool active, PlayerModelView playerModelView)
        {
            try
            {
                slotViews = playerModelView.PlayerBody.SlotViews;
                foreach (GameObject gameObject in _hiddenSlots.Where(new Func<EquipmentSlot, bool>(getSlotType)).Select(new Func<EquipmentSlot, GameObject>(getSlotKey)).Where(new Func<GameObject, bool>(getModel)))
                {
                    gameObject.SetActive(active);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during change selected head: {ex.Message}");
            }
        }
        
        public static bool getSlotType(EquipmentSlot slotType)
        {
            return slotViews.ContainsKey(slotType);
        }
        
        public static GameObject getSlotKey(EquipmentSlot slotType)
        {
            return slotViews.GetByKey(slotType).ParentedModel.Value;
        }
        
        public static bool getModel(GameObject model)
        {
            return model != null;
        }
        
        public static void selectVoiceEvent(int selectedIndex)
        {
            try
            {
                _selectedVoiceIndex = selectedIndex;
                selectVoice(selectedIndex).HandleExceptions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select voice event: {ex.Message}");
            }
        }
        
        public static async Task selectVoice(int selectedIndex)
        {
            try
            {
                TagBank tagBank;
                if (!_voices.TryGetValue(selectedIndex, out tagBank))
                {
                    // Use reflection to access the voice system since GClass868 might not be accessible
                    var voiceSystemType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.Name == "GClass868");
                    
                    if (voiceSystemType == null)
                    {
                        Console.WriteLine("Voice system class not found");
                        return;
                    }
                    
                    var singletonType = typeof(Singleton<>).MakeGenericType(voiceSystemType);
                    var instanceProp = singletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var voiceSystemInstance = instanceProp?.GetValue(null);
                    
                    if (voiceSystemInstance == null)
                    {
                        Console.WriteLine("Voice system instance is null");
                        return;
                    }
                    
                    var takeVoiceMethod = voiceSystemType.GetMethod("TakeVoice", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (takeVoiceMethod == null)
                    {
                        Console.WriteLine("TakeVoice method not found");
                        return;
                    }
                    
                    var task = (Task<TagBank>)takeVoiceMethod.Invoke(voiceSystemInstance, new object[] { _voiceTemplates[_selectedVoiceIndex].Value.Name, EPhraseTrigger.OnMutter });
                    TagBank result = await task;
                    
                    _voices.Add(selectedIndex, result);
                    if (result == null)
                    {
                        Console.WriteLine($"Voice not available for index: {selectedIndex}");
                        return;
                    }
                }
                string key = _voiceTemplates[_selectedVoiceIndex].Value.Id;

                if (_currentProfile == null)
                {
                    Console.WriteLine("Current profile is null.");
                    return;
                }
                
                _currentProfile.Info.Voice = key;

                int num = global::UnityEngine.Random.Range(0, _voices[selectedIndex].Clips.Length);
                TaggedClip taggedClip = _voices[selectedIndex].Clips[num];
#pragma warning disable CS4014
                Singleton<GUISounds>.Instance.ForcePlaySound(taggedClip.Clip);
#pragma warning restore CS4014

                WTTChangeVoice(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select voice: {ex.Message}");
            }
        }
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using TranslationCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace DspFontPatcher
{
    [BepInPlugin("org.kremnev8.plugins.dspfontpatcher", "DSP Font Patcher", "0.1.0.1")]
    public class DspFontPatcher : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        public static Font newFont;
        
        private ConfigEntry<string> configFontName;
        private static ConfigEntry<string> configTargetLanguage;
        private static ConfigEntry<bool> configDebugLogging;

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            logger = Logger;
            
            configFontName = Config.Bind("General",   
                "FontName",  
                "Arial", 
                "Font to be used, can be any registered in system font"); 
            
            configDebugLogging = Config.Bind("General",   
                "OutputDebug",  
                false, 
                "If set to true, plugin will output some debug info to console"); 
            
            configTargetLanguage = Config.Bind("General", 
                "TargetLanguage",  
                "Russian",
                "language for which we need to change fonts"); 

            newFont = Font.CreateDynamicFontFromOSFont(configFontName.Value, 16);
            
           // newFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            logger.LogInfo("DSP Font Patcher is initialized!");
            
            String currentLang = PlayerPrefs.GetString(TranslationManager.PlayerPrefsCode);
            logger.LogInfo("Selected language: " + currentLang);
            logger.LogInfo("Selected font: " + configFontName.Value);

            if (currentLang.Equals(configTargetLanguage.Value))
            {
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

                SceneManager.sceneLoaded += LevelLoaded;
            }
        }

        public static void logDebug(String output)
        {
            if (configDebugLogging.Value)
            {
                logger.LogDebug(output);
            }
        }

        private void LevelLoaded(Scene scene, LoadSceneMode mode)
        {
            logDebug("Level loaded");
            Invoke(nameof(FixLater), 3);
        }

        private void FixLater()
        {
            Text[] allFields = FindObjectsOfType<Text>();
            foreach (Text field in allFields)
            {
                field.font = newFont;
            }
        }
        
    }
    
    [HarmonyPatch(typeof(Localizer), "Refresh")]
    static class LocalizerPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___text)
        {
            if (___text != null)
            {
                ___text.font = DspFontPatcher.newFont;
            }
        }
    }
    
    [HarmonyPatch(typeof(UIItemTip), "SetTip")]
    static class ItemTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___nameText, Text ___categoryText, Text ___descText, Text ___propsText,
            Text ___valuesText, Text ___preTechText)
        {
            DspFontPatcher.logDebug("Patching item tip");
            ___nameText.font = DspFontPatcher.newFont;
            ___categoryText.font = DspFontPatcher.newFont;
            ___descText.font = DspFontPatcher.newFont;
            ___propsText.font = DspFontPatcher.newFont;
            ___valuesText.font = DspFontPatcher.newFont;
            ___preTechText.font = DspFontPatcher.newFont;
        }
    }

    [HarmonyPatch(typeof(UIRealtimeTip), "SetText")]
    static class UiTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___textComp)
        {
            DspFontPatcher.logDebug("Patching UIRealtimeTip");
            ___textComp.font = DspFontPatcher.newFont;
        }
    }

    [HarmonyPatch(typeof(UIButtonTip), "SetTip")]
    static class UiButtonTipPatch
    {
        [HarmonyPrefix]
        static bool Postfix(Text ___titleComp, Text ___subTextComp)
        {
            DspFontPatcher.logDebug("Patching UIButtonTip");
            ___titleComp.font = DspFontPatcher.newFont;
            ___titleComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            ___subTextComp.font = DspFontPatcher.newFont;
            return true;
        }
    }

    [HarmonyPatch(typeof(UIKeyTipNode), nameof(UIKeyTipNode.SetKeyTip))]
    [HarmonyPatch(typeof(UIKeyTipNode), nameof(UIKeyTipNode.SetMouseTip))]
    [HarmonyPatch(typeof(UIKeyTipNode), nameof(UIKeyTipNode.SetCombineKeyTip))]
    [HarmonyPatch(typeof(UIKeyTipNode), nameof(UIKeyTipNode.SetCombineKeyMouseTip))]
    static class UIKeyTipNodePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___tipTextComp)
        {
            DspFontPatcher.logDebug("Patching UIButtonTip");
            ___tipTextComp.font = DspFontPatcher.newFont;
        }
    }

    [HarmonyPatch(typeof(UIEscMenu), "_OnOpen")]
    static class EscMenuPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___button2Text, Text ___stateText)
        {
            DspFontPatcher.logDebug("Patching UIButtonTip");
            ___button2Text.font = DspFontPatcher.newFont;
            ___stateText.font = DspFontPatcher.newFont;
        }
    }

    [HarmonyPatch(typeof(UIComboBox), "Start")]
    static class ComboBoxPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___m_Text, List<Button> ___ItemButtons)
        {
            DspFontPatcher.logDebug("Patching UIComboBox");

            foreach (Button button in ___ItemButtons)
            {
                button.GetComponentInChildren<Text>().font = DspFontPatcher.newFont;
            }

            ___m_Text.font = DspFontPatcher.newFont;
        }
    }
    
    [HarmonyPatch(typeof(UIButton), "Init")]
    static class ButtonPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIButton __instance)
        {
            Text text = __instance.GetComponentInChildren<Text>();
            if (text != null)
            {
                DspFontPatcher.logDebug("Patching Button");
                text.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIProductEntry), "OnEntryCreated")]
    static class ProductEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIProductEntry __instance, Text ___consumeLabel, Text ___productText, Text ___consumeText, Text ___chargeCapacityText, Text ___energyConsumptionText)
        {
            DspFontPatcher.logDebug("Patching UIProductEntry");

            ___consumeLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            
            ___productText.horizontalOverflow = HorizontalWrapMode.Overflow;
            ___consumeText.horizontalOverflow = HorizontalWrapMode.Overflow;
            
            if (___chargeCapacityText != null) ___chargeCapacityText.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (___energyConsumptionText != null) ___energyConsumptionText.horizontalOverflow = HorizontalWrapMode.Overflow;

            Text[] allFields = __instance.GetComponentsInChildren<Text>();
            foreach (Text field in allFields)
            {
                field.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(ManualBehaviour), "_Init")]
    static class ManualBehaviourPatch
    {
        [HarmonyPrefix]
        static bool Postfix(UIProductEntry __instance)
        {
            DspFontPatcher.logDebug("Patching ManualBehaviour");

            Text[] allFields = __instance.GetComponentsInChildren<Text>();
            foreach (Text field in allFields)
            {
                field.font = DspFontPatcher.newFont;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UIStationStorage), "_OnOpen")]
    static class UIStationStoragePatch
    {
        [HarmonyPostfix]
        static void Postfix(UIStationStorage __instance, Text ___optionText0, Text ___optionText1, Text ___optionText2)
        {
            DspFontPatcher.logDebug("Patching UIStationStorage");

            Text[] allFields = __instance.GetComponentsInChildren<Text>();
            foreach (Text field in allFields)
            {
                field.font = DspFontPatcher.newFont;
            }

            ___optionText0.font = DspFontPatcher.newFont;
            ___optionText1.font = DspFontPatcher.newFont;
            ___optionText2.font = DspFontPatcher.newFont;

        }
    }

    [HarmonyPatch(typeof(UIResAmountEntry), "SetInfo")]
    static class UIResAmountEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___labelText, Text ___valueText)
        {
            DspFontPatcher.logDebug("Patching UIResAmountEntry");

            ___labelText.font = DspFontPatcher.newFont;
            ___valueText.font = DspFontPatcher.newFont;

        }
    }
    
    [HarmonyPatch(typeof(UIAssemblerWindow), "_OnOpen")]
    static class UIAssemblerWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___speedText)
        {
            DspFontPatcher.logDebug("Patching UIAssemblerWindow");

            ___speedText.font = DspFontPatcher.newFont;

        }
    }
    
    [HarmonyPatch(typeof(UIPowerGeneratorWindow), "_OnOpen")]
    static class UIPowerGeneratorWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___fuelText2, Text ___cataSpeedText)
        {
            DspFontPatcher.logDebug("Patching UIPowerGeneratorWindow");

            ___fuelText2.font = DspFontPatcher.newFont;
            ___cataSpeedText.font = DspFontPatcher.newFont;

        }
    }
    
    [HarmonyPatch(typeof(UIStarmap), "_OnOpen")]
    static class UIStarmapPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___cursorViewText)
        {
            DspFontPatcher.logDebug("Patching UIStarmap");

            ___cursorViewText.font = DspFontPatcher.newFont;

        }
    }
    
    [HarmonyPatch(typeof(UIPlanetGlobe), "_OnOpen")]
    static class UIPlanetGlobePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___geoInfoText)
        {
            DspFontPatcher.logDebug("Patching UIStarmap");

            ___geoInfoText.font = DspFontPatcher.newFont;
            //___positionText3.font = DspFontPatcher.newFont;

        }
    }
    
    [HarmonyPatch(typeof(UITechNode), "_OnOpen")]
    static class UITechNodePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___progressSpeedText, Text ___techDescText, Text ___unlockText, RectTransform ___unlockGroup)
        {
            DspFontPatcher.logDebug("Patching UITechNode");

            Transform trs = ___unlockGroup.Find("unlock-label");
            if (trs != null)
            {
                RectTransform rtrs = (RectTransform) trs;
                rtrs.anchoredPosition = new Vector2(-5,10);
            }

            ___progressSpeedText.font = DspFontPatcher.newFont;
            ___techDescText.font = DspFontPatcher.newFont;
            ___unlockText.font = DspFontPatcher.newFont;

        }
    }
    [HarmonyPatch(typeof(UIMechaEnergy), "_OnCreate")]
    static class UIMechaEnergyPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___energyText)
        {
            DspFontPatcher.logDebug("Patching UIMechaEnergy");

            ___energyText.horizontalOverflow = HorizontalWrapMode.Overflow;

        }
    }
    
    [HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
    static class UIDysonPanelPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___addTitleText, Text ___nameText, UIButton ___pauseButton, Text ___modeToolText, Text ___gridToolText)
        {
            DspFontPatcher.logDebug("Patching UIDysonPanel");

            ___addTitleText.font = DspFontPatcher.newFont;
            ___nameText.font = DspFontPatcher.newFont;
            ___modeToolText.font = DspFontPatcher.newFont;
            ___gridToolText.font = DspFontPatcher.newFont;
            
            Text pauseText = ___pauseButton.GetComponentInChildren<Text>();
            if (pauseText != null)
            {
                float width = pauseText.preferredWidth + 70;
                RectTransform trs = (RectTransform) ___pauseButton.button.transform;
                trs.offsetMin = new Vector2(-width+trs.offsetMax.x, trs.offsetMin.y);
            }
        }
    }
    
    [HarmonyPatch(typeof(UIKeyEntry), "SetEntry")]
    static class UIOptionWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___functionText, Text ___keyText)
        {
            DspFontPatcher.logDebug("Patching UIKeyEntry");

            ___functionText.font = DspFontPatcher.newFont;
            ___keyText.font = DspFontPatcher.newFont;
        }
    }

    [HarmonyPatch(typeof(UIAutoSave), "_OnOpen")]
    static class UIAutoSavePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___saveText)
        {
            DspFontPatcher.logDebug("Patching UIAutoSave");

            ___saveText.font = DspFontPatcher.newFont;
        }
    }
    
    [HarmonyPatch(typeof(UIHandTip), "_OnOpen")]
    static class UIHandTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___tipText)
        {
            DspFontPatcher.logDebug("Patching UIHandTip");

            ___tipText.font = DspFontPatcher.newFont;
        }
    }
    
    
/*
/* 0x000DD05F 02            IL_0337: ldarg.0
/* 0x000DD060 7B1E190004    IL_0338: ldfld     class [UnityEngine.UI]UnityEngine.UI.Text UITechNode::unlockText
/* 0x000DD065 6FD405000A    IL_033D: callvirt  instance float32 [UnityEngine.UI]UnityEngine.UI.Text::get_preferredWidth()
/* 0x000DD06A 2200002042    IL_0342: ldc.r4    40
/* 0x000DD06F 59            IL_0347: sub


/* 0x000DD06A 2200002042    IL_0342: ldc.r4    164
*/
//DOES NOT WORK, UNFORTUNATELY
   /* [HarmonyPatch(typeof(UITechNode), "UpdateLayoutDynamic")]
    public static class UITechNode_Patch
    {
        [HarmonyDebug]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher match = new CodeMatcher(instructions)
                .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UITechNode), "unlockText")),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Text), "get_preferredWidth")),
                    new CodeMatch(OpCodes.Ldc_R4, 40),
                    new CodeMatch(OpCodes.Sub)).Advance(5) // Move cursor to Sub
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldc_R4, 164)
                );
            var codes = new List<CodeInstruction>(match.InstructionEnumeration());
            DspFontPatcher.logger.LogInfo(codes);
            DspFontPatcher.logger.LogInfo(match.IsValid);
            
            return match.InstructionEnumeration();
        }
    }*/
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [BepInPlugin("org.kremnev8.plugins.dspfontpatcher", "DSP Font Patcher", "0.1.1.0")]
    public class DspFontPatcher : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        public static Font newFont;

        private ConfigEntry<string> configFontName;
        private static ConfigEntry<bool> configDebugLogging;
        public static ConfigEntry<bool> configFixFonts;
        public static ConfigEntry<bool> configDynamicSize;

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
            
            configFixFonts = Config.Bind("General",
                "FixFonts",
                false,
                "if true all fonts will be changed to specified font and dynamic sizing will be enabled too");
            
            configDynamicSize = Config.Bind("General",
                "DynamicSize",
                true,
                "if true some textboxes size will be changed to fit content");

            newFont = Font.CreateDynamicFontFromOSFont(configFontName.Value, 16);

            // newFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            logger.LogInfo("DSP Font Patcher is initialized!");

            String currentLang = PlayerPrefs.GetString(TranslationManager.PlayerPrefsCode);
            logger.LogInfo("Selected language: " + currentLang);
            logger.LogInfo("Selected font: " + configFontName.Value);

            if (configFixFonts.Value)
            {
                configDynamicSize.Value = true;
                logger.LogInfo("Font fixing is enabled!");
            }else if (configDynamicSize.Value)
            {
                logger.LogInfo("Only dynamic sizing is enabled!");
            }
            
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            SceneManager.sceneLoaded += LevelLoaded;
            
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
            Invoke(nameof(FixLater), 5);
        }

        private void FixLater()
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                Text[] allFields = FindObjectsOfType<Text>();
                foreach (Text field in allFields)
                {
                    field.font = newFont;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Localizer), "Refresh")]
    static class LocalizerPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___text)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                if (___text != null)
                {
                    ___text.font = DspFontPatcher.newFont;
                }
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
            if (DspFontPatcher.configFixFonts.Value)
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
    }

    [HarmonyPatch(typeof(UIRealtimeTip), "SetText")]
    static class UiTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___textComp)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIRealtimeTip");
                ___textComp.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIButtonTip), "SetTip")]
    static class UiButtonTipPatch
    {
        [HarmonyPrefix]
        static bool Postfix(Text ___titleComp, Text ___subTextComp)
        {
            DspFontPatcher.logDebug("Patching UIButtonTip");

            if (DspFontPatcher.configDynamicSize.Value)
                ___titleComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            if (DspFontPatcher.configFixFonts.Value)
            {
                ___titleComp.font = DspFontPatcher.newFont;
                ___subTextComp.font = DspFontPatcher.newFont;
            }

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
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIButtonTip");
                ___tipTextComp.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIEscMenu), "_OnOpen")]
    static class EscMenuPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___button2Text, Text ___stateText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIButtonTip");
                ___button2Text.font = DspFontPatcher.newFont;
                ___stateText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIComboBox), "Start")]
    static class ComboBoxPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___m_Text, List<Button> ___ItemButtons)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIComboBox");

                foreach (Button button in ___ItemButtons)
                {
                    button.GetComponentInChildren<Text>().font = DspFontPatcher.newFont;
                }

                ___m_Text.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIButton), "Init")]
    static class ButtonPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIButton __instance)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                Text text = __instance.GetComponentInChildren<Text>();
                if (text != null)
                {
                    DspFontPatcher.logDebug("Patching Button");
                    text.font = DspFontPatcher.newFont;
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIProductEntry), "OnEntryCreated")]
    static class ProductEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIProductEntry __instance, Text ___consumeLabel, Text ___productText, Text ___consumeText,
            Text ___chargeCapacityText, Text ___energyConsumptionText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIProductEntry");

                ___consumeLabel.horizontalOverflow = HorizontalWrapMode.Overflow;

                ___productText.horizontalOverflow = HorizontalWrapMode.Overflow;
                ___consumeText.horizontalOverflow = HorizontalWrapMode.Overflow;

                if (___chargeCapacityText != null)
                    ___chargeCapacityText.horizontalOverflow = HorizontalWrapMode.Overflow;
                if (___energyConsumptionText != null)
                    ___energyConsumptionText.horizontalOverflow = HorizontalWrapMode.Overflow;

                Text[] allFields = __instance.GetComponentsInChildren<Text>();
                foreach (Text field in allFields)
                {
                    field.font = DspFontPatcher.newFont;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ManualBehaviour), "_Init")]
    static class ManualBehaviourPatch
    {
        [HarmonyPrefix]
        static bool Postfix(UIProductEntry __instance)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching ManualBehaviour");

                Text[] allFields = __instance.GetComponentsInChildren<Text>();
                foreach (Text field in allFields)
                {
                    field.font = DspFontPatcher.newFont;
                }

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
            if (DspFontPatcher.configFixFonts.Value)
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
    }

    [HarmonyPatch(typeof(UIResAmountEntry), "SetInfo")]
    static class UIResAmountEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___labelText, Text ___valueText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIResAmountEntry");

                ___labelText.font = DspFontPatcher.newFont;
                ___valueText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIAssemblerWindow), "_OnOpen")]
    static class UIAssemblerWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___speedText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIAssemblerWindow");

                ___speedText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIPowerGeneratorWindow), "_OnOpen")]
    static class UIPowerGeneratorWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___fuelText2, Text ___cataSpeedText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIPowerGeneratorWindow");

                ___fuelText2.font = DspFontPatcher.newFont;
                ___cataSpeedText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIStarmap), "_OnOpen")]
    static class UIStarmapPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___cursorViewText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIStarmap");

                ___cursorViewText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIPlanetGlobe), "_OnOpen")]
    static class UIPlanetGlobePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___geoInfoText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIStarmap");

                ___geoInfoText.font = DspFontPatcher.newFont;
                //___positionText3.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UITechNode), "_OnOpen")]
    static class UITechNodePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___progressSpeedText, Text ___techDescText, Text ___unlockText,
            RectTransform ___unlockGroup)
        {
            DspFontPatcher.logDebug("Patching UITechNode");
            if (DspFontPatcher.configDynamicSize.Value)
            {
                Transform trs = ___unlockGroup.Find("unlock-label");
                if (trs != null)
                {
                    RectTransform rtrs = (RectTransform) trs;
                    rtrs.anchoredPosition = new Vector2(-5, 10);
                }
            }

            if (DspFontPatcher.configFixFonts.Value)
            {
                ___progressSpeedText.font = DspFontPatcher.newFont;
                ___techDescText.font = DspFontPatcher.newFont;
                ___unlockText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIMechaEnergy), "_OnCreate")]
    static class UIMechaEnergyPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___energyText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIMechaEnergy");

                ___energyText.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
        }
    }

    [HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
    static class UIDysonPanelPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___addTitleText, Text ___nameText, UIButton ___pauseButton, Text ___modeToolText,
            Text ___gridToolText)
        {
            DspFontPatcher.logDebug("Patching UIDysonPanel");

            if (DspFontPatcher.configFixFonts.Value)
            {
                ___addTitleText.font = DspFontPatcher.newFont;
                ___nameText.font = DspFontPatcher.newFont;
                ___modeToolText.font = DspFontPatcher.newFont;
                ___gridToolText.font = DspFontPatcher.newFont;
            }

            if (DspFontPatcher.configDynamicSize.Value)
            {
                Text pauseText = ___pauseButton.GetComponentInChildren<Text>();
                if (pauseText != null)
                {
                    float width = pauseText.preferredWidth + 70;
                    RectTransform trs = (RectTransform) ___pauseButton.button.transform;
                    trs.offsetMin = new Vector2(-width + trs.offsetMax.x, trs.offsetMin.y);
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIKeyEntry), "SetEntry")]
    static class UIOptionWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___functionText, Text ___keyText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIKeyEntry");

                ___functionText.font = DspFontPatcher.newFont;
                ___keyText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIAutoSave), "_OnOpen")]
    static class UIAutoSavePatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___saveText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIAutoSave");

                ___saveText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIHandTip), "_OnOpen")]
    static class UIHandTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___tipText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIHandTip");

                ___tipText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UITutorialWindow), "_OnOpen")]
    static class UITutorialWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___titleText, Text ___preText, Text ___postText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logger.LogInfo("Patching UITutorialWindow");

                ___titleText.font = DspFontPatcher.newFont;
                ___preText.font = DspFontPatcher.newFont;
                ___postText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UITutorialListEntry), "SetText")]
    static class UITutorialListEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___nameText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logger.LogInfo("Patching UITutorialListEntry");

                ___nameText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIGeneralTips), "OnWarningTextChanged")]
    static class UIGeneralTipsPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___warningText, Image ___warningIcon)
        {
            DspFontPatcher.logger.LogInfo("Patching UIGeneralTips");

            if (DspFontPatcher.configFixFonts.Value)
                ___warningText.font = DspFontPatcher.newFont;
            //Fix Not enough fuel warning icon position
            if (DspFontPatcher.configDynamicSize.Value)
                ___warningIcon.rectTransform.anchoredPosition = new Vector2(-160, 0);
        }
    }

    //title-text
    [HarmonyPatch(typeof(UIResearchResultWindow), "_OnOpen")]
    static class UIResearchResultWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___contentGroup)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logger.LogInfo("Patching UIResearchResultWindow");

                Transform textTrs = ___contentGroup.transform.Find("title-text");
                if (textTrs != null)
                {
                    DspFontPatcher.logger.LogInfo("Found text");
                    RectTransform rTrs = (RectTransform) textTrs;
                    Text title = textTrs.GetComponent<Text>();
                    title.font = DspFontPatcher.newFont;
                    title.horizontalOverflow = HorizontalWrapMode.Overflow;
                    // rTrs.anchoredPosition = new Vector2(0, 48);
                }
            }
        }
    }

    //speedText
    [HarmonyPatch(typeof(UILabWindow), "_OnOpen")]
    static class UILabWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___speedText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logger.LogInfo("Patching UILabWindow");

                ___speedText.font = DspFontPatcher.newFont;
            }
        }
    }

    [HarmonyPatch(typeof(UIGame), "_OnInit")]
    static class UIGamePatch
    {
        [HarmonyPostfix]
        static void Postfix(UIGame __instance)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logger.LogInfo("Patching UIGame");

                Image[] icons = __instance.GetComponentsInChildren<Image>(true);

                foreach (Image icon in icons)
                {
                    if (icon.gameObject.name.Equals("power-icon"))
                    {
                        ((RectTransform) icon.transform).anchoredPosition = new Vector2(-45, 0);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIMechaWindow), "_OnOpen")]
    static class UIMechaWindowPatch
    {
        
        [HarmonyPostfix]
        static void Postfix(UIMechaWindow __instance)
        {
            DspFontPatcher.logger.LogInfo("Patching UIMechaWindow");

            if (DspFontPatcher.configDynamicSize.Value)
            {

                try
                {
                    Transform t1 = __instance.transform.Find("mecha-group");
                    Transform infTrs = t1.Find("information");

                    List<Text> values = new List<Text>();
                    List<Image> lines = new List<Image>();
                    float maxWidth = 0;

                    foreach (Transform child in infTrs)
                    {
                        Text label = child.Find("label").GetComponent<Text>();
                        Text value = child.Find("value").GetComponent<Text>();
                        Image line = child.Find("line").GetComponent<Image>();
                        float thisWidth = label.preferredWidth + value.preferredWidth + 30;
                        if (thisWidth > maxWidth) maxWidth = thisWidth;
                        lines.Add(line);
                        values.Add(value);
                    }

                    for (int i = 0; i < values.Count; i++)
                    {
                        values[i].rectTransform.anchoredPosition = new Vector2(maxWidth - 190, -9);
                        lines[i].rectTransform.offsetMax = new Vector2(maxWidth - 180, 7);
                    }
                }
                catch (NullReferenceException)
                {
                    DspFontPatcher.logger.LogWarning("Patching UIMechaWindow failed!");
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
    static class UIRandomTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(RectTransform ___balloonTrans)
        {
            DspFontPatcher.logger.LogInfo("Patching UIRandomTip");

            if (DspFontPatcher.configDynamicSize.Value)
            {
                Text tipText = ___balloonTrans.GetComponentInChildren<Text>();
                if (tipText != null)
                {
                    ___balloonTrans.sizeDelta = new Vector2(___balloonTrans.sizeDelta.x, tipText.preferredHeight + 23);
                }
            }
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
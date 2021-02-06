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

            logger.LogInfo("DSP Font Patcher is initialized!");

            String currentLang = PlayerPrefs.GetString(TranslationManager.PlayerPrefsCode);
            logger.LogInfo("Selected language: " + currentLang);
            logger.LogInfo("Selected font: " + configFontName.Value);

            if (configFixFonts.Value)
            {
                configDynamicSize.Value = true;
                logger.LogInfo("Font fixing is enabled!");
            }
            else if (configDynamicSize.Value)
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

        //Change fonts
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
    
    //Change fonts
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
    
    //Change fonts
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
    
    //Change fonts
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

    //Change fonts and removing horizontal overflow for button tips
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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

    //Change fonts
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
    
    //Change fonts
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
            }
        }
    }

    //Change fonts and adjusting tech nodes title position
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

    //Change fonts
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

    //Change fonts and dynamic resizing of "run game" button
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
    
    //Change fonts
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

    //Change fonts
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

    //Change fonts
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
    
    //Change fonts
    [HarmonyPatch(typeof(UITutorialWindow), "_OnOpen")]
    static class UITutorialWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___titleText, Text ___preText, Text ___postText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UITutorialWindow");

                ___titleText.font = DspFontPatcher.newFont;
                ___preText.font = DspFontPatcher.newFont;
                ___postText.font = DspFontPatcher.newFont;
            }
        }
    }
    
    //Change fonts
    [HarmonyPatch(typeof(UITutorialListEntry), "SetText")]
    static class UITutorialListEntryPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___nameText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UITutorialListEntry");

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
            DspFontPatcher.logDebug("Patching UIGeneralTips");

            if (DspFontPatcher.configFixFonts.Value)
                ___warningText.font = DspFontPatcher.newFont;
            //Fix Not enough fuel warning icon position
            if (DspFontPatcher.configDynamicSize.Value)
                ___warningIcon.rectTransform.anchoredPosition = new Vector2(-160, 0);
        }
    }

    //fix research ui title position (Only needed when fonts are changed)
    [HarmonyPatch(typeof(UIResearchResultWindow), "_OnOpen")]
    static class UIResearchResultWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___contentGroup)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIResearchResultWindow");

                Transform textTrs = ___contentGroup.transform.Find("title-text");
                if (textTrs != null)
                {
                    RectTransform rTrs = (RectTransform) textTrs;
                    Text title = textTrs.GetComponent<Text>();
                    title.font = DspFontPatcher.newFont;
                    title.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
        }
    }

    //Change fonts
    [HarmonyPatch(typeof(UILabWindow), "_OnOpen")]
    static class UILabWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___speedText)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UILabWindow");

                ___speedText.font = DspFontPatcher.newFont;
            }
        }
    }

    //Fix icon position (Only needed when fonts are changed)
    [HarmonyPatch(typeof(UIGame), "_OnInit")]
    static class UIGamePatch
    {
        [HarmonyPostfix]
        static void Postfix(UIGame __instance)
        {
            if (DspFontPatcher.configFixFonts.Value)
            {
                DspFontPatcher.logDebug("Patching UIGame");

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

    //Mecha dynamic UI
    [HarmonyPatch(typeof(UIMechaWindow), "_OnOpen")]
    static class UIMechaWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIMechaWindow __instance)
        {
            DspFontPatcher.logDebug("Patching UIMechaWindow");

            if (DspFontPatcher.configDynamicSize.Value)
            {
                try
                {
                    Transform t1 = __instance.transform.Find("mecha-group");
                    Transform infTrs = t1.Find("information");

                    List<Text> values = new List<Text>();
                    List<Image> lines = new List<Image>();
                    float maxWidth = 140;

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
                    //Cap max extension
                    maxWidth = Math.Min(maxWidth, 390);

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

    //Random tip dynamic resize
    [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
    static class UIRandomTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(RectTransform ___balloonTrans)
        {
            DspFontPatcher.logDebug("Patching UIRandomTip");

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
    
    //Tech tree node bugfix
    [HarmonyPatch(typeof(UITechNode), "UpdateLayoutDynamic")]
    public static class UITechNodePatch2
    {
        //Fixes tech nodes being oversized when unlockText is long. (Game devs seem to put \n in unlockText to fix this) 
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            if (DspFontPatcher.configDynamicSize.Value)
            {
                DspFontPatcher.logDebug("Patching UITechNode");
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt &&
                        ((MethodInfo) codes[i].operand).Name == "get_preferredWidth")
                    {
                        codes[i] = Transpilers.EmitDelegate<Func<Text, float>>(text =>
                            text.text.Equals("") ? 0f : 164f);
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 1].operand = null;
                        codes[i + 2].opcode = OpCodes.Nop;
                        codes[i + 2].operand = null;
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }
    
    //tech tree info panel dynamic resizing
    [HarmonyPatch(typeof(UITechTree), "OnPageChanged")]
    static class UITechTreePatch
    {
        [HarmonyPostfix]
        static void Postfix(RectTransform ___dataValueGroup)
        {
            DspFontPatcher.logDebug("Patching UITechTree");

            if (DspFontPatcher.configDynamicSize.Value)
            {
                Text label = ___dataValueGroup.Find("label").GetComponent<Text>();
                Text value = ___dataValueGroup.Find("value").GetComponent<Text>();
                float thisWidth = label.preferredWidth + value.preferredWidth + 70f;
                ___dataValueGroup.offsetMin = new Vector2(-thisWidth, 0);
            }
        }
    }
    
    //Player inventory bottom tip dynamic resizing
    [HarmonyPatch(typeof(UIStorageGrid), "_OnOpen")]
    static class UIStorageGridPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIStorageGrid __instance)
        {
            if (DspFontPatcher.configDynamicSize.Value)
            {
                if (__instance.name.ToLower().Contains("player"))
                {
                    DspFontPatcher.logDebug("Patching Player inventory");
                    
                    RectTransform trs = (RectTransform)__instance.transform.Find("panel-bg");
                    Text tip = trs.Find("tip-text").GetComponent<Text>();
                    tip.verticalOverflow = VerticalWrapMode.Overflow;
                    trs.offsetMin = new Vector2(-42, -31 - tip.preferredHeight);
                }
            }
        }
    }
}
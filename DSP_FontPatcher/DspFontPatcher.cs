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
    [BepInPlugin("org.kremnev8.plugins.dspfontpatcher", "DSP Font Patcher", "0.2.0")]
    public class DspFontPatcher : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        private static ConfigEntry<bool> configDynamicSize;

        void Awake()
        {
            logger = Logger;

            configDynamicSize = Config.Bind("General",
                "DynamicSize",
                true,
                "if true some textboxes size will be changed to fit content");

            logger.LogInfo("DSP Font Patcher is initialized!");

            if (configDynamicSize.Value)
            {
                logger.LogInfo("Only dynamic sizing is enabled!");
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                logger.LogInfo("Dynamic resizing is disabled.");
            }
        }

        public static void logDebug(String output)
        {
            logger.LogDebug(output);
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

            ___titleComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            return true;
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
            DspFontPatcher.logDebug("Patching UIProductEntry");

            ___consumeLabel.horizontalOverflow = HorizontalWrapMode.Overflow;

            ___productText.horizontalOverflow = HorizontalWrapMode.Overflow;
            ___consumeText.horizontalOverflow = HorizontalWrapMode.Overflow;

            if (___chargeCapacityText != null)
                ___chargeCapacityText.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (___energyConsumptionText != null)
                ___energyConsumptionText.horizontalOverflow = HorizontalWrapMode.Overflow;
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

            Transform trs = ___unlockGroup.Find("unlock-label");
            if (trs != null)
            {
                RectTransform rtrs = (RectTransform) trs;
                rtrs.anchoredPosition = new Vector2(-5, 10);
            }
        }
    }

    //Fix overflow
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

    //Change dynamic resizing of "run game" button
    [HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
    static class UIDysonPanelPatch
    {
        [HarmonyPostfix]
        static void Postfix(Text ___addTitleText, Text ___nameText, UIButton ___pauseButton, Text ___modeToolText,
            Text ___gridToolText)
        {
            DspFontPatcher.logDebug("Patching UIDysonPanel");

            Text pauseText = ___pauseButton.GetComponentInChildren<Text>();
            if (pauseText != null)
            {
                float width = pauseText.preferredWidth + 70;
                RectTransform trs = (RectTransform) ___pauseButton.button.transform;
                trs.offsetMin = new Vector2(-width + trs.offsetMax.x, trs.offsetMin.y);
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

            //Fix Not enough fuel warning icon position
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
            DspFontPatcher.logDebug("Patching UIResearchResultWindow");

            Transform textTrs = ___contentGroup.transform.Find("title-text");
            if (textTrs != null)
            {
                RectTransform rTrs = (RectTransform) textTrs;
                Text title = textTrs.GetComponent<Text>();
                title.horizontalOverflow = HorizontalWrapMode.Overflow;
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

    //Mecha dynamic UI
    [HarmonyPatch(typeof(UIMechaWindow), "_OnOpen")]
    static class UIMechaWindowPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIMechaWindow __instance)
        {
            DspFontPatcher.logDebug("Patching UIMechaWindow");

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

    //Random tip dynamic resize
    [HarmonyPatch(typeof(UIRandomTip), "_OnOpen")]
    static class UIRandomTipPatch
    {
        [HarmonyPostfix]
        static void Postfix(RectTransform ___balloonTrans)
        {
            DspFontPatcher.logDebug("Patching UIRandomTip");

            Text tipText = ___balloonTrans.GetComponentInChildren<Text>();
            if (tipText != null)
            {
                ___balloonTrans.sizeDelta = new Vector2(___balloonTrans.sizeDelta.x, tipText.preferredHeight + 23);
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

            Text label = ___dataValueGroup.Find("label").GetComponent<Text>();
            Text value = ___dataValueGroup.Find("value").GetComponent<Text>();
            float thisWidth = label.preferredWidth + value.preferredWidth + 70f;
            ___dataValueGroup.offsetMin = new Vector2(-thisWidth, 0);
        }
    }

    //Player inventory bottom tip dynamic resizing
    [HarmonyPatch(typeof(UIStorageGrid), "_OnOpen")]
    static class UIStorageGridPatch
    {
        [HarmonyPostfix]
        static void Postfix(UIStorageGrid __instance)
        {
            DspFontPatcher.logDebug("Patching Player inventory");

            Transform trs = __instance.transform.Find("panel-bg");
            if (trs != null)
            {
                Text tip = trs.Find("tip-text").GetComponent<Text>();
                tip.verticalOverflow = VerticalWrapMode.Overflow;
                ((RectTransform) trs).offsetMin = new Vector2(-42, -31 - tip.preferredHeight);
            }
        }
    }
}
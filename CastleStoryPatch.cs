using Brix.Engine;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.UI.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


namespace Doorstop
{
    class Entrypoint
    {
        public static void Start()
        {
            new Thread(() =>
            {
                Thread.Sleep(3000);

                var harmony = new Harmony("CastleStoryPatch");

                harmony.PatchAll();
            }).Start();
        }
    }
}

namespace CastleStoryPatch
{
    [HarmonyPatch(typeof(CanvasScalerDriver))]
    public class CanvasScalerDriver_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnSetSettingIntoGame")]
        public static bool OnSetSettingIntoGame(CanvasScalerDriver __instance)
        {
            var canvasScalers = Traverse.Create(__instance).Field("CanvasScalers").GetValue<List<CanvasScaler>>();
            foreach (CanvasScaler canvasScaler in canvasScalers)
            {
                if (Neo.canvasScale == 1.0f)
                {
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                } 
                else
                {
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    canvasScaler.scaleFactor = Neo.canvasScale;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ObjectPoolSingleton))]
    public class ObjectPoolSingleton_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ResetTransform")]
        public static void ResetTransform(GameObject go)
        {
            go.transform.localScale = Vector3.one;
        }
    }

    [HarmonyPatch(typeof(SelectionBox))]
    public class SelectionBox_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RedrawSelectionBox")]
        public static void RedrawSelectionBox(SelectionBox __instance)
        {
            if (Neo.canvasScale != 1.0f)
            {
                return;
            }

            var transform = Traverse.Create(__instance).Field("_selectionBox").GetValue<RectTransform>();
            if (transform != null) {
                var localScale = transform.localScale;
                localScale.x = 1f / transform.lossyScale.x;
                localScale.y = 1f / transform.lossyScale.y;
                localScale.z = 1f / transform.lossyScale.z;
                transform.localScale = localScale;
            }
        }
    }

    [HarmonyPatch(typeof(RadialLayoutGroup))]
    public class RadialLayoutGroup_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BaseSizeFactor", MethodType.Getter)]
        public static bool BaseSizeFactor(RadialLayoutGroup __instance, ref float __result)
        {
            if (Neo.canvasScale != 1.0f)
            {
                return true;
            }

            __result = __instance.transform.lossyScale.x;
            return false;
        }
    }
}
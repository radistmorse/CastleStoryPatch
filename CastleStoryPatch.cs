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
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
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
}
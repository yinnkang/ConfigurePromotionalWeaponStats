// File: ConfigurePromotionalWeaponStatsTacticalProbe.cs
using System;
using System.Reflection;
using HarmonyLib;
using PhoenixPoint.Modding;
using UnityEngine;

namespace ConfigurePromotionalWeaponStats
{
    internal static class CPWS_TacticalProbe
    {
        private static ModLogger _log;

        public static void Install(Harmony h, ModLogger logger)
        {
            _log = logger;

            // Patch Tactical level start (robust, reflection-only)
            var tlc = AccessTools.TypeByName("PhoenixPoint.Tactical.Levels.TacticalLevelController");
            if (tlc != null)
            {
                // Start() runs when the tactical scene is live
                var m = AccessTools.Method(tlc, "Start");
                if (m != null)
                {
                    h.Patch(m, postfix: new HarmonyMethod(typeof(CPWS_TacticalProbe), nameof(Postfix_TacticalStart)));
                    _log?.LogInfo("[CPWS] TacticalProbe armed (TacticalLevelController.Start)");
                    return;
                }
            }

            // Fallback: patch any MonoBehaviour in Tactical.Levels with OnEnable (best-effort)
            var tll = AccessTools.TypeByName("PhoenixPoint.Tactical.Levels.TacticalLevel");
            if (tll != null)
            {
                var m2 = AccessTools.Method(tll, "OnEnable");
                if (m2 != null)
                {
                    h.Patch(m2, postfix: new HarmonyMethod(typeof(CPWS_TacticalProbe), nameof(Postfix_TacticalStart)));
                    _log?.LogInfo("[CPWS] TacticalProbe armed (TacticalLevel.OnEnable)");
                }
            }
        }

        private static void Postfix_TacticalStart(object __instance)
        {
            try
            {
                _log?.LogInfo("[CPWS] Tactical start: probing weapons for Piercing...");

                var weaponType = AccessTools.TypeByName("PhoenixPoint.Tactical.Entities.Equipment.Weapon");
                if (weaponType == null)
                {
                    _log?.LogWarning("[CPWS] Weapon runtime type not found.");
                    return;
                }

                // Find all runtime Weapon components
                var findArr = typeof(UnityEngine.Object)
                    .GetMethod("FindObjectsOfType", new Type[] { typeof(Type), typeof(bool) });
                if (findArr == null)
                {
                    _log?.LogWarning("[CPWS] Could not reflect FindObjectsOfType(Type,bool).");
                    return;
                }

                var allWeapons = findArr.Invoke(null, new object[] { weaponType, true }) as UnityEngine.Object[];
                if (allWeapons == null || allWeapons.Length == 0)
                {
                    _log?.LogInfo("[CPWS] No weapons found in scene.");
                    return;
                }

                foreach (var w in allWeapons)
                {
                    try
                    {
                        // weapon.Def -> WeaponDef
                        var def = w.GetType().GetProperty("Def", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(w);
                        if (def == null) continue;

                        var name = def.GetType().GetProperty("name")?.GetValue(def) as string ?? def.ToString();
                        float pierce = ReadPiercingFromWeaponDef(def);
                        _log?.LogInfo($"[CPWS] WeaponDef='{name}' Piercing={pierce}");
                    }
                    catch (Exception e)
                    {
                        _log?.LogWarning("[CPWS] Probe error on weapon: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                _log?.LogWarning("[CPWS] Tactical probe failed: " + e);
            }
        }

        private static float ReadPiercingFromWeaponDef(object weaponDefObj)
        {
            try
            {
                if (weaponDefObj == null) return 0f;
                var payload = weaponDefObj.GetType().GetProperty("DamagePayload", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(weaponDefObj);
                if (payload == null) return 0f;

                var keywords = payload.GetType().GetProperty("DamageKeywords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(payload) as System.Collections.IEnumerable;
                if (keywords == null) return 0f;

                foreach (var pair in keywords)
                {
                    var def = pair.GetType().GetProperty("DamageKeywordDef")?.GetValue(pair);
                    var defName = def?.GetType().GetProperty("name")?.GetValue(def) as string ?? def?.ToString();
                    if (!string.IsNullOrEmpty(defName) && defName.IndexOf("pierc", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var valObj = pair.GetType().GetProperty("Value")?.GetValue(pair);
                        if (valObj is float f) return f;
                        if (valObj is int i) return i;
                        if (valObj is double d) return (float)d;
                    }
                }
            }
            catch { }
            return 0f;
        }
    }
}

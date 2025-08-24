using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PhoenixPoint.Modding;
using UnityEngine;

namespace ConfigurePromotionalWeaponStats
{
    internal static class CPWS_UIDamageRowPatch
    {
        private static ModLogger _log;
        private static ConfigurePromotionalWeaponStatsConfig _cfg;

        public static void Install(Harmony h, ModLogger logger, ConfigurePromotionalWeaponStatsConfig cfg)
        {
            _log = logger;
            _cfg = cfg;

            // Preferred targets: item info/tooltip modules (names differ across builds)
            string[] typeNames = new[] {
                "PhoenixPoint.Common.View.ViewModules.UIModuleItemInfo",
                "PhoenixPoint.Common.View.ViewModules.UIModuleItemTooltip",
                "PhoenixPoint.Common.View.ViewModules.UIModuleInventoryItemInfo",
                "PhoenixPoint.Common.View.ViewModules.UIModuleItemDetails",
            };
            string[] methods = new[] { "Bind", "ShowItem", "Show", "Refresh", "OnShow" };

            int patched = 0;
            foreach (var tn in typeNames)
            {
                var t = AccessTools.TypeByName(tn);
                if (t == null) continue;
                foreach (var mn in methods)
                {
                    var m = AccessTools.Method(t, mn, null, null);
                    if (m == null) continue;
                    try
                    {
                        h.Patch(m, postfix: new HarmonyMethod(typeof(CPWS_UIDamageRowPatch), nameof(Postfix_UIModule_Any)));
                        patched++;
                    }
                    catch (Exception e)
                    {
                        _log?.LogWarning($"UI row patch failed on {tn}.{mn}: {e.Message}");
                    }
                }
            }

            // Fallback: generic text write path
            var bu = AccessTools.TypeByName("Base.UnityUtil");
            if (bu != null)
            {
                foreach (var mi in bu.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (mi.Name != "SetText") continue;
                    var pars = mi.GetParameters();
                    if (pars.Length == 2 && typeof(string).IsAssignableFrom(pars[1].ParameterType))
                    {
                        try
                        {
                            h.Patch(mi, postfix: new HarmonyMethod(typeof(CPWS_UIDamageRowPatch), nameof(Postfix_Base_UnityUtil_SetText)));
                            patched++;
                        }
                        catch (Exception e)
                        {
                            _log?.LogWarning("Fallback SetText patch failed: " + e.Message);
                        }
                    }
                }
            }

            _log?.LogInfo($"CPWS UI damage row patch ready. Patched methods={patched}");
        }

        // Any of the item UI methods above
        private static void Postfix_UIModule_Any(object __instance)
        {
            try
            {
                var comp = __instance as Component;
                if (comp == null) return;
                
                var damageValues = GetDamageValuesFromBoundWeapon(__instance);
                if (damageValues.piercing > 0f)
                    TryInjectDamageRow(comp.transform, "Piercing", damageValues.piercing);
                if (damageValues.fire > 0f)
                    TryInjectDamageRow(comp.transform, "Fire", damageValues.fire);
                if (damageValues.viral > 0f)
                    TryInjectDamageRow(comp.transform, "Viral", damageValues.viral);
                if (damageValues.poison > 0f)
                    TryInjectDamageRow(comp.transform, "Poison", damageValues.poison);
            }
            catch (Exception e)
            {
                _log?.LogWarning("Postfix_UIModule_Any err: " + e);
            }
        }

        // Fallback: when a row's text is set
        private static void Postfix_Base_UnityUtil_SetText(object __0, string __1)
        {
            try
            {
                var textComp = __0 as Component;
                if (textComp == null) return;
                string s = __1 ?? "";
                if (!(s.IndexOf("Damage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      s.IndexOf("Shred", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      s.IndexOf("Piercing", StringComparison.OrdinalIgnoreCase) >= 0))
                    return;

                Transform t = textComp.transform;
                for (int i = 0; i < 6 && t != null; i++) t = t.parent;
                if (t == null) return;

                // We can't reliably find the bound WeaponDef from here; bail to avoid noisy inserts.
                // The "Any" postfix above is the intended path.
            }
            catch (Exception e)
            {
                _log?.LogWarning("Postfix_SetText err: " + e);
            }
        }

        // ===== Core helpers =====

        private static (float piercing, float fire, float viral, float poison) GetDamageValuesFromBoundWeapon(object moduleInstance)
        {
            // Common fields/properties for current item/def
            string[] candNames = new[] { "Item", "ItemDef", "DisplayedItem", "CurrentItem", "m_Item", "m_ItemDef", "ItemData", "EquipmentDef", "WeaponDef" };
            foreach (var name in candNames)
            {
                try
                {
                    var val = GetMemberValue(moduleInstance, name);
                    if (val == null) continue;

                    var wd = ExtractWeaponDef(val);
                    if (wd != null) return ReadDamageValuesFromWeaponDef(wd);

                    var vd = GetMemberValue(val, "ItemDef") ?? GetMemberValue(val, "Def");
                    if (vd != null)
                    {
                        var wd2 = ExtractWeaponDef(vd);
                        if (wd2 != null) return ReadDamageValuesFromWeaponDef(wd2);
                    }
                }
                catch { /* keep trying */ }
            }
            return (0f, 0f, 0f, 0f);
        }

        private static object ExtractWeaponDef(object obj)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            if (t.FullName == "PhoenixPoint.Common.Entities.Items.WeaponDef") return obj;
            var w = GetMemberValue(obj, "WeaponDef");
            if (w != null && w.GetType().FullName == "PhoenixPoint.Common.Entities.Items.WeaponDef") return w;
            if (t.FullName != null && t.FullName.Contains("WeaponDef")) return obj;
            return null;
        }

        private static (float piercing, float fire, float viral, float poison) ReadDamageValuesFromWeaponDef(object weaponDefObj)
        {
            float piercing = 0f, fire = 0f, viral = 0f, poison = 0f;
            try
            {
                if (weaponDefObj == null) return (0f, 0f, 0f, 0f);
                var payload = GetMemberValue(weaponDefObj, "DamagePayload");
                if (payload == null) return (0f, 0f, 0f, 0f);
                var keywords = GetMemberValue(payload, "DamageKeywords") as System.Collections.IEnumerable;
                if (keywords == null) return (0f, 0f, 0f, 0f);
                
                foreach (var pair in keywords)
                {
                    var def = GetMemberValue(pair, "DamageKeywordDef");
                    if (def == null) continue;
                    var name = GetMemberValue(def, "name") as string ?? def.ToString();
                    if (string.IsNullOrEmpty(name)) continue;
                    
                    var valObj = GetMemberValue(pair, "Value");
                    float value = 0f;
                    if (valObj is float f) value = f;
                    else if (valObj is int i) value = i;
                    else if (valObj is double d) value = (float)d;
                    
                    if (name.IndexOf("pierc", StringComparison.OrdinalIgnoreCase) >= 0)
                        piercing = value;
                    else if (name.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           name.IndexOf("burn", StringComparison.OrdinalIgnoreCase) >= 0)
                        fire = value;
                    else if (name.IndexOf("viral", StringComparison.OrdinalIgnoreCase) >= 0)
                        viral = value;
                    else if (name.IndexOf("poison", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           name.IndexOf("toxic", StringComparison.OrdinalIgnoreCase) >= 0)
                        poison = value;
                }
            }
            catch (Exception e)
            {
                _log?.LogWarning("ReadDamageValuesFromWeaponDef failed: " + e);
            }
            return (piercing, fire, viral, poison);
        }

        private static void TryInjectDamageRow(Transform root, string damageType, float val)
        {
            if (root == null || val <= 0.0f) return;

            // Don't duplicate
            if (FindAnyTextContains(root, damageType) != null) return;

            // Place directly UNDER Shred; if no Shred row, fall back under Damage
            var baseText = FindAnyTextContains(root, "Shred") ?? FindAnyTextContains(root, "Damage");
            if (baseText == null) return;

            var row = baseText.transform.parent;     // text under row container
            if (row == null || row.parent == null) return;

            var parent = row.parent;
            var clone = UnityEngine.Object.Instantiate(row.gameObject, parent);
            clone.name = $"Row_{damageType}";
            clone.SetActive(true);

            // Move clone to immediately after the template row
            try { clone.transform.SetSiblingIndex(row.GetSiblingIndex() + 1); } catch { }

            // Label text
            string displayText = $"{damageType} +{(int)val}";
            SetAnyTextOn(clone.transform, displayText);

            // Copy the icon from the template row (if any) via reflection on UnityEngine.UI.Image
            try
            {
                var srcImg = FindFirstComponentOfType(row, "UnityEngine.UI.Image");
                var dstImg = FindFirstComponentOfType(clone.transform, "UnityEngine.UI.Image");
                if (srcImg != null && dstImg != null)
                {
                    var imgType = srcImg.GetType();
                    var spriteProp = imgType.GetProperty("sprite");
                    var colorProp = imgType.GetProperty("color");
                    var spriteVal = spriteProp?.GetValue(srcImg, null);
                    var colorVal = colorProp?.GetValue(srcImg, null);
                    spriteProp?.SetValue(dstImg, spriteVal, null);
                    colorProp?.SetValue(dstImg, colorVal, null);
                }
            }
            catch { /* non-fatal */ }

            _log?.LogInfo($"Injected {damageType} stat row into item panel (under Shred).");
        }

        // ===== Reflection utilities =====

        private static object GetMemberValue(object obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return null;
            var t = obj.GetType();
            const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var p = t.GetProperty(name, BF);
            if (p != null && p.CanRead) return p.GetValue(obj, null);
            var f = t.GetField(name, BF);
            if (f != null) return f.GetValue(obj);
            return null;
        }

        private static Component FindAnyTextContains(Transform root, string token)
        {
            foreach (var comp in GetComponentsOfType(root, "UnityEngine.UI.Text"))
            {
                var s = GetStringProperty(comp, "text");
                if (!string.IsNullOrEmpty(s) && s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return comp;
            }
            foreach (var comp in GetComponentsOfType(root, "TMPro.TMP_Text"))
            {
                var s = GetStringProperty(comp, "text");
                if (!string.IsNullOrEmpty(s) && s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return comp;
            }
            return null;
        }

        private static void SetAnyTextOn(Transform node, string value)
        {
            var ugui = GetFirstComponentOfType(node, "UnityEngine.UI.Text");
            if (ugui != null) { SetStringProperty(ugui, "text", value); return; }
            var tmp = GetFirstComponentOfType(node, "TMPro.TMP_Text");
            if (tmp != null) SetStringProperty(tmp, "text", value);
        }

        private static Component FindFirstComponentOfType(Transform root, string typeName)
        {
            var comps = GetComponentsOfType(root, typeName);
            return comps.FirstOrDefault();
        }

        private static Component GetFirstComponentOfType(Transform root, string typeName)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) return null;
            var mi = typeof(Component).GetMethod("GetComponentInChildren", new Type[] { typeof(Type), typeof(bool) });
            if (mi != null) return mi.Invoke(root, new object[] { type, true }) as Component;
            return GetComponentsOfType(root, typeName).FirstOrDefault();
        }

        private static System.Collections.Generic.IEnumerable<Component> GetComponentsOfType(Transform root, string typeName)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) yield break;
            var mi = typeof(Component).GetMethod("GetComponentsInChildren", new Type[] { typeof(Type), typeof(bool) });
            if (mi == null) yield break;
            var arr = mi.Invoke(root, new object[] { type, true }) as Component[];
            if (arr == null) yield break;
            foreach (var c in arr) yield return c;
        }

        private static string GetStringProperty(object obj, string propName)
        {
            if (obj == null) return null;
            var p = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            if (p == null || p.PropertyType != typeof(string) || !p.CanRead) return null;
            return p.GetValue(obj) as string;
        }

        private static void SetStringProperty(object obj, string propName, string value)
        {
            if (obj == null) return;
            var p = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            if (p == null || p.PropertyType != typeof(string) || !p.CanWrite) return;
            p.SetValue(obj, value);
        }
    }
}
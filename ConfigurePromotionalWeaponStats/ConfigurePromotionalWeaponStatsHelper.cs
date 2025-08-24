using System.Reflection;
using PhoenixPoint.Tactical.Entities.Weapons;

namespace ConfigurePromotionalWeaponStats
{
    internal static class ConfigurePromotionalWeaponStatsHelper
    {
        private const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Matches the ballistic model the UI expects: PP shows ~41m for Spread=1deg.
        /// </summary>
        public static float CalculateSpreadDegreesFromRange(float effectiveRange)
        {
            if (effectiveRange == 999) return 0f; // PP convention for "unlimited"
            return 1f / (effectiveRange / 41f);
        }

        /// <summary>
        /// Best-effort push so the inventory/tooltip UI also shows the edited range.
        /// We touch several potential sources the UI reads from across builds:
        /// - backing field of WeaponDef.EffectiveRange (if it's an auto-property)
        /// - FireMode / FireModes[].EffectiveRange
        /// - any Ability on the weapon that exposes EffectiveRange
        /// - (optional) a Range string in the ViewElementDef
        /// </summary>
        public static void PushEffectiveRangeToUI(WeaponDef weaponDef, int effectiveRange)
        {
            if (weaponDef == null) return;

            // 1) If EffectiveRange is an auto-property with a private backing field, set it.
            try
            {
                var backing = weaponDef.GetType().GetField("<EffectiveRange>k__BackingField", BF);
                if (backing != null) backing.SetValue(weaponDef, effectiveRange);
            }
            catch { /* ignore */ }

            // 2) Try a direct property set (on some builds EffectiveRange is writable)
            TrySetProp(weaponDef, "EffectiveRange", effectiveRange);

            // 3) FireMode (single) and FireModes (array/list)
            try
            {
                var fireModeProp = weaponDef.GetType().GetProperty("FireMode", BF);
                var fireMode = fireModeProp != null ? fireModeProp.GetValue(weaponDef) : null;
                TrySetProp(fireMode, "EffectiveRange", (float)effectiveRange);

                var fireModesProp = weaponDef.GetType().GetProperty("FireModes", BF);
                var fireModes = fireModesProp != null ? fireModesProp.GetValue(weaponDef) as System.Collections.IEnumerable : null;
                if (fireModes != null)
                {
                    foreach (var fm in fireModes)
                        TrySetProp(fm, "EffectiveRange", (float)effectiveRange);
                }
            }
            catch { /* ignore */ }

            // 4) Any attached abilities that expose EffectiveRange
            try
            {
                var abilitiesProp = weaponDef.GetType().GetProperty("Abilities", BF);
                var abilities = abilitiesProp != null ? abilitiesProp.GetValue(weaponDef) as System.Collections.IEnumerable : null;
                if (abilities != null)
                {
                    foreach (var ability in abilities)
                        TrySetProp(ability, "EffectiveRange", (float)effectiveRange);
                }
            }
            catch { /* ignore */ }

            // 5) Optional: if ViewElementDef has a range text field, set a friendly string
            try
            {
                var vedProp = weaponDef.GetType().GetProperty("ViewElementDef", BF);
                var ved = vedProp != null ? vedProp.GetValue(weaponDef) : null;
                if (ved != null)
                {
                    foreach (var p in ved.GetType().GetProperties(BF))
                    {
                        if (!p.CanWrite) continue;
                        if (p.PropertyType == typeof(string) &&
                            p.Name.ToLowerInvariant().Contains("range"))
                        {
                            p.SetValue(ved, effectiveRange.ToString());
                        }
                    }
                }
            }
            catch { /* ignore */ }
        }

        private static void TrySetProp(object target, string propName, object value)
        {
            if (target == null) return;
            var pi = target.GetType().GetProperty(propName, BF);
            if (pi == null || !pi.CanWrite) return;

            var t = pi.PropertyType;
            try
            {
                if (t == typeof(float) && value is int i) pi.SetValue(target, (float)i);
                else if (t == typeof(int) && value is float f) pi.SetValue(target, (int)f);
                else pi.SetValue(target, value);
            }
            catch { /* ignore */ }
        }
    }
}

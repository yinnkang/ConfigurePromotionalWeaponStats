using Base.Core;
using Base.Defs;
using Base.Levels;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.Game;
using PhoenixPoint.Modding;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;

namespace ConfigurePromotionalWeaponStats
{
    /// <summary>
    /// This is the main mod class. Only one can exist per assembly.
    /// If no ModMain is detected in assembly, then no other classes/callbacks will be called.
    /// </summary>
    public class ConfigurePromotionalWeaponStats : ModMain
    {
        /// <summary>
        /// defines the modifiable values for any given weapon.
        /// </summary>
        private struct WeaponValues
        {
            public float[] Damage;
            public int AmmoCapacity;
            public int Burst;
            public int ProjectilesPerShot;
            public int EffectiveRange;
            public int ApCost;
            public int HandsToUse;
            public int Weight;
            public bool StopOnFirstHit;

            public WeaponValues(float[] damage, int ammoCapacity, int burst,
                int projectilesPerShot, int effectiveRange, int apCost,
                int handsToUse, int weight, bool stopOnFirstHit)
            {
                Damage = damage;
                AmmoCapacity = ammoCapacity;
                Burst = burst;
                ProjectilesPerShot = projectilesPerShot;
                EffectiveRange = effectiveRange;
                ApCost = apCost;
                HandsToUse = handsToUse;
                Weight = weight;
                StopOnFirstHit = stopOnFirstHit;
            }
        }

        /// Config is accessible at any time, if any is declared.
        public new ConfigurePromotionalWeaponStatsConfig Config => (ConfigurePromotionalWeaponStatsConfig)base.Config;

        /// This property indicates if mod can be Safely Disabled from the game.
        /// Safely disabled mods can be reenabled again. Unsafely disabled mods will need game restart to take effect.
        /// Unsafely disabled mods usually cannot revert their changes in OnModDisabled
        public override bool CanSafelyDisable => true;

        private WeaponDef AresGold, FirebirdGold, HelGold, FirebirdPR, WhiteNeonDeimos, NeonDeimos, TobiasHandgun;
        private ItemDef AresClip, FirebirdClip, HelClip, DeimosClip, TobiasHandgunClip;
        private WeaponValues DefaultAresGold, DefaultFirebirdGold, DefaultHelGold, DefaultFirebirdPR, DefaultWhiteNeonDeimos, DefaultNeonDeimos, DefaultTobiasHandgun;

        /// <summary>
        /// Callback for when mod is enabled. Called even on game startup.
        /// </summary>
        public override void OnModEnabled()
        {
            try
            {
                DefRepository Repo = GameUtl.GameComponent<DefRepository>();

                AresGold = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("PX_AssaultRifle_Gold_WeaponDef"));
                FirebirdGold = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("PX_SniperRifle_Gold_WeaponDef"));
                HelGold = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("PX_HeavyCannon_Gold_WeaponDef"));
                FirebirdPR = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("PX_SniperRifle_RisingSun_WeaponDef"));
                WhiteNeonDeimos = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("SY_LaserAssaultRifle_WhiteNeon_WeaponDef"));
                NeonDeimos = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("SY_LaserAssaultRifle_Neon_WeaponDef"));
                TobiasHandgun = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("NJ_Gauss_HandGun_WeaponDef"));

                AresClip = Repo.GetAllDefs<ItemDef>().FirstOrDefault(a => a.name.Equals("PX_AssaultRifle_AmmoClip_ItemDef"));
                FirebirdClip = Repo.GetAllDefs<ItemDef>().FirstOrDefault(a => a.name.Equals("PX_SniperRifle_AmmoClip_ItemDef"));
                HelClip = Repo.GetAllDefs<ItemDef>().FirstOrDefault(a => a.name.Equals("PX_HeavyCannon_AmmoClip_ItemDef"));
                DeimosClip = Repo.GetAllDefs<ItemDef>().FirstOrDefault(a => a.name.Equals("SY_LaserAssaultRifle_AmmoClip_ItemDef"));
                TobiasHandgunClip = Repo.GetAllDefs<ItemDef>().FirstOrDefault(a => a.name.Equals("NJ_Gauss_HandGun_AmmoClip_ItemDef"));

                DefaultAresGold = getWeaponValuesFromWeaponDef(AresGold);
                DefaultFirebirdGold = getWeaponValuesFromWeaponDef(FirebirdGold);
                DefaultHelGold = getWeaponValuesFromWeaponDef(HelGold);
                DefaultFirebirdPR = getWeaponValuesFromWeaponDef(FirebirdPR);
                DefaultWhiteNeonDeimos = getWeaponValuesFromWeaponDef(WhiteNeonDeimos);
                DefaultNeonDeimos = getWeaponValuesFromWeaponDef(NeonDeimos);
                DefaultTobiasHandgun = getWeaponValuesFromWeaponDef(TobiasHandgun);

                OnConfigChanged();
                // Install UI damage row patch (handles piercing and fire)
                try
                {
                    var hUi = new Harmony("com.quinn11235.CPWS.UIRow");
                    CPWS_UIDamageRowPatch.Install(hUi, Logger, Config);

                    // Optional: only if you added CPWS_TacticalProbe.cs
                    var hProbe = new Harmony("com.quinn11235.CPWS.Probe");
                    CPWS_TacticalProbe.Install(hProbe, Logger);
                }
                catch (Exception e)
                {
                    Logger.LogWarning("CPWS UI row patch failed: " + e);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Callback for when mod is disabled. This will be called even if mod cannot be safely disabled.
        /// Guaranteed to have OnModEnabled before.
        /// </summary>
        public override void OnModDisabled()
        {
            setDefsFromWeaponValues(DefaultAresGold, AresGold, AresClip);
            setDefsFromWeaponValues(DefaultFirebirdGold, FirebirdGold, FirebirdClip);
            setDefsFromWeaponValues(DefaultHelGold, HelGold, HelClip);
            setDefsFromWeaponValues(DefaultFirebirdPR, FirebirdPR, FirebirdClip);
            setDefsFromWeaponValues(DefaultWhiteNeonDeimos, WhiteNeonDeimos, DeimosClip);
            setDefsFromWeaponValues(DefaultNeonDeimos, NeonDeimos, DeimosClip);
            if (Config.EnableTobiasHandgunMods)
            {
                setDefsFromWeaponValues(DefaultTobiasHandgun, TobiasHandgun, TobiasHandgunClip);
            }
        }

        /// <summary>
        /// Callback for when any property from mod's config is changed.
        /// </summary>
        public override void OnConfigChanged()
        {
            float[] AresGoldDamage = { Config.AresGoldArDamage, Config.AresGoldArShred };
            WeaponValues AresGoldValues = new WeaponValues(
                AresGoldDamage,
                Config.AresGoldArAmmoCapacity,
                Config.AresGoldArBurst,
                Config.AresGoldArProjectilesPerShot,
                Config.AresGoldArEffectiveRange,
                Config.AresGoldArApCost,
                Config.AresGoldArHandsToUse,
                Config.AresGoldArWeight,
                Config.AresGoldArStopOnFirstHit
            );
            float[] FirebirdGoldDamage = { Config.FirebirdGoldSrDamage };
            WeaponValues FirebirdGoldValues = new WeaponValues(
                FirebirdGoldDamage,
                Config.FirebirdGoldSrAmmoCapacity,
                Config.FirebirdGoldSrBurst,
                Config.FirebirdGoldSrProjectilesPerShot,
                Config.FirebirdGoldSrEffectiveRange,
                Config.FirebirdGoldSrApCost,
                Config.FirebirdGoldSrHandsToUse,
                Config.FirebirdGoldSrWeight,
                Config.FirebirdGoldSrStopOnFirstHit
            );
            float[] HelGoldDamage = { Config.HelGoldIiDamage, Config.HelGoldIiShred, Config.HelGoldIiShock };
            WeaponValues HelGoldValues = new WeaponValues(
                HelGoldDamage,
                Config.HelGoldIiAmmoCapacity,
                Config.HelGoldIiBurst,
                Config.HelGoldIiProjectilesPerShot,
                Config.HelGoldIiEffectiveRange,
                Config.HelGoldIiApCost,
                Config.HelGoldIiHandsToUse,
                Config.HelGoldIiWeight,
                Config.HelGoldIiStopOnFirstHit
            );
            float[] FirebirdPRDamage = { Config.FirebirdPRSrDamage };
            WeaponValues FirebirdPRValues = new WeaponValues(
                FirebirdPRDamage,
                Config.FirebirdPRSrAmmoCapacity,
                Config.FirebirdPRSrBurst,
                Config.FirebirdPRSrProjectilesPerShot,
                Config.FirebirdPRSrEffectiveRange,
                Config.FirebirdPRSrApCost,
                Config.FirebirdPRSrHandsToUse,
                Config.FirebirdPRSrWeight,
                Config.FirebirdPRSrStopOnFirstHit
            );
            float[] WhiteNeonDeimosDamage = { Config.WhiteNeonDeimosArDamage };
            WeaponValues WhiteNeonDeimosValues = new WeaponValues(
                WhiteNeonDeimosDamage,
                Config.WhiteNeonDeimosArAmmoCapacity,
                Config.WhiteNeonDeimosArBurst,
                Config.WhiteNeonDeimosArProjectilesPerShot,
                Config.WhiteNeonDeimosArEffectiveRange,
                Config.WhiteNeonDeimosArApCost,
                Config.WhiteNeonDeimosArHandsToUse,
                Config.WhiteNeonDeimosArWeight,
                Config.WhiteNeonDeimosArStopOnFirstHit
            );
            setDefsFromWeaponValues(AresGoldValues, AresGold, AresClip);
            setDefsFromWeaponValues(FirebirdGoldValues, FirebirdGold, FirebirdClip);
            SetDamageKeywordValue(FirebirdGold, "pierc", Config.FirebirdGoldSrPiercing, Logger);
            setDefsFromWeaponValues(HelGoldValues, HelGold, HelClip);
            setDefsFromWeaponValues(FirebirdPRValues, FirebirdPR, FirebirdClip);
            SetDamageKeywordValue(FirebirdPR, "pierc", Config.FirebirdPRSrPiercing, Logger);
            // Setup WhiteNeonDeimos with custom damage keywords
            SetupWhiteNeonDeimosDamageKeywords(WhiteNeonDeimos, Config.WhiteNeonDeimosArDamage, Config.WhiteNeonDeimosArPiercing, Config.WhiteNeonDeimosArFire, Logger);
            SetWeaponPropertiesOnly(WhiteNeonDeimos, DeimosClip, Config.WhiteNeonDeimosArAmmoCapacity, Config.WhiteNeonDeimosArBurst, Config.WhiteNeonDeimosArProjectilesPerShot, Config.WhiteNeonDeimosArEffectiveRange, Config.WhiteNeonDeimosArApCost, Config.WhiteNeonDeimosArHandsToUse, Config.WhiteNeonDeimosArWeight, Config.WhiteNeonDeimosArStopOnFirstHit);
            
            // Setup NeonDeimos with custom damage keywords
            SetupNeonDeimosDamageKeywords(NeonDeimos, Config.NeonDeimosArDamage, Config.NeonDeimosArPiercing, Config.NeonDeimosArViral, Config.NeonDeimosArPoison, Logger);
            SetWeaponPropertiesOnly(NeonDeimos, DeimosClip, Config.NeonDeimosArAmmoCapacity, Config.NeonDeimosArBurst, Config.NeonDeimosArProjectilesPerShot, Config.NeonDeimosArEffectiveRange, Config.NeonDeimosArApCost, Config.NeonDeimosArHandsToUse, Config.NeonDeimosArWeight, Config.NeonDeimosArStopOnFirstHit);
            
            // Setup Tobias West Handgun modifications if enabled
            if (Config.EnableTobiasHandgunMods)
            {
                float[] TobiasHandgunDamage = { Config.TobiasHandgunDamage, Config.TobiasHandgunShred };
                WeaponValues TobiasHandgunValues = new WeaponValues(
                    TobiasHandgunDamage,
                    Config.TobiasHandgunAmmoCapacity,
                    1, // Burst (handguns are single shot)
                    1, // ProjectilesPerShot
                    Config.TobiasHandgunEffectiveRange,
                    Config.TobiasHandgunAPCost,
                    1, // HandsToUse (handguns are 1-handed)
                    1, // Weight (keep original weight)
                    true // StopOnFirstHit (typical for handguns)
                );
                setDefsFromWeaponValues(TobiasHandgunValues, TobiasHandgun, TobiasHandgunClip);
                
                // Set piercing damage and spread
                if (TobiasHandgun != null)
                {
                    SetDamageKeywordValue(TobiasHandgun, "pierc", Config.TobiasHandgunPiercing, Logger);
                    TobiasHandgun.SpreadDegrees = Config.TobiasHandgunSpread;
                    Logger.LogInfo($"Applied Tobias Handgun modifications: Damage={Config.TobiasHandgunDamage}, Shred={Config.TobiasHandgunShred}, Piercing={Config.TobiasHandgunPiercing}, AmmoCapacity={Config.TobiasHandgunAmmoCapacity}, Spread={Config.TobiasHandgunSpread}, EffectiveRange={Config.TobiasHandgunEffectiveRange}, APCost={Config.TobiasHandgunAPCost} points");
                }
            }
        }

        /* CALCULATION FUNCTIONS */


        private static int CalculateAPToUsePerc(int ApCost)
        {
            return ApCost * 25;
        }
        private static int CalculateApCost(int APToUsePerc)
        {
            return APToUsePerc / 25;
        }

        /* WEAPON DATA FUNCTIONS */
        private WeaponValues getWeaponValuesFromWeaponDef(WeaponDef weaponDef)
        {
            float[] Damage = new float[weaponDef.DamagePayload.DamageKeywords.Count];
            for (int i = 0; i < weaponDef.DamagePayload.DamageKeywords.Count; i++)
            {
                Damage[i] = weaponDef.DamagePayload.DamageKeywords[i].Value;
            }
            return new WeaponValues(
                Damage,
                weaponDef.ChargesMax,
                weaponDef.DamagePayload.AutoFireShotCount,
                weaponDef.DamagePayload.ProjectilesPerShot,
                weaponDef.EffectiveRange,
                CalculateApCost(weaponDef.APToUsePerc),
                weaponDef.HandsToUse,
                weaponDef.Weight,
                weaponDef.DamagePayload.StopOnFirstHit
            );
        }
        private void setDefsFromWeaponValues(WeaponValues weaponValues, WeaponDef weaponDef, ItemDef itemDef)
        {
            if (weaponDef == null) return;

            weaponDef.APToUsePerc = CalculateAPToUsePerc(weaponValues.ApCost);
            weaponDef.DamagePayload.AutoFireShotCount = weaponValues.Burst;
            for (int i = 0; i < weaponValues.Damage.Length; i++)
            {
                weaponDef.DamagePayload.DamageKeywords[i].Value = weaponValues.Damage[i];
            }
            weaponDef.DamagePayload.ProjectilesPerShot = weaponValues.ProjectilesPerShot;
            weaponDef.DamagePayload.StopOnFirstHit = weaponValues.StopOnFirstHit;

            // Keep the ballistic calculation in sync
            weaponDef.SpreadDegrees = ConfigurePromotionalWeaponStatsHelper.CalculateSpreadDegreesFromRange(weaponValues.EffectiveRange);
            // Nudge all the UI-facing places that cache or read EffectiveRange
            ConfigurePromotionalWeaponStatsHelper.PushEffectiveRangeToUI(weaponDef, weaponValues.EffectiveRange);

            weaponDef.HandsToUse = weaponValues.HandsToUse;
            weaponDef.ChargesMax = weaponValues.AmmoCapacity;
            if (itemDef != null) itemDef.ChargesMax = weaponValues.AmmoCapacity;
            weaponDef.Weight = weaponValues.Weight;
        }

        private static void SetupWhiteNeonDeimosDamageKeywords(WeaponDef weaponDef, float damageValue, float piercingValue, float fireValue, ModLogger logger)
        {
            try
            {
                if (weaponDef == null || weaponDef.DamagePayload == null) return;

                // Set base damage first
                if (weaponDef.DamagePayload.DamageKeywords != null && weaponDef.DamagePayload.DamageKeywords.Count > 0)
                {
                    weaponDef.DamagePayload.DamageKeywords[0].Value = damageValue;
                }

                // Remove shred damage (replace with piercing)
                RemoveDamageKeyword(weaponDef, "shred", logger);
                
                // Set or add new damage types
                SetDamageKeywordValue(weaponDef, "pierc", piercingValue, logger);
                SetDamageKeywordValue(weaponDef, "fire", fireValue, logger);
                SetDamageKeywordValue(weaponDef, "burn", fireValue, logger);

                logger?.LogInfo($"Set up damage keywords for {weaponDef.name}: Damage={damageValue}, Piercing={piercingValue}, Fire={fireValue}");
            }
            catch (System.Exception e)
            {
                logger?.LogWarning("SetupWhiteNeonDeimosDamageKeywords failed: " + e);
            }
        }

        private static void SetupNeonDeimosDamageKeywords(WeaponDef weaponDef, float damageValue, float piercingValue, float viralValue, float poisonValue, ModLogger logger)
        {
            try
            {
                if (weaponDef == null || weaponDef.DamagePayload == null) return;

                // Set base damage first
                if (weaponDef.DamagePayload.DamageKeywords != null && weaponDef.DamagePayload.DamageKeywords.Count > 0)
                {
                    weaponDef.DamagePayload.DamageKeywords[0].Value = damageValue;
                }

                // Remove shred damage (replace with piercing/viral/poison)
                RemoveDamageKeyword(weaponDef, "shred", logger);
                
                // Set or add new damage types
                SetDamageKeywordValue(weaponDef, "pierc", piercingValue, logger);
                SetDamageKeywordValue(weaponDef, "viral", viralValue, logger);
                SetDamageKeywordValue(weaponDef, "poison", poisonValue, logger);
                SetDamageKeywordValue(weaponDef, "toxic", poisonValue, logger);

                logger?.LogInfo($"Set up damage keywords for {weaponDef.name}: Damage={damageValue}, Piercing={piercingValue}, Viral={viralValue}, Poison={poisonValue}");
            }
            catch (System.Exception e)
            {
                logger?.LogWarning("SetupNeonDeimosDamageKeywords failed: " + e);
            }
        }

        private static void SetWeaponPropertiesOnly(WeaponDef weaponDef, ItemDef itemDef, int ammoCapacity, int burst, int projectilesPerShot, int effectiveRange, int apCost, int handsToUse, int weight, bool stopOnFirstHit)
        {
            if (weaponDef == null) return;

            weaponDef.APToUsePerc = CalculateAPToUsePerc(apCost);
            weaponDef.DamagePayload.AutoFireShotCount = burst;
            weaponDef.DamagePayload.ProjectilesPerShot = projectilesPerShot;
            weaponDef.DamagePayload.StopOnFirstHit = stopOnFirstHit;

            // Keep the ballistic calculation in sync
            weaponDef.SpreadDegrees = ConfigurePromotionalWeaponStatsHelper.CalculateSpreadDegreesFromRange(effectiveRange);
            // Nudge all the UI-facing places that cache or read EffectiveRange
            ConfigurePromotionalWeaponStatsHelper.PushEffectiveRangeToUI(weaponDef, effectiveRange);

            weaponDef.HandsToUse = handsToUse;
            weaponDef.ChargesMax = ammoCapacity;
            if (itemDef != null) itemDef.ChargesMax = ammoCapacity;
            weaponDef.Weight = weight;
        }

        private static void RemoveDamageKeyword(WeaponDef weaponDef, string keywordName, ModLogger logger)
        {
            try
            {
                if (weaponDef == null || weaponDef.DamagePayload == null || weaponDef.DamagePayload.DamageKeywords == null) return;
                
                var keywordList = weaponDef.DamagePayload.DamageKeywords.ToList();
                bool removed = false;
                
                for (int i = keywordList.Count - 1; i >= 0; i--)
                {
                    var dk = keywordList[i];
                    try
                    {
                        var def = dk.DamageKeywordDef;
                        if (def == null) continue;
                        string n = def.name ?? def.ToString();
                        if (!string.IsNullOrEmpty(n) && n.IndexOf(keywordName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            keywordList.RemoveAt(i);
                            removed = true;
                            logger?.LogInfo($"Removed {keywordName} damage from {weaponDef.name}");
                        }
                    }
                    catch { /* continue */ }
                }
                
                if (removed)
                {
                    weaponDef.DamagePayload.DamageKeywords = keywordList;
                }
            }
            catch (System.Exception e)
            {
                logger?.LogWarning($"RemoveDamageKeyword failed for {keywordName}: " + e);
            }
        }

        private static void SetDamageKeywordValue(WeaponDef weaponDef, string keywordName, float value, ModLogger logger)
        {
            try
            {
                if (weaponDef == null || weaponDef.DamagePayload == null || weaponDef.DamagePayload.DamageKeywords == null) return;
                if (value <= 0f) return; // Don't add zero values
                
                foreach (var dk in weaponDef.DamagePayload.DamageKeywords)
                {
                    try
                    {
                        var def = dk.DamageKeywordDef;
                        if (def == null) continue;
                        string n = def.name ?? def.ToString();
                        if (!string.IsNullOrEmpty(n) && n.IndexOf(keywordName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            dk.Value = value;
                            logger?.LogInfo($"Set {keywordName} damage on {weaponDef.name} -> {value}");
                            return;
                        }
                    }
                    catch { /* continue */ }
                }
                
                // If we didn't find an existing keyword, try to add one from repository
                try
                {
                    var repo = GameUtl.GameComponent<DefRepository>();
                    var damageKeyword = repo.GetAllDefs<DamageKeywordDef>()
                        .FirstOrDefault(d => d.name != null && 
                            d.name.IndexOf(keywordName, System.StringComparison.OrdinalIgnoreCase) >= 0);
                    
                    if (damageKeyword != null)
                    {
                        var newKeywordPair = new DamageKeywordPair()
                        {
                            DamageKeywordDef = damageKeyword,
                            Value = value
                        };
                        
                        var keywordList = weaponDef.DamagePayload.DamageKeywords.ToList();
                        keywordList.Add(newKeywordPair);
                        weaponDef.DamagePayload.DamageKeywords = keywordList;
                        
                        logger?.LogInfo($"Added {keywordName} damage to {weaponDef.name} -> {value}");
                    }
                }
                catch (System.Exception e)
                {
                    logger?.LogWarning($"Failed to add {keywordName} damage keyword: " + e.Message);
                }
            }
            catch (System.Exception e)
            {
                logger?.LogWarning($"SetDamageKeywordValue failed for {keywordName}: " + e);
            }
        }

        private static void SetPiercing(WeaponDef weaponDef, float value, ModLogger logger)
        {
            try
            {
                if (weaponDef == null || weaponDef.DamagePayload == null || weaponDef.DamagePayload.DamageKeywords == null) return;
                foreach (var dk in weaponDef.DamagePayload.DamageKeywords)
                {
                    try
                    {
                        var def = dk.DamageKeywordDef;
                        if (def == null) continue;
                        // match by name contains "Pierc" to be robust across builds
                        string n = def.name ?? def.ToString();
                        if (!string.IsNullOrEmpty(n) && n.IndexOf("pierc", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            dk.Value = value;
                            logger?.LogInfo($"Set Piercing on {weaponDef.name} -> {value}");
                            break;
                        }
                    }
                    catch { /* continue */ }
                }
            }
            catch (System.Exception e)
            {
                logger?.LogWarning("SetPiercing failed: " + e);
            }
        }

    
    }
}

using PhoenixPoint.Modding;

namespace ConfigurePromotionalWeaponStats
{
    /// <summary>
    /// ModConfig is mod settings that players can change from within the game.
    /// Config is only editable from players in main menu.
    /// Only one config can exist per mod assembly.
    /// Config is serialized on disk as json.
    /// </summary>
    public class ConfigurePromotionalWeaponStatsConfig : ModConfig
    {
        /// these are the default game values as far as I can tell.
        /// we need to set them here so that they show up when enabling
        /// the mod, but not when starting the game with the mod already
        /// enabled.

        /// Gold Ares AR-1
        public float AresGoldArDamage = 35f;
        public float AresGoldArShred = 8f;
        public int AresGoldArAmmoCapacity = 36;
        public int AresGoldArBurst = 6;
        public int AresGoldArProjectilesPerShot = 1;
        public int AresGoldArEffectiveRange = 30;
        public int AresGoldArApCost = 1;
        public int AresGoldArHandsToUse = 2;
        public int AresGoldArWeight = 3;
        public bool AresGoldArStopOnFirstHit = false;

        /// Gold Firebird SR
        public float FirebirdGoldSrDamage = 150f;
        public float FirebirdGoldSrPiercing = 50f;
        public int FirebirdGoldSrAmmoCapacity = 8;
        public int FirebirdGoldSrBurst = 1;
        public int FirebirdGoldSrProjectilesPerShot = 1;
        public int FirebirdGoldSrEffectiveRange = 60;
        public int FirebirdGoldSrApCost = 3;
        public int FirebirdGoldSrHandsToUse = 2;
        public int FirebirdGoldSrWeight = 4;
        public bool FirebirdGoldSrStopOnFirstHit = false;

        /// Gold Hel II Cannon
        public float HelGoldIiDamage = 240f;
        public float HelGoldIiShred = 20f;
        public float HelGoldIiShock = 280f;
        public int HelGoldIiAmmoCapacity = 6;
        public int HelGoldIiBurst = 1;
        public int HelGoldIiProjectilesPerShot = 1;
        public int HelGoldIiEffectiveRange = 20;
        public int HelGoldIiApCost = 2;
        public int HelGoldIiHandsToUse = 2;
        public int HelGoldIiWeight = 5;
        public bool HelGoldIiStopOnFirstHit = false;

        /// Phoenix Rising Firebird SR
        public float FirebirdPRSrDamage = 120f;
        public float FirebirdPRSrPiercing = 30f;
        public int FirebirdPRSrAmmoCapacity = 8;
        public int FirebirdPRSrBurst = 1;
        public int FirebirdPRSrProjectilesPerShot = 1;
        public int FirebirdPRSrEffectiveRange = 51;
        public int FirebirdPRSrApCost = 2;
        public int FirebirdPRSrHandsToUse = 2;
        public int FirebirdPRSrWeight = 4;
        public bool FirebirdPRSrStopOnFirstHit = false;

        /// White Neon Deimos AR
        public float WhiteNeonDeimosArDamage = 40f;
        public float WhiteNeonDeimosArPiercing = 40f;
        public float WhiteNeonDeimosArFire = 0f;
        public int WhiteNeonDeimosArAmmoCapacity = 60;
        public int WhiteNeonDeimosArBurst = 6;
        public int WhiteNeonDeimosArProjectilesPerShot = 1;
        public int WhiteNeonDeimosArEffectiveRange = 35;
        public int WhiteNeonDeimosArApCost = 2;
        public int WhiteNeonDeimosArHandsToUse = 2;
        public int WhiteNeonDeimosArWeight = 3;
        public bool WhiteNeonDeimosArStopOnFirstHit = false;

        /// Neon Deimos AR
        public float NeonDeimosArDamage = 30f;
        public float NeonDeimosArPiercing = 30f;
        public float NeonDeimosArViral = 2f;
        public float NeonDeimosArPoison = 10f;
        public int NeonDeimosArAmmoCapacity = 60;
        public int NeonDeimosArBurst = 6;
        public int NeonDeimosArProjectilesPerShot = 1;
        public int NeonDeimosArEffectiveRange = 32;
        public int NeonDeimosArApCost = 2;
        public int NeonDeimosArHandsToUse = 2;
        public int NeonDeimosArWeight = 3;
        public bool NeonDeimosArStopOnFirstHit = false;

        /// Tobias West Handgun (NJ_Gauss_HandGun_WeaponDef) - Iron Fury HG - Always Active
        [ConfigField(text: "Tobias Handgun Damage")]
        public int TobiasHandgunDamage = 80;
        [ConfigField(text: "Tobias Handgun Piercing")]
        public float TobiasHandgunPiercing = 20f;
        [ConfigField(text: "Tobias Handgun Shred")]
        public float TobiasHandgunShred = 10f;
        [ConfigField(text: "Tobias Handgun Ammo Capacity")]
        public int TobiasHandgunAmmoCapacity = 12;
        [ConfigField(text: "Tobias Handgun Effective Range")]
        public int TobiasHandgunEffectiveRange = 18;
        [ConfigField(text: "Tobias Handgun AP Cost (1-4 points)")]
        public int TobiasHandgunAPCost = 1;

    }
}

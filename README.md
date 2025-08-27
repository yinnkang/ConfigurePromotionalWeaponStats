# ConfigurePromotionalWeaponStats

Phoenix Point mod for configuring promotional weapon stats (damage, ammo, burst, range, AP cost).

## Weapons

- **Gold Ares** - Assault rifle
- **Gold Firebird** - Sniper Rifle  
- **Gold Hel II** - Heavy Cannon
- **Phoenix Rising Firebird** - Sniper Rifle
- **Neon White Deimos** - Assault rifle
- **Neon Deimos** - Assault rifle
- **Bonus: Tobias West pistol** 

## Configuration

### Per weapon
- Damage (Base, Shred where applicable, Piercing has been added to most weapons)
- Ammunition capacity, Burst fire, Projectiles per shot
- Effective range, Action Point cost
- Hands required, Weight, Stop on first hit

### Usage
1. Enable mod in Phoenix Point Mods menu
2. Configure values in mod settings
3. Changes apply immediately to all weapons

## Technical Details

- Modifies weapon TacticalItemDefs using DefRepository system
- Supports multiple damage types via DamageKeywordPair
- Real-time updates without restart required
- Safe to enable/disable mid-campaign

## Installation

Extract to `Documents/My Games/Phoenix Point/Mods/` directory.

## Build

Requires ModSDK in parent directory. Output in `Dist/` folder.

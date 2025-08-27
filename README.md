# ConfigurePromotionalWeaponStats

Phoenix Point mod for configuring promotional weapon stats (damage, ammo, burst, range, AP cost).

## Weapons

- **Gold Ares** - Assault rifle
- **Gold Firebird** - Shotgun  
- **Gold Hel II** - Sniper rifle
- **Phoenix Rising Firebird** - Special shotgun
- **Neon White Deimos** - Special pistol

## Configuration

### Per weapon
- Base/Piercing/Paralysing damage
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
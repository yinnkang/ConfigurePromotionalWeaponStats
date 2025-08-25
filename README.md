# Configure Promotional Weapon Stats

A Phoenix Point mod that allows configuration of statistics for promotional weapons through the mod settings interface.

## Supported Promotional Weapons

- **Gold Ares**: Promotional assault rifle variant
- **Gold Firebird**: Promotional shotgun variant  
- **Gold Hel II**: Promotional sniper rifle variant
- **Phoenix Rising Firebird**: Special edition shotgun
- **Neon White Deimos**: Special edition pistol

## Configurable Statistics

Each weapon can have the following values modified:

### Damage Properties
- **Base Damage**: Primary damage value
- **Piercing Damage**: Armor penetration damage
- **Paralysing Damage**: Non-lethal paralysis damage

### Combat Stats  
- **Ammunition Capacity**: Magazine size
- **Burst Fire**: Number of shots per burst
- **Projectiles Per Shot**: Multiple projectiles (for shotguns)
- **Effective Range**: Optimal engagement distance
- **Action Point Cost**: AP required to fire

### Physical Properties
- **Hands Required**: One-handed or two-handed weapon
- **Weight**: Equipment weight
- **Stop on First Hit**: Whether projectiles stop after first target

## Installation

1. Extract the mod to your Phoenix Point Mods directory
2. Enable "Configure Promotional Weapon Stats" in the Mods menu  
3. Configure desired weapon values in the mod settings
4. Changes apply to existing and newly acquired weapons

## Configuration

Access configuration through the in-game mod settings menu. Each weapon has individual controls for:

- Damage type values (base/piercing/paralysing)
- Ammunition and burst fire settings
- Range and accuracy properties  
- Action point costs
- Physical characteristics

Default values match the original game statistics.

## Technical Notes

### Damage System Integration
- Supports multiple damage types per weapon
- Properly integrates with Phoenix Point's damage keyword system
- Damage modifications apply to weapon definitions
- Real-time updates without game restart required

### Weapon Modification
- Changes apply to weapon definitions when mod loads
- Mod can be safely enabled/disabled during campaigns
- Original values restored when mod is disabled
- Uses DefRepository system for weapon modification

## Build Instructions

1. Ensure ModSDK is available in the parent directory
2. Build using Visual Studio or MSBuild
3. Output will be in the Dist directory
4. Copy Dist contents to Phoenix Point Mods folder

## Compatibility

- Compatible with other weapon modification mods
- Can be enabled/disabled mid-campaign
- Preserves original weapon values when disabled
- Works with UI enhancement mods
namespace RuneGrid.Tactics.Core;

public static class TacticalClassRules
{
    public static RouteIntent PreferredRoute(UnitTemplate template) => template.TacticalClass switch
    {
        TacticalClass.Duelist or TacticalClass.Harrier or TacticalClass.Stalker => RouteIntent.Flank,
        TacticalClass.Skywarden => RouteIntent.Fastest,
        TacticalClass.Warden or TacticalClass.Sentinel or TacticalClass.Runesmith => RouteIntent.Safe,
        _ => RouteIntent.Direct
    };

    public static int DamageBonus(UnitState attacker, UnitState target, AbilityDefinition ability)
    {
        var distance = attacker.Position.ManhattanDistance(target.Position);
        var bonus = attacker.Template.TacticalClass switch
        {
            TacticalClass.Duelist when distance <= 1 => 1,
            TacticalClass.Artillery when distance >= 3 => 1,
            TacticalClass.Sapper when target.Statuses.ContainsKey("snared") => 2,
            TacticalClass.Harrier when attacker.Template.Mobility == MobilityProfile.Winged => 1,
            _ => 0
        };
        return ability.Element == ElementKind.Arcane && attacker.Template.TacticalClass == TacticalClass.Seer ? bonus + 1 : bonus;
    }

    public static int HealingBonus(UnitState caster) => caster.Template.TacticalClass is TacticalClass.Runesmith or TacticalClass.Support ? 1 : 0;

    public static int ThreatPenalty(UnitTemplate template) => template.TacticalClass switch
    {
        TacticalClass.Skywarden => 0,
        TacticalClass.Duelist or TacticalClass.Harrier => 1,
        TacticalClass.Warden or TacticalClass.Sentinel => 4,
        _ => 3
    };

    public static int CoverReward(UnitTemplate template) => template.TacticalClass switch
    {
        TacticalClass.Warden or TacticalClass.Sentinel => 3,
        TacticalClass.Runesmith or TacticalClass.Artillery => 2,
        _ => 1
    };
}

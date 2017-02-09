// The Skill struct only contains the dynamic skill properties and a name, so
// that the static properties can be read from the scriptable object. The
// benefits are low bandwidth and easy Player database saving (saves always
// refer to the scriptable skill, so we can change that any time).
//
// Skills have to be structs in order to work with SyncLists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to Time.time + cooldown after casting each time. This
// way we don't need an extra Update method that increases the elapsed time for
// each skill all the time.
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct Skill {
    // name used to reference the database entry (cant save template directly
    // because synclist only support simple types)
    public string name;

    // dynamic stats (cooldowns etc.)
    public bool learned;
    public int level;
    public float castTimeEnd; // server time
    public float cooldownEnd; // server time
    public float buffTimeEnd; // server time

    // constructors
    public Skill(SkillTemplate template) {
        name = template.name;

        // learned only if learned by default
        learned = template.learnDefault;
        level = 1;

        // ready immediately
        castTimeEnd = cooldownEnd = buffTimeEnd = Time.time;
    }

    // does the template still exist?
    public bool TemplateExists() {
        return SkillTemplate.dict.ContainsKey(name);
    }

    // database quest property access etc.
    public SkillTemplate template {
        get { return SkillTemplate.dict[name]; }
    }
    public string category {
        get { return template.category; }
    }
    public int damage {
        get { return template.levels[level-1].damage; }
    }
    public float castTime {
        get { return template.levels[level-1].castTime; }
    }
    public float cooldown {
        get { return template.levels[level-1].cooldown; }
    }
    public float castRange {
        get { return template.levels[level-1].castRange; }
    }
    public float aoeRadius {
        get { return template.levels[level-1].aoeRadius; }
    }
    public int manaCosts {
        get { return template.levels[level-1].manaCosts; }
    }
    public int healsHp {
        get { return template.levels[level-1].healsHp; }
    }
    public int healsMp {
        get { return template.levels[level-1].healsMp; }
    }
    public float buffTime {
        get { return template.levels[level-1].buffTime; }
    }
    public int buffsHpMax {
        get { return template.levels[level-1].buffsHpMax; }
    }
    public int buffsMpMax {
        get { return template.levels[level-1].buffsMpMax; }
    }
    public int buffsDamage {
        get { return template.levels[level-1].buffsDamage; }
    }
    public int buffsDefense {
        get { return template.levels[level-1].buffsDefense; }
    }
    public float buffsBlock {
        get { return template.levels[level-1].buffsBlock; }
    }
    public float buffsCrit {
        get { return template.levels[level-1].buffsCrit; }
    }
    public float buffsHpPercentPerSecond {
        get { return template.levels[level-1].buffsHpPercentPerSecond; }
    }
    public float buffsMpPercentPerSecond {
        get { return template.levels[level-1].buffsMpPercentPerSecond; }
    }
    public bool followupDefaultAttack {
        get { return template.followupDefaultAttack; }
    }
    public Sprite image {
        get { return template.image; }
    }
    public Projectile projectile {
        get { return template.levels[level-1].projectile; }
    }
    public bool learnDefault {
        get { return template.learnDefault; }
    }
    public int requiredLevel {
        get { return template.levels[level-1].requiredLevel; }
    }
    public long requiredSkillExp {
        get { return template.levels[level-1].requiredSkillExp; }
    }
    public int maxLevel {
        get { return template.levels.Length; }
    }
    public int upgradeRequiredLevel {
        get { return (level < maxLevel) ? template.levels[level].requiredLevel : 0; }
    }
    public long upgradeRequiredSkillExp {
        get { return (level < maxLevel) ? template.levels[level].requiredSkillExp : 0; }
    }

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // SkillTemplate because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the template.
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME} Lvl {LEVEL}</b>
    Description here...

    Damage: {DAMAGE}
    Cast Time: {CASTTIME}
    Cooldown: {COOLDOWN}
    Cast Range: {CASTRANGE}
    AoE Radius: {AOERADIUS}
    Heals Health: {HEALSHP}
    Heals Mana: {HEALSMP}
    Buff Time: {BUFFTIME}
    Buffs max Health: {BUFFSHPMAX}
    Buffs max Mana: {BUFFSMPMAX}
    Buffs damage: {BUFFSDAMAGE}
    Buffs defense: {BUFFSDEFENSE}
    Buffs block: {BUFFSBLOCK}
    Buffs critical: {BUFFSCRIT}
    Buffs Health % per Second: {BUFFSHPPERCENTPERSECOND}
    Buffs Mana % per Second: {BUFFSMPPERCENTPERSECOND}
    Mana Costs: {MANACOSTS}
    */
    public string Tooltip(bool showRequirements = false) {
        string tip = template.tooltip;
        tip = tip.Replace("{NAME}", name);
        tip = tip.Replace("{CATEGORY}", category);
        tip = tip.Replace("{LEVEL}", level.ToString());
        tip = tip.Replace("{DAMAGE}", damage.ToString());
        tip = tip.Replace("{CASTTIME}", Utils.PrettyTime(castTime));
        tip = tip.Replace("{COOLDOWN}", Utils.PrettyTime(cooldown));
        tip = tip.Replace("{CASTRANGE}", castRange.ToString());
        tip = tip.Replace("{AOERADIUS}", aoeRadius.ToString());
        tip = tip.Replace("{HEALSHP}", healsHp.ToString());
        tip = tip.Replace("{HEALSMP}", healsMp.ToString());
        tip = tip.Replace("{BUFFTIME}", Utils.PrettyTime(buffTime));
        tip = tip.Replace("{BUFFSHPMAX}", buffsHpMax.ToString());
        tip = tip.Replace("{BUFFSMPMAX}", buffsMpMax.ToString());
        tip = tip.Replace("{BUFFSDAMAGE}", buffsDamage.ToString());
        tip = tip.Replace("{BUFFSDEFENSE}", buffsDefense.ToString());
        tip = tip.Replace("{BUFFSBLOCK}", Mathf.RoundToInt(buffsBlock * 100).ToString());
        tip = tip.Replace("{BUFFSCRIT}", Mathf.RoundToInt(buffsCrit * 100).ToString());
        tip = tip.Replace("{BUFFSHPPERCENTPERSECOND}", Mathf.RoundToInt(buffsHpPercentPerSecond * 100).ToString());
        tip = tip.Replace("{BUFFSMPPERCENTPERSECOND}", Mathf.RoundToInt(buffsMpPercentPerSecond * 100).ToString());
        tip = tip.Replace("{MANACOSTS}", manaCosts.ToString());

        // only show requirements if necessary
        if (showRequirements) {
            tip += "\n<b><i>Required Level: " + requiredLevel + "</i></b>\n" +
                   "<b><i>Required Skill Exp.: " + requiredSkillExp + "</i></b>\n";
        }
        // only show upgrade if necessary (not if not learned yet etc.)
        if (learned && level < maxLevel) {
            tip += "\n<i>Upgrade:</i>\n" +
                   "<i>  Required Level: " + upgradeRequiredLevel + "</i>\n" +
                   "<i>  Required Skill Exp.: " + upgradeRequiredSkillExp + "</i>\n";
        }
        
        return tip;
    }

    public float CastTimeRemaining() {
        // how much time remaining until the casttime ends? (using server time)
        return NetworkTime.time >= castTimeEnd ? 0 : castTimeEnd - NetworkTime.time;
    }

    public bool IsCasting() {
        // we are casting a skill if the casttime remaining is > 0
        return CastTimeRemaining() > 0;
    }

    public float CooldownRemaining() {
        // how much time remaining until the cooldown ends? (using server time)
        return NetworkTime.time >= cooldownEnd ? 0 : cooldownEnd - NetworkTime.time;
    }

    public float BuffTimeRemaining() {
        // how much time remaining until the buff ends? (using server time)
        return NetworkTime.time >= buffTimeEnd ? 0 : buffTimeEnd - NetworkTime.time;        
    }

    public bool IsReady() {
        return CooldownRemaining() == 0;
    }    
}

public class SyncListSkill : SyncListStruct<Skill> { }

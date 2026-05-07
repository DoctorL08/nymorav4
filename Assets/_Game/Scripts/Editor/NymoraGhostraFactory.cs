#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Génère le PassiveData + les 15 sorts Ghostra et les injecte dans ClassData Ghostra.
/// Menu : Oracle > Generate Ghostra Spells
/// </summary>
public static class NymoraGhostraFactory
{
    private const string BasePath    = "Assets/_Game/ScriptableObjects/Spells/Ghostra";
    private const string AttackPath  = BasePath + "/Attaques";
    private const string TactPath    = BasePath + "/Tactiques";
    private const string SurvivePath = BasePath + "/Survie";
    private const string PassivePath = BasePath;
    private const string ClassPath   = "Assets/_Game/ScriptableObjects/Classes/Ghostra.asset";

    [MenuItem("Oracle/Generate Ghostra Spells")]
    public static void GenerateAll()
    {
        EnsureFolders();

        int created = 0;

        var passive  = CreatePassive(ref created);
        var attacks  = CreateAttaques(ref created);
        var tactics  = CreateTactiques(ref created);
        var survives = CreateSurvie(ref created);
        var saignee  = CreateSaignee(ref created);

        InjectIntoClassData(passive, attacks, tactics, survives, saignee);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Nymora — Ghostra Factory",
            $"{created} assets créés.\n\nGhostra est prête au combat.",
            "Let's go !");

        Debug.Log($"[NymoraGhostraFactory] {created} assets générés.");
    }

    // ── Dossiers ─────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        CreateFolder("Assets/_Game/ScriptableObjects/Spells", "Ghostra");
        CreateFolder(BasePath, "Attaques");
        CreateFolder(BasePath, "Tactiques");
        CreateFolder(BasePath, "Survie");
    }

    static void CreateFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }

    // ── Passif ───────────────────────────────────────────────────────────────

    static PassiveData CreatePassive(ref int created)
    {
        string path = $"{PassivePath}/Ghostra_PresenceFuneste.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PassiveData>(path);
        if (existing != null) return existing;

        var p = ScriptableObject.CreateInstance<PassiveData>();
        p.passiveName        = "Présence Funeste";
        p.passiveType        = PassiveType.PresenceFuneste;
        p.trigger            = PassiveTrigger.Permanent;
        p.effectValue        = 0f;
        p.procChance         = 1f;
        p.conditionThreshold = 0;
        p.description        = "Chaque sort lancé dans le dos de l'ennemi inflige +35/+55/+75 et incrémente la jauge Présence Funeste.";

        AssetDatabase.CreateAsset(p, path);
        created++;
        return p;
    }

    // ── Attaques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateAttaques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Lame Spectrale : 3PA, mêlée, 115 dmg
        {
            var s = Spell("Lame Spectrale", pa: 3, cd: 0,
                melee: true, rMin: 1, rMax: 1,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Frappe spectrale au corps-à-corps. 115 dégâts.",
                synergy: "Idéal combiné à Présence Funeste (dos).");
            s.effects.Add(E(SpellEffectType.Damage, 115));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 2 — Frappe Fantôme : 4PA, range 1-3, 130dmg + Bleed 40×2t
        {
            var s = Spell("Frappe Fantôme", pa: 4, cd: 0,
                melee: false, rMin: 1, rMax: 3,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "130 dégâts + saignement 40/tour (2 tours).",
                synergy: "Pose un dot pour Saigne-Âme ou Lame Vorace.");
            s.effects.Add(E(SpellEffectType.Damage, 130));
            s.effects.Add(E(SpellEffectType.Bleed, 40, duration: 2));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 3 — Lame Vorace Spectrale : 3PA, mêlée, 100dmg + 60 si cible a debuff
        {
            var s = Spell("Lame Vorace Spectrale", pa: 3, cd: 0,
                melee: true, rMin: 1, rMax: 1,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "100 dégâts. +60 si la cible est affaiblie.",
                synergy: "Frappe Fantôme → Lame Vorace.");
            s.effects.Add(E(SpellEffectType.Damage, 100));
            s.effects.Add(E(SpellEffectType.Damage, 60, cond: SpellCondition.TargetHasDebuff));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 4 — Saigne-Âme : 4PA, range 1-2, 130dmg + 70 si cible a debuff
        {
            var s = Spell("Saigne-Âme", pa: 4, cd: 0,
                melee: false, rMin: 1, rMax: 2,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "130 dégâts. +70 si la cible a un effet négatif actif.",
                synergy: "Frappe Fantôme → Saigne-Âme pour burst maximum.");
            s.effects.Add(E(SpellEffectType.Damage, 130));
            s.effects.Add(E(SpellEffectType.Damage, 70, cond: SpellCondition.TargetHasDebuff));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 5 — Danse des Lames : 5PA, Circle radius 1, 110dmg AoE
        {
            var s = Spell("Danse des Lames", pa: 5, cd: 2,
                melee: false, rMin: 1, rMax: 1,
                zone: ZoneType.Circle, aoe: 1,
                cat: SpellDeckCategory.Attack,
                desc: "110 dégâts à tous les ennemis dans le cercle rayon 1.",
                synergy: "Chaque cible dans le dos reçoit le bonus Présence Funeste.");
            s.effects.Add(E(SpellEffectType.Damage, 110));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        return list;
    }

    // ── Tactiques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateTactiques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Pas dans l'Ombre : 2PA, range 1-4, FreeCell, Teleport
        {
            var s = Spell("Pas dans l'Ombre", pa: 2, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.FreeCell, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Téléportation vers une case libre à portée 4. Ignore les obstacles.",
                synergy: "Repositionnement pour frapper dans le dos.");
            s.ignoresLineOfSight = true;
            s.effects.Add(E(SpellEffectType.Teleport, 0));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 2 — Volte-Face : 2PA, range 1-4, RemovePM 1
        {
            var s = Spell("Volte-Face", pa: 2, cd: 1,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Retire 1 PM à la cible.",
                synergy: "Bride le repositionnement adverse.");
            s.effects.Add(E(SpellEffectType.RemovePM, 1));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 3 — Désorientation : 3PA, range 1-3, RemovePM 1 + GravityDebuff 1t
        {
            var s = Spell("Désorientation", pa: 3, cd: 2,
                melee: false, rMin: 1, rMax: 3,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Retire 1 PM et applique Gravité (1 tour) : téléportation et traction bloquées.",
                synergy: "Piège la cible pour faciliter le dos.");
            s.effects.Add(E(SpellEffectType.RemovePM, 1));
            s.effects.Add(E(SpellEffectType.GravityDebuff, 0, duration: 1));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 4 — Dague Lancée : 2PA, range 1-5, 50dmg
        {
            var s = Spell("Dague Lancée", pa: 2, cd: 0,
                melee: false, rMin: 1, rMax: 5,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "50 dégâts à distance.",
                synergy: "Harcèlement longue portée ou finisseur léger.");
            s.effects.Add(E(SpellEffectType.Damage, 50));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 5 — Marque de l'Ombre : 2PA, range 1-4, ReduceFirstAttack 15 × 2t
        {
            var s = Spell("Marque de l'Ombre", pa: 2, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Première attaque de la cible réduite de 15 % pendant 2 tours.",
                synergy: "Affaiblit les sorts d'attaque ennemis.");
            s.effects.Add(E(SpellEffectType.ReduceFirstAttack, 15, duration: 2));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        return list;
    }

    // ── Survie ────────────────────────────────────────────────────────────────

    static SpellData[] CreateSurvie(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Voile Spectral : 2PA, self, Cleanse, cd 10
        {
            var s = Spell("Voile Spectral", pa: 2, cd: 10,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Retire tous les effets négatifs. Recharge longue.",
                synergy: "Panic button contre contrôles ou DoT.");
            s.effects.Add(E(SpellEffectType.Cleanse, 0));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 2 — Réplique Fantôme : 3PA, self, Shield 200×2t + Heal 60
        {
            var s = Spell("Réplique Fantôme", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Bouclier 200 (2 tours) + soin 60.",
                synergy: "Tient les échanges courts.");
            s.effects.Add(E(SpellEffectType.Shield, 200, duration: 2));
            s.effects.Add(E(SpellEffectType.Heal, 60));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 3 — Pas de l'Au-Delà : 2PA, self, BonusPM 2
        {
            var s = Spell("Pas de l'Au-Delà", pa: 2, cd: 1,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "+2 PM ce tour.",
                synergy: "Allonge la portée de repositionnement pour trouver le dos.");
            s.effects.Add(E(SpellEffectType.BonusPM, 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 4 — Linceul d'Ombres : 3PA, self, Shield 130×2t + Thorns 40×2t
        {
            var s = Spell("Linceul d'Ombres", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Bouclier 130 (2 tours) + épines 40 (2 tours).",
                synergy: "Punit les attaquants CàC.");
            s.effects.Add(E(SpellEffectType.Shield, 130, duration: 2));
            s.effects.Add(E(SpellEffectType.Thorns, 40, duration: 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 5 — Dernier Pas : 4PA, self, Heal 180 + Teleport, si HP < 30 %
        {
            var s = Spell("Dernier Pas", pa: 4, cd: 5,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.FreeCell, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Soin 180 + téléportation vers une case libre. Seulement sous 30 % de PV.",
                synergy: "Évasion de secours en situation critique.");
            s.ignoresLineOfSight = true;
            s.effects.Add(E(SpellEffectType.Heal, 180, cond: SpellCondition.SelfHPBelow, threshold: 30));
            s.effects.Add(E(SpellEffectType.Teleport, 0, cond: SpellCondition.SelfHPBelow, threshold: 30));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        return list;
    }

    // ── Sort Enragé — Saignée ─────────────────────────────────────────────────

    static SpellData CreateSaignee(ref int created)
    {
        string path = $"{BasePath}/Saignee.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpellData>(path);
        if (existing != null) return existing;

        var s = Spell("Saignée", pa: 1, cd: 0,
            melee: false, rMin: 1, rMax: 1,
            zone: ZoneType.SingleTarget, aoe: 0,
            cat: SpellDeckCategory.Attack,
            desc: "200 dégâts + saignement 40/tour (2 tours). Sort Enragé — 1×/match.",
            synergy: "Débloqué à palier Enragé (8 jauges dorsales).");
        s.effects.Add(E(SpellEffectType.Damage, 200));
        s.effects.Add(E(SpellEffectType.Bleed, 40, duration: 2));

        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }

    // ── Injection dans ClassData ──────────────────────────────────────────────

    static void InjectIntoClassData(PassiveData passive,
        SpellData[] attacks, SpellData[] tactics, SpellData[] survives,
        SpellData saignee)
    {
        var classData = AssetDatabase.LoadAssetAtPath<ClassData>(ClassPath);
        if (classData == null)
        {
            Debug.LogError($"[NymoraGhostraFactory] ClassData introuvable : {ClassPath}");
            return;
        }

        classData.passiveData  = passive;
        classData.enragedSpell = saignee;

        classData.attackSpells.Clear();
        foreach (var s in attacks)
            if (s != null) classData.attackSpells.Add(s);

        classData.tacticSpells.Clear();
        foreach (var s in tactics)
            if (s != null) classData.tacticSpells.Add(s);

        classData.survivalSpells.Clear();
        foreach (var s in survives)
            if (s != null) classData.survivalSpells.Add(s);

        EditorUtility.SetDirty(classData);
        Debug.Log("[NymoraGhostraFactory] ClassData Ghostra mise à jour : passif + 15 sorts + Saignée injectés.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static SpellData Spell(string name, int pa, int cd,
        bool melee, int rMin, int rMax,
        ZoneType zone, int aoe,
        SpellDeckCategory cat,
        string desc, string synergy)
    {
        var s = ScriptableObject.CreateInstance<SpellData>();
        s.spellName          = name;
        s.paCost             = pa;
        s.cooldown           = cd;
        s.isMeleeOnly        = melee;
        s.rangeMin           = rMin;
        s.rangeMax           = rMax;
        s.zoneType           = zone;
        s.aoeRadius          = aoe;
        s.deckCategory       = cat;
        s.description        = desc;
        s.synergyDescription = synergy;
        return s;
    }

    static SpellEffect E(SpellEffectType type, int value,
        int duration = 0,
        SpellCondition cond = SpellCondition.Always,
        int threshold = 0)
    {
        return new SpellEffect
        {
            type               = type,
            value              = value,
            duration           = duration,
            condition          = cond,
            conditionThreshold = threshold,
        };
    }

    static SpellData SaveSpell(SpellData s, string folder, ref int created)
    {
        string path = $"{folder}/{s.spellName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpellData>(path);
        if (existing != null)
        {
            Debug.Log($"[NymoraGhostraFactory] Ignoré (déjà existant) : {s.spellName}");
            return existing;
        }
        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }
}
#endif

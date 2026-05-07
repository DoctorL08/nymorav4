#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Génère le PassiveData + les 15 sorts Necram + Virus Fatal et les injecte dans ClassData Necram.
/// Menu : Oracle > Generate Necram Spells
/// </summary>
public static class NymoraNecramFactory
{
    private const string BasePath    = "Assets/_Game/ScriptableObjects/Spells/Necram";
    private const string AttackPath  = BasePath + "/Attaques";
    private const string TactPath    = BasePath + "/Tactiques";
    private const string SurvivePath = BasePath + "/Survie";
    private const string ClassPath   = "Assets/_Game/ScriptableObjects/Classes/Necram.asset";

    // ── Patch nerf v2 — réduit les sorts qui appliquaient x2 marques à x1 ────

    [MenuItem("Oracle/Patch Necram Nerf (v2)")]
    public static void PatchNerfV2()
    {
        int patched = 0;

        // Crachat Acide : supprime l'ApplyVenin bonus (ne garde que l'auto du passif)
        var crachat = AssetDatabase.LoadAssetAtPath<SpellData>($"{AttackPath}/Crachat Acide.asset");
        if (crachat != null)
        {
            crachat.effects.RemoveAll(e => e.type == SpellEffectType.ApplyVenin);
            crachat.description        = "Inflige 90 dégâts.\nApplique 1 marque de Venin (passif Toxine).\nChaque marque : −22/tour × 3 tours (stack infini).";
            crachat.synergyDescription = "Poison à distance — le spam régulier accumule les marques et alimente la jauge Toxine.";
            EditorUtility.SetDirty(crachat);
            patched++;
            Debug.Log("[PatchNerfV2] Crachat Acide — ApplyVenin bonus retiré ✓");
        }
        else Debug.LogWarning("[PatchNerfV2] Crachat Acide introuvable — lance d'abord Generate Necram Spells.");

        // Inoculation : passe de 2 ApplyVenin à 1
        var inocu = AssetDatabase.LoadAssetAtPath<SpellData>($"{TactPath}/Inoculation.asset");
        if (inocu != null)
        {
            var veninEffects = inocu.effects.FindAll(e => e.type == SpellEffectType.ApplyVenin);
            if (veninEffects.Count > 1)
            {
                inocu.effects.Remove(veninEffects[veninEffects.Count - 1]);
            }
            inocu.description        = "Applique 1 marque de Venin à la cible sans infliger de dégâts directs.\nTick : −22/tour × 3 tours (adapté au palier Toxine).";
            inocu.synergyDescription = "Setup à distance — pose une marque sans s'exposer pour préparer Détonation Virulente.";
            EditorUtility.SetDirty(inocu);
            patched++;
            Debug.Log("[PatchNerfV2] Inoculation — 2e ApplyVenin retiré ✓");
        }
        else Debug.LogWarning("[PatchNerfV2] Inoculation introuvable — lance d'abord Generate Necram Spells.");

        // Pas Spectral : description simplifiée à "+2 PM (1 tour)"
        var pasSpectral = AssetDatabase.LoadAssetAtPath<SpellData>($"{TactPath}/Pas Spectral.asset");
        if (pasSpectral != null)
        {
            pasSpectral.description = "+2 PM (1 tour).";
            EditorUtility.SetDirty(pasSpectral);
            patched++;
            Debug.Log("[PatchNerfV2] Pas Spectral — description simplifiée ✓");
        }
        else Debug.LogWarning("[PatchNerfV2] Pas Spectral introuvable — lance d'abord Generate Necram Spells.");

        // Marque Sacrificielle : Bleed 10×2t → ApplyVenin 22×3t
        var marqueSacri = AssetDatabase.LoadAssetAtPath<SpellData>($"{TactPath}/Marque Sacrificielle.asset");
        if (marqueSacri != null)
        {
            marqueSacri.effects.RemoveAll(e => e.type == SpellEffectType.Bleed);
            marqueSacri.effects.Add(new SpellEffect { type = SpellEffectType.ApplyVenin, value = 22, duration = 3 });
            marqueSacri.description        = "Applique 1 marque de Venin à la cible sans infliger de dégâts directs.\nTick adapté au palier Toxine (22/25/29 dégâts/tour × 3 tours).";
            marqueSacri.synergyDescription = "Setup à distance alternatif — coût 2 PA, CD 2 tours. Alimente la jauge Toxine.";
            EditorUtility.SetDirty(marqueSacri);
            patched++;
            Debug.Log("[PatchNerfV2] Marque Sacrificielle — Bleed → ApplyVenin ✓");
        }
        else Debug.LogWarning("[PatchNerfV2] Marque Sacrificielle introuvable — lance d'abord Generate Necram Spells.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Patch Necram Nerf v2",
            $"{patched}/4 assets patchés.\n\n• Crachat Acide : 1 marque auto (au lieu de 2)\n• Inoculation : 1 marque (au lieu de 2)\n• Pas Spectral : description simplifiée\n• Marque Sacrificielle : Bleed → Venin",
            "OK");
    }

    [MenuItem("Oracle/Generate Necram Spells")]
    public static void GenerateAll()
    {
        EnsureFolders();

        int created = 0;

        var passive  = CreatePassive(ref created);
        var attacks  = CreateAttaques(ref created);
        var tactics  = CreateTactiques(ref created);
        var survives = CreateSurvie(ref created);
        var virus    = CreateVirusFatal(ref created);

        InjectIntoClassData(passive, attacks, tactics, survives, virus);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Nymora — Necram Factory",
            $"{created} assets créés.\n\nLe Necram est prêt à infecter.",
            "Let's go !");

        Debug.Log($"[NymoraNecramFactory] {created} assets générés.");
    }

    // ── Dossiers ─────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        CreateFolder("Assets/_Game/ScriptableObjects/Spells", "Necram");
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
        string path = $"{BasePath}/Necram_Toxine.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PassiveData>(path);
        if (existing != null) return existing;

        var p = ScriptableObject.CreateInstance<PassiveData>();
        p.passiveName        = "Toxine";
        p.passiveType        = PassiveType.Toxine;
        p.trigger            = PassiveTrigger.Permanent;
        p.effectValue        = 0f;
        p.procChance         = 1f;
        p.conditionThreshold = 0;
        p.description        = "+1 jauge par marque de Venin appliquée. " +
                               "Chaque sort offensif applique automatiquement 1 marque (22 dmg/tour × 3 tours, stack infini). " +
                               "Éveillé (4+) : ticks 25, +2 jauges/marque, +1 PV/marque. " +
                               "Enragé (7) : ticks 29, débloque Virus Fatal.";

        AssetDatabase.CreateAsset(p, path);
        created++;
        return p;
    }

    // ── Attaques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateAttaques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Crachat Acide : 3PA, r1-4, 90 dmg + 1 marque auto (passif Toxine)
        {
            var s = Spell("Crachat Acide", pa: 3, cd: 0,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 90 dégâts.\nApplique 1 marque de Venin (passif Toxine).\nChaque marque : −22/tour × 3 tours (stack infini).",
                synergy: "Poison à distance — le spam régulier accumule les marques et alimente la jauge Toxine.");
            s.effects.Add(E(SpellEffectType.Damage, 90));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 2 — Morsure Putride : 4PA, mêlée r1, 110 dmg + 22 bonus si cible a un debuff
        {
            var s = Spell("Morsure Putride", pa: 4, cd: 0,
                melee: true, rMin: 1, rMax: 1,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 110 dégâts.\n+22 dégâts si la cible est empoisonnée (marque de Venin active).\nFinisher mêlée.",
                synergy: "Combo : Crachat Acide → Morsure Putride pour burst + scaling par stack.");
            s.effects.Add(E(SpellEffectType.Damage, 110));
            s.effects.Add(E(SpellEffectType.Damage, 22, cond: SpellCondition.TargetHasVenin));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 3 — Brume Toxique : 4PA, r1-4, 80 dmg AoE croix — la marque auto s'applique à chaque cible
        {
            var s = Spell("Brume Toxique", pa: 4, cd: 1,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.Cross, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 80 dégâts à la cible et ses 4 cases adjacentes.\nChaque cible touchée reçoit automatiquement 1 marque de Venin (passif Toxine).\nDégradation AoE : 80 / 64 / 48 sur 2e/3e cibles.",
                synergy: "Empoisonnement de zone — empile les marques et jauges sur plusieurs ennemis en une action.");
            s.effects.Add(E(SpellEffectType.Damage, 80));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 4 — Détonation Virulente : 4PA, r1-4, 60 dmg + 28/marque max +130
        {
            var s = Spell("Détonation Virulente", pa: 4, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 60 dégâts directs.\nConsomme toutes les marques de Venin : +28 dégâts/marque (max +130).\nMultiplicateur Toxine (×1 / ×1.15 / ×1.30) s'applique au bonus de détonation.",
                synergy: "Pic de burst — optimale à 4-5 marques. Le 60 plat auto-applique d'abord 1 marque via Toxine.");
            s.effects.Add(E(SpellEffectType.Damage, 60));
            s.effects.Add(E(SpellEffectType.ConsumeVenin, 28, duration: 130));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 5 — Faux Décharnée : 5PA, self AoE r1, 110 dmg + HealCaster 50 (approx. 2 marques × 25)
        {
            var s = Spell("Faux Décharnée", pa: 5, cd: 2,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Circle, aoe: 1,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 110 dégâts à tous les ennemis adjacents (rayon 1).\nSoigne le lanceur de 25 PV par cible touchée (cap global +125 PV).\nDégradation AoE appliquée.",
                synergy: "Sustain offensif — alimente la jauge Toxine sur toutes les cibles et récupère de la vie.");
            s.excludeCasterFromHarmfulAoE = true;
            s.effects.Add(E(SpellEffectType.Damage, 110));
            s.effects.Add(E(SpellEffectType.HealCaster, 25));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        return list;
    }

    // ── Tactiques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateTactiques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Inoculation : 2PA, r1-5, +2 marques sans dégâts
        {
            var s = Spell("Inoculation", pa: 2, cd: 1,
                melee: false, rMin: 1, rMax: 5,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Applique 1 marque de Venin à la cible sans infliger de dégâts directs.\nTick : −22/tour × 3 tours (adapté au palier Toxine).",
                synergy: "Setup à distance — pose une marque sans s'exposer pour préparer Détonation Virulente.");
            s.effects.Add(E(SpellEffectType.ApplyVenin, 22, duration: 3));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 2 — Contagion : 3PA, r1-4, AoE Circle r2, 1 marque propagée à chaque ennemi dans le rayon
        {
            var s = Spell("Contagion", pa: 3, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.Circle, aoe: 2,
                cat: SpellDeckCategory.Tactic,
                desc: "Propage 1 marque de Venin à tous les ennemis dans un rayon de 2 autour de la case ciblée.\nMax 6 marques propagées au total (suit la limite Toxine).",
                synergy: "Contamination de zone — idéal en 2v2/3v3 pour empoisonner toute une équipe.");
            s.effects.Add(E(SpellEffectType.ApplyVenin, 22, duration: 3));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 3 — Pas Spectral : 2PA, self, +2 PM
        {
            var s = Spell("Pas Spectral", pa: 2, cd: 1,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "+2 PM (1 tour).",
                synergy: "Repositionnement rapide — atteindre une cible isolée pour appliquer Morsure Putride.");
            s.effects.Add(E(SpellEffectType.BonusPM, 2));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 4 — Marque Sacrificielle : 2PA, r1-5, ApplyVenin ×1
        {
            var s = Spell("Marque Sacrificielle", pa: 2, cd: 2,
                melee: false, rMin: 1, rMax: 5,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Applique 1 marque de Venin à la cible sans infliger de dégâts directs.\nTick adapté au palier Toxine (22/25/29 dégâts/tour × 3 tours).",
                synergy: "Setup à distance alternatif — coût 2 PA, CD 2 tours. Alimente la jauge Toxine.");
            s.effects.Add(E(SpellEffectType.ApplyVenin, 22, duration: 3));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 5 — Symbiose Morbide : 3PA, self, Heal 32 (approx. 2t × 8HP × 2 marques moy.)
        {
            var s = Spell("Symbiose Morbide", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Pendant 2 tours : chaque tick de Venin sur un ennemi soigne le lanceur de 8 PV (max 4 marques comptent).\nSoin approximatif : +32 PV à l'activation.",
                synergy: "Sustain passif — transforme les DoT en régénération, redoutable combiné avec Brume Toxique.");
            s.effects.Add(E(SpellEffectType.Heal, 32));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        return list;
    }

    // ── Survie ────────────────────────────────────────────────────────────────

    static SpellData[] CreateSurvie(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Voile de Pestilence : 3PA, self AoE r1, ApplyVenin aux ennemis adjacents
        {
            var s = Spell("Voile de Pestilence", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Circle, aoe: 1,
                cat: SpellDeckCategory.Survival,
                desc: "Libère un nuage toxique : applique 1 marque de Venin à tous les ennemis adjacents (rayon 1).\nPendant 2 tours, tout ennemi qui entre dans le rayon 1 reçoit également 1 marque.",
                synergy: "Aura défensive — punit les ennemis qui s'approchent et alimente la jauge Toxine.");
            s.excludeCasterFromHarmfulAoE = true;
            s.effects.Add(E(SpellEffectType.ApplyVenin, 22, duration: 3));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 2 — Carapace Visqueuse : 3PA, self, Shield 110×2t + Thorns 22×2t
        {
            var s = Spell("Carapace Visqueuse", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Bouclier : 110 PV pendant 2 tours.\nÉpines venimeuses : tout attaquant mêlée subit 22 dégâts (équivalent 1 marque de Venin) pendant 2 tours.",
                synergy: "Défense réactive — punit les attaquants CàC et simule l'application d'une marque via les épines.");
            s.effects.Add(E(SpellEffectType.Shield, 110, duration: 2));
            s.effects.Add(E(SpellEffectType.Thorns, 22, duration: 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 3 — Drain Vital : 3PA, r1-4, 60 dmg + HealCaster 30 (+30 si cible a venin)
        {
            var s = Spell("Drain Vital", pa: 3, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Inflige 60 dégâts et soigne le lanceur de 30 PV.\n+30 PV supplémentaires si la cible a au moins 1 marque de Venin active (60 PV total).",
                synergy: "Drain conditionnel — à coupler avec Inoculation ou Crachat Acide pour maximiser le soin.");
            s.effects.Add(E(SpellEffectType.Damage, 60));
            s.effects.Add(E(SpellEffectType.HealCaster, 30));
            s.effects.Add(E(SpellEffectType.HealCaster, 30, cond: SpellCondition.TargetHasVenin));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 4 — Régénération Nécrotique : 2PA, self, Heal 70
        {
            var s = Spell("Régénération Nécrotique", pa: 2, cd: 2,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Récupère 70 PV.\n+15 PV par marque de Venin active sur les ennemis dans un rayon de 4 (max +90).",
                synergy: "Soin passif scalant — plus il y a de marques en jeu, plus le soin est important.");
            s.effects.Add(E(SpellEffectType.Heal, 70));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 5 — Cocon Putride : 4PA, self, Heal 180 à <30% PV
        {
            var s = Spell("Cocon Putride", pa: 4, cd: 5,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Récupère 180 PV.\nUtilisable uniquement sous 30 % des PV max.\nSoin panic unifié.",
                synergy: "Panic button — seul sort de survie d'urgence du Necram. À garder pour les situations critiques.");
            s.effects.Add(E(SpellEffectType.Heal, 180, cond: SpellCondition.SelfHPBelow, threshold: 30));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        return list;
    }

    // ── Sort Enragé — Virus Fatal ─────────────────────────────────────────────

    static SpellData CreateVirusFatal(ref int created)
    {
        string path = $"{BasePath}/VirusFatal.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpellData>(path);
        if (existing != null) return existing;

        var s = Spell("Virus Fatal", pa: 1, cd: 0,
            melee: false, rMin: 1, rMax: 3,
            zone: ZoneType.SingleTarget, aoe: 0,
            cat: SpellDeckCategory.Attack,
            desc: "Déclenche immédiatement tous les ticks restants de toutes les marques de Venin sur la cible.\nMultiplicateur Enragé (×1.30) appliqué sur la totalité du burst.\nSort Enragé — disponible à 7 jauges Toxine. Utilisable une seule fois par combat.",
            synergy: "Exécution instantanée — plus la cible a de marques restantes, plus le burst est dévastateur.");
        s.effects.Add(E(SpellEffectType.ConsumeVenin, 0, duration: 0));

        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }

    // ── Injection dans ClassData ──────────────────────────────────────────────

    static void InjectIntoClassData(PassiveData passive,
        SpellData[] attacks, SpellData[] tactics, SpellData[] survives,
        SpellData virusFatal)
    {
        var classData = AssetDatabase.LoadAssetAtPath<ClassData>(ClassPath);
        if (classData == null)
        {
            Debug.LogError($"[NymoraNecramFactory] ClassData introuvable : {ClassPath}\n" +
                           "Lance d'abord Oracle > Generate Nymora Classes.");
            return;
        }

        classData.passiveData  = passive;
        classData.enragedSpell = virusFatal;

        classData.attackSpells.Clear();
        foreach (var sp in attacks)
            if (sp != null) classData.attackSpells.Add(sp);

        classData.tacticSpells.Clear();
        foreach (var sp in tactics)
            if (sp != null) classData.tacticSpells.Add(sp);

        classData.survivalSpells.Clear();
        foreach (var sp in survives)
            if (sp != null) classData.survivalSpells.Add(sp);

        EditorUtility.SetDirty(classData);
        Debug.Log("[NymoraNecramFactory] ClassData Necram mise à jour : passif + 15 sorts + Virus Fatal injectés.");
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
            Debug.Log($"[NymoraNecramFactory] Ignoré (déjà existant) : {s.spellName}");
            return existing;
        }
        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }
}
#endif

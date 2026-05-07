#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Génère le PassiveData + les 15 sorts Soulrender + Âme Lacérée et les injecte dans ClassData Soulrender.
/// Menu : Oracle > Generate Soulrender Spells
/// </summary>
public static class NymoraSoulrenderFactory
{
    private const string BasePath    = "Assets/_Game/ScriptableObjects/Spells/Soulrender";
    private const string AttackPath  = BasePath + "/Attaques";
    private const string TactPath    = BasePath + "/Tactiques";
    private const string SurvivePath = BasePath + "/Survie";
    private const string ClassPath   = "Assets/_Game/ScriptableObjects/Classes/Soulrender.asset";

    // ── Patch des assets existants (v2) ──────────────────────────────────────

    /// <summary>
    /// Patche les assets déjà générés sans les recréer.
    /// Corrige Tornade de Lames (portée 0), Marque du Sang (CD 1) et Second Pacte (CD 3).
    /// </summary>
    [MenuItem("Oracle/Patch Soulrender Spells (v2)")]
    public static void PatchV2()
    {
        int patched = 0;

        // Tornade de Lames : portée 0→0, exclude caster de l'AoE
        var tornade = AssetDatabase.LoadAssetAtPath<SpellData>($"{AttackPath}/Tornade de Lames.asset");
        if (tornade != null)
        {
            tornade.rangeMin = 0;
            tornade.rangeMax = 0;
            tornade.excludeCasterFromHarmfulAoE = true;
            EditorUtility.SetDirty(tornade);
            patched++;
            Debug.Log("[PatchV2] Tornade de Lames — portée 0, excludeCaster ✓");
        }
        else Debug.LogWarning("[PatchV2] Tornade de Lames introuvable — lance d'abord Generate Soulrender Spells.");

        // Marque du Sang : CD 0 → CD 1
        var marque = AssetDatabase.LoadAssetAtPath<SpellData>($"{TactPath}/Marque du Sang.asset");
        if (marque != null)
        {
            marque.cooldown = 1;
            EditorUtility.SetDirty(marque);
            patched++;
            Debug.Log("[PatchV2] Marque du Sang — CD 1 ✓");
        }
        else Debug.LogWarning("[PatchV2] Marque du Sang introuvable — lance d'abord Generate Soulrender Spells.");

        // Second Pacte : CD 3
        var secondPacte = AssetDatabase.LoadAssetAtPath<SpellData>($"{SurvivePath}/Second Pacte.asset");
        if (secondPacte != null)
        {
            secondPacte.cooldown = 3;
            EditorUtility.SetDirty(secondPacte);
            patched++;
            Debug.Log("[PatchV2] Second Pacte — CD 3 ✓");
        }
        else Debug.LogWarning("[PatchV2] Second Pacte introuvable — lance d'abord Generate Soulrender Spells.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Patch Soulrender v2",
            $"{patched}/3 assets patchés.\n\n• Tornade de Lames : portée 0, auto-centré\n• Marque du Sang : CD 1 (1×/tour)\n• Second Pacte : CD 3",
            "OK");
    }

    /// <summary>
    /// Met à jour toutes les descriptions et synergies des sorts existants.
    /// Ne recrée pas les assets — met à jour uniquement les textes.
    /// </summary>
    [MenuItem("Oracle/Patch Soulrender Descriptions (v3)")]
    public static void PatchDescriptions()
    {
        int patched = 0;

        void Patch(string path, string desc, string synergy)
        {
            var s = AssetDatabase.LoadAssetAtPath<SpellData>(path);
            if (s == null) { Debug.LogWarning($"[PatchV3] Introuvable : {path}"); return; }
            s.description        = desc;
            s.synergyDescription = synergy;
            EditorUtility.SetDirty(s);
            patched++;
        }

        // ── Attaques ────────────────────────────────────────────────────────
        Patch($"{AttackPath}/Estoc de Sang.asset",
            "Inflige 120 dégâts.",
            "Pas de recharge — utilisable à chaque tour. Chaque 60 dégâts infligés génèrent 1 jauge Vol d'Âme.");

        Patch($"{AttackPath}/Déchirement.asset",
            "Inflige 110 dégâts.\nBrûlure : −35 dégâts/tour pendant 2 tours.\nLes brûlures se cumulent.",
            "Pose le debuff pour déclencher le bonus de Hémorragie Forcée.");

        Patch($"{AttackPath}/Hémorragie Forcée.asset",
            "Inflige 130 dégâts.\n+65 dégâts supplémentaires si la cible subit\nun effet négatif (brûlure, silence, gravité…).",
            "Burst à coupler avec Déchirement ou Marque du Sang.");

        Patch($"{AttackPath}/Frappe Carnivore.asset",
            "Inflige 105 dégâts.\nLe Soulrender récupère immédiatement 45 PV.",
            "Drain en mêlée — soin garanti même à PV max ignoré. Alimente Vol d'Âme.");

        Patch($"{AttackPath}/Tornade de Lames.asset",
            "Inflige 100 dégâts à tous les personnages\nadjacents (rayon 1). N'affecte pas le lanceur.",
            "Dégâts de zone rapprochée — chaque cible touchée alimente Vol d'Âme.");

        // ── Tactiques ───────────────────────────────────────────────────────
        Patch($"{TactPath}/Ruée du Boucher.asset",
            "Se téléporte vers une case libre à portée 1–4.\nIgnore les lignes de vue et les obstacles.",
            "Engagement ou repositionnement rapide sans coût en PM.");

        Patch($"{TactPath}/Brisement.asset",
            "Retire 2 PM à la cible.",
            "Entrave l'ennemi pour l'empêcher de fuir la mêlée.");

        Patch($"{TactPath}/Pression Sanguine.asset",
            "Inflige 70 dégâts.\nGravité (1 tour) : la cible ne peut pas se téléporter\nni être déplacée par un effet de sort.",
            "Verrouille l'ennemi en place pour sécuriser la mêlée.");

        Patch($"{TactPath}/Marque du Sang.asset",
            "Brûlure : −40 dégâts/tour pendant 3 tours.\nLes brûlures se cumulent.\nUtilisable 1 fois par tour (recharge : 1 tour).",
            "Cumul de brûlures à distance — active le bonus conditionnel de Hémorragie Forcée.");

        Patch($"{TactPath}/Fléau.asset",
            "Inflige 60 dégâts à portée 1–5.",
            "Harcèlement à distance ou finisseur léger.");

        // ── Survie ──────────────────────────────────────────────────────────
        Patch($"{SurvivePath}/Carapace de Sang.asset",
            "Bouclier : 180 PV pendant 2 tours.",
            "Protection d'urgence pour absorber un burst ennemi en mêlée.");

        Patch($"{SurvivePath}/Sève de Guerre.asset",
            "Récupère 200 PV.",
            "Sustain principal — à utiliser entre deux phases d'assaut.");

        Patch($"{SurvivePath}/Pas de l'Arène.asset",
            "Gagne 2 PM supplémentaires ce tour.",
            "Comble la distance pour atteindre le corps-à-corps.");

        Patch($"{SurvivePath}/Second Pacte.asset",
            "Bouclier : 120 PV pendant 2 tours.\nÉpines : renvoie 50 dégâts à chaque attaquant\npendant 2 tours.",
            "Contre-attaque passive — punit les ennemis qui frappent en mêlée.");

        Patch($"{SurvivePath}/Retraite Sanglante.asset",
            "Récupère 150 PV.\nSe téléporte vers une case libre à portée 1–4.\nIgnore les lignes de vue.",
            "Évasion d'urgence — soin et repositionnement en une seule action.");

        // ── Âme Lacérée (Enragé) ────────────────────────────────────────────
        Patch($"{BasePath}/AmeLaceree.asset",
            "Inflige 130 dégâts.\nLe Soulrender récupère 70 PV.\nSort Enragé — disponible à 6 jauges Vol d'Âme.\nUtilisable une seule fois par combat.",
            "Sort ultime débloqué au palier Enragé — récompense un gameplay agressif.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Patch Soulrender Descriptions v3",
            $"{patched}/16 assets mis à jour.\n\nToutes les descriptions et synergies sont désormais complètes.",
            "OK");
    }

    /// <summary>
    /// Patche les assets existants : remplace SpellEffectType.Bleed par Brulure
    /// sur Déchirement et Marque du Sang, et met à jour leurs descriptions.
    /// </summary>
    [MenuItem("Oracle/Patch Soulrender Brulure (v4)")]
    public static void PatchBrulure()
    {
        int patched = 0;

        // Déchirement : Bleed 35×2t → Brulure 35×2t
        var dech = AssetDatabase.LoadAssetAtPath<SpellData>($"{AttackPath}/Déchirement.asset");
        if (dech != null)
        {
            foreach (var e in dech.effects)
                if (e.type == SpellEffectType.Bleed) e.type = SpellEffectType.Brulure;
            dech.description        = "Inflige 110 dégâts.\nBrûlure : −35 dégâts/tour pendant 2 tours.\nLes brûlures se cumulent.";
            dech.synergyDescription = "Pose le debuff pour déclencher le bonus de Hémorragie Forcée.";
            EditorUtility.SetDirty(dech);
            patched++;
            Debug.Log("[PatchBrulure] Déchirement — Bleed → Brulure ✓");
        }
        else Debug.LogWarning("[PatchBrulure] Déchirement introuvable — lance d'abord Generate Soulrender Spells.");

        // Marque du Sang : Bleed 40×3t → Brulure 40×3t
        var marque = AssetDatabase.LoadAssetAtPath<SpellData>($"{TactPath}/Marque du Sang.asset");
        if (marque != null)
        {
            foreach (var e in marque.effects)
                if (e.type == SpellEffectType.Bleed) e.type = SpellEffectType.Brulure;
            marque.description        = "Brûlure : −40 dégâts/tour pendant 3 tours.\nLes brûlures se cumulent.\nUtilisable 1 fois par tour (recharge : 1 tour).";
            marque.synergyDescription = "Cumul de brûlures à distance — active le bonus conditionnel de Hémorragie Forcée.";
            EditorUtility.SetDirty(marque);
            patched++;
            Debug.Log("[PatchBrulure] Marque du Sang — Bleed → Brulure ✓");
        }
        else Debug.LogWarning("[PatchBrulure] Marque du Sang introuvable — lance d'abord Generate Soulrender Spells.");

        // Hémorragie Forcée : mise à jour description (saignement → brûlure)
        var hemo = AssetDatabase.LoadAssetAtPath<SpellData>($"{AttackPath}/Hémorragie Forcée.asset");
        if (hemo != null)
        {
            hemo.description = "Inflige 130 dégâts.\n+65 dégâts supplémentaires si la cible subit\nun effet négatif (brûlure, silence, gravité…).";
            EditorUtility.SetDirty(hemo);
            patched++;
            Debug.Log("[PatchBrulure] Hémorragie Forcée — description ✓");
        }
        else Debug.LogWarning("[PatchBrulure] Hémorragie Forcée introuvable — lance d'abord Generate Soulrender Spells.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Patch Soulrender Brûlure v4",
            $"{patched}/3 assets patchés.\n\n• Déchirement : Bleed → Brûlure\n• Marque du Sang : Bleed → Brûlure\n• Hémorragie Forcée : description mise à jour",
            "OK");
    }

    [MenuItem("Oracle/Generate Soulrender Spells")]
    public static void GenerateAll()
    {
        EnsureFolders();

        int created = 0;

        var passive  = CreatePassive(ref created);
        var attacks  = CreateAttaques(ref created);
        var tactics  = CreateTactiques(ref created);
        var survives = CreateSurvie(ref created);
        var ameLac   = CreateAmeLaceree(ref created);

        InjectIntoClassData(passive, attacks, tactics, survives, ameLac);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Nymora — Soulrender Factory",
            $"{created} assets créés.\n\nSoulrender est prêt au combat.",
            "Let's go !");

        Debug.Log($"[NymoraSoulrenderFactory] {created} assets générés.");
    }

    // ── Dossiers ─────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        CreateFolder("Assets/_Game/ScriptableObjects/Spells", "Soulrender");
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
        string path = $"{BasePath}/Soulrender_VolDame.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PassiveData>(path);
        if (existing != null) return existing;

        var p = ScriptableObject.CreateInstance<PassiveData>();
        p.passiveName        = "Vol d'Âme";
        p.passiveType        = PassiveType.VolDAme;
        p.trigger            = PassiveTrigger.Permanent;
        p.effectValue        = 0f;
        p.procChance         = 1f;
        p.conditionThreshold = 0;
        p.description        = "+1 jauge par 60 dégâts infligés. Bonus dégâts croissant par palier. " +
                               "Au palier Enragé, débloque Âme Lacérée.";

        AssetDatabase.CreateAsset(p, path);
        created++;
        return p;
    }

    // ── Attaques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateAttaques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Estoc de Sang : 3PA, mêlée r1, 120 dmg
        {
            var s = Spell("Estoc de Sang", pa: 3, cd: 0,
                melee: true, rMin: 1, rMax: 1,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 120 dégâts.",
                synergy: "Pas de recharge — utilisable à chaque tour. Chaque 60 dégâts infligés génèrent 1 jauge Vol d'Âme.");
            s.effects.Add(E(SpellEffectType.Damage, 120));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 2 — Déchirement : 4PA, r1-2, 110 dmg + Brulure 35×2t
        {
            var s = Spell("Déchirement", pa: 4, cd: 0,
                melee: false, rMin: 1, rMax: 2,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 110 dégâts.\nBrûlure : −35 dégâts/tour pendant 2 tours.\nLes brûlures se cumulent.",
                synergy: "Pose le debuff pour déclencher le bonus de Hémorragie Forcée.");
            s.effects.Add(E(SpellEffectType.Damage, 110));
            s.effects.Add(E(SpellEffectType.Brulure, 35, duration: 2));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 3 — Hémorragie Forcée : 4PA, r1-3, 130 dmg + 65 si cible a debuff
        {
            var s = Spell("Hémorragie Forcée", pa: 4, cd: 0,
                melee: false, rMin: 1, rMax: 3,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 130 dégâts.\n+65 dégâts supplémentaires si la cible subit\nun effet négatif (saignement, silence, gravité…).",
                synergy: "Burst à coupler avec Déchirement ou Marque du Sang.");
            s.effects.Add(E(SpellEffectType.Damage, 130));
            s.effects.Add(E(SpellEffectType.Damage, 65, cond: SpellCondition.TargetHasDebuff));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 4 — Frappe Carnivore : 3PA, mêlée r1, 105 dmg + HealCaster 45
        {
            var s = Spell("Frappe Carnivore", pa: 3, cd: 0,
                melee: true, rMin: 1, rMax: 1,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 105 dégâts.\nLe Soulrender récupère immédiatement 45 PV.",
                synergy: "Drain en mêlée — soin garanti + alimente Vol d'Âme.");
            s.effects.Add(E(SpellEffectType.Damage, 105));
            s.effects.Add(E(SpellEffectType.HealCaster, 45));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        // 5 — Tornade de Lames : 5PA, Circle r1 centré sur soi (portée 0), 100 dmg, CD 2
        {
            var s = Spell("Tornade de Lames", pa: 5, cd: 2,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Circle, aoe: 1,
                cat: SpellDeckCategory.Attack,
                desc: "Inflige 100 dégâts à tous les personnages\nadjacents (rayon 1). N'affecte pas le lanceur.",
                synergy: "Dégâts de zone rapprochée — chaque cible touchée alimente Vol d'Âme.");
            s.excludeCasterFromHarmfulAoE = true;
            s.effects.Add(E(SpellEffectType.Damage, 100));
            list[i++] = SaveSpell(s, AttackPath, ref created);
        }

        return list;
    }

    // ── Tactiques ─────────────────────────────────────────────────────────────

    static SpellData[] CreateTactiques(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Ruée du Boucher : 3PA, FreeCell r1-4, Teleport, CD 2, ignoresLoS
        {
            var s = Spell("Ruée du Boucher", pa: 3, cd: 2,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.FreeCell, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Se téléporte vers une case libre à portée 1–4.\nIgnore les lignes de vue et les obstacles.",
                synergy: "Engagement ou repositionnement rapide sans coût en PM.");
            s.ignoresLineOfSight = true;
            s.effects.Add(E(SpellEffectType.Teleport, 0));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 2 — Brisement : 2PA, r1-3, RemovePM 2, CD 1
        {
            var s = Spell("Brisement", pa: 2, cd: 1,
                melee: false, rMin: 1, rMax: 3,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Retire 2 PM à la cible.",
                synergy: "Entrave l'ennemi pour l'empêcher de fuir la mêlée.");
            s.effects.Add(E(SpellEffectType.RemovePM, 2));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 3 — Pression Sanguine : 3PA, r1-3, 70 dmg + GravityDebuff 1t, CD 2
        {
            var s = Spell("Pression Sanguine", pa: 3, cd: 2,
                melee: false, rMin: 1, rMax: 3,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Inflige 70 dégâts.\nGravité (1 tour) : la cible ne peut pas se téléporter\nni être déplacée par un effet de sort.",
                synergy: "Verrouille l'ennemi en place pour sécuriser la mêlée.");
            s.effects.Add(E(SpellEffectType.Damage, 70));
            s.effects.Add(E(SpellEffectType.GravityDebuff, 0, duration: 1));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 4 — Marque du Sang : 2PA, r1-4, Brulure 40×3t, CD 1 (1×/tour par ennemi)
        {
            var s = Spell("Marque du Sang", pa: 2, cd: 1,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Brûlure : −40 dégâts/tour pendant 3 tours.\nLes brûlures se cumulent.\nUtilisable 1 fois par tour (recharge : 1 tour).",
                synergy: "Cumul de brûlures à distance — active le bonus de Hémorragie Forcée.");
            s.effects.Add(E(SpellEffectType.Brulure, 40, duration: 3));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        // 5 — Fléau : 2PA, r1-5, 60 dmg, CD 0
        {
            var s = Spell("Fléau", pa: 2, cd: 0,
                melee: false, rMin: 1, rMax: 5,
                zone: ZoneType.SingleTarget, aoe: 0,
                cat: SpellDeckCategory.Tactic,
                desc: "Inflige 60 dégâts à portée 1–5.",
                synergy: "Harcèlement à distance ou finisseur léger.");
            s.effects.Add(E(SpellEffectType.Damage, 60));
            list[i++] = SaveSpell(s, TactPath, ref created);
        }

        return list;
    }

    // ── Survie ────────────────────────────────────────────────────────────────

    static SpellData[] CreateSurvie(ref int created)
    {
        var list = new SpellData[5];
        int i = 0;

        // 1 — Carapace de Sang : 2PA, Self, Shield 180×2t, CD 3
        {
            var s = Spell("Carapace de Sang", pa: 2, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Bouclier : 180 PV pendant 2 tours.",
                synergy: "Protection d'urgence pour absorber un burst ennemi en mêlée.");
            s.effects.Add(E(SpellEffectType.Shield, 180, duration: 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 2 — Sève de Guerre : 3PA, Self, Heal 200, CD 3
        {
            var s = Spell("Sève de Guerre", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Récupère 200 PV.",
                synergy: "Sustain principal — à utiliser entre deux phases d'assaut.");
            s.effects.Add(E(SpellEffectType.Heal, 200));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 3 — Pas de l'Arène : 2PA, Self, BonusPM 2, CD 1
        {
            var s = Spell("Pas de l'Arène", pa: 2, cd: 1,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Gagne 2 PM supplémentaires ce tour.",
                synergy: "Comble la distance pour atteindre le corps-à-corps.");
            s.effects.Add(E(SpellEffectType.BonusPM, 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 4 — Second Pacte : 3PA, Self, Shield 120×2t + Thorns 50×2t, CD 3
        {
            var s = Spell("Second Pacte", pa: 3, cd: 3,
                melee: false, rMin: 0, rMax: 0,
                zone: ZoneType.Self, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Bouclier : 120 PV pendant 2 tours.\nÉpines : renvoie 50 dégâts à chaque attaquant\npendant 2 tours.",
                synergy: "Contre-attaque passive — punit les ennemis qui frappent en mêlée.");
            s.effects.Add(E(SpellEffectType.Shield, 120, duration: 2));
            s.effects.Add(E(SpellEffectType.Thorns, 50, duration: 2));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        // 5 — Retraite Sanglante : 4PA, FreeCell r1-4, Heal 150 + Teleport, CD 5
        {
            var s = Spell("Retraite Sanglante", pa: 4, cd: 5,
                melee: false, rMin: 1, rMax: 4,
                zone: ZoneType.FreeCell, aoe: 0,
                cat: SpellDeckCategory.Survival,
                desc: "Récupère 150 PV.\nSe téléporte vers une case libre à portée 1–4.\nIgnore les lignes de vue.",
                synergy: "Évasion d'urgence — soin et repositionnement en une seule action.");
            s.ignoresLineOfSight = true;
            s.effects.Add(E(SpellEffectType.Heal, 150));
            s.effects.Add(E(SpellEffectType.Teleport, 0));
            list[i++] = SaveSpell(s, SurvivePath, ref created);
        }

        return list;
    }

    // ── Sort Enragé — Âme Lacérée ─────────────────────────────────────────────

    static SpellData CreateAmeLaceree(ref int created)
    {
        string path = $"{BasePath}/AmeLaceree.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SpellData>(path);
        if (existing != null) return existing;

        var s = Spell("Âme Lacérée", pa: 1, cd: 0,
            melee: false, rMin: 1, rMax: 1,
            zone: ZoneType.SingleTarget, aoe: 0,
            cat: SpellDeckCategory.Attack,
            desc: "Inflige 130 dégâts.\nLe Soulrender récupère 70 PV.\nSort Enragé — disponible à 6 jauges Vol d'Âme.\nUtilisable une seule fois par combat.",
            synergy: "Sort ultime débloqué au palier Enragé — récompense un gameplay agressif.");
        s.effects.Add(E(SpellEffectType.Damage, 130));
        s.effects.Add(E(SpellEffectType.HealCaster, 70));

        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }

    // ── Injection dans ClassData ──────────────────────────────────────────────

    static void InjectIntoClassData(PassiveData passive,
        SpellData[] attacks, SpellData[] tactics, SpellData[] survives,
        SpellData ameLaceree)
    {
        var classData = AssetDatabase.LoadAssetAtPath<ClassData>(ClassPath);
        if (classData == null)
        {
            Debug.LogError($"[NymoraSoulrenderFactory] ClassData introuvable : {ClassPath}\n" +
                           "Lance d'abord Oracle > Generate Nymora Classes.");
            return;
        }

        classData.passiveData  = passive;
        classData.enragedSpell = ameLaceree;

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
        Debug.Log("[NymoraSoulrenderFactory] ClassData Soulrender mise à jour : passif + 15 sorts + Âme Lacérée injectés.");
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
            Debug.Log($"[NymoraSoulrenderFactory] Ignoré (déjà existant) : {s.spellName}");
            return existing;
        }
        AssetDatabase.CreateAsset(s, path);
        created++;
        return s;
    }
}
#endif

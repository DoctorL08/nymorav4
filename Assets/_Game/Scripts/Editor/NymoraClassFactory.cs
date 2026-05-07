#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Génère les 5 assets ClassData de Nymora avec lore et passif pré-remplis.
/// Menu : Oracle > Generate Nymora Classes
/// Spells : à assigner manuellement ou via NymoraGhostraFactory après Phase 4.
/// </summary>
public static class NymoraClassFactory
{
    private const string ClassPath    = "Assets/_Game/ScriptableObjects/Classes";
    private const string RegistryDir  = "Assets/_Game/Resources/NymoraClasses";
    private const string RegistryPath = "Assets/_Game/Resources/NymoraClasses/ClassRegistry.asset";

    [MenuItem("Oracle/Generate Nymora Classes")]
    public static void GenerateAll()
    {
        EnsureFolder();

        int created = 0;

        var ghostra    = BuildGhostra();
        var soulrender = BuildSoulrender();
        var nightseer  = BuildNightseer();
        var colossar   = BuildColossar();
        var necram     = BuildNecram();

        created += Save(ghostra);
        created += Save(soulrender);
        created += Save(nightseer);
        created += Save(colossar);
        created += Save(necram);

        // Créer / mettre à jour le ClassRegistry dans Resources
        EnsureResourcesFolder();
        var registry = AssetDatabase.LoadAssetAtPath<NymoraClassRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<NymoraClassRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }
        registry.classes.Clear();
        registry.classes.Add(AssetDatabase.LoadAssetAtPath<ClassData>($"{ClassPath}/Ghostra.asset"));
        registry.classes.Add(AssetDatabase.LoadAssetAtPath<ClassData>($"{ClassPath}/Soulrender.asset"));
        registry.classes.Add(AssetDatabase.LoadAssetAtPath<ClassData>($"{ClassPath}/Nightseer.asset"));
        registry.classes.Add(AssetDatabase.LoadAssetAtPath<ClassData>($"{ClassPath}/Colossar.asset"));
        registry.classes.Add(AssetDatabase.LoadAssetAtPath<ClassData>($"{ClassPath}/Necram.asset"));
        EditorUtility.SetDirty(registry);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Nymora — Classes",
            $"{created} assets de classe créés dans {ClassPath}.\nClassRegistry mis à jour dans Resources.",
            "OK");

        Debug.Log($"[NymoraClassFactory] {created} classes générées + ClassRegistry mis à jour.");
    }

    // =========================================================
    // GHOSTRA
    // =========================================================
    static ClassData BuildGhostra()
    {
        var c = ScriptableObject.CreateInstance<ClassData>();
        c.classId     = NymoraClassId.Ghostra;
        c.displayName = "Ghostra";
        c.emoji       = "👻";
        c.lore =
            "Les Ghostras sont des anomalies chroniques — des individus dont l'existence a été effacée mais dont l'écho refuse de disparaître.\n\n" +
            "Trahis et assassinés, ils reviennent sous forme de silhouettes floues pour arracher ce qu'on leur a pris.\n\n" +
            "Leur monde est désaturé, sans couleur, où ne subsistent que des murmures glaçants et des images rémanentes de ce qui fut. " +
            "Des personnages aux contours instables, comme un bug graphique permanent, devenant des ombres noires et floues qui font grésiller la réalité.";

        c.passiveName    = "Présence Funeste";
        c.passiveMechanic = "+1 jauge à chaque coup porté dans le dos de l'ennemi. " +
                            "Bonus fixe par sort dorsal selon le palier actif. " +
                            "Le bonus FIXE empêche l'empilement burst tout en rendant chaque dorsal puissant individuellement.";

        c.stageBase = new PassiveStage
        {
            label             = "Base",
            jaugeMin          = 0,
            jaugeMax          = 3,
            effectDescription = "+35 dégâts fixes par sort dorsal."
        };
        c.stageEveille = new PassiveStage
        {
            label             = "Éveillé",
            jaugeMin          = 4,
            jaugeMax          = 7,
            effectDescription = "+55 dégâts fixes par sort dorsal."
        };
        c.stageEnrage = new PassiveStage
        {
            label             = "Enragé",
            jaugeMin          = 8,
            jaugeMax          = -1,
            effectDescription = "+75 dégâts fixes par sort dorsal. Débloque le sort signature Saignée."
        };

        c.enragedSpellName        = "Saignée";
        c.enragedSpellDescription = "1 PA — Portée 1 — 200 dégâts + Plaie Ouverte (40 HP/tour × 2 tours). Utilisable qu'une fois en combat.";

        return c;
    }

    // =========================================================
    // SOULRENDER
    // =========================================================
    static ClassData BuildSoulrender()
    {
        var c = ScriptableObject.CreateInstance<ClassData>();
        c.classId     = NymoraClassId.Soulrender;
        c.displayName = "Soulrender";
        c.emoji       = "🩸";
        c.lore =
            "Les Soulrenders sont les héritiers d'un pacte de guerre oublié, où le sang a cessé d'être un fluide vital pour devenir une arme brute.\n\n" +
            "Nés dans les arènes de souffrance, ils ne connaissent la paix que dans l'hémorragie.\n\n" +
            "Une atmosphère de fer rouillé et de vapeur chaude. Leur présence sature l'air d'une odeur de cuivre et du son sourd d'un cœur qui tambourine contre la réalité. " +
            "Des silhouettes nerveuses entourées d'une brume rouge pixelisée qui semble s'échapper de leurs pores comme une sueur écarlate.";

        c.passiveName    = "Vol d'Âme";
        c.passiveMechanic = "+1 jauge par 60 dégâts infligés. " +
                            "Chaque palier augmente le bonus de dégâts et accélère la montée de jauge. " +
                            "Les sorts offensifs génèrent +1 jauge supplémentaire au palier Éveillé.";

        c.stageBase = new PassiveStage
        {
            label             = "Base",
            jaugeMin          = 0,
            jaugeMax          = 2,
            effectDescription = "+5% dégâts."
        };
        c.stageEveille = new PassiveStage
        {
            label             = "Éveillé",
            jaugeMin          = 3,
            jaugeMax          = 5,
            effectDescription = "+8% dégâts. Sorts offensifs génèrent +1 jauge."
        };
        c.stageEnrage = new PassiveStage
        {
            label             = "Enragé",
            jaugeMin          = 6,
            jaugeMax          = -1,
            effectDescription = "+12% dégâts. Débloque Âme Lacérée."
        };

        c.enragedSpellName        = "Âme Lacérée";
        c.enragedSpellDescription = "1 PA — Portée 1 — 130 dégâts. Le lanceur récupère 70 HP. 1x/match.";

        return c;
    }

    // =========================================================
    // NIGHTSEER
    // =========================================================
    static ClassData BuildNightseer()
    {
        var c = ScriptableObject.CreateInstance<ClassData>();
        c.classId     = NymoraClassId.Nightseer;
        c.displayName = "Nightseer";
        c.emoji       = "🏹";
        c.lore =
            "Ancienne lignée d'éclaireurs ayant traversé la 'faille sans lune', leur regard est désormais figé sur des horizons invisibles aux mortels.\n\n" +
            "Ils chassent pour nourrir un vide intérieur que seule l'obscurité peut combler.\n\n" +
            "Un silence glacial, troué par le sifflement d'un vent spectral. Autour d'eux, la lumière 'meurt' en petits scintillements violets, comme si l'espace-temps se déchirait. " +
            "Des formes longilignes et vaporeuses, laissant derrière elles une poussière d'étoiles sombre qui évoque une nuit éternelle.";

        c.passiveName    = "Prédateur";
        c.passiveMechanic = "Poser une embûche = +0.5 jauge. Déclencher une embûche = +1 jauge. " +
                            "Chaque palier augmente les dégâts des embûches et débloque des bonus de mobilité.";

        c.stageBase = new PassiveStage
        {
            label             = "Base",
            jaugeMin          = 0,
            jaugeMax          = 2,
            effectDescription = "+10% dégâts d'embûche. Applique Traqué si embûche déclenchée."
        };
        c.stageEveille = new PassiveStage
        {
            label             = "Éveillé",
            jaugeMin          = 3,
            jaugeMax          = 4,
            effectDescription = "+20% dégâts d'embûche. -1 PM/embûche déclenchée."
        };
        c.stageEnrage = new PassiveStage
        {
            label             = "Enragé",
            jaugeMin          = 5,
            jaugeMax          = -1,
            effectDescription = "+30% dégâts d'embûche. Débloque Traquenard."
        };

        c.enragedSpellName        = "Traquenard";
        c.enragedSpellDescription = "1 PA — Portée 3 — Téléportation case adjacente + 170 dégâts + Paralysie. 1x/match.";

        return c;
    }

    // =========================================================
    // COLOSSAR
    // =========================================================
    static ClassData BuildColossar()
    {
        var c = ScriptableObject.CreateInstance<ClassData>();
        c.classId     = NymoraClassId.Colossar;
        c.displayName = "Colossar";
        c.emoji       = "🛡️";
        c.lore =
            "Les Colossars ne sont pas nés, ils ont été sculptés par la pression des montagnes et le deuil des cités englouties.\n\n" +
            "Derniers piliers d'une civilisation de pierre, ils portent l'inertie du monde sur leurs épaules.\n\n" +
            "Un univers de fracas et de gravité lourde. Leur simple respiration sonne comme du granit qu'on broie, et l'espace semble s'affaisser sous leur poids. " +
            "Des colosses massifs dont le corps s'effrite en pixels de roche, entourés de débris en lévitation défiant les lois de la physique.";

        c.passiveName    = "Absorption";
        c.passiveMechanic = "+1 jauge par 40 dégâts reçus. " +
                            "Chaque palier réduit les dégâts subis et accélère la montée de jauge via les sorts défensifs.";

        c.stageBase = new PassiveStage
        {
            label             = "Base",
            jaugeMin          = 0,
            jaugeMax          = 2,
            effectDescription = "-5% dégâts subis."
        };
        c.stageEveille = new PassiveStage
        {
            label             = "Éveillé",
            jaugeMin          = 3,
            jaugeMax          = 5,
            effectDescription = "-15% dégâts subis. Sorts défensifs génèrent +1 jauge."
        };
        c.stageEnrage = new PassiveStage
        {
            label             = "Enragé",
            jaugeMin          = 6,
            jaugeMax          = -1,
            effectDescription = "-20% dégâts subis. Débloque Reflet Punitif."
        };

        c.enragedSpellName        = "Reflet Punitif";
        c.enragedSpellDescription = "1 PA — AoE 2 cases autour — 130 dégâts + Trauma (-2 PA prochain tour). 1x/match.";

        return c;
    }

    // =========================================================
    // NECRAM
    // =========================================================
    static ClassData BuildNecram()
    {
        var c = ScriptableObject.CreateInstance<ClassData>();
        c.classId     = NymoraClassId.Necram;
        c.displayName = "Necram";
        c.emoji       = "☠️";
        c.lore =
            "Pour cette race, la vie n'est qu'une infection temporaire. Ils se voient comme les jardiniers d'une décomposition sacrée.\n\n" +
            "Issus des marais de putréfaction, ils ont troqué leur humanité contre la symbiose avec des parasites ancestraux.\n\n" +
            "Une ambiance de bourdonnement incessant et de pourriture fertile. Leur monde est celui des spores vertes, des moisissures rampantes et des râles d'agonie étouffés. " +
            "Des silhouettes voûtées, presque organiques, drapées dans un nuage toxique dense où s'agitent des milliers de mouches pixelisées.";

        c.passiveName    = "Toxine";
        c.passiveMechanic = "+1 jauge par marque de Venin appliquée. " +
                            "Chaque sort offensif applique des marques qui tick des dégâts par tour (stack infini). " +
                            "Les DoT de Venin ignorent la réduction de dégâts du Colossar.";

        c.stageBase = new PassiveStage
        {
            label             = "Base",
            jaugeMin          = 0,
            jaugeMax          = 3,
            effectDescription = "Chaque sort offensif applique 1 marque (22 dmg/tour × 3 tours, stack infini)."
        };
        c.stageEveille = new PassiveStage
        {
            label             = "Éveillé",
            jaugeMin          = 4,
            jaugeMax          = 6,
            effectDescription = "+15% dégâts DoT venin (ticks 22→25). Regen 1 HP/marque. +2 jauges/marque."
        };
        c.stageEnrage = new PassiveStage
        {
            label             = "Enragé",
            jaugeMin          = 7,
            jaugeMax          = -1,
            effectDescription = "+30% dégâts DoT venin (ticks 22→29). Débloque Virus Fatal."
        };

        c.enragedSpellName        = "Virus Fatal";
        c.enragedSpellDescription = "1 PA — Portée 3 — Déclenche immédiatement toutes les marques sur la cible (avec multiplicateur Enragé) puis les retire. 1x/match.";

        return c;
    }

    // =========================================================
    // UTILITAIRES
    // =========================================================
    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(ClassPath))
            AssetDatabase.CreateFolder("Assets/_Game/ScriptableObjects", "Classes");
    }

    static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Resources"))
            AssetDatabase.CreateFolder("Assets/_Game", "Resources");
        if (!AssetDatabase.IsValidFolder(RegistryDir))
            AssetDatabase.CreateFolder("Assets/_Game/Resources", "NymoraClasses");
    }

    static int Save(ClassData c)
    {
        string path = $"{ClassPath}/{c.displayName}.asset";
        if (AssetDatabase.LoadAssetAtPath<ClassData>(path) != null)
        {
            Debug.Log($"[NymoraClassFactory] Ignoré (déjà existant) : {c.displayName}");
            return 0;
        }
        AssetDatabase.CreateAsset(c, path);
        return 1;
    }
}
#endif

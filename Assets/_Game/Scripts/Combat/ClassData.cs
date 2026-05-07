using UnityEngine;
using System.Collections.Generic;

public enum NymoraClassId
{
    Ghostra,
    Soulrender,
    Nightseer,
    Colossar,
    Necram
}

[System.Serializable]
public class PassiveStage
{
    public string label;
    public int    jaugeMin;
    public int    jaugeMax;   // -1 = infini (palier Enragé)
    [TextArea(2, 4)]
    public string effectDescription;
}

[CreateAssetMenu(fileName = "NewClass", menuName = "Nymora/Class Data")]
public class ClassData : ScriptableObject
{
    [Header("Identité")]
    public NymoraClassId classId;
    public string        displayName;
    public string        emoji;
    [Tooltip("Clé du dossier Resources/Characters/GifSource/{characterKey}/ (ex: Ghostra, Soulrender).")]
    public string        characterKey;
    [TextArea(5, 12)]
    public string        lore;

    [Header("Passif évolutif — Présentation")]
    public string passiveName;
    [TextArea(2, 4)]
    public string passiveMechanic;

    [Header("Paliers")]
    public PassiveStage stageBase;
    public PassiveStage stageEveille;
    public PassiveStage stageEnrage;

    [Header("Sort Enragé (1x/match)")]
    public string       enragedSpellName;
    [TextArea(2, 4)]
    public string       enragedSpellDescription;
    public SpellData    enragedSpell;

    [Header("Passif — Asset runtime (lié à la classe)")]
    public PassiveData passiveData;

    [Header("Sorts — Attaque (5)")]
    public List<SpellData> attackSpells   = new List<SpellData>();

    [Header("Sorts — Tactique (5)")]
    public List<SpellData> tacticSpells   = new List<SpellData>();

    [Header("Sorts — Survie (5)")]
    public List<SpellData> survivalSpells = new List<SpellData>();

    public List<SpellData> GetAllSpells()
    {
        var all = new List<SpellData>();
        all.AddRange(attackSpells);
        all.AddRange(tacticSpells);
        all.AddRange(survivalSpells);
        return all;
    }
}

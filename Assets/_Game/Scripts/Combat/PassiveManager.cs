using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TacticalCharacter))]
public class PassiveManager : MonoBehaviour
{
    private TacticalCharacter character;
    public PassiveData activePassive;

    /// <summary>
    /// Classe propriétaire de ce personnage (joueur ou adversaire).
    /// Sert à résoudre le sort enragé sans passer par HubManager.SelectedClass,
    /// ce qui permet à l'adversaire d'avoir sa propre classe indépendamment du joueur.
    /// Assigner via CombatInitializer.PlaceCharacter.
    /// </summary>
    [HideInInspector] public ClassData ownerClass;

    /// <summary>Sort enragé de ce personnage, résolu depuis ownerClass en priorité.</summary>
    SpellData EnragedSpell => ownerClass?.enragedSpell
                           ?? HubManager.Instance?.SelectedClass?.enragedSpell;

    private bool castSpellThisTurn  = false;
    private bool masseCritiqueBonus = false;
    private int  bouclierHitsReceived = 0;

    // ── Ghostra — Présence Funeste ───────────────────────────────────────
    private int  _ghostraJauges           = 0;
    private bool _ghostraDorsalThisSpell  = false;
    private bool _ghostraEnrageUnlocked   = false;

    public int GhostrazJauges => _ghostraJauges;
    public int GhostraPalier  => _ghostraJauges >= 8 ? 2 : _ghostraJauges >= 4 ? 1 : 0;

    int GhostraDorsalBonus => _ghostraJauges >= 8 ? 75 : _ghostraJauges >= 4 ? 55 : 35;

    // ── Soulrender — Vol d'Âme ───────────────────────────────────────────
    private int  _soulrenderJauges         = 0;
    private int  _soulrenderDamageAccum    = 0;  // dégâts cumulés (carry-over entre sorts)
    private int  _soulrenderPendingDamage  = 0;  // dégâts du sort en cours (reset après HandleSpellCast)
    private bool _soulrenderEnrageUnlocked = false;

    public int SoulrenderJauges => _soulrenderJauges;
    public int SoulrenderPalier => _soulrenderJauges >= 6 ? 2 : _soulrenderJauges >= 3 ? 1 : 0;

    float SoulrenderDamageMult => _soulrenderJauges >= 6 ? 0.20f : _soulrenderJauges >= 3 ? 0.10f : 0.05f;

    // ── Necram — Toxine ──────────────────────────────────────────────────
    private int  _necramJauges         = 0;
    private bool _necramEnrageUnlocked = false;

    public int NecramJauges => _necramJauges;
    public int NecramPalier => _necramJauges >= 7 ? 2 : _necramJauges >= 4 ? 1 : 0;

    /// <summary>Valeur du tick venin selon le palier Toxine : 22 (Base) / 25 (Éveillé) / 29 (Enragé).</summary>
    public int GetVeninTickValue()
    {
        if (activePassive?.passiveType != PassiveType.Toxine) return 22;
        return NecramPalier >= 2 ? 29 : NecramPalier >= 1 ? 25 : 22;
    }

    /// <summary>Multiplicateur sur les dégâts de détonation venin : x1 / x1.15 / x1.30.</summary>
    public float GetVeninBurstMult()
    {
        if (activePassive?.passiveType != PassiveType.Toxine) return 1f;
        return NecramPalier >= 2 ? 1.50f : NecramPalier >= 1 ? 1.15f : 1f;
    }

    /// <summary>Auto-marque Toxine : appelé par SpellResolver sur chaque hit offensif (1 fois par cible par sort).
    /// Retourne le tickVal de la marque appliquée, ou 0 si ce passif n'est pas Toxine.</summary>
    public int NotifyOffensiveHit(TacticalCharacter target)
    {
        if (activePassive?.passiveType != PassiveType.Toxine) return 0;
        if (target == null || target == character) return 0;
        int tickVal = GetVeninTickValue();
        target.AddStatusEffect(new StatusEffect(StatusEffectType.Venin, tickVal, 3, true));
        IncrementNecramJauges(1);
        return tickVal;
    }

    /// <summary>Appelé par SpellResolver pour les effets ApplyVenin explicites (Inoculation, Contagion…).</summary>
    public void NotifyVeninApplied(int count)
    {
        if (activePassive?.passiveType != PassiveType.Toxine) return;
        IncrementNecramJauges(count);
    }

    private void IncrementNecramJauges(int count)
    {
        int jaugesPerMark = NecramPalier >= 1 ? 2 : 1;
        _necramJauges = Mathf.Min(7, _necramJauges + count * jaugesPerMark);

        // Éveillé regen : +1 PV par marque appliquée
        if (NecramPalier >= 1)
            character.Heal(count, suppressFloatingText: true);

        string palierName = NecramPalier == 2 ? "ENRAGÉ" : NecramPalier == 1 ? "Éveillé" : "Base";
        Debug.Log($"[Necram] +{count * jaugesPerMark} jauge(s) — {_necramJauges}/7 — Palier : {palierName}");

        if (NecramPalier == 2 && !_necramEnrageUnlocked)
        {
            _necramEnrageUnlocked = true;
            var enragedSpell = EnragedSpell;
            if (enragedSpell != null)
            {
                character.AddRuntimeSpell(enragedSpell);
                Debug.Log("[Necram] Virus Fatal ajouté en main.");
            }
            else
                Debug.LogWarning("[Necram] enragedSpell non assigné dans ClassData Necram !");
        }
    }

    // ── Interface Nymora générique (lue par GaugeHUDWidget) ──────────────
    public bool IsNymoraPassive => activePassive?.passiveType == PassiveType.PresenceFuneste
                                || activePassive?.passiveType == PassiveType.VolDAme
                                || activePassive?.passiveType == PassiveType.Toxine;
    public int NymoraJauges    => activePassive?.passiveType == PassiveType.VolDAme ? _soulrenderJauges
                                : activePassive?.passiveType == PassiveType.Toxine  ? _necramJauges
                                : _ghostraJauges;
    public int NymoraMaxJauges => activePassive?.passiveType == PassiveType.VolDAme ? 6
                                : activePassive?.passiveType == PassiveType.Toxine  ? 7
                                : 8;
    public int NymoPalier      => activePassive?.passiveType == PassiveType.VolDAme ? SoulrenderPalier
                                : activePassive?.passiveType == PassiveType.Toxine  ? NecramPalier
                                : GhostraPalier;

    void Awake()
    {
        character = GetComponent<TacticalCharacter>();
    }

    void Start()
    {
        character.OnSpellCast += HandleSpellCast;
        character.OnTurnStart_Passive += HandleTurnStart;
        character.OnTurnEnd_Passive   += HandleTurnEnd;

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnEnd += HandleAnyTurnEnd;
    }

    void OnDestroy()
    {
        character.OnSpellCast         -= HandleSpellCast;
        character.OnTurnStart_Passive -= HandleTurnStart;
        character.OnTurnEnd_Passive   -= HandleTurnEnd;

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnEnd -= HandleAnyTurnEnd;
    }

    public void SetPassive(PassiveData passive) => activePassive = passive;

    public int ModifyOutgoingDamage(int damage, SpellData spell, TacticalCharacter target, int distance)
    {
        if (activePassive == null || character.stats == null) return damage;

        switch (activePassive.passiveType)
        {
            case PassiveType.Berserker:
            {
                int th = Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.conditionThreshold / 100f)));
                if (character.CurrentHP <= th)
                    damage = Mathf.RoundToInt(damage * (1f + activePassive.effectValue));
                break;
            }

            case PassiveType.MaitreArme:
                if (spell != null && spell.isMeleeOnly)
                    damage += Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
                break;

            case PassiveType.Sniper:
                if (spell != null && !spell.isMeleeOnly && distance > activePassive.conditionThreshold)
                    damage += Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
                break;

            case PassiveType.MasseCritique:
                if (masseCritiqueBonus)
                {
                    damage = Mathf.RoundToInt(damage * (1f + activePassive.effectValue));
                    masseCritiqueBonus = false;
                }
                break;

            case PassiveType.PresenceFuneste:
            {
                if (target == null) break;
                bool isDorsal = SpellResolver.IsAttackerBehindTarget(character, target);
                if (!isDorsal) break;
                damage += GhostraDorsalBonus;
                _ghostraDorsalThisSpell = true;
                break;
            }

            case PassiveType.VolDAme:
            {
                damage = Mathf.RoundToInt(damage * (1f + SoulrenderDamageMult));
                _soulrenderPendingDamage += damage;
                break;
            }
        }
        return damage;
    }

    /// <summary>Même logique que ModifyOutgoingDamage mais sans effets de bord — sûr pour la prévisualisation.</summary>
    public int ModifyOutgoingDamagePreview(int damage, SpellData spell, TacticalCharacter target, int distance)
    {
        if (activePassive == null || character.stats == null) return damage;

        switch (activePassive.passiveType)
        {
            case PassiveType.Berserker:
            {
                int th = Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.conditionThreshold / 100f)));
                if (character.CurrentHP <= th)
                    damage = Mathf.RoundToInt(damage * (1f + activePassive.effectValue));
                break;
            }
            case PassiveType.MaitreArme:
                if (spell != null && spell.isMeleeOnly)
                    damage += Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
                break;
            case PassiveType.Sniper:
                if (spell != null && !spell.isMeleeOnly && distance > activePassive.conditionThreshold)
                    damage += Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
                break;
            case PassiveType.MasseCritique:
                if (masseCritiqueBonus)
                    damage = Mathf.RoundToInt(damage * (1f + activePassive.effectValue));
                break;
            case PassiveType.PresenceFuneste:
            {
                if (target == null) break;
                if (SpellResolver.IsAttackerBehindTarget(character, target))
                    damage += GhostraDorsalBonus;
                break;
            }
            case PassiveType.VolDAme:
                damage = Mathf.RoundToInt(damage * (1f + SoulrenderDamageMult));
                break;
        }
        return damage;
    }

    public int ModifyIncomingDamage(int damage, TacticalCharacter attacker)
    {
        if (activePassive == null || damage <= 0) return damage;

        switch (activePassive.passiveType)
        {
            case PassiveType.Evasif:
            {
                bool isRanged = attacker != null && !IsAdjacent(attacker);
                if (isRanged && Random.value < activePassive.procChance)
                    damage = Mathf.RoundToInt(damage * 0.5f);
                break;
            }

            case PassiveType.BouclierHasardeux:
                bouclierHitsReceived++;
                if (bouclierHitsReceived % 5 == 0)
                    return 0;
                break;
        }
        return damage;
    }

    private void HandleSpellCast(SpellData spell)
    {
        castSpellThisTurn = true;

        if (activePassive?.passiveType == PassiveType.MasseCritique)
            if (Random.value < activePassive.procChance)
                masseCritiqueBonus = true;

        if (activePassive?.passiveType == PassiveType.PresenceFuneste)
        {
            // Retrait Saignée (1×/match) dès qu'elle est utilisée
            var enragedSpell = EnragedSpell;
            if (enragedSpell != null && spell == enragedSpell)
                character.RemoveRuntimeSpell(enragedSpell);

            // Incrémentation jauge — bloquée à 8 (Enragé = palier max)
            if (_ghostraDorsalThisSpell && _ghostraJauges < 8)
            {
                _ghostraJauges++;
                int palier = GhostraPalier;
                string palierName = palier == 2 ? "ENRAGÉ" : palier == 1 ? "Éveillé" : "Base";
                Debug.Log($"[Ghostra] Coup dorsal ! Jauge : {_ghostraJauges}/8 — Palier : {palierName}" +
                          (palier == 2 ? " — Saignée débloquée !" : ""));

                if (palier == 2 && !_ghostraEnrageUnlocked)
                {
                    _ghostraEnrageUnlocked = true;
                    if (enragedSpell != null)
                    {
                        character.AddRuntimeSpell(enragedSpell);
                        Debug.Log("[Ghostra] Saignée ajoutée en main (slot 7).");
                    }
                    else
                    {
                        Debug.LogWarning("[Ghostra] enragedSpell non assigné dans ClassData Ghostra !");
                    }
                }
            }
            _ghostraDorsalThisSpell = false;
        }

        if (activePassive?.passiveType == PassiveType.Toxine)
        {
            var enragedSpell = EnragedSpell;
            if (enragedSpell != null && spell == enragedSpell)
                character.RemoveRuntimeSpell(enragedSpell);
        }

        if (activePassive?.passiveType == PassiveType.VolDAme)
        {
            var enragedSpell = EnragedSpell;

            // Retrait Âme Lacérée (1×/match) dès qu'elle est utilisée
            if (enragedSpell != null && spell == enragedSpell)
                character.RemoveRuntimeSpell(enragedSpell);

            // Conversion dégâts → jauges (+1 par 60 dmg, carry-over)
            if (_soulrenderPendingDamage > 0)
            {
                _soulrenderDamageAccum += _soulrenderPendingDamage;
                int gained = _soulrenderDamageAccum / 60;
                _soulrenderDamageAccum %= 60;

                if (gained > 0 && _soulrenderJauges < 6)
                {
                    int prev = _soulrenderJauges;
                    _soulrenderJauges = Mathf.Min(6, _soulrenderJauges + gained);
                    string palierName = SoulrenderPalier == 2 ? "ENRAGÉ" : SoulrenderPalier == 1 ? "Éveillé" : "Base";
                    Debug.Log($"[Soulrender] +{_soulrenderJauges - prev} jauge(s) — {_soulrenderJauges}/6 — Palier : {palierName}");
                }
                _soulrenderPendingDamage = 0;
            }

            // Déverrouillage Âme Lacérée à palier Enragé
            if (SoulrenderPalier == 2 && !_soulrenderEnrageUnlocked)
            {
                _soulrenderEnrageUnlocked = true;
                if (enragedSpell != null)
                {
                    character.AddRuntimeSpell(enragedSpell);
                    Debug.Log("[Soulrender] Âme Lacérée ajoutée en main.");
                }
                else
                {
                    Debug.LogWarning("[Soulrender] enragedSpell non assigné dans ClassData Soulrender !");
                }
            }
        }
    }

    private void HandleTurnStart()
    {
        castSpellThisTurn = false;

        if (character.stats == null || activePassive == null) return;

        if (activePassive.passiveType == PassiveType.DernierRempart)
        {
            int th = Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.conditionThreshold / 100f)));
            if (character.CurrentHP <= th)
            {
                int sh = Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
                character.AddStatusEffect(new StatusEffect(StatusEffectType.Shield, sh, 1));
            }
        }

        if (activePassive.passiveType == PassiveType.Vigilance && HasAdjacentEnemy())
            character.AddBonusPM(1);
    }

    private void HandleTurnEnd()
    {
        if (activePassive?.passiveType == PassiveType.Camouflage && !castSpellThisTurn)
        {
            character.AddStatusEffect(new StatusEffect(StatusEffectType.Invisible, 0, 1));
            character.AddNextTurnBonusPM(1);
            ShiftOneRandomCell();
        }
    }

    private void HandleAnyTurnEnd(TacticalCharacter whoJustPlayed)
    {
        if (activePassive?.passiveType != PassiveType.Toxicite) return;
        if (whoJustPlayed == character) return;
        if (character.stats == null) return;
        if (IsAdjacent(whoJustPlayed))
        {
            int d = Mathf.Max(1, Mathf.RoundToInt(character.stats.maxHP * (activePassive.effectValue / 100f)));
            whoJustPlayed.TakeDamage(d, character);
        }
    }

    private bool HasAdjacentEnemy()
    {
        if (character.CurrentCell == null || GridManager.Instance == null) return false;
        foreach (var n in GridManager.Instance.GetNeighbors(character.CurrentCell))
        {
            if (n == null || !n.IsOccupied) continue;
            var other = n.Occupant?.GetComponent<TacticalCharacter>();
            if (other != null && other != character && other.IsAlive) return true;
        }
        return false;
    }

    private bool IsAdjacent(TacticalCharacter other)
    {
        if (character.CurrentCell == null || other.CurrentCell == null) return false;
        int dx = Mathf.Abs(character.CurrentCell.GridX - other.CurrentCell.GridX);
        int dy = Mathf.Abs(character.CurrentCell.GridY - other.CurrentCell.GridY);
        return dx + dy == 1;
    }

    private void ShiftOneRandomCell()
    {
        if (character.CurrentCell == null) return;
        var neighbors = GridManager.Instance.GetNeighbors(character.CurrentCell);
        var free = neighbors.FindAll(c => c.IsWalkable && !c.IsOccupied);
        if (free.Count == 0) return;

        Cell dest = free[Random.Range(0, free.Count)];
        character.CurrentCell.ClearOccupant();
        dest.SetOccupant(character.gameObject);
        character.transform.position = dest.WorldPosition;
        character.ForceSetCell(dest);
    }
}

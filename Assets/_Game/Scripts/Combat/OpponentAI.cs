using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// IA adversaire — joue la méta Soulrender :
///   1. Soin d'urgence si HP critique (&lt; 40 %)
///   2. Marque de Carnage à portée pour poser Brulure avant de s'approcher
///   3. Empoignade (Pull) si l'ennemi est à portée — ramène adjacent + dégâts
///   4. Déplacement vers la cible
///   5. Attaque contextuelle :
///        adjacent + debuff  → Ouvre-Plaie  (50 + 50 bonus)
///        adjacent           → Marque de Carnage d'abord, puis Tranche-Âme
///        pas adjacent       → Curée ou Marque de Carnage à distance
/// </summary>
[RequireComponent(typeof(TacticalCharacter))]
public class OpponentAI : MonoBehaviour
{
    [Header("Délais IA (secondes)")]
    [Tooltip("Pause avant d'agir (simule une réflexion).")]
    public float thinkDelay   = 0.6f;
    [Tooltip("Délai entre le déplacement et le lancer de sort.")]
    public float actionDelay  = 0.4f;
    [Tooltip("Délai avant d'appeler Fin de tour.")]
    public float endTurnDelay = 0.5f;
    [Tooltip("Seuil de PV (%) en dessous duquel l'IA se soigne en priorité.")]
    [Range(0f, 1f)]
    public float healThreshold = 0.40f;

    private TacticalCharacter _self;
    private SpellCaster       _caster;
    private bool              _myTurn;

    void Awake()
    {
        _self   = GetComponent<TacticalCharacter>();
        _caster = GetComponent<SpellCaster>();
    }

    void Start()
    {
        if (IsNetworkDuel()) return;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStart += OnTurnStart;
            TurnManager.Instance.OnCombatEnd += OnCombatEnd;
        }
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStart -= OnTurnStart;
            TurnManager.Instance.OnCombatEnd -= OnCombatEnd;
        }
    }

    // =========================================================
    // CALLBACKS TURN MANAGER
    // =========================================================
    private void OnTurnStart(TacticalCharacter character)
    {
        if (character != _self) return;
        if (_myTurn) return;
        _myTurn = true;
        StartCoroutine(PlayTurn());
    }

    private void OnCombatEnd(int winnerTeamId)
    {
        StopAllCoroutines();
        _myTurn = false;
    }

    // =========================================================
    // DÉROULEMENT DU TOUR — Méta Soulrender
    // =========================================================
    private IEnumerator PlayTurn()
    {
        yield return new WaitForSeconds(thinkDelay);

        TacticalCharacter target = FindPlayerCharacter();
        if (target == null) { EndMyTurn(); yield break; }

        float hpRatio = _self.stats != null
            ? (float)_self.CurrentHP / _self.stats.maxHP
            : 1f;

        // ── 1. URGENCE : soin si HP < seuil ──────────────────────────────
        if (hpRatio < healThreshold && _self.CurrentPA > 0)
        {
            var healSpell = FindSurvivalSpell();
            if (healSpell != null)
            {
                yield return StartCoroutine(CastSelfSpell(healSpell));
                yield return new WaitForSeconds(actionDelay);
            }
        }

        // ── 2. DEBUFF À DISTANCE : Marque de Carnage si ennemi pas encore debuffé ──
        if (!IsAdjacentTo(target) && _self.CurrentPA > 0 && !target.HasAnyDebuff())
        {
            var marque = FindSpellCastable("Marque de Carnage", target);
            if (marque != null)
            {
                yield return StartCoroutine(CastSpellAt(marque, target.CurrentCell));
                yield return new WaitForSeconds(actionDelay);
            }
        }

        // ── 3. PULL : Empoignade pour ramener l'ennemi adjacent ──────────
        if (!IsAdjacentTo(target) && _self.CurrentPA > 0)
        {
            var empoignade = FindSpellCastable("Empoignade", target);
            if (empoignade != null)
            {
                yield return StartCoroutine(CastSpellAt(empoignade, target.CurrentCell));
                yield return new WaitForSeconds(actionDelay);
            }
        }

        // ── 4. DÉPLACEMENT vers la cible ─────────────────────────────────
        if (!IsAdjacentTo(target) && _self.CurrentPM > 0)
            yield return StartCoroutine(MoveToward(target));

        yield return new WaitForSeconds(actionDelay);

        // ── 5. ATTAQUE contextuelle ───────────────────────────────────────
        if (_self.CurrentPA > 0 && _caster != null)
        {
            bool adjacent       = IsAdjacentTo(target);
            bool targetDebuffed = target.HasAnyDebuff();

            SpellData attackSpell = null;

            if (adjacent)
            {
                // Priorité : Ouvre-Plaie (bonus x2 si debuff)
                if (targetDebuffed)
                    attackSpell = FindSpellCastable("Ouvre-Plaie", target);

                // Poser Marque de Carnage si l'ennemi n'a pas de debuff (prépare prochain tour)
                if (attackSpell == null && !targetDebuffed)
                    attackSpell = FindSpellCastable("Marque de Carnage", target);

                // Tranche-Âme en core attack
                if (attackSpell == null)
                    attackSpell = FindSpellCastable("Tranche-Âme", target);

                // Lame Vorace en alternative
                if (attackSpell == null)
                    attackSpell = FindSpellCastable("Lame Vorace", target);
            }
            else
            {
                // Pas adjacent : Curée > Marque de Carnage > Charge Brutal
                attackSpell = FindSpellCastable("Curée", target)
                           ?? FindSpellCastable("Marque de Carnage", target)
                           ?? FindSpellCastable("Charge Brutal", target)
                           ?? FindAnyCastableAttack(target);
            }

            if (attackSpell != null)
            {
                Cell castCell = PickCastTarget(attackSpell, target);
                if (castCell != null)
                    yield return StartCoroutine(CastSpellAt(attackSpell, castCell));
            }
        }

        yield return new WaitForSeconds(endTurnDelay);
        EndMyTurn();
    }

    // =========================================================
    // HELPERS — SORT
    // =========================================================

    /// <summary>Cherche un sort par spellName dans le deck actif, castable ET avec une cible valide.</summary>
    private SpellData FindSpellCastable(string spellName, TacticalCharacter target)
    {
        if (_self.ActiveSpells == null) return null;
        foreach (var spell in _self.ActiveSpells)
        {
            if (spell == null || spell.spellName != spellName) continue;
            if (!_self.CanCastSpell(spell)) continue;
            if (PickCastTarget(spell, target) != null) return spell;
        }
        return null;
    }

    /// <summary>Premier sort de survie (Self/Boost) castable dans le deck.</summary>
    private SpellData FindSurvivalSpell()
    {
        if (_self.ActiveSpells == null) return null;
        foreach (var spell in _self.ActiveSpells)
        {
            if (spell == null) continue;
            if (spell.deckCategory != SpellDeckCategory.Survival) continue;
            if (!_self.CanCastSpell(spell)) continue;
            if (spell.zoneType == ZoneType.Self || spell.zoneType == ZoneType.Boost)
                return spell;
        }
        return null;
    }

    /// <summary>Premier sort Attaque castable sur la cible (pour fallback générique).</summary>
    private SpellData FindAnyCastableAttack(TacticalCharacter target)
    {
        if (_self.ActiveSpells == null) return null;
        foreach (var spell in _self.ActiveSpells)
        {
            if (spell == null || spell.deckCategory == SpellDeckCategory.Survival) continue;
            if (!_self.CanCastSpell(spell)) continue;
            if (PickCastTarget(spell, target) != null) return spell;
        }
        return null;
    }

    // =========================================================
    // HELPERS — CAST
    // =========================================================

    /// <summary>Sélectionne et lance un sort Self (ou Boost) sans attente de cible externe.</summary>
    private IEnumerator CastSelfSpell(SpellData spell)
    {
        if (!_caster.SelectSpell(spell)) yield break;
        bool cast = _caster.TryCast(_self.CurrentCell);
        if (!cast) { _caster.CancelSpell(); yield break; }

        float timeout = 3f;
        while (_self.State == CharacterState.Casting && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>Sélectionne et lance un sort sur une case donnée.</summary>
    private IEnumerator CastSpellAt(SpellData spell, Cell targetCell)
    {
        if (targetCell == null || !_caster.SelectSpell(spell)) yield break;
        bool cast = _caster.TryCast(targetCell);
        if (!cast) { _caster.CancelSpell(); yield break; }

        float timeout = 3f;
        while (_self.State == CharacterState.Casting && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    // =========================================================
    // HELPERS — CIBLE DE SORT
    // =========================================================
    private Cell PickCastTarget(SpellData spell, TacticalCharacter target)
    {
        if (_self.CurrentCell == null) return null;

        // Sorts sur soi-même ou Boost
        if (spell.zoneType == ZoneType.Self || spell.zoneType == ZoneType.Boost)
            return _self.CurrentCell;

        // Essayer la case directe de la cible
        if (target?.CurrentCell != null && _caster.WouldAcceptCast(spell, target.CurrentCell))
            return target.CurrentCell;

        // Parcourir les cases à portée à la recherche d'une cible valide
        int effectiveMax = spell.rangeMax + _self.GetBonusRange();
        Cell origin = _self.CurrentCell;
        for (int dx = -effectiveMax; dx <= effectiveMax; dx++)
        for (int dy = -effectiveMax; dy <= effectiveMax; dy++)
        {
            int dist = Mathf.Abs(dx) + Mathf.Abs(dy);
            if (dist < spell.rangeMin || dist > effectiveMax) continue;
            var cell = GridManager.Instance.GetCell(origin.GridX + dx, origin.GridY + dy);
            if (cell == null) continue;
            if (_caster.WouldAcceptCast(spell, cell)) return cell;
        }
        return null;
    }

    // =========================================================
    // HELPERS — DÉPLACEMENT
    // =========================================================
    private IEnumerator MoveToward(TacticalCharacter target)
    {
        Cell moveCell = FindBestMoveCell(target);
        if (moveCell == null) yield break;

        _self.MoveToCell(moveCell);
        float timeout = 8f;
        while (_self.State == CharacterState.Moving && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    private Cell FindBestMoveCell(TacticalCharacter target)
    {
        if (_self.CurrentCell == null || target?.CurrentCell == null) return null;

        var reachable = new Pathfinding().GetReachableCells(_self.CurrentCell, _self.CurrentPM);
        if (reachable == null || reachable.Count == 0) return null;

        Cell best     = null;
        int  bestDist = int.MaxValue;
        foreach (var cell in reachable)
        {
            if (cell.IsOccupied && cell != _self.CurrentCell) continue;
            int d = ManhattanDist(cell, target.CurrentCell);
            if (d < bestDist) { bestDist = d; best = cell; }
        }

        return best == _self.CurrentCell ? null : best;
    }

    // =========================================================
    // HELPERS — POSITION
    // =========================================================
    private bool IsAdjacentTo(TacticalCharacter target)
    {
        if (_self.CurrentCell == null || target?.CurrentCell == null) return false;
        return ManhattanDist(_self.CurrentCell, target.CurrentCell) == 1;
    }

    private static int ManhattanDist(Cell a, Cell b)
        => Mathf.Abs(a.GridX - b.GridX) + Mathf.Abs(a.GridY - b.GridY);

    // =========================================================
    // HELPERS — GÉNÉRAUX
    // =========================================================
    private TacticalCharacter FindPlayerCharacter()
    {
        var ci = CombatInitializer.Instance;
        if (ci != null && ci.player != null && ci.player.IsAlive)
            return ci.player;

        foreach (var tc in FindObjectsByType<TacticalCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (tc != _self && tc.IsAlive) return tc;
        return null;
    }

    private void EndMyTurn()
    {
        _myTurn = false;
        if (TurnManager.Instance != null && TurnManager.Instance.IsCombatActive)
            TurnManager.Instance.EndTurn();
    }

    private static bool IsNetworkDuel()
    {
        if (OracleCombatNetBridge.Instance == null) return false;
        return OracleCombatNetBridge.Instance.ShouldSendCommandsOverNetwork;
    }
}

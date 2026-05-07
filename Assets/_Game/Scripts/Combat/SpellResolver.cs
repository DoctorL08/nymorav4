using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SpellResolver
{
    sealed class ResolveScratch
    {
        public bool FirstDamageLineProcessed;

        // Necram — empêche l'auto-marque de se déclencher deux fois sur la même cible
        readonly HashSet<TacticalCharacter> _autoMarkedThisSpell = new HashSet<TacticalCharacter>();
        public bool TryAutoMark(TacticalCharacter t)
        {
            if (t == null || _autoMarkedThisSpell.Contains(t)) return false;
            _autoMarkedThisSpell.Add(t);
            return true;
        }

        readonly Dictionary<TacticalCharacter, int> _dmg    = new Dictionary<TacticalCharacter, int>();
        readonly Dictionary<TacticalCharacter, int> _heal   = new Dictionary<TacticalCharacter, int>();
        readonly Dictionary<TacticalCharacter, int> _shield = new Dictionary<TacticalCharacter, int>();

        public void AddFloatingDmgBatch(TacticalCharacter t, int v) => AddTo(_dmg, t, v);
        public void AddHealBatch(TacticalCharacter t, int v)        => AddTo(_heal, t, v);
        public void AddShieldBatch(TacticalCharacter t, int v)      => AddTo(_shield, t, v);

        static void AddTo(Dictionary<TacticalCharacter, int> dict, TacticalCharacter t, int v)
        {
            if (t == null || v <= 0) return;
            if (dict.TryGetValue(t, out int cur)) dict[t] = cur + v;
            else dict[t] = v;
        }

        /// <summary>Flush dans l'ordre : dégâts → soin → bouclier, à des hauteurs Y différentes.</summary>
        public void FlushAll(bool isSignature = false)
        {
            foreach (var kv in _dmg)
                if (kv.Key != null && kv.Value > 0)
                {
                    if (isSignature) FloatingDamageText.SpawnSignatureDamage(kv.Value, kv.Key.transform.position);
                    else             FloatingDamageText.SpawnDamage(kv.Value, kv.Key.transform.position);
                }
            _dmg.Clear();

            foreach (var kv in _heal)
                if (kv.Key != null && kv.Value > 0)
                    FloatingDamageText.SpawnHeal(kv.Value, kv.Key.transform.position);
            _heal.Clear();

            foreach (var kv in _shield)
                if (kv.Key != null && kv.Value > 0)
                    FloatingDamageText.SpawnShield(kv.Value, kv.Key.transform.position);
            _shield.Clear();
        }
    }

    public static void Resolve(SpellData spell, TacticalCharacter caster, List<Cell> affectedCells, Cell primaryTargetCell = null)
    {
        int totalDmg = 0, totalHeal = 0;
        var secondary = new List<string>();
        var scratch = new ResolveScratch();

        foreach (SpellEffect effect in spell.effects)
            ApplyEffect(effect, spell, caster, affectedCells, primaryTargetCell, scratch, ref totalDmg, ref totalHeal, secondary);

        // Résoudre le sort enragé depuis la classe du lanceur (pas HubManager.SelectedClass global),
        // pour que l'adversaire (Soulrender) utilise bien sa propre classe.
        var casterPm   = caster.GetComponent<PassiveManager>();
        var casterClass = casterPm?.ownerClass ?? HubManager.Instance?.SelectedClass;
        bool isSignature = casterClass?.enragedSpell == spell;
        scratch.FlushAll(isSignature);

        string cn = Fmt(caster);
        string sn = $"<b>{spell.spellName}</b>";
        string log;
        if      (totalDmg > 0 && totalHeal > 0)
            log = $"{cn} lance {sn} <color=#FF6B6B>-{totalDmg}</color> <color=#88FF88>+{totalHeal}</color>";
        else if (totalDmg > 0)
            log = $"{cn} lance {sn} <color=#FF6B6B>-{totalDmg}</color>";
        else if (totalHeal > 0)
            log = $"{cn} lance {sn} <color=#88FF88>+{totalHeal}</color>";
        else
            log = $"{cn} lance {sn}";

        CombatLog.Append(log);

        foreach (var msg in secondary)
            CombatLog.Append(msg);

        caster.NotifySpellCast(spell);
    }

    private static void ApplyEffect(
        SpellEffect effect, SpellData spell, TacticalCharacter caster,
        List<Cell> cells, Cell primaryTargetCell, ResolveScratch scratch,
        ref int totalDmg, ref int totalHeal, List<string> sec)
    {
        if (effect.type == SpellEffectType.ChipDamageAroundCaster)
        {
            ApplyChipDamageAroundCaster(effect, spell, caster, scratch, ref totalDmg, sec);
            return;
        }

        foreach (Cell cell in cells)
        {
            TacticalCharacter target = GetCharacterAt(cell);

            switch (effect.type)
            {
                case SpellEffectType.Damage:
                    if (target == null) break;
                    if (spell != null && spell.excludeCasterFromHarmfulAoE && target == caster) break;
                    if (!ProcessConditionalDamage(effect, spell, caster, target, cell, out int dmg))
                        break;

                    if (spell.zoneType == ZoneType.Bounce && primaryTargetCell != null && cell != primaryTargetCell)
                        dmg = Mathf.RoundToInt(dmg * 0.65f);

                    if (!scratch.FirstDamageLineProcessed)
                    {
                        caster.ApplyReducedAttackIfAny(ref dmg);
                        scratch.FirstDamageLineProcessed = true;
                    }

                    var pm = caster.GetComponent<PassiveManager>();
                    if (pm != null)
                    {
                        int dist = Mathf.Abs(caster.CurrentCell.GridX - cell.GridX)
                                 + Mathf.Abs(caster.CurrentCell.GridY - cell.GridY);
                        dmg = pm.ModifyOutgoingDamage(dmg, spell, target, dist);
                    }
                    int lost = target.TakeDamage(dmg, caster, suppressFloatingText: true);
                    scratch.AddFloatingDmgBatch(target, lost);
                    totalDmg += dmg;

                    // Necram — Toxine : auto-marque venin sur chaque sort offensif (1 fois par cible par sort)
                    if (spell?.deckCategory == SpellDeckCategory.Attack && pm != null && scratch.TryAutoMark(target))
                    {
                        int veninTick = pm.NotifyOffensiveHit(target);
                        if (veninTick > 0)
                            sec.Add($"{Fmt(target)} : <color=#88FF44>Venin</color> (<color=#FF6B6B>-{veninTick}</color>/tour, 3T)");
                    }
                    break;

                case SpellEffectType.SelfDamage:
                    int lostSelf = caster.TakeDamage(effect.value, null, suppressFloatingText: true);
                    scratch.AddFloatingDmgBatch(caster, lostSelf);
                    break;

                case SpellEffectType.Heal:
                {
                    // Condition SelfHPBelow : Dernier Souffle (<30% HP), Sève Vive (+50 si saigne ≈ HP bas)
                    if (effect.condition == SpellCondition.SelfHPBelow && caster.stats != null)
                    {
                        int th = Mathf.Max(1, Mathf.RoundToInt(caster.stats.maxHP * (effect.conditionThreshold / 100f)));
                        if (caster.CurrentHP >= th) break;
                    }
                    var healTarget = target ?? caster;
                    healTarget.Heal(effect.value, suppressFloatingText: true);
                    scratch.AddHealBatch(healTarget, effect.value);
                    totalHeal += effect.value;
                    if (spell != null && spell.spellName == "Pansement")
                        healTarget.RemoveAllBleedEffects();
                    break;
                }

                case SpellEffectType.Bleed:
                    if (target == null) break;
                    target.AddStatusEffect(new StatusEffect(StatusEffectType.Bleed, effect.value, effect.duration, true));
                    sec.Add($"{Fmt(target)} : <color=#FF4444>Saignement</color> (<color=#FF6B6B>-{effect.value}</color>/tour, {effect.duration}T)");
                    break;

                case SpellEffectType.Brulure:
                    if (target == null) break;
                    target.AddStatusEffect(new StatusEffect(StatusEffectType.Brulure, effect.value, effect.duration, true));
                    sec.Add($"{Fmt(target)} : <color=#FF8844>Brûlure</color> (<color=#FF6B6B>-{effect.value}</color>/tour, {effect.duration}T)");
                    break;

                case SpellEffectType.Silence:
                    if (target == null) break;
                    if (effect.condition == SpellCondition.TargetHasDebuff && !target.HasAnyDebuff())
                        break;
                    {
                        var attackPool = new List<SpellData>();
                        foreach (var s in target.ActiveSpells)
                            if (s != null && s.deckCategory == SpellDeckCategory.Attack)
                                attackPool.Add(s);
                        SpellData banned = null;
                        if (attackPool.Count > 0)
                            banned = attackPool[Random.Range(0, attackPool.Count)];
                        target.AddStatusEffect(new StatusEffect(StatusEffectType.Silence, 0, effect.duration, true, banned));
                        if (banned != null)
                            sec.Add($"{Fmt(target)} : <color=#BB88FF>Silence</color> — sort Attaque banni : <b>{banned.spellName}</b> ({effect.duration} tour{(effect.duration > 1 ? "s" : "")})");
                        else
                            sec.Add($"{Fmt(target)} : <color=#BB88FF>Silence</color> — aucun sort Attaque à bannir ({effect.duration} tour{(effect.duration > 1 ? "s" : "")})");
                    }
                    break;

                case SpellEffectType.GravityDebuff:
                    if (target == null) break;
                    target.AddStatusEffect(new StatusEffect(StatusEffectType.GravityDebuff, 0, effect.duration, true));
                    sec.Add($"{Fmt(target)} : <color=#BB88FF>Gravité</color> ({effect.duration}T)");
                    break;

                case SpellEffectType.ReduceFirstAttack:
                    if (target == null) break;
                    target.AddStatusEffect(new StatusEffect(StatusEffectType.ReducedAttack, effect.value, effect.duration, true));
                    sec.Add($"{Fmt(target)} : Frappe réduite ({effect.duration}T)");
                    break;

                case SpellEffectType.RemovePM:
                    if (target == null) break;
                    { int removed = target.RemovePM(effect.value);
                      if (removed > 0)
                      {
                          sec.Add($"{Fmt(target)} : <color=#88AAFF>-{removed} PM</color>");
                          FloatingDamageText.SpawnPMLoss(removed, target.transform.position);
                      }
                    }
                    break;

                case SpellEffectType.StealPM:
                    if (target == null) break;
                    { int stolen = target.RemovePM(effect.value);
                      caster.AddBonusPM(stolen);
                      if (stolen > 0)
                      {
                          sec.Add($"{Fmt(caster)} vole <color=#88AAFF>{stolen} PM</color> à {Fmt(target)}");
                          FloatingDamageText.SpawnPMLoss(stolen, target.transform.position);
                          FloatingDamageText.SpawnPMGain(stolen, caster.transform.position);
                      }
                    }
                    break;

                case SpellEffectType.Shield:
                    caster.AddStatusEffect(new StatusEffect(StatusEffectType.Shield, effect.value, effect.duration));
                    scratch.AddShieldBatch(caster, effect.value);
                    sec.Add($"{Fmt(caster)} : <color=#4499FF>Bouclier +{effect.value} PV</color>");
                    break;

                case SpellEffectType.DamageReduction:
                    caster.AddStatusEffect(new StatusEffect(StatusEffectType.DamageReduction, effect.value, effect.duration));
                    sec.Add($"{Fmt(caster)} : Réduction <color=#4499FF>-{effect.value} dmg</color> ({effect.duration}T)");
                    break;

                case SpellEffectType.Thorns:
                    caster.AddStatusEffect(new StatusEffect(StatusEffectType.Thorns, effect.value, effect.duration));
                    sec.Add($"{Fmt(caster)} : <color=#88FF44>Épines</color> ({effect.value} dmg, {effect.duration}T)");
                    break;

                case SpellEffectType.Invisible:
                    caster.AddStatusEffect(new StatusEffect(StatusEffectType.Invisible, 0, effect.duration));
                    sec.Add($"{Fmt(caster)} : Invisible ({effect.duration}T)");
                    break;

                case SpellEffectType.LastBreath:
                    caster.AddStatusEffect(new StatusEffect(StatusEffectType.LastBreath, 1, effect.duration));
                    sec.Add($"{Fmt(caster)} : <color=#FFFF44>Second Souffle</color> actif");
                    break;

                case SpellEffectType.BonusPA:
                    caster.AddBonusPA(effect.value);
                    FloatingDamageText.SpawnPAGain(effect.value, caster.transform.position);
                    sec.Add($"{Fmt(caster)} : <color=#FFD700>+{effect.value} PA</color>");
                    break;

                case SpellEffectType.BonusPM:
                    caster.AddBonusPM(effect.value);
                    FloatingDamageText.SpawnPMGain(effect.value, caster.transform.position);
                    sec.Add($"{Fmt(caster)} : <color=#88FF88>+{effect.value} PM</color>");
                    break;

                case SpellEffectType.BonusPANextTurn:
                    caster.AddNextTurnBonusPA(effect.value);
                    FloatingDamageText.SpawnPAGain(effect.value, caster.transform.position);
                    sec.Add($"{Fmt(caster)} : <color=#FFD700>+{effect.value} PA</color> (prochain tour)");
                    break;

                case SpellEffectType.BonusRange:
                    caster.AddBonusRange(effect.value, effect.duration);
                    sec.Add($"{Fmt(caster)} : <color=#88CCFF>+{effect.value} portée</color> ({effect.duration}T)");
                    break;

                case SpellEffectType.ConvertPMtoPA:
                    if (caster.RemovePM(1) > 0)
                    {
                        caster.AddBonusPA(2);
                        FloatingDamageText.SpawnPMLoss(1, caster.transform.position);
                        FloatingDamageText.SpawnPAGain(2, caster.transform.position);
                        sec.Add($"{Fmt(caster)} : 1 PM → 2 PA");
                    }
                    break;

                case SpellEffectType.Cleanse:
                {
                    var ct = target ?? caster;
                    if (effect.value > 0)
                    {
                        // Cautérisation : retire les Brulures et soigne N PV par Brulure (min N PV)
                        int stacks = ct.CountBrulureStacks();
                        ct.RemoveAllBrulureEffects();
                        int healAmt = Mathf.Max(effect.value, stacks * effect.value);
                        ct.Heal(healAmt, suppressFloatingText: true);
                        scratch.AddHealBatch(ct, healAmt);
                        totalHeal += healAmt;
                        sec.Add($"{Fmt(ct)} : {stacks} saignement(s) retiré(s) → <color=#88FF88>+{healAmt} PV</color>");
                    }
                    else
                    {
                        ct.ClearAllDebuffs();
                        sec.Add($"{Fmt(ct)} est purifié");
                    }
                    break;
                }

                case SpellEffectType.Push:
                    if (target == null) break;
                    if (spell != null && spell.excludeCasterFromHarmfulAoE && target == caster) break;
                    Push(target, caster.CurrentCell, effect.value);
                    sec.Add($"{Fmt(target)} repoussé");
                    break;

                case SpellEffectType.Pull:
                    if (target != null && target != caster)
                    {
                        // Empoignade : tire l'ennemi vers le lanceur
                        if (IsGravityBlocked(target, sec, "Empoignade")) break;
                        PullTargetTowardCaster(target, caster.CurrentCell, effect.value);
                        sec.Add($"{Fmt(target)} est tiré vers {Fmt(caster)}");
                    }
                    else
                    {
                        // Liane de Fer : le lanceur s'approche de la cible
                        if (IsGravityBlocked(caster, sec, "Liane")) break;
                        PullToward(caster, cell, effect.value);
                        sec.Add($"{Fmt(caster)} s'approche");
                    }
                    break;

                case SpellEffectType.Swap:
                    if (target == null) break;
                    if (IsGravityBlocked(caster, sec, "Échange")) break;
                    Swap(caster, target);
                    sec.Add($"{Fmt(caster)} ↔ {Fmt(target)}");
                    break;

                case SpellEffectType.Teleport:
                    if (IsGravityBlocked(caster, sec, "Téléportation")) break;
                    if (!cell.IsOccupied && cell.IsWalkable)
                    {
                        MoveInstant(caster, cell);
                        sec.Add($"{Fmt(caster)} se téléporte");
                    }
                    break;

                case SpellEffectType.CreateWall:
                    if (cell != null && !cell.IsOccupied && cell.IsWalkable)
                    {
                        int turns = effect.value > 0 ? effect.value : Mathf.Max(1, effect.duration);
                        cell.IsWalkable = false;
                        cell.TemporaryWallTurns = turns;
                        sec.Add($"Obstacle créé ({turns} tour{(turns > 1 ? "s" : "")})");
                    }
                    break;

                case SpellEffectType.HealCaster:
                {
                    // Conditions optionnelles (ex. Drain Vital : soin bonus si cible a des marques)
                    if (effect.condition == SpellCondition.TargetHasVenin)
                    {
                        if (target == null || target.CountVeninStacks() == 0) break;
                    }
                    else if (effect.condition == SpellCondition.TargetHasDebuff)
                    {
                        if (target == null || !target.HasAnyDebuff()) break;
                    }
                    caster.Heal(effect.value, suppressFloatingText: true);
                    scratch.AddHealBatch(caster, effect.value);
                    totalHeal += effect.value;
                    sec.Add($"{Fmt(caster)} : <color=#88FF88>+{effect.value}</color> (drain)");
                    break;
                }

                case SpellEffectType.ApplyVenin:
                {
                    if (target == null) break;
                    if (spell != null && spell.excludeCasterFromHarmfulAoE && target == caster) break;
                    var pm_av = caster.GetComponent<PassiveManager>();
                    int tickVal = pm_av != null ? pm_av.GetVeninTickValue() : effect.value;
                    target.AddStatusEffect(new StatusEffect(StatusEffectType.Venin, tickVal, effect.duration, true));
                    pm_av?.NotifyVeninApplied(1);
                    sec.Add($"{Fmt(target)} : <color=#88FF44>Venin</color> (<color=#FF6B6B>-{tickVal}</color>/tour, {effect.duration}T)");
                    break;
                }

                case SpellEffectType.ConsumeVenin:
                {
                    if (target == null) break;
                    var pm_cv = caster.GetComponent<PassiveManager>();
                    float burstMult = pm_cv != null ? pm_cv.GetVeninBurstMult() : 1f;

                    if (effect.value > 0)
                    {
                        // Détonation Virulente : bonus plat par marque, plafonné
                        int stacks = target.CountVeninStacks();
                        target.RemoveAllVeninEffects();
                        int perMark = Mathf.RoundToInt(effect.value * burstMult);
                        int burst   = stacks * perMark;
                        if (burst > 0)
                        {
                            int lostB = target.TakeDamage(burst, caster, suppressFloatingText: true);
                            scratch.AddFloatingDmgBatch(target, lostB);
                            totalDmg += burst;
                            caster.Heal(burst, suppressFloatingText: true);
                            scratch.AddHealBatch(caster, burst);
                            totalHeal += burst;
                            sec.Add($"{Fmt(target)} : <color=#44FF88>Drain Venin</color> ({stacks} marques × {perMark} → <color=#FF6B6B>-{burst}</color> / <color=#88FF88>+{burst}</color>)");
                        }
                        else if (stacks > 0)
                            sec.Add($"{Fmt(target)} : <color=#44FF88>Drain</color> — aucune marque active");
                    }
                    else
                    {
                        // Virus Fatal : déclenche tous les ticks restants avec multiplicateur Enragé
                        int rawBurst = target.ConsumeAllVenin();
                        int burst = Mathf.RoundToInt(rawBurst * burstMult);
                        if (burst > 0)
                        {
                            int lostVF = target.TakeDamage(burst, caster, suppressFloatingText: true);
                            scratch.AddFloatingDmgBatch(target, lostVF);
                            totalDmg += burst;
                            sec.Add($"{Fmt(target)} : <color=#FF44FF>Virus Fatal</color> (<color=#FF6B6B>-{burst}</color>)");
                        }
                    }
                    break;
                }
            }
        }
    }

    static void ApplyChipDamageAroundCaster(
        SpellEffect effect, SpellData spell, TacticalCharacter caster,
        ResolveScratch scratch, ref int totalDmg, List<string> _sec)
    {
        if (caster.CurrentCell == null || GridManager.Instance == null) return;
        int r = effect.duration > 0 ? effect.duration : 1;
        var around = AoECalculator.GetAffectedCells(
            ZoneType.Circle, caster.CurrentCell, caster.CurrentCell, r);
        foreach (Cell c in around)
        {
            TacticalCharacter t = GetCharacterAt(c);
            if (t == null || t == caster) continue;
            int chip = effect.value;
            if (!scratch.FirstDamageLineProcessed)
            {
                caster.ApplyReducedAttackIfAny(ref chip);
                scratch.FirstDamageLineProcessed = true;
            }
            var pm2 = caster.GetComponent<PassiveManager>();
            if (pm2 != null)
            {
                int dist = Mathf.Abs(caster.CurrentCell.GridX - c.GridX)
                         + Mathf.Abs(caster.CurrentCell.GridY - c.GridY);
                chip = pm2.ModifyOutgoingDamage(chip, spell, t, dist);
            }
            int lost = t.TakeDamage(chip, caster, suppressFloatingText: true);
            scratch.AddFloatingDmgBatch(t, lost);
            totalDmg += chip;
        }
    }

    static bool ProcessConditionalDamage(SpellEffect effect, SpellData spell, TacticalCharacter caster, TacticalCharacter target, Cell targetCell, out int dmg)
    {
        dmg = effect.value;
        switch (effect.condition)
        {
            case SpellCondition.TargetHPBelow:
                if (target.stats == null) return false;
                int th = Mathf.Max(1, Mathf.RoundToInt(target.stats.maxHP * (effect.conditionThreshold / 100f)));
                if (target.CurrentHP >= th) return false;
                dmg = Mathf.RoundToInt(dmg * effect.conditionMultiplier);
                return true;
            case SpellCondition.FromBehind:
                if (!IsAttackerBehindTarget(caster, target)) return false;
                dmg = effect.value;
                return true;
            case SpellCondition.TargetHasNoPM:
                return target.CurrentPM <= 0;
            case SpellCondition.AtMaxCastRange:
                if (caster.CurrentCell == null) return false;
                int effectiveMax = spell.rangeMax + caster.GetBonusRange();
                int distM = Mathf.Abs(caster.CurrentCell.GridX - targetCell.GridX) + Mathf.Abs(caster.CurrentCell.GridY - targetCell.GridY);
                return distM == effectiveMax;
            case SpellCondition.TargetAdjacentToObstacle:
                return targetCell != null && IsCellAdjacentToObstacle(targetCell);
            case SpellCondition.ThirdMeleeSpellThisTurn:
                return spell.isMeleeOnly && caster.MeleeSpellsCastThisTurn >= 2;
            case SpellCondition.TargetHasDebuff:
                return target != null && target.HasAnyDebuff();
            case SpellCondition.TargetHasVenin:
                return target != null && target.CountVeninStacks() > 0;
            case SpellCondition.AtExactCastDistance:
                if (caster.CurrentCell == null || targetCell == null) return false;
                int dm = Mathf.Abs(caster.CurrentCell.GridX - targetCell.GridX) + Mathf.Abs(caster.CurrentCell.GridY - targetCell.GridY);
                return dm == effect.conditionThreshold;
            default:
                return true;
        }
    }

    public static bool IsCellAdjacentToObstacle(Cell cell)
    {
        if (cell == null || GridManager.Instance == null) return false;
        foreach (Cell n in GridManager.Instance.GetNeighbors(cell))
        {
            if (n == null) continue;
            if (!n.IsWalkable || n.TemporaryWallTurns > 0) return true;
            if (n.TileType == CellTileType.Obstacle) return true;
        }
        return false;
    }

    static bool IsGravityBlocked(TacticalCharacter caster, List<string> sec, string actionLabel)
    {
        if (caster == null || !caster.HasStatusEffect(StatusEffectType.GravityDebuff)) return false;
        sec.Add($"{Fmt(caster)} : <color=#BB88FF>Gravité</color> — {actionLabel} bloqué(e)");
        return true;
    }

    private static string Fmt(TacticalCharacter t) => t != null ? $"<b>{t.name}</b>" : "?";

    private static TacticalCharacter GetCharacterAt(Cell cell)
    {
        if (cell == null || !cell.IsOccupied) return null;
        return cell.Occupant?.GetComponent<TacticalCharacter>();
    }

    /// <summary>
    /// Dos de la cible : cohérent avec le facing calculé en <b>monde</b> dans <see cref="TacticalCharacter"/> (pas les seuls Δ grille).
    /// </summary>
    public static bool IsAttackerBehindTarget(TacticalCharacter attacker, TacticalCharacter target)
    {
        if (attacker == null || target == null) return false;
        Cell ac = attacker.CurrentCell, tc = target.CurrentCell;
        if (ac == null || tc == null) return false;
        var gm = GridManager.Instance;
        if (gm == null) return false;

        Vector3 aw = gm.GridToWorld(ac.GridX, ac.GridY);
        Vector3 tw = gm.GridToWorld(tc.GridX, tc.GridY);
        Vector3 toAttacker = aw - tw;
        if (toAttacker.sqrMagnitude < 1e-10f) return false;
        toAttacker.z = 0f;

        // Avant : mêmes quadrants monde que TacticalCharacter.UpdateFacing
        Vector3 forward;
        switch (target.Facing)
        {
            case FacingDirection.NorthEast: forward = new Vector3(1f, 1f, 0f); break;
            case FacingDirection.NorthWest: forward = new Vector3(-1f, 1f, 0f); break;
            case FacingDirection.SouthEast: forward = new Vector3(1f, -1f, 0f); break;
            case FacingDirection.SouthWest: forward = new Vector3(-1f, -1f, 0f); break;
            default: return false;
        }

        // Strictement derrière : l'attaquant doit être dans le quadrant opposé au forward (X ET Y inversés)
        return (toAttacker.x * forward.x < 0f) && (toAttacker.y * forward.y < 0f);
    }

    private static void Push(TacticalCharacter target, Cell pushSource, int distance)
    {
        Cell tc = target.CurrentCell;
        int stepX = tc.GridX - pushSource.GridX;
        int stepY = tc.GridY - pushSource.GridY;
        if (stepX != 0) stepX = stepX > 0 ? 1 : -1;
        if (stepY != 0) stepY = stepY > 0 ? 1 : -1;

        Cell dest = tc;
        for (int i = 0; i < distance; i++)
        {
            Cell next = GridManager.Instance.GetCell(dest.GridX + stepX, dest.GridY + stepY);
            if (next == null || !next.IsWalkable || next.IsOccupied) break;
            dest = next;
        }
        if (dest != tc) MoveInstant(target, dest);
    }

    private static void PullToward(TacticalCharacter caster, Cell targetCell, int distance)
    {
        Cell origin = caster.CurrentCell;
        int stepX = targetCell.GridX - origin.GridX;
        int stepY = targetCell.GridY - origin.GridY;
        if (stepX != 0) stepX = stepX > 0 ? 1 : -1;
        if (stepY != 0) stepY = stepY > 0 ? 1 : -1;

        Cell dest = GridManager.Instance.GetCell(targetCell.GridX - stepX, targetCell.GridY - stepY);
        if (dest == null || !dest.IsWalkable || dest.IsOccupied) return;
        MoveInstant(caster, dest);
    }

    // Empoignade : tire la cible step-by-step vers le lanceur, s'arrête adjacente ou sur obstacle
    private static void PullTargetTowardCaster(TacticalCharacter target, Cell casterCell, int maxDistance)
    {
        if (GridManager.Instance == null || casterCell == null) return;
        Cell tc = target.CurrentCell;
        if (tc == null) return;

        int stepX = casterCell.GridX - tc.GridX;
        int stepY = casterCell.GridY - tc.GridY;
        if (stepX == 0 && stepY == 0) return;
        if (stepX != 0) stepX = stepX > 0 ? 1 : -1;
        if (stepY != 0) stepY = stepY > 0 ? 1 : -1;

        Cell dest = tc;
        for (int i = 0; i < maxDistance; i++)
        {
            Cell next = GridManager.Instance.GetCell(dest.GridX + stepX, dest.GridY + stepY);
            if (next == null || next == casterCell || !next.IsWalkable || next.IsOccupied) break;
            dest = next;
        }
        if (dest != tc) MoveInstant(target, dest);
    }

    private static void Swap(TacticalCharacter a, TacticalCharacter b)
    {
        Cell ca = a.CurrentCell;
        Cell cb = b.CurrentCell;
        ca.ClearOccupant();
        cb.ClearOccupant();
        ca.SetOccupant(b.gameObject);
        cb.SetOccupant(a.gameObject);
        if (GridManager.Instance != null)
        {
            b.transform.position = GridManager.Instance.GridToWorldFace(ca.GridX, ca.GridY);
            a.transform.position = GridManager.Instance.GridToWorldFace(cb.GridX, cb.GridY);
        }
        else
        {
            b.transform.position = ca.WorldPosition;
            a.transform.position = cb.WorldPosition;
        }
        b.ForceSetCell(ca);
        a.ForceSetCell(cb);
    }

    private static void MoveInstant(TacticalCharacter character, Cell destination)
    {
        character.CurrentCell?.ClearOccupant();
        destination.SetOccupant(character.gameObject);
        character.transform.position = GridManager.Instance != null
            ? GridManager.Instance.GridToWorldFace(destination.GridX, destination.GridY)
            : destination.WorldPosition;
        character.ForceSetCell(destination);
    }
}

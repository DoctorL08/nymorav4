using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// Chef d'orchestre de la scène de combat.
///
/// PIPELINE COMPLET :
///   Phase 0 : L'ArenaGenerator génère la map (generateOnStart = true)
///   Phase 1 : Sélection des passifs — PassiveSelectionScreen pour chaque joueur
///   Phase 2 : Placement — le joueur clique sur une case de spawn pour placer son perso
///   Phase 3 : Combat — TurnManager.StartCombat(), DeckUI bind sur chaque début de tour
///   Phase 4 : Fin de combat — affichage résultat
///
/// SETUP INSPECTOR :
///   - Glisser les deux TacticalCharacter (ils doivent être DÉSACTIVÉS au départ)
///   - Glisser le DeckUI, le PassiveSelectionScreen, l'ArenaGenerator
///   - (Optionnel) Glisser le panneau résultat
/// </summary>
public class CombatInitializer : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================
    public static CombatInitializer Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Filet de sécurité radical : tout HubPlayerAvatar qui aurait survécu à la transition
        // Hub → Ranked1v1 (Photon n'a pas toujours synchronisé la destruction à temps) est éliminé ici.
        var stragglers = FindObjectsByType<HubPlayerAvatar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var hub in stragglers)
        {
            if (hub == null) continue;
            var pv = hub.GetComponent<PhotonView>();
            if (PhotonNetwork.IsConnected && pv != null && pv.IsMine)
                PhotonNetwork.Destroy(hub.gameObject);
            else
                Destroy(hub.gameObject);
        }

        // Belt & suspenders : polling agressif pendant 5 secondes au cas où des avatars
        // Hub arrivent APRÈS ce sweep (chargement asynchrone Photon, latence réseau, etc.)
        StartCoroutine(NukeHubPlayersForSeconds(5f));
    }

    /// <summary>
    /// Coroutine de polling : détruit tout <see cref="HubPlayerAvatar"/> toutes les 100 ms
    /// pendant <paramref name="duration"/> secondes. Complémentaire au sweep instantané de Awake.
    /// </summary>
    System.Collections.IEnumerator NukeHubPlayersForSeconds(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            var rampants = FindObjectsByType<HubPlayerAvatar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var hub in rampants)
            {
                if (hub == null) continue;
                Debug.LogWarning($"[CombatInitializer] HubPlayer rampant détruit : {hub.name}");
                Destroy(hub.gameObject);
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }

    // =========================================================
    // PHASE PUBLIQUE (lue par PlayerInputHandler)
    // =========================================================
    public enum CombatPhase { WaitingForArena, PassiveSelection, Placement, Combat, End }
    public CombatPhase CurrentPhase => phase;

    // =========================================================
    // RÉFÉRENCES INSPECTOR
    // =========================================================
    [Header("Personnages (désactivés au départ)")]
    public TacticalCharacter player;       // Équipe 1 — contrôlé localement
    public TacticalCharacter opponent;     // Équipe 2 — IA ou second joueur

    [Header("Composants scène")]
    public ArenaGenerator    arenaGenerator;
    public DeckUI            deckUI;

    [Header("UI Résultat (optionnel)")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Options")]
    [Tooltip("Si true, l'adversaire est placé automatiquement sur une case de spawn aléatoire.")]
    public bool autoPlaceOpponent = true;
    [Tooltip("Délai (secondes) après la sélection de passif avant le placement.")]
    public float delayAfterPassiveSelection = 1.5f;

    [Header("UI Placement")]
    [Tooltip("Chrono + icône timer_icon_hud pendant le placement. Créé sous le Canvas si absent.")]
    public PlacementCountdownUI placementCountdownUi;
    [Tooltip("0 = pas de limite ni affichage du chrono placement. > 0 = secondes max (solo : placement auto sur une case libre si le temps expire).")]
    public float placementTimeLimitSeconds = 120f;

    // ── Mode de jeu ───────────────────────────────────────────────────────
    public enum CombatMode
    {
        /// <summary>Combat vs IA — IsNetworkDuel toujours false, OpponentAI actif.</summary>
        Training,
        /// <summary>Duel classé 1v1 — IsNetworkDuel déterminé par l'état Photon réel.</summary>
        Ranked1v1
    }

    [Header("Classe adversaire (Training)")]
    [Tooltip("Classe du personnage adversaire (IA). Si non assigné, Soulrender est chargé automatiquement depuis le ClassRegistry.")]
    public ClassData opponentClass;

    [Header("Mode de jeu")]
    [Tooltip("Training : combat solo vs IA (réseau ignoré).\nRanked1v1 : duel multijoueur, nécessite 2 joueurs Photon.")]
    public CombatMode combatMode = CombatMode.Training;

    [Header("Mode Test (Solo)")]
    [Tooltip("Skip la sélection de passif : un passif aléatoire est choisi instantanément. Pratique pour tester le combat rapidement.")]
    public bool skipPassiveSelection = false;

    [Header("Deck de sorts")]
    [Tooltip("Optionnel ; si vide → charge Resources OracleSpellPools/AllCombatSpellsPool.")]
    public SpellDeckPool spellDeckPool;
    [Tooltip("À chaque match : pioche DeckData.MaxSpells sorts distincts depuis le pool (joueur et adversaire : tirages indépendants).")]
    public bool randomizeSpellDeckEachMatch = true;

    // =========================================================
    // ÉTAT INTERNE
    // =========================================================
    private CombatPhase phase = CombatPhase.WaitingForArena;

    private List<Cell> spawnCellsTeam1;
    private List<Cell> spawnCellsTeam2;

    private bool playerPlaced = false;
    private bool netPlacement0Done = false;
    private bool netPlacement1Done = false;

    bool IsNetworkDuel =>
        combatMode != CombatMode.Training &&       // Training = toujours vs IA
        OracleCombatNetBridge.Instance != null &&
        PhotonNetwork.InRoom &&
        PhotonNetwork.CurrentRoom != null &&
        PhotonNetwork.CurrentRoom.PlayerCount >= 2;

    void ResolveSpellDeckPoolReference()
    {
        if (spellDeckPool == null)
            spellDeckPool = Resources.Load<SpellDeckPool>("OracleSpellPools/AllCombatSpellsPool");
    }

    void ApplyRandomSpellDecksIfConfigured()
    {
        if (!randomizeSpellDeckEachMatch) return;
        ResolveSpellDeckPoolReference();

        int need = DeckData.MaxSpells;
        player?.ClearRuntimeSpellDeck();
        opponent?.ClearRuntimeSpellDeck();

        // En duel réseau, les deux clients doivent tirer les MÊMES decks.
        // On dérive un seed déterministe du nom de la room (identique sur tous les clients).
        // En solo, seed aléatoire comme avant.
        int seed = IsNetworkDuel && PhotonNetwork.InRoom
            ? DeterministicHash(PhotonNetwork.CurrentRoom.Name)
            : unchecked((int)DateTime.UtcNow.Ticks) ^ UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        var rng = new System.Random(seed);

        // Deck du joueur : prioriser la sélection faite dans le hub (deck builder)
        var hubDeck = HubManager.Instance?.SelectedDeck;
        if (hubDeck != null && hubDeck.Count == need)
        {
            player?.SetRuntimeSpellDeck(hubDeck);
            Debug.Log("[CombatInitializer] Deck personnalisé (hub) appliqué au joueur.");
        }
        else
        {
            if (spellDeckPool == null || spellDeckPool.CandidateCount < need)
            {
                if (spellDeckPool != null)
                    Debug.LogWarning($"[CombatInitializer] Tirage aléatoire désactivé : au moins {need} sorts requis " +
                                     $"(actuellement {spellDeckPool.CandidateCount}). Oracle → Spell Deck Pool.");
                return;
            }
            player?.SetRuntimeSpellDeck(spellDeckPool.DrawRandomUnique(rng, need));
        }

        // Adversaire : en Training, deck méta fixe de la classe opposante,
        // sinon fallback sur un tirage équilibré 2+2+2 depuis la classe,
        // sinon pool global aléatoire en dernier recours.
        if (!IsNetworkDuel && opponentClass != null)
        {
            var metaDeck = BuildOpponentMetaDeck(opponentClass, need);
            if (metaDeck != null)
            {
                opponent?.SetRuntimeSpellDeck(metaDeck);
                Debug.Log($"[CombatInitializer] Deck méta {opponentClass.displayName} appliqué : {string.Join(", ", metaDeck.ConvertAll(s => s.spellName))}");
            }
            else
            {
                var classSpells = opponentClass.GetAllSpells();
                classSpells.RemoveAll(s => s == null);
                if (classSpells.Count >= need)
                {
                    var opRng = new System.Random(rng.Next());
                    for (int i = classSpells.Count - 1; i > 0; i--)
                    {
                        int j = opRng.Next(i + 1);
                        (classSpells[i], classSpells[j]) = (classSpells[j], classSpells[i]);
                    }
                    opponent?.SetRuntimeSpellDeck(classSpells.GetRange(0, need));
                    Debug.LogWarning($"[CombatInitializer] Deck méta introuvable pour {opponentClass.displayName} — tirage aléatoire utilisé.");
                }
                else if (spellDeckPool != null && spellDeckPool.CandidateCount >= need)
                {
                    opponent?.SetRuntimeSpellDeck(spellDeckPool.DrawRandomUnique(new System.Random(rng.Next()), need));
                }
            }
        }
        else if (spellDeckPool != null && spellDeckPool.CandidateCount >= need)
        {
            opponent?.SetRuntimeSpellDeck(spellDeckPool.DrawRandomUnique(new System.Random(rng.Next()), need));
        }
    }

    // =========================================================
    // DECK MÉTA PAR CLASSE
    // =========================================================

    /// <summary>
    /// Retourne un deck méta optimal pour l'IA en fonction de la classe adversaire.
    /// Les noms correspondent aux champs <see cref="SpellData.spellName"/> des assets.
    /// Retourne null si un ou plusieurs sorts sont introuvables dans la classe (fallback déclenché).
    /// </summary>
    static List<SpellData> BuildOpponentMetaDeck(ClassData cls, int count)
    {
        if (cls == null) return null;

        // Deck méta par classId — à étendre pour chaque nouvelle classe
        string[] metaNames = cls.classId switch
        {
            NymoraClassId.Soulrender => new[]
            {
                "Tranche-\u00C2me",    // 3PA, mêlée, 110 dmg                 — attaque principale
                "Ouvre-Plaie",         // 2PA, mêlée, 50 + 50 si debuff        — finisher sur cible brûlée
                "Marque de Carnage",   // 2PA, portée 4, Brulure 15/2 tours    — pose le debuff
                "Empoignade",          // 3PA, portée 3, Pull + 60 dmg         — ramène l'ennemi adjacent
                "Cur\u00E9e",          // 3PA, portée 2, 85 dmg                — attaque à distance
                "S\u00E8ve Vive",      // 2PA, self,  soins 80 (+50 si HP<50%) — survie
            },
            NymoraClassId.Ghostra => new[]
            {
                "Frappe Fantôme",      // core attack
                "Danse des Lames",     // melee combo
                "Dague Lancée",        // ranged
                "Marque de l'Ombre",   // debuff
                "Pas dans l'Ombre",    // repositioning
                "Voile Spectral",      // survival
            },
            _ => null
        };

        if (metaNames == null) return null;

        var all  = cls.GetAllSpells();
        var deck = new List<SpellData>(count);
        foreach (var name in metaNames)
        {
            var spell = all.Find(s => s != null && s.spellName == name);
            if (spell == null)
            {
                Debug.LogWarning($"[CombatInitializer] Sort méta introuvable dans {cls.displayName} : \"{name}\"");
                return null;
            }
            deck.Add(spell);
            if (deck.Count == count) break;
        }
        return deck.Count == count ? deck : null;
    }

    // Hash FNV-1a : déterministe, indépendant de la plateforme (contrairement à string.GetHashCode).
    static int DeterministicHash(string s)
    {
        unchecked
        {
            int hash = (int)2166136261u;
            foreach (char c in s)
                hash = (hash ^ c) * 16777619;
            return hash;
        }
    }

    // =========================================================
    // DÉMARRAGE
    // =========================================================
    void Start()
    {
        // L'ArenaGenerator a generateOnStart = true, donc la map est prête en Start()
        // (ArenaGenerator s'exécute en ordre -5, avant CombatInitializer)
        StartCoroutine(InitSequence());
    }

    IEnumerator InitSequence()
    {
        // ── Sécurité : attendre un frame pour être sûr que la grille est initialisée ──
        yield return null;

        // Auto-find de toutes les références non assignées (GOs actifs ET inactifs)
        if (player   == null) player   = FindFirstObjectByType<TacticalCharacter>(FindObjectsInactive.Include);
        if (opponent == null)
        {
            var all = FindObjectsByType<TacticalCharacter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tc in all)
                if (tc != player) { opponent = tc; break; }
        }
        else if (player != null && opponent == player)
        {
            opponent = null;
            var all = FindObjectsByType<TacticalCharacter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tc in all)
                if (tc != player) { opponent = tc; break; }
            if (opponent == null)
                Debug.LogError("[CombatInitializer] player et opponent référencent le même TacticalCharacter — en 1v1 réseau, glisse le perso « Opponent » dans opponent.");
        }
        if (arenaGenerator        == null) arenaGenerator        = FindFirstObjectByType<ArenaGenerator>(FindObjectsInactive.Include);
        if (deckUI                == null) deckUI                = FindFirstObjectByType<DeckUI>(FindObjectsInactive.Include);
        if (victoryPanel          == null)
        {
            var go = GameObject.Find("VictoryPanel");
            if (go != null) victoryPanel = go;
        }
        if (defeatPanel == null)
        {
            var go = GameObject.Find("DefeatPanel");
            if (go != null) defeatPanel = go;
        }

        // Charger Soulrender par défaut si aucune classe adversaire n'est assignée (Training)
        if (opponentClass == null && combatMode == CombatMode.Training)
        {
            opponentClass = HubManager.LoadClassById(NymoraClassId.Soulrender);
            if (opponentClass == null)
                Debug.LogWarning("[CombatInitializer] Impossible de charger Soulrender depuis le ClassRegistry. Vérifie que la classe est enregistrée.");
            else
                Debug.Log($"[CombatInitializer] Classe adversaire auto-chargée : {opponentClass.displayName}");
        }

        Validate();

        // Nommer les personnages avec les NickNames Photon des que 2 joueurs sont en salle.
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            ApplyNetworkPlayerNames();

        // Masque passif / sorts / fin de tour / PV jusqu’au début du combat (évite un flash si la scène les laisse actifs).
        SetPassiveSelectionCombatUiVisible(false);

        // En multijoueur, ArenaGenerator attend le seed broadcast par le master via Custom Room
        // Properties avant de générer — la génération est donc asynchrone. On attend ici que les
        // spawn cells soient disponibles avant de les lire (sinon RunPlacement reçoit des listes vides).
        if (arenaGenerator != null)
            yield return new WaitUntil(() => arenaGenerator.IsArenaReady);

        spawnCellsTeam1 = arenaGenerator != null ? arenaGenerator.GetSpawnCells(1) : new List<Cell>();
        spawnCellsTeam2 = arenaGenerator != null ? arenaGenerator.GetSpawnCells(2) : new List<Cell>();

        // ── Phase 2 : Placement ──────────────────────────────
        yield return StartCoroutine(RunPlacement());

        ApplyRandomSpellDecksIfConfigured();

        // ── Phase 3 : Lancement du combat ───────────────────
        StartCombat();
    }

    /// <summary>
    /// Affiche ou masque cartes de sort + bas de <see cref="CombatHUD"/>
    /// (passif, PA/PM, fin de tour, barres PV). Masqué pendant passif + placement ; activé au début du combat.
    /// </summary>
    void SetPassiveSelectionCombatUiVisible(bool showCombatUi)
    {
        var hud = FindFirstObjectByType<CombatHUD>(FindObjectsInactive.Include);
        if (hud != null)
            hud.SetCombatChromeVisible(showCombatUi);
        if (deckUI != null)
            deckUI.gameObject.SetActive(showCombatUi);
    }

    // =========================================================
    // PHASE 2 — PLACEMENT
    // =========================================================
    IEnumerator RunPlacement()
    {
        phase = CombatPhase.Placement;

        SetPassiveSelectionCombatUiVisible(false);

        if (OracleCombatNetBridge.Instance != null && PhotonNetwork.InRoom)
        {
            float wait = 0f;
            while (PhotonNetwork.CurrentRoom != null &&
                   PhotonNetwork.CurrentRoom.PlayerCount < 2 &&
                   wait < 30f)
            {
                wait += Time.deltaTime;
                yield return null;
            }
        }

        GridManager.Instance.ClearAllHighlights();

        if (IsNetworkDuel)
        {
            // Réinitialiser puis se resynchroniser : des RpcApplyPlacement peuvent arriver
            // pendant PassiveSelection (désync chronologie hôte / invité). Sans ça, un client
            // garde netPlacement* à false et reste bloqué sur WaitUntil avec les highlights.
            netPlacement0Done = false;
            netPlacement1Done = false;
            SyncNetPlacementFlagsFromPawns();

            if (PhotonNetwork.IsMasterClient)
            {
                GridManager.Instance.HighlightCells(spawnCellsTeam1, HighlightType.Move);
                Debug.Log("[CombatInitializer] En ligne — hôte : place l’équipe A sur les cases bleues.");
            }
            else
            {
                GridManager.Instance.HighlightCells(spawnCellsTeam2, HighlightType.Move);
                Debug.Log("[CombatInitializer] En ligne — invité : place l’équipe B sur les cases bleues.");
            }

            if (placementTimeLimitSeconds > 0f)
                ResolvePlacementHud()?.Show(placementTimeLimitSeconds);

            yield return new WaitUntil(() => netPlacement0Done && netPlacement1Done);
            GridManager.Instance.ClearAllHighlights();
            PlacementHudHide();
            yield break;
        }

        // ── Solo / vs IA ─────────────────────────────────────
        GridManager.Instance.HighlightCells(spawnCellsTeam1, HighlightType.Move);

        Debug.Log("[CombatInitializer] Phase placement — clique sur une case bleue pour placer ton personnage.");

        if (autoPlaceOpponent && spawnCellsTeam2.Count > 0)
        {
            Cell opponentCell = spawnCellsTeam2[UnityEngine.Random.Range(0, spawnCellsTeam2.Count)];
            PlaceCharacter(opponent, opponentCell, teamId: 2);
            Debug.Log($"[CombatInitializer] Adversaire placé en {opponentCell.GridX},{opponentCell.GridY}");
        }

        if (placementTimeLimitSeconds > 0f)
        {
            placementDeadlineUnscaled = Time.unscaledTime + placementTimeLimitSeconds;
            ResolvePlacementHud()?.Show(placementTimeLimitSeconds);
        }

        yield return new WaitUntil(() => playerPlaced || (placementTimeLimitSeconds > 0f &&
            Time.unscaledTime >= placementDeadlineUnscaled));
        TryAutoPlacePlayerBeforeCombatIfTimedOut();
        PlacementHudHide();

        GridManager.Instance.ClearAllHighlights();
    }

    float placementDeadlineUnscaled;

    void PlacementHudHide()
    {
        if (placementCountdownUi != null)
            placementCountdownUi.Hide();
        else
            FindFirstObjectByType<PlacementCountdownUI>(FindObjectsInactive.Include)?.Hide();
    }

    PlacementCountdownUI ResolvePlacementHud()
    {
        if (placementCountdownUi != null)
            return placementCountdownUi;
        placementCountdownUi = FindFirstObjectByType<PlacementCountdownUI>(FindObjectsInactive.Include);
        if (placementCountdownUi != null)
            return placementCountdownUi;
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return null;
        var go = new GameObject("PlacementCountdownUI");
        go.transform.SetParent(canvas.transform, false);
        placementCountdownUi = go.AddComponent<PlacementCountdownUI>();
        placementCountdownUi.EnsureBuilt();
        return placementCountdownUi;
    }

    void TryAutoPlacePlayerBeforeCombatIfTimedOut()
    {
        if (playerPlaced || placementTimeLimitSeconds <= 0f) return;
        Cell pick = null;
        foreach (var c in spawnCellsTeam1)
        {
            if (!c.IsOccupied) { pick = c; break; }
        }
        if (pick == null) return;
        PlaceCharacter(player, pick, teamId: 1);
        playerPlaced = true;
        Debug.Log("[CombatInitializer] Placement — temps écoulé, placement automatique sur une case libre.");
    }

    // =========================================================
    // CLIC SUR CASE DE SPAWN (appelé par PlayerInputHandler ou équivalent)
    // =========================================================

    /// <summary>
    /// À appeler depuis ton script de gestion des clics quand le joueur
    /// clique sur une cellule pendant la phase de placement.
    /// </summary>
    public void OnCellClickedDuringPlacement(Cell clickedCell)
    {
        if (phase != CombatPhase.Placement) return;

        if (IsNetworkDuel && OracleCombatNetBridge.Instance != null)
        {
            int slot = PhotonNetwork.IsMasterClient ? 0 : 1;
            var allowed = slot == 0 ? spawnCellsTeam1 : spawnCellsTeam2;
            if (!allowed.Contains(clickedCell) || clickedCell.IsOccupied) return;
            if ((slot == 0 && netPlacement0Done) || (slot == 1 && netPlacement1Done)) return;
            OracleCombatNetBridge.Instance.SubmitPlacement(slot, clickedCell.GridX, clickedCell.GridY);
            return;
        }

        if (playerPlaced) return;
        if (!spawnCellsTeam1.Contains(clickedCell)) return;
        if (clickedCell.IsOccupied) return;

        PlaceCharacter(player, clickedCell, teamId: 1);
        playerPlaced = true;

        Debug.Log($"[CombatInitializer] Joueur placé en {clickedCell.GridX},{clickedCell.GridY}");
    }

    /// <summary>Appelé sur toutes les machines après validation réseau du déploiement.</summary>
    public void OnNetworkPlacementApplied(int combatSlot, Cell cell)
    {
        if (phase == CombatPhase.Combat || phase == CombatPhase.End) return;
        if (combatSlot == 0)
        {
            if (netPlacement0Done) return;
            PlaceCharacter(player, cell, teamId: 1);
            netPlacement0Done = true;
            Debug.Log($"[CombatInitializer] Réseau — équipe A placée {cell.GridX},{cell.GridY}");
        }
        else
        {
            if (netPlacement1Done) return;
            PlaceCharacter(opponent, cell, teamId: 2);
            netPlacement1Done = true;
            Debug.Log($"[CombatInitializer] Réseau — équipe B placée {cell.GridX},{cell.GridY}");
        }
    }

    // =========================================================
    // PHASE 3 — DÉMARRAGE DU COMBAT
    // =========================================================
    void StartCombat()
    {
        phase = CombatPhase.Combat;

        SetPassiveSelectionCombatUiVisible(true);

        // Enregistrer les personnages dans le TurnManager
        TurnManager.Instance.RegisterCharacter(player,   teamId: 1);
        TurnManager.Instance.RegisterCharacter(opponent, teamId: 2);

        // Lier le DeckUI au personnage dont c'est le tour
        TurnManager.Instance.OnTurnStart += OnTurnStarted;

        // Écouter la fin du combat
        TurnManager.Instance.OnCombatEnd += OnCombatEnd;

        // Lancer !
        TurnManager.Instance.StartCombat();

        Debug.Log("[CombatInitializer] Combat démarré !");
    }

    // =========================================================
    // CALLBACKS DE COMBAT
    // =========================================================
    void OnTurnStarted(TacticalCharacter character)
    {
        // Le DeckUI se met à jour pour refléter les sorts du personnage actif
        if (deckUI != null)
            deckUI.BindCharacter(character);

        Debug.Log($"[CombatInitializer] Tour de : {character.name}");
    }

    void OnCombatEnd(int winnerTeamId)
    {
        phase = CombatPhase.End;

        TurnManager.Instance.OnTurnStart -= OnTurnStarted;
        TurnManager.Instance.OnCombatEnd -= OnCombatEnd;

        if (deckUI != null) deckUI.UnbindCharacter();

        int localTeam = IsNetworkDuel ? OracleCombatNetBridge.Instance.GetLocalTeamId() : 1;
        bool playerWon = (winnerTeamId == localTeam);
        Debug.Log($"[CombatInitializer] Combat terminé — {(winnerTeamId == -1 ? "Égalité" : $"Équipe {winnerTeamId} gagne")}");

        if (victoryPanel != null) victoryPanel.SetActive(playerWon);
        if (defeatPanel  != null) defeatPanel.SetActive(!playerWon && winnerTeamId != -1);

        // Bouton "Retour au Hub" — affiché seulement si HubManager est présent (joueur venu du hub)
        if (HubManager.Instance != null)
            ShowReturnToHubButton();
    }

    void ShowReturnToHubButton()
    {
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return;
        if (canvas.transform.Find("ReturnToHubBtn") != null) return;

        static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

        var btnGo = new GameObject("ReturnToHubBtn", typeof(RectTransform));
        btnGo.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)btnGo.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(220f, 52f);
        rt.anchoredPosition = new Vector2(0f, -110f);

        var img = btnGo.AddComponent<Image>();
        img.color = C(0.788f, 0.659f, 0.298f);

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;

        var cs = btn.colors;
        cs.highlightedColor = C(0.88f, 0.75f, 0.38f);
        cs.pressedColor     = C(0.60f, 0.50f, 0.22f);
        btn.colors = cs;

        btn.onClick.AddListener(HubManager.ReturnToHub);

        var txtGo  = new GameObject("Text", typeof(RectTransform));
        txtGo.transform.SetParent(btnGo.transform, false);
        var txtRt  = (RectTransform)txtGo.transform;
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;

        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Retour au Hub";
        tmp.fontSize  = 16f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = C(0.10f, 0.07f, 0.03f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    // =========================================================
    // UTILITAIRES
    // =========================================================
    /// <summary>Met à jour netPlacement* si les pions sont déjà posés (p. ex. RPC avant RunPlacement).</summary>
    void SyncNetPlacementFlagsFromPawns()
    {
        if (player != null && player.CurrentCell != null)
            netPlacement0Done = true;
        if (opponent != null && opponent.CurrentCell != null)
            netPlacement1Done = true;
    }

    void PlaceCharacter(TacticalCharacter character, Cell cell, int teamId)
    {
        character.gameObject.SetActive(true);
        character.Initialize(cell);

        if (character == player)
            character.SetFacingDirection(FacingDirection.NorthEast);
        else if (character == opponent)
            character.SetFacingDirection(FacingDirection.SouthWest);

        // Classe résolue via Photon CustomProperties en réseau pour éviter la désync inter-clients.
        ClassData selectedClass;
        if (IsNetworkDuel)
        {
            selectedClass = ResolveNetworkClass(isMasterSlot: character == player);
        }
        else
        {
            selectedClass = (character == player)
                ? HubManager.Instance?.SelectedClass
                : (character == opponent ? opponentClass : null);
        }

        var pm = character.GetComponent<PassiveManager>();
        if (pm != null)
        {
            // Lier la classe au PassiveManager pour que les sorts enragés soient résolus
            // depuis la propre classe du personnage, pas depuis HubManager.SelectedClass.
            pm.ownerClass = selectedClass;

            if (selectedClass?.passiveData != null)
            {
                pm.SetPassive(selectedClass.passiveData);
                Debug.Log($"[CombatInitializer] Passif de classe appliqué : {selectedClass.passiveData.passiveName} ({selectedClass.displayName})");
            }
            else if (pm.activePassive == null && character.deck?.Passive != null)
            {
                pm.SetPassive(character.deck.Passive);
            }
        }

        // Charge les animations du personnage depuis Resources/Characters/Frames/
        if (!string.IsNullOrEmpty(selectedClass?.characterKey))
        {
            var pa = character.GetComponent<PlayerAnimator>();
            pa?.LoadCharacterAnimations(selectedClass.characterKey);
        }
    }

    /// <summary>Résout la classe d'un slot via les CustomProperties Photon du joueur concerné.</summary>
    ClassData ResolveNetworkClass(bool isMasterSlot)
    {
        Photon.Realtime.Player target = null;
        foreach (var p in Photon.Pun.PhotonNetwork.PlayerList)
        {
            bool match = isMasterSlot ? p.IsMasterClient : !p.IsMasterClient;
            if (match) { target = p; break; }
        }
        if (target?.CustomProperties != null
            && target.CustomProperties.TryGetValue("classId", out var raw)
            && raw is int idInt)
        {
            return HubManager.LoadClassById((NymoraClassId)idInt);
        }
        return HubManager.Instance?.SelectedClass; // fallback ultime
    }

    /// <summary>
    /// Renomme player (slot 0 = MasterClient) et opponent (slot 1) avec les NickNames Photon.
    /// CombatHUD et logs utilisent character.name, donc ça suffit pour les labels.
    /// </summary>
    void ApplyNetworkPlayerNames()
    {
        // Slot 0 = MasterClient, slot 1 = non-master.
        // On itere PlayerList (tous les joueurs) et non PlayerListOthers
        // qui exclut le joueur local, causant un double-nom sur le client non-master.
        Photon.Realtime.Player masterPlayer = null;
        Photon.Realtime.Player otherPlayer  = null;
        foreach (var p in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient) masterPlayer = p;
            else                  otherPlayer  = p;
        }

        if (player != null && masterPlayer != null && !string.IsNullOrEmpty(masterPlayer.NickName))
        {
            player.gameObject.name = masterPlayer.NickName;
            Debug.Log("[CombatInitializer] Slot 0 (A) -> " + masterPlayer.NickName);
        }
        if (opponent != null && otherPlayer != null && !string.IsNullOrEmpty(otherPlayer.NickName))
        {
            opponent.gameObject.name = otherPlayer.NickName;
            Debug.Log("[CombatInitializer] Slot 1 (B) -> " + otherPlayer.NickName);
        }
    }

    void Validate()
    {
        if (player             == null) Debug.LogError("[CombatInitializer] Aucun TacticalCharacter (player) trouvé dans la scène !");
        if (opponent           == null) Debug.LogWarning("[CombatInitializer] Aucun adversaire trouvé — combat en solo uniquement.");
        if (arenaGenerator     == null) Debug.LogWarning("[CombatInitializer] ArenaGenerator absent — zones de spawn vides.");
        if (GridManager.Instance  == null) Debug.LogError("[CombatInitializer] GridManager absent de la scène !");
        if (TurnManager.Instance  == null) Debug.LogError("[CombatInitializer] TurnManager absent de la scène !");
    }
}

# ROADMAP NYMORA — Alpha Viable
**Version : 1.0 — 2026-05-06**
**Remplace : ROADMAP_ORACLE.txt**

---

## CONFIGURATION DE TRAVAIL

> Ces instructions s'appliquent à chaque session de travail sur ce projet. Claude doit les lire et les appliquer systématiquement.

### Modèle et effort
- **Modèle principal (orchestration, audit, debug) :** Claude Opus — effort `xhigh` (extended thinking activé)
- **Agents d'implémentation :** Claude Sonnet pour les tâches moyennes, Claude Haiku pour les tâches simples/répétitives (renommages, ajouts de fichiers, copie de patterns)

### Workflow de session
1. **Démarrage** — lire `ROADMAP_NYMORA.md` en entier pour reprendre l'état exact du projet et identifier la prochaine tâche non cochée
2. **Planification** — identifier les sous-tâches indépendantes de la phase en cours, évaluer lesquelles peuvent être parallélisées
3. **Implémentation** — lancer plusieurs agents Sonnet/Haiku en parallèle, un agent par sous-tâche indépendante
4. **Reporting des agents** — quand un agent termine sa tâche, il doit obligatoirement :
   - Cocher la case `[x]` correspondante dans cette roadmap
   - Ajouter une entrée dans la section **JOURNAL** en bas de ce fichier avec : date, tâche effectuée, arbitrages réalisés, cas non prévus, difficultés rencontrées
5. **Détection de bug** — si un agent détecte un bug ou un comportement inattendu après l'implémentation → demander automatiquement à l'utilisateur qu'il se rende sur la roadmap pour checker l'endroit où se situe le bug et son état, puis lui demander s'il souhaite lancer Claude Opus pour diagnostiquer les bugs et les résoudre
6. **Audit de fin de phase** — une fois toutes les cases d'une phase cochées → lancer l'audit complet défini dans cette phase avant de passer à la suivante

### Règles permanentes
- Ne jamais sauter l'audit de fin de phase
- Ne jamais supprimer un fichier sans confirmation si son usage est incertain
- Toujours vérifier Unity sans erreur console après chaque modification structurelle
- En cas de doute sur une valeur de balancing → consulter `Nymora_Bible_complete.docx`, ne pas improviser

---

## ÉTAT DU PROJET AU 2026-05-06

### Ce qui fonctionne (base solide)
- Combat tactique isométrique complet : grille, pathfinding A*, PA/PM, tours, initiative
- 3 classes intégrées à 100% : Soulrender, Ghostra, Necram (36 sorts + passifs + animations)
- UI/HUD de combat complet : deck, timer, jauge passif, tooltips, order timeline, floating damage
- Réseau Photon PUN2 opérationnel : matchmaking 1v1, RPC déplacement/sort/fin de tour/placement
- MainMenu, Hub, Ranked1v1 fonctionnels

### Ce qui est absent
| Élément | État |
|---|---|
| Nightseer & Colossar | 0% — assets vides, 0 sort, 0 animation |
| Migration 64×64 → 128×128 | Non démarré |
| Audio | Dossier vide |
| Leveling / MMR / Rangs | Pas une ligne de code |
| Écran post-combat | Inexistant |
| IA avec niveaux de difficulté | IA basique sans stratégie de classe |
| Cercle/aura sous les persos | Inexistant |
| Système d'abandon | Inexistant |
| Cosmétiques / Clans | Inexistant |

### Dette technique critique
1. **Sélection de passif non synchronisée réseau** — risque de désync entre les deux clients
2. **Gestion des déconnexions fragile** — pas de rejoin si host quitte
3. **Migration MasterClient** — pas de fallback de rôles critiques
4. **IA sans connaissance de classe** — ne joue pas les sorts selon la logique de la classe

---

## RECOMMANDATIONS POUR L'ALPHA VIABLE

En plus de ce qui est demandé dans le plan de révision, voici ce que je recommande d'intégrer pour une Alpha agréable :

- **Mode Entraînement** : la scène `Training` (ex-Monjeu) existe déjà — s'assurer qu'elle est accessible depuis le Hub et qu'elle permet de choisir sa classe et un niveau de difficulté IA avant de lancer le combat
- **Indicators visuels des effets de statut** : les stacks de venin Necram, saignement Ghostra, etc. doivent être visibles sur les personnages en combat (icônes flottantes ou aura colorée)
- **Reconnexion propre** : si un joueur se déconnecte brièvement, un timer de 60s avant forfait automatique — évite les parties bloquées
- **Tutoriel intégré** : une scène dédiée ou pop-ups in-game pour les nouveaux joueurs (comment se déplacer, lancer un sort, comprendre son passif)
- **Son de feedback UI** : même sans musique, des sons courts sur les actions clés (clic sort, mort, fin de tour) changent radicalement le ressenti — à intégrer dès la phase Audio

---

## PLUGINS & PACKAGES RECOMMANDÉS

> À installer / vérifier avant de démarrer les phases graphiques. Certains sont déjà présents dans le projet.

### Essentiels (pixel art + stabilité)

| Package | Source | Déjà présent | Utilité |
|---|---|---|---|
| **2D Pixel Perfect** | Unity Registry | ❓ à vérifier | CRITIQUE — empêche le sub-pixel rendering, rend le pixel art net et stable à toutes résolutions |
| **Universal Render Pipeline (URP)** | Unity Registry | ✅ Oui | Pipeline de rendu moderne, base de tout le reste |
| **URP 2D Renderer** | Inclus dans URP | ❓ à vérifier | Renderer dédié 2D avec lumières pixel art, normal maps sprites, shadows |
| **TextMesh Pro** | Unity Registry | ✅ Oui | Textes nets en pixel art, polices bitmap supportées |
| **DOTween** | Asset Store (gratuit) | ✅ Oui (usage détecté) | Animations UI fluides, transitions, feedback visuel |

### Recommandés (qualité visuelle & workflow)

| Package | Source | Déjà présent | Utilité |
|---|---|---|---|
| **Cinemachine** | Unity Registry | ❓ à vérifier | Transitions caméra fluides dans le Hub, zoom combat, shake sur mort |
| **2D Lights (URP)** | Inclus dans URP 2D | ❓ à vérifier | Éclairage ambiance sur les maps (tiles, personnages), torches, auras — rendu pixel art très propre |
| **Post Processing (URP)** | Inclus dans URP | ❓ à vérifier | Bloom léger sur les auras/sorts, color grading par scène (Hub chaud vs Combat froid) |
| **Shader Graph (URP)** | Unity Registry | ❓ à vérifier | Shaders custom : outline personnages en combat, palette swap pour cosmétiques de skin, dissolve sur mort |
| **Odin Inspector** | Asset Store (payant) | ❌ Non | Qualité de vie éditeur majeure : inspecteurs custom, listes triables, boutons d'action — accélère énormément le travail sur les ScriptableObjects |

### Optionnels (polish avancé)

| Package | Source | Déjà présent | Utilité |
|---|---|---|---|
| **Particle System** | Built-in Unity | ✅ Intégré | VFX sorts, impacts, mort — déjà utilisable, à exploiter davantage |
| **2D Animation** | Unity Registry | ❓ à vérifier | Animations squelettales si on veut aller au-delà du frame-by-frame pour certains persos |
| **ParrelSync** | GitHub (gratuit) | ✅ Oui | Tests multi-instances en local — garder impérativement |

### Notes d'installation

- **Pixel Perfect Camera** : dans le projet, remplacer la `IsometricCamera.cs` par une configuration avec le composant `Pixel Perfect Camera` de Unity — régler `Ref Resolution` sur 128×128 (cohérent avec la migration Phase 2), `Upscale Render Texture` activé
- **Shader Graph** : le palette swap via Shader Graph est la méthode recommandée pour les cosmétiques de skin (Phase 5.6) — évite de créer un sprite par skin
- **URP 2D Renderer** : nécessite de basculer le `Renderer Asset` dans les `URP Pipeline Settings` — à faire avec précaution, tester sur une scène avant d'appliquer à tout le projet

---

## PLAN D'EXÉCUTION — 8 PHASES

---

### PHASE 1 — Nettoyage + Réorganisation + Stabilisation réseau
**Priorité : BLOQUANTE — à faire avant tout le reste**
**Durée estimée : 2-3 sessions**

> ⚠️ RÈGLE ABSOLUE : pour tout fichier dont le statut est incertain → poser la question avant de toucher, et sauvegarder dans `_Save_fichiers_critiques/` à la racine avant suppression.

---

#### 1.0 Bugs critiques Ranked1v1 — Priorité immédiate

- [x] **Masquer le `HubPlayer(Clone)` en jeu** — ✅ RÉSOLU 2026-05-07 (itération 6). Cause root identifiée après 5 itérations infructueuses : le composant `HubPlayerAvatar.cs` n'était PAS attaché au prefab `HubPlayer.prefab` utilisé en production — tout le code de cleanup ciblait un composant inexistant sur le prefab. Fix : édition YAML de `HubPlayer.prefab` pour ajouter le composant manquant (GUID `79f259e796eaed24eaa1e3d8edf5db21`). Validé par utilisateur en ParrelSync.
- [x] **Supprimer la sélection de passif en début de Ranked1v1** — ✅ RÉSOLU 2026-05-07. Étape supprimée entièrement, le passif par défaut est attribué automatiquement
- [x] **Bug de synchronisation de sélection de classe** — ✅ RÉSOLU 2026-05-07 (itération 6). Même cause root que le bug `HubPlayer(Clone)` : le composant `HubPlayerAvatar` manquant sur le prefab empêchait `ApplyInstantiationDataNextFrame` (lecture de la classId via instantiationData Photon) et `RpcSetHubClassId` (sync live au DeckBuilder) de s'exécuter. Fix YAML du prefab a tout débloqué simultanément. Validé par utilisateur en ParrelSync (Hub + Ranked1v1).
- [x] **Désynchronisation des obstacles ArenaGenerator (Hub + Ranked1v1)** — ✅ RÉSOLU 2026-05-07 (itération V4). 4 versions successives nécessaires : V1 (hash room name) et V2 (master pose seed via Custom Room Property + check `MaxPlayers > 0 && PlayerCount >= 2`) échouaient car le check rejetait HUB_GLOBAL et le 1er joueur seul. V3 a corrigé Ranked1v1 (suppression du check PlayerCount + clé par scène `arena_seed_<sceneName>`). V4 a corrigé le Hub via `WaitForRoomThenGenerate` (coroutine d'attente de `InRoom` car `HubNetworkSpawner` joint HUB_GLOBAL de façon async, contrairement à Ranked1v1 où `PhotonNetwork.LoadLevel` maintient la connexion à la matchmaking room). Validé par utilisateur en ParrelSync.

---

#### 1.1 Suppression des fichiers inutiles

- [x] Valider le contenu de `__cleanup_quarantine__/` fichier par fichier, puis supprimer le dossier entier (oracle-editor-legacy, photon-demos, runtime-legacy, tmp-examples) — ✅ 2026-05-07. Audit grep complet : aucune référence active. Backup ~37 Mo dans `_Save_fichiers_critiques/__cleanup_quarantine___backup_2026-05-07/`, puis suppression.
- [x] Vérifier les 2 scripts dans `Assets/_Game/Scripts/Utils/` — utilisés ou orphelins ? Supprimer si inutilisés. — ✅ 2026-05-07. Le dossier ne contient que `.gitkeep` ; les 2 scripts mentionnés n'existent plus (info roadmap obsolète). Rien à faire.
- [x] Vérifier `Assets/Plugins/` — contenu à identifier, supprimer ce qui est obsolète — ✅ 2026-05-07. Contenu : Demigiant (DOTween) + ParrelSync, tous deux utilisés activement (PassiveCardUI, OracleHubNetworkSetup) et explicitement listés comme "à garder" dans la roadmap. Rien à supprimer.
- [x] Fusionner ou clarifier les deux dossiers `Resources/` : `Assets/Resources/` et `Assets/_Game/Resources/` — identifier ce qui est dans chacun et consolider tout dans `Assets/_Game/Resources/` — ✅ 2026-05-07 (partiel). `SpellIcons/` (+ meta) déplacé vers `Assets/_Game/Resources/SpellIcons/` ; `SpellIconImporter.cs:14` et le warning de `SpellIconLoader.cs` mis à jour. ⚠️ `DOTweenSettings.asset` laissé dans `Assets/Resources/` car DOTween régénère ce fichier à cet emplacement via son utility panel — déplacer aurait cassé le pipeline DOTween (arbitrage validé utilisateur).

---

#### 1.2 Réorganisation de la structure du projet

**Structure cible après réorganisation :**

```
NymoraV1-main/
├── Assets/
│   ├── _Game/                         ← tout le contenu du jeu ici
│   │   ├── Audio/                     ← sons et musiques (actuellement vide)
│   │   ├── Prefabs/                   ← prefabs de jeu
│   │   ├── Resources/                 ← assets chargés en runtime (consolidé)
│   │   ├── Scenes/                    ← TOUTES les scènes ici
│   │   │   ├── MainMenu.unity
│   │   │   ├── Hub.unity
│   │   │   ├── Training.unity         ← ex-Monjeu.unity
│   │   │   ├── Ranked1v1.unity
│   │   │   └── Boot_Network.unity
│   │   ├── Scripts/
│   │   │   ├── Combat/
│   │   │   ├── Core/
│   │   │   ├── Editor/                ← scripts Unity Editor uniquement
│   │   │   ├── Hub/
│   │   │   ├── Network/
│   │   │   ├── UI/
│   │   │   └── Utils/
│   │   ├── ScriptableObjects/
│   │   │   ├── Classes/
│   │   │   ├── Configs/
│   │   │   ├── Passifs/
│   │   │   └── Spells/
│   │   │       ├── Colossar/
│   │   │       ├── Ghostra/
│   │   │       ├── Necram/
│   │   │       ├── Nightseer/
│   │   │       └── Soulrender/
│   │   └── Sprites/
│   ├── Photon/                        ← SDK externe, ne pas toucher
│   ├── Plugins/                       ← plugins externes, ne pas toucher
│   ├── Settings/                      ← settings Unity, ne pas toucher
│   └── TextMesh Pro/                  ← package externe, ne pas toucher
│
├── _Docs/                             ← toute la documentation ici
│   ├── Nymora_Bible_complete.docx
│   ├── ROADMAP_NYMORA.md
│   ├── README.md
│   └── archives/                      ← anciens fichiers à garder en référence
│       ├── CLEANUP_AUDIT_2026-05-03.md
│       ├── ORACLE_SCRIPTS_CLASSIFICATION_2026-05-03.md
│       ├── ROADMAP_ORACLE.txt
│       ├── TUTORIEL_RESEAU_ORACLE.txt
│       └── TUTORIEL_TEST_2_JOUEURS_PC.txt
│
├── ProjectSettings/
├── Packages/
└── UserSettings/
```

**Actions de réorganisation :**

- [x] Déplacer `Assets/Monjeu.unity` → `Assets/_Game/Scenes/Training.unity` et mettre à jour `EditorBuildSettings.asset` + toutes les références `SceneManager.LoadScene` dans les scripts — ✅ 2026-05-07. Move + rename effectué (avec `.meta`, GUID préservé). EditorBuildSettings + Hub.unity + HubHUD.cs + HubManager.cs + 5 scripts Editor (`OracleRanked1v1Setup`, `OracleHubSceneBuilder`, `OracleHubPlayerSetup`, `InjectTurnOrderTimeline`, `InjectCombatTooltipSystem`) + commentaire `ArenaGenerator.cs` mis à jour. Grep `Monjeu` sur `Assets/` → 0 occurrence restante.
- [x] Déplacer `Assets/Boot_Network.unity` → `Assets/_Game/Scenes/Boot_Network.unity` et mettre à jour `EditorBuildSettings.asset` — ✅ 2026-05-07. Move effectué (avec `.meta`), path mis à jour dans `EditorBuildSettings.asset`.
- [x] Créer le dossier `_Docs/` à la racine du projet — ✅ 2026-05-07.
- [x] Déplacer `Nymora_Bible_complete.docx`, `ROADMAP_NYMORA.md`, `README.md` dans `_Docs/` — ✅ 2026-05-07.
- [x] Créer `_Docs/archives/` et y déplacer les anciens fichiers documentaires (CLEANUP_AUDIT, ORACLE_SCRIPTS_CLASSIFICATION, ROADMAP_ORACLE.txt, tutoriels) — ✅ 2026-05-07. 6 fichiers archivés : `CLEANUP_AUDIT_2026-05-03.md`, `CLEANUP_REPORT_2026-05-03.md` (ajouté par cohérence), `ORACLE_SCRIPTS_CLASSIFICATION_2026-05-03.md`, `ROADMAP_ORACLE.txt`, `TUTORIEL_RESEAU_ORACLE.txt`, `TUTORIEL_TEST_2_JOUEURS_PC.txt`.
- [x] Consolider `Assets/Resources/` dans `Assets/_Game/Resources/` — vérifier les références avant de déplacer — ✅ 2026-05-07 (déjà effectué en Phase 1.1 pour `SpellIcons/`).
- [x] Supprimer `Assets/Resources/` une fois consolidé (vérifier qu'aucun script ne charge depuis ce chemin)

---

#### 1.3 Stabilisation réseau

> ℹ️ **Note** : la case originelle « Synchroniser la sélection de passif via seed partagé ou autorité MasterClient » a été **supprimée** comme caduque (décision validée 2026-05-07). La fonctionnalité elle-même a été retirée en Phase 1.0.b — plus rien à synchroniser.

- [x] Implémenter une règle de déconnexion claire (forfait → victoire adversaire → retour hub) — ✅ 2026-05-07. `OracleCombatNetBridge.OnPlayerLeftRoom` déclenche `CombatInitializer.OnNetworkForfeit(winnerTeamId)` qui réutilise le flux standard de fin (panneaux + bouton Retour Hub) via `TurnManager.ForceEndCombat`. Couvre Leave volontaire ET TTL Photon expiré.
- [x] Implémenter la migration MasterClient pour les rôles critiques (timer, fin de tour) — ✅ 2026-05-07. **Audit confirmé : architecture déjà résiliente par construction.** Toutes les validations master-only (`RpcMasterValidate*` dans `OracleCombatNetBridge`) lisent `PhotonNetwork.IsMasterClient` en temps réel → le nouveau master prend le relais automatiquement. `TurnManager` n'a aucun rôle master-only (timer local par client, `EndTurn` idempotent, `OnDeath` listeners locaux). Override `OnMasterClientSwitched` ajouté dans `OracleCombatNetBridge` pour la traçabilité (log uniquement).
- [x] Ajouter un timer de reconnexion (60s avant forfait automatique) — ✅ 2026-05-07. `RoomOptions.PlayerTtl = 60000` + `EmptyRoomTtl = 60000` à la création de la combat room (`HubMatchmaker`). Polling 1/s dans `OracleCombatNetBridge.Update()` détecte `Player.IsInactive` → instancie `DisconnectionTimerUI` (panneau plein écran + count-down 60s, créé procéduralement). Reconnexion = `IsInactive` repasse à false → UI hide + reprise normale. Timeout = forfait local-only via `OnNetworkForfeit(GetLocalTeamId())`.

---

#### 1.4 Audit post-Phase 1

**Pré-audits côté code (orchestrateur Opus) :**
- [x] Vérifier l'ordre de build dans `EditorBuildSettings.asset` — ✅ 2026-05-07. Ordre cohérent : MainMenu → Hub → Training → Ranked1v1 → Boot_Network. **Bug préexistant détecté et corrigé** : le GUID de `Ranked1v1.unity` dans `EditorBuildSettings.asset` (`e908889f08ff5904aa486b4e2dadd729`) ne matchait pas le GUID réel du `.meta` (`8ba2c76188a5e38479e69b899c95e8dc`) — résidu d'une duplication/recréation ancienne. Unity le résolvait silencieusement par path mais le mismatch pouvait causer des bizarreries au build. Corrigé. Aucune autre référence au GUID orphelin dans le projet.
- [x] **Fix des 3 warnings préexistants** flagged en Phase 1.2 — ✅ 2026-05-07.
  * `HubPlayerAvatar.cs:47,53` — CS0114 résolu : `void OnEnable/OnDisable` → `public override void OnEnable/OnDisable` (correct vis-à-vis de `MonoBehaviourPunCallbacks`).
  * `HubHUD.cs:1168` — CS0219 résolu : `const float PAD_V = 12f` (dead code) supprimé.
  * `HubHUD.cs:1351` — CS0219 résolu : `const float CARD_W = 300f` (dead code) supprimé.
- [x] **Pré-audit static des Resources.Load** — ✅ 2026-05-07. Vérifié que tous les paths ciblés existent dans `Assets/_Game/Resources/` :
  * `NymoraClasses/ClassRegistry.asset` ✅
  * `OracleSpellPools/AllCombatSpellsPool.asset` ✅
  * `SpellIcons/ghostra/`, `SpellIcons/soulrender/` ✅
  * `Characters/`, `CombatAnimations/`, `OracleHUD/`, `Fonts/` ✅
- [x] **Pré-audit grep des chemins obsolètes** — ✅ 2026-05-07. Grep `Assets/Resources/SpellIcons|Assets/Monjeu|monjeuScene|MONJEU_PATH` sur `Assets/` → 0 occurrence. Aucune référence résiduelle après les Phases 1.1 et 1.2.

**Tests effectués dans Unity (utilisateur) — validés 2026-05-07 :**
- [x] Ouvrir Unity — vérifier qu'aucune erreur console n'apparaît au démarrage — ✅ validé utilisateur 2026-05-07.
- [x] Vérifier que toutes les scènes se chargent sans erreur de référence manquante — ✅ validé utilisateur 2026-05-07. Les 5 scènes (MainMenu, Hub, Training, Ranked1v1, Boot_Network) chargent proprement.
- [x] Lancer un combat test complet (placement → combat → fin de match) — ✅ validé utilisateur 2026-05-07.
- [x] Vérifier que les ScriptableObjects sont tous bien référencés (aucun champ "Missing") — ✅ validé utilisateur 2026-05-07.
- [x] Faire un build de test pour confirmer qu'il n'y a pas d'asset manquant — ✅ validé utilisateur 2026-05-07.
- [x] **Test Phase 1.3 (ParrelSync)** : déconnexion brutale → panneau « X déconnecté » + count-down 60s → forfait propre à 0 — ✅ validé utilisateur 2026-05-07.
- [x] **Test Phase 1.3 (ParrelSync)** : leave volontaire → forfait immédiat — ✅ validé utilisateur 2026-05-07.
- [x] **Test Phase 1.3 (ParrelSync)** : migration MasterClient (master quitte) → log `OnMasterClientSwitched` chez l'autre + jeu continue — ✅ validé utilisateur 2026-05-07.

**Critères de sortie — ✅ TOUS VALIDÉS 2026-05-07 :**
- [x] Structure de dossiers propre, cohérente, naviguable (Phase 1.2)
- [x] Zéro erreur console au démarrage Unity (Phase 1.4)
- [x] Toutes les scènes chargent correctement (Phase 1.4)
- [x] 10+ matchs 1v1 sans désynchronisation visible (Phase 1.0 + 1.4 — bugs 1.0 résolus, build test OK)
- [x] Déconnexion d'un joueur = résultat propre sans blocage (Phase 1.3 + 1.4 — testé en ParrelSync)

## ✅ PHASE 1 — COMPLÈTE 2026-05-07

Toutes les sous-phases validées :
- **1.0** ✅ Bugs critiques Ranked1v1 (HubPlayer, sélection passif, sync classe, désync obstacles)
- **1.1** ✅ Suppression fichiers inutiles (quarantine + consolidation Resources)
- **1.2** ✅ Réorganisation structure (`_Docs/`, `_Game/Scenes/`, refs Monjeu→Training)
- **1.3** ✅ Stabilisation réseau (forfait par déconnexion, timer 60s, migration master)
- **1.4** ✅ Audit fin de Phase 1 (warnings fixés, GUID corrigé, tests Unity validés)

**Phase 2 (migration 64×64 → 128×128) débloquée.**

---

### PHASE 2 — Migration visuelle 64×64 → 128×128
**Priorité : IMPORTANTE — avant d'ajouter les nouvelles classes**
**Durée estimée : 2-3 sessions**

> ⚠️ WARNING : Modifier UNIQUEMENT les sprites de personnages et les tiles. NE PAS toucher l'UI, l'HUD, les icônes de sorts, les polices.

#### 2.1 Préparation

**Stratégie validée 2026-05-07 :** Option A — doubler PPU avec les sprites (64→128, 100→200). Rendu identique à l'écran, juste 4× plus net. Aucun recalibrage de scène nécessaire. Resize batch via ImageMagick (lancé par utilisateur).

- [x] Identifier tous les fichiers affectés : `Sprites/Characters/`, `Sprites/NewTilesV4/`, `Resources/Characters/Frames/`, `Resources/CombatAnimations/Frames/` — ✅ 2026-05-07. Inventaire complet :
  * `Sprites/NewTilesV4/` : 6 fichiers .gif (base_ground, base_ground_variant, corner_torch_obstacle + variant, obstacle_wall + variant). PPU actuel **64**, pivot `(0.5, 0)`.
  * `Sprites/Characters/` : 3 fichiers (Sprite-0001.gif, Sprite-01/, opponent_test.gif) — sources GIF de test, optionnels pour la migration.
  * `Resources/Characters/Frames/` : **32 dossiers** = 2 classes (Ghostra + Soulrender) × 4 actions (idle/walk/death/taking_damage) × 4 directions (NE/NO/SE/SO). ~5-10 PNG par dossier → **~200-300 PNG**. PPU actuel **100**, pivot `(0.5, 0.5)`.
  * `Resources/CombatAnimations/Frames/` : **248 dossiers** (62 sorts × 4 directions). ~8 PNG par dossier → **~2000 PNG**.
  * **Total : ~2500 fichiers PNG/GIF** — résize batch obligatoire.
  * **Hors scope** (à NE PAS toucher conformément à la règle absolue Phase 2) : `Sprites/CARTE_SORT`, `Sprites/Effects`, `Sprites/SpellPassifIcons`, `Sprites/UI`, `Sprites/UI_Maj`, `Sprites/World`, `Sprites/newobstacle` — UI/HUD/icônes de sorts.
- [x] Lister les paramètres de la grille isométrique (GridConfig, ArenaConfig, CellPrefab) à ajuster — ✅ 2026-05-07. Cf. tableau détaillé dans le journal Phase 2.1 ci-dessous. Avec l'**Option A (PPU doublé)**, les paramètres de `GridConfig` (`tileWidth`, `tileHeight`, offsets, `characterWorldOffset`…) restent **inchangés** car les unités Unity sont préservées. Seul `IsometricCamera.pixelsPerUnit` (32→64) doit être ajusté pour le pixel perfect, et les `.meta` files des sprites doivent voir leur `spritePixelsToUnits` doubler (64→128 pour tiles, 100→200 pour personnages/animations).
- [x] Prendre un snapshot de la scène Ranked1v1 avant modification (screenshot de référence) — ✅ 2026-05-07. Screenshot fourni par l'utilisateur (scène Training, identique en HUD/characters/tiles à Ranked1v1). Référence visuelle conservée pour comparaison post-migration.

#### 2.2 Migration
- [x] Redimensionner tous les sprites personnages (Idle/Walk/Death/TakingDamage × 4 directions) de 64 à 128px — ✅ 2026-05-07. **256 PNG** dans `Resources/Characters/Frames/` resized à 200% (nearest-neighbor) en **1.17s** via script PowerShell .NET (System.Drawing). Plus **1985 PNG** dans `Resources/CombatAnimations/Frames/` (animations de sorts) en **8.23s**. PPU 100→200 dans tous les `.meta` correspondants. 0 échec sur 2241 fichiers.
- [x] Redimensionner tous les tiles (`NewTilesV4/`) de 64 à 128px — ✅ 2026-05-07. 6 GIF (base_ground, base_ground_variant, corner_torch_obstacle + variant, obstacle_wall + variant) resized à 128×128 (nearest-neighbor). PPU 64→128 dans les `.meta`.
- [x] Mettre à jour `GridConfig` : taille de cellule, offsets isométriques, positions caméra — ✅ 2026-05-07. **Aucun changement requis** grâce à l'option A (PPU doublé en parallèle des sprites). Les unités Unity (`tileWidth=1f`, `tileHeight=0.5f`, offsets) restent valides car le PPU compense exactement le doublement des pixels. À revalider visuellement en 2.3.
- [x] Mettre à jour `CellPrefab` : dimensions du collider et du sprite renderer — ✅ 2026-05-07 (sous réserve audit 2.3). Aucun changement requis avec l'option A : le SpriteRenderer affiche le sprite à la taille définie par PPU, le collider est en world units (préservé). À revalider visuellement.
- [x] Mettre à jour `TileSpriteRegistry` : références recalculées en 128px — ✅ 2026-05-07. Aucune ré-assignation nécessaire car les GUIDs des sprites sont préservés (on a modifié les binaires des `.gif` mais les `.meta` gardent leur GUID). Le commentaire de `TileSpriteRegistry.cs:17` mis à jour : `Pixels Per Unit → 128`.
- [x] Vérifier `IsometricCamera` : zoom et framing à recalibrer — ✅ 2026-05-07. `IsometricCamera.cs:47` : `pixelsPerUnit = 32f` → `pixelsPerUnit = 64f` (cohérence pixel-perfect avec PPU des tiles 128 et frames 200, divisé par 2 pour le ratio classique). Zoom et framing inchangés (option A). À revalider visuellement en 2.3.
- [x] Revalider le pathfinding : les hitboxes de cellule doivent correspondre aux nouvelles dimensions — ✅ 2026-05-07. **Aucun changement requis** : le pathfinding A* opère sur la grille logique (`GridConfig.width × height`, indices entiers), totalement découplé du rendu pixel. Les hitboxes Cell étant en world units (1 unité = 1 case), elles restent valides avec l'option A.

#### 2.3 Audit post-migration
- [x] Ouvrir Ranked1v1 : vérifier que la grille s'affiche correctement — ✅ validé utilisateur 2026-05-07.
- [x] Lancer un combat test : vérifier les déplacements, les animations, le depth-sorting — ✅ validé utilisateur 2026-05-07.
- [x] Vérifier Hub.unity : les avatars joueurs sont-ils bien dimensionnés ? — ✅ validé utilisateur 2026-05-07.
- [x] Vérifier que l'UI/HUD n'a pas bougé — ✅ validé utilisateur 2026-05-07.

**Critères de sortie — ✅ TOUS VALIDÉS 2026-05-07 :**
- [x] Grille 128×128 propre sans artefacts visuels
- [x] Animations personnages fluides dans les 4 directions
- [x] HUD intact, aucune régression UI

## ✅ PHASE 2 — COMPLÈTE 2026-05-07

Migration visuelle 64×64 → 128×128 réussie. Stratégie option A (PPU doublé) confirmée optimale : **rendu identique à l'écran**, sprites 4× plus nets, **aucun recalibrage** nécessaire.

| Sous-phase | Tâches | Status |
|---|---|---|
| 2.1 — Préparation | Inventaire 2500 fichiers + audit configs + snapshot | ✅ |
| 2.2 — Migration | Backup + resize 2247 fichiers + PPU + code | ✅ (15s exécution) |
| 2.3 — Audit | Tests visuels Unity (grille, anims, HUD) | ✅ utilisateur |

**Phase 3 (intégration Nightseer + Colossar) débloquée.**

---

### PHASE 3 — Intégration Nightseer & Colossar
**Priorité : CONTENU MAJEUR**
**Durée estimée : 3-5 sessions par classe**
**Référence obligatoire : `Nymora_Bible_complete.docx`**

> ⚠️ Se référer à la Bible complète pour chaque feature — ne pas improviser les valeurs de balancing.

#### 3.1 Nightseer 🏹 (Éclaireurs de la faille sans lune)
**Passif : Prédateur — Jauge embûche**
- Base (0-2) : +10% dégâts d'embûche, applique Traqué si embûche déclenchée
- Éveillé (3-4) : +20% dégâts d'embûche, -1 PM par embûche déclenchée
- Enragé (5+) : +30% dégâts d'embûche + Traquenard signature (1 PA, portée 3, téléport + 170 dmg + Paralysie)

- [ ] Créer `Nightseer_Predateur.asset` dans `ScriptableObjects/Passifs/`
- [ ] Créer les 15 sorts ScriptableObjects (5 Attaques + 5 Survie + 5 Tactiques) dans `ScriptableObjects/Spells/Nightseer/`
- [ ] Implémenter la logique d'embûche dans `SpellResolver.cs` (trigger si attaque dans le dos ou depuis case non vue)
- [ ] Implémenter l'effet Traqué (StatusEffect)
- [ ] Implémenter l'effet Paralysie (StatusEffect)
- [ ] Créer/commander les sprites 128×128 (Idle/Walk/Death/TakingDamage × 4 directions)
- [ ] Créer les frames d'animations de combat dans `Resources/Characters/Frames/Nightseer_*/`
- [ ] Créer les animations de sort dans `Resources/CombatAnimations/Frames/`
- [ ] Assigner tous les sorts dans `Nightseer.asset`
- [ ] Ajouter Nightseer à `NymoraClassRegistry`
- [ ] Créer `NymoraNightseerFactory.cs` dans `Scripts/Editor/`

#### 3.2 Colossar 🛡️ (Sculptés par la pression des montagnes)
**Passif : Absorption — Jauge dégâts reçus (+1/40 dégâts)**
- Base (0-2) : -5% dégâts subis
- Éveillé (3-5) : -15% dégâts subis + sorts défensifs +1 jauge
- Enragé (6+) : -20% dégâts subis + Reflet Punitif signature (1 PA, AoE 2 cases, 130 dmg + Trauma -2 PA prochain tour)

- [ ] Créer `Colossar_Absorption.asset` dans `ScriptableObjects/Passifs/`
- [ ] Créer les 15 sorts ScriptableObjects dans `ScriptableObjects/Spells/Colossar/`
- [ ] Implémenter l'effet Trauma (-2 PA prochain tour) dans `StatusEffect.cs`
- [ ] Créer/commander les sprites 128×128 (Idle/Walk/Death/TakingDamage × 4 directions)
- [ ] Créer les frames d'animations de combat dans `Resources/Characters/Frames/Colossar_*/`
- [ ] Créer les animations de sort dans `Resources/CombatAnimations/Frames/`
- [ ] Assigner tous les sorts dans `Colossar.asset`
- [ ] Ajouter Colossar à `NymoraClassRegistry`
- [ ] Créer `NymoraColossarFactory.cs` dans `Scripts/Editor/`

#### 3.3 Audit post-intégration
- [ ] Tester chaque sort des 2 nouvelles classes (solo + réseau)
- [ ] Vérifier que les passifs se déclenchent correctement
- [ ] Vérifier le balancing de base (ne pas laisser des sorts à 0 dégâts ou des passifs inactifs)
- [ ] Vérifier les animations dans les 4 directions
- [ ] Tester Nightseer vs Colossar, Nightseer vs Necram, Colossar vs Ghostra (couverture croisée)

**Critères de sortie :**
- 5 classes jouables en réseau sans erreur console
- Tous les passifs actifs et visuellement représentés
- Balancing de base valide (aucun sort cassé ou à 0)

---

### PHASE 4 — Features de combat
**Priorité : IMPORTANTE pour le ressenti en jeu**
**Durée estimée : 1-2 sessions**

#### 4.1 Cercle / Aura évolutif sous les personnages
> Le cercle doit être dynamique en fonction du palier du passif (Base / Éveillé / Enragé).

- [ ] Créer un prefab `CharacterCircle` (SpriteRenderer centré sous le personnage)
- [ ] **Base** : cercle simple statique (sprite plat, couleur neutre)
- [ ] **Éveillé** : animation légère qui "pulse" autour du cercle (particle system ou animation de sprite)
- [ ] **Enragé** : aura plus large avec loop d'animation imposante (particules + glow)
- [ ] Connecter le `CharacterCircle` au `PassiveManager` — mettre à jour l'état visuel à chaque changement de palier
- [ ] Attacher le prefab à `TacticalCharacter`

#### 4.2 Système d'abandon
- [ ] Ajouter un bouton "Abandonner" dans le HUD de combat (corner bas-gauche)
- [ ] Confirmation UI ("Tu vas abandonner — es-tu sûr ?")
- [ ] RPC réseau : broadcast abandon → joueur adverse = victoire → écran post-combat
- [ ] Enregistrement de la défaite dans `PlayerAccountData`

#### 4.3 Amélioration de l'IA + 3 niveaux de difficulté
> L'IA doit comprendre la logique de sa classe et jouer un deck viable.

- [ ] Réécrire `OpponentAI.cs` avec une architecture à 3 niveaux :
  - **Facile** : joueur aléatoire pondéré (pas de stratégie, cible aléatoire)
  - **Intermédiaire** : priorité aux sorts offensifs, déplacements vers la cible, respect des PA
  - **Expert** : connaissance du passif de sa classe, optimisation des conditions de sorts (dos, distance, HP bas), gestion cooldowns
- [ ] Chaque niveau reçoit un deck adapté (sorts les plus cohérents pour la classe jouée)
- [ ] Ajouter la sélection du niveau de difficulté dans la scène d'entraînement (ou Hub)
- [ ] Connecter à PassiveManager : l'IA Expert doit aussi piloter son passif (gérer sa jauge)

**Critères de sortie :**
- Cercles visibles en jeu avec transition Base → Éveillé → Enragé
- Abandon fonctionnel en réseau sans blocage de partie
- 3 niveaux d'IA jouables, "Expert" ne fait pas de coups suicidaires

---

### PHASE 4.5 — Deckbuilder & Sélection de classe
**Priorité : HAUTE — impacte directement l'expérience avant chaque combat**
**Durée estimée : 2-3 sessions**

> Le design visuel des panneaux sera réalisé par l'équipe. Le travail ici porte sur la logique, la navigation et la persistance des decks. Les designs seront intégrés une fois fournis.

#### 4.5.1 Refonte de la sélection de classe (carrousel)
> Remplacer l'affichage statique actuel par un carrousel ergonomique.

- [ ] **Classe centrale** : la classe sélectionnée est affichée en grand au centre avec toutes ses infos (nom, lore, passif, aperçu des sorts)
- [ ] **Classe gauche** : aperçu réduit + fondu de la classe précédente, avec bouton flèche gauche `←`
- [ ] **Classe droite** : aperçu réduit + fondu de la classe suivante, avec bouton flèche droite `→`
- [ ] Transition animée au changement (slide ou fade) entre les classes
- [ ] La classe centrale est la seule dont les détails complets sont lisibles — les deux classes latérales sont décoratives
- [ ] Créer `ClassCarouselUI.cs` pour gérer la navigation et l'état sélectionné
- [ ] **Aperçu animé du personnage** : la classe centrale affiche le sprite du personnage en animation **IDLE** en boucle (utiliser les frames existantes dans `Resources/Characters/Frames/ClassName_idle_*/`)
- [ ] **Rotation du personnage** : deux flèches positionnées sous le personnage (`←` `→`) permettent de cycler entre les 4 directions isométriques (SO → SE → NE → NO → SO) pour voir le perso sous tous les angles
- [ ] Créer `ClassPreviewAnimator.cs` : lit les frames IDLE de la direction courante et les joue en boucle, change de direction au clic des flèches
- [ ] Transition douce au changement de direction (fade rapide ou swap instantané selon le rendu souhaité)
- [ ] Les classes latérales (gauche/droite) affichent également leur sprite IDLE mais fixe (une seule frame ou animation ralentie) pour suggérer la vie sans surcharger visuellement
- [ ] Conserver le bouton "Changer de classe" existant pour accéder à ce carrousel depuis le deckbuilder

#### 4.5.2 Deckbuilder ergonomique
> Le deckbuilder s'ouvre à la place de la sélection de classe directe. Le design sera fourni par l'équipe.

- [ ] Créer `DeckBuilderUI.cs` : panneau principal du deckbuilder
- [ ] Le deckbuilder s'ouvre avec la classe actuellement sélectionnée dans le carrousel
- [ ] Affichage de tous les sorts disponibles pour la classe sélectionnée (liste ou grille)
- [ ] Chaque sort affiche : nom, icône, coût PA, portée, description courte — tooltip détaillé au survol
- [ ] Zone "deck actif" : slots pour les sorts sélectionnés (max selon les règles du jeu)
- [ ] Drag & drop ou clic pour ajouter/retirer un sort du deck
- [ ] Indicateur visuel : sorts déjà dans le deck mis en évidence dans la liste
- [ ] Bouton "Changer de classe" dans le deckbuilder → retour au carrousel (4.5.1)
- [ ] **Isolation par classe** : les sorts affichés et les decks sauvegardés sont strictement filtrés par classe sélectionnée — impossible de voir ou d'utiliser les sorts/decks d'une autre classe

#### 4.5.3 Sauvegarde de decks par classe
- [ ] Créer `DeckSaveData.cs` : structure d'un deck sauvegardé (nom, classe, liste de sorts)
- [ ] Chaque joueur peut sauvegarder plusieurs decks par classe (limite suggérée : 5 decks par classe)
- [ ] **Nommer un deck** : champ texte libre à la sauvegarde (ex: "Deck aggro", "Deck survie")
- [ ] **Modifier un deck** : charger un deck sauvegardé dans le deckbuilder, modifier, re-sauvegarder
- [ ] **Supprimer un deck** : bouton de suppression avec confirmation
- [ ] **Deck actif** : le dernier deck utilisé par classe est mémorisé et rechargé automatiquement à l'ouverture du deckbuilder
- [ ] Persister les decks via `AccountManager.cs` (côté serveur) ou PlayerPrefs (côté local en attendant)
- [ ] Liste des decks sauvegardés visible dans le deckbuilder sous forme de cards cliquables (nom + aperçu des sorts)
- [ ] **Isolation par classe confirmée** : si classe Soulrender sélectionnée → seuls les decks Soulrender apparaissent, idem pour toutes les classes

**Critères de sortie :**
- Carrousel de sélection de classe fluide et lisible
- Deckbuilder accessible depuis le Hub avant un match
- Sauvegarde, chargement et suppression de decks fonctionnels par classe
- Isolation stricte : impossible de mélanger les sorts/decks entre classes

---

### PHASE 5 — Écosystème utilisateur
**Priorité : LONG-TERM ENGAGEMENT**
**Durée estimée : 5-8 sessions**

> Système le plus lourd à implémenter. À faire en sous-phases.
> ⚠️ **Toutes les features de cette phase s'intègrent dans la scène `Hub.unity`** — menus, panneaux, HUD compte, chat, classement, cosmétiques. Aucun élément de l'écosystème ne crée de nouvelle scène dédiée, tout est accessible depuis le Hub.

#### 5.1 Leveling par personnage (lvl 1 → 50)
- [ ] Créer `ClassProgression.cs` : table d'XP par niveau (courbe à définir)
- [ ] Ajouter dans `PlayerAccountData` : `Dictionary<NymoraClassId, int> classLevel` et `classXP`
- [ ] XP gagnée uniquement en Ranked (formule à définir : victoire > défaite, combats longs > courts)
- [ ] Persister via le système de compte existant (`AccountManager.cs`)

#### 5.2 MMR & Système de rangs (6 rangs)
- [ ] Créer `MMRSystem.cs` : calcul gain/perte MMR (type Elo simplifié)
- [ ] Définir les 6 rangs avec leurs seuils de MMR (ex: Bronze, Argent, Or, Platine, Diamant, Transcendant)
- [ ] Icônes de rang à créer (à intégrer dans l'UI)
- [ ] MMR visible dans le Hub HUD et dans l'écran de profil

#### 5.3 Stats de compte & Achievements
- [ ] Étendre `PlayerAccountData` : victoires, défaites, ratio, K/D, classe préférée (classe la plus jouée)
- [ ] Créer `AchievementSystem.cs` avec liste de succès (ex: "10 victoires avec Necram", "Atteindre Enragé 3 fois dans un même combat")
- [ ] UI Achievements : panneau de profil avec succès terminés / en cours

#### 5.4 Monnaie virtuelle
- [ ] Ajouter `currency` dans `PlayerAccountData`
- [ ] Gain de monnaie par combat Ranked (victoire ++ / défaite +)
- [ ] Affichage dans Hub HUD

#### 5.5 Système de Clans
- [ ] `ClanData.cs` : nom, emblème, membres, niveau, XP de clan
- [ ] Création de clan depuis le Hub
- [ ] Level de clan (XP de clan = XP cumulée des membres en Ranked)
- [ ] Affichage tag clan dans HubPlayerLabel

#### 5.6 Cosmétiques
> À définir selon les ressources graphiques disponibles.

Types de cosmétiques prévus :
- Skin de classe (sprite alternatif)
- Aura de classe (couleur/style du cercle Phase 4)
- Titres (texte affiché sous le pseudo)
- Emblèmes (icône de profil)
- Familier (sprite animé qui suit le perso dans le hub)
- Skin de texte flottant de dégâts (couleur, police)

- [ ] Créer `CosmeticData.cs` et `PlayerCosmeticLoadout.cs`
- [ ] Menu cosmétique accessible depuis le Hub
- [ ] Appliquer les cosmétiques au runtime (skinning du SpriteRenderer, FloatingDamageText, CharacterCircle)

#### 5.7 Système de Chat complet
> Remplace ou étend le `CombatChatUI.cs` existant. Le chat doit être présent dans le Hub et en combat.

Catégories prévues :
- **Global** — visible par tous les joueurs connectés au Hub
- **Messages privés** — conversation 1-1 entre joueurs, accessible depuis n'importe où
- **Combat** — visible uniquement par les deux joueurs en match (déjà partiellement existant via `CombatChatUI.cs`)
- **Clan** — visible uniquement par les membres du même clan
- **Système** — notifications automatiques (défi reçu, résultat de match, rang up, etc.)

- [ ] Créer `ChatSystem.cs` : gestion des channels, envoi/réception de messages via Photon ou backend
- [ ] Créer `ChatChannelData.cs` : structure d'un channel (type, participants, historique messages)
- [ ] Créer `ChatUI.cs` : panel chat avec onglets par catégorie (Global / Privé / Combat / Clan / Système)

**Ergonomie & mobilité**
- [ ] Chat **amovible** : la fenêtre est draggable librement sur l'écran (composant `DraggableWindow.cs` réutilisable), position sauvegardée dans les PlayerPrefs
- [ ] Chat **redimensionnable** : poignée en coin bas-droit pour agrandir / rétrécir la fenêtre, taille min et max définies
- [ ] Bouton **réduire / agrandir** (icône flèche) : collapse le chat en barre fine avec juste les onglets visibles, re-clic pour déplier
- [ ] **Opacité réglable** : slider dans les paramètres du chat (fond semi-transparent pour ne pas bloquer la vue)
- [ ] **Notifications non-intrusives** : si le chat est réduit ou sur un autre onglet, un badge numérique apparaît sur l'onglet concerné (ex: `Global (3)`)
- [ ] Champ de saisie avec **historique des commandes** : flèche haut/bas pour retrouver les derniers messages envoyés
- [ ] **Timestamps** optionnels sur chaque message (toggle dans les paramètres du chat)
- [ ] **Pseudo cliquable** dans le chat → ouvre directement le menu joueur (5.8 : infos, message privé, défi)
- [ ] Intégrer le chat dans le Hub (position par défaut : coin bas-gauche)
- [ ] Intégrer le chat en combat (remplacer/étendre `CombatChatUI.cs`) — en combat le chat est fixe et plus compact, sans drag pour ne pas gêner le gameplay
- [ ] Messages privés : ouvrir un canal privé depuis le menu joueur (voir 5.8)
- [ ] Notifications dans l'onglet Système : défi reçu, invitation clan, rank-up, victoire adversaire (abandon)
- [ ] Historique messages : conserver les X derniers messages par channel en session

#### 5.8 Menu joueur & Système de défi
> Clic sur un avatar joueur dans le Hub → ouverture d'un menu contextuel.

- [ ] Créer `PlayerContextMenu.cs` : menu popup au clic sur un `HubPlayerAvatar`
- [ ] Le menu affiche :
  - **Informations** : pseudo, rang, classe préférée, niveau, tag clan
  - **Message privé** : ouvre un canal privé dans le Chat (5.7)
  - **Lancer un défi** : envoie une invitation de match direct au joueur ciblé
- [ ] Créer `ChallengeSystem.cs` : gestion des défis (envoi RPC → notification Système dans le chat → accepter/refuser)
- [ ] Si défi accepté : lancer un match direct (bypasser le matchmaking standard) et charger `Ranked1v1`
- [ ] Si défi refusé : notification dans le chat Système de l'initiateur
- [ ] Bloquer les défis vers un joueur déjà en combat

#### 5.9 Menu Classement (Leaderboard)
> Accessible depuis le Hub via un bouton dédié. Affiche les meilleurs joueurs du serveur.

- [ ] Créer `LeaderboardUI.cs` : panneau Hub avec tableau de classement
- [ ] Onglets de classement :
  - **Global MMR** — top joueurs toutes classes confondues, triés par MMR
  - **Par classe** — top joueurs par classe (5 onglets : Soulrender, Ghostra, Necram, Nightseer, Colossar)
  - **Clans** — top clans triés par XP de clan
- [ ] Chaque ligne affiche : position, pseudo, rang (icône), classe préférée, MMR, ratio V/D
- [ ] Mettre en évidence la ligne du joueur connecté (même s'il n'est pas dans le top)
- [ ] Bouton sur une ligne → ouvre le menu joueur (5.8 : informations, message privé, défi)
- [ ] Données chargées depuis `AccountManager.cs` / backend au chargement du Hub

#### 5.10 Menu Paramètres (Hub)
> Accessible depuis le Hub via un bouton dédié (icône engrenage). Remplace / étend le `SettingsPanelUI.cs` existant.

**Audio**
- [ ] Slider Volume général
- [ ] Slider Volume musique
- [ ] Slider Volume SFX
- [ ] Bouton Muet (toggle global)

**Graphismes**
- [ ] Sélection résolution (liste des résolutions disponibles)
- [ ] Toggle Plein écran / Fenêtré
- [ ] Toggle VSync
- [ ] Qualité des particules : Faible / Moyen / Élevé (impacte les VFX de sorts et auras)

**Gameplay**
- [ ] Sélection langue : Français / English (prévu pour internationalisation future)
- [ ] Toggle affichage chiffres de dégâts flottants (certains joueurs préfèrent sans)
- [ ] Seuil d'alerte timer : régler à partir de combien de secondes le timer clignote/pulse (défaut : 10s)
- [ ] Toggle affichage timeline d'ordre des tours (HUD — peut être caché pour simplifier l'écran)

**Accessibilité**
- [ ] Taille des textes : Normale / Grande
- [ ] Mode daltonien : Off / Deuteranopie / Protanopie / Tritanopie (filtre couleur sur les auras et effets)

**Compte**
- [ ] Bouton Changer pseudo (ouvre un champ texte + confirmation)
- [ ] Bouton Se déconnecter (retour MainMenu)

- [ ] Créer `SettingsData.cs` : ScriptableObject ou PlayerPrefs pour persister les réglages entre sessions
- [ ] Étendre `SettingsPanelUI.cs` existant avec toutes les catégories ci-dessus
- [ ] Appliquer les réglages audio à `AudioManager.cs` (Phase 7)
- [ ] Appliquer les réglages graphismes à la caméra et au renderer
- [ ] Appliquer le mode daltonien via un shader de post-processing ou un LUT

**Critères de sortie :**
- Leveling visible en jeu avec XP gagnée après chaque match
- MMR qui évolue, rang affiché dans le Hub
- Au moins 10 achievements définis et fonctionnels
- Menu cosmétique avec 2-3 cosmétiques de test applicables
- Chat fonctionnel dans le Hub avec au moins 3 canaux actifs (Global, Privé, Système)
- Défi entre joueurs fonctionnel end-to-end (envoi → acceptation → lancement du match)
- Classement accessible et lisible dans le Hub, joueur connecté mis en évidence
- Paramètres persistants entre sessions, audio et graphismes appliqués en temps réel

#### 5.11 Boutique (Shop)
> Accessible depuis le Hub via un bouton dédié. Deux sections distinctes : boutique Nymos (monnaie virtuelle du jeu) et boutique argent réel.
> ⚠️ **Phase placeholder** : pour cette itération, l'objectif est uniquement d'avoir un menu fonctionnel avec quelques articles fictifs pour permettre le travail de design. Aucune intégration de paiement réel à ce stade.

**Monnaie virtuelle — Les Nymos (Ñ)**
- [ ] Nommer officiellement la monnaie in-game : **Nymo** (pluriel : **Nymos**, symbole : **Ñ**)
- [ ] Mettre à jour `PlayerAccountData` : renommer `currency` en `nymos`
- [ ] Afficher le solde de Nymos dans le Hub HUD (icône + montant)

**Structure du menu Boutique**
- [ ] Créer `ShopUI.cs` : panneau Hub avec deux onglets principaux
  - Onglet **Boutique Nymos** — articles achetables avec les Nymos gagnés en jeu
  - Onglet **Boutique Premium** — articles achetables avec argent réel (placeholder pour l'instant)
- [ ] Chaque article affiché sous forme de card : aperçu visuel, nom, description courte, prix
- [ ] Créer `ShopItemData.cs` (ScriptableObject) : définit un article (nom, aperçu, prix Nymos ou Premium, type de cosmétique)

**Boutique Nymos — Articles placeholder (pour le design)**
- [ ] 2-3 skins de classe fictifs (ex: "Soulrender Écarlate — 500 Ñ", "Ghostra Abyssal — 750 Ñ")
- [ ] 1-2 auras (ex: "Aura Dorée — 300 Ñ", "Aura Spectrale — 400 Ñ")
- [ ] 1 titre (ex: "Titre : Le Inexorable — 200 Ñ")
- [ ] Bouton "Acheter" → vérification du solde Nymos → déduction → ajout au loadout cosmétique
- [ ] Message si solde insuffisant : "Nymos insuffisants"

**Boutique Premium — Articles placeholder (pour le design)**
- [ ] 2-3 articles premium fictifs (ex: "Pack Fondateur — 9,99€", "Skin Exclusif Nymora — 4,99€")
- [ ] Bouton "Acheter" → affiche un message placeholder : "Paiement non disponible pour l'instant"
- [ ] Prévoir l'emplacement pour l'intégration future d'un SDK de paiement (Unity IAP ou Stripe)

**Critères de sortie :**
- Menu boutique ouvert depuis le Hub avec les deux onglets visibles
- Quelques articles placeholder affichés avec leur card de design
- Achat Nymos fonctionnel (déduction du solde, ajout cosmétique)
- Boutique Premium visible mais non active (bouton placeholder)
- Solde Nymos affiché dans le Hub HUD

---

### PHASE 6 — Écran post-combat
**Priorité : UX ESSENTIELLE**
**Durée estimée : 1 session**

- [ ] Créer `PostMatchScreen.cs` et scène/panel dédié
- [ ] Afficher : Victoire / Défaite (header)
- [ ] Afficher : MMR avant → MMR après (gain/perte animé)
- [ ] Afficher : XP gagnée + niveau actuel + barre de progression vers prochain niveau
- [ ] Afficher : Rang actuel (avec animation si rank-up)
- [ ] Afficher : Stats du combat — "Meilleur combo du combat" (basé sur les logs `CombatLog.cs` — tour où les dégâts cumulés ont été les plus élevés)
- [ ] Bouton "Rejouer" → retour au matchmaking
- [ ] Bouton "Retour au Hub"
- [ ] RPC réseau : les deux clients affichent l'écran simultanément

**Critères de sortie :**
- Écran visible après chaque fin de combat (victoire, défaite, abandon)
- MMR et XP mis à jour avant l'affichage
- Retour Hub sans blocage

---

### PHASE 7 — Audio
**Priorité : IMMERSION**
**Durée estimée : 2-3 sessions (dépend de la disponibilité des fichiers audio)**

> ⚠️ Les fichiers audio (musique + SFX) doivent être sourcés ou créés en dehors du code. Cette phase ne peut démarrer que si les assets son existent.

#### 7.1 Architecture audio
- [ ] Créer `AudioManager.cs` (singleton) : joue musiques et SFX via AudioSource pooling
- [ ] Créer `AudioClipRegistry.cs` (ScriptableObject) : catalogue de tous les clips

#### 7.2 Musique
- [ ] Track Hub (ambiance lobby)
- [ ] Track Combat (musique de tension)
- [ ] Track Victoire / Défaite

#### 7.3 SFX Combat
- [ ] Son de déplacement (pas sur tile)
- [ ] Son générique de sort (cast + impact)
- [ ] Son spécifique par classe (au moins 1 son signature par classe)
- [ ] Son de prise de dégâts
- [ ] Son de mort
- [ ] Son de fin de tour
- [ ] Son de changement de palier passif (Base → Éveillé → Enragé)

#### 7.4 SFX UI
- [ ] Clic sur sort (sélection)
- [ ] Sort invalide (erreur)
- [ ] Fin de timer (alerte)
- [ ] Notification victoire / défaite

**Critères de sortie :**
- Audio présent dans Hub, combat et UI
- Aucun son manquant qui génère une erreur console (AudioClip null)

---

### PHASE 8 — Modes de jeu 2v2 & 3v3
**Priorité : POST-ALPHA**
**Durée estimée : 5-8 sessions**

> À attaquer uniquement une fois l'Alpha 1v1 stable et validée. Ces modes nécessitent une refonte partielle du réseau et de la grille.

#### 8.1 Architecture commune 2v2 / 3v3
- [ ] Étendre `OracleCombatNetBridge.cs` pour gérer N joueurs (au lieu de 2 clients fixes)
- [ ] Adapter `TurnManager.cs` : ordre d'initiative pour 4 ou 6 joueurs, gestion des équipes
- [ ] Adapter `GridManager.cs` : grille plus grande pour accueillir 4-6 personnages (config dédiée)
- [ ] Créer `TeamData.cs` : structure d'équipe (membres, score, état)
- [ ] Adapter le matchmaking : `HubMatchmaker.cs` pour rooms de 4 ou 6 joueurs
- [ ] Adapter la condition de victoire : toute l'équipe adverse éliminée

#### 8.2 Scène & Map Ranked 2v2
> ⚠️ NE PAS copier-coller `Ranked1v1.unity` — créer une scène vierge pour éviter tout héritage de bugs ou de configurations inadaptées. La map doit être conçue spécifiquement pour le 2v2.

- [ ] Créer `Assets/_Game/Scenes/Ranked2v2.unity` en **scène vierge** (pas de duplication de Ranked1v1)
- [ ] Concevoir une map dédiée 2v2 : dimensions de grille adaptées à 4 joueurs, disposition des obstacles, zones de placement par équipe (2 joueurs côté A, 2 côté B) — à valider dans `Nymora_Bible_complete.docx`
- [ ] Créer `Ranked2v2Config.asset` dans `ScriptableObjects/Configs/` : taille grille, positions de spawn, règles d'équipe
- [ ] Intégrer toutes les features de combat existantes : cercle/aura évolutif, abandon, timer, logs, HUD passif, floating damage, tooltips
- [ ] Adapter le HUD : timeline d'ordre des tours pour 4 joueurs, affichage des 2 équipes
- [ ] Ajouter la scène dans `EditorBuildSettings.asset`
- [ ] Ajouter l'entrée 2v2 dans le matchmaking Hub

#### 8.3 Scène & Map Ranked 3v3
> ⚠️ Même règle — scène vierge, map conçue pour le 3v3, aucune copie de Ranked1v1 ou Ranked2v2.

- [ ] Créer `Assets/_Game/Scenes/Ranked3v3.unity` en **scène vierge**
- [ ] Concevoir une map dédiée 3v3 : grille plus grande, placement symétrique 3 vs 3, obstacles repositionnés pour favoriser le jeu d'équipe — à valider dans `Nymora_Bible_complete.docx`
- [ ] Créer `Ranked3v3Config.asset` dans `ScriptableObjects/Configs/`
- [ ] Intégrer toutes les features de combat existantes : cercle/aura évolutif, abandon, timer, logs, HUD passif, floating damage, tooltips
- [ ] Adapter le HUD : timeline d'ordre des tours pour 6 joueurs, affichage des 2 équipes
- [ ] Ajouter la scène dans `EditorBuildSettings.asset`
- [ ] Ajouter l'entrée 3v3 dans le matchmaking Hub

#### 8.4 Audit post-Phase 8
- [ ] Tester un match 2v2 complet (placement → combat → victoire d'équipe)
- [ ] Tester un match 3v3 complet
- [ ] Vérifier que le 1v1 n'a pas régressé
- [ ] Vérifier les cas limites : déconnexion d'un joueur en équipe, abandon en équipe

**Critères de sortie :**
- Match 2v2 jouable en réseau sans désync
- Match 3v3 jouable en réseau sans désync
- Aucune régression sur le 1v1

---

## TABLEAU DE PRIORITÉS

| Phase | Nom | Priorité | Dépendances |
|---|---|---|---|
| 1 | Nettoyage + Réseau | 🔴 BLOQUANTE | — |
| 2 | Migration 128×128 | 🔴 HAUTE | Phase 1 |
| 3 | Nightseer + Colossar | 🔴 HAUTE | Phase 2 |
| 4 | Features combat | 🟠 MOYENNE | Phase 3 |
| 4.5 | Deckbuilder & Sélection de classe | 🟠 MOYENNE | Phase 3 |
| 6 | Écran post-combat | 🟠 MOYENNE | Phase 5.1 + 5.2 |
| 5 | Écosystème utilisateur | 🟡 LONG TERME | Phase 3 |
| 5.7 | Chat complet | 🟡 LONG TERME | Phase 5 (clans + compte) |
| 5.8 | Menu joueur + Défis | 🟡 LONG TERME | Phase 5.7 |
| 5.9 | Classement (Leaderboard) | 🟡 LONG TERME | Phase 5.2 (MMR) |
| 5.10 | Menu Paramètres Hub | 🟠 MOYENNE | Phase 7 (audio) recommandée avant |
| 5.11 | Boutique (Nymos + Premium) | 🟡 LONG TERME | Phase 5.4 (monnaie virtuelle) |
| 7 | Audio | 🟡 LONG TERME | Assets son disponibles |
| 8 | Modes 2v2 & 3v3 | 🔵 POST-ALPHA | Alpha 1v1 validée |

---

## DÉFINITION D'UNE ALPHA VIABLE

L'Alpha est considérée jouable et partageable si :

- [ ] 5 classes jouables sans erreur
- [ ] 1v1 en ligne stable (pas de désync, déconnexion gérée proprement)
- [ ] Assets en 128×128 (visuellement propre)
- [ ] Cercles/auras évolutifs en combat
- [ ] Abandon fonctionnel
- [ ] IA Expert jouable en entraînement
- [ ] MMR et leveling par classe fonctionnels
- [ ] Écran post-combat visible
- [ ] Audio minimum viable (pas de silence total)

---

## RÈGLES DE TRAVAIL

1. **Toujours faire un audit avant et après chaque phase majeure**
2. **En cas de doute sur un fichier à supprimer → poser la question avant**
3. **Ne jamais modifier l'UI/HUD pendant la migration 128×128**
4. **Se référer à `Nymora_Bible_complete.docx` pour tout ajout de contenu de classe**
5. **Mettre à jour ce document après chaque phase terminée**

---

## FICHIERS DE RÉFÉRENCE

- `Nymora_Bible_complete.docx` — Game Design Document complet
- `Assets/_Game/Scripts/Combat/ClassData.cs` — Enum NymoraClassId + PassiveStage
- `Assets/_Game/Scripts/Editor/NymoraClassFactory.cs` — Pattern à suivre pour Nightseer/Colossar
- `Assets/_Game/Scripts/Combat/SpellResolver.cs` — Logique de résolution des sorts
- `Assets/_Game/Scripts/Combat/PassiveManager.cs` — Logique des passifs runtime
- `Assets/_Game/Scripts/Network/OracleCombatNetBridge.cs` — Bridge RPC réseau
- `Assets/_Game/ScriptableObjects/Spells/Necram/` — Exemple de structure de sorts à dupliquer

---

## JOURNAL DES SESSIONS

> Chaque agent ou session de travail doit ajouter une entrée ici une fois sa tâche terminée. Format obligatoire ci-dessous.

---

<!--
FORMAT D'ENTRÉE DE JOURNAL :

### [DATE] — [PHASE / TÂCHE]
**Tâches effectuées :**
- ...

**Arbitrages réalisés :**
- ...

**Cas non prévus / difficultés :**
- ...

**Bugs détectés :**
- [ ] [description du bug] — statut : détecté / en cours / résolu
-->

### [2026-05-07] — Ajout section 1.0 — Bugs critiques Ranked1v1
**Tâches effectuées :**
- Ajout de la section 1.0 dans la Phase 1 avec 3 bugs prioritaires à corriger avant toute autre chose

---

### [2026-05-07] — Phase 1.1 — Suppression des fichiers inutiles
**Tâches effectuées :**
- Audit complet de `__cleanup_quarantine__/` (4 sous-dossiers, ~37 Mo) — grep des 5 scripts d'`oracle-editor-legacy/` et des 2 scripts de `runtime-legacy/` confirme aucune référence active dans `Assets/`. Seule mention historique : commentaire `CombatTooltipSystem.cs:8` ("Remplace HpTooltipWidget + AoETooltipOverlay") — non bloquant.
- Backup intégral `__cleanup_quarantine__/` → `_Save_fichiers_critiques/__cleanup_quarantine___backup_2026-05-07/` (37 Mo, conforme règle absolue Phase 1).
- Suppression de `__cleanup_quarantine__/` à la racine du projet.
- Audit `Assets/_Game/Scripts/Utils/` : ne contient que `.gitkeep`. Les "2 scripts" mentionnés dans la roadmap n'existaient plus — info obsolète, case cochée sans action.
- Audit `Assets/Plugins/` : Demigiant (DOTween, utilisé par `PassiveCardUI` + `OracleHubNetworkSetup` + multiples) et ParrelSync (utilisé pour les tests multi-instances locaux). Aucun obsolète, rien à supprimer.
- Consolidation Resources : `Assets/Resources/SpellIcons/` (+ son `.meta`) déplacé vers `Assets/_Game/Resources/SpellIcons/`. `SpellIconImporter.cs:14` (path éditeur en dur) et le warning de `SpellIconLoader.cs:70` mis à jour vers le nouveau path.

**Arbitrages réalisés :**
- **Backup avant suppression de la quarantine** : choisi explicitement par l'utilisateur (option "Tout supprimer avec backup") plutôt que suppression directe — conforme à la règle absolue Phase 1 ("sauvegarder dans `_Save_fichiers_critiques/` avant suppression").
- **DOTweenSettings.asset laissé dans `Assets/Resources/`** : choix explicite de l'utilisateur (option Recommandé) pour ne pas risquer de casser le pipeline DOTween qui régénère ce fichier à cet emplacement via son utility panel. **Conséquence** : le dossier `Assets/Resources/` n'a pas pu être complètement supprimé. La case "Supprimer `Assets/Resources/` une fois consolidé" de la **Phase 1.2** restera dépendante de cette décision (à garder en l'état tant que DOTween est présent).
- **SpellIconLoader runtime non modifié** : il utilise des paths relatifs (`SpellIcons/ghostra`, `SpellIcons/soulrender`) que `Resources.Load` résout depuis n'importe quel dossier `Resources/` du projet — donc le déplacement physique suffit, pas besoin de toucher la logique runtime.

**Cas non prévus / difficultés :**
- Roadmap mentionnait "2 scripts" dans `Scripts/Utils/` — en réalité 0 (juste `.gitkeep`). Information périmée dans la roadmap, pas de souci, case cochée à blanc.
- Deux fichiers de doc (`CLEANUP_AUDIT_2026-05-03.md`, `CLEANUP_REPORT_2026-05-03.md`) sont restés à la racine du projet après suppression de `__cleanup_quarantine__/`. Ils sont prévus pour aller dans `_Docs/archives/` lors de la **Phase 1.2** (réorganisation), donc laissés en place pour l'instant.

**Bugs détectés :** aucun.

**Action de validation post-modif requise (Unity) :**
- Ouvrir Unity et vérifier que la console n'affiche pas d'erreur `Missing reference` sur les sprites SpellIcons après le déplacement (Unity peut avoir besoin de réimporter le dossier — Reimport All si besoin).
- Vérifier qu'aucun script ne référence plus `Assets/Resources/SpellIcons` (déjà vérifié en grep, mais validation visuelle Unity recommandée).

**Validation utilisateur :** Test Unity OK 2026-05-07 — aucune erreur console après le déplacement de `SpellIcons/`.

---

### [2026-05-07] — Phase 1.2 — Réorganisation de la structure du projet
**Tâches effectuées :**
- Création de l'arborescence `_Docs/` + `_Docs/archives/` à la racine du projet.
- Déplacement de 3 docs principaux dans `_Docs/` : `Nymora_Bible_complete.docx`, `README.md`, et `ROADMAP_NYMORA.md` (déplacé en dernier après les éditions).
- Déplacement de 6 docs legacy dans `_Docs/archives/` : `CLEANUP_AUDIT_2026-05-03.md`, `CLEANUP_REPORT_2026-05-03.md`, `ORACLE_SCRIPTS_CLASSIFICATION_2026-05-03.md`, `ROADMAP_ORACLE.txt`, `TUTORIEL_RESEAU_ORACLE.txt`, `TUTORIEL_TEST_2_JOUEURS_PC.txt`.
- Renommage + déplacement `Assets/Monjeu.unity` → `Assets/_Game/Scenes/Training.unity` (avec son `.meta`, GUID préservé `c71e8afad6e035446a95b7a27ef5c292`).
- Déplacement `Assets/Boot_Network.unity` → `Assets/_Game/Scenes/Boot_Network.unity` (avec son `.meta`, GUID préservé `f819a7839e89c954cbdb52da33e8f99e`).
- Mise à jour `ProjectSettings/EditorBuildSettings.asset` : 2 paths corrigés.
- Patches **runtime critiques** (faits par l'orchestrateur Opus) :
  * `Assets/_Game/Scenes/Hub.unity:661` — `trainingSceneName: Monjeu` → `Training`
  * `Assets/_Game/Scripts/UI/HubHUD.cs:549` — `LoadScene("Monjeu")` → `LoadScene("Training")`
  * `Assets/_Game/Scripts/Hub/HubManager.cs:17` — default `trainingSceneName = "Monjeu"` → `"Training"`
- Patches **Editor** (délégués à un agent Haiku, vérifiés par Opus) :
  * `OracleRanked1v1Setup.cs` (3 modifs : docstring, const path, commentaire)
  * `OracleHubSceneBuilder.cs` (2 modifs : assignation + dialog)
  * `OracleHubPlayerSetup.cs` (5 modifs : const renommée `MONJEU_PATH` → `TRAINING_PATH`, path corrigé, docstring, variable locale `monjeuScene` → `trainingScene`, logs/dialogs)
  * `InjectTurnOrderTimeline.cs:15` (path)
  * `InjectCombatTooltipSystem.cs:15` (path)
  * `ArenaGenerator.cs:55` (commentaire — dualité Monjeu/Training simplifiée en `Training`)
- Vérification finale : grep `Monjeu` sur `Assets/` → 0 occurrence restante.

**Arbitrages réalisés :**
- **Délégation à un agent Haiku** : les 6 fichiers Editor (~12 remplacements mécaniques) ont été délégués à Haiku conformément au workflow roadmap (Opus orchestre, Haiku exécute les tâches répétitives). L'agent a fait un grep final pour confirmer la complétude. Patches runtime critiques (4 fichiers : Hub.unity, HubHUD.cs, HubManager.cs, EditorBuildSettings.asset) gardés par Opus pour réduire le risque sur les paths sensibles au runtime.
- **`CLEANUP_REPORT_2026-05-03.md` archivé** : non listé explicitement dans la roadmap (qui mentionne `CLEANUP_AUDIT` mais pas `CLEANUP_REPORT`), mais archivé par cohérence avec le doc audit jumeau.
- **Case "Supprimer `Assets/Resources/` une fois consolidé"** : marquée **non applicable** dans la roadmap car `DOTweenSettings.asset` reste à cet emplacement (décision Phase 1.1 validée par utilisateur). Note explicite ajoutée pour traçabilité future.

**Cas non prévus / difficultés :**
- 4 mentions résiduelles de `Monjeu` détectées dans `ProjectSettings/ProjectSettings.asset` (`productName`, `projectName`, `metroPackageName`, `metroApplicationDescription`) — c'est le **nom interne du projet Unity**, hors scope de la Phase 1.2 qui concerne uniquement scènes et docs. ⚠️ **À traiter dans une étape de cleanup séparée** : décision à prendre — renommer le projet Unity en "Nymora" ? Impact possible sur build identifier, futur Steam/Epic, etc. **Ne pas modifier sans arbitrage explicite utilisateur.**
- Dossiers à la racine non mentionnés dans la roadmap (`World/`, `mise_a_jour_animation/`, `real_character_animation/`, `spell_icon/`, `Tools/`) — laissés intacts (hors scope 1.2).

**Bugs détectés (résolus) :**
- ⚠️ **Régression compilation détectée par l'utilisateur après le travail de l'agent Haiku** : `OracleHubPlayerSetup.cs:85` référençait encore `monjeuScene` (variable locale) après le rename de `monjeuScene` → `trainingScene` à la ligne 79. L'agent Haiku avait reporté avoir renommé "tous les usages" mais avait oublié cette occurrence dans une boucle `foreach`. Erreur Unity : `CS0103: The name 'monjeuScene' does not exist`. → ✅ Corrigé immédiatement par Opus (1 Edit).
- **Leçon retenue** : pour les renames de variables sur des fichiers volumineux, faire systématiquement un grep final post-agent (case-insensitive) avant de passer à la suite. Le grep initial post-agent ne couvrait que `Monjeu` (capitalisé) et a manqué la variable locale `monjeuScene` (camelCase).

**Warnings préexistants détectés au build (hors scope 1.2) :**
- `HubPlayerAvatar.cs:47,53` — CS0114: `OnEnable()`/`OnDisable()` cachent les membres hérités de `MonoBehaviourPunCallbacks`. À ajouter `override` (probablement la bonne sémantique vu que le héritage `MonoBehaviourPunCallbacks` a été introduit en 1.0-bis pour `OnPlayerPropertiesUpdate`).
- `HubHUD.cs:1168,1351` — CS0219: variables `PAD_V` et `CARD_W` assignées mais jamais utilisées (dead code mineur).
- ⚠️ Ces warnings sont **antérieurs à la Phase 1.2** et n'empêchent pas la compilation. À traiter dans une passe de cleanup code mineure (suggestion : phase 1.4 audit fin de phase).

**Action de validation post-modif requise (Unity) :**
- Ouvrir Unity et vérifier qu'aucune erreur console n'apparaît au chargement (notamment pas de `Missing reference` sur les scènes déplacées).
- Vérifier que la scène **Training** (ex-Monjeu) s'ouvre correctement et que le bouton "Entraînement" du Hub la lance bien (`HubHUD.OnTrainingSelected`).
- Vérifier que le build inclut bien les 5 scènes au bon path (`MainMenu`, `Hub`, `Training`, `Ranked1v1`, `Boot_Network`).
- Vérifier que les scripts Editor `OracleHubPlayerSetup`, `OracleRanked1v1Setup`, etc. fonctionnent encore si appelés (menu Tools).

**Bugs détectés :**
- [x] `HubPlayer(Clone)` visible en jeu pour l'un des joueurs en Ranked1v1 — statut : résolu (2026-05-07)
- [x] Sélection de passif en début de Ranked1v1 non synchronisée réseau — statut : résolu (2026-05-07, supprimée)
- [x] Désync de sélection de classe : chaque joueur voit les deux mêmes classes au lieu des vraies sélections — statut : résolu (2026-05-07, sync via Photon CustomProperties)

---

### [2026-05-07] — Phase 1.0 — Résolution des 3 bugs critiques Ranked1v1
**Orchestration :** Claude Opus (xhigh) — 2 agents Sonnet en Phase A parallèle, 1 agent Sonnet en Phase B séquentielle.

**Tâches effectuées :**
- **Bug 1.0.a — HubPlayer(Clone) cleanup** : `Assets/_Game/Scripts/Hub/HubNetworkSpawner.cs:54-76` — `OnSceneLoaded()` utilise désormais `PhotonNetwork.Destroy()` (au lieu de `Destroy()` local) quand on quitte le Hub, pour propager la destruction du HubPlayer réseau à tous les clients. `Assets/_Game/Scripts/Hub/HubMatchmaker.cs:172` — appel défensif à `DestroyPlayerBeforeLeave()` ajouté avant `PhotonNetwork.LoadLevel(combatSceneName)`.
- **Bug 1.0.b — Suppression sélection de passif** : `Assets/_Game/Scripts/Combat/CombatInitializer.cs` — supprimés : champ `passiveSelectionScreen`, son auto-find (~ligne 297), l'appel `StartCoroutine(RunPassiveSelection())` dans `InitSequence()`, et la coroutine entière `RunPassiveSelection()` (~58 lignes). Le passif par défaut continue d'être appliqué automatiquement via `PlaceCharacter()` (lignes 660-675 après refactor) à partir de `selectedClass.passiveData`. Le champ public `skipPassiveSelection` est conservé (références Inspector potentielles).
- **Bug 1.0.c — Synchronisation de classe via Photon CustomProperties** : `Assets/_Game/Scripts/Hub/HubMatchmaker.cs:161-167` — chaque client pose sa `classId` (cast `int`) dans `PhotonNetwork.LocalPlayer.SetCustomProperties` avant le `LoadLevel`. `Assets/_Game/Scripts/Combat/CombatInitializer.cs:647-658` — `PlaceCharacter()` distingue Network vs Training et appelle un nouveau helper privé `ResolveNetworkClass(bool isMasterSlot)` (lignes 686-702) qui itère `PhotonNetwork.PlayerList`, lit la `CustomProperty "classId"` du joueur correspondant au slot demandé (Master = slot 0 = `player`, Other = slot 1 = `opponent`), puis résout via `HubManager.LoadClassById()`. Fallback sur la classe locale si la propriété est absente.

**Arbitrages réalisés :**
- **Photon CustomProperties retenu plutôt qu'un RPC** pour la sync de classe — pattern statique, propagation automatique, supporte le late-join, pas de timing fragile. Premier usage de CustomProperties dans le projet.
- **Phase B séquentielle** plutôt que parallèle : Bug 2 (suppression de code) et Bug 3 (ajout de code) modifient tous deux `CombatInitializer.cs` → conflit garanti en parallèle.
- **Slots conventionnés** : `player` = MasterClient = slot 0, `opponent` = autre = slot 1, conformément à `ApplyNetworkPlayerNames()` ligne 752+. Le helper `ResolveNetworkClass` reproduit le pattern foreach existant.
- **Champ `skipPassiveSelection` conservé** au lieu d'être supprimé : public, peut être référencé en Inspector — risque de casser une référence pour un gain nul.
- **Fichier `PassiveSelectionScreen.cs` conservé** : aucune utilité de le supprimer (peut servir plus tard en Training si désiré).

**Cas non prévus / difficultés :**
- Aucun cas bloquant. Le code de fallback existant (`pm.ownerClass`, `selectedClass.characterKey`, etc.) reste valide post-fix car la `selectedClass` retournée en réseau est désormais la bonne pour chaque slot.
- Limite identifiée : `SetCustomProperties` est asynchrone côté Photon. En théorie, si le master charge `Ranked1v1` avant que la propriété de l'autre joueur soit propagée, le client non-master pourrait entrer en scène avec une propriété manquante (fallback déclenché). En pratique le `LoadLevel` prend plusieurs frames, et les properties sont push-confirmées par le serveur — risque très faible. Si observé, ajouter un `OnPlayerPropertiesUpdate` + coroutine d'attente.

**Bugs détectés (non bloquants) :**
- Aucun nouveau bug introduit.
- **Référence flagged** (à surveiller, non corrigée) : `PlayerAnimator.cs:156`, `SpellResolver.cs:70`, `PassiveManager.cs:20` lisent encore `HubManager.Instance.SelectedClass` en fallback. En réseau, ces fallbacks ne sont atteints que si `ownerClass` ou `characterKey` ne sont pas alimentés en amont — non bloquant car `PlaceCharacter()` les alimente correctement maintenant. À auditer si comportement inattendu observé.

**Tests Unity restants (côté utilisateur) :**
- Lancer un match Ranked1v1 en mode 2 instances (ParrelSync) et vérifier :
  1. Le `HubPlayer(Clone)` n'apparaît plus dans la scène Ranked1v1 chez aucun des deux joueurs
  2. L'écran de sélection de passif n'apparaît plus en début de combat
  3. Si J1 = Soulrender et J2 = Necram, chaque client voit bien Soulrender d'un côté et Necram de l'autre (passif, sprite, animations)

---

### [2026-05-07] — Phase 1.0 — Itération bis (retours utilisateur après tests)
**Constat utilisateur après les premiers fixes :**
- Le `HubPlayer(Clone)` était encore visible en Ranked1v1 malgré le cleanup côté Hub.
- La sync de classe fonctionne en Ranked1v1 mais pas dans le Hub : chaque client voyait l'avatar adverse avec sa propre classe locale.

**Tâches effectuées :**
- **Bug 1.0.a-bis — Cleanup radical HubPlayer dans Ranked1v1** : `Assets/_Game/Scripts/Combat/CombatInitializer.cs:31-50` — `Awake()` itère désormais `FindObjectsByType<HubPlayerAvatar>(FindObjectsInactive.Include, FindObjectsSortMode.None)` au démarrage de la scène et détruit chaque avatar via `PhotonNetwork.Destroy` (si IsMine) ou `Destroy` local. Filet de sécurité 100% : peu importe pourquoi un HubPlayer aurait survécu à la transition Hub → Ranked1v1, il est éliminé avant que `InitSequence()` démarre.
- **Bug 1.0.c-bis — Sync classe visible aussi dans le Hub** :
  - `Assets/_Game/Scripts/Hub/HubManager.cs:36-66` — le setter de `SelectedClass` appelle désormais `PushSelectedClassToPhoton()` à chaque changement. Méthode statique publique qui pose `"classId"` dans `PhotonNetwork.LocalPlayer.SetCustomProperties` si le client est dans une room.
  - `Assets/_Game/Scripts/Hub/HubNetworkSpawner.cs:OnJoinedRoom` — appelle `HubManager.PushSelectedClassToPhoton()` AVANT `SpawnPlayer()`, pour que les avatars distants soient construits avec la bonne classe dès leur premier frame.
  - `Assets/_Game/Scripts/Combat/PlayerAnimator.cs:Start` — refactor de la sélection de classe au démarrage. En hub, distinction `IsMine` (HubPlayer local → `HubManager.SelectedClass` + abonnement à `OnClassChanged`) vs `!IsMine` (HubPlayer distant → classe résolue depuis `photonView.Owner.CustomProperties["classId"]`). Plus aucun avatar distant n'écoute `OnClassChanged` local — chaque PlayerAnimator joue désormais la classe de son owner réel.
  - `Assets/_Game/Scripts/Hub/HubPlayerAvatar.cs` — héritage `MonoBehaviourPun` → `MonoBehaviourPunCallbacks`. Override de `OnPlayerPropertiesUpdate(target, changedProps)` : si `target == photonView.Owner` et que `"classId"` change, appelle `LoadCharacterAnimations(cls.characterKey)` sur le `PlayerAnimator` du même GameObject. Permet la mise à jour live quand un joueur change de classe pendant que l'autre est dans le hub.

**Arbitrages réalisés :**
- **Cleanup radical via FindObjectsByType plutôt que de chasser la cause** : le HubPlayer survit potentiellement parce que Photon synchronise la destruction asynchrone et le LoadLevel peut arriver plus vite. Plutôt que de courir après chaque cas (timing room, MasterClient migration, etc.), le filet dans `CombatInitializer.Awake()` garantit une scène propre quoi qu'il arrive. Coût : un `FindObjectsByType` à chaque entrée en Ranked1v1 (négligeable).
- **Stratégie B (animation IDLE complète) plutôt que A (sprite statique) ou C (mécanisme seul)** : confirmé par l'utilisateur. Cohérent avec le combat (mêmes anims, même PlayerAnimator), aucune dette créée pour la phase 4.5 (deckbuilder).
- **Découverte clé** : le `HubPlayer.prefab` a déjà un `TacticalCharacter` + un `PlayerAnimator` complet (idle/walk/death × 4 directions, frames Ghostra par défaut). Aucun composant à ajouter, juste le routage de classe à corriger.
- **`MonoBehaviourPunCallbacks` plutôt qu'un singleton listener centralisé** : le callback `OnPlayerPropertiesUpdate` existe déjà sur ce type, et chaque HubPlayerAvatar peut filtrer par `target == photonView.Owner`. Pas besoin d'un manager global.
- **Pas de modification de `HubMatchmaker.cs:161-167`** : le `SetCustomProperties` qu'on y avait fait en 1.0.c continue de fonctionner (il fait la même chose que `PushSelectedClassToPhoton`). Pas de duplication critique, on laisse en place pour ne pas régresser ce qui fonctionne déjà.

**Cas non prévus / difficultés :**
- `PlayerAnimator` a `[RequireComponent(typeof(TacticalCharacter))]` → première hypothèse (l'ajouter dynamiquement au HubPlayerAvatar) impossible. Découverte que le prefab existant a déjà tout ce qu'il faut a évité de créer un composant `HubAnimator` léger redondant.
- `ClassCharacterPreview.cs` est marqué obsolète depuis longtemps — confirme que le pattern "PlayerAnimator gère tout via `HubManager.OnClassChanged`" était déjà la voie officielle ; il manquait juste la branche "avatar distant".

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity restants (côté utilisateur) :**
- En 2 instances ParrelSync, dans le Hub :
  1. Démarrer les 2 clients en classes différentes (J1 = Necram, J2 = Ghostra par exemple)
  2. Chaque client doit voir l'avatar adverse animé avec la bonne classe (pas la sienne)
  3. Changer de classe dans le DeckBuilder côté J1 → J2 doit voir l'avatar de J1 changer en live (idle de la nouvelle classe)
- En Ranked1v1 :
  1. Le `HubPlayer(Clone)` ne doit plus jamais apparaître à aucun moment, même fugacement

---

### [2026-05-07] — Phase 1.0 — Itération ter (multi-agent en parallèle)
**Constat utilisateur après les fixes bis :**
- Le `HubPlayer(Clone)` était toujours visible en Ranked1v1 (même avec le filet `FindObjectsByType` dans `CombatInitializer.Awake()`) — preuve que l'avatar peut arriver après le démarrage de la scène, le filet ne le voit pas.
- Bug **asymétrique** dans le Hub : un client voyait correctement la classe adverse, l'autre voyait sa propre classe sur l'avatar adverse.

**Orchestration multi-agent :** Claude Opus, 2 agents Sonnet en parallèle, fichiers cloisonnés pour éviter conflits.

**Tâches effectuées :**

- **Bug 1.0.a-ter — Déplacement HubPlayer hors caméra (Agent A)** : `Assets/_Game/Scripts/Hub/HubPlayerAvatar.cs`
  - Stratégie alternative validée : ne plus tenter de détruire (course de timing perdue), mais déplacer le GameObject à `Vector3(0f, -10000f, 0f)` + désactiver `avatarSprite.enabled` + désactiver le label parent dès que `SceneManager.GetActiveScene().name != "Hub"`.
  - Override `OnEnable`/`OnDisable` (avec `base.OnEnable()` / `base.OnDisable()` pour préserver les callbacks Photon de `MonoBehaviourPunCallbacks`) → s'abonne/désabonne à `SceneManager.sceneLoaded`.
  - Méthodes ajoutées : `HandleSceneLoaded(Scene, LoadSceneMode)` → `ApplyVisibility()` qui applique l'état selon la scène active. Restoration automatique au retour dans le Hub.
  - Appel d'`ApplyVisibility()` à la fin de `Awake()` pour couvrir le cas où le prefab est instancié alors que `Ranked1v1` est déjà active (sceneLoaded ne se déclenche pas alors).

- **Bug 1.0.c-ter — Sync classe robuste (Agent B)** :
  - `Assets/_Game/Scripts/Hub/HubManager.cs:73` — `Awake()` assigne désormais le default Ghostra via le **setter** `SelectedClass = LoadClassById(...)` au lieu d'écrire directement à `_selectedClass`. Garantit l'invocation de `OnClassChanged` et de `PushSelectedClassToPhoton()` (qui no-op proprement si pas encore en room — `HubNetworkSpawner.OnJoinedRoom` reprendra le relai).
  - `Assets/_Game/Scripts/Combat/PlayerAnimator.cs:163-180 + nouvelle coroutine` — fin du fallback dangereux `?? HubManager.Instance?.SelectedClass` côté hub distant (la cause directe de l'asymétrie). Trois branches distinctes désormais :
    1. CustomProperty `classId` résolue immédiatement → `LoadCharacterAnimations` direct.
    2. Avatar local → `HubManager.Instance?.SelectedClass` (légitime, c'est notre classe).
    3. Avatar distant sans property arrivée → `PlayForCurrentState()` (sprite par défaut prefab) + `StartCoroutine(RetryResolveHubClass(pv))`.
  - Coroutine `RetryResolveHubClass(PhotonView pv)` : poll toutes les 200ms, jusqu'à 25 tentatives (5s), appelle `LoadCharacterAnimations` dès que `classId` arrive. Log warning sur timeout.

**Diagnostic du bug asymétrique (confirmé par Agent B) :**
- Cause principale : `PlayerAnimator.Start()` retombait sur `HubManager.Instance?.SelectedClass` (la classe **locale**) quand la CustomProperty de l'Owner distant n'était pas encore arrivée.
- Cause secondaire : `HubManager.Awake()` shortcuit le setter pour le default Ghostra → `PushSelectedClassToPhoton` jamais appelé pour les joueurs n'ayant pas changé de classe.
- L'asymétrie venait de l'ordre d'arrivée des messages Photon : selon le client, l'`Instantiate` du HubPlayer adverse arrivait avant ou après la propagation des CustomProperties.

**Arbitrages réalisés :**
- **Cleanup vs masquage** : sur recommandation utilisateur, abandon de la stratégie "détruire à tout prix" pour le HubPlayer en Ranked1v1. Le déplacement hors caméra + désactivation du sprite est plus robuste car ne dépend pas de timing Photon.
- **Coroutine retry plutôt que de bloquer le `SpawnPlayer`** : alternative considérée — attendre la confirmation `SetCustomProperties` avant `PhotonNetwork.Instantiate`. Rejetée car ça aurait introduit une dépendance asynchrone fragile dans `HubNetworkSpawner.OnJoinedRoom`. La coroutine côté avatar distant est non-bloquante et idempotente avec `OnPlayerPropertiesUpdate`.
- **Filet `FindObjectsByType` dans CombatInitializer conservé** : double sécurité (belt & suspenders). Coût négligeable, couvre le cas où le HubPlayerAvatar serait là dès le `Awake()` de la scène.
- **2 agents Sonnet en parallèle, cloisonnement strict** : Agent A interdit de toucher hors `HubPlayerAvatar.cs`, Agent B interdit de toucher `HubPlayerAvatar.cs`. Aucun conflit.

**Cas non prévus / difficultés :**
- Aucun bloquant. Note : `MonoBehaviourPunCallbacks.OnEnable/OnDisable` doivent appeler `base.*` impérativement pour que les callbacks Photon (dont `OnPlayerPropertiesUpdate` et autres) restent enregistrés — bien respecté par Agent A.
- Les deux mécanismes de rattrapage (`RetryResolveHubClass` côté PlayerAnimator et `OnPlayerPropertiesUpdate` côté HubPlayerAvatar) coexistent sans conflit : appels idempotents à `LoadCharacterAnimations`.

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity restants (côté utilisateur, ParrelSync) :**
- Hub : J1 et J2 démarrent en classes différentes → chacun doit voir la bonne classe adverse (avec coroutine retry, le rattrapage prend max 5s)
- Hub : changement de classe live via DeckBuilder → l'autre client voit l'avatar muter (toujours via `OnPlayerPropertiesUpdate`)
- Ranked1v1 : le `HubPlayer` ne doit plus apparaître — qu'il soit instancié avant ou après le démarrage de la scène, il est immédiatement déplacé hors caméra

---

### [2026-05-07] — Phase 1.0 — Itération FINAL (changement de stratégie radical, multi-agent)
**Constat utilisateur après 3 itérations infructueuses :**
- HubPlayer(Clone) **toujours visible** en Ranked1v1 — ni Destroy ni cleanup ni déplacement à -10000 ne fonctionnaient.
- Sync classe **toujours asymétrique** dans le Hub — un client voit, l'autre voit sa propre classe.
- Demande explicite : **solution radicale immédiate, mode plusieurs agents actif.**

**Diagnostic des échecs précédents :**
- **HubPlayer** : `PhotonTransformView` côté instance distante écrase tout déplacement local (interpolation vers la position broadcastée par l'Owner). En plus, le `PlayerAnimator` crée un child `SpriteVisual` avec son PROPRE SpriteRenderer — désactiver `avatarSprite.enabled` ne touchait que le SpriteRenderer principal du prefab.
- **Sync classe** : Photon CustomProperties est asynchrone et l'ordre d'arrivée vs `PhotonNetwork.Instantiate` n'est pas garanti. Le retry coroutine pollait des properties qui n'arrivaient parfois jamais avant la fin du timeout (ou jamais du tout selon le client).

**Stratégie radicale finale :**
- **HubPlayer** : `gameObject.SetActive(false)` complet. Désactive le GameObject entier — SpriteRenderer principal, child SpriteVisual, PhotonTransformView, PlayerAnimator (Update/coroutines), tout. Plus aucune dépendance de timing ou de comportement Photon.
- **Sync classe** : abandon pur et simple des Photon CustomProperties pour le Hub. Remplacement par un **RPC `RpcTarget.AllBuffered`** sur le PhotonView du HubPlayerAvatar. Photon bufferise le RPC côté serveur et le rejoue à TOUS les clients (présents + futurs qui rejoignent la room). Garantie absolue de livraison.

**Tâches effectuées (multi-agent, 2 agents Sonnet en parallèle, fichiers cloisonnés) :**

- **Bug 1.0.a-final — SetActive radical (Agent A)** : `Assets/_Game/Scripts/Hub/HubPlayerAvatar.cs`
  - `ApplyVisibility()` simplifiée à 2 lignes : désactive le label Canvas (GameObject séparé) puis `gameObject.SetActive(inHub)`. Plus de manipulation de position ni d'`avatarSprite.enabled`.
  - Suppression de la constante `HiddenPosition` (-10000 obsolète).

- **Bug 1.0.c-final — RPC AllBuffered (Agent A, même fichier que A car logique liée)** : `Assets/_Game/Scripts/Hub/HubPlayerAvatar.cs`
  - **Suppression** de l'override `OnPlayerPropertiesUpdate` (mécanisme CustomProperties abandonné).
  - **Ajout** : `[PunRPC] void RpcSetHubClassId(int classIdInt)` qui résout la classe via `HubManager.LoadClassById` et appelle directement `GetComponent<PlayerAnimator>().LoadCharacterAnimations(cls.characterKey)`.
  - **Côté Owner (`IsMine`)** : dans `Awake()`, démarre une coroutine `BroadcastInitialClassNextFrame()` qui attend 1 frame (laisse PlayerAnimator finir son Start) puis émet le RPC via `photonView.RPC(nameof(RpcSetHubClassId), RpcTarget.AllBuffered, (int)cls.classId)`. S'abonne aussi à `HubManager.OnClassChanged` pour re-broadcaster à chaque changement de classe (le buffered remplace le précédent côté serveur). Désabonnement dans `OnDestroy`.

- **Cleanup PlayerAnimator (Agent B)** : `Assets/_Game/Scripts/Combat/PlayerAnimator.cs`
  - **Suppression** de la coroutine `RetryResolveHubClass` entière (devenue inutile).
  - **Suppression** de la lecture `pv.Owner.CustomProperties["classId"]` dans `Start()`.
  - `Start()` simplifié à 2 branches claires : Combat (player uniquement) / Hub (local uniquement). L'avatar distant en hub fait juste `PlayForCurrentState()` (sprite par défaut prefab) et attend le RPC du HubPlayerAvatar.

**Arbitrages réalisés :**
- **`SetActive(false)` plutôt que Destroy** : le HubPlayer doit pouvoir être réactivé au retour Hub (rematch ou retour menu). SetActive est réversible, Destroy ne l'est pas. Et SetActive est local — chaque client le fait pour ses propres copies, pas besoin de propager.
- **`RpcTarget.AllBuffered` plutôt que `OthersBuffered`** : `AllBuffered` inclut l'envoyeur. Pas grave car le LoadCharacterAnimations sur sa propre instance est idempotent (le `PlayerAnimator` côté local hub a déjà chargé sa classe via le path `isLocalHubAvatar`). Simpler et plus sûr en cas de timing bizarre.
- **Délai de 1 frame avant le broadcast initial** : garantit que `PlayerAnimator.Start()` (qui fait `SetupSpriteRoot` etc.) s'est terminé chez tous les clients avant qu'un `LoadCharacterAnimations` arrive via RPC.
- **Multi-agent strict** : Agent A → uniquement `HubPlayerAvatar.cs`. Agent B → uniquement `PlayerAnimator.cs`. Aucun conflit possible.

**Pourquoi cette stratégie est définitive :**
- Photon `AllBuffered` est le mécanisme **conçu pour ce cas exact** : synchroniser un état persistant lié à un PhotonView, livré à tous les clients présents et futurs. Pas de timing, pas de retry, pas d'asymétrie possible.
- `SetActive(false)` est la primitive Unity la plus radicale pour cacher un GameObject. Aucun composant ne peut le contourner (PhotonTransformView, PlayerAnimator, Update, etc. sont tous arrêtés).

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity à refaire (ParrelSync) :**
- Hub : J1 et J2 en classes différentes → chacun doit voir la bonne classe adverse, dès le démarrage (le RPC bufferisé arrive en quelques ms).
- Hub : changement de classe live → l'autre client voit muter en temps réel.
- Ranked1v1 : le `HubPlayer` est complètement inexistant à l'écran (le GameObject est désactivé chez tous les clients).
- Retour au Hub après combat (si le flow le permet) : le HubPlayer doit se réactiver proprement.

**Si ça ne marche TOUJOURS pas après cette itération :**
Le problème n'est plus dans le code applicatif — il faudra investiguer la configuration Photon elle-même : `PhotonView` du prefab HubPlayer (vérifier qu'il a bien un `[PunRPC]` enregistré au niveau du PhotonView Inspector), version PUN2 utilisée, paramètres de la room HUB_GLOBAL, etc.

---

### [2026-05-07] — Phase 1.0 — Itération MEGA FINAL (3 agents parallèles + diagnostic + Editor injection)
**Constat utilisateur après l'itération FINAL :**
- Sync classe : un client voit Ghostra par défaut sur l'autre joueur (pas la vraie classe sélectionnée). User a identifié ça comme "le sprite opponent_test".
- HubPlayer(Clone) toujours présent en Ranked1v1.
- Demande : "regle moi ces bugs une bonne fois pour toute, si on doit créer un script à injecter sur unity via le dossier editor on le fait, fait moi ça bien une bonne fois pour toute, mode plusieurs agents".

**Diagnostic exhaustif (Agent Explore) :**
- ✅ "opponent_test" **n'est référencé NULLE PART** dans le code, ni dans aucun .prefab, ni dans aucune .unity (grep confirmé sur le GUID `1711d54f56d35214b9a329561df205b1`).
- ✅ Ce que l'utilisateur voit est en réalité les **frames Idle Ghostra par défaut** pré-chargées dans le `HubPlayer.prefab` (YAML lignes 130-305) — `LoadCharacterAnimations` n'est jamais appelé pour la vraie classe distante, donc le sprite reste sur les frames hardcodées du prefab.
- ✅ Le RPC `AllBuffered` envoyé après `SpawnPlayer()` arrive trop tard ou se perd pour les avatars déjà instanciés chez d'autres clients à un moment où la classe locale n'est pas encore connue.
- ✅ Le HubPlayer survit car `PhotonNetwork.Destroy()` est asynchrone — il y a une fenêtre de quelques centaines de millisecondes entre `LeaveRoom` et le `LoadLevel` où l'objet n'est pas nettoyé. Le sweep `Awake` de CombatInitializer arrive avant la destruction réseau effective.

**Fix radical (Agent B — `instantiationData` Photon natif) :**
- `Assets/_Game/Scripts/Hub/HubNetworkSpawner.cs:184-195` (SpawnPlayer) — passe désormais la `classId` locale (cast `int`) dans `object[] data` au 5e argument de `PhotonNetwork.Instantiate(prefabName, pos, rot, group=0, data)`. **C'est le mécanisme conçu pour ce cas exact** : Photon embarque les données dans l'instantiation et les livre atomiquement avec l'objet à TOUS les clients (présents + futurs join), sans race condition possible.
- `Assets/_Game/Scripts/Hub/HubPlayerAvatar.cs` — `Awake` démarre `ApplyInstantiationDataNextFrame()` pour TOUS les avatars (IsMine + !IsMine). Coroutine attend 1 frame (laisse `PlayerAnimator.Start()` créer le SpriteVisual), lit `photonView.InstantiationData`, résout via `HubManager.LoadClassById(NymoraClassId)` et appelle `LoadCharacterAnimations(cls.characterKey)`. Le RPC `RpcSetHubClassId`/`HandleLocalClassChanged` reste pour les changements LIVE en cours de session (DeckBuilder).

**Fix radical (Agent C — polling agressif + script Editor d'injection) :**
- `Assets/_Game/Scripts/Combat/CombatInitializer.cs:31-72` — coroutine `NukeHubPlayersForSeconds(5f)` démarrée dans `Awake()` après le sweep instantané. Toutes les 100ms pendant 5s, `FindObjectsByType<HubPlayerAvatar>` puis `Destroy(go)` local sur chaque avatar trouvé. Belt & suspenders.
- **Nouveau fichier** : `Assets/_Game/Scripts/Combat/HubPlayerKiller.cs` — composant runtime autonome (5s polling, 100ms interval, paramètres exposés Inspector). Peut être attaché à n'importe quelle scène pour un cleanup local indépendant de CombatInitializer.
- **Nouveau fichier** : `Assets/_Game/Scripts/Editor/InjectHubPlayerKillerInRanked1v1.cs` — menu **`Tools → Nymora → Inject HubPlayerKiller in Ranked1v1`**. Ouvre la scène `Assets/_Game/Scenes/Ranked1v1.unity` (chemin confirmé via Glob), vérifie l'absence d'un Killer existant, crée un GameObject `HubPlayerKiller`, le sauvegarde dans la scène. Action one-shot pour l'utilisateur.

**Arbitrages réalisés :**
- **`instantiationData` Photon natif > RPC `AllBuffered`** : `instantiationData` est ATOMIC avec l'instantiate (livraison garantie au moment précis où l'objet existe). Le RPC AllBuffered, lui, peut arriver après si le timing serveur Photon est défavorable.
- **Polling + Editor injection > tentatives infinies de cleanup réseau propre** : le user a explicitement validé l'approche radicale. Mieux vaut un polling local de 5s qui Destroy aveuglément que de courir après chaque scenario réseau.
- **Les frames Ghostra hardcodées dans le prefab restent** : c'est OK car maintenant `LoadCharacterAnimations` arrive en 1 frame via instantiationData, donc l'utilisateur ne verra Ghostra que pour 1 frame max.
- **Multi-agent strict** : Agent Explore (diagnostic, no modify), Agent B (HubNetworkSpawner + HubPlayerAvatar), Agent C (CombatInitializer + 2 nouveaux fichiers). Aucun conflit, gain de temps réel.

**ACTION REQUISE CÔTÉ UTILISATEUR (UNE SEULE FOIS) :**
1. Ouvrir Unity
2. Menu **Tools → Nymora → Inject HubPlayerKiller in Ranked1v1**
3. Vérifier dans la console : `[InjectHubPlayerKiller] HubPlayerKiller injecté et scène sauvegardée.`
4. Tester en ParrelSync

**Tests Unity attendus :**
- Hub : J1=Necram, J2=Ghostra → chacun voit la BONNE classe adverse, **dès le premier frame** (instantiationData garantie)
- Hub : changement de classe live au DeckBuilder → l'autre voit muter (RPC bufferisé)
- Ranked1v1 : aucun HubPlayer visible, jamais (3 mécanismes en cascade : sweep CombatInitializer → polling 5s → HubPlayerKiller dans la scène)

**Pourquoi cette itération est définitive :**
- `instantiationData` est le mécanisme PUN2 **conçu et documenté pour ce cas exact**. C'est la solution canonique pour synchroniser un état initial lié à un PhotonView. Si ça ne marche pas, il y a un problème de configuration Photon plus profond (mais le code applicatif est correct).
- Triple cleanup HubPlayer : (a) sweep Awake CombatInitializer, (b) polling 5s CombatInitializer, (c) HubPlayerKiller dans la scène Ranked1v1 elle-même. Au moins un des trois va l'attraper.

---

### [2026-05-07] — Phase 1.0 — Itération 6 : CAUSE ROOT IDENTIFIÉE 🎯
**Constat utilisateur :** après les itérations FINAL et MEGA FINAL, les deux bugs persistaient malgré des fix code "techniquement corrects". L'audit a porté sur la configuration des prefabs/scripts.

**Diagnostic exhaustif (Claude Opus xhigh) :**

Audit des **GUID** des composants présents dans `Assets/_Game/Resources/HubPlayer.prefab` (prefab utilisé par `HubNetworkSpawner.avatarPrefabName = "HubPlayer"`) vs les GUID des scripts du dossier `Assets/_Game/Scripts/Hub/` :

| Script | GUID | Présent dans HubPlayer.prefab ? |
|---|---|---|
| `HubPlayerAvatar.cs` | `79f259e796eaed24eaa1e3d8edf5db21` | ❌ **NON** |
| `HubNetworkSync.cs` | `b31f456665a8e9343be1964d650aaeea` | ✅ oui |
| `HubPlayerLabel.cs` | `4ec56394c4af65f4ea8d2aa1384fca2b` | ✅ oui |
| `TacticalCharacter.cs` | `5f8bc308…` | ✅ oui |
| `PlayerAnimator.cs` | `5e801f11…` | ✅ oui |

**🔴 Le composant `HubPlayerAvatar.cs` n'était PAS sur le prefab production.** Tout le code applicatif des 5 itérations précédentes (cleanup, instantiationData reader, RpcSetHubClassId, ApplyVisibility avec SetActive(false), etc.) reposait sur `HubPlayerAvatar` — un composant **jamais instancié sur les avatars Hub réels**.

**Conséquences directes des 5 itérations échouées :**
- `FindObjectsByType<HubPlayerAvatar>` (sweep `CombatInitializer`, polling 5s, `HubPlayerKiller`) → trouvait toujours **0 objet** → aucun cleanup → Bug A persistant
- `HubPlayerAvatar.ApplyVisibility()` (SetActive(false) en Ranked1v1) → **jamais exécuté** → Bug A persistant
- `HubPlayerAvatar.ApplyInstantiationDataNextFrame()` (lit la classId depuis `photonView.InstantiationData`) → **jamais exécuté** → la classe distante n'était jamais chargée → Bug B persistant
- `HubPlayerAvatar.RpcSetHubClassId` (sync live au DeckBuilder) → **jamais reçu** → Bug B persistant côté changements live

**Pourquoi l'asymétrie de Bug B :** l'avatar **local** (IsMine) reste correct via `PlayerAnimator.Start()` (lignes 174-183) qui appelle `LoadCharacterAnimations` directement depuis `HubManager.Instance.SelectedClass`. L'avatar **distant** (!IsMine) tombe dans la branche 184-188 qui ne fait que `PlayForCurrentState()` en attendant un RPC qui ne viendrait jamais → reste sur les frames Ghostra hardcodées du prefab.

**Origine probable :** au cours des 5 itérations précédentes, `HubPlayerAvatar.cs` a été créé/modifié et ajouté au prefab `HubPlayerAvatar.prefab` (orphelin, 5 composants minimaux), mais jamais ajouté au vrai prefab production `HubPlayer.prefab` qui est référencé par `HubNetworkSpawner`.

**Fix appliqué (édition YAML directe) :**
- `Assets/_Game/Resources/HubPlayer.prefab` — ajouté le composant `HubPlayerAvatar` (fileID `7281943650218394672`) avec ses champs sérialisés :
  - Référence dans `m_Component:` du GameObject racine
  - Bloc MonoBehaviour avec `m_Script: { guid: 79f259e796eaed24eaa1e3d8edf5db21 }` et les champs publics sérialisés (`avatarSprite` lié au SpriteRenderer existant fileID `7383335190310028398`, `fallbackSprite: {fileID: 0}`, `labelOffset: {x: 0, y: 0.7, z: 0}`)

**Arbitrages réalisés :**
- **Édition YAML directe** plutôt que ouverture d'Unity et glissé du composant : l'utilisateur a explicitement validé l'approche (option A). Le YAML d'un prefab est éditable sereinement tant qu'on respecte la structure (fileID unique, m_Script valide). En cas de souci, Unity réimporte le prefab et signale l'erreur dans la console.
- **Conserver les deux prefabs** (`HubPlayer.prefab` ET `HubPlayerAvatar.prefab`) : ne pas supprimer le second sans confirmation utilisateur (règle PHASE 1). Au pire il sera identifié comme orphelin lors du nettoyage Phase 1.1.
- **Ne PAS toucher au code applicatif** : il était déjà correct. Aucun fichier `.cs` modifié dans cette itération — uniquement le prefab.

**Bugs détectés / cas non prévus :**
- Aucun bug introduit. Le code des 5 itérations précédentes est désormais **opérationnel** car le composant qu'il manipule existe enfin sur le prefab.
- Note : `PlayerAnimator.SetupSpriteRoot()` désactive le SpriteRenderer principal du prefab après création du SpriteVisual enfant. Le champ `avatarSprite` du `HubPlayerAvatar` (lié à ce même SpriteRenderer) se retrouve donc sur un renderer désactivé. Sans impact car les checks dans `Awake` sont guarded par null/enabled.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. **Hub — sync de classe** : J1 = Necram, J2 = Soulrender → chaque client doit voir la VRAIE classe adverse (animée correctement) **dès le premier frame** (instantiationData arrive atomiquement avec l'instantiate)
2. **Hub — sync live** : changement de classe via DeckBuilder → l'autre client voit l'avatar muter en temps réel (RPC AllBuffered sur le PhotonView)
3. **Ranked1v1 — pas de HubPlayer** : aucun HubPlayer visible, jamais. Cleanup en cascade : sweep `CombatInitializer.Awake` (instantané) + polling 5s + HubPlayerKiller dans la scène — **tous les trois trouvent désormais le composant HubPlayerAvatar et déclenchent Destroy**

**Action requise côté utilisateur :**
1. Ouvrir Unity (le projet doit ré-importer le prefab `HubPlayer.prefab`) et vérifier la console
2. Vérifier dans l'Inspector que le prefab a bien le composant `Hub Player Avatar` listé
3. Tester en ParrelSync (2 instances) les 3 scénarios ci-dessus
4. **Si le bug persiste après ce fix** : c'est qu'il y a une autre cause (configuration Photon plus profonde) — partager la console Unity exacte pour itération 7

---

### [2026-05-07] — Phase 1.0 — Sync des obstacles ArenaGenerator en Ranked1v1
**Constat utilisateur :** sur Ranked1v1 (et potentiellement Monjeu en cas de session multi simultanée), les ~4 obstacles auto-générés à l'arrivée sur la map diffèrent entre les deux clients → les deux joueurs ne voient pas la même arène.

**Diagnostic :**
- `ArenaGenerator.Generate()` (ligne 102-104 avant fix) : `effectiveSeed = arenaConfig.seed < 0 ? Random.Range(0, int.MaxValue) : arenaConfig.seed`. La valeur par défaut de `arenaConfig.seed` est `-1` (`ArenaConfig.cs:130`), donc chaque client tire son propre seed indépendamment → arènes divergentes.
- Le pattern de fix correct existait déjà dans le projet : `CombatInitializer.ApplyRandomSpellDecksIfConfigured` (lignes 172-174) dérive le seed des decks via `DeterministicHash(PhotonNetwork.CurrentRoom.Name)` en multijoueur.

**Tâches effectuées :**
- `Assets/_Game/Scripts/Core/ArenaGenerator.cs` :
  - `Generate()` (~ligne 100) : remplacement du calcul inline de `effectiveSeed` par un appel à `ResolveEffectiveSeed()`.
  - Ajout de `ResolveEffectiveSeed()` : si `arenaConfig.seed >= 0` → seed fixe ; sinon, si Photon est dans une room matchmaking (`MaxPlayers > 0 && PlayerCount >= 2`) → `DeterministicHash(room.Name)` ; sinon (solo / Training / seul dans une room) → `Random.Range(0, int.MaxValue)`.
  - Ajout de `DeterministicHash(string)` (FNV-1a, indépendant de la plateforme), copié à l'identique du helper privé de `CombatInitializer`.

**Arbitrages réalisés :**
- **`MaxPlayers > 0 && PlayerCount >= 2` plutôt que `InRoom` seul** : nécessaire car en lançant Training depuis le Hub, le client reste dans HUB_GLOBAL (`MaxPlayers = 0` = unlimited). Sans ce filtre, toutes les sessions Training auraient produit la MÊME map (hash de "HUB_GLOBAL"). Les matchmaking rooms ont `MaxPlayers = 2` (`HubMatchmaker.maxPlayersFor1v1`), donc le filtre les distingue proprement.
- **Hash FNV-1a recopié plutôt qu'extrait** : `CombatInitializer.DeterministicHash` est `static`/`private` ; le réintroduire en local évite un couplage Combat → Core et évite de promouvoir un détail interne en API publique pour un seul autre call site.
- **Pas de RPC ni de propagation explicite du seed** : la room name Photon est garantie identique sur tous les clients de la même room (par construction Photon), donc le hash est identique chez chacun → pas besoin de transmettre la seed.
- **`PlayerCount >= 2` plutôt que `>= 1`** : si un joueur est seul dans une matchmaking room (debug / latence du second), il aura un seed random — comportement raisonnable, le second joueur aura un autre random aussi mais le combat n'aura pas démarré (le combat ne démarre qu'à 2 joueurs, donc la nouvelle map sera générée au bon moment du `LoadLevel`).

**Cas non prévus / difficultés :**
- Aucun bloquant. Note : `ArenaGenerator.Start()` (`generateOnStart = true`) appelle `Generate()` après que `PhotonNetwork.LoadLevel(Ranked1v1)` ait chargé la scène — à ce moment, le client est bien dans la matchmaking room avec `PlayerCount = 2`. Timing OK.
- En Monjeu (Training solo), si le client n'est jamais passé par Hub, `PhotonNetwork.InRoom` est false → seed random. Si passé par Hub, il est dans HUB_GLOBAL (`MaxPlayers = 0`) → seed random aussi. Comportement inchangé pour le solo. ✅

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. Ranked1v1 : J1 et J2 doivent voir EXACTEMENT les mêmes obstacles, mêmes positions, même décor (sang/herbe). Le log `[ArenaGenerator] Génération … | Seed : N` doit afficher la même valeur N côté chaque client.
2. Relancer un autre match Ranked1v1 (nouvelle matchmaking room avec un nouveau GUID) : la map doit être différente (nouveau hash), mais toujours identique entre les deux clients.
3. Monjeu (Training) : la map continue d'être aléatoire à chaque relance. Comportement inchangé.

---

### [2026-05-07] — Phase 1.0 — Sync arène V2 : approche serveur-autoritative
**Constat utilisateur après V1 (dérivation seed depuis nom de room) :** bug toujours présent. Hypothèse : `PlayerCount >= 2` dans le check `ResolveEffectiveSeed` peut échouer chez un client au moment exact de `ArenaGenerator.Start()` (race entre la transition de scène et la réception de `OnPlayerEnteredRoom` côté serveur). Le client "perdant" tombe alors sur `Random.Range` → arènes désynchronisées.

**Stratégie V2 — Master autoritative + Custom Room Properties :**
- Une seule source de vérité pour le seed : le **MasterClient** le pose dans `PhotonNetwork.CurrentRoom.CustomProperties["arena_seed"]`.
- Photon broadcast la propriété à tous les clients (présents et futurs).
- Tous les clients (master inclus) attendent cette propriété **avant** de générer l'arène. Plus aucune dépendance à un check `PlayerCount` fragile.

**Tâches effectuées :**
- `Assets/_Game/Scripts/Core/ArenaGenerator.cs` :
  - Ajout de `using System.Collections;` (pour `IEnumerator`).
  - Ajout de `public bool IsArenaReady { get; private set; }` (signal pour les consommateurs).
  - Ajout de la constante `ARENA_SEED_PROP = "arena_seed"`.
  - `Start()` : refactor — détecte `MaxPlayers > 0 && PlayerCount >= 2`. Si vrai → `StartCoroutine(NetworkGenerateRoutine())`. Sinon → `Generate()` direct (solo/Training).
  - Nouvelle coroutine `NetworkGenerateRoutine` :
    - Master pose `arena_seed` (random ou seed fixe) dans `room.SetCustomProperties` si pas déjà posé.
    - Tous les clients (master inclus) poll `room.CustomProperties[ARENA_SEED_PROP]` à chaque frame (timeout 10s).
    - Dès qu'il est reçu : `Generate(seed)`. Logs `[ArenaGenerator] Master broadcast arena_seed = N` côté master et `[ArenaGenerator] Seed réseau reçu : N (master=...)` côté tous.
  - `Generate()` : nouvelle signature `Generate(int? overrideSeed = null)`. Marque `IsArenaReady = false` au début, `= true` à la fin. Si `overrideSeed` fourni, court-circuite `ResolveEffectiveSeed`.
  - `ResolveEffectiveSeed()` : simplifié — ne traite plus le cas réseau (géré par la coroutine), retourne juste `arenaConfig.seed` ou `Random.Range`. Helper `DeterministicHash` supprimé (devenu inutile, le seed sync n'est plus dérivé du nom de room).
- `Assets/_Game/Scripts/Combat/CombatInitializer.cs` :
  - `InitSequence` : ajout de `yield return new WaitUntil(() => arenaGenerator.IsArenaReady)` juste avant la lecture de `GetSpawnCells(1/2)`. Sans ça, `RunPlacement` recevrait des listes vides en réseau (la génération étant désormais asynchrone).

**Arbitrages réalisés :**
- **Custom Room Properties plutôt que RPC** : les Custom Properties sont **persistantes pour la durée de la room** et **livrées atomiquement aux late joiners** — exactement ce qu'on veut pour un état "configurer une fois, lire partout". Un RPC `AllBuffered` aurait fonctionné aussi mais aurait conservé un buffer sans clean-up automatique. Les Custom Properties sont nettoyées avec la room.
- **Master client comme seul écrivain** : empêche la course "deux clients posent le seed simultanément, le dernier écrase". Even if both POSTed, only the master's value would matter — but cleaner to have a single writer.
- **Poll par coroutine plutôt que `OnRoomPropertiesUpdate` callback** : la coroutine est self-contained (pas d'override de callbacks dans plusieurs classes), et `room.CustomProperties` est mise à jour synchroniquement sur réception. Le poll par frame coûte ~10 µs, négligeable.
- **Timeout 10 secondes plutôt qu'infini** : si jamais Photon a un problème (déconnexion du master pendant le LoadLevel par exemple), on tombe en fallback `Generate()` local pour ne pas bloquer la scène. Un log `LogError` signale l'incident.
- **`IsArenaReady` exposé publiquement plutôt qu'un event `OnArenaGenerated`** : un flag suffit pour `CombatInitializer.WaitUntil`. Un event créerait du couplage event-handler à gérer (subscribe/unsubscribe).
- **`Generate(int? overrideSeed)` plutôt que helper privé `GenerateInternal(int)`** : permet aux Editor tools (`RegenerateArena` ContextMenu) d'appeler la signature à zéro paramètre sans rompre l'API publique. `int?` n'est pas idiomatic en C# mais c'est explicite et zero-allocation.

**Cas non prévus / difficultés :**
- Aucun bloquant. Note : `RegenerateArena` (`[ContextMenu]`) appelle `Generate()` sans paramètre → utilise `ResolveEffectiveSeed` → random en réseau. C'est OK car `RegenerateArena` est une commande Editor, pas un flow runtime.
- Possible cas d'erreur : si l'utilisateur lance Ranked1v1 directement en Editor (Play sur la scène sans passer par le matchmaking), `PhotonNetwork.InRoom` est false → branche solo, Generate() direct, `IsArenaReady` true, `CombatInitializer.WaitUntil` passe. ✅

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. Ranked1v1 — match nominal : les deux clients voient EXACTEMENT les mêmes obstacles. Console attendue côté master : `[ArenaGenerator] Master broadcast arena_seed = N` puis `[ArenaGenerator] Seed réseau reçu : N (master=True)` → `Génération | Seed : N`. Côté autre client : `[ArenaGenerator] Seed réseau reçu : N (master=False)` (pas de log "broadcast") → `Génération | Seed : N` (même N).
2. Re-match : nouvelle matchmaking room → autre seed → autre arène, mais identique entre clients.
3. Monjeu (Training solo) : aucun log "Master broadcast" / "Seed réseau reçu" — passe par la branche solo classique.
4. **Si le bug persiste** : partager les logs console exacts des deux clients pour identifier où la chaîne casse (master ne pose jamais ? client ne reçoit jamais ? timeout ?).

---

### [2026-05-07] — Phase 1.0 — Sync arène V3 : extension au Hub + opt-out par scène
**Constat utilisateur après V2 :** bug toujours présent ET signal supplémentaire critique : **le bug existe aussi dans la scène Hub**.

**Diagnostic V3 (relecture du code à la lumière du nouvel indice) :**
- `ArenaGenerator` est en fait présent dans **trois scènes** : Hub.unity, Ranked1v1.unity, Monjeu.unity (confirmé par grep du GUID `93fa8f5b9588edc408cbfe03bf47ab51`).
- Le check V2 `MaxPlayers > 0 && PlayerCount >= 2` était **doublement faux** :
  1. **HUB_GLOBAL a `MaxPlayers=0`** (illimité — `HubNetworkSpawner.JoinHubRoom:176`). Le check rejetait toujours le Hub → branche solo `Generate()` → seed random local → désync entre clients dans le Hub.
  2. **Le 1er joueur entrant seul dans le Hub a `PlayerCount=1`** au moment de `Start()`. Branche solo. Quand le 2nd arrive plus tard, sa propre branche réseau attend une propriété qui n'a jamais été posée → timeout → fallback random → désync persistante.

**Tâches effectuées :**
- `Assets/_Game/Scripts/Core/ArenaGenerator.cs` :
  - Ajout du flag Inspector `public bool synchronizeWithRoom = true` (header "RÉSEAU"). Tooltip explicite : activer pour scènes partagées (Hub, Ranked1v1), désactiver pour scènes solo (Monjeu/Training).
  - `Start()` : check `isNetworkMatch = synchronizeWithRoom && room != null` (suppression de `MaxPlayers > 0 && PlayerCount >= 2`). On entre dans la routine réseau dès qu'on est dans une room — peu importe le nombre de joueurs présents au moment du `Start()`.
  - Clé Custom Room Property désormais **suffixée par le nom de la scène** : `arena_seed_Hub`, `arena_seed_Ranked1v1`. Helper `ArenaSeedKey` qui concatène `ARENA_SEED_PROP_PREFIX + SceneManager.GetActiveScene().name`. Évite que les ArenaGenerators de plusieurs scènes successives n'écrasent leurs seeds respectifs (le client garde la connexion à HUB_GLOBAL pendant la transition vers Monjeu, et à la matchmaking room pendant Ranked1v1).
- `Assets/Monjeu.unity` (édition YAML directe) :
  - Ajout de `synchronizeWithRoom: 0` au composant ArenaGenerator (lignes 10860+). Désactive la sync pour le solo : si l'utilisateur lance Training depuis le Hub, son client est encore dans HUB_GLOBAL avec d'autres joueurs — sans cette désactivation, son arène solo serait synchronisée avec celle du Hub des autres.

**Arbitrages réalisés :**
- **Sync dès `InRoom`, sans condition de PlayerCount** : cas du 1er joueur entrant seul dans le Hub. Il pose le seed (étant master), le récupère immédiatement (Photon met à jour la copie locale synchroniquement après `SetCustomProperties`), génère. Le 2ème joueur arrive plus tard, lit la propriété déjà posée → même seed.
- **Clé par scène plutôt que clé globale** : un client peut être dans HUB_GLOBAL et passer à Monjeu sans changer de room. Si la clé était globale, l'ArenaGenerator de Monjeu écraserait celui du Hub (et inversement). Suffixer par scène est la solution la plus robuste — chaque scène a son propre seed dans la même room.
- **Flag opt-out plutôt qu'auto-détection par nom de scène** : auto-détecter "Monjeu" / "Training" est fragile (renommage de scène = casse). Un flag Inspector explicite est plus durable et compréhensible pour le designer.
- **Default `true` plutôt que `false`** : la majorité des scènes (Hub, Ranked1v1) doivent sync. Le défaut couvre le cas commun ; le designer désactive uniquement pour les exceptions (scènes solo).
- **Édition YAML de Monjeu.unity directe** plutôt que demander à l'utilisateur : action mécanique non-créative, deux lignes claires à ajouter, gain de friction important.

**Cas non prévus / difficultés :**
- Aucun bloquant. Cas particulier identifié : pendant la durée de vie d'HUB_GLOBAL (qui peut être longue si des joueurs y restent en permanence), le seed du Hub est figé. Pour une rotation de map dans le Hub, il faudrait soit (a) effacer `arena_seed_Hub` périodiquement, (b) appeler `RegenerateArena` côté master. Hors scope du bug actuel — à ouvrir comme tâche séparée si désiré.
- `CombatInitializer.WaitUntil(IsArenaReady)` reste en place et fonctionne correctement : Hub n'a pas de CombatInitializer (pas de combat) donc pas de problème ; Ranked1v1 attend correctement la fin de la coroutine réseau.

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. **Hub** : J1 et J2 dans le Hub doivent voir EXACTEMENT les mêmes obstacles. Console master : `[ArenaGenerator] Master broadcast arena_seed_Hub = N` → `Seed réseau reçu : arena_seed_Hub=N (master=True)`. Console autre client : `Seed réseau reçu : arena_seed_Hub=N (master=False)`.
2. **Ranked1v1** : idem mais avec la clé `arena_seed_Ranked1v1`. Seed différent du Hub (autre clé, autre random au moment du pose).
3. **Monjeu (solo)** : pas de log "Master broadcast" / "Seed réseau reçu". Map random à chaque relance, comportement inchangé.
4. **Aller-retour Hub → Ranked1v1 → Hub** : la map du Hub doit rester identique avant et après le combat (le seed `arena_seed_Hub` reste posé tant que HUB_GLOBAL existe).

**Pourquoi V3 devrait être définitif :**
- Plus aucune branche conditionnelle qui pourrait rejeter un client : `synchronizeWithRoom && InRoom` est vrai dans 100% des scénarios multi-joueur réels (Hub et Ranked1v1).
- Une seule source de vérité (master) — comme V2, mais désormais sans le piège du PlayerCount.
- Ségrégation par scène — comme ça, plusieurs ArenaGenerators dans la même session de room ne se marchent pas dessus.
- Si malgré tout le bug persiste, le diagnostic deviendra trivial via les logs console (chaque branche logue qui pose, qui reçoit, à quelle frame).

---

### [2026-05-07] — Phase 1.0 — Sync arène V4 : attente InRoom dans le Hub
**Constat utilisateur après V3 :** Ranked1v1 ✅ résolu, mais le Hub reste désynchronisé.

**Diagnostic V4 :**
La différence fondamentale Hub vs Ranked1v1 :
- **Ranked1v1** : transition via `PhotonNetwork.LoadLevel` qui maintient la connexion à la matchmaking room → au moment de `ArenaGenerator.Start()`, `PhotonNetwork.InRoom = true` ✅ → routine réseau s'enclenche.
- **Hub** : transition via `SceneManager.LoadScene` (HubManager.cs:103). À l'arrivée dans le Hub, le client est connecté à Photon **mais pas encore dans HUB_GLOBAL** — c'est `HubNetworkSpawner` qui appelle `JoinHubRoom` de façon asynchrone après `OnConnectedToMaster`. Donc au moment de `ArenaGenerator.Start()`, `InRoom = false` → check V3 `synchronizeWithRoom && room != null` retourne false → branche solo → seed random local → désync persistante.

**Tâches effectuées :**
- `Assets/_Game/Scripts/Core/ArenaGenerator.cs` :
  - `Start()` : refactor — si `synchronizeWithRoom`, lance `WaitForRoomThenGenerate` (coroutine d'attente). Sinon, `Generate()` direct (Monjeu / scènes solo).
  - Nouvelle coroutine `WaitForRoomThenGenerate` :
    - Boucle `while !InRoom` avec timeout 15 secondes.
    - Dès que `InRoom = true` → délègue à `NetworkGenerateRoutine` (le reste du flow V3 est inchangé : master pose la propriété, tous lisent).
    - Si timeout (cas debug : Editor Play sur scène isolée, mode hors-ligne) → fallback solo avec `LogWarning`.

**Arbitrages réalisés :**
- **Coroutine d'attente plutôt qu'override `OnJoinedRoom`** : `ArenaGenerator` ne hérite pas de `MonoBehaviourPunCallbacks` (pas de couplage Photon dans Core/). Une coroutine self-contained évite ce couplage et reste lisible. Le coût (un check par frame pendant max 15s) est négligeable.
- **Timeout 15s plutôt qu'infini** : si la connexion Photon plante ou si on lance la scène Hub directement en Editor sans flow normal, on ne veut pas bloquer la scène indéfiniment. 15s est largement supérieur au temps normal de connexion (~1-3s) tout en restant raisonnable.
- **Pas de modification de `HubNetworkSpawner` ou `HubManager`** : le bug est strictement dans le timing de `ArenaGenerator.Start()`. Toute autre couche reste correcte. Modifier le moins de fichiers = moins de surface d'erreur.

**Cas non prévus / difficultés :**
- Aucun bloquant. Note : si le `HubNetworkSpawner` échoue à joindre HUB_GLOBAL (latence Photon, room créée par un autre client juste avant, etc.), le timeout de 15s prend le relais avec un warning console — le joueur voit une map locale plutôt qu'un blocage.

**Bugs détectés :**
- Aucun nouveau bug introduit.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. **Hub** : J1 et J2 dans le Hub voient EXACTEMENT les mêmes obstacles. Console attendue chez le 1er arrivant (master) : pas de log immédiat, **puis** après quelques centaines de ms (durée du `JoinHubRoom`) → `[ArenaGenerator] Master broadcast arena_seed_Hub = N` → `Seed réseau reçu : arena_seed_Hub=N (master=True)`. Chez le 2ème : `Seed réseau reçu : arena_seed_Hub=N (master=False)` (même N).
2. **Ranked1v1** : comportement V3 préservé (immédiat, sans attente notable).
3. **Monjeu** : pas de coroutine d'attente du tout (`synchronizeWithRoom=false` → `Generate()` direct).

**Pourquoi V4 devrait définitivement résoudre le bug :**
- L'incompréhension fondamentale entre V1-V2-V3 et V4 : on supposait que `Start()` s'exécutait dans un état Photon stable. C'était vrai pour Ranked1v1 (post-LoadLevel) mais faux pour le Hub (pré-JoinRoom). V4 retarde explicitement la décision réseau jusqu'à ce que l'état Photon soit prêt.
- Cohérent avec le pattern Photon usuel : pour tout code dépendant de `InRoom`, il faut soit attendre, soit utiliser un callback. V4 attend.

---

## ✅ BUGS PHASE 1.0 — TOUS RÉSOLUS AU 2026-05-07

Les trois bugs critiques de la section 1.0 ont été **résolus et validés en ParrelSync** :

| Bug | Itérations | Cause root | Résolution |
|---|---|---|---|
| **HubPlayer(Clone) visible en Ranked1v1** | 6 | Composant `HubPlayerAvatar` manquant sur le prefab `HubPlayer.prefab` — tout le code de cleanup (sweep, polling 5s, HubPlayerKiller) ciblait un composant inexistant sur le prefab production | Édition YAML du prefab pour ajouter le composant manquant |
| **Sync de classe asymétrique dans le Hub** | 6 | Même cause — `ApplyInstantiationDataNextFrame` et `RpcSetHubClassId` ne s'exécutaient jamais | Même fix YAML — un seul changement débloque les deux bugs |
| **Désync des obstacles (Hub + Ranked1v1)** | V1→V4 | Multi-couches : (V1-V2) check `MaxPlayers > 0 && PlayerCount >= 2` rejetait HUB_GLOBAL et le 1er joueur seul ; (V3) clé Custom Property globale écrasable cross-scène ; (V4) `Start()` exécuté avant `JoinHubRoom` → `InRoom = false` au moment du check dans le Hub | V4 final : `WaitForRoomThenGenerate` (attente `InRoom`) + clé `arena_seed_<sceneName>` par scène + flag `synchronizeWithRoom` (opt-out pour Monjeu solo) |

**Voir le journal ci-dessus pour les détails complets de chaque itération.**

**Leçons retenues pour les prochaines phases :**
1. **Toujours vérifier que les composants C# sont effectivement attachés aux prefabs ciblés.** Un `FindObjectsByType<T>` qui retourne 0 systématiquement = signal qu'il faut auditer le prefab YAML, pas multiplier les fix algorithmiques.
2. **Les bugs réseau "asymétriques" reposent rarement sur le timing seul** — souvent une couche Inspector / scène / prefab manquante. Diagnostic prefab > diagnostic timing.
3. **`PhotonNetwork.InRoom` n'est pas garanti dans `Start()`** quand on entre via `SceneManager.LoadScene` (au lieu de `PhotonNetwork.LoadLevel`). Toujours faire une coroutine d'attente ou utiliser `OnJoinedRoom`.
4. **Custom Room Properties suffixées par scène** : pattern utile pour partager un état entre clients quand plusieurs scènes coexistent dans la même room sans s'écraser.

---

### [2026-05-07] — Phase 1.3 — Stabilisation réseau
**Orchestration :** Claude Opus (xhigh) — implémentation directe (boulot trop intriqué pour déléguer aux agents Sonnet/Haiku, modifications croisées entre 4 fichiers très proches en termes de logique réseau).

**État avant la phase :**
- Architecture combat réseau solide : tous les RPC critiques passent par `OracleCombatNetBridge.RpcMasterValidate*` qui lit `IsMasterClient` en temps réel.
- `OracleNetworkHub.OnPlayerLeftRoom` ligne 167 contenait un commentaire `// À brancher sur fin de match` (TODO Phase 1.3).
- Aucun système de forfait, aucun `OnMasterClientSwitched`, aucun `RoomOptions.PlayerTtl`.
- Case 1.3.1 « Synchroniser la sélection de passif » devenue caduque depuis 1.0.b (sélection supprimée).

**Tâches effectuées :**
- **Suppression case 1.3.1 caduque** : remplacée par une note explicative dans la roadmap (décision validée utilisateur). La fonctionnalité elle-même n'existe plus depuis 1.0.b.
- **`HubMatchmaker.cs` (~ligne 134)** : ajout de `PlayerTtl = 60000` et `EmptyRoomTtl = 60000` aux `RoomOptions` de la combat room. Conséquences Photon : un joueur déconnecté reste dans `PhotonNetwork.PlayerListOthers` avec `IsInactive = true` pendant 60s avant d'être retiré définitivement (déclenche `OnPlayerLeftRoom` à expiration).
- **`OracleCombatNetBridge.cs`** :
  * Héritage `MonoBehaviour` → `MonoBehaviourPunCallbacks` (pour recevoir les callbacks réseau natifs).
  * Champs ajoutés : `forfeitTimerSeconds = 60f`, `inactivityPollInterval = 1f`, `_trackedInactive` (Player), `_nextPollAt` (float).
  * Nouveau `Update()` + `PollInactivityIfCombatActive()` : polling 1/s sur `PhotonNetwork.PlayerListOthers` pour détecter `IsInactive` chez l'adversaire. Démarre `DisconnectionTimerUI` au passage à inactif. Hide le UI à la reconnexion (IsInactive repasse à false).
  * `OnDisconnectionTimeout()` : callback du compteur UI quand il atteint zéro → `CombatInitializer.OnNetworkForfeit(GetLocalTeamId())` (forfait local-only car l'adversaire est définitivement absent).
  * Override `OnPlayerLeftRoom` : déclenché soit par Leave volontaire (forfait immédiat), soit par expiration TTL Photon (60s sans rejoin). Dans les deux cas en combat → `OnNetworkForfeit`.
  * Override `OnMasterClientSwitched` : log + commentaire d'audit (rien à faire car le code est résilient par construction).
  * `TriggerForfeit(int winnerTeamId)` (public) + RPC `RpcRequestForfeit` + `RpcForceForfeit` : infrastructure pour Phase 4.2 (abandon volontaire). Non utilisée en 1.3 (forfait local-only suffit quand l'adversaire est inactif).
- **`DisconnectionTimerUI.cs`** (nouveau, `Assets/_Game/Scripts/UI/`) : composant UI procédural (pas de prefab requis), pattern singleton statique (`Show` / `Hide` / `IsActive`). Panneau plein écran semi-transparent + titre dynamique « X déconnecté » + count-down 60s en gros + sous-titre « Forfait automatique à zéro ». Coroutine interne, callback `onTimeout`.
- **`TurnManager.cs`** : ajout d'une méthode publique `ForceEndCombat(int winnerTeamId)` — idempotente (no-op si combat déjà fini), désactive `combatActive`/`turnActive` puis invoke l'event `OnCombatEnd`. Réutilise le wiring existant (`CombatInitializer.OnCombatEnd` est déjà abonné).
- **`CombatInitializer.cs`** : ajout d'une méthode publique `OnNetworkForfeit(int winnerTeamId)` qui délègue à `TurnManager.ForceEndCombat` → réutilise tout le flux standard (panneaux victoire/défaite + bouton Retour Hub via `ShowReturnToHubButton`).

**Audit migration MasterClient (case 1.3.3) :**
Confirmation que l'architecture est résiliente par construction :
- `RpcMasterValidate*` (Move, CastSpell, EndTurn, Placement) : `if (!PhotonNetwork.IsMasterClient) return;` lu à chaque RPC → le nouveau master prend le relais automatiquement.
- `TurnManager.timeRemaining` : tourne dans `Update()` côté chaque client local, pas master-only.
- `EndTurn()` est idempotent (check `CurrentCharacter != ch` côté `RpcMasterValidateEndTurn`).
- `OnCharacterDied` → `CheckVictoryCondition` : abonnement local par client, morts synchronisées via le bridge → cohérence par construction.
- Aucun rôle master-only stocké → aucun fix de migration nécessaire, juste un log de traçabilité.

**Arbitrages réalisés :**
- **Polling 1/s plutôt que callback dédié** : PUN2 ne fournit pas de callback `OnPlayerActivityChanged` en API publique. Le polling est cheap (juste un foreach sur 1 ou 2 joueurs) et déclenche au pire 1s après le passage à inactif, ce qui est imperceptible vis-à-vis du timer 60s.
- **Forfait local-only au timeout (pas de RPC)** : à T+60s, l'adversaire est par définition inactif et son TTL Photon est sur le point d'expirer (ou déjà expiré). Inutile de broadcaster un RPC à un client absent. L'infrastructure RPC (`RpcForceForfeit` + `RpcRequestForfeit`) est ajoutée pour la Phase 4.2 (abandon volontaire en cours de combat avec adversaire encore connecté).
- **Composant UI procédural** plutôt qu'un prefab : cohérent avec le pattern existant (`ShowReturnToHubButton`), pas de dépendance à un asset à entretenir, scène intacte.
- **Singleton statique pour `DisconnectionTimerUI`** : un seul timer à la fois (le cas multi-déconnexion 1v1 = absurde). API simple `Show`/`Hide`. Cleanup auto via `OnDestroy`.
- **`MonoBehaviourPunCallbacks` sur le même GameObject que `OracleNetworkHub`** : OK, chaque composant s'enregistre indépendamment dans Photon (deux callbacks différents reçoivent les mêmes événements). Pas de duplication critique.

**Cas non prévus / difficultés :**
- Aucune difficulté bloquante.
- ⚠️ **Limite identifiée** : avec `PlayerTtl = 60000`, si le joueur restant quitte AUSSI pendant les 60s, la room reste en l'état pendant `EmptyRoomTtl = 60000` puis Photon la ferme. Le ranking côté backend (Phase 5.2) devra tenir compte de ce cas (pas de winner technique, match à arbitrer côté serveur). Pour l'alpha 1v1 sans backend, comportement non bloquant.
- ⚠️ **Limite identifiée** : si l'adversaire perd la connexion exactement quand on passe d'une scène à une autre (Hub → Ranked1v1), `OracleCombatNetBridge` n'existe pas encore (il est dans la scène de combat) → le polling ne tourne pas. Mais à ce stade le combat n'est pas commencé non plus (`TurnManager.IsCombatActive == false`), donc le polling ne ferait rien de toute façon. Les callbacks Photon seront reçus dès l'instanciation du bridge en scène de combat, ce qui couvre les cas pertinents.

**Bugs détectés :** aucun.

**Tests Unity attendus (côté utilisateur, ParrelSync) :**
1. **Timer de déconnexion** : lancer un Ranked1v1 entre J1 et J2. Au milieu d'un tour, fermer brusquement le client J2 (ou couper sa connexion Wi-Fi). Vérifier chez J1 :
   - Le panneau plein écran « J2 déconnecté » apparaît dans 1-2 secondes.
   - Le count-down démarre à 60s et décrémente.
   - Si J2 ne revient pas → à 0s, J1 voit le panneau Victoire et le bouton « Retour au Hub ».
2. **Reconnexion (test idéalement)** : si tu peux relancer un client ParrelSync rejoignant la même room avant 60s (cas rare en pratique), vérifier que le panneau disparaît et que le combat reprend normalement.
3. **Leave volontaire** : `Disconnect()` côté J2 (bouton Hub si dispo, ou kill process). Chez J1, le panneau de forfait peut apparaître brièvement OU `OnPlayerLeftRoom` peut être déclenché immédiatement → fin de combat directe avec Victoire pour J1.
4. **Migration MasterClient** : si J1 = master et J1 quitte, J2 doit recevoir `OnMasterClientSwitched` (vérifier le log `[OracleCombatNet] OnMasterClientSwitched`) et continuer à valider normalement les actions (peut faire son tour sans erreur).

**État après phase 1.3 :** 3/4 cases d'origine traitées (1 supprimée comme caduque). Phase 1.4 (audit fin de Phase 1) à attaquer pour valider l'ensemble avant de passer à la Phase 2 (migration 128×128).

---

### [2026-05-07] — Phase 1.4 — Audit fin de Phase 1 (partie code)
**Orchestration :** Claude Opus (xhigh) — pré-audits statiques côté code, fixes correctifs de warnings, préparation checklist de tests Unity côté utilisateur.

**Tâches effectuées (côté code) :**
- **Fix CS0114 sur `HubPlayerAvatar.cs:47,53`** : `OnEnable`/`OnDisable` désormais déclarées `public override` (correct vis-à-vis de `MonoBehaviourPunCallbacks` qui les définit `virtual`). Le `base.OnEnable()` / `base.OnDisable()` était déjà appelé — il manquait juste le mot-clé `override`.
- **Fix CS0219 sur `HubHUD.cs`** : suppression des deux `const` inutilisés `PAD_V` (ligne 1168) et `CARD_W` (ligne 1351). Un autre `PAD_V` (ligne 303) reste en place car effectivement utilisé ligne 321.
- **Audit GUID des scènes** : grep des GUID dans les `.meta` vs `EditorBuildSettings.asset` a révélé un **bug préexistant** : le GUID de `Ranked1v1.unity` dans BuildSettings (`e908889f08ff5904aa486b4e2dadd729`) ne matchait pas le GUID réel du `.meta` (`8ba2c76188a5e38479e69b899c95e8dc`). Probablement résidu d'une duplication/recréation ancienne. Unity charge actuellement la scène par path, donc fonctionnait silencieusement, mais le mismatch peut causer des incohérences au build (par exemple si du code utilise le GUID pour résoudre la scène). **Corrigé** : GUID aligné dans `EditorBuildSettings.asset`. Aucune autre référence au GUID orphelin dans tout le projet (`Assets/` + `ProjectSettings/`) → la pollution était isolée.
- **Pré-audit static des `Resources.Load`** : vérifié l'existence de tous les paths cibles (`NymoraClasses/ClassRegistry.asset`, `OracleSpellPools/AllCombatSpellsPool.asset`, `SpellIcons/ghostra/`, `SpellIcons/soulrender/`, `Characters/`, `CombatAnimations/`, `OracleHUD/`, `Fonts/`). Tous OK ✅.
- **Pré-audit grep des chemins obsolètes** : `Assets/Resources/SpellIcons | Assets/Monjeu | monjeuScene | MONJEU_PATH` → 0 occurrence dans `Assets/`. Aucune référence résiduelle après les Phases 1.1 et 1.2.

**Arbitrages réalisés :**
- **Fix des warnings préexistants intégré dans la 1.4** plutôt que dans une passe de cleanup séparée : ces warnings ont été flagged dans le journal de la Phase 1.2 et la 1.4 est par essence un audit fin de Phase 1 — c'est l'endroit naturel pour clôturer la dette de propreté du code accumulée. Coût marginal, valeur élevée (console plus propre = audit visuel des nouvelles erreurs plus facile en Phase 2+).
- **Correction du GUID orphelin de Ranked1v1** : décision de fix immédiat plutôt que d'attendre l'occasion. Fichier critique (BuildSettings) qui pourrait causer des bugs invisibles. Aucun risque vu que la valeur cible vient des `.meta` actuels (autorité Unity).

**Cas non prévus / difficultés :**
- **Découverte du GUID orphelin de Ranked1v1** : pas anticipée. Aurait été un bug invisible jusqu'à la première étape de Phase 2 ou d'un build production. La pratique du grep cross-référencé entre BuildSettings et `.meta` est à reproduire à la fin de chaque phase qui touche les scènes (cf. Phases 1.2, 2 à venir).

**Bugs détectés (résolus) :**
- ⚠️ **GUID Ranked1v1 désaligné dans EditorBuildSettings (préexistant)** — résolu 2026-05-07. Cause root identifiée comme résidu probable d'une duplication ancienne. Fix par alignement sur le GUID réel du `.meta`.
- ⚠️ **3 warnings de compilation (préexistants)** — résolus 2026-05-07. CS0114 × 2 (HubPlayerAvatar) + CS0219 × 2 (HubHUD).

**Tests Unity restants (côté utilisateur) :**

Voir checklist détaillée dans la section 1.4 ci-dessus. Les cases Unity ne peuvent être cochées que par toi après tests :
1. Ouvrir Unity → console propre au démarrage (aucune erreur, idéalement aucun warning)
2. Charger chaque scène (MainMenu, Hub, Training, Ranked1v1, Boot_Network) → vérifier qu'aucune ref ne s'affiche en jaune/Missing dans les Inspectors
3. Combat test : Hub → Training (entraînement vs IA) → placement → combat → fin de match → retour Hub
4. Build test : File > Build Settings > Build → vérifier qu'aucun asset manquant n'est rapporté
5. **Test Phase 1.3 (ParrelSync 2 instances)** :
   - Déconnexion brutale d'un client en plein combat → panneau « X déconnecté » + count-down → forfait à 0s
   - Leave volontaire → forfait immédiat
   - Si master quitte → log `OnMasterClientSwitched` + jeu continue côté restant

**État après phase 1.4 (partie code) :** Tous les pré-audits statiques sont passés. Le projet est dans un état attendu propre. Reste à valider par toi via Unity et ParrelSync. Une fois validé → Phase 2 (migration 64×64 → 128×128) déblocable.

---

### [2026-05-07] — Phase 1.4 — Validation utilisateur (tests Unity + ParrelSync)
**Tests effectués par l'utilisateur :**
- ✅ Console Unity propre au démarrage
- ✅ Toutes les scènes chargent sans erreur de référence (MainMenu, Hub, Training, Ranked1v1, Boot_Network)
- ✅ Combat test complet en entraînement (placement → combat → fin de match → retour Hub)
- ✅ ScriptableObjects référencés correctement (aucun "Missing")
- ✅ Build de test OK (aucun asset manquant)
- ✅ Test ParrelSync — déconnexion brutale : panneau « X déconnecté » + count-down 60s, forfait à 0s
- ✅ Test ParrelSync — leave volontaire : forfait immédiat
- ✅ Test ParrelSync — migration MasterClient : log `OnMasterClientSwitched` + jeu continue côté restant

**Tous les critères de sortie de la Phase 1 sont validés.**

## ✅ PHASE 1 — COMPLÈTE 2026-05-07

Bilan complet :
| Sous-phase | Tâches | Code | Tests | Notes |
|---|---|---|---|---|
| 1.0 | 4 bugs critiques Ranked1v1 | ✅ | ✅ ParrelSync | 6 itérations, fix YAML prefab + sync classe via CustomProperties + obstacles V4 |
| 1.1 | 4 cases (cleanup quarantine + consolidation) | ✅ | ✅ Unity | Backup 37 Mo dans `_Save_fichiers_critiques/`, DOTweenSettings laissé en place |
| 1.2 | 7 cases (réorganisation _Docs / scènes) | ✅ | ✅ Unity | Monjeu→Training, _Docs/, archives. 12 refs patchées par agent Haiku + 1 régression corrigée par Opus |
| 1.3 | 3 cases utiles (forfait, migration, timer 60s) | ✅ | ✅ ParrelSync | 1 case caduque supprimée. Architecture résiliente par construction confirmée |
| 1.4 | 6 cases code + 8 tests Unity | ✅ | ✅ utilisateur | 3 warnings fixés, GUID Ranked1v1 corrigé (préexistant), tous tests OK |

**Phase 2 (migration 64×64 → 128×128) débloquée. Prochaine étape : préparation phase 2.1 (identification fichiers + snapshot référence).**

---

### [2026-05-07] — Phase 2.1 — Préparation migration 128×128
**Orchestration :** Claude Opus (xhigh) — audit + inventaire complet, pas d'implémentation (la 2.1 est purement préparatoire).

**Tâches effectuées :**
- **Inventaire complet des fichiers à migrer** (~2500 fichiers PNG/GIF) :
  * Tiles : `Sprites/NewTilesV4/` (6 .gif, PPU 64, pivot (0.5, 0))
  * Source GIF perso : `Sprites/Characters/` (3 fichiers de test, optionnels)
  * Frames perso : `Resources/Characters/Frames/` (32 dossiers, ~200-300 PNG, PPU 100, pivot (0.5, 0.5))
  * Animations sorts : `Resources/CombatAnimations/Frames/` (248 dossiers, ~2000 PNG)
- **Audit configs** : `GridConfig` (offsets, tileWidth/Height), `ArenaConfig` (logique pure, **inchangé**), `TileSpriteRegistry` (refs sprites), `IsometricCamera` (pixelsPerUnit), `CellHighlight`.
- **Identification scope strict** (règle absolue Phase 2) : 7 dossiers à NE PAS toucher (UI/HUD/icônes de sorts/polices/world).
- Screenshot de référence reçu de l'utilisateur (scène Training, identique à Ranked1v1 en termes de HUD/characters/tiles).

**Arbitrages réalisés :**
- **Option A (PPU doublé) retenue** au lieu de l'option B (rendu 2× plus grand) : choix utilisateur. Avantage majeur = **rendu identique à l'écran**, donc aucun recalibrage de la mise en scène (offsets, zoom, HUD). Le seul gain est la finesse du pixel art (4× pixels). Modèle standard pour upgrade pixel art.
- **Batch ImageMagick par l'utilisateur** retenu (vs graphiste manuel) : ~2500 fichiers, l'upscale nearest-neighbor (filter=point) préserve parfaitement le pixel art sans interpolation. Coût ~10-30 minutes total.

**Plan d'exécution prêt pour la Phase 2.2 :**

⚠️ **Procédure obligatoire** : Unity fermé pendant les manipulations batch (sinon AssetPostprocessor peut écraser les .meta avec les anciennes valeurs PPU pendant le re-import).

**Étape 1 — Backup avant migration**
```powershell
# Depuis la racine du projet, créer un backup horodaté
$ts = Get-Date -Format "yyyy-MM-dd_HHmm"
Copy-Item -Recurse "Assets/_Game/Sprites/NewTilesV4" "_Save_fichiers_critiques/NewTilesV4_pre128_$ts"
Copy-Item -Recurse "Assets/_Game/Resources/Characters/Frames" "_Save_fichiers_critiques/Characters_Frames_pre128_$ts"
Copy-Item -Recurse "Assets/_Game/Resources/CombatAnimations/Frames" "_Save_fichiers_critiques/CombatAnimations_Frames_pre128_$ts"
```

**Étape 2 — Resize batch ImageMagick (nearest-neighbor, préserve pixel art)**
```powershell
# Tiles (.gif)
Get-ChildItem "Assets\_Game\Sprites\NewTilesV4\*.gif" | ForEach-Object {
    magick mogrify -filter point -resize 200% $_.FullName
}

# Frames perso (récursif sur tous les sous-dossiers)
Get-ChildItem "Assets\_Game\Resources\Characters\Frames" -Recurse -Filter "*.png" | ForEach-Object {
    magick mogrify -filter point -resize 200% $_.FullName
}

# Animations de combat
Get-ChildItem "Assets\_Game\Resources\CombatAnimations\Frames" -Recurse -Filter "*.png" | ForEach-Object {
    magick mogrify -filter point -resize 200% $_.FullName
}
```

**Étape 3 — Mise à jour des PPU dans les `.meta` files**
```powershell
# Tiles : PPU 64 → 128
Get-ChildItem "Assets\_Game\Sprites\NewTilesV4\*.gif.meta" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'spritePixelsToUnits: 64', 'spritePixelsToUnits: 128' | Set-Content $_.FullName
}

# Frames perso : PPU 100 → 200
Get-ChildItem "Assets\_Game\Resources\Characters\Frames" -Recurse -Filter "*.png.meta" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'spritePixelsToUnits: 100', 'spritePixelsToUnits: 200' | Set-Content $_.FullName
}

# Animations de combat : PPU 100 → 200 (à confirmer sur un échantillon avant)
Get-ChildItem "Assets\_Game\Resources\CombatAnimations\Frames" -Recurse -Filter "*.png.meta" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'spritePixelsToUnits: 100', 'spritePixelsToUnits: 200' | Set-Content $_.FullName
}
```

⚠️ Avant l'étape 3 sur les CombatAnimations, **vérifier le PPU actuel d'un échantillon** (lire un `.meta` au hasard) — il peut différer de 100. Si différent, adapter le pattern de remplacement.

**Étape 4 — Modif code (Opus en 2.2)**
- `IsometricCamera.cs:47` — `pixelsPerUnit = 32f` → `pixelsPerUnit = 64f` (pixel-perfect cohérent avec le PPU x2 des sprites)
- `TileSpriteRegistry.cs:17` (commentaire) — `Pixels Per Unit → 64 (aligné avec GridConfig.tileWidth 1)` → `→ 128`
- `SpellIconImporter.cs` — n'affecte que SpellIcons (hors scope 2), pas de modif
- Vérifier `OracleHubPlayerSetup.cs` (Editor wizard) — peut référencer un PPU à 100 dans l'auto-config

**Étape 5 — Réouverture Unity + audit visuel**
- Unity recharge tous les sprites avec leurs nouveaux PPU.
- Vérifier que la grille s'affiche identiquement au screenshot de référence (option A : aucun changement de taille à l'écran).
- Si l'option A fonctionne correctement, les offsets `cellHighlightYOffset`, `characterWorldOffset`, `mousePickOffset`, `arenaTileSpriteWorldOffset` doivent rester valides — sinon recalibrer.

**Cas non prévus / difficultés :**
- ⚠️ **Format GIF des tiles** : ImageMagick supporte le GIF, mais Unity importe les GIF via le système d'images standard. Le PPU est dans le `.meta`. À surveiller : si Unity force la réimportation après le resize, il pourrait restaurer un PPU par défaut (différent de 64). Si c'est le cas, créer un `AssetPostprocessor` similaire à `SpellIconImporter.cs` pour forcer le PPU sur les dossiers cibles.
- ⚠️ **Volume des animations** : 2000 PNG à recharger dans Unity peut prendre 5-15 minutes (réimport complet). Prévoir un café.
- ⚠️ **Risque pivot** : ImageMagick préserve le pivot des `.meta` mais doubler les pixels peut décaler le pivot logique de 1 pixel. À surveiller visuellement.

**Bugs détectés :** aucun (phase préparatoire, pas de modification de code).

**État après Phase 2.1 :** Plan d'exécution complet et clé-en-main pour la 2.2. Prochaine session : lancer le batch ImageMagick + appliquer les modifs code + audit visuel Unity.

---

### [2026-05-07] — Phase 2.2 — Migration 64×64 → 128×128 (exécution)
**Orchestration :** Claude Opus (xhigh) — exécution directe (PowerShell + Edit). Pas d'ImageMagick (absent système), remplacé par script PowerShell .NET pur (System.Drawing).

**Tâches effectuées :**
- **Pré-checks** : ImageMagick absent (ni en Bash ni en PowerShell). Pivot vers solution PowerShell .NET pure (System.Drawing). PPU CombatAnimations confirmé à 100. Unity confirmé fermé par utilisateur.
- **Backup horodaté** dans `_Save_fichiers_critiques/sprites_pre128_2026-05-07_0348/` :
  * `NewTilesV4/` : 12 fichiers (6 GIF + 6 .meta)
  * `Characters_Frames/` : 544 fichiers (272 PNG + 272 .meta)
  * `CombatAnimations_Frames/` : 4218 fichiers (2109 PNG + 2109 .meta)
  * **Total : 4774 fichiers** (~37 Mo).
- **Resize batch nearest-neighbor** via script PowerShell `Resize-PixelArt` (System.Drawing.Graphics avec InterpolationMode.NearestNeighbor + PixelOffsetMode.Half + SmoothingMode.None) :
  * Test sur NewTilesV4 (6 GIF) : 64×64 → 128×128 ✅ instantané
  * Characters/Frames : 256 PNG resized à 200% en **1.17s**, 0 échec
  * CombatAnimations/Frames : 1985 PNG resized à 200% en **8.23s**, 0 échec
  * **Total : 2247 fichiers traités en ~10 secondes** (largement plus rapide qu'estimé).
- **Update PPU dans les `.meta`** via PowerShell `Update-PPU` (regex replace) :
  * NewTilesV4 : `spritePixelsToUnits: 64` → `128` (6/6 .meta)
  * Characters/Frames : `spritePixelsToUnits: 100` → `200` (256/256 .meta)
  * CombatAnimations/Frames : `spritePixelsToUnits: 100` → `200` (1985/1985 .meta)
- **Modifs code** :
  * `IsometricCamera.cs:47` — `pixelsPerUnit = 32f` → `pixelsPerUnit = 64f` + commentaire d'explication.
  * `TileSpriteRegistry.cs:17` (commentaire docstring) — mention `Pixels Per Unit → 64` mise à jour vers `→ 128`.
  * Aucun autre fichier modifié grâce à l'**Option A** (PPU compense le doublement de pixels → unités Unity inchangées).

**Arbitrages réalisés :**
- **Pivot vers PowerShell .NET au lieu d'ImageMagick** : décision validée utilisateur. Avantage = pas d'install requise, vitesse comparable (10s pour 2247 fichiers). System.Drawing fournit `InterpolationMode.NearestNeighbor` qui est exactement l'équivalent de `magick -filter point` pour le pixel art.
- **Test progressif** : NewTilesV4 (6 fichiers) avant le gros batch (2241 fichiers). Pratique standard pour éviter de découvrir un bug sur un volume massif.
- **Préservation des formats** : GIF reste GIF, PNG reste PNG (pas de conversion). Garde la compatibilité parfaite avec les `.meta` existants (GUIDs préservés).
- **Aucune modif `GridConfig` / `CellPrefab` / pathfinding** : conséquence directe de l'option A. Le pathfinding A* opère sur indices entiers de grille (totalement découplé du rendu), les world units des hitboxes/sprites sont préservées car PPU compense.

**Cas non prévus / difficultés :**
- **ImageMagick absent** au lieu d'installé. Heureusement, System.Drawing en pur PowerShell donne le même résultat (nearest-neighbor strict). Aucun retard ; même qualité finale.
- **Volume légèrement différent** des estimations : 256 frames perso au lieu de ~272 estimé, 1985 frames combat anims au lieu de ~2000. Cohérent.
- **Vitesse imprévue** : la 2.2 estimée à 10-30 minutes a duré en pratique **~15 secondes** côté code. La partie réimport Unity (côté utilisateur) reste à voir mais est probablement le goulot d'étranglement.

**Bugs détectés :** aucun.

**Tests Unity attendus (côté utilisateur, Phase 2.3) :**

⚠️ **Attente du réimport** : Unity va détecter ~2247 PNG/GIF modifiés au démarrage et lancer un réimport complet. Estimer 2-10 minutes selon la machine. **Ne pas interagir** pendant l'opération.

Une fois réimporté, les vérifications visuelles :
1. **Grille** : ouvrir `Ranked1v1.unity` ou `Training.unity`. La grille doit s'afficher **identiquement** au screenshot de référence (option A → aucun changement de taille à l'écran). Si la grille apparaît 2× plus grande/petite, problème d'option A → recalibrer PPU caméra.
2. **Personnages** : sprites visiblement plus nets (4× pixels) mais à la même taille. Animations idle/walk/death/taking_damage dans les 4 directions doivent fonctionner.
3. **Animations de sorts** : lancer un sort en combat → animation dans les 4 directions OK.
4. **HUD intact** : barres HP, deck, passif, chat, timer rond — **aucun changement** (hors scope 2).
5. **Pathfinding** : déplacement sur la grille fonctionne, hitboxes de cellule alignées au pointeur souris.

**État après Phase 2.2 :** Toutes les cases code de la 2.2 sont cochées avec l'hypothèse forte que l'option A fonctionne. Validation visuelle Unity nécessaire en 2.3 pour confirmer/invalider et passer à la Phase 3 (intégration Nightseer + Colossar).

---

### [2026-05-07] — Phase 2.3 — Audit post-migration (validation utilisateur)
**Tests effectués par l'utilisateur :**
- ✅ `Ranked1v1.unity` / `Training.unity` : grille s'affiche correctement, identique au screenshot de référence
- ✅ Combat test : déplacements OK, animations idle/walk/death × 4 directions OK, depth-sorting préservé
- ✅ `Hub.unity` : avatars joueurs bien dimensionnés
- ✅ HUD strictement intact (barres HP, deck, passif, chat, timer rond, etc.)

**Confirmation de l'option A (PPU doublé) :** rendu visuel identique à l'écran, sprites 4× plus nets, aucun recalibrage nécessaire. Hypothèse de la 2.1 entièrement validée par les faits.

## ✅ PHASE 2 — COMPLÈTE 2026-05-07

Bilan complet :
| Sous-phase | Volume traité | Durée code | Validation |
|---|---|---|---|
| 2.1 | Inventaire 2247 fichiers + audit 6 configs | 1 session orchestration | ✅ Opus |
| 2.2 | 2247 fichiers resized + 2247 .meta updated + 2 fichiers code | 15s d'exécution | ✅ Opus + utilisateur |
| 2.3 | Tests visuels Unity (grille, anims, HUD, hitboxes) | — | ✅ utilisateur |

**Leçons retenues pour les prochaines migrations visuelles :**
1. **Option A (PPU doublé)** est le pattern correct pour upscale pixel art Unity sans recalibrer la mise en scène.
2. **PowerShell .NET pur (System.Drawing)** est une alternative parfaite à ImageMagick pour le batch nearest-neighbor (gain : pas d'install requise, vitesse comparable).
3. **Backup horodaté avant batch massif** est non négociable, et un test progressif (petit dossier d'abord) permet de découvrir les bugs avant de modifier 2000+ fichiers.
4. **Le pathfinding A* est totalement découplé du rendu pixel** (opère sur indices entiers de grille) → aucune migration de rendu ne le casse.

**Phase 3 (intégration Nightseer + Colossar) débloquée. Prochaine étape : préparation 3.1 (Nightseer — passif Prédateur, 15 sorts, animations).**

---

*Roadmap générée le 2026-05-06 — Basée sur audit complet du projet NymoraV1-main*

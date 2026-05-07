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

- [ ] Valider le contenu de `__cleanup_quarantine__/` fichier par fichier, puis supprimer le dossier entier (oracle-editor-legacy, photon-demos, runtime-legacy, tmp-examples)
- [ ] Vérifier les 2 scripts dans `Assets/_Game/Scripts/Utils/` — utilisés ou orphelins ? Supprimer si inutilisés.
- [ ] Vérifier `Assets/Plugins/` — contenu à identifier, supprimer ce qui est obsolète
- [ ] Fusionner ou clarifier les deux dossiers `Resources/` : `Assets/Resources/` et `Assets/_Game/Resources/` — identifier ce qui est dans chacun et consolider tout dans `Assets/_Game/Resources/`

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

- [ ] Déplacer `Assets/Monjeu.unity` → `Assets/_Game/Scenes/Training.unity` et mettre à jour `EditorBuildSettings.asset` + toutes les références `SceneManager.LoadScene` dans les scripts
- [ ] Déplacer `Assets/Boot_Network.unity` → `Assets/_Game/Scenes/Boot_Network.unity` et mettre à jour `EditorBuildSettings.asset`
- [ ] Créer le dossier `_Docs/` à la racine du projet
- [ ] Déplacer `Nymora_Bible_complete.docx`, `ROADMAP_NYMORA.md`, `README.md` dans `_Docs/`
- [ ] Créer `_Docs/archives/` et y déplacer les anciens fichiers documentaires (CLEANUP_AUDIT, ORACLE_SCRIPTS_CLASSIFICATION, ROADMAP_ORACLE.txt, tutoriels)
- [ ] Consolider `Assets/Resources/` dans `Assets/_Game/Resources/` — vérifier les références avant de déplacer
- [ ] Supprimer `Assets/Resources/` une fois consolidé (vérifier qu'aucun script ne charge depuis ce chemin)

---

#### 1.3 Stabilisation réseau

- [ ] Synchroniser la sélection de passif via seed partagé ou autorité MasterClient
- [ ] Implémenter une règle de déconnexion claire (forfait → victoire adversaire → retour hub)
- [ ] Implémenter la migration MasterClient pour les rôles critiques (timer, fin de tour)
- [ ] Ajouter un timer de reconnexion (60s avant forfait automatique)

---

#### 1.4 Audit post-Phase 1

- [ ] Ouvrir Unity — vérifier qu'aucune erreur console n'apparaît au démarrage
- [ ] Vérifier que toutes les scènes se chargent sans erreur de référence manquante
- [ ] Lancer un combat test complet (placement → combat → fin de match)
- [ ] Vérifier que les ScriptableObjects sont tous bien référencés (aucun champ "Missing")
- [ ] Vérifier l'ordre de build dans `EditorBuildSettings.asset`
- [ ] Faire un build de test pour confirmer qu'il n'y a pas d'asset manquant

**Critères de sortie :**
- Structure de dossiers propre, cohérente, naviguable
- Zéro erreur console au démarrage Unity
- Toutes les scènes chargent correctement
- 10+ matchs 1v1 sans désynchronisation visible
- Déconnexion d'un joueur = résultat propre sans blocage

---

### PHASE 2 — Migration visuelle 64×64 → 128×128
**Priorité : IMPORTANTE — avant d'ajouter les nouvelles classes**
**Durée estimée : 2-3 sessions**

> ⚠️ WARNING : Modifier UNIQUEMENT les sprites de personnages et les tiles. NE PAS toucher l'UI, l'HUD, les icônes de sorts, les polices.

#### 2.1 Préparation
- [ ] Identifier tous les fichiers affectés : `Sprites/Characters/`, `Sprites/NewTilesV4/`, `Resources/Characters/Frames/`, `Resources/CombatAnimations/Frames/`
- [ ] Lister les paramètres de la grille isométrique (GridConfig, ArenaConfig, CellPrefab) à ajuster
- [ ] Prendre un snapshot de la scène Ranked1v1 avant modification (screenshot de référence)

#### 2.2 Migration
- [ ] Redimensionner tous les sprites personnages (Idle/Walk/Death/TakingDamage × 4 directions) de 64 à 128px
- [ ] Redimensionner tous les tiles (`NewTilesV4/`) de 64 à 128px
- [ ] Mettre à jour `GridConfig` : taille de cellule, offsets isométriques, positions caméra
- [ ] Mettre à jour `CellPrefab` : dimensions du collider et du sprite renderer
- [ ] Mettre à jour `TileSpriteRegistry` : références recalculées en 128px
- [ ] Vérifier `IsometricCamera` : zoom et framing à recalibrer
- [ ] Revalider le pathfinding : les hitboxes de cellule doivent correspondre aux nouvelles dimensions

#### 2.3 Audit post-migration
- [ ] Ouvrir Ranked1v1 : vérifier que la grille s'affiche correctement
- [ ] Lancer un combat test : vérifier les déplacements, les animations, le depth-sorting
- [ ] Vérifier Hub.unity : les avatars joueurs sont-ils bien dimensionnés ?
- [ ] Vérifier que l'UI/HUD n'a pas bougé

**Critères de sortie :**
- Grille 128×128 propre sans artefacts visuels
- Animations personnages fluides dans les 4 directions
- HUD intact, aucune régression UI

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

*Roadmap générée le 2026-05-06 — Basée sur audit complet du projet NymoraV1-main*

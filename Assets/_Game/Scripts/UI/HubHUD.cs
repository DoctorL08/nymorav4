using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD du hub : chat global, barre de navigation, modal d'arène et deck builder.
/// Se construit entièrement en runtime depuis Awake (même pattern que CombatChatUI).
/// Créer la scène Hub via Oracle > Create Hub Scene.
/// </summary>
public class HubHUD : MonoBehaviour
{
    // ── Palette Oracle ──────────────────────────────────────────────────
    static readonly Color BgDark      = new Color(0.05f, 0.05f, 0.08f, 0.88f);
    static readonly Color BgMedium    = new Color(0.10f, 0.10f, 0.14f, 0.95f);
    static readonly Color BgField     = new Color(0.14f, 0.14f, 0.18f, 1.00f);
    static readonly Color AccentGold  = new Color(0.788f, 0.659f, 0.298f, 1f);
    static readonly Color AccentGoldH = new Color(0.88f,  0.75f,  0.38f, 1f);
    static readonly Color AccentGoldP = new Color(0.60f,  0.50f,  0.22f, 1f);
    static readonly Color TextWhite   = Color.white;
    static readonly Color TextGray    = new Color(0.78f, 0.78f, 0.78f, 1f);
    static readonly Color TextDim     = new Color(0.42f, 0.42f, 0.45f, 1f);
    static readonly Color Overlay     = new Color(0f, 0f, 0f, 0.65f);
    static readonly Color BtnDisabled = new Color(0.22f, 0.22f, 0.26f, 1f);
    static readonly Color TextDisabled= new Color(0.45f, 0.45f, 0.48f, 1f);
    static readonly Color NavBtnNorm  = new Color(0.12f, 0.12f, 0.17f, 0.95f);
    static readonly Color NavBtnHov   = new Color(0.18f, 0.18f, 0.24f, 1f);
    static readonly Color NavBtnPrs   = new Color(0.07f, 0.07f, 0.10f, 1f);

    // ── Palette deck builder ─────────────────────────────────────────────
    static readonly Color CardBg       = new Color(0.12f, 0.12f, 0.17f, 1f);
    static readonly Color CardBgSel    = new Color(0.20f, 0.16f, 0.07f, 1f);
    static readonly Color CardBgDim    = new Color(0.08f, 0.08f, 0.10f, 0.65f);
    static readonly Color TabActive    = new Color(0.22f, 0.18f, 0.08f, 1f);
    static readonly Color TabNorm      = new Color(0.10f, 0.10f, 0.14f, 1f);
    // catégories
    static readonly Color CatAtk  = new Color(0.75f, 0.25f, 0.20f, 1f);
    static readonly Color CatTac  = new Color(0.25f, 0.45f, 0.75f, 1f);
    static readonly Color CatSur  = new Color(0.25f, 0.65f, 0.30f, 1f);

    // ── Paramètres chat ─────────────────────────────────────────────────
    [Header("Chat")]
    public float chatWidth  = 320f;
    public float chatHeight = 210f;
    public int   maxMessages = 60;

    RectTransform  _chatContent;
    ScrollRect     _chatScroll;
    TMP_InputField _chatInput;
    readonly List<GameObject> _chatMessages = new();

    // ── Modal arène ─────────────────────────────────────────────────────
    GameObject      _arenaModal;
    GameObject      _searchingPanel;
    TextMeshProUGUI _searchingLabel;
    Button          _cancelSearchBtn;

    // ── Deck builder ─────────────────────────────────────────────────────
    GameObject        _deckModal;
    RectTransform     _deckScrollContent;
    TextMeshProUGUI   _deckCounter;

    SpellDeckCategory _currentTab     = SpellDeckCategory.Attack;
    readonly List<SpellData> _allSpells      = new();
    readonly List<SpellData> _selectedSpells = new();

    // Références aux tuiles icônes visibles (onglet courant)
    struct SpellCardView
    {
        public SpellData  spell;
        public Image      bg;
        public Image      iconImg;   // image de l'icône PixelLab
        public GameObject selMark;
    }
    readonly List<SpellCardView> _currentCards = new();

    // ── Barre de deck (6 slots au bas du builder) ─────────────────────────
    const  float   DeckBarH       = 92f;
    readonly Image[] _deckBarIcons = new Image[DeckData.MaxSpells];
    GameObject       _deckBarRoot;

    // (tooltip inline supprimé — descriptions visibles directement sur les cartes)

    // Références aux boutons d'onglet (0=Attack,1=Tactic,2=Survival)
    static readonly (SpellDeckCategory cat, string label, Color color)[] Tabs =
    {
        (SpellDeckCategory.Attack,   "Attaques", CatAtk),
        (SpellDeckCategory.Tactic,   "Tactique", CatTac),
        (SpellDeckCategory.Survival, "Survie",   CatSur),
    };
    readonly Image[] _tabImages = new Image[3];

    // ── Références show/hide entre vue classe et vue sorts ───────────────
    GameObject _sepGo;
    GameObject _tabBarGo;
    GameObject _scrollGoRef;
    GameObject _footerGo;

    // ── Sélection de classe ──────────────────────────────────────────────
    NymoraClassRegistry _classRegistry;
    ClassData           _selectedClass;
    GameObject          _classPanelGo;
    readonly Image[]    _classCardBgs = new Image[5];

    // Caméra isométrique — pour bloquer le zoom quand le deck builder est ouvert
    IsometricCamera _isoCam;

    // Label PA/PM (mis à jour à l'ouverture du deck builder)
    TextMeshProUGUI _statsLabel;

    // ── Lifecycle ───────────────────────────────────────────────────────
    void Awake() => BuildAll();

    void BuildAll()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[HubHUD] Aucun Canvas dans la scène — HUD non construit.");
            return;
        }

        BuildChatPanel(canvas);
        BuildNavBar(canvas);
        BuildArenaModal(canvas);
        BuildDeckModal(canvas);
    }

    // ════════════════════════════════════════════════════════════════════
    // CHAT PANEL  — bas gauche
    // ════════════════════════════════════════════════════════════════════

    void BuildChatPanel(Canvas canvas)
    {
        const float INPUT_H  = 28f;
        const float SEND_W   = 58f;
        const float PAD      = 4f;
        const float OFFSET_X = 12f;
        const float OFFSET_Y = 60f;   // au-dessus de la barre de navigation

        var root   = MakeChild(canvas.gameObject, "HubChatPanel");
        var rootRt = RT(root);
        rootRt.anchorMin = rootRt.anchorMax = Vector2.zero;
        rootRt.pivot     = Vector2.zero;
        rootRt.sizeDelta = new Vector2(chatWidth, chatHeight);
        rootRt.anchoredPosition = new Vector2(OFFSET_X, OFFSET_Y);
        root.AddComponent<Image>().color = BgDark;

        // ── Zone scrollable ──
        var scrollGo = MakeChild(root, "Scroll");
        var scrollRt = RT(scrollGo);
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(0f, INPUT_H);
        scrollRt.offsetMax = Vector2.zero;

        _chatScroll = scrollGo.AddComponent<ScrollRect>();
        _chatScroll.horizontal        = false;
        _chatScroll.vertical          = true;
        _chatScroll.movementType      = ScrollRect.MovementType.Clamped;
        _chatScroll.scrollSensitivity = 20f;
        _chatScroll.verticalScrollbar = null;

        var vpGo = MakeChild(scrollGo, "Viewport");
        Stretch(RT(vpGo));
        vpGo.AddComponent<RectMask2D>();
        _chatScroll.viewport = RT(vpGo);

        var contentGo = MakeChild(vpGo, "Content");
        _chatContent = RT(contentGo);
        _chatContent.anchorMin        = new Vector2(0f, 1f);
        _chatContent.anchorMax        = new Vector2(1f, 1f);
        _chatContent.pivot            = new Vector2(0.5f, 1f);
        _chatContent.sizeDelta        = Vector2.zero;
        _chatContent.anchoredPosition = Vector2.zero;

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(6, 6, 4, 4);
        contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _chatScroll.content = _chatContent;

        // ── Ligne d'input ──
        var rowGo = MakeChild(root, "InputRow");
        var rowRt = RT(rowGo);
        rowRt.anchorMin = Vector2.zero;
        rowRt.anchorMax = new Vector2(1f, 0f);
        rowRt.pivot     = Vector2.zero;
        rowRt.sizeDelta = new Vector2(0f, INPUT_H);
        rowRt.anchoredPosition = Vector2.zero;
        rowGo.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 1f);

        var ifGo = MakeChild(rowGo, "Field");
        var ifRt = RT(ifGo);
        ifRt.anchorMin = Vector2.zero; ifRt.anchorMax = Vector2.one;
        ifRt.offsetMin = new Vector2(PAD, 3f);
        ifRt.offsetMax = new Vector2(-(SEND_W + PAD + 2f), -3f);
        ifGo.AddComponent<Image>().color = BgField;

        _chatInput = ifGo.AddComponent<TMP_InputField>();
        _chatInput.lineType = TMP_InputField.LineType.SingleLine;

        var taGo = MakeChild(ifGo, "TextArea");
        var taRt = RT(taGo);
        taRt.anchorMin = Vector2.zero; taRt.anchorMax = Vector2.one;
        taRt.offsetMin = new Vector2(5f, 1f); taRt.offsetMax = new Vector2(-5f, -1f);
        taGo.AddComponent<RectMask2D>();
        _chatInput.textViewport = taRt;

        var phTmp = MakeLabel(MakeChild(taGo, "Placeholder"), "Envoyer un message…", 10f, TextDim);
        phTmp.fontStyle          = FontStyles.Italic;
        phTmp.enableWordWrapping = false;
        phTmp.raycastTarget      = false;

        var txtTmp = MakeLabel(MakeChild(taGo, "Text"), string.Empty, 10f, TextWhite);
        txtTmp.enableWordWrapping = false;
        txtTmp.raycastTarget      = false;

        _chatInput.textComponent = txtTmp;
        _chatInput.placeholder   = phTmp;
        _chatInput.onSubmit.AddListener(SubmitChat);

        var sendGo  = MakeChild(rowGo, "Send");
        var sendRt  = RT(sendGo);
        sendRt.anchorMin = new Vector2(1f, 0f); sendRt.anchorMax = new Vector2(1f, 1f);
        sendRt.pivot     = new Vector2(1f, 0.5f);
        sendRt.sizeDelta = new Vector2(SEND_W, -6f);
        sendRt.anchoredPosition = new Vector2(-PAD, 0f);
        var sendImg = sendGo.AddComponent<Image>(); sendImg.color = AccentGold;
        var sendBtn = sendGo.AddComponent<Button>(); sendBtn.targetGraphic = sendImg;
        ApplyColors(sendBtn, AccentGold, AccentGoldH, AccentGoldP);
        sendBtn.onClick.AddListener(OnSendClicked);

        var sendLbl = MakeLabel(MakeChild(sendGo, "Lbl"), "Envoyer", 8.5f, new Color(0.10f, 0.07f, 0.03f, 1f));
        Stretch(RT(sendLbl.gameObject));

        AppendChat("Bienvenue dans le hub Oracle !", AccentGold);
    }

    void OnSendClicked() => SubmitChat(_chatInput != null ? _chatInput.text : string.Empty);

    void SubmitChat(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        string msg = text.Trim();
        if (_chatInput != null) _chatInput.text = string.Empty;
        AppendChat($"<b>Vous</b> : {msg}", TextWhite);

        // Afficher la bulle au-dessus du personnage
        FindObjectOfType<HubChatBubble>()?.ShowMessage(msg);
    }

    void AppendChat(string text, Color color)
    {
        if (_chatContent == null) return;

        if (_chatMessages.Count >= maxMessages)
        {
            Destroy(_chatMessages[0]);
            _chatMessages.RemoveAt(0);
        }

        var go  = MakeChild(_chatContent.gameObject, "Msg");
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text               = text;
        tmp.fontSize           = 11f;
        tmp.color              = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(tmp);

        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;

        _chatMessages.Add(go);
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (_chatScroll != null)
            _chatScroll.verticalNormalizedPosition = 0f;
    }

    // ════════════════════════════════════════════════════════════════════
    // BARRE DE NAVIGATION  — bas droite (5 boutons : +Deck)
    // ════════════════════════════════════════════════════════════════════

    void BuildNavBar(Canvas canvas)
    {
        const float BAR_H    = 50f;
        const float BTN_W    = 110f;
        const float BTN_H    = 36f;
        const float SPACING  = 6f;
        const float PAD_H    = 10f;
        const float PAD_V    = 7f;
        const float OFFSET_X = -10f;
        const float OFFSET_Y = 8f;

        int   btnCount = 5;
        float totalW   = btnCount * BTN_W + (btnCount - 1) * SPACING + 2f * PAD_H;

        var bar   = MakeChild(canvas.gameObject, "HubNavBar");
        var barRt = RT(bar);
        barRt.anchorMin = barRt.anchorMax = new Vector2(1f, 0f);
        barRt.pivot     = new Vector2(1f, 0f);
        barRt.sizeDelta = new Vector2(totalW, BAR_H);
        barRt.anchoredPosition = new Vector2(OFFSET_X, OFFSET_Y);
        bar.AddComponent<Image>().color = BgDark;

        var hlg = bar.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment      = TextAnchor.MiddleCenter;
        hlg.spacing             = SPACING;
        hlg.padding             = new RectOffset((int)PAD_H, (int)PAD_H, (int)PAD_V, (int)PAD_V);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        MakeNavBtn(bar, "Classement", BTN_W, BTN_H, false,
            () => AppendChat("Classement — bientôt disponible.", TextGray));
        MakeNavBtn(bar, "Boutique",   BTN_W, BTN_H, false,
            () => AppendChat("Boutique — bientôt disponible.", TextGray));
        MakeNavBtn(bar, "Paramètres", BTN_W, BTN_H, false,
            () => AppendChat("Paramètres — bientôt disponible.", TextGray));
        MakeNavBtn(bar, "Deck",       BTN_W, BTN_H, false, OpenDeckModal);
        MakeNavBtn(bar, "Arène",      BTN_W, BTN_H, true,  OpenArenaMenu);
    }

    void MakeNavBtn(GameObject parent, string label, float w, float h, bool isAccent, System.Action onClick)
    {
        var go  = MakeChild(parent, label + "Btn");
        var le  = go.AddComponent<LayoutElement>();
        le.preferredWidth  = w;
        le.preferredHeight = h;

        var img = go.AddComponent<Image>();
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        if (isAccent)
        {
            img.color = AccentGold;
            ApplyColors(btn, AccentGold, AccentGoldH, AccentGoldP);
            var lbl = MakeLabel(MakeChild(go, "Lbl"), label, 13f, new Color(0.10f, 0.07f, 0.03f, 1f));
            lbl.fontStyle = FontStyles.Bold;
            Stretch(RT(lbl.gameObject));
        }
        else
        {
            img.color = NavBtnNorm;
            ApplyColors(btn, NavBtnNorm, NavBtnHov, NavBtnPrs);
            var lbl = MakeLabel(MakeChild(go, "Lbl"), label, 12f, AccentGold);
            Stretch(RT(lbl.gameObject));
        }

        btn.onClick.AddListener(() => onClick());
    }

    // ════════════════════════════════════════════════════════════════════
    // MODAL ARÈNE  — centré, overlay semi-transparent
    // ════════════════════════════════════════════════════════════════════

    void BuildArenaModal(Canvas canvas)
    {
        const float PANEL_W = 440f;
        const float PANEL_H = 290f;

        _arenaModal = MakeChild(canvas.gameObject, "ArenaModal");
        Stretch(RT(_arenaModal));
        var overlayImg = _arenaModal.AddComponent<Image>();
        overlayImg.color = Overlay;
        var overlayBtn = _arenaModal.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        overlayBtn.onClick.AddListener(CloseArenaMenu);
        _arenaModal.SetActive(false);

        var panel   = MakeChild(_arenaModal, "Panel");
        var panelRt = RT(panel);
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot     = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(PANEL_W, PANEL_H);
        panelRt.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = BgMedium;
        panel.AddComponent<GraphicRaycaster>();

        var titleArea = MakeChild(panel, "TitleArea");
        var titleRt   = RT(titleArea);
        titleRt.anchorMin = new Vector2(0f, 1f); titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot     = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0f, 52f);
        titleRt.anchoredPosition = Vector2.zero;

        var titleTmp = MakeLabel(MakeChild(titleArea, "Title"), "ARÈNE", 22f, AccentGold);
        titleTmp.fontStyle = FontStyles.Bold;
        Stretch(RT(titleTmp.gameObject));

        var sep   = MakeChild(panel, "TitleSep");
        var sepRt = RT(sep);
        sepRt.anchorMin = new Vector2(0f, 1f); sepRt.anchorMax = new Vector2(1f, 1f);
        sepRt.pivot     = new Vector2(0.5f, 1f);
        sepRt.sizeDelta = new Vector2(-24f, 1f);
        sepRt.anchoredPosition = new Vector2(0f, -50f);
        sep.AddComponent<Image>().color = AccentGold;

        var grid   = MakeChild(panel, "ModeGrid");
        var gridRt = RT(grid);
        gridRt.anchorMin = new Vector2(0.05f, 0.12f);
        gridRt.anchorMax = new Vector2(0.95f, 0.76f);
        gridRt.offsetMin = gridRt.offsetMax = Vector2.zero;

        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize       = new Vector2(180f, 68f);
        glg.spacing        = new Vector2(12f, 10f);
        glg.startCorner    = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis      = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment = TextAnchor.MiddleCenter;

        BuildArenaMode(grid, "Entraînement", active: true,  () => OnTrainingSelected());
        BuildArenaMode(grid, "1 VS 1",       active: true,  () => OnVS1Selected());
        BuildArenaMode(grid, "2 VS 2",       active: false, null);
        BuildArenaMode(grid, "3 VS 3",       active: false, null);

        var closeGo  = MakeChild(panel, "CloseBtn");
        var closeRt  = RT(closeGo);
        closeRt.anchorMin = new Vector2(1f, 0f); closeRt.anchorMax = new Vector2(1f, 0f);
        closeRt.pivot     = new Vector2(1f, 0f);
        closeRt.sizeDelta = new Vector2(95f, 34f);
        closeRt.anchoredPosition = new Vector2(-14f, 12f);

        var closeImg = closeGo.AddComponent<Image>(); closeImg.color = BgDark;
        var closeBtn = closeGo.AddComponent<Button>(); closeBtn.targetGraphic = closeImg;
        ApplyColors(closeBtn, BgDark, NavBtnHov, NavBtnPrs);
        closeBtn.onClick.AddListener(CloseArenaMenu);
        var closeLbl = MakeLabel(MakeChild(closeGo, "Lbl"), "Fermer", 12f, TextGray);
        Stretch(RT(closeLbl.gameObject));

        // ── Panel "Recherche en cours" (superposé, caché par défaut) ─────
        BuildSearchingPanel(panel);
    }

    void BuildSearchingPanel(GameObject parent)
    {
        _searchingPanel = MakeChild(parent, "SearchingPanel");
        var spRt = RT(_searchingPanel);
        spRt.anchorMin = Vector2.zero; spRt.anchorMax = Vector2.one;
        spRt.offsetMin = spRt.offsetMax = Vector2.zero;
        _searchingPanel.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.09f, 0.96f);
        _searchingPanel.SetActive(false);

        // Label "Recherche..."
        _searchingLabel = MakeLabel(MakeChild(_searchingPanel, "SearchLbl"),
            "Recherche d'un adversaire…\n<size=28><color=#C8A84B>0 / 2</color></size>",
            18f, TextWhite);
        _searchingLabel.enableWordWrapping = true;
        var slRt = RT(_searchingLabel.gameObject);
        slRt.anchorMin = new Vector2(0.1f, 0.45f); slRt.anchorMax = new Vector2(0.9f, 0.85f);
        slRt.offsetMin = slRt.offsetMax = Vector2.zero;

        // Bouton Annuler
        var cancelGo = MakeChild(_searchingPanel, "CancelBtn");
        var cancelRt = RT(cancelGo);
        cancelRt.anchorMin = new Vector2(0.3f, 0.2f); cancelRt.anchorMax = new Vector2(0.7f, 0.38f);
        cancelRt.offsetMin = cancelRt.offsetMax = Vector2.zero;
        var cancelImg = cancelGo.AddComponent<Image>(); cancelImg.color = BtnDisabled;
        _cancelSearchBtn = cancelGo.AddComponent<Button>(); _cancelSearchBtn.targetGraphic = cancelImg;
        ApplyColors(_cancelSearchBtn, BtnDisabled, NavBtnHov, NavBtnPrs);
        _cancelSearchBtn.onClick.AddListener(OnCancelSearchClicked);
        var cancelLbl = MakeLabel(MakeChild(cancelGo, "Lbl"), "Annuler", 14f, TextGray);
        Stretch(RT(cancelLbl.gameObject));
    }

    void BuildArenaMode(GameObject parent, string label, bool active, System.Action onClick)
    {
        var go  = MakeChild(parent, label.Replace(" ", "") + "Mode");
        var img = go.AddComponent<Image>();
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable  = active;

        if (active)
        {
            img.color = BgDark;
            ApplyColors(btn, BgDark, new Color(0.14f, 0.14f, 0.20f, 1f), new Color(0.03f, 0.03f, 0.05f, 1f));
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var lbl = MakeLabel(MakeChild(go, "Lbl"), label, 14f, AccentGold);
            lbl.fontStyle = FontStyles.Bold;
            Stretch(RT(lbl.gameObject));

            var badge   = MakeChild(go, "Badge");
            var badgeRt = RT(badge);
            badgeRt.anchorMin = new Vector2(0f, 0f); badgeRt.anchorMax = new Vector2(0f, 0f);
            badgeRt.pivot     = new Vector2(0f, 0f);
            badgeRt.sizeDelta = new Vector2(60f, 14f);
            badgeRt.anchoredPosition = new Vector2(6f, 6f);
            badge.AddComponent<Image>().color = new Color(0.20f, 0.58f, 0.25f, 0.85f);
            var badgeTmp = MakeLabel(MakeChild(badge, "T"), "disponible", 8f, TextWhite);
            Stretch(RT(badgeTmp.gameObject));
        }
        else
        {
            img.color = BtnDisabled;
            var cs = btn.colors;
            cs.disabledColor = new Color(0.35f, 0.35f, 0.38f, 0.6f);
            btn.colors = cs;

            var lbl = MakeLabel(MakeChild(go, "Lbl"), label, 14f, TextDisabled);
            lbl.fontStyle = FontStyles.Italic;
            Stretch(RT(lbl.gameObject));

            var badge   = MakeChild(go, "Badge");
            var badgeRt = RT(badge);
            badgeRt.anchorMin = new Vector2(0f, 0f); badgeRt.anchorMax = new Vector2(0f, 0f);
            badgeRt.pivot     = new Vector2(0f, 0f);
            badgeRt.sizeDelta = new Vector2(50f, 14f);
            badgeRt.anchoredPosition = new Vector2(6f, 6f);
            badge.AddComponent<Image>().color = new Color(0.35f, 0.30f, 0.20f, 0.70f);
            var badgeTmp = MakeLabel(MakeChild(badge, "T"), "à venir", 8f, new Color(0.65f, 0.55f, 0.25f, 1f));
            Stretch(RT(badgeTmp.gameObject));
        }
    }

    void OpenArenaMenu()
    {
        if (_arenaModal != null) _arenaModal.SetActive(true);
    }

    void CloseArenaMenu()
    {
        if (_arenaModal != null) _arenaModal.SetActive(false);
    }

    void OnTrainingSelected()
    {
        CloseArenaMenu();
        if (HubManager.Instance != null)
            HubManager.Instance.LaunchTraining();
        else
        {
            Debug.LogWarning("[HubHUD] HubManager introuvable — chargement direct par nom.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Training");
        }
    }

    void OnVS1Selected()
    {
        var mm = HubMatchmaker.Instance;
        if (mm == null)
        {
            AppendChat("HubMatchmaker introuvable — ajoute-le sur NetworkRoot.", new Color(1f, 0.4f, 0.4f));
            return;
        }

        // Afficher le panel de recherche
        if (_searchingPanel != null) _searchingPanel.SetActive(true);

        // S'abonner aux events du matchmaker
        mm.OnPlayerCountUpdated += UpdateSearchingLabel;
        mm.OnMatchFound         += OnMatchFound;
        mm.OnSearchCancelled    += OnSearchCancelled;

        mm.StartSearch1v1();
        AppendChat("Recherche 1v1 en cours…", AccentGold);
    }

    void OnCancelSearchClicked()
    {
        HubMatchmaker.Instance?.CancelSearch();
    }

    void UpdateSearchingLabel(int current, int max)
    {
        if (_searchingLabel != null)
            _searchingLabel.text =
                $"Recherche d'un adversaire…\n<size=28><color=#C8A84B>{current} / {max}</color></size>";
    }

    void OnMatchFound()
    {
        if (_searchingPanel != null) _searchingPanel.SetActive(false);
        UnsubscribeMatchmaker();
        AppendChat("Adversaire trouvé ! Chargement du combat…", AccentGold);
    }

    void OnSearchCancelled()
    {
        if (_searchingPanel != null) _searchingPanel.SetActive(false);
        UnsubscribeMatchmaker();
        AppendChat("Recherche annulée.", TextGray);
    }

    void UnsubscribeMatchmaker()
    {
        var mm = HubMatchmaker.Instance;
        if (mm == null) return;
        mm.OnPlayerCountUpdated -= UpdateSearchingLabel;
        mm.OnMatchFound         -= OnMatchFound;
        mm.OnSearchCancelled    -= OnSearchCancelled;
    }

    // ════════════════════════════════════════════════════════════════════
    // DECK BUILDER MODAL  — centré, onglets par catégorie
    // ════════════════════════════════════════════════════════════════════

    void BuildDeckModal(Canvas canvas)
    {
        // Hauteurs fixes (en unités canvas 1920×1080)
        const float HEADER_H = 76f;
        const float TABS_H   = 52f;
        const float FOOTER_H = 68f;
        // DeckBarH est la constante de classe (92f) — barre de sorts sélectionnés

        // ── Overlay ──
        _deckModal = MakeChild(canvas.gameObject, "DeckModal");
        Stretch(RT(_deckModal));
        var olImg = _deckModal.AddComponent<Image>();
        olImg.color = Overlay;
        var olBtn = _deckModal.AddComponent<Button>();
        olBtn.targetGraphic = olImg;
        olBtn.onClick.AddListener(CloseDeckModal);
        _deckModal.SetActive(false);

        // ── Panel — remplit 92 % × 94 % de l'écran (anchor-based) ──
        var panel   = MakeChild(_deckModal, "Panel");
        var panelRt = RT(panel);
        panelRt.anchorMin        = new Vector2(0.04f, 0.03f);
        panelRt.anchorMax        = new Vector2(0.96f, 0.97f);
        panelRt.pivot            = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta        = Vector2.zero;
        panelRt.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = BgMedium;
        panel.AddComponent<GraphicRaycaster>();

        // ── Header ──
        var header   = MakeChild(panel, "Header");
        var headerRt = RT(header);
        headerRt.anchorMin        = new Vector2(0f, 1f);
        headerRt.anchorMax        = new Vector2(1f, 1f);
        headerRt.pivot            = new Vector2(0.5f, 1f);
        headerRt.sizeDelta        = new Vector2(0f, HEADER_H);
        headerRt.anchoredPosition = Vector2.zero;
        header.AddComponent<Image>().color = BgDark;

        var titleTmp = MakeLabel(MakeChild(header, "Title"), "DECK BUILDER", 30f, AccentGold);
        titleTmp.fontStyle = FontStyles.Bold;
        var titleRt = RT(titleTmp.gameObject);
        titleRt.anchorMin = new Vector2(0f, 0f); titleRt.anchorMax = new Vector2(0.48f, 1f);
        titleRt.offsetMin = new Vector2(24f, 0f); titleRt.offsetMax = Vector2.zero;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

        _statsLabel = MakeLabel(MakeChild(header, "Stats"), "— PA / — PM", 20f, TextGray);
        var statsRt = RT(_statsLabel.gameObject);
        statsRt.anchorMin = new Vector2(0.48f, 0f); statsRt.anchorMax = new Vector2(0.74f, 1f);
        statsRt.offsetMin = statsRt.offsetMax = Vector2.zero;
        _statsLabel.alignment = TextAlignmentOptions.Center;

        _deckCounter = MakeLabel(MakeChild(header, "Counter"), "0 / 6 sorts", 20f, TextGray);
        var cntRt = RT(_deckCounter.gameObject);
        cntRt.anchorMin = new Vector2(0.74f, 0f); cntRt.anchorMax = new Vector2(1f, 1f);
        cntRt.offsetMin = Vector2.zero; cntRt.offsetMax = new Vector2(-18f, 0f);
        _deckCounter.alignment = TextAlignmentOptions.MidlineRight;

        // Séparateur doré
        var sep   = MakeChild(panel, "Sep");
        var sepRt = RT(sep);
        sepRt.anchorMin        = new Vector2(0f, 1f);
        sepRt.anchorMax        = new Vector2(1f, 1f);
        sepRt.pivot            = new Vector2(0.5f, 1f);
        sepRt.sizeDelta        = new Vector2(-20f, 2f);
        sepRt.anchoredPosition = new Vector2(0f, -HEADER_H);
        sep.AddComponent<Image>().color = AccentGold;
        _sepGo = sep;

        // ── Onglets ──
        var tabBar   = MakeChild(panel, "TabBar");
        var tabBarRt = RT(tabBar);
        tabBarRt.anchorMin        = new Vector2(0f, 1f);
        tabBarRt.anchorMax        = new Vector2(1f, 1f);
        tabBarRt.pivot            = new Vector2(0.5f, 1f);
        tabBarRt.sizeDelta        = new Vector2(0f, TABS_H);
        tabBarRt.anchoredPosition = new Vector2(0f, -(HEADER_H + 2f));
        tabBar.AddComponent<Image>().color = BgDark;

        var tabHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        tabHlg.childAlignment      = TextAnchor.MiddleLeft;
        tabHlg.spacing             = 4f;
        tabHlg.padding             = new RectOffset(16, 16, 6, 6);
        tabHlg.childForceExpandWidth  = false;
        tabHlg.childForceExpandHeight = true;
        tabHlg.childControlWidth      = false;
        tabHlg.childControlHeight     = true;

        for (int i = 0; i < Tabs.Length; i++)
        {
            var (cat, lbl, col) = Tabs[i];

            var tabGo  = MakeChild(tabBar, lbl + "Tab");
            var tabImg = tabGo.AddComponent<Image>();
            var tabBtn = tabGo.AddComponent<Button>();
            tabBtn.targetGraphic = tabImg;

            var le = tabGo.AddComponent<LayoutElement>();
            le.preferredWidth  = 220f;
            le.preferredHeight = 40f;

            _tabImages[i] = tabImg;
            tabImg.color   = TabNorm;

            var stripe   = MakeChild(tabGo, "Stripe");
            var stripeRt = RT(stripe);
            stripeRt.anchorMin        = new Vector2(0f, 0f);
            stripeRt.anchorMax        = new Vector2(1f, 0f);
            stripeRt.pivot            = new Vector2(0.5f, 0f);
            stripeRt.sizeDelta        = new Vector2(0f, 4f);
            stripeRt.anchoredPosition = Vector2.zero;
            stripe.AddComponent<Image>().color = col;

            var tabLbl = MakeLabel(MakeChild(tabGo, "Lbl"), lbl, 17f, TextGray);
            Stretch(RT(tabLbl.gameObject));

            tabBtn.onClick.AddListener(() => SelectDeckTab(cat));
        }
        _tabBarGo = tabBar;

        // ── Barre de deck (entre onglets et scroll) ──
        BuildDeckBar(panel, HEADER_H, TABS_H);

        // ── Zone de défilement ──
        float scrollTopOffset = HEADER_H + 2f + TABS_H + DeckBarH;

        var scrollGo = MakeChild(panel, "DeckScroll");
        var scrollRt = RT(scrollGo);
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(0f, FOOTER_H);
        scrollRt.offsetMax = new Vector2(0f, -scrollTopOffset);

        var sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.movementType      = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 50f;
        sr.verticalScrollbar = null;

        var vpGo = MakeChild(scrollGo, "Viewport");
        Stretch(RT(vpGo));
        vpGo.AddComponent<RectMask2D>();
        sr.viewport = RT(vpGo);

        var contentGo = MakeChild(vpGo, "Content");
        _deckScrollContent                    = RT(contentGo);
        _deckScrollContent.anchorMin          = new Vector2(0f, 1f);
        _deckScrollContent.anchorMax          = new Vector2(1f, 1f);
        _deckScrollContent.pivot              = new Vector2(0.5f, 1f);
        _deckScrollContent.anchoredPosition   = Vector2.zero;
        _deckScrollContent.sizeDelta          = Vector2.zero;

        // Grille de cartes riches : 190 × 248, 5 colonnes — assez de place pour décrire
        var glg = contentGo.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(190f, 248f);
        glg.spacing         = new Vector2(14f, 14f);
        glg.padding         = new RectOffset(20, 20, 16, 16);
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperLeft;
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;

        contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = _deckScrollContent;
        _scrollGoRef = scrollGo;

        // ── Footer : Sauvegarder + Fermer ──
        var footer   = MakeChild(panel, "Footer");
        var footerRt = RT(footer);
        footerRt.anchorMin        = new Vector2(0f, 0f);
        footerRt.anchorMax        = new Vector2(1f, 0f);
        footerRt.pivot            = new Vector2(0.5f, 0f);
        footerRt.sizeDelta        = new Vector2(0f, FOOTER_H);
        footerRt.anchoredPosition = Vector2.zero;
        footer.AddComponent<Image>().color = BgDark;

        // Bouton "Retour aux classes" (gauche)
        var backGo  = MakeChild(footer, "BackToClassBtn");
        var backRt  = RT(backGo);
        backRt.anchorMin        = new Vector2(0f, 0.5f);
        backRt.anchorMax        = new Vector2(0f, 0.5f);
        backRt.pivot            = new Vector2(0f, 0.5f);
        backRt.sizeDelta        = new Vector2(200f, 44f);
        backRt.anchoredPosition = new Vector2(14f, 0f);
        var backImg = backGo.AddComponent<Image>(); backImg.color = NavBtnNorm;
        var backBtn = backGo.AddComponent<Button>(); backBtn.targetGraphic = backImg;
        ApplyColors(backBtn, NavBtnNorm, NavBtnHov, NavBtnPrs);
        backBtn.onClick.AddListener(ShowClassView);
        var backLbl = MakeLabel(MakeChild(backGo, "Lbl"), "<< Changer de classe", 13f, AccentGold);
        Stretch(RT(backLbl.gameObject));

        var saveGo = MakeChild(footer, "SaveBtn");
        var saveRt = RT(saveGo);
        saveRt.anchorMin        = new Vector2(0.5f, 0.5f);
        saveRt.anchorMax        = new Vector2(0.5f, 0.5f);
        saveRt.pivot            = new Vector2(1f, 0.5f);
        saveRt.sizeDelta        = new Vector2(240f, 44f);
        saveRt.anchoredPosition = new Vector2(-8f, 0f);
        var saveImg = saveGo.AddComponent<Image>(); saveImg.color = AccentGold;
        var saveBtn = saveGo.AddComponent<Button>(); saveBtn.targetGraphic = saveImg;
        ApplyColors(saveBtn, AccentGold, AccentGoldH, AccentGoldP);
        saveBtn.onClick.AddListener(SaveAndCloseDeck);
        var saveLbl = MakeLabel(MakeChild(saveGo, "Lbl"), "Sauvegarder le deck", 16f, new Color(0.10f, 0.07f, 0.03f, 1f));
        saveLbl.fontStyle = FontStyles.Bold;
        Stretch(RT(saveLbl.gameObject));

        var closeGo  = MakeChild(footer, "CloseBtn");
        var closeRt  = RT(closeGo);
        closeRt.anchorMin        = new Vector2(0.5f, 0.5f);
        closeRt.anchorMax        = new Vector2(0.5f, 0.5f);
        closeRt.pivot            = new Vector2(0f, 0.5f);
        closeRt.sizeDelta        = new Vector2(140f, 44f);
        closeRt.anchoredPosition = new Vector2(8f, 0f);
        var closeImg = closeGo.AddComponent<Image>(); closeImg.color = BgMedium;
        var closeBtn = closeGo.AddComponent<Button>(); closeBtn.targetGraphic = closeImg;
        ApplyColors(closeBtn, BgMedium, NavBtnHov, NavBtnPrs);
        closeBtn.onClick.AddListener(CloseDeckModal);
        var closeLbl = MakeLabel(MakeChild(closeGo, "Lbl"), "Fermer", 16f, TextGray);
        Stretch(RT(closeLbl.gameObject));

        _footerGo = footer;

        // Les descriptions sont affichées inline dans chaque carte — pas de tooltip séparé

        // Charger le registry et construire le panneau de sélection de classe
        _classRegistry = Resources.Load<NymoraClassRegistry>("NymoraClasses/ClassRegistry");
        _classPanelGo  = BuildClassPanel(panel, HEADER_H);

        // Démarrer en vue sélection de classe
        ShowClassView();
    }

    // ── Deck builder — logique ────────────────────────────────────────────

    void OpenDeckModal()
    {
        _deckModal.SetActive(true);
        SetCameraZoom(false);

        // Si une classe était déjà choisie : aller directement à la vue sorts
        _selectedClass = HubManager.Instance?.SelectedClass;
        if (_selectedClass != null)
        {
            _allSpells.Clear();
            foreach (var s in _selectedClass.GetAllSpells())
                if (s != null) _allSpells.Add(s);

            _selectedSpells.Clear();
            var saved = HubManager.Instance?.SelectedDeck;
            if (saved != null) _selectedSpells.AddRange(saved);

            _currentTab = SpellDeckCategory.Attack;
            ShowSpellView();
            RefreshTabVisuals();
            PopulateSpellCards();
            UpdateDeckCounter();
            UpdateStatsLabel();
            RefreshDeckBar();
        }
        else
        {
            ShowClassView();
        }
    }

    void CloseDeckModal()
    {
        if (_deckModal != null) _deckModal.SetActive(false);
        SetCameraZoom(true);
    }

    void SetCameraZoom(bool enabled)
    {
        if (_isoCam == null && Camera.main != null)
            _isoCam = Camera.main.GetComponent<IsometricCamera>();
        if (_isoCam != null) _isoCam.zoomEnabled = enabled;
    }

    void UpdateStatsLabel()
    {
        if (_statsLabel == null) return;
        var tc    = FindObjectOfType<HubCharacterController>()?.GetComponent<TacticalCharacter>();
        var stats = tc?.stats;
        _statsLabel.text  = stats != null ? $"{stats.maxPA} PA  /  {stats.maxPM} PM" : "— PA / — PM";
        _statsLabel.color = AccentGold;
    }

    void SelectDeckTab(SpellDeckCategory cat)
    {
        if (_currentTab == cat) return;
        _currentTab = cat;
        RefreshTabVisuals();
        PopulateSpellCards();
    }

    void RefreshTabVisuals()
    {
        for (int i = 0; i < Tabs.Length; i++)
            _tabImages[i].color = (Tabs[i].cat == _currentTab) ? TabActive : TabNorm;
    }

    void PopulateSpellCards()
    {
        // Vider le contenu précédent
        foreach (Transform child in _deckScrollContent)
            Destroy(child.gameObject);
        _currentCards.Clear();

        foreach (var spell in _allSpells)
        {
            if (spell.deckCategory != _currentTab) continue;
            MakeSpellCard(spell);
        }

        RefreshCardVisuals();
    }

    /// <summary>
    /// Crée une carte de sort dans le deck builder.
    /// Cellule 190 × 248 : bande catégorie | icône 96px | nom | description | footer PA/portée.
    /// Description visible inline — visuellement épurée et harmonieuse.
    /// </summary>
    void MakeSpellCard(SpellData spell)
    {
        const float ICON_H = 96f;
        const float NAME_H = 22f;
        const float DESC_H = 74f;
        const float FOOT_H = 26f;
        const float STRIPE = 4f;   // bande colorée catégorie (bord gauche)
        const float PAD_X  = 6f;   // padding horizontal interne

        var go  = MakeChild(_deckScrollContent.gameObject, spell.spellName);
        var bg  = go.AddComponent<Image>();
        bg.color = CardBg;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        ApplyColors(btn, CardBg, new Color(CardBg.r + 0.04f, CardBg.g + 0.04f, CardBg.b + 0.06f), CardBgSel);

        // ── Bande catégorie gauche ──────────────────────────────────────────
        var stripeGo = MakeChild(go, "CatStripe");
        var stripeRt = RT(stripeGo);
        stripeRt.anchorMin = Vector2.zero;
        stripeRt.anchorMax = new Vector2(0f, 1f);
        stripeRt.pivot     = new Vector2(0f, 0.5f);
        stripeRt.sizeDelta = new Vector2(STRIPE, 0f);
        stripeGo.AddComponent<Image>().color = CategoryColor(spell.deckCategory);

        // ── Icône PixelLab (avec fond sombre) ─────────────────────────────
        float iconOffX = STRIPE + 2f;
        var iconBgGo = MakeChild(go, "IconBg");
        var iconBgRt = RT(iconBgGo);
        iconBgRt.anchorMin        = new Vector2(0f, 1f);
        iconBgRt.anchorMax        = new Vector2(1f, 1f);
        iconBgRt.pivot            = new Vector2(0.5f, 1f);
        iconBgRt.sizeDelta        = new Vector2(-iconOffX, ICON_H);
        iconBgRt.anchoredPosition = new Vector2(iconOffX * 0.5f, -0f);
        iconBgGo.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.11f, 1f);

        var iconGo = MakeChild(iconBgGo, "Icon");
        var iconRt = RT(iconGo);
        iconRt.anchorMin = new Vector2(0.08f, 0.06f);
        iconRt.anchorMax = new Vector2(0.92f, 0.94f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.raycastTarget = false;

        Sprite icon = SpellIconLoader.GetIcon(spell.spellName);
        if (icon != null)
        {
            iconImg.sprite         = icon;
            iconImg.preserveAspect = true;
        }
        else
        {
            iconImg.color = CategoryColor(spell.deckCategory) * new Color(1f, 1f, 1f, 0.35f);
        }

        // ── Séparateur fin ────────────────────────────────────────────────
        var sepGo = MakeChild(go, "Sep");
        var sepRt = RT(sepGo);
        sepRt.anchorMin        = new Vector2(0f, 1f);
        sepRt.anchorMax        = new Vector2(1f, 1f);
        sepRt.pivot            = new Vector2(0.5f, 1f);
        sepRt.sizeDelta        = new Vector2(-iconOffX, 1f);
        sepRt.anchoredPosition = new Vector2(iconOffX * 0.5f, -ICON_H);
        sepGo.AddComponent<Image>().color = new Color(0.35f, 0.35f, 0.42f, 0.40f);

        // ── Nom du sort ───────────────────────────────────────────────────
        float nameY = -(ICON_H + 1f);
        var nameGo = MakeChild(go, "SpellName");
        var nameRt = RT(nameGo);
        nameRt.anchorMin        = new Vector2(0f, 1f);
        nameRt.anchorMax        = new Vector2(1f, 1f);
        nameRt.pivot            = new Vector2(0.5f, 1f);
        nameRt.sizeDelta        = new Vector2(-(iconOffX + PAD_X * 2f), NAME_H);
        nameRt.anchoredPosition = new Vector2(iconOffX * 0.5f, nameY - 4f);
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text               = spell.spellName;
        nameTmp.fontSize           = 12.5f;
        nameTmp.color              = TextWhite;
        nameTmp.fontStyle          = FontStyles.Bold;
        nameTmp.alignment          = TextAlignmentOptions.Center;
        nameTmp.enableWordWrapping = false;
        nameTmp.overflowMode       = TextOverflowModes.Ellipsis;
        nameTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(nameTmp);

        // ── Description ───────────────────────────────────────────────────
        float descY = nameY - 4f - NAME_H;
        string descText = !string.IsNullOrEmpty(spell.description)
            ? spell.description
            : BuildEffectSummary(spell);
        var descGo = MakeChild(go, "Desc");
        var descRt = RT(descGo);
        descRt.anchorMin        = new Vector2(0f, 1f);
        descRt.anchorMax        = new Vector2(1f, 1f);
        descRt.pivot            = new Vector2(0.5f, 1f);
        descRt.sizeDelta        = new Vector2(-(iconOffX + PAD_X * 2f), DESC_H);
        descRt.anchoredPosition = new Vector2(iconOffX * 0.5f, descY - 3f);
        var descTmp = descGo.AddComponent<TextMeshProUGUI>();
        descTmp.text               = descText;
        descTmp.fontSize           = 10f;
        descTmp.color              = TextGray;
        descTmp.alignment          = TextAlignmentOptions.TopLeft;
        descTmp.enableWordWrapping = true;
        descTmp.overflowMode       = TextOverflowModes.Ellipsis;
        descTmp.lineSpacing        = -2f;
        descTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(descTmp);

        // ── Footer : PA · portée · recharge ──────────────────────────────
        var footGo = MakeChild(go, "Footer");
        var footRt = RT(footGo);
        footRt.anchorMin        = new Vector2(0f, 0f);
        footRt.anchorMax        = new Vector2(1f, 0f);
        footRt.pivot            = new Vector2(0.5f, 0f);
        footRt.sizeDelta        = new Vector2(-iconOffX, FOOT_H);
        footRt.anchoredPosition = new Vector2(iconOffX * 0.5f, 4f);
        footGo.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.09f, 0.80f);

        var footHlg = footGo.AddComponent<HorizontalLayoutGroup>();
        footHlg.padding              = new RectOffset(7, 7, 0, 0);
        footHlg.spacing              = 6f;
        footHlg.childAlignment       = TextAnchor.MiddleLeft;
        footHlg.childControlWidth    = false;
        footHlg.childControlHeight   = true;
        footHlg.childForceExpandWidth  = false;
        footHlg.childForceExpandHeight = true;

        MakeFooterTag(footGo, $"{spell.paCost} PA",       AccentGold,                        52f);
        MakeFooterTag(footGo, BuildPorteeText(spell),      new Color(0.70f, 0.76f, 0.90f),   58f);
        if (spell.cooldown > 0)
            MakeFooterTag(footGo, $"Rech. {spell.cooldown}t", new Color(0.72f, 0.58f, 0.96f), 56f);

        // ── Indicateur de sélection (coin sup. droit, carré doré) ────────
        var selGo = MakeChild(go, "SelMark");
        var selRt = RT(selGo);
        selRt.anchorMin        = new Vector2(1f, 1f);
        selRt.anchorMax        = new Vector2(1f, 1f);
        selRt.pivot            = new Vector2(1f, 1f);
        selRt.sizeDelta        = new Vector2(20f, 20f);
        selRt.anchoredPosition = new Vector2(-4f, -4f);
        selGo.AddComponent<Image>().color = AccentGold;
        selGo.SetActive(false);

        var captured = spell;
        btn.onClick.AddListener(() => ToggleSpell(captured));

        _currentCards.Add(new SpellCardView { spell = spell, bg = bg, iconImg = iconImg, selMark = selGo });
    }

    /// <summary>Crée un tag texte dans le footer horizontal d'une carte sort.</summary>
    static void MakeFooterTag(GameObject parent, string text, Color color, float width)
    {
        var go  = MakeChild(parent, "Tag_" + text);
        var le  = go.AddComponent<LayoutElement>();
        le.preferredWidth  = width;
        le.preferredHeight = 26f;
        le.flexibleWidth   = 0f;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = 10f;
        tmp.color         = color;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        OracleUIImportantFont.Apply(tmp);
    }

    /// <summary>Résumé lisible des effets si la description est vide.</summary>
    static string BuildEffectSummary(SpellData s)
    {
        if (s.effects == null || s.effects.Count == 0) return "Aucun effet décrit.";
        var sb = new System.Text.StringBuilder();
        foreach (var e in s.effects)
        {
            if (e.value != 0)
                sb.Append($"{e.type}: {e.value}");
            else
                sb.Append(e.type.ToString());
            if (e.duration > 0) sb.Append($" ({e.duration}t)");
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    static string BuildPorteeText(SpellData s)
    {
        if (s.isMeleeOnly) return "CàC";
        if (s.zoneType == ZoneType.Self || (s.rangeMin == 0 && s.rangeMax == 0)) return "0 PP";
        if (s.rangeMin == s.rangeMax) return $"{s.rangeMax} PP";
        return $"{s.rangeMin}-{s.rangeMax} PP";
    }

    void ToggleSpell(SpellData spell)
    {
        if (_selectedSpells.Contains(spell))
            _selectedSpells.Remove(spell);
        else if (_selectedSpells.Count < DeckData.MaxSpells)
            _selectedSpells.Add(spell);

        RefreshCardVisuals();
        UpdateDeckCounter();
        RefreshDeckBar();
    }

    void RefreshCardVisuals()
    {
        bool maxReached = _selectedSpells.Count >= DeckData.MaxSpells;
        foreach (var card in _currentCards)
        {
            bool selected = _selectedSpells.Contains(card.spell);
            bool dim      = maxReached && !selected;
            card.bg.color = selected ? CardBgSel : dim ? CardBgDim : CardBg;
            card.selMark.SetActive(selected);

            // Ternir légèrement l'icône si hors sélection et deck plein
            if (card.iconImg != null)
                card.iconImg.color = dim ? new Color(0.5f, 0.5f, 0.5f, 0.55f) : Color.white;
        }
        RefreshDeckBar();
    }

    // ── Barre de deck ─────────────────────────────────────────────────────

    /// <summary>
    /// Construit la barre de 6 slots icônes entre les onglets et la zone de scroll.
    /// </summary>
    void BuildDeckBar(GameObject panel, float headerH, float tabsH)
    {
        const float SLOT_SIZE = 68f;
        const float SLOT_GAP  = 6f;

        _deckBarRoot = MakeChild(panel, "DeckBar");
        var barRt    = RT(_deckBarRoot);
        barRt.anchorMin        = new Vector2(0f, 1f);
        barRt.anchorMax        = new Vector2(1f, 1f);
        barRt.pivot            = new Vector2(0.5f, 1f);
        barRt.sizeDelta        = new Vector2(0f, DeckBarH);
        barRt.anchoredPosition = new Vector2(0f, -(headerH + 2f + tabsH));
        _deckBarRoot.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.10f, 0.95f);

        // Étiquette gauche
        var lblGo = MakeChild(_deckBarRoot, "DeckBarLabel");
        var lblRt = RT(lblGo);
        lblRt.anchorMin        = new Vector2(0f, 0.5f);
        lblRt.anchorMax        = new Vector2(0f, 0.5f);
        lblRt.pivot            = new Vector2(0f, 0.5f);
        lblRt.sizeDelta        = new Vector2(110f, DeckBarH - 4f);
        lblRt.anchoredPosition = new Vector2(12f, 0f);
        var lblTmp = MakeLabel(MakeChild(lblGo, "T"), "DECK\nSÉLECTIONNÉ", 10f, AccentGold);
        lblTmp.fontStyle   = FontStyles.Bold;
        lblTmp.alignment   = TextAlignmentOptions.MidlineLeft;
        Stretch(RT(lblTmp.gameObject));

        // 6 slots icônes
        float totalW = DeckData.MaxSpells * SLOT_SIZE + (DeckData.MaxSpells - 1) * SLOT_GAP;
        float startX = -totalW * 0.5f + SLOT_SIZE * 0.5f;

        for (int i = 0; i < DeckData.MaxSpells; i++)
        {
            float x = startX + i * (SLOT_SIZE + SLOT_GAP);

            var slotGo = MakeChild(_deckBarRoot, $"DeckSlot{i}");
            var slotRt = RT(slotGo);
            slotRt.anchorMin        = new Vector2(0.5f, 0.5f);
            slotRt.anchorMax        = new Vector2(0.5f, 0.5f);
            slotRt.pivot            = new Vector2(0.5f, 0.5f);
            slotRt.sizeDelta        = new Vector2(SLOT_SIZE, SLOT_SIZE);
            slotRt.anchoredPosition = new Vector2(x, 0f);

            var slotImg = slotGo.AddComponent<Image>();
            slotImg.color = new Color(0.12f, 0.12f, 0.17f, 1f);

            var iconGo = MakeChild(slotGo, "Icon");
            var iconRt = RT(iconGo);
            iconRt.anchorMin = new Vector2(0.07f, 0.07f);
            iconRt.anchorMax = new Vector2(0.93f, 0.93f);
            iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
            var img = iconGo.AddComponent<Image>();
            img.preserveAspect = true;
            img.enabled        = false;
            _deckBarIcons[i]   = img;

            // Numéro de slot
            var numGo = MakeChild(slotGo, "Num");
            var numRt = RT(numGo);
            numRt.anchorMin        = new Vector2(0f, 0f);
            numRt.anchorMax        = new Vector2(0.45f, 0.45f);
            numRt.offsetMin        = numRt.offsetMax = Vector2.zero;
            var numTmp = numGo.AddComponent<TextMeshProUGUI>();
            numTmp.text          = $"{i + 1}";
            numTmp.fontSize      = 10f;
            numTmp.color         = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            numTmp.alignment     = TextAlignmentOptions.BottomLeft;
            numTmp.raycastTarget = false;
            OracleUIImportantFont.Apply(numTmp);

            // Clic sur le slot = retirer le sort
            int captured = i;
            var btn = slotGo.AddComponent<Button>();
            btn.targetGraphic = slotImg;
            btn.onClick.AddListener(() => RemoveFromDeckBar(captured));
        }
    }

    void RemoveFromDeckBar(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _selectedSpells.Count) return;
        _selectedSpells.RemoveAt(slotIndex);
        RefreshCardVisuals();
        UpdateDeckCounter();
        RefreshDeckBar();
    }

    void RefreshDeckBar()
    {
        for (int i = 0; i < DeckData.MaxSpells; i++)
        {
            if (_deckBarIcons[i] == null) continue;

            if (i < _selectedSpells.Count && _selectedSpells[i] != null)
            {
                var spell  = _selectedSpells[i];
                Sprite spr = SpellIconLoader.GetIcon(spell.spellName);
                if (spr == null) spr = spell.icon;

                if (spr != null)
                {
                    _deckBarIcons[i].sprite         = spr;
                    _deckBarIcons[i].preserveAspect = true;
                    _deckBarIcons[i].color          = Color.white;
                    _deckBarIcons[i].enabled        = true;
                }
                else
                {
                    _deckBarIcons[i].sprite  = null;
                    _deckBarIcons[i].color   = CategoryColor(spell.deckCategory) * new Color(1f, 1f, 1f, 0.6f);
                    _deckBarIcons[i].enabled = true;
                }
            }
            else
            {
                _deckBarIcons[i].sprite  = null;
                _deckBarIcons[i].enabled = false;
            }
        }
    }

    void UpdateDeckCounter()
    {
        if (_deckCounter == null) return;
        int n = _selectedSpells.Count;
        _deckCounter.text  = $"{n} / {DeckData.MaxSpells} sorts";
        _deckCounter.color = (n == DeckData.MaxSpells) ? AccentGold : TextGray;
    }

    void SaveAndCloseDeck()
    {
        if (HubManager.Instance != null)
        {
            HubManager.Instance.SelectedDeck  = new List<SpellData>(_selectedSpells);
            HubManager.Instance.SelectedClass = _selectedClass;
        }

        string className = _selectedClass != null ? $" [{_selectedClass.displayName}]" : "";
        string info = _selectedSpells.Count == DeckData.MaxSpells
            ? $"Deck sauvegardé{className} ({DeckData.MaxSpells} sorts)."
            : $"Deck partiel sauvegardé{className} ({_selectedSpells.Count} / {DeckData.MaxSpells} sorts). Complète-le avant le combat !";
        AppendChat(info, AccentGold);

        CloseDeckModal();
    }

    static Color CategoryColor(SpellDeckCategory cat)
    {
        switch (cat)
        {
            case SpellDeckCategory.Attack:   return CatAtk;
            case SpellDeckCategory.Tactic:   return CatTac;
            case SpellDeckCategory.Survival: return CatSur;
            default: return Color.gray;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // SÉLECTION DE CLASSE
    // ════════════════════════════════════════════════════════════════════

    void ShowClassView()
    {
        if (_classPanelGo != null) _classPanelGo.SetActive(true);
        if (_sepGo        != null) _sepGo.SetActive(false);
        if (_tabBarGo     != null) _tabBarGo.SetActive(false);
        if (_deckBarRoot  != null) _deckBarRoot.SetActive(false);
        if (_scrollGoRef  != null) _scrollGoRef.SetActive(false);
        if (_footerGo     != null) _footerGo.SetActive(false);
        RefreshClassCardHighlights();
    }

    void ShowSpellView()
    {
        if (_classPanelGo != null) _classPanelGo.SetActive(false);
        if (_sepGo        != null) _sepGo.SetActive(true);
        if (_tabBarGo     != null) _tabBarGo.SetActive(true);
        if (_deckBarRoot  != null) _deckBarRoot.SetActive(true);
        if (_scrollGoRef  != null) _scrollGoRef.SetActive(true);
        if (_footerGo     != null) _footerGo.SetActive(true);
    }

    GameObject BuildClassPanel(GameObject panel, float headerH)
    {
        const float TITLE_H = 50f;
        const float SEP_H   = 2f;
        const float PAD     = 16f;

        var cp   = MakeChild(panel, "ClassPanel");
        var cpRt = RT(cp);
        cpRt.anchorMin        = new Vector2(0f, 0f);
        cpRt.anchorMax        = new Vector2(1f, 1f);
        cpRt.offsetMin        = new Vector2(0f, 0f);
        cpRt.offsetMax        = new Vector2(0f, -headerH);
        cp.AddComponent<Image>().color = BgMedium;

        // Titre
        var titleGo = MakeChild(cp, "ClassTitle");
        var titleRt = RT(titleGo);
        titleRt.anchorMin        = new Vector2(0f, 1f);
        titleRt.anchorMax        = new Vector2(1f, 1f);
        titleRt.pivot            = new Vector2(0.5f, 1f);
        titleRt.sizeDelta        = new Vector2(0f, TITLE_H);
        titleRt.anchoredPosition = Vector2.zero;
        var titleTmp = MakeLabel(MakeChild(titleGo, "T"), "CHOISIR TA CLASSE", 22f, AccentGold);
        titleTmp.fontStyle = FontStyles.Bold;
        Stretch(RT(titleTmp.gameObject));

        // Séparateur
        var csep   = MakeChild(cp, "ClassSep");
        var csepRt = RT(csep);
        csepRt.anchorMin        = new Vector2(0f, 1f);
        csepRt.anchorMax        = new Vector2(1f, 1f);
        csepRt.pivot            = new Vector2(0.5f, 1f);
        csepRt.sizeDelta        = new Vector2(-20f, SEP_H);
        csepRt.anchoredPosition = new Vector2(0f, -TITLE_H);
        csep.AddComponent<Image>().color = AccentGold;

        // Zone des 5 cartes
        var cardsGo = MakeChild(cp, "CardsArea");
        var cardsRt = RT(cardsGo);
        cardsRt.anchorMin = new Vector2(0f, 0f);
        cardsRt.anchorMax = new Vector2(1f, 1f);
        cardsRt.offsetMin = new Vector2(PAD, PAD);
        cardsRt.offsetMax = new Vector2(-PAD, -(TITLE_H + SEP_H + 4f));

        var hlg = cardsGo.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = PAD;
        hlg.padding              = new RectOffset(0, 0, 0, 0);
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        // Cartes de classe
        if (_classRegistry != null && _classRegistry.classes != null)
        {
            int count = Mathf.Min(_classRegistry.classes.Count, _classCardBgs.Length);
            for (int i = 0; i < count; i++)
            {
                var cd = _classRegistry.classes[i];
                if (cd != null) MakeClassCard(cardsGo, cd, i);
            }
        }
        else
        {
            // Fallback si le registry n'est pas encore généré
            var fallback = MakeLabel(MakeChild(cardsGo, "FallbackMsg"),
                "Lance Oracle > Generate Nymora Classes\npuis relance la scène Hub.",
                16f, TextGray);
            fallback.enableWordWrapping = true;
            Stretch(RT(fallback.gameObject));
        }

        return cp;
    }

    void MakeClassCard(GameObject parent, ClassData cd, int index)
    {
        var card   = MakeChild(parent, cd.displayName + "Card");
        var cardBg = card.AddComponent<Image>();
        cardBg.color = CardBg;
        var cardBtn = card.AddComponent<Button>();
        cardBtn.targetGraphic = cardBg;
        ApplyColors(cardBtn, CardBg,
            new Color(CardBg.r + 0.06f, CardBg.g + 0.06f, CardBg.b + 0.06f, 1f),
            new Color(CardBg.r - 0.03f, CardBg.g - 0.03f, CardBg.b - 0.03f, 1f));

        if (index < _classCardBgs.Length) _classCardBgs[index] = cardBg;

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = 0f;
        vlg.padding              = new RectOffset(10, 10, 10, 10);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        // Initiale de classe (remplace emoji — Aseprite ne contient pas les glyphes emoji)
        var emojiGo = MakeChild(card, "Emoji");
        var emojiLe = emojiGo.AddComponent<LayoutElement>();
        emojiLe.preferredHeight = 54f;
        var emojiTmp = emojiGo.AddComponent<TextMeshProUGUI>();
        emojiTmp.text          = cd.displayName.Length > 0 ? cd.displayName[0].ToString().ToUpper() : "";
        emojiTmp.fontSize      = 36f;
        emojiTmp.color         = AccentGold;
        emojiTmp.fontStyle     = FontStyles.Bold;
        emojiTmp.alignment     = TextAlignmentOptions.Center;
        emojiTmp.raycastTarget = false;
        OracleUIImportantFont.Apply(emojiTmp);

        // Nom
        var nameGo = MakeChild(card, "ClassName");
        var nameLe = nameGo.AddComponent<LayoutElement>();
        nameLe.preferredHeight = 34f;
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text               = cd.displayName.ToUpper();
        nameTmp.fontSize           = 17f;
        nameTmp.color              = AccentGold;
        nameTmp.fontStyle          = FontStyles.Bold;
        nameTmp.alignment          = TextAlignmentOptions.Center;
        nameTmp.enableWordWrapping = false;
        nameTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(nameTmp);

        // Séparateur
        var sep1Go = MakeChild(card, "Sep1");
        sep1Go.AddComponent<LayoutElement>().preferredHeight = 1f;
        sep1Go.AddComponent<Image>().color = new Color(0.35f, 0.35f, 0.40f, 0.6f);

        // Lore (3 lignes max)
        var loreGo = MakeChild(card, "Lore");
        var loreLe = loreGo.AddComponent<LayoutElement>();
        loreLe.preferredHeight = 270f;
        var loreTmp = loreGo.AddComponent<TextMeshProUGUI>();
        loreTmp.text               = cd.lore;
        loreTmp.fontSize           = 12.5f;
        loreTmp.color              = TextGray;
        loreTmp.alignment          = TextAlignmentOptions.TopLeft;
        loreTmp.enableWordWrapping = true;
        loreTmp.overflowMode       = TextOverflowModes.Ellipsis;
        loreTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(loreTmp);

        // Séparateur
        var sep2Go = MakeChild(card, "Sep2");
        sep2Go.AddComponent<LayoutElement>().preferredHeight = 1f;
        sep2Go.AddComponent<Image>().color = new Color(0.35f, 0.35f, 0.40f, 0.6f);

        // Passif — titre
        var passiveNameGo = MakeChild(card, "PassiveName");
        passiveNameGo.AddComponent<LayoutElement>().preferredHeight = 26f;
        var passiveNameTmp = passiveNameGo.AddComponent<TextMeshProUGUI>();
        passiveNameTmp.text               = cd.passiveName;
        passiveNameTmp.fontSize           = 13.5f;
        passiveNameTmp.color              = AccentGold;
        passiveNameTmp.fontStyle          = FontStyles.Bold;
        passiveNameTmp.alignment          = TextAlignmentOptions.Center;
        passiveNameTmp.enableWordWrapping = false;
        passiveNameTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(passiveNameTmp);

        // Mécanique de jauge (+1 jauge par…)
        var mechGo = MakeChild(card, "PassiveMechanic");
        mechGo.AddComponent<LayoutElement>().preferredHeight = 42f;
        var mechTmp = mechGo.AddComponent<TextMeshProUGUI>();
        mechTmp.text               = cd.passiveMechanic;
        mechTmp.fontSize           = 10.5f;
        mechTmp.color              = new Color(0.70f, 0.82f, 1.0f, 0.9f);
        mechTmp.fontStyle          = FontStyles.Italic;
        mechTmp.alignment          = TextAlignmentOptions.Center;
        mechTmp.enableWordWrapping = true;
        mechTmp.overflowMode       = TextOverflowModes.Ellipsis;
        mechTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(mechTmp);

        // Paliers
        BuildPassiveStageRow(card, cd.stageBase);
        BuildPassiveStageRow(card, cd.stageEveille);
        BuildPassiveStageRow(card, cd.stageEnrage);

        // Sort Enragé (si la classe en a un)
        if (!string.IsNullOrEmpty(cd.enragedSpellName))
            BuildEnragedSpellBlock(card, cd);

        // Spacer
        var spacerGo = MakeChild(card, "Spacer");
        spacerGo.AddComponent<LayoutElement>().flexibleHeight = 1f;

        // Bouton Sélectionner
        var selGo  = MakeChild(card, "SelectBtn");
        var selLe  = selGo.AddComponent<LayoutElement>();
        selLe.preferredHeight = 38f;
        var selImg = selGo.AddComponent<Image>(); selImg.color = AccentGold;
        var selBtn = selGo.AddComponent<Button>(); selBtn.targetGraphic = selImg;
        ApplyColors(selBtn, AccentGold, AccentGoldH, AccentGoldP);
        var captured = cd;
        int captIdx  = index;
        selBtn.onClick.AddListener(() => SelectClass(captured, captIdx));
        cardBtn.onClick.AddListener(() => SelectClass(captured, captIdx));
        var selLbl = MakeLabel(MakeChild(selGo, "Lbl"), "SÉLECTIONNER", 12f, new Color(0.10f, 0.07f, 0.03f, 1f));
        selLbl.fontStyle = FontStyles.Bold;
        Stretch(RT(selLbl.gameObject));
    }

    void BuildPassiveStageRow(GameObject parent, PassiveStage stage)
    {
        if (stage == null) return;
        var rowGo = MakeChild(parent, stage.label + "Row");
        rowGo.AddComponent<LayoutElement>().preferredHeight = 52f;
        var rowVlg = rowGo.AddComponent<VerticalLayoutGroup>();
        rowVlg.childForceExpandWidth = true;
        rowVlg.childForceExpandHeight = false;
        rowVlg.childControlWidth = true;
        rowVlg.childControlHeight = true;
        rowVlg.spacing = 1f;

        // Label palier
        var labelGo  = MakeChild(rowGo, "StageLabel");
        labelGo.AddComponent<LayoutElement>().preferredHeight = 18f;
        var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
        string range = stage.jaugeMax < 0
            ? $"{stage.jaugeMin} jauges"
            : $"{stage.jaugeMin}-{stage.jaugeMax} jauges";
        labelTmp.text               = $"<b>{stage.label}</b>  <color=#C8A84B><size=11>{range}</size></color>";
        labelTmp.fontSize           = 11.5f;
        labelTmp.color              = TextWhite;
        labelTmp.enableWordWrapping = false;
        labelTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(labelTmp);

        // Effet
        var effectGo  = MakeChild(rowGo, "StageEffect");
        effectGo.AddComponent<LayoutElement>().preferredHeight = 32f;
        var effectTmp = effectGo.AddComponent<TextMeshProUGUI>();
        effectTmp.text               = stage.effectDescription;
        effectTmp.fontSize           = 11f;
        effectTmp.color              = TextGray;
        effectTmp.enableWordWrapping = true;
        effectTmp.overflowMode       = TextOverflowModes.Ellipsis;
        effectTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(effectTmp);
    }

    void BuildEnragedSpellBlock(GameObject parent, ClassData cd)
    {
        // Conteneur
        var blockGo = MakeChild(parent, "EnragedSpellBlock");
        var blockLe = blockGo.AddComponent<LayoutElement>();
        blockLe.preferredHeight = 100f;
        var blockVlg = blockGo.AddComponent<VerticalLayoutGroup>();
        blockVlg.childForceExpandWidth  = true;
        blockVlg.childForceExpandHeight = false;
        blockVlg.childControlWidth      = true;
        blockVlg.childControlHeight     = true;
        blockVlg.spacing  = 2f;
        blockVlg.padding  = new RectOffset(6, 6, 4, 4);

        // Fond teinté rouge sombre pour distinguer le bloc
        var blockBg = blockGo.AddComponent<Image>();
        blockBg.color = new Color(0.22f, 0.06f, 0.06f, 0.55f);

        // Titre : "★ Sort signature — Saignée"
        var titleGo  = MakeChild(blockGo, "EnragedTitle");
        titleGo.AddComponent<LayoutElement>().preferredHeight = 17f;
        var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text               = $"[!] Sort signature  —  {cd.enragedSpellName}";
        titleTmp.fontSize           = 11f;
        titleTmp.fontStyle          = FontStyles.Bold;
        titleTmp.color              = new Color(1f, 0.32f, 0.32f, 1f);
        titleTmp.enableWordWrapping = false;
        titleTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(titleTmp);

        // Description : PA + portée + effet
        var descGo  = MakeChild(blockGo, "EnragedDesc");
        descGo.AddComponent<LayoutElement>().preferredHeight = 55f;
        var descTmp = descGo.AddComponent<TextMeshProUGUI>();
        descTmp.text               = cd.enragedSpellDescription;
        descTmp.fontSize           = 10.5f;
        descTmp.color              = TextGray;
        descTmp.enableWordWrapping = true;
        descTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(descTmp);

        // Ligne "Utilisable qu'une fois en combat" en italique rouge clair
        var onceTxtGo  = MakeChild(blockGo, "EnragedOnce");
        onceTxtGo.AddComponent<LayoutElement>().preferredHeight = 15f;
        var onceTmp = onceTxtGo.AddComponent<TextMeshProUGUI>();
        onceTmp.text               = "Utilisable qu'une fois en combat.";
        onceTmp.fontSize           = 10f;
        onceTmp.fontStyle          = FontStyles.Italic;
        onceTmp.color              = new Color(1f, 0.55f, 0.55f, 1f);
        onceTmp.enableWordWrapping = false;
        onceTmp.raycastTarget      = false;
        OracleUIImportantFont.Apply(onceTmp);
    }

    void SelectClass(ClassData cd, int cardIndex)
    {
        _selectedClass = cd;

        // Charger les sorts de la classe dans le pool local
        _allSpells.Clear();
        if (cd != null)
            foreach (var s in cd.GetAllSpells())
                if (s != null) _allSpells.Add(s);

        _selectedSpells.Clear();

        _currentTab = SpellDeckCategory.Attack;
        ShowSpellView();
        RefreshTabVisuals();
        PopulateSpellCards();
        UpdateDeckCounter();
        UpdateStatsLabel();

        if (_allSpells.Count == 0)
            AppendChat($"{cd.displayName} sélectionné — sorts à venir en Phase 4.", AccentGold);
        else
            AppendChat($"{cd.displayName} sélectionné — {_allSpells.Count} sorts disponibles.", AccentGold);
    }

    void RefreshClassCardHighlights()
    {
        if (_classRegistry == null) return;
        for (int i = 0; i < _classCardBgs.Length && i < _classRegistry.classes.Count; i++)
        {
            if (_classCardBgs[i] == null) continue;
            bool isSelected = _selectedClass != null && _classRegistry.classes[i] == _selectedClass;
            _classCardBgs[i].color = isSelected ? CardBgSel : CardBg;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // TOOLTIP DECK BUILDER  (supprimé — descriptions inline sur chaque carte)
    // ════════════════════════════════════════════════════════════════════
#if false   // bloc désactivé — conservé pour référence
    void BuildDeckBuilderTooltip(Canvas canvas)
    {
        const float W = 300f;

        _deckTooltip = MakeChild(canvas.gameObject, "DeckBuilderTooltip");
        var rt = RT(_deckTooltip);
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot     = Vector2.zero;
        rt.sizeDelta = new Vector2(W, 0f); // hauteur gérée par ContentSizeFitter

        var bg = _deckTooltip.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.08f, 0.97f);

        // Outline visuelle
        var outline = _deckTooltip.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor    = AccentGold * new Color(1f, 1f, 1f, 0.6f);
        outline.effectDistance = new Vector2(1f, -1f);

        var vlg = _deckTooltip.AddComponent<VerticalLayoutGroup>();
        vlg.padding             = new RectOffset(12, 12, 10, 10);
        vlg.spacing             = 5f;
        vlg.childControlWidth   = true;
        vlg.childControlHeight  = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        _deckTooltip.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Ligne 1 : icône + nom
        var headerGo = MakeChild(_deckTooltip, "Header");
        var headerHlg = headerGo.AddComponent<HorizontalLayoutGroup>();
        headerHlg.spacing           = 8f;
        headerHlg.childControlWidth  = false;
        headerHlg.childControlHeight = true;
        headerHlg.childForceExpandHeight = true;
        headerHlg.childAlignment    = TextAnchor.MiddleLeft;
        headerGo.AddComponent<LayoutElement>().preferredHeight = 48f;

        // Icône sort
        var iconGo = MakeChild(headerGo, "Icon");
        var iconLe = iconGo.AddComponent<LayoutElement>();
        iconLe.preferredWidth = iconLe.preferredHeight = 44f;
        iconLe.minWidth = iconLe.minHeight = 44f;
        _ttIcon = iconGo.AddComponent<Image>();
        _ttIcon.preserveAspect = true;

        // Nom + meta (colonne droite)
        var nameColGo = MakeChild(headerGo, "NameCol");
        var nameColLe = nameColGo.AddComponent<LayoutElement>();
        nameColLe.flexibleWidth = 1f;
        var nameColVlg = nameColGo.AddComponent<VerticalLayoutGroup>();
        nameColVlg.childControlWidth  = true;
        nameColVlg.childControlHeight = true;
        nameColVlg.childForceExpandWidth = true;
        nameColVlg.childForceExpandHeight = false;

        _ttName = MakeLabel(MakeChild(nameColGo, "Name"), "", 15f, AccentGold);
        _ttName.alignment  = TextAlignmentOptions.MidlineLeft;
        _ttName.fontStyle  = FontStyles.Bold;

        _ttMeta = MakeLabel(MakeChild(nameColGo, "Meta"), "", 11f, new Color(0.72f, 0.78f, 0.88f));
        _ttMeta.alignment  = TextAlignmentOptions.MidlineLeft;

        // Séparateur
        var sepGo = MakeChild(_deckTooltip, "Sep");
        sepGo.AddComponent<LayoutElement>().preferredHeight = 1f;
        sepGo.AddComponent<Image>().color = new Color(0.38f, 0.38f, 0.44f, 0.50f);

        // Description
        var descGo = MakeChild(_deckTooltip, "Desc");
        var descLe = descGo.AddComponent<LayoutElement>();
        descLe.flexibleWidth = 1f;
        _ttDesc = descGo.AddComponent<TextMeshProUGUI>();
        _ttDesc.fontSize           = 12f;
        _ttDesc.color              = new Color(0.88f, 0.90f, 0.93f);
        _ttDesc.alignment          = TextAlignmentOptions.TopLeft;
        _ttDesc.enableWordWrapping = true;
        _ttDesc.raycastTarget      = false;
        OracleUIImportantFont.Apply(_ttDesc);

        // Synergie (optionnelle)
        var synGo = MakeChild(_deckTooltip, "Synergy");
        var synLe = synGo.AddComponent<LayoutElement>();
        synLe.flexibleWidth = 1f;
        _ttSynergy = synGo.AddComponent<TextMeshProUGUI>();
        _ttSynergy.fontSize           = 11f;
        _ttSynergy.color              = new Color(0.72f, 0.88f, 0.76f);
        _ttSynergy.alignment          = TextAlignmentOptions.TopLeft;
        _ttSynergy.enableWordWrapping = true;
        _ttSynergy.raycastTarget      = false;
        OracleUIImportantFont.Apply(_ttSynergy);

        _deckTooltip.SetActive(false);
    }

    void ShowDeckTooltip(SpellData spell, RectTransform anchor)
    {
        if (_deckTooltip == null || spell == null) return;

        // Icône PixelLab (pas de fallback CARTE_SORT)
        if (_ttIcon != null)
        {
            Sprite icon = SpellIconLoader.GetIcon(spell.spellName);
            _ttIcon.sprite  = icon;
            _ttIcon.enabled = icon != null;
        }

        if (_ttName    != null) _ttName.text = spell.spellName;

        if (_ttMeta != null)
        {
            string portee = spell.isMeleeOnly ? "CàC"
                          : (spell.zoneType == ZoneType.Self) ? "Soi-même"
                          : (spell.rangeMin == spell.rangeMax) ? $"Portée {spell.rangeMax}"
                          : $"Portée {spell.rangeMin}–{spell.rangeMax}";
            string cd = spell.cooldown > 0 ? $"  ·  Recharge {spell.cooldown}t" : "";
            _ttMeta.text = $"{spell.paCost} PA  ·  {portee}{cd}";
        }

        if (_ttDesc != null)
        {
            string txt = !string.IsNullOrEmpty(spell.description)
                ? spell.description
                : BuildEffectSummary(spell);
            _ttDesc.text = txt;
        }

        if (_ttSynergy != null)
        {
            bool hasSyn = !string.IsNullOrEmpty(spell.synergyDescription);
            _ttSynergy.gameObject.SetActive(hasSyn);
            _ttSynergy.text = hasSyn ? $"⟐ {spell.synergyDescription}" : "";
        }

        _deckTooltip.SetActive(true);

        // Positionnement : à droite de l'icône, recalé si hors écran
        Canvas.ForceUpdateCanvases();
        PositionDeckTooltip(anchor);
    }

    void PositionDeckTooltip(RectTransform anchor)
    {
        if (_deckTooltip == null || anchor == null) return;

        var canvasRt = FindFirstObjectByType<Canvas>()?.GetComponent<RectTransform>();
        if (canvasRt == null) return;

        var ttRt = RT(_deckTooltip);

        // Position monde → canvas local
        Vector2 localPt;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt,
            RectTransformUtility.WorldToScreenPoint(null, anchor.position),
            null,
            out localPt);

        float ttW = ttRt.rect.width;
        float ttH = ttRt.rect.height;
        float cW  = canvasRt.rect.width;
        float cH  = canvasRt.rect.height;

        // Essaie de placer à droite du slot
        float x = localPt.x + anchor.rect.width * 0.5f + 8f;
        float y = localPt.y;

        // Recadrage horizontal
        if (x + ttW > cW * 0.5f - 8f)
            x = localPt.x - anchor.rect.width * 0.5f - ttW - 8f;
        x = Mathf.Clamp(x, -cW * 0.5f + 4f, cW * 0.5f - ttW - 4f);

        // Recadrage vertical
        y = Mathf.Clamp(y, -cH * 0.5f + 4f, cH * 0.5f - ttH - 4f);

        ttRt.anchorMin = ttRt.anchorMax = new Vector2(0.5f, 0.5f);
        ttRt.pivot     = Vector2.zero;
        ttRt.anchoredPosition = new Vector2(x, y);
    }

    void HideDeckTooltip() { }  // stub — tooltip supprimé (descriptions inline)
#endif

    // ════════════════════════════════════════════════════════════════════
    // HELPERS UI
    // ════════════════════════════════════════════════════════════════════

    static GameObject MakeChild(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static RectTransform RT(GameObject go) => (RectTransform)go.transform;

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void ApplyColors(Button btn, Color normal, Color highlighted, Color pressed)
    {
        var cs = btn.colors;
        cs.normalColor      = normal;
        cs.highlightedColor = highlighted;
        cs.pressedColor     = pressed;
        btn.colors = cs;
    }

    static TextMeshProUGUI MakeLabel(GameObject go, string text, float fontSize, Color color)
    {
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        OracleUIImportantFont.Apply(tmp);
        return tmp;
    }
}

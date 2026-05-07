using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Barre de sorts en combat — 6 slots carrés alignés horizontalement en bas de l'écran.
/// Remplace l'ancien éventail de cartes (fan layout).
/// Hover et clic gérés manuellement (RectTransformUtility) pour s'affranchir
/// des problèmes de GraphicRaycaster / EventSystem.
/// </summary>
public class DeckUI : MonoBehaviour
{
    // ── Configuration ─────────────────────────────────────────────────────
    [Header("Slots (6 max)")]
    public List<SpellSlotUI> slots = new List<SpellSlotUI>();

    [Header("Tooltip")]
    public SpellTooltip tooltip;

    [Header("Bar Layout")]
    [Tooltip("Taille de chaque slot en pixels (carré).")]
    public float barSlotSize    = 72f;
    [Tooltip("Espacement entre les slots.")]
    public float barSpacing     = 8f;
    [Tooltip("Distance du bord bas de l'écran (anchorMin.y = 0).")]
    public float barBaseY       = 14f;
    [Tooltip("Décalage vertical au survol / sélection.")]
    public float barRaise       = 8f;

    // Compat. héritage — conservés pour que les scripts éditeur ne cassent pas
    [HideInInspector] public bool  fanMode = false;
    [HideInInspector] public float fanCardW = 72f;
    [HideInInspector] public float fanCardH = 72f;
    [HideInInspector] public float spellSlotHudReferenceHeight = 72f;
    [HideInInspector] public float fanSpread = 0f;
    [HideInInspector] public float fanAngleMax = 0f;
    [HideInInspector] public float fanArcHeight = 0f;
    [HideInInspector] public float fanBaseY = 14f;
    [HideInInspector] public float fanRaise = 8f;

    const int MaxVisibleSlots = 7; // 6 sorts + 1 sort Enragé éventuel

    // ── État ─────────────────────────────────────────────────────────────
    TacticalCharacter _activeCharacter;
    SpellCaster       _activeCaster;
    int               _selectedSlotIndex = -1;

    // Hover manuel
    SpellSlotUI _hoveredSlot;
    Camera      _uiCamera;

    // ── Liaison personnage ────────────────────────────────────────────────
    public void BindCharacter(TacticalCharacter character)
    {
        if (_activeCharacter != null)
        {
            _activeCharacter.OnPAChanged     -= OnResourceChanged;
            _activeCharacter.OnStateChanged  -= OnCharacterStateChanged;
            _activeCharacter.OnSpellsChanged -= RebuildSlots;
        }

        _activeCharacter = character;
        _activeCaster    = character != null ? character.GetComponent<SpellCaster>() : null;

        if (_activeCharacter != null)
        {
            _activeCharacter.OnPAChanged     += OnResourceChanged;
            _activeCharacter.OnStateChanged  += OnCharacterStateChanged;
            _activeCharacter.OnSpellsChanged += RebuildSlots;
        }

        RebuildSlots();
    }

    public void UnbindCharacter()
    {
        BindCharacter(null);
        ClearSelection();
    }

    // ── Construction de la barre ──────────────────────────────────────────
    void EnsureSlotsCount(int needed)
    {
        if (slots.Count == 0 || needed <= slots.Count) return;
        while (slots.Count < needed && slots.Count < MaxVisibleSlots)
        {
            var clone  = Instantiate(slots[0].gameObject, slots[0].transform.parent);
            var slotUI = clone.GetComponent<SpellSlotUI>();
            clone.SetActive(false);
            slots.Add(slotUI);
        }
    }

    void RebuildSlots()
    {
        if (_activeCharacter != null)
            EnsureSlotsCount(_activeCharacter.ActiveSpells.Count);

        var spellList = _activeCharacter?.ActiveSpells;

        // ── Passe 1 : activer/désactiver les slots selon les sorts disponibles ─
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;
            if (i >= MaxVisibleSlots) { slots[i].gameObject.SetActive(false); continue; }

            SpellData spell = spellList != null && i < spellList.Count ? spellList[i] : null;
            bool visible = _activeCharacter != null && spell != null;
            // Active sans contenu : le slot est en place avant Setup pour que
            // _restPos soit déjà correct quand SetSelected → AnimateTo se déclenche.
            slots[i].gameObject.SetActive(visible);
        }

        // ── Passe 2 : positionner AVANT Setup (fixe _restPos avant toute animation) ──
        ApplyBarLayout();

        // ── Passe 3 : initialiser le contenu (icône, PA, hotkey) ────────────────
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null || i >= MaxVisibleSlots) continue;
            SpellData spell = spellList != null && i < spellList.Count ? spellList[i] : null;
            if (spell != null)
                slots[i].Setup(spell, _activeCharacter, this, i);
        }

        ClearSelection();
    }

    // ── Bar Layout ────────────────────────────────────────────────────────
    /// <summary>
    /// Dispose les slots visibles en ligne horizontale centrée, ancrée en bas de l'écran.
    /// </summary>
    public void ApplyBarLayout()
    {
        // Supprimer un éventuel HorizontalLayoutGroup qui écraserait les positions manuelles
        var hlg = GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (hlg != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.DestroyObjectImmediate(hlg);
            else
#endif
            Destroy(hlg);
        }

        // DeckUI couvre tout le canvas
        var deckRT = GetComponent<RectTransform>();
        if (deckRT != null)
        {
            deckRT.anchorMin        = Vector2.zero;
            deckRT.anchorMax        = Vector2.one;
            deckRT.offsetMin        = Vector2.zero;
            deckRT.offsetMax        = Vector2.zero;
            deckRT.anchoredPosition = Vector2.zero;
        }

        // Collecter les slots actifs
        var visible = new List<SpellSlotUI>();
        for (int i = 0; i < slots.Count && visible.Count < MaxVisibleSlots; i++)
            if (slots[i] != null && slots[i].gameObject.activeSelf)
                visible.Add(slots[i]);

        int n = visible.Count;
        if (n == 0) return;

        float step       = barSlotSize + barSpacing;
        float totalWidth = n * barSlotSize + (n - 1) * barSpacing;
        float startX     = -totalWidth * 0.5f + barSlotSize * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float x  = startX + i * step;
            float y  = barBaseY;

            var rt = visible[i].GetComponent<RectTransform>();
            // Ancre bas-centre du canvas
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);

            visible[i].SetRestPose(
                new Vector2(x, y),
                0f,                          // pas de rotation
                new Vector2(barSlotSize, barSlotSize),
                barRaise);

            // Sibling order plat (pas de superposition en éventail)
            visible[i].transform.SetSiblingIndex(i);
        }
    }

    /// <summary>Alias de compatibilité héritage (anciens appels à ApplyFanLayout).</summary>
    public void ApplyFanLayout() => ApplyBarLayout();

    /// <summary>No-op : la barre n'a pas de sibling order à restaurer.</summary>
    public void RestoreSiblingOrder() { }

    // ── Update — hover et clic 100 % manuels ─────────────────────────────
    void Awake()
    {
        // Supprimer tout Animator hérité (animation d'éventail de l'ancien système de cartes)
        var anim = GetComponent<Animator>();
        if (anim != null) Destroy(anim);

        var canvas = GetComponentInParent<Canvas>();
        _uiCamera  = canvas != null ? canvas.worldCamera : null;

        EnsureTooltip();
    }

    /// <summary>
    /// Cherche un <see cref="SpellTooltip"/> dans la scène, sinon en crée un minimal au runtime.
    /// </summary>
    void EnsureTooltip()
    {
        if (tooltip != null) return;

        tooltip = FindFirstObjectByType<SpellTooltip>(FindObjectsInactive.Include);
        if (tooltip != null) return;

        // Création d'un tooltip runtime complet sous le canvas parent
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("SpellBarTooltip", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        go.SetActive(false);

        // Panel flottant
        var panelRt  = go.GetComponent<RectTransform>();
        panelRt.sizeDelta        = new Vector2(280f, 0f);
        panelRt.anchorMin        = panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot            = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.10f, 0.96f);

        var outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor    = new Color(0.788f, 0.659f, 0.298f, 0.5f);
        outline.effectDistance = new Vector2(1f, -1f);

        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.padding             = new RectOffset(10, 10, 8, 8);
        vlg.spacing             = 4f;
        vlg.childControlWidth   = true;
        vlg.childControlHeight  = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Ligne header : icône + nom
        var headerGo = new GameObject("Header", typeof(RectTransform));
        headerGo.transform.SetParent(go.transform, false);
        var hHlg = headerGo.AddComponent<HorizontalLayoutGroup>();
        hHlg.spacing           = 8f;
        hHlg.childControlWidth  = false;
        hHlg.childControlHeight = true;
        hHlg.childForceExpandHeight = true;
        hHlg.childAlignment    = TextAnchor.MiddleLeft;
        headerGo.AddComponent<LayoutElement>().preferredHeight = 44f;

        // Icône
        var iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(headerGo.transform, false);
        var iconLe = iconGo.AddComponent<LayoutElement>();
        iconLe.preferredWidth = iconLe.preferredHeight = 40f;
        iconLe.minWidth = iconLe.minHeight = 40f;
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.preserveAspect = true;

        // Colonne nom + meta
        var colGo  = new GameObject("NameCol", typeof(RectTransform));
        colGo.transform.SetParent(headerGo.transform, false);
        colGo.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var colVlg = colGo.AddComponent<VerticalLayoutGroup>();
        colVlg.childControlWidth   = true;
        colVlg.childControlHeight  = true;
        colVlg.childForceExpandWidth  = true;
        colVlg.childForceExpandHeight = false;

        var nameTmp = AddTmpToGo(new GameObject("Name", typeof(RectTransform)), colGo.transform,
            15f, FontStyles.Bold, new Color(0.94f, 0.84f, 0.58f), TextAlignmentOptions.MidlineLeft);
        var paTmp   = AddTmpToGo(new GameObject("PA",   typeof(RectTransform)), colGo.transform,
            11f, FontStyles.Normal, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft);

        // Séparateur
        var sepGo = new GameObject("Sep", typeof(RectTransform));
        sepGo.transform.SetParent(go.transform, false);
        sepGo.AddComponent<LayoutElement>().preferredHeight = 1f;
        sepGo.AddComponent<Image>().color = new Color(0.38f, 0.38f, 0.44f, 0.45f);

        // Description
        var descTmp = AddTmpToGo(new GameObject("Desc", typeof(RectTransform)), go.transform,
            12f, FontStyles.Normal, new Color(0.88f, 0.9f, 0.93f), TextAlignmentOptions.TopLeft);
        descTmp.enableWordWrapping = true;

        // Synergie
        var synTmp = AddTmpToGo(new GameObject("Synergy", typeof(RectTransform)), go.transform,
            11f, FontStyles.Normal, new Color(0.72f, 0.88f, 0.76f), TextAlignmentOptions.TopLeft);
        synTmp.enableWordWrapping = true;

        // Portée + Recharge
        var rangeTmp   = AddTmpToGo(new GameObject("Range",    typeof(RectTransform)), go.transform,
            11f, FontStyles.Normal, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft);
        var cdTmp      = AddTmpToGo(new GameObject("Cooldown", typeof(RectTransform)), go.transform,
            11f, FontStyles.Normal, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft);

        var st = go.AddComponent<SpellTooltip>();
        st.tooltipPanel    = panelRt;
        st.rootCanvas      = canvas;
        st.panelBackdrop   = bg;
        st.iconImage       = iconImg;
        st.spellNameText   = nameTmp;
        st.paCostText      = paTmp;
        st.descriptionText = descTmp;
        st.synergyText     = synTmp;
        st.rangeText       = rangeTmp;
        st.cooldownText    = cdTmp;

        // Style discret pour la barre de sort en combat
        st.backdropColor   = new Color(0.04f, 0.04f, 0.08f, 0.88f);
        st.titleColor      = new Color(1f, 0.90f, 0.65f, 1f);
        st.metaColor       = new Color(0.65f, 0.72f, 0.85f, 1f);
        st.bodyColor       = new Color(0.82f, 0.84f, 0.87f, 1f);
        st.titleOutline    = 0f;

        tooltip = st;
    }

    static TextMeshProUGUI AddTmpToGo(GameObject go, Transform parent, float size, FontStyles style, Color color,
        TextAlignmentOptions align)
    {
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize           = size;
        tmp.fontStyle          = style;
        tmp.color              = color;
        tmp.alignment          = align;
        tmp.raycastTarget      = false;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    void OnEnable()
    {
        // Quand la barre de sorts réapparaît (début de tour / résurrection UI),
        // re-snapper immédiatement les slots sans animation résiduelle.
        if (_activeCharacter != null)
            ApplyBarLayout();
    }

    // Touches &é"'(-è sur AZERTY = Alpha1–Alpha7
    static readonly KeyCode[] SlotKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7
    };

    void Update()
    {
        if (_activeCharacter == null) return;
        HandleMouseHover();
        HandleMouseClick();
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        for (int i = 0; i < SlotKeys.Length; i++)
        {
            if (Input.GetKeyDown(SlotKeys[i]))
            {
                SelectSlot(i);
                return;
            }
        }
    }

    void HandleMouseHover()
    {
        Vector2 mp = Input.mousePosition;

        var visible = CollectVisibleSlots();

        SpellSlotUI hit = null;
        foreach (var s in visible)
        {
            var rt = s.GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mp, _uiCamera))
            {
                hit = s;
                break;
            }
        }

        if (hit == _hoveredSlot) return;

        _hoveredSlot?.ForceExit();
        _hoveredSlot = hit;
        _hoveredSlot?.ForceEnter();
    }

    void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mp      = Input.mousePosition;
        var     visible = CollectVisibleSlots();

        foreach (var s in visible)
        {
            var rt = s.GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mp, _uiCamera))
            {
                SelectSlot(s.SlotIndex);
                return;
            }
        }
    }

    List<SpellSlotUI> CollectVisibleSlots()
    {
        var list = new List<SpellSlotUI>();
        for (int i = 0; i < Mathf.Min(slots.Count, MaxVisibleSlots); i++)
            if (slots[i] != null && slots[i].gameObject.activeSelf)
                list.Add(slots[i]);
        return list;
    }

    // ── Refresh ───────────────────────────────────────────────────────────
    void OnResourceChanged(int current, int max) => RefreshAll();

    void OnCharacterStateChanged(CharacterState state)
    {
        if (state == CharacterState.Idle)
            RefreshAll();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < Mathf.Min(slots.Count, MaxVisibleSlots); i++)
            if (slots[i] != null) slots[i].Refresh();
    }

    // ── Sélection ─────────────────────────────────────────────────────────
    public void SelectSlot(int index)
    {
        if (_activeCaster == null || _activeCharacter == null) return;
        if (index < 0 || index >= Mathf.Min(slots.Count, MaxVisibleSlots)) return;

        SpellSlotUI slot = slots[index];
        if (slot == null || !slot.HasSpell) return;

        if (_selectedSlotIndex == index)
        {
            ClearSelection();
            _activeCaster.CancelSpell();
            return;
        }

        bool ok = _activeCaster.SelectSpell(slot.Spell);
        if (!ok) return;

        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < slots.Count)
            slots[_selectedSlotIndex].SetSelected(false);

        _selectedSlotIndex = index;
        slot.SetSelected(true);
    }

    public void ClearSelection()
    {
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < slots.Count)
            slots[_selectedSlotIndex]?.SetSelected(false);
        _selectedSlotIndex = -1;
    }

    // ── Tooltip ───────────────────────────────────────────────────────────
    public void ShowTooltip(SpellData spell, Vector3 anchorWorldPos)
    {
        if (tooltip != null) tooltip.Show(spell, anchorWorldPos);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.Hide();
    }
}

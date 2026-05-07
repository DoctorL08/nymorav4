using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Slot de sort dans la barre de sorts (spell bar).
/// Remplace l'ancien système de cartes en éventail : chaque slot est un carré d'icône
/// avec badge PA (bas-droite) et numéro de raccourci (haut-gauche).
///
/// Tous les enfants UI sont auto-créés au premier Awake si les références Inspector sont vides.
/// Compatible avec le SetRestPose de DeckUI (angle ignoré — bar slots ne pivotent pas).
/// </summary>
public class SpellSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ── Références Inspector (optionnelles : auto-générées si nulles) ─────
    [Header("Icône")]
    public Image iconImage;
    public Image backgroundImage;

    [Header("Badges")]
    public TextMeshProUGUI paCostText;
    public TextMeshProUGUI hotkeyText;

    [Header("Contour sélection (pulsé)")]
    public Color glowColor      = new Color(1f, 0.78f, 0.22f, 1f);
    public float glowPulseSpeed = 2.2f;
    public float glowMinAlpha   = 0.55f;
    public float glowMaxAlpha   = 1f;
    public float selectionOutlineSize = 3f;

    [Header("Couleurs fond")]
    public Color bgNormal      = new Color(0.08f, 0.08f, 0.12f, 0.95f);
    public Color bgSelected    = new Color(0.22f, 0.17f, 0.06f, 1f);
    public Color bgUnavailable = new Color(0.05f, 0.05f, 0.07f, 0.95f);

    [Header("Couleurs icône")]
    public Color availableColor   = Color.white;
    public Color unavailableColor = new Color(0.32f, 0.32f, 0.32f, 0.85f);

    [Header("Hover / animation")]
    public float hoverScale   = 1.08f;
    public float animDuration = 0.12f;

    // ── Champs de compatibilité héritage (Inspector seulement, inutilisés) ─
    [HideInInspector] public Image cardFrameImage;
    [HideInInspector] public Image dimOverlay;
    [HideInInspector] public Image selectionBorder;
    [HideInInspector] public float hudReferenceCardHeight = 72f;
    [HideInInspector] public float spellSlotHudReferenceHeight = 72f;

    // ── État interne ─────────────────────────────────────────────────────
    SpellData         _spell;
    TacticalCharacter _owner;
    DeckUI            _deckUI;
    int               _slotIndex;
    bool              _isSelected;
    bool              _isHovered;
    bool              _lastCanCast;

    Coroutine _animCoroutine;
    Coroutine _selectionPulseCoroutine;
    Outline   _iconOutline;

    // Pose repos dans la barre (position XY, taille ; angle = toujours 0)
    Vector2 _restPos;
    Vector2 _restSize;
    float   _raiseDelta = 8f;

    bool _bootstrapped;

    // ── Propriétés publiques ──────────────────────────────────────────────
    public SpellData Spell     => _spell;
    public bool      HasSpell  => _spell != null;
    public int       SlotIndex => _slotIndex;

    // ── Pose (appelé par DeckUI lors du layout bar) ───────────────────────
    /// <summary>
    /// Positionne le slot dans la barre. L'angle est ignoré (bar slots : toujours droit).
    /// Annule tout coroutine d'animation en cours pour que la position soit immédiate dès le premier frame
    /// (évite le bug "icônes stackées puis dépliage" au début du combat).
    /// </summary>
    public void SetRestPose(Vector2 pos, float angleDeg, Vector2 size, float raiseDelta)
    {
        _restPos    = pos;
        _restSize   = size;
        _raiseDelta = raiseDelta;

        // Tuer toute animation en cours — position immédiate, sans glissement résiduel
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }

        var rt = GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.localRotation    = Quaternion.identity;
        rt.sizeDelta        = size;
        rt.localScale       = Vector3.one;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────
    void Awake()
    {
        // Supprimer tout Animator hérité (ancienne animation d'éventail)
        var anim = GetComponent<Animator>();
        if (anim != null) Destroy(anim);

        HideCardLegacyElements();
        Bootstrap();
    }

    void OnEnable()
    {
        HideCardLegacyElements();
        Bootstrap();
    }

    /// <summary>
    /// Masque tous les éléments de l'ancien système de cartes (recherche récursive) :
    /// cadre UI_CARTE_SORT, IconArea (conteneur hérité), voile DimOverlay, SelectionBorder.
    /// Appelé avant Bootstrap pour éviter tout flash visuel.
    /// </summary>
    void HideCardLegacyElements()
    {
        // 1. Références explicites
        if (selectionBorder != null) selectionBorder.enabled = false;
        if (cardFrameImage  != null) cardFrameImage.enabled  = false;
        if (dimOverlay      != null) dimOverlay.enabled      = false;

        // 2. Recherche récursive par nom (couvre les cas où les refs sont nulles ou le prefab hérité)
        var all = GetComponentsInChildren<Transform>(true);
        foreach (var child in all)
        {
            if (child == null || child == transform) continue;
            string n = child.name;

            bool isCardElement =
                n.IndexOf("CARTE_SORT",      System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("CardFrame",       System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.Equals("SelectionBorder",  System.StringComparison.OrdinalIgnoreCase) ||
                n.Equals("DimOverlay",       System.StringComparison.OrdinalIgnoreCase) ||
                n.Equals("HoverSheen",       System.StringComparison.OrdinalIgnoreCase);

            if (isCardElement)
            {
                // Masque le composant Image sans désactiver le GameObject
                // (un enfant peut aussi porter des scripts utiles)
                var img = child.GetComponent<Image>();
                if (img != null) img.enabled = false;
                child.gameObject.SetActive(false);
            }
        }
    }

    // ── Bootstrap (construction des enfants si non assignés) ─────────────
    void Bootstrap()
    {
        if (_bootstrapped) return;
        _bootstrapped = true;

        // Fond du slot
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        backgroundImage.color = bgNormal;

        EnsureIconChild();
        EnsurePABadge();
        EnsureHotkeyBadge();
    }

    void EnsureIconChild()
    {
        if (iconImage != null)
        {
            // S'assurer que l'image existante est active et sans sprite CARTE_SORT hérité
            iconImage.gameObject.SetActive(true);
            iconImage.raycastTarget = false;
            return;
        }

        // 1. Cherche "Icon" enfant direct
        Transform tf = transform.Find("Icon");

        // 2. Fallback : l'ancien layout stockait l'icône dans "IconArea/Icon"
        if (tf == null)
        {
            Transform iconArea = transform.Find("IconArea");
            if (iconArea != null)
            {
                tf = iconArea.Find("Icon");
                // Remonter l'icône au niveau du slot pour simplifier le layout
                if (tf != null)
                    tf.SetParent(transform, false);
                // Désactive le conteneur IconArea devenu inutile
                iconArea.gameObject.SetActive(false);
            }
        }

        // 3. Crée l'enfant si rien trouvé
        if (tf == null)
        {
            var go = new GameObject("Icon", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            tf = go.transform;
        }

        // Positionne et configure (remplit 84 % du slot, marges homogènes)
        var rt = tf.GetComponent<RectTransform>() ?? tf.gameObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.08f, 0.08f);
        rt.anchorMax = new Vector2(0.92f, 0.92f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        iconImage = tf.GetComponent<Image>() ?? tf.gameObject.AddComponent<Image>();
        iconImage.sprite        = null;           // Efface tout sprite CARTE_SORT hérité
        iconImage.preserveAspect = true;
        iconImage.raycastTarget  = false;
        tf.gameObject.SetActive(true);
    }

    void EnsurePABadge()
    {
        if (paCostText != null) return;

        Transform tf = transform.Find("PABadge");
        if (tf == null)
        {
            var go = new GameObject("PABadge", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.52f, 0f);
            rt.anchorMax = new Vector2(1f,    0.44f);
            rt.offsetMin = new Vector2(2f, 2f);
            rt.offsetMax = new Vector2(-2f, 0f);
            tf = go.transform;
        }

        paCostText = tf.GetComponent<TextMeshProUGUI>() ?? tf.gameObject.AddComponent<TextMeshProUGUI>();
        paCostText.fontSize      = 11f;
        paCostText.fontStyle     = FontStyles.Bold;
        paCostText.color         = new Color(1f, 0.85f, 0.4f, 1f);
        paCostText.alignment     = TextAlignmentOptions.BottomRight;
        paCostText.raycastTarget = false;
        OracleUIImportantFont.Apply(paCostText);
    }

    void EnsureHotkeyBadge()
    {
        if (hotkeyText != null) return;

        Transform tf = transform.Find("HotkeyBadge");
        if (tf == null)
        {
            var go = new GameObject("HotkeyBadge", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f,    0.56f);
            rt.anchorMax = new Vector2(0.44f, 1f);
            rt.offsetMin = new Vector2(2f, 0f);
            rt.offsetMax = new Vector2(0f, -2f);
            tf = go.transform;
        }

        hotkeyText = tf.GetComponent<TextMeshProUGUI>() ?? tf.gameObject.AddComponent<TextMeshProUGUI>();
        hotkeyText.fontSize      = 10f;
        hotkeyText.fontStyle     = FontStyles.Bold;
        hotkeyText.color         = new Color(0.78f, 0.78f, 0.78f, 0.85f);
        hotkeyText.alignment     = TextAlignmentOptions.TopLeft;
        hotkeyText.raycastTarget = false;
        OracleUIImportantFont.Apply(hotkeyText);
    }

    // ── Initialisation par DeckUI ─────────────────────────────────────────
    public void Setup(SpellData spellData, TacticalCharacter character, DeckUI deck, int index)
    {
        _spell     = spellData;
        _owner     = character;
        _deckUI    = deck;
        _slotIndex = index;

        HideCardLegacyElements();
        Bootstrap();

        if (_spell != null)
        {
            // Icône PixelLab uniquement — on n'utilise PLUS spell.icon (anciens GIF CARTE_SORT)
            Sprite icon = SpellIconLoader.GetIcon(_spell.spellName);

            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.sprite         = icon;
                    iconImage.color          = availableColor;
                    iconImage.preserveAspect = true;
                    iconImage.enabled        = true;
                }
                else
                {
                    // Placeholder couleur catégorie si l'icône n'est pas encore chargée
                    iconImage.sprite  = null;
                    iconImage.color   = CategoryColor(_spell.deckCategory) * new Color(1f, 1f, 1f, 0.60f);
                    iconImage.enabled = true;
                }
            }

            if (paCostText != null) { paCostText.text = $"{_spell.paCost}"; paCostText.enabled = true; }
            if (hotkeyText != null) { hotkeyText.text = $"{index + 1}";     hotkeyText.enabled = true; }
        }
        else
        {
            if (iconImage  != null) { iconImage.sprite = null; iconImage.enabled = false; }
            if (paCostText != null) { paCostText.text  = ""; paCostText.enabled = false; }
            if (hotkeyText != null) { hotkeyText.text  = ""; hotkeyText.enabled = false; }
        }

        SetSelected(false);
        Refresh();
    }

    static Color CategoryColor(SpellDeckCategory cat)
    {
        return cat switch
        {
            SpellDeckCategory.Attack   => new Color(0.75f, 0.25f, 0.20f),
            SpellDeckCategory.Tactic   => new Color(0.25f, 0.45f, 0.75f),
            SpellDeckCategory.Survival => new Color(0.25f, 0.65f, 0.30f),
            _                          => Color.gray,
        };
    }

    // ── Refresh (appelé quand les PA changent ou l'état du personnage) ────
    public void Refresh()
    {
        if (_spell == null || _owner == null) return;
        _lastCanCast = _owner.CanCastSpell(_spell);
        ApplyVisualState();
    }

    void ApplyVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = _isSelected  ? bgSelected
                                  : !_lastCanCast ? bgUnavailable
                                  : bgNormal;
        }

        if (iconImage != null && iconImage.enabled)
        {
            Color c = _lastCanCast ? availableColor : unavailableColor;
            if (_isHovered && _lastCanCast)
            {
                c.r = Mathf.Min(1f, c.r * 1.12f);
                c.g = Mathf.Min(1f, c.g * 1.12f);
                c.b = Mathf.Min(1f, c.b * 1.12f);
            }
            c.a = 1f;
            iconImage.color = c;
        }
    }

    // ── Sélection ────────────────────────────────────────────────────────
    public void SetSelected(bool value)
    {
        _isSelected = value;

        if (selectionBorder != null) selectionBorder.enabled = false;

        if (value)
        {
            EnsureIconOutline();
            if (_iconOutline != null)
            {
                _iconOutline.enabled = true;
                if (_selectionPulseCoroutine != null) StopCoroutine(_selectionPulseCoroutine);
                _selectionPulseCoroutine = StartCoroutine(SelectionOutlinePulse());
            }
            if (!_isHovered)
                AnimateTo(GetComponent<RectTransform>(),
                    _restPos + Vector2.up * _raiseDelta,
                    Quaternion.identity,
                    Vector3.one * hoverScale);
        }
        else
        {
            if (_selectionPulseCoroutine != null)
            {
                StopCoroutine(_selectionPulseCoroutine);
                _selectionPulseCoroutine = null;
            }
            if (_iconOutline != null) _iconOutline.enabled = false;

            if (!_isHovered)
                AnimateTo(GetComponent<RectTransform>(),
                    _restPos, Quaternion.identity, Vector3.one);
        }

        ApplyVisualState();
    }

    void EnsureIconOutline()
    {
        if (_iconOutline != null || iconImage == null) return;
        _iconOutline = iconImage.GetComponent<Outline>()
                    ?? iconImage.gameObject.AddComponent<Outline>();
        _iconOutline.effectDistance  = new Vector2(selectionOutlineSize, -selectionOutlineSize);
        _iconOutline.useGraphicAlpha = false;
        _iconOutline.enabled         = false;
    }

    IEnumerator SelectionOutlinePulse()
    {
        while (_iconOutline != null && _iconOutline.enabled)
        {
            float t = (Mathf.Sin(Time.unscaledTime * glowPulseSpeed) + 1f) * 0.5f;
            float w = Mathf.Lerp(selectionOutlineSize * 0.7f, selectionOutlineSize * 1.4f, t);
            float a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);
            _iconOutline.effectDistance = new Vector2(w, -w);
            _iconOutline.effectColor    = new Color(glowColor.r, glowColor.g, glowColor.b, a);
            yield return null;
        }
    }

    // ── Hover ─────────────────────────────────────────────────────────────
    public void ForceEnter()
    {
        if (_isHovered) return;
        _isHovered = true;

        if (!_isSelected)
            AnimateTo(GetComponent<RectTransform>(),
                _restPos + Vector2.up * _raiseDelta,
                Quaternion.identity,
                Vector3.one * hoverScale);

        ApplyVisualState();

        if (_spell != null && _deckUI != null)
            _deckUI.ShowTooltip(_spell, transform.position);
    }

    public void ForceExit()
    {
        if (!_isHovered) return;
        _isHovered = false;

        if (!_isSelected)
            AnimateTo(GetComponent<RectTransform>(),
                _restPos, Quaternion.identity, Vector3.one);

        ApplyVisualState();
        _deckUI?.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData) => ForceEnter();
    public void OnPointerExit(PointerEventData eventData)  => ForceExit();

    // ── Animation ─────────────────────────────────────────────────────────
    void AnimateTo(RectTransform rt, Vector2 targetPos, Quaternion targetRot, Vector3 targetScale)
    {
        if (!gameObject.activeInHierarchy)
        {
            rt.anchoredPosition = targetPos;
            rt.localRotation    = targetRot;
            rt.localScale       = targetScale;
            return;
        }
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimCoroutine(rt, targetPos, targetRot, targetScale));
    }

    IEnumerator AnimCoroutine(RectTransform rt, Vector2 targetPos, Quaternion targetRot, Vector3 targetScale)
    {
        float      elapsed    = 0f;
        Vector2    startPos   = rt.anchoredPosition;
        Quaternion startRot   = rt.localRotation;
        Vector3    startScale = rt.localScale;

        while (elapsed < animDuration)
        {
            elapsed            += Time.unscaledDeltaTime;
            float t             = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / animDuration));
            rt.anchoredPosition = Vector2.Lerp(startPos,   targetPos,   t);
            rt.localRotation    = Quaternion.Slerp(startRot,  targetRot,  t);
            rt.localScale       = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
        rt.localRotation    = targetRot;
        rt.localScale       = targetScale;
        _animCoroutine      = null;
    }
}

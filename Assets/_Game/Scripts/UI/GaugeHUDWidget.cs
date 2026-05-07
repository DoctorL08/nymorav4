using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Widget de jauge passive (Présence Funeste / futurs passifs évolutifs).
/// Créé au runtime par CombatHUD — haut-gauche pour le joueur, haut-droite pour l'ennemi.
/// N'utilise jamais SetActive(false) sur lui-même : la visibilité passe par un CanvasGroup.
/// </summary>
public class GaugeHUDWidget : MonoBehaviour
{
    const int MaxPips = 8;

    TacticalCharacter _character;
    PassiveManager    _pm;
    bool              _alignRight;

    CanvasGroup     _cg;
    Image[]         _pips;
    TextMeshProUGUI _headerLabel;
    Image           _flashOverlay;

    int _lastJauges = -1;

    static readonly Color ColEmpty   = new Color(0.18f, 0.18f, 0.22f, 0.9f);
    static readonly Color ColBase    = new Color(0.60f, 0.68f, 0.90f, 1f);
    static readonly Color ColEveille = new Color(1.00f, 0.72f, 0.28f, 1f);
    static readonly Color ColEnrage  = new Color(1.00f, 0.22f, 0.22f, 1f);
    static readonly Color ColBg      = new Color(0.04f, 0.04f, 0.09f, 0.88f);

    public TacticalCharacter Character => _character;

    // ──────────────────────────────────────────────────────────────────────
    // Init (appelé par CombatHUD juste après AddComponent)
    // ──────────────────────────────────────────────────────────────────────

    public void Init(bool alignRight)
    {
        _alignRight = alignRight;

        // RectTransform
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rt.anchorMax = rt.anchorMin;
        rt.pivot     = rt.anchorMin;
        rt.anchoredPosition = alignRight ? new Vector2(-10f, -90f) : new Vector2(10f, -90f);
        rt.sizeDelta = new Vector2(116f, 26f);

        // Fond
        var bg = gameObject.AddComponent<Image>();
        bg.color = ColBg;

        // CanvasGroup : contrôle la visibilité sans jamais désactiver le GO
        _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha          = 0f;
        _cg.blocksRaycasts = false;
        _cg.interactable   = false;

        // Layout
        var csf = gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth      = vlg.childControlHeight     = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(4, 4, 2, 2);

        BuildHeaderRow();
        BuildPipsRow();
        BuildFlashOverlay();
    }

    void BuildHeaderRow()
    {
        var row = MakeRowGo("HeaderRow", 10f);
        _headerLabel = MakeTMP(row, "Header", 7f, FontStyles.Bold,
            new Color(0.788f, 0.659f, 0.298f),
            _alignRight ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
        _headerLabel.enableWordWrapping = false;
        _headerLabel.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        _headerLabel.text = "Vol d'Âme";
    }

    void BuildPipsRow()
    {
        var rowGo = MakeRowGo("PipsRow", 9f);
        var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth     = true;
        hlg.childControlHeight    = false;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.spacing        = 2f;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        _pips = new Image[MaxPips];
        for (int i = 0; i < MaxPips; i++)
        {
            var pipGo = new GameObject($"Pip{i}", typeof(RectTransform));
            pipGo.transform.SetParent(rowGo.transform, false);
            var le = pipGo.AddComponent<LayoutElement>();
            le.minHeight       = 7f;
            le.preferredHeight = 7f;
            le.flexibleWidth   = 1f;
            var img = pipGo.AddComponent<Image>();
            img.color = ColEmpty;
            img.raycastTarget = false;
            _pips[i] = img;
        }
    }

    void BuildFlashOverlay()
    {
        var go = new GameObject("FlashOverlay", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var frt = (RectTransform)go.transform;
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = frt.offsetMax = Vector2.zero;
        _flashOverlay = go.AddComponent<Image>();
        _flashOverlay.color = Color.clear;
        _flashOverlay.raycastTarget = false;
    }

    // ──────────────────────────────────────────────────────────────────────
    // Liaison personnage (peut être appelé en différé)
    // ──────────────────────────────────────────────────────────────────────

    public void SetCharacter(TacticalCharacter character)
    {
        _character  = character;
        _pm         = character != null ? character.GetComponent<PassiveManager>() : null;
        _lastJauges = -1;
    }

    // ──────────────────────────────────────────────────────────────────────
    // Update — tourne toujours (le GO reste actif)
    // ──────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (_cg == null) return;

        bool relevant = _pm != null
                        && _pm.IsNymoraPassive
                        && (_character == null || _character.IsAlive);

        float targetAlpha = relevant ? 1f : 0f;
        _cg.alpha          = targetAlpha;
        _cg.blocksRaycasts = relevant;

        if (!relevant || _pm == null) return;

        int jauges = _pm.NymoraJauges;
        if (jauges == _lastJauges) return;

        bool gained = _lastJauges >= 0 && jauges > _lastJauges;
        int  newIdx = jauges - 1;
        _lastJauges = jauges;

        RefreshDisplay(jauges);

        if (gained) StartCoroutine(ProcFlash(newIdx));
    }

    void RefreshDisplay(int jauges)
    {
        int   palier     = _pm != null ? _pm.NymoPalier : 0;
        int   maxJauges  = _pm != null ? _pm.NymoraMaxJauges : 8;
        Color col        = PipColorForPalier(palier);

        for (int i = 0; i < MaxPips; i++)
            if (_pips[i] != null)
                _pips[i].color = i >= maxJauges ? Color.clear : i < jauges ? col : ColEmpty;

        if (_headerLabel != null)
        {
            string passiveName = _pm?.activePassive?.passiveName ?? "Jauge";
            string palierName  = palier == 2 ? "ENRAGE" : palier == 1 ? "Eveille" : "Base";
            _headerLabel.text  = $"{passiveName}  {jauges}/{maxJauges}  — {palierName}";
            _headerLabel.color = col;
        }
    }

    IEnumerator ProcFlash(int pipIdx)
    {
        // Scale bounce sur le pip qui vient de s'allumer
        if (pipIdx >= 0 && pipIdx < MaxPips && _pips[pipIdx] != null)
        {
            var tf = _pips[pipIdx].transform;
            float dur = 0.30f, e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                tf.localScale = Vector3.one * Mathf.Lerp(2.0f, 1f, Mathf.SmoothStep(0f, 1f, e / dur));
                yield return null;
            }
            tf.localScale = Vector3.one;
        }

        // Flash coloré sur le fond du widget
        if (_flashOverlay != null)
        {
            int   palier = _pm != null ? _pm.NymoPalier : 0;
            Color fc     = new Color(PipColorForPalier(palier).r, PipColorForPalier(palier).g,
                                     PipColorForPalier(palier).b, 0.40f);
            float dur = 0.28f, e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                _flashOverlay.color = Color.Lerp(fc, Color.clear, e / dur);
                yield return null;
            }
            _flashOverlay.color = Color.clear;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────

    static Color PipColorForPalier(int palier) =>
        palier >= 2 ? ColEnrage : palier >= 1 ? ColEveille : ColBase;

    GameObject MakeRowGo(string name, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.flexibleWidth   = 1f;
        return go;
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string goName, float size,
        FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize      = size;
        t.fontStyle     = style;
        t.color         = color;
        t.alignment     = align;
        t.raycastTarget = false;
        go.AddComponent<LayoutElement>().flexibleWidth = 1f;
        return t;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellTooltip : MonoBehaviour
{
    // =========================================================
    // RÉFÉRENCES
    // =========================================================
    [Header("Textes")]
    public TextMeshProUGUI spellNameText;
    public TextMeshProUGUI paCostText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI synergyText;

    [Header("Icône")]
    public Image iconImage;

    [Header("Panel")]
    public RectTransform tooltipPanel;
    public Canvas rootCanvas;
    [Tooltip("Fond du panneau (auto si vide : cherche sur tooltipPanel).")]
    public Image panelBackdrop;

    [Header("Style")]
    public Color titleColor = new Color(0.94f, 0.84f, 0.58f, 1f);
    public Color metaColor  = new Color(0.72f, 0.78f, 0.88f, 1f);
    public Color bodyColor  = new Color(0.88f, 0.9f, 0.93f, 1f);
    public Color synergyColor = new Color(0.72f, 0.88f, 0.76f, 1f);
    public Color backdropColor = new Color(0.05f, 0.06f, 0.1f, 0.93f);
    [Range(0f, 0.4f)] public float titleOutline = 0.15f;

    void Awake() { }

    // =========================================================
    // AFFICHAGE
    // =========================================================
    public void Show(SpellData spell, Vector3 anchorWorldPos)
    {
        gameObject.SetActive(true);

        ApplyBackdrop();

        if (spellNameText != null)
        {
            spellNameText.text = spell.spellName;
            spellNameText.color = titleColor;
            spellNameText.fontStyle = FontStyles.Bold;
            spellNameText.outlineWidth = titleOutline;
            spellNameText.outlineColor = new Color(0f, 0f, 0f, 0.55f);
        }
        if (paCostText != null)
        {
            paCostText.text  = $"Coût : {spell.paCost} PA";
            paCostText.color = metaColor;
        }
        if (descriptionText != null)
        {
            descriptionText.text  = spell.description;
            descriptionText.color = bodyColor;
        }
        if (synergyText != null)
        {
            bool hasSynergy = !string.IsNullOrEmpty(spell.synergyDescription);
            synergyText.gameObject.SetActive(hasSynergy);
            synergyText.text  = hasSynergy ? $"Synergie : {spell.synergyDescription}" : "";
            synergyText.color = synergyColor;
        }

        if (rangeText != null)
        {
            if (spell.isMeleeOnly)
                rangeText.text = "Portée : Corps à corps";
            else if (spell.zoneType == ZoneType.Self || (spell.rangeMin == 0 && spell.rangeMax == 0))
                rangeText.text = "Portée : Soi-même";
            else
                rangeText.text = $"Portée : {spell.rangeMin}–{spell.rangeMax}";
            rangeText.color = metaColor;
            rangeText.gameObject.SetActive(true);
        }

        if (cooldownText != null)
        {
            if (spell.cooldown > 0)
            {
                cooldownText.text  = $"Recharge : {spell.cooldown} tour{(spell.cooldown > 1 ? "s" : "")}";
                cooldownText.color = metaColor;
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }

        if (iconImage != null)
        {
            // Priorité icône PixelLab, pas de fallback vers les anciens GIF CARTE_SORT
            Sprite icon = SpellIconLoader.GetIcon(spell.spellName);
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
            if (icon != null) iconImage.preserveAspect = true;
        }

        PositionNearAnchor(anchorWorldPos);
    }

    void ApplyBackdrop()
    {
        var bg = panelBackdrop;
        if (bg == null && tooltipPanel != null)
            bg = tooltipPanel.GetComponent<Image>();
        if (bg != null)
        {
            panelBackdrop = bg;
            bg.color = backdropColor;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // =========================================================
    // POSITIONNEMENT (au-dessus du slot, reste dans l'écran)
    // =========================================================
    private void PositionNearAnchor(Vector3 worldPos)
    {
        if (tooltipPanel == null || rootCanvas == null) return;

        Vector2 screenPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            rootCanvas.worldCamera,
            out screenPos
        );

        float lift = Mathf.Max(72f, tooltipPanel.rect.height * 0.35f + 32f);
        screenPos.y += lift;

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        float halfW = tooltipPanel.rect.width  * 0.5f;
        float halfH = tooltipPanel.rect.height * 0.5f;
        float margin = 16f;
        screenPos.x = Mathf.Clamp(screenPos.x,
            -canvasRect.rect.width  * 0.5f + halfW + margin,
            canvasRect.rect.width  * 0.5f - halfW - margin);
        screenPos.y = Mathf.Clamp(screenPos.y,
            -canvasRect.rect.height * 0.5f + halfH + margin,
            canvasRect.rect.height * 0.5f - halfH - margin);

        tooltipPanel.anchoredPosition = screenPos;
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Texte flottant de combat (-X dégâts / +X soins / +X bouclier) en espace monde, police Aseprite.
/// Les trois types spawent à des hauteurs différentes pour ne jamais se chevaucher visuellement.
/// Usage : FloatingDamageText.SpawnDamage / SpawnHeal / SpawnShield
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    static readonly Color ColorDamage = new Color(1f,    0.38f, 0.38f);
    static readonly Color ColorHeal   = new Color(0.42f, 1f,    0.52f);
    static readonly Color ColorShield = new Color(0.45f, 0.82f, 1f);
    static readonly Color ColorPA     = new Color(1f,    0.80f, 0.20f);   // or
    static readonly Color ColorPM     = new Color(0.55f, 0.75f, 1f);      // bleu clair

    // Signature spell
    static readonly Color ColorSigFlash  = Color.white;
    static readonly Color ColorSigGold   = new Color(1f,  0.85f, 0f);
    static readonly Color ColorSigFire   = new Color(1f,  0.22f, 0.05f);
    static readonly Color ColorSigStar   = new Color(1f,  0.75f, 0.10f);

    const float Duration      = 1.1f;
    const float RiseDistance  = 1.4f;
    const float PeakScale     = 1.35f;
    const float FontSize      = 5f;
    const float FontSizeSmall = 3f;  // coût PA (cast)

    const float SigDuration    = 2.1f;
    const float SigRise        = 2.6f;
    const float SigFontSize    = 9.5f;
    const float SigPeakScale   = 3.4f;
    const float SigSettleScale = 1.85f;

    // PA/PM en bas, dégâts au-dessus, soin plus haut, bouclier en tête
    const float OffsetPA     = 0.0f;
    const float OffsetPM     = 0.0f;
    const float OffsetDamage = 0.6f;
    const float OffsetHeal   = 1.15f;
    const float OffsetShield = 1.70f;

    TextMeshPro _label;
    Color       _baseColor;
    bool        _isSignature;

    // =========================================================
    // API PUBLIQUE
    // =========================================================
    public static void SpawnDamage(int amount, Vector3 worldPos) =>
        Create($"-{amount}", ColorDamage, worldPos, OffsetDamage);

    public static void SpawnHeal(int amount, Vector3 worldPos) =>
        Create($"+{amount}", ColorHeal, worldPos, OffsetHeal);

    public static void SpawnShield(int amount, Vector3 worldPos) =>
        Create($"+{amount}", ColorShield, worldPos, OffsetShield);

    public static void SpawnPACost(int amount, Vector3 worldPos) =>
        Create($"-{amount} PA", ColorPA, worldPos, OffsetPA, FontSizeSmall);

    public static void SpawnPALoss(int amount, Vector3 worldPos) =>
        Create($"-{amount} PA", ColorPA, worldPos, OffsetPA);

    public static void SpawnPMLoss(int amount, Vector3 worldPos) =>
        Create($"-{amount} PM", ColorPM, worldPos, OffsetPM);

    public static void SpawnPAGain(int amount, Vector3 worldPos) =>
        Create($"+{amount} PA", ColorPA, worldPos, OffsetPA);

    public static void SpawnPMGain(int amount, Vector3 worldPos) =>
        Create($"+{amount} PM", ColorPM, worldPos, OffsetPM);

    public static void SpawnSignatureDamage(int amount, Vector3 worldPos)
    {
        // Nombre principal — gros, dramatique
        float jitter = Random.Range(-0.12f, 0.12f);
        var go = new GameObject("FloatingDmgSig");
        go.transform.position = worldPos + new Vector3(jitter, OffsetDamage, 0f);
        go.transform.localScale = Vector3.zero;

        var tmp       = go.AddComponent<TextMeshPro>();
        tmp.text      = $"-{amount}";
        tmp.color     = ColorSigFlash;
        tmp.fontSize  = SigFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        var font = OracleUIImportantFont.GetFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 10000;

        var fdt          = go.AddComponent<FloatingDamageText>();
        fdt._label       = tmp;
        fdt._baseColor   = ColorSigFire;
        fdt._isSignature = true;

        // Étoiles latérales — petites, disparaissent vite
        SpawnSigStar(worldPos + new Vector3(-0.55f, OffsetDamage + 0.25f, 0f));
        SpawnSigStar(worldPos + new Vector3( 0.55f, OffsetDamage + 0.25f, 0f));
    }

    static void SpawnSigStar(Vector3 pos)
    {
        var go  = new GameObject("SigStar");
        go.transform.position = pos;

        var tmp       = go.AddComponent<TextMeshPro>();
        tmp.text      = "[!]";
        tmp.color     = ColorSigStar;
        tmp.fontSize  = 4.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        var font = OracleUIImportantFont.GetFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9998;

        var fdt        = go.AddComponent<FloatingDamageText>();
        fdt._label     = tmp;
        fdt._baseColor = ColorSigStar;
    }

    // =========================================================
    // CRÉATION
    // =========================================================
    static void Create(string text, Color color, Vector3 worldPos, float offsetY, float fontSize = FontSize)
    {
        float jitter = Random.Range(-0.25f, 0.25f);
        var go = new GameObject("FloatingDmgText");
        go.transform.position = worldPos + new Vector3(jitter, offsetY, 0f);

        var tmp       = go.AddComponent<TextMeshPro>();
        tmp.text      = text;
        tmp.color     = color;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        var font = OracleUIImportantFont.GetFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9999;

        var fdt        = go.AddComponent<FloatingDamageText>();
        fdt._label     = tmp;
        fdt._baseColor = color;
    }

    // =========================================================
    // ANIMATION
    // =========================================================
    void Start() => StartCoroutine(_isSignature ? AnimateSignature() : Animate());

    IEnumerator Animate()
    {
        float   elapsed  = 0f;
        Vector3 startPos = transform.position;

        transform.localScale = Vector3.one * PeakScale;

        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / Duration;

            // Montée régulière
            transform.position = startPos + Vector3.up * (RiseDistance * t);

            // Pop-in rapide puis retour à l'échelle normale
            float scaleT = Mathf.Clamp01(elapsed / 0.12f);
            transform.localScale = Vector3.one * Mathf.Lerp(PeakScale, 1f, scaleT);

            // Fade-out sur la seconde moitié
            float alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
            _label.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator AnimateSignature()
    {
        float   elapsed  = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < SigDuration)
        {
            elapsed += Time.deltaTime;
            float n = elapsed / SigDuration;

            // Montée rapide au début puis ralentit (courbe racine)
            float rise = SigRise * Mathf.Sqrt(n);
            Vector3 pos = startPos + Vector3.up * rise;

            // Tremblement horizontal sur les 0.30 premières secondes — impact physique
            if (elapsed < 0.30f)
            {
                float shakeDecay = 1f - elapsed / 0.30f;
                pos.x += Mathf.Sin(elapsed * 95f) * 0.26f * shakeDecay;
            }
            transform.position = pos;

            // Échelle : claque à SigPeakScale en 0.05s → retombe à SigSettleScale en 0.15s
            float scale;
            if      (elapsed < 0.05f) scale = Mathf.LerpUnclamped(0f, SigPeakScale, elapsed / 0.05f);
            else if (elapsed < 0.20f) scale = Mathf.Lerp(SigPeakScale, SigSettleScale, (elapsed - 0.05f) / 0.15f);
            else                      scale = SigSettleScale;
            transform.localScale = Vector3.one * scale;

            // Couleur : flash blanc → or → rouge feu
            Color col;
            if      (elapsed < 0.05f) col = Color.Lerp(ColorSigFlash, ColorSigGold, elapsed / 0.05f);
            else if (elapsed < 0.35f) col = Color.Lerp(ColorSigGold,  ColorSigFire, (elapsed - 0.05f) / 0.30f);
            else                      col = ColorSigFire;

            // Fade : démarre à 70 % de la durée
            float alpha = n > 0.70f ? Mathf.Lerp(1f, 0f, (n - 0.70f) / 0.30f) : 1f;
            col.a = alpha;
            _label.color = col;

            yield return null;
        }

        Destroy(gameObject);
    }
}

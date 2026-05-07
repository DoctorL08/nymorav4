using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

/// <summary>
/// Charge les icônes de sorts depuis Resources/SpellIcons/ghostra/ et Resources/SpellIcons/soulrender/.
/// Utilise Resources.Load&lt;Texture2D&gt; + Sprite.Create pour fonctionner sans importer les PNG
/// en type Sprite (compatible avec l'import par défaut).
/// Appelé automatiquement avant le chargement de la première scène.
/// </summary>
public static class SpellIconLoader
{
    // slug → Sprite (clés en minuscules sans accents)
    static readonly Dictionary<string, Sprite> _cache =
        new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);

    static bool _initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInit() => Initialize();

    /// <summary>Force le chargement (idempotent).</summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        LoadDir("SpellIcons/ghostra");
        LoadDir("SpellIcons/soulrender");

        Debug.Log($"[SpellIconLoader] {_cache.Count} icônes chargées (ghostra + soulrender).");
    }

    static void LoadDir(string resourcesPath)
    {
        // Tentative 1 : importés comme Sprite (textureType = 8, après SpellIconImporter)
        var sprites = Resources.LoadAll<Sprite>(resourcesPath);
        if (sprites != null && sprites.Length > 0)
        {
            foreach (var s in sprites)
                if (s != null) _cache[s.name] = s;
            Debug.Log($"[SpellIconLoader] '{resourcesPath}' → {sprites.Length} sprites chargés via Sprite.");
            return;
        }

        // Tentative 2 : importés comme Texture2D (import par défaut Unity)
        var textures = Resources.LoadAll<Texture2D>(resourcesPath);
        if (textures != null && textures.Length > 0)
        {
            foreach (var tex in textures)
            {
                if (tex == null) continue;
                var sprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);
                sprite.name      = tex.name;
                _cache[tex.name] = sprite;
            }
            Debug.Log($"[SpellIconLoader] '{resourcesPath}' → {textures.Length} textures converties en Sprite.");
        }
        else
        {
            Debug.LogWarning($"[SpellIconLoader] Aucun asset trouvé dans Resources/{resourcesPath}. " +
                             "Vérifier que les PNG sont bien dans Assets/Resources/SpellIcons/ghostra/ et soulrender/.");
        }
    }

    // ── API publique ─────────────────────────────────────────────────────

    /// <summary>
    /// Retourne le sprite correspondant au nom de sort (normalisation automatique des accents / espaces).
    /// Ex. "Saignée (ENRAGÉ)" → cherche "saignee-enrage".
    /// </summary>
    public static Sprite GetIcon(string spellName)
    {
        if (string.IsNullOrEmpty(spellName)) return null;
        if (!_initialized) Initialize();
        _cache.TryGetValue(Slugify(spellName), out var sprite);
        return sprite;
    }

    /// <summary>Lookup direct par slug (= nom de fichier sans extension, ex. "lame-spectrale").</summary>
    public static Sprite GetIconBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return null;
        if (!_initialized) Initialize();
        _cache.TryGetValue(slug, out var sprite);
        return sprite;
    }

    /// <summary>
    /// Convertit un nom de sort en slug de fichier.
    /// "Lame Vorace Spectrale" → "lame-vorace-spectrale"
    /// "Saignée (ENRAGÉ)"     → "saignee-enrage"
    /// </summary>
    public static string Slugify(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";

        // Retire le contenu entre parenthèses
        int paren = name.IndexOf('(');
        if (paren >= 0) name = name.Substring(0, paren);

        name = name.Trim().ToLowerInvariant();

        // Retire les diacritiques (é→e, â→a, etc.)
        name = RemoveDiacritics(name);

        // Remplace espaces, apostrophes et tirets multiples par un seul tiret
        name = name.Replace(' ', '-')
                   .Replace('\'', '-')
                   .Replace('\u2019', '-'); // apostrophe courbe

        // Supprime tout caractère non alphanumérique sauf le tiret
        name = Regex.Replace(name, @"[^a-z0-9\-]", "");

        // Fusionne les tirets successifs
        name = Regex.Replace(name, @"-+", "-");

        return name.Trim('-');
    }

    static string RemoveDiacritics(string text)
    {
        var norm = text.Normalize(NormalizationForm.FormD);
        var sb   = new StringBuilder(norm.Length);
        foreach (char c in norm)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

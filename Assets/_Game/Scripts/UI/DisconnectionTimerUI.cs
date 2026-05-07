using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Phase 1.3 — UI affichée pendant qu'un joueur est déconnecté en plein combat.
/// Compteur 60s avant forfait automatique. Si le joueur revient, <see cref="Hide"/>
/// est appelé et l'UI est détruite sans déclencher le timeout.
///
/// Utilisation typique (depuis <c>OracleCombatNetBridge</c>) :
/// <code>
/// DisconnectionTimerUI.Show("Adversaire", 60f, OnForfeitTimeout);
/// // ...plus tard si reconnexion :
/// DisconnectionTimerUI.Hide();
/// </code>
///
/// L'UI est créée procéduralement (pas de prefab requis) et s'attache au premier
/// <see cref="Canvas"/> de la scène, comme <see cref="CombatInitializer.ShowReturnToHubButton"/>.
/// </summary>
public class DisconnectionTimerUI : MonoBehaviour
{
    static DisconnectionTimerUI _instance;

    TextMeshProUGUI _titleLabel;
    TextMeshProUGUI _countdownLabel;
    Coroutine       _countdown;
    Action          _onTimeout;

    /// <summary>Affiche le compteur. Si déjà visible, met simplement à jour le texte.</summary>
    public static void Show(string playerName, float seconds, Action onTimeout)
    {
        if (_instance == null)
            _instance = Build();
        _instance.Begin(playerName, seconds, onTimeout);
    }

    /// <summary>Masque le compteur et annule le timeout (cas reconnexion).</summary>
    public static void Hide()
    {
        if (_instance == null) return;
        _instance.Cancel();
        Destroy(_instance.gameObject);
        _instance = null;
    }

    /// <summary>Vrai si un compteur est en cours d'affichage.</summary>
    public static bool IsActive => _instance != null;

    static DisconnectionTimerUI Build()
    {
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogWarning("[DisconnectionTimerUI] Aucun Canvas trouvé dans la scène — UI non affichée.");
            return null;
        }

        // Panneau plein écran semi-transparent
        var rootGo = new GameObject("DisconnectionTimerUI", typeof(RectTransform));
        rootGo.transform.SetParent(canvas.transform, false);

        var rootRt = (RectTransform)rootGo.transform;
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;
        // S'assurer que ce panneau passe au-dessus du HUD existant.
        rootGo.transform.SetAsLastSibling();

        var bg = rootGo.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);
        bg.raycastTarget = true;   // bloque les clics derrière le panneau

        var ui = rootGo.AddComponent<DisconnectionTimerUI>();

        // Titre
        var titleGo = new GameObject("Title", typeof(RectTransform));
        titleGo.transform.SetParent(rootGo.transform, false);
        var titleRt = (RectTransform)titleGo.transform;
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot     = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(800f, 60f);
        titleRt.anchoredPosition = new Vector2(0f, 60f);

        ui._titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
        ui._titleLabel.fontSize  = 32f;
        ui._titleLabel.fontStyle = FontStyles.Bold;
        ui._titleLabel.color     = new Color(0.95f, 0.85f, 0.55f);
        ui._titleLabel.alignment = TextAlignmentOptions.Center;
        ui._titleLabel.raycastTarget = false;

        // Compteur
        var cdGo = new GameObject("Countdown", typeof(RectTransform));
        cdGo.transform.SetParent(rootGo.transform, false);
        var cdRt = (RectTransform)cdGo.transform;
        cdRt.anchorMin = new Vector2(0.5f, 0.5f);
        cdRt.anchorMax = new Vector2(0.5f, 0.5f);
        cdRt.pivot     = new Vector2(0.5f, 0.5f);
        cdRt.sizeDelta = new Vector2(400f, 80f);
        cdRt.anchoredPosition = new Vector2(0f, -10f);

        ui._countdownLabel = cdGo.AddComponent<TextMeshProUGUI>();
        ui._countdownLabel.fontSize  = 64f;
        ui._countdownLabel.fontStyle = FontStyles.Bold;
        ui._countdownLabel.color     = new Color(1f, 0.4f, 0.4f);
        ui._countdownLabel.alignment = TextAlignmentOptions.Center;
        ui._countdownLabel.raycastTarget = false;

        // Sous-titre (info forfait)
        var subGo = new GameObject("SubLabel", typeof(RectTransform));
        subGo.transform.SetParent(rootGo.transform, false);
        var subRt = (RectTransform)subGo.transform;
        subRt.anchorMin = new Vector2(0.5f, 0.5f);
        subRt.anchorMax = new Vector2(0.5f, 0.5f);
        subRt.pivot     = new Vector2(0.5f, 0.5f);
        subRt.sizeDelta = new Vector2(800f, 40f);
        subRt.anchoredPosition = new Vector2(0f, -80f);

        var sub = subGo.AddComponent<TextMeshProUGUI>();
        sub.text      = "Forfait automatique à zéro.";
        sub.fontSize  = 18f;
        sub.color     = new Color(0.85f, 0.85f, 0.85f);
        sub.alignment = TextAlignmentOptions.Center;
        sub.raycastTarget = false;

        return ui;
    }

    void Begin(string playerName, float seconds, Action onTimeout)
    {
        Cancel();
        _onTimeout = onTimeout;
        if (_titleLabel != null)
            _titleLabel.text = $"{playerName} déconnecté";
        _countdown = StartCoroutine(CountdownRoutine(seconds));
    }

    IEnumerator CountdownRoutine(float seconds)
    {
        float remaining = seconds;
        while (remaining > 0f)
        {
            if (_countdownLabel != null)
                _countdownLabel.text = Mathf.CeilToInt(remaining).ToString() + " s";
            yield return null;
            remaining -= Time.deltaTime;
        }
        if (_countdownLabel != null) _countdownLabel.text = "0 s";
        var cb = _onTimeout;
        _onTimeout = null;
        cb?.Invoke();
    }

    void Cancel()
    {
        if (_countdown != null)
        {
            StopCoroutine(_countdown);
            _countdown = null;
        }
        _onTimeout = null;
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}

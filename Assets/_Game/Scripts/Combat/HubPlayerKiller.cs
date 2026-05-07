using System.Collections;
using UnityEngine;

/// <summary>
/// Composant à attacher dans une scène (ex : Ranked1v1) qui détruit en boucle pendant
/// <see cref="duration"/> secondes tout <see cref="HubPlayerAvatar"/> qui apparaît.
/// Filet de sécurité autonome contre les avatars réseau qui arrivent après le chargement
/// de scène, indépendamment de <see cref="CombatInitializer"/>.
/// </summary>
public class HubPlayerKiller : MonoBehaviour
{
    [Tooltip("Durée totale du polling de cleanup, en secondes.")]
    public float duration = 5f;

    [Tooltip("Intervalle entre deux passes de cleanup, en secondes.")]
    public float pollInterval = 0.1f;

    void Awake()
    {
        StartCoroutine(NukeRoutine());
    }

    IEnumerator NukeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            var stragglers = FindObjectsByType<HubPlayerAvatar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var hub in stragglers)
            {
                if (hub == null) continue;
                Debug.LogWarning($"[HubPlayerKiller] HubPlayer rampant détruit : {hub.name}");
                Destroy(hub.gameObject);
            }
            yield return new WaitForSeconds(pollInterval);
            elapsed += pollInterval;
        }
    }
}

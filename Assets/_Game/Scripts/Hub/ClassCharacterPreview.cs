using UnityEngine;

/// <summary>
/// Plus nécessaire — le PlayerAnimator se met désormais à jour automatiquement
/// quand la classe change (via HubManager.OnClassChanged).
/// Supprime ce composant de tes scènes.
/// </summary>
public class ClassCharacterPreview : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning("[ClassCharacterPreview] Ce composant est obsolète. Supprime-le de la scène — le PlayerAnimator gère maintenant le changement de classe automatiquement.", this);
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Outil éditeur : Tools → Nymora → Inject HubPlayerKiller in Ranked1v1
///
/// Ouvre la scène Ranked1v1, y ajoute un GameObject "HubPlayerKiller" portant le composant
/// <see cref="HubPlayerKiller"/> si celui-ci n'est pas déjà présent, puis sauvegarde la scène.
/// </summary>
public static class InjectHubPlayerKillerInRanked1v1
{
    private const string ScenePath = "Assets/_Game/Scenes/Ranked1v1.unity";

    [MenuItem("Tools/Nymora/Inject HubPlayerKiller in Ranked1v1")]
    public static void Inject()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogError($"[InjectHubPlayerKiller] Scène introuvable : {ScenePath}. Vérifie son chemin dans le projet.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var existing = Object.FindAnyObjectByType<HubPlayerKiller>();
        if (existing != null)
        {
            Debug.Log($"[InjectHubPlayerKiller] HubPlayerKiller déjà présent dans la scène sur : {existing.gameObject.name}");
            return;
        }

        var go = new GameObject("HubPlayerKiller");
        go.AddComponent<HubPlayerKiller>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[InjectHubPlayerKiller] HubPlayerKiller injecté et scène sauvegardée avec succès.");
    }
}

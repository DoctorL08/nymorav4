using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestionnaire persistant du hub — point central pour les transitions de scène.
/// Survit au changement de scène (DontDestroyOnLoad) pour que CombatInitializer
/// puisse détecter sa présence et afficher le bouton "Retour au Hub".
/// </summary>
public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }

    [Header("Scènes")]
    [Tooltip("Nom de la scène de combat pour l'entraînement solo (vs IA).")]
    public string trainingSceneName = "Training";
    [Tooltip("Nom de cette scène hub.")]
    public string hubSceneName = "Hub";

    /// <summary>
    /// Deck personnalisé sélectionné dans le hub.
    /// Si non null et contient 6 sorts, il est appliqué au joueur en combat
    /// (via CombatInitializer.ApplyRandomSpellDecksIfConfigured).
    /// </summary>
    public List<SpellData> SelectedDeck { get; set; }

    /// <summary>Déclenché quand le joueur change de classe dans le deck builder.</summary>
    public static event Action<ClassData> OnClassChanged;

    private ClassData _selectedClass;
    /// <summary>
    /// Classe Nymora sélectionnée dans le hub.
    /// Le passif et le pool de sorts sont déduits de cette classe au lancement du combat.
    /// </summary>
    public ClassData SelectedClass
    {
        get => _selectedClass;
        set
        {
            _selectedClass = value;
            OnClassChanged?.Invoke(value);
            PushSelectedClassToPhoton();
        }
    }

    /// <summary>
    /// Diffuse la classe locale via Photon CustomProperties pour que les avatars distants
    /// (Hub) et CombatInitializer (Ranked1v1) puissent la résoudre.
    /// </summary>
    public static void PushSelectedClassToPhoton()
    {
        if (Instance?._selectedClass == null) return;
        if (!Photon.Pun.PhotonNetwork.InRoom) return;
        var props = new ExitGames.Client.Photon.Hashtable
        {
            { "classId", (int)Instance._selectedClass.classId }
        };
        Photon.Pun.PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Sélectionner Ghostra par défaut si aucune classe n'a encore été choisie.
        // On passe par le setter pour déclencher OnClassChanged et PushSelectedClassToPhoton
        // (PushSelectedClassToPhoton ne fait rien si on n'est pas encore en room, ce qui est
        //  normal ici — HubNetworkSpawner.OnJoinedRoom s'en chargera au bon moment).
        if (_selectedClass == null)
            SelectedClass = LoadClassById(NymoraClassId.Ghostra);
    }

    /// <summary>Charge un ClassData depuis le ClassRegistry par son identifiant de classe.</summary>
    public static ClassData LoadClassById(NymoraClassId id)
    {
        var registry = Resources.Load<NymoraClassRegistry>("NymoraClasses/ClassRegistry");
        if (registry?.classes == null) return null;
        foreach (var cls in registry.classes)
            if (cls != null && cls.classId == id) return cls;
        return null;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Lance le mode entraînement (combat solo vs IA).</summary>
    public void LaunchTraining()
    {
        Debug.Log($"[HubManager] Lancement entraînement → {trainingSceneName}");
        SceneManager.LoadScene(trainingSceneName);
    }

    /// <summary>Retourne au hub depuis n'importe quelle scène (statique pour usage depuis CombatInitializer).</summary>
    public static void ReturnToHub()
    {
        string hub = Instance != null ? Instance.hubSceneName : "Hub";
        Debug.Log($"[HubManager] Retour au hub → {hub}");
        SceneManager.LoadScene(hub);
    }
}

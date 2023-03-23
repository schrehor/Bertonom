using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private int sceneToLoad = -1;
    [SerializeField] private DestinationIdentifier destPortal;
    [SerializeField] private Transform spawnPoint;

    private PlayerController _player;
    private Fader _fader;

    private void Start()
    {
        _fader = FindObjectOfType<Fader>();
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        this._player = player;
        StartCoroutine(SwitchScene());
    }
    

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        yield return _fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var targetPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destPortal == this.destPortal);
        _player.Character.SetPositionAndSnapToTile(targetPortal.SpawnPoint.position);

        yield return _fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B, C, D, E, F, G, H, I, J, K}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Teleports the player to a different position without switching scenes
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] DestinationIdentifier destPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;
    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destPortal == this.destPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }

    public Transform SpawnPoint => spawnPoint;
}

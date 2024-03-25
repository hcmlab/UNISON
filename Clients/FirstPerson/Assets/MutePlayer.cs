using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutePlayer : MonoBehaviour
{
    public World.WorldManager worldManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && GameObject.Find("MasterManager(Clone)") == null)
        {
            if (other.transform.parent.TryGetComponent<PUN.PunPlayer>(out PUN.PunPlayer player))
            {
                Debug.Log("Player");
                if (!player.IsMine && worldManager.DayState != World.DayState.NewDay)
                {
                    Debug.Log("Mute");
                    player.gameObject.transform.GetChild(0).GetChild(3).GetComponent<AudioSource>().mute = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent != null && GameObject.Find("MasterManager(Clone)") == null)
        {
            if (other.transform.parent.TryGetComponent<PUN.PunPlayer>(out PUN.PunPlayer player))
            {
                if (!player.IsMine)
                {
                    player.gameObject.transform.GetChild(0).GetChild(3).GetComponent<AudioSource>().mute = false;
                }
            }
        }
    }
}

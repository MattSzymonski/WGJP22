using Mighty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueStorage : MonoBehaviour
{
    public float dissolveStrength = 4;

    public void TriggerDie()
    {
        Destroy(transform.parent.gameObject);
    }

    public void TriggerExplosionVFX()
    {
        MightyVFXManager.Instance.SpawnVFX(this.transform.parent.transform.position, Quaternion.identity, 2, 0.0f, "EnergyExplosion");

    }

    public void TriggerSound()
    {
        MightyAudioManager.Instance.PlayRandomSound("Kill_1", "Kill_2", "Kill_3", "Kill_4", "Kill_5");
        // MightyVFXManager.Instance.SpawnVFX(playerShootSelectionList[i].transform.position, Quaternion.identity, 2, 0.5f, "EnergyExplosion");
    }

    public void TriggerShake()
    {
        Camera.main.transform.parent.GetComponent<CameraShaker>().ShakeOnce(2.0f, 0.7f, 1f, 1.55f);
    }

}

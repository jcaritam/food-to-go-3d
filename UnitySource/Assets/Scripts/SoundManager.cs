using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

  private void Start()
  {
    DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
    DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed; 
  }

  private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
  {
    Debug.Log("DeliveryManager_OnRecipeFailed");
  }

  private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
  {
    Debug.Log("DeliveryManager_OnRecipeSuccess");
  }

  private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
  {
    AudioSource.PlayClipAtPoint(audioClip, position, volume);
  }
}

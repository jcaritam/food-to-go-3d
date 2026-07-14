using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

  [SerializeField] private AudioClipRefsSO audioClipRefsSO;

  private void Start()
  {
    DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
    DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
    CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
    Player.Instance.OnPickedSomething += Player_OnPickedSomething;
    BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
    TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    PlateKitchenObject.OnAnyIngredientRejected += PlateKitchenObject_OnAnyIngredientRejected;
  }

  private void OnDestroy()
  {
    if (DeliveryManager.Instance != null)
    {
      DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
      DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
    }
    CuttingCounter.OnAnyCut -= CuttingCounter_OnAnyCut;
    if (Player.Instance != null)
      Player.Instance.OnPickedSomething -= Player_OnPickedSomething;
    BaseCounter.OnAnyObjectPlacedHere -= BaseCounter_OnAnyObjectPlacedHere;
    TrashCounter.OnAnyObjectTrashed -= TrashCounter_OnAnyObjectTrashed;
    PlateKitchenObject.OnAnyIngredientRejected -= PlateKitchenObject_OnAnyIngredientRejected;
  }

  private void PlateKitchenObject_OnAnyIngredientRejected(object sender, EventArgs e)
  {
    PlateKitchenObject plate = sender as PlateKitchenObject;
    if (plate == null) return;
    PlaySound(audioClipRefsSO.deliveryFail, plate.transform.position);
  }

  private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
  {
    TrashCounter trashCounter = sender as TrashCounter;
    PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
  }

  private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
  {
    BaseCounter baseCounter = sender as BaseCounter;

    PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
  }

  private void Player_OnPickedSomething(object sender, EventArgs e)
  {
    PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
  }

  private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
  {
    CuttingCounter cuttingCounter = sender as CuttingCounter;
    PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position);
  }

  private void DeliveryManager_OnRecipeFailed(object sender, DeliveryManager.OnRecipeEventArgs e)
  {
    DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
    PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
  }

  private void DeliveryManager_OnRecipeSuccess(object sender, DeliveryManager.OnRecipeEventArgs e)
  {
    DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
    PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
  }

  private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
  {
    PlaySound(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], position, volume);
  }
  private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
  {
    if (audioClip == null) return;

    #if UNITY_WEBGL
      var go = new GameObject("OneShotAudio");
      var source = go.AddComponent<AudioSource>();
      source.clip = audioClip;
      source.volume = volume;
      source.spatialBlend = 0f;
      source.Play();
      Destroy(go, audioClip.length + 0.1f);
    #else
      AudioSource.PlayClipAtPoint(audioClip, position, volume);
    #endif
  }
}

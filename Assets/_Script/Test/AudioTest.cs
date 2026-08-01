using UnityEngine;

public class AudioTest : MonoBehaviour
{
  public void PlayButtonSound()
  {
    SoundManager.PlaySFXSound(SFXType.Button);
  }
}

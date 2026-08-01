using UnityEngine;

public class AudioTest : MonoBehaviour
{
   public void PlayWinSFX()
   {
      SoundManager.PlaySFXSound(SFXType.Win);
   }

   public void PlayLoseSFX()
   {
      SoundManager.PlaySFXSound(SFXType.Lose);
   }
   
   public void PlayCoinSFX()
   {
      SoundManager.PlaySFXSound(SFXType.Coin);
   }

   public void PlayGamePlayBGM()
   {
      SoundManager.PlayBGMSound(BGMType.GamePlay);
   }

   public void PlayMainMenuBGM()
   {
      SoundManager.PlayBGMSound(BGMType.MainMenu);
   }


}

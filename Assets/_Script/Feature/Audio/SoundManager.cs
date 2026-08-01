using System;
using UnityEngine;
using Random = UnityEngine.Random;
[ExecuteInEditMode]
public class SoundManager : Singleton<SoundManager>
{
   [SerializeField] private SoundList[] SFXList;
   [SerializeField] private SoundList[] BGMList;
   [SerializeField] private AudioSource sfxSource;
   [SerializeField] private AudioSource bgmSource;

   public void MuteAndUnMuteSFX() => sfxSource.mute = !sfxSource.mute;
   public void MuteAndUnMuteBGM() => bgmSource.mute = !bgmSource.mute;
   
   public static void PlaySFXSound(SFXType type, float volume = 1)
   {
      AudioClip[] audioClips = Instance.SFXList[(int)type].AudioList;
      AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
      // Instance.sfxSource.Stop();
      Instance.sfxSource.PlayOneShot(randomClip,volume);
   }

   public static void PlayBGMSound(BGMType type, float volume = 1)
   {
      AudioClip[] audioClips = Instance.BGMList[(int)type].AudioList;
      AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
      Instance.bgmSource.Stop();
      Instance.bgmSource.clip = randomClip;
      Instance.bgmSource.volume = volume;
      Instance.bgmSource.loop = true;
      Instance.bgmSource.Play();
   }
   
   
   #if UNITY_EDITOR
   private void OnEnable()
   {
      string[] sfxNames = Enum.GetNames(typeof(SFXType));
      Array.Resize(ref SFXList,sfxNames.Length);
      for (int i = 0; i < SFXList.Length; i++)
      {
         SFXList[i].name = sfxNames[i];
      }      
      
      string[] bgmNames = Enum.GetNames(typeof(BGMType));
      Array.Resize(ref BGMList,bgmNames.Length);
      for (int i = 0; i < BGMList.Length; i++)
      {
         BGMList[i].name = bgmNames[i];
      }
   }

#endif
}
[Serializable]
public struct SoundList
{
   [HideInInspector] public string name;
   [field:SerializeField] public AudioClip[] AudioList { get; private set; }
}



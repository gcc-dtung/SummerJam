using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource)),ExecuteInEditMode]
public class SoundManager : Singleton<SoundManager>
{
   [SerializeField] private SoundList[] soundList;
   private AudioSource audioSource;

   private void Start()
   {
      audioSource = this.GetComponent<AudioSource>();
   }

   public static void PlaySound(SoundType type, float volume = 1)
   {
      AudioClip[] audioClips = Instance.soundList[(int)type].AudioList;
      AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
      Instance.audioSource.PlayOneShot(randomClip,volume);
   }
   #if UNITY_EDITOR
   private void OnEnable()
   {
      string[] names = Enum.GetNames(typeof(SoundType));
      Array.Resize(ref soundList,names.Length);
      for (int i = 0; i < soundList.Length; i++)
      {
         soundList[i].name = names[i];
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



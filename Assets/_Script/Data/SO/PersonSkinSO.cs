using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PersonSkinSO", menuName = "ScriptableObjects/PersonSkinSO")]
public class PersonSkinSO : ScriptableObject
{
    [Header("Base Skin")]
    [SerializeField] private Sprite[] baseList;

    [Header("Face")]
    [SerializeField] private Sprite normalFace;
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite angryFace;

    [Header("Trait Overlay")]
    [FormerlySerializedAs("trail")]
    [SerializeField] private PersonTraitSkin[] traitSkins;

    public Sprite GetBaseSkin(int index = 0)
    {
        if (baseList == null || baseList.Length == 0) return null;

        index = Mathf.Clamp(index, 0, baseList.Length - 1);
        return baseList[index];
    }

    public Sprite GetNormalFace()
    {
        return normalFace;
    }

    public Sprite GetStateFace(bool isHappy)
    {
        return isHappy ? happyFace : angryFace;
    }

    public Sprite GetTraitSkin(IReadOnlyList<Trait> traits)
    {
        if (traits == null || traitSkins == null) return null;

        for (int i = 0; i < traits.Count; i++)
        {
            Sprite traitSkin = GetTraitSkin(traits[i]);
            if (traitSkin != null)
                return traitSkin;
        }

        return null;
    }

    public Sprite GetTraitSkin(Trait trait)
    {
        if (traitSkins == null) return null;

        for (int i = 0; i < traitSkins.Length; i++)
        {
            if (traitSkins[i].trait == trait)
                return traitSkins[i].sprite;
        }

        return null;
    }
}

[System.Serializable]
public class PersonTraitSkin
{
    public Trait trait;
    [FormerlySerializedAs("trailFace")]
    [FormerlySerializedAs("face")]
    public Sprite sprite;
}

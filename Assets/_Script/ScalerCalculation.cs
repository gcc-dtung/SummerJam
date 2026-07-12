using System;
using UnityEngine;

public static class ScalerCalculation
{
  private static Vector2 OriginalResolution = new Vector2(1080, 1920);
  private static float DesignAspect =>  (float) OriginalResolution.x / OriginalResolution.y;
  private static float RealityAspect => (float) Screen.width / Screen.height;
  public static float ScaleFactor => Mathf.Min(RealityAspect/DesignAspect,1);
}

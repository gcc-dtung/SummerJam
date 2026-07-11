using System;
using UnityEngine;

public static class ScalerCalculation
{
  private static Vector2 OriginalResolution = new Vector2(1080, 1920);
  public static float ScaleX => Screen.width / OriginalResolution.x;
  public static float ScaleY => Screen.height / OriginalResolution.y;
  public static float UniformScale => Mathf.Min(ScaleX, ScaleY);
}

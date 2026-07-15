
public struct ConditionInfo
{
   public string Description;
   public bool IsSatisfied;

   public void SetUpIsSatisfied(bool condition)
   {
      IsSatisfied = condition;
   }
}

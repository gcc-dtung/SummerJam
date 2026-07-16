using System.Collections.Generic;
using UnityEngine;

public class UndoManager : Singleton<UndoManager>
{
   private Stack<MoveCommand> commandHistory = new Stack<MoveCommand>();

   public void RecordMove(Person person1,Cell from, Person person2,Cell to,bool moveDeducted )
   {
      MoveCommand command = new MoveCommand(person1,from,person2,to,moveDeducted);
      commandHistory.Push(command);
   }

   public void UndoMove()
   {
      if (commandHistory.Count == 0)
      {
         Debug.LogWarning("There was no move detected");
         return;
      }

      MoveCommand lastmove = commandHistory.Pop();
      lastmove.Undo();
   }

   public void ClearHistory()
   {
      commandHistory.Clear();
   }
}

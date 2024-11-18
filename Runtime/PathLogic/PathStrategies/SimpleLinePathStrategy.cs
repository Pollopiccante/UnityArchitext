using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "linepathstrategy", menuName = "ScriptableObjects/Paths/LinePathStrategy", order = 0)]
public class SimpleLinePathStrategy : PathStrategy
{
   protected override Path GetPath(string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
   {
      float length = alphabet.CalculateTextLength(text, letterScaling);
      // create straight path of length
      return new Path(new Vector3[] {new Vector3(0, 0, 0), new Vector3(length, 0, 0)});
   }

   private static SimpleLinePathStrategy _default = null;
   public static SimpleLinePathStrategy Default()
   {
      if (_default == null)
      {
         _default = CreateInstance<SimpleLinePathStrategy>();
      }
      
      return _default;
   }
}

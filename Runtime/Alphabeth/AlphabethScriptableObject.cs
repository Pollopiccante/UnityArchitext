using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshAlphabet", menuName = "ScriptableObjects/MeshAlphabet", order = 1)]
public class AlphabethScriptableObject : ScriptableObject
{
   public Mesh[] meshes;
   public float spaceWidth = 1f;
   public float interLetterDistance = 0.2f;

   public void ConfigureWidthAutomatically()
   {
      spaceWidth = meshes[0].bounds.size.x;
      interLetterDistance = spaceWidth / 5f;
   }

   public Dictionary<char, float> GetWidthDictionary()
   {
      Dictionary<char, float> outDict = new Dictionary<char, float>();
      for (int i = 0; i < TextUtil.alphabeth.Length; i++)
      {
         char letter = TextUtil.alphabeth[i];
         float width = meshes[i].bounds.size.x + interLetterDistance;
         outDict.Add(letter, width);
      }
      return outDict;
   }

   public float CalculateTextLength(string text, List<float> letterScaling)
   {
      // calculate length of text
      float length = 0f;
      Dictionary<char, float> widthDictionary = GetWidthDictionary();
      for (int i = 0; i < text.Length; i++)
      {
         char currentCharacter = text[i];
         if (currentCharacter == ' ')
            length += spaceWidth * letterScaling.ElementAt(i);
         else
         {
            float charWidth = (widthDictionary[text[i]]) * letterScaling.ElementAt(i);
            length += charWidth;
         }
      }
      return length;
   }

}

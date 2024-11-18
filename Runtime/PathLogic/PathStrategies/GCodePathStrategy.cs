using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "gcodepathstrategy", menuName = "ScriptableObjects/Paths/GCodePathStrategy", order = 0)]
public class GCodePathStrategy : PathStrategy
{

    
    public TextAsset gcodeFile;
    public int ignoreFirstNPositions = 0;
    public bool extrusionOnly = false;
    
    public class GCodeWord
    {
        public string originalString;
        public string prefix;
        public float zValue = 0f;
        public float relativeExtrusion = 0f;
        private Dictionary<char, float> paramMap = new Dictionary<char, float>();
        

        public void AddParam(char paramName, float paramValue)
        {
            if (HasParam(paramName))
                throw new ArgumentException("Param was present already.");
            paramMap.Add(paramName, paramValue);
        }
        public bool HasParam(char paramName)
        {
            return paramMap.Keys.Contains(paramName);
        }

        public Vector3 GetXYZ()
        {
            
            return new Vector3(paramMap['X'], zValue, paramMap['Y']);
        }

        public float GetZ()
        {
            return paramMap['Z'];
        }
        
        public static GCodeWord FromGCodeLine(string gCodeLine)
        {
            GCodeWord res = new GCodeWord();
            res.originalString = gCodeLine;
            
            // split words, get prefix
            string[] words = gCodeLine.Split(" ");
            res.prefix = words[0];
            
            // parse the rest of the parameters
            for (int i = 1; i < words.Length; i++)
            {
                string paramString = words[i];
                if (paramString.Length < 2)
                    continue;
                if (paramString[0] == ';')
                    continue;
                
                char paramName = paramString[0]; // take first char as param name
                float paramValue;
                try
                {
                    paramValue = float.Parse(paramString.Substring(1, paramString.Length - 1));
                }
                catch (FormatException fe)
                {
                    continue;
                }
                
                res.AddParam(paramName, paramValue);
            }

            return res;
        }

        public static List<GCodeWord> FromGCodeLines(string[] gCodeLines, Func<GCodeWord, bool> filterFunc)
        {
            List<GCodeWord> outWords = new List<GCodeWord>();
            
            // parse lines, apply filter
            float zLevel = 0f; // keep track of the Z value
            float previousExtrusion = 0f; // track extrusion to calc relative extrusion
            for (int i = 0; i < gCodeLines.Length; i++)
            {
                GCodeWord word = GCodeWord.FromGCodeLine(gCodeLines[i]);
                
                // update z level
                if (word.HasParam('Z'))
                    zLevel = word.GetZ();
                // save z level
                word.zValue = zLevel;
                
                // calc and save relative extrusion
                float relativeExtrusion = 0f;
                if (word.HasParam('E'))
                {
                    float currentExtrusion = word.paramMap['E'];
                    relativeExtrusion = currentExtrusion - previousExtrusion;
                    previousExtrusion = currentExtrusion;
                }
                // save relative extrusion
                word.relativeExtrusion = relativeExtrusion;
                
                // filter
                if (filterFunc.Invoke(word))
                    outWords.Add(word);
                
            }
            
            return outWords;
        }
    }
    protected override Path GetPath(string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        TextInsertionResult textInsertionResult = GetPathPrototype().ConvertToPointData(text, alphabet, letterScaling);
        return GetPathPrototype();
    }

    public Path GetPathPrototype()
    {
        List<int> holes = new List<int>();
        
        // split gcode in lines
        string[] gcodeLines = gcodeFile.text.Split("\n");

        Debug.Log($"Lines : {gcodeLines.Length}");
        
        // only take lines with "G1" (movements with extrusion)
        // only take lines with X,Y coordinate data
        int holeIndex = 0;
        List<GCodeWord> gCodeWords = GCodeWord.FromGCodeLines(gcodeLines, gCodeWord =>
        {
            
            // take paths that have extrusion, also save holes
            if (gCodeWord.relativeExtrusion <= 0.1f)
            {
                if (extrusionOnly && !holes.Contains(holeIndex))
                {
                    holes.Add(holeIndex);
                }
                
                return false;
            }
                
            if (!gCodeWord.prefix.Equals("G1"))
                return false;
            if (!gCodeWord.HasParam('X') || !gCodeWord.HasParam('Y'))
                return false;
            
            holeIndex++;
            return true;
        });

        // skip first n words
        gCodeWords = gCodeWords.Skip(ignoreFirstNPositions).ToList();
        Debug.Log($"Words : {gCodeWords.Count}");
        
        
        // convert gCode positions to vector array
        Vector3[] positions = new Vector3[gCodeWords.Count];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = gCodeWords[i].GetXYZ();

      
        // remove last hole
        if(holes.Any()) 
            holes.RemoveAt(holes.Count - 1);
        
        Debug.Log($"Holes: {holes.Count}");
        return new Path(positions, holes);
        
    }
}

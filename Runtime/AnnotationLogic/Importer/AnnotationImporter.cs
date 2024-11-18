using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class AnnotationImporter: GenericSection
{
    public TextAsset annotationFile;
    public abstract ValueWrapper ReadValue(string encodedValue);
    public abstract ValueWrapper GetDefaultValue();


    private GenericSection _sectionInstance = null;
    public GenericSection GetSectionInstance()
    {
        if (_sectionInstance == null)
            _sectionInstance = Import();
        return _sectionInstance;
    }
    
    public override dynamic GetValueAt(int index)
    {
        return GetSectionInstance().GetValueAt(index);
        // return Import().GetValueAt(index);
    }

    public override int GetLength()
    {
        return GetSectionInstance().GetLength();
        // return Import().GetLength();
    }

    public override bool MustForceIterable()
    {
        return GetSectionInstance().MustForceIterable();
        // return Import().MustForceIterable();
        
    }
    
    
    private GenericSection Import()
    {
        
        // read text file
        string content = TextUtil.ToSingleLine(annotationFile.text);
        
        // extract all annotations
        Regex getStartRegex = new Regex("(.*?)(?=\\$\\()");
        Regex getMiddleRegex = new Regex("(((?<=(\\)\\$))([^)]*)(?=\\$\\())|((?<=\\$)\\(([^(]*)\\)(?=\\$)))", RegexOptions.Compiled);
        Regex getEndRegex = new Regex("(?<=\\)\\$)(.*?)$", RegexOptions.RightToLeft);
        
        List<Match> matches = new List<Match>();
        
        
        // beginning
        MatchCollection startMatches = getStartRegex.Matches(content);
        if (startMatches.Count > 0)
            matches.Add(startMatches[0]);
        // middle
        matches.AddRange(getMiddleRegex.Matches(content));
        // end
        MatchCollection endMatches = getEndRegex.Matches(content);
        if (endMatches.Count > 0)
            matches.Add(endMatches[0]);


        List<Tuple<string, ValueWrapper, int>> textValuePairs = new List<Tuple<string, ValueWrapper, int>>();
        Debug.Log("MATCHES: ");
        foreach (Match m in matches)
        {
            Debug.Log($"{m.ToString()}");
            string matchString = m.ToString();
            // empty
            if (matchString.Length == 0)
                continue;
            // specified section
            if (matchString[0] == '(')
            {
                string[] textValue = m.ToString().Split("|");
                string text = textValue[0].Substring(1);
                string valueString = textValue[1].Substring(0, textValue[1].Length - 1);
                ValueWrapper value = ReadValue(valueString);
                textValuePairs.Add(new Tuple<string, ValueWrapper, int>(text, value, TextUtil.ToSingleLine(text).Replace(" ", "").Length));
            }
            // area between specified sections
            else
            {
                textValuePairs.Add(new Tuple<string, ValueWrapper, int>(matchString, GetDefaultValue(), TextUtil.ToSingleLine(matchString).Replace(" ", "").Length));
            }
        }

        // create sections
        GenericCompoundSection compoundSection = ScriptableObject.CreateInstance<GenericCompoundSection>();
        compoundSection.values = new List<ValueWrapper>();
        for (int i = 0; i < textValuePairs.Count; i++)
        {
            // add new value to compound section
            compoundSection.values.Add(new ValueWrapper());
            
            Tuple<string, ValueWrapper, int> pairs = textValuePairs[i];
            
            GenericIdentitySection genericIdentitySection = ScriptableObject.CreateInstance<GenericIdentitySection>();
            genericIdentitySection.value = pairs.Item2;
            genericIdentitySection.length = pairs.Item3;

            compoundSection.values[i].genericSectionValue = genericIdentitySection;
        }
        
        return compoundSection;
    }
    
    public override bool JustBaseInspector()
    {
        return true;
    }

    private void OnValidate()
    {
        Debug.Log($"RESET Annotation Importer {this.name}");
        _sectionInstance = null;
    }
}

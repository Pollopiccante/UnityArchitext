using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TextArangement<TextDimensions, EffectDimensions> : InspectableTextArrangement
{
    public TextScriptableObject text;
    public PathScriptableObject mainPath;
    public AnnotationData[] completeAnnotationData;
    public string mappingName;
    
    private AbstractMapping<TextDimensions, EffectDimensions> mapping;
    private CompleteAnnotation<TextDimensions> completeTextNotation;


    private FieldInfo[] GetInputFields()
    {
        return typeof(TextDimensions).GetFields().Where(field => field.Name != "Letter").ToArray();
    }

    override public void MakeValid()
    {
        // ensure correct length
        if (completeAnnotationData == null)
        {
            completeAnnotationData = InitializeAnnotationData();
        }
        if (completeAnnotationData.Length != GetInputFields().Length)
        {
            AnnotationData[] newCompleteAnnotationData = new AnnotationData[GetInputFields().Length];

            // add extra field
            if (GetInputFields().Length < completeAnnotationData.Length)
            {
                newCompleteAnnotationData = InitializeAnnotationData();
                for (int i = 0; i < completeAnnotationData.Length; i++)
                    newCompleteAnnotationData[i] = completeAnnotationData[i];
            }
            // remove redundant fields
            else
            {
                for (int i = 0; i < newCompleteAnnotationData.Length; i++)
                    newCompleteAnnotationData[i] = completeAnnotationData[i];
                
                completeAnnotationData = newCompleteAnnotationData;
            }
        }

    }

    private AnnotationData[] InitializeAnnotationData()
    {
        AnnotationData[] data = new AnnotationData[GetInputFields().Length];
        for (int i = 0; i < GetInputFields().Length; i++)
        {
            FieldInfo fieldInfo = GetInputFields()[i];
            data[i] = new AnnotationData(fieldInfo.Name, null);
        }
        return data;
    }
    
    private void Awake()
    {
        MakeValid();
    }


    public VFXDataScriptableObject CreateVfxEffectData()
    {
        // create mapping
        Type mapType = Type.GetType(mappingName);
        if (mapType == null)
            throw new ArgumentException($"mapping type \"{mappingName}\" was not found");
        mapping = (AbstractMapping<TextDimensions, EffectDimensions>)Activator.CreateInstance(mapType);
        
        // create complete annotation
        // dimensions specified in object
        Dictionary<string, GenericSection> importedDimensions = new Dictionary<string, GenericSection>();
        foreach (AnnotationData data in completeAnnotationData)
            importedDimensions.Add(data.SectionName, data.Data);
        // letter dimension
        GenericEnumerableSection letterSection = CreateInstance<GenericEnumerableSection>();
        letterSection.values = ValueWrapper.FromString(TextUtil.ToSingleLine(text.GetContent()));
        
        importedDimensions.Add("Letter", letterSection);

        completeTextNotation = new CompleteAnnotation<TextDimensions>(importedDimensions);

        return VFXUtil.CreateVFXDataFromCompleteAnnotation(completeTextNotation, mapping, mainPath.LoadPath());
    }
    
    override public void CreateVfxEffect()
    {
        VFXUtil.CreateEffectFromVFXData(CreateVfxEffectData());
    }

    #if (UNITY_EDITOR)
    override public void SaveVfxData(string fileName)
    {
        VFXUtil.SaveVFXData(CreateVfxEffectData(), fileName);
    }
    #endif
}

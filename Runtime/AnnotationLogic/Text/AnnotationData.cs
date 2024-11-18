using System;

[Serializable]
public class AnnotationData
{
    public string SectionName;
    public GenericSection Data;

    public AnnotationData(string sectionName, GenericSection data)
    {
        SectionName = sectionName;
        Data = data;
    }
}

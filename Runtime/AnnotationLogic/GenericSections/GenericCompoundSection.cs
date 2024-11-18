using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SODG", menuName = "SO/CompoundSection", order = 0)]
public class GenericCompoundSection : GenericSection
{
    
    public GenericCompoundSection()
    {
        this.type = AllowedGenericTypes.GENERIC_SECTION;
    }
    
    public override dynamic GetValueAt(int index)
    {
        GenericSection[] parts = values.Select(v => v.genericSectionValue).ToArray();
        
        GenericSection part = null;
        for (int i = 0; i < parts.Length; i++)
        {
            part = parts[i];
            if (index - part.GetLength() >= 0)
                index -= part.GetLength();
            else
                break;    
        }

        if (part == null)
            throw new Exception($"index {index} not found in compound section");

        return part.GetValueAt(index);

    }

    public override int GetLength()
    {
        GenericSection[] parts = values.Select(v => v.genericSectionValue).ToArray();
        int length = 0;
        foreach (GenericSection section in parts)
            length += section.GetLength();
        return length;
    }
    
    public override bool MustForceIterable()
    {
        return true;
    }

}

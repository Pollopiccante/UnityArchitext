using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SODG", menuName = "SO/EnumerableSection", order = 0)]

public class GenericEnumerableSection : GenericSection
{ 
    public override dynamic GetValueAt(int index)
    {
        return values.ElementAt(index);
    }

    public override int GetLength()
    {
        return values.Count;
    }
    
    public override bool MustForceIterable()
    {
        return true;
    }

}

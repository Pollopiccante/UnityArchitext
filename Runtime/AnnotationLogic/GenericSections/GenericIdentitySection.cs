using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "SODG", menuName = "SO/IdentitySection", order = 0)]
public class GenericIdentitySection : GenericSection
{
    public int length;
    
    public override dynamic GetValueAt(int index)
    {
        return value;
    }

    public override int GetLength()
    {
        return length;
    }

    public override bool MustForceIterable()
    {
        return false;
    }

}

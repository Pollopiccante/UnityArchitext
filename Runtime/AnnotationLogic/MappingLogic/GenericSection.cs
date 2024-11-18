using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;






public abstract class GenericSection : ScriptableObject
{
    public AllowedGenericTypes type = AllowedGenericTypes.FLOAT;
    [HideInInspector]
    public ValueWrapper value = new ValueWrapper();
    [HideInInspector]
    public List<ValueWrapper> values = new List<ValueWrapper>();

    public abstract object GetValueAt(int index);
    public abstract int GetLength();
    public abstract bool MustForceIterable();

    public bool Contains(int i)
    {
        return i < GetLength();
    }
 
    public virtual bool JustBaseInspector()
    {
        return false; 
    }
}


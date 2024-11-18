using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDimension : Dimension<string>
{
    public override string GetDefaultValue()
    {
        return "none";
    }
}

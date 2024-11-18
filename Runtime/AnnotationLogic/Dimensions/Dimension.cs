
using System.Collections.Generic;

public abstract class Dimension<T>
{
    public T value;

    public Dimension()
    {
        value = GetDefaultValue();
    }
    public abstract T GetDefaultValue();
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class CompleteAnnotation<T>
{
    private int _dimensionsToImport = 0;
    private Dictionary<string, GenericSection> _importedDimensions = new Dictionary<string, GenericSection>();
    private int _initialPosition = 0;
    private int _finalPosition;

    public CompleteAnnotation()
    {
        _dimensionsToImport = typeof(T).GetFields().Length;
    }

    public Dictionary<string, GenericSection> getImportedDimensions()
    {
        return _importedDimensions;
    }
    public CompleteAnnotation(Dictionary<string, GenericSection> importedDimensions)
    {
        _dimensionsToImport = typeof(T).GetFields().Length;
        _importedDimensions = importedDimensions;
    }


    private string GetFieldNameForDimension(Type dimensionType)
    {
        foreach (FieldInfo field in typeof(T).GetFields())
            if (field.FieldType == dimensionType)
                return field.Name;
        throw new Exception($"Field of type {dimensionType.ToString()} was not found.");
    }

    public void Import<TDimensionType>(GenericSection section)
    {
        string fieldName = GetFieldNameForDimension(typeof(TDimensionType));
        
        // check if field was imported already
        if (_importedDimensions.Keys.Contains(fieldName))
            throw new Exception($"Field {fieldName} was imported already");
        
        // update final position
        if (section.GetLength() > _finalPosition)
            _finalPosition = section.GetLength();
        
        // import
        _importedDimensions.Add(fieldName, section);
    }

    public List<T> Finish()
    {
        // check if all dimensions were imported
        if (_importedDimensions.Count != _dimensionsToImport) 
            throw new Exception($"Not all dimensions were imported. Imported: {_importedDimensions.Count}, Needed: {_dimensionsToImport}");
        
        // recalculate length
        foreach (GenericSection sec in _importedDimensions.Values)
            if (sec.GetLength() > _finalPosition)
                _finalPosition = sec.GetLength();

        List<T> outList = new List<T>();
        // iterate initial to final position
        for (int i = 0; i < _finalPosition; i++)
        {
            // write dimension into each field of element
            T element = (T)Activator.CreateInstance(typeof(T));
            foreach (string field in _importedDimensions.Keys)
            {
                // search for value in sections
                GenericSection section = _importedDimensions[field];
                if (!section.Contains(i))
                {
                    typeof(T).GetField(field).SetValue(element, Activator.CreateInstance(typeof(T).GetField(field).FieldType));
                }
                else
                {
                    ValueWrapper value = (ValueWrapper)section.GetValueAt(i);

                    // set value
                    Type dimensionType = typeof(T).GetField(field).FieldType;
                    object wrappedValue = Activator.CreateInstance(dimensionType);

                    typeof(T).GetField(field).FieldType.GetField("value").SetValue(wrappedValue, value.getValue(typeof(T).GetField(field).FieldType.GetField("value").FieldType));
                    typeof(T).GetField(field).SetValue(element, wrappedValue);
                }
            }
            outList.Add(element);
        }
        return outList;
    }
}

using System.Collections.Generic;
using System.Linq;

public abstract class AbstractMapping<TextDimensions, EffectDimensions>
{
    public abstract EffectDimensions Convert(TextDimensions textDimensions, int index);
    public List<EffectDimensions> ConvertMany(List<TextDimensions> textDimensionsListing)
    {
        
        return textDimensionsListing.Select((elem, i) => Convert(elem, i)).ToList();
    }
    
}



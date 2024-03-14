using System;
using System.Collections.Generic;

namespace UnitySpec.Bindings.Discovery
{
    public interface ISpecFlowAttributesFilter
    {
        IEnumerable<Attribute> FilterForSpecFlowAttributes(IEnumerable<Attribute> customAttributes);
    }
}

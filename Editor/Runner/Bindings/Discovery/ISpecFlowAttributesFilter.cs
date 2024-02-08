using System;
using System.Collections.Generic;

namespace UnityFlow.Bindings.Discovery
{
    public interface ISpecFlowAttributesFilter
    {
        IEnumerable<Attribute> FilterForSpecFlowAttributes(IEnumerable<Attribute> customAttributes);
    }
}

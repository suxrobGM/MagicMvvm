﻿using System.Collections.Generic;

namespace MagicMvvm.Parameters;

internal class ParameterCache : IParameterCache
{
    private readonly Dictionary<Type, ParameterInfo> _cache = new();

    public ParameterInfo Get(Type type)
    {
        if (type == null) 
            throw new ArgumentNullException(nameof(type));
        
        return _cache.TryGetValue(type, out var info) ? info : null;
    }

    public void Set(Type type, ParameterInfo info)
    {
        if (type == null) 
            throw new ArgumentNullException(nameof(type));
        
        _cache[type] = info ?? throw new ArgumentNullException(nameof(info));
    }
}
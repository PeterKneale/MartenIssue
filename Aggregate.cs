using System;
using System.Collections.Generic;
using System.Linq;

namespace Issue;

public class Aggregate
{
    private List<ValueObject> _list = new List<ValueObject>();

    public Guid Id { get; }

    public Aggregate(Guid id)
    {
        Id = id;
    }

    public int Count() => _list.Count();

    public void Add(ValueObject value)
    {
        _list.Add(value);
    }
}

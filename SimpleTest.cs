using System;
using Xunit;

namespace Issue;

public class SimpleTest
{

    [Fact]    public void Value_added_to_aggregate_found_in_list() // This tests passes
    {
        // arrange
        var id = Guid.NewGuid();
        var aggregate = new Aggregate(id);
        var value = new ValueObject();

        // act
        aggregate.Add(value);

        // assert
        Assert.Equal(1, aggregate.Count());
    }
}

using Marten;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Weasel.Core;
using Xunit;

namespace Issue;

public class ReproduceInconsistantBehaviour : IDisposable
{
    private readonly ServiceProvider _provider;

    public ReproduceInconsistantBehaviour()
    {
        var connectionString = "host=localhost;database=postgres;password=postgres;username=postgres";
        var services = new ServiceCollection();
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Schema.For<Aggregate>();
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);
        });
        _provider = services.BuildServiceProvider();        
    }
    
    [Fact]
    public void Value_added_to_aggregate_found_in_list() // This tests passes
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

    [Fact]
    public void Value_added_to_aggregate_found_in_list_when_using_root_provider() // This tests passes
    {
        // arrange
        var id = Guid.NewGuid();
        var saved = new Aggregate(id);
        var value = new ValueObject();
        
        using var writeSession = _provider.GetRequiredService<IDocumentSession>();
        {
            // act
            saved.Add(value);
            writeSession.Insert(saved);
            writeSession.SaveChanges();
        }

        // assert
        using var readSession = _provider.GetRequiredService<IDocumentSession>();
        {
            var loaded = readSession.Load<Aggregate>(id);
            Assert.Equal(1, loaded.Count());
        }
    }


    [Fact]
    public void Value_added_to_aggregate_found_in_list_when_using_scoped_provider() // This tests fails
    {
        // arrange
        var id = Guid.NewGuid();
        var saved = new Aggregate(id);
        var value = new ValueObject();
        
        using var writeScope = _provider.CreateScope();
        using var writeSession = writeScope.ServiceProvider.GetRequiredService<IDocumentSession>();
        {

            // act
            saved.Add(value);
            writeSession.Insert(saved);
            writeSession.SaveChanges();
        }

        // assert
        using var readScope = _provider.CreateScope();
        using var readSession = readScope.ServiceProvider.GetRequiredService<IDocumentSession>();
        {
            var loaded = readSession.Load<Aggregate>(id);
            Assert.Equal(1, loaded.Count());
        }
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    public class Aggregate
    {
        private readonly List<ValueObject> _list = new List<ValueObject>();

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
    
    public class ValueObject
    {
        public ValueObject()
        {

        }
    }
}
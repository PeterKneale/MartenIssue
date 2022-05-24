using Marten;
using Microsoft.Extensions.DependencyInjection;
using System;
using Weasel.Core;
using Xunit;

namespace Issue;

public class ReproduceWithLightweightSessionsTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public ReproduceWithLightweightSessionsTests()
    {
        var connectionString = "host=localhost;database=postgres;password=postgres;username=postgres";
        var services = new ServiceCollection();
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Schema.For<Aggregate>();
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);
        }).UseLightweightSessions();
        _provider = services.BuildServiceProvider();        
    }

    [Fact]
    public void Value_added_to_aggregate_found_in_list_when_using_root_provider()  // This tests fails
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
}
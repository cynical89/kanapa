using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.WebEncoders;
using Kanapa.Tests.Misc;
using Xunit;

namespace Kanapa.Tests
{
  public class CouchClientTests : IDisposable
  {
    private readonly string _hostName;
    private readonly IUrlEncoder _urlEncoder;
    private readonly IList<string> _list;

    public CouchClientTests()
    {
      _hostName = "http://couch:5984";
      _urlEncoder = new UrlEncoder();
      _list = new List<string>();
    }

    [Fact]
    public void Client_Created_WithoutErrors() =>
      Assert.NotNull(CreateClient());

    [Fact]
    public void Client_CreatesDatabase_WithoutErrors() => Wrap(async () =>
        await CreateClient().CreateDatabase(GenerateDbName()));

    [Fact]
    public void Client_DeletsDatabase_WithoutErrors() => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      await (await CreateClient().CreateDatabase(dbName)).DeleteDatabase(dbName);
    });

    [Fact]
    public void Client_DeleteEntity_WithoutErrors() => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var info =
        await (await client.CreateDatabase(dbName)).Create(dbName, new SimpleObject { Name = "NamedObject", Value = 120 });
      await client.Delete(dbName, info.Id, info.ETag);
    });

    [Fact]
    public void Client_ReadMetadata_WithoutErrors() => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var assertion = await (await client.CreateDatabase(dbName)).GetDatabaseMetadata(dbName);
      Assert.NotNull(assertion);
    });

    [Fact]
    public void Client_GetDatabases_ReturnsNotEmptySet() => Wrap(async () =>
      Assert.NotEmpty(await CreateClient().GetDatabaseNames()));

    [Fact]
    public void Client_Create_WtihoutErrors() => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).Create(dbName, new SimpleObject
      {
        Name = "SomeName",
        Value = 1
      });
    });

    [Fact]
    public void Client_GetAllDocuments_ReturnsValues() => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).Create(dbName, new SimpleObject
      {
        Name = "SomeName",
        Value = 2
      });
      var documents = (await client.GetAllDocuments(dbName)).ToArray();
      Assert.True(documents.Length > 0);
    });

    [Theory]
    [InlineData("Name0", 0)]
    [InlineData("Some other value", 2)]
    [InlineData("They killed kenny!", 4)]
    public void Client_GetDocument_ReturnsCorrectValue(string name, int value) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).Create(dbName, new SimpleObject
      {
        Name = name,
        Value = value
      });
      var documents = (await client.GetAllDocuments(dbName)).ToArray();
      var document = await client.Get<SimpleObject>(dbName, documents[0].Id);
      Assert.True(documents.Length > 0);
      Assert.NotNull(document);
      Assert.Equal(name, document.Name);
      Assert.Equal(value, document.Value);
    });

    [Theory]
    [InlineData("Some1,Some2,Some3,Some4", 2, 1)]
    [InlineData("Some1, Some2", 0, 1)]
    [InlineData("Base0,Base2,Base4, Base5, Base6, Base7", 2, 3)]
    public void Client_CreateAndQueryTemporaryView_ReturnsValues(string values, int greaterThen, int count)
      => Wrap(async () =>
      {
        var dbName = GenerateDbName();
        var client = CreateClient();
        var splitted = values.Split(',');
        await client.CreateDatabase(dbName);
        for (var i = 0; i < splitted.Length; i++)
        {
          await client.Create(dbName, new SimpleObject
          {
            Name = splitted[0],
            Value = i
          });
        }
        var mapReduce = new MapReduce
        {
          Map = $"function(doc) {{ if(doc.Value > {greaterThen}) {{ emit(doc.Name, doc); }} }}"
        };
        var result = await client.CreateAndQueryTemporaryView<SimpleObject>(dbName, mapReduce);
        Assert.True(result.Rows.Length == count);
      });

    [Theory]
    [InlineData("Some1,Some2,Some3,Some4", 2, 1)]
    [InlineData("Some1, Some2", 0, 1)]
    [InlineData("Base0,Base2,Base4, Base5, Base6, Base7", 2, 3)]
    public void Client_QueryView_ReturnsValues(string values, int greaterThen, int count) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var splitted = values.Split(',');
      await client.CreateDatabase(dbName);
      for (var i = 0; i < splitted.Length; i++)
      {
        await client.Create(dbName, new SimpleObject
        {
          Name = splitted[0],
          Value = i
        });
      }
      var mapReduce = new MapReduce
      {
        Map = $"function(doc) {{ if(doc.Value > {greaterThen}) {{ emit(doc.Name, doc); }} }}"
      };
      const string designName = "design0";
      const string viewName = "greater_view";
      await client.CreateDesign(dbName, designName, new[]
      {
        new ViewDefinition
        {
          Mapping = mapReduce,
          Name = viewName
        }
      });
      var result = await client.QueryView<SimpleObject>(dbName, designName, viewName);
      Assert.True(result.Rows.Length == count);
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_CreateViews_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var view = design.Items[0];
      await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, new[] {view});
      for (var i = 1; i < design.Items.Length; i++)
      {
        await client.CreateView(dbName, design.AdditionalData, design.Items[i]);
      }
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_DeleteView_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var view = design.Items[0];
      await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, new[] {view});
      for (var i = 1; i < design.Items.Length; i++)
      {
        await client.CreateView(dbName, design.AdditionalData, design.Items[i]);
      }
      foreach (var item in design.Items)
      {
        await client.DeleteView(dbName, design.AdditionalData, item.Name);
      }
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_PutView_Test_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var view = design.Items[0];
      await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, new[] {view});
      for (var i = 1; i < design.Items.Length; i++)
      {
        await client.CreateView(dbName, design.AdditionalData, design.Items[i]);
      }
      foreach (var item in design.Items)
      {
        item.Mapping.Map = "function(doc) { emit(doc); }";
        await client.PutView(dbName, design.AdditionalData, item);
      }
    });

    [Theory]
    [InlineData("doc0")]
    [InlineData("doc1")]
    [InlineData("doc2")]
    [InlineData("doc3")]
    public void Client_PutEntity_WithoutErrors(string docName) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).Put(dbName, docName, new SimpleObject {Name = GenerateDbName(), Value = 1});
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_CreateDesign_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, design.Items);
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_DeleteDesign_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      var einfo = await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, design.Items);
      await client.DeleteDesign(dbName, design.AdditionalData, einfo.ETag);
    });

    [Theory]
    [MemberData("TestCasesWithViews")]
    public void Client_GetDesign_WithoutErrors(TestCaseWithViews design) => Wrap(async () =>
    {
      var dbName = GenerateDbName();
      var client = CreateClient();
      await (await client.CreateDatabase(dbName)).CreateDesign(dbName, design.AdditionalData, design.Items);
      var dd = await client.GetDesign(dbName, design.AdditionalData);
      Assert.Equal(dd.Name, dd.Name);
    });


    private string GenerateDbName()
    {
      var name = "testdabatase" + Guid.NewGuid().ToString("N");
      _list.Add(name);
      return name;
    }

    private CouchClient CreateClient() => new CouchClient(_hostName, _urlEncoder);

    public static IEnumerable<object[]> TestCasesWithViews => new[]
    {
      (new object[]
      {
        new TestCaseWithViews
        {
          AdditionalData = "designDocument",
          Items = new[]
          {
            new ViewDefinition
            {
              Name = "view0",
              Mapping = new MapReduce
              {
                Map = "function(doc) { emit(doc.Id);}",
                Reduce = "function (key, values, rereduce) { return count(values); }"
              }
            },
            new ViewDefinition
            {
              Name = "view1",
              Mapping = new MapReduce
              {
                Map =  "function(doc) { emit(doc.Id);}"
              }
            },
            new ViewDefinition
            {
              Name = "view2",
              Mapping = new MapReduce
              {
                Reduce = "function (key, values, rereduce) { return count(values); }"
              }
            },
            new ViewDefinition
            {
              Name = "view3"
            }

          }
        }
      })
    };

    public void Wrap(Func<Task> f)
    {
      try
      {
        Task.Run(f).Wait();
      }
      catch(AggregateException e)
      {
        var preserveStackTrace = e.InnerException.GetType().GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        preserveStackTrace.Invoke(e.InnerException, null);
        throw e.InnerException;
      }
    }

    void IDisposable.Dispose()
    {
      try
      {
        var client = CreateClient();
        foreach (var item in _list)
        {
          try
          {
            Wrap(async () => await client.DeleteDatabase(item));
          }
          catch
          {
            // ignored
          }
        }
      }
      catch
      {
        // ignored
      }
    }
  };
}
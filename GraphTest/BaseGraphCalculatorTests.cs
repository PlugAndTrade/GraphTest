using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tester.Helpers;
using Tester.Implementations;
using Tester.Interfaces;
using Tester.Model;

namespace Tester
{
    public class BaseGraphCalculatorTests
    {
        private static ServiceProvider _serviceProvider;

        protected static void InitializeClass(TestContext context)
        {
            var serviceCollection = new ServiceCollection();

            var contextGraphs = GetGraphsFromContext(context);
            if (contextGraphs != null)
                serviceCollection.AddSingleton<IGraphRepository>(sp => new MemoryGraphRepository(contextGraphs));

            var sqlConnection = context.Properties["sqlConnection"]?.ToString();
            if (sqlConnection != null)
                serviceCollection.AddSingleton<IGraphRepository>(sp =>
                {
                    var repository = new SqlGraphRepository(sqlConnection);
                    repository.Initialize(GetSqlScript()).Wait();
                    return repository;
                });

            serviceCollection
                .AddSingleton<IGraphRepository, MemoryGraphRepository>(sp => new MemoryGraphRepository(GetStaticGraphs()))
                .AddSingleton(sp => sp.GetServices<IGraphRepository>().ToArray())
                .AddSingleton<GraphProvider>()

                .AddSingleton<IGraphCalculator, PatrickGraphCalculator>()
                .AddSingleton<IGraphCalculator, MasterGraphCalculator>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static Graph[] GetStaticGraphs()
        {
            return new[]
            {
                new Graph
                {
                    Name = "Very simple",
                    Nodes = new[]
                    {
                        new Node {Id = "A", ParentId = null},
                    }
                },
                new Graph
                {
                    Name = "Simple",
                    Nodes = new[]
                    {
                        new Node {Id = "A", ParentId = null},
                        new Node {Id = "B", ParentId = "A"},
                    }
                },

            };
        }

        private static string GetSqlScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            var name = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith("SqlRepository.txt"));

            using var stream = assembly.GetManifestResourceStream(name);

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        private static Graph[] GetGraphsFromContext(TestContext context)
        {
            var graphsJson = context.Properties["graphs"]?.ToString();
            if (graphsJson == null)
                return null;

            return JsonConvert.DeserializeObject<Graph[]>(graphsJson);
        }

        protected readonly GraphProvider GraphProvider;
        protected readonly IGraphCalculator[] GraphCalculators;

        public BaseGraphCalculatorTests()
        {
            GraphProvider = _serviceProvider.GetService<GraphProvider>();
            GraphCalculators = _serviceProvider.GetServices<IGraphCalculator>().ToArray();
        }

        protected class GraphCalculatorResult
        {
            public IGraphCalculator Calculator { get; }
            public int Value { get; }

            public GraphCalculatorResult(int value, IGraphCalculator calculator)
            {
                Value = value;
                Calculator = calculator;
            }
        }

        protected async Task RunTest(Func<IGraphCalculator, Graph, int> test)
        {
            await foreach (var graph in GraphProvider.Get())
            {
                var results = GraphCalculators
                    .Select(c => new GraphCalculatorResult(test.Invoke(c, graph), c))
                    .ToArray();

                Assert.AreEqual(1, GetUniqueValues(results), GetMessage(results));
            }
        }

        private static int GetUniqueValues(GraphCalculatorResult[] results)
        {
            return results.Select(r => r.Value).Distinct().Count();
        }

        private static string GetMessage(GraphCalculatorResult[] results)
        {
            return $"{Environment.NewLine}Results:{Environment.NewLine}"
                   + string.Join(Environment.NewLine, results
                       .GroupBy(r => r.Value)
                       .OrderByDescending(g => g.Count())
                       .ThenBy(g => g.Key)
                       .Select(g => $"Value:{g.Key} {string.Join(", ", g.Select(r => r.Calculator.GetType().Name))}"));
        }
    }
}
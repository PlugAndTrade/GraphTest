using System.Collections.Generic;
using System.Linq;
using Tester.Interfaces;
using Tester.Model;

namespace Tester.Helpers
{
    public class GraphProvider
    {
        private readonly IGraphRepository[] _repositories;

        public GraphProvider(IGraphRepository[] repositories)
        {
            _repositories = repositories.ToArray();
        }

        public async IAsyncEnumerable<Graph> Get()
        {
            foreach (var repository in _repositories)
            {
                await foreach (var graph in repository.GetAll())
                {
                    yield return graph;
                }
            }
        }
    }
}
using System.Collections.Generic;
using Tester.Interfaces;
using Tester.Model;

namespace Tester.Helpers
{
    public class MemoryGraphRepository : IGraphRepository
    {
        private readonly Graph[] _graphs;

        public MemoryGraphRepository(Graph[] graphs)
        {
            _graphs = graphs;
        }
        
        public async IAsyncEnumerable<Graph> GetAll()
        {
            foreach (var graph in _graphs)
            {
                yield return graph;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Tester.Interfaces;
using Tester.Model;

namespace Tester.Implementations
{
    public class MasterGraphCalculator : IGraphCalculator
    {
        public int GetNumberOfNodes(Graph graph)
        {
            return graph.Nodes.Length + 1;
        }

        public int GetNumberOfChildlessNodes(Graph graph)
        {
            return graph.Nodes.Count(n => graph.Nodes.All(cn => cn.ParentId != n.Id));
        }

        public int GetMaxDepth(Graph graph)
        {
            var nodes = graph.Nodes;
            if (nodes.Length == 0)
                return 0;
                    
            var visitedNodes = new HashSet<string>();
            return nodes.Max(n => GetDepth(n, nodes, visitedNodes));
        }

        private int GetDepth(Node node, Node[] nodes, HashSet<string> visitedNodes)
        {
            if (node.ParentId == null || visitedNodes.Contains(node.Id))
                return 0;

            visitedNodes.Add(node.Id);
            var parent = nodes.Single(n => n.Id == node.ParentId);

            return GetDepth(parent, nodes, visitedNodes) + 1;
        }

        public int GetNumberOfSubGraphs(Graph graph)
        {
            return GetSubGraphs(graph).Length;
        }

        public int GetMaxConnectedNodes(Graph graph)
        {
            return GetSubGraphs(graph)
                .OrderByDescending(g => g.Count)
                .FirstOrDefault()?.Count ?? 0;
        }

        private SubGraph[] GetSubGraphs(Graph graph)
        {
            var subGraphs = new List<SubGraph>();
            foreach (var node in graph.Nodes)
            {
                var subGraph = node.ParentId != null
                    ? subGraphs.FirstOrDefault(g => g.Contains(node.ParentId))
                    : null;

                if (subGraph == null)
                {
                    subGraph = new SubGraph();
                    subGraphs.Add(subGraph);
                }

                subGraph.Add(node.Id);
            }
            return subGraphs.ToArray();
        }

        private class SubGraph
        {
            private readonly HashSet<string> _nodeIds;
            public int Count => _nodeIds.Count;

            public SubGraph()
            {
                _nodeIds = new HashSet<string>();
            }

            public bool Contains(string nodeId) => _nodeIds.Contains(nodeId);

            public void Add(string nodeId)
            {
                if (_nodeIds.Contains(nodeId))
                    return;

                _nodeIds.Add(nodeId);
            }
        }

    }
}
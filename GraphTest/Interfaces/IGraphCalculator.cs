using Tester.Model;

namespace Tester.Interfaces
{
    public interface IGraphCalculator
    {
        int GetNumberOfNodes(Graph graph);
        int GetNumberOfChildlessNodes(Graph graph);
        int GetMaxDepth(Graph graph);
        int GetNumberOfSubGraphs(Graph graph);
        int GetMaxConnectedNodes(Graph graph);
    }
}
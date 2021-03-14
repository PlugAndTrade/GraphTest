using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tester
{

    [TestClass]
    public class GraphCalculatorTests : BaseGraphCalculatorTests
    {
        [ClassInitialize]
        public new static void InitializeClass(TestContext context) => BaseGraphCalculatorTests.InitializeClass(context);


        [TestMethod]
        public async Task Nodes()
        {
            await RunTest((calculator, graph) => calculator.GetNumberOfNodes(graph));
        }

        [TestMethod]
        public async Task ChildlessNodes()
        {
            await RunTest((calculator, graph) => calculator.GetNumberOfChildlessNodes(graph));
        }

        [TestMethod]
        public async Task MaxDepth()
        {
            await RunTest((calculator, graph) => calculator.GetMaxDepth(graph));
        }

        [TestMethod]
        public async Task NumberOfSubGraphs()
        {
            await RunTest((calculator, graph) => calculator.GetNumberOfSubGraphs(graph));
        }
    }
}

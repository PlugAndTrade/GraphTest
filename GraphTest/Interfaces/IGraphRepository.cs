using System.Collections.Generic;
using Tester.Model;

namespace Tester.Interfaces
{
    public interface IGraphRepository
    {
        IAsyncEnumerable<Graph> GetAll();
    }
}
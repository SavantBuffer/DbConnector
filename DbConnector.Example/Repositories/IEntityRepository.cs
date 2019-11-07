using DbConnector.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbConnector.Example.Repositories
{
    public interface IEntityRepository<T> where T : new()
    {
        Task<IDbResult<List<T>>> GetAll();
        Task<IDbResult<T>> Get();
    }
}

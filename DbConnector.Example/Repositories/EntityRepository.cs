using DbConnector.Core;
using DbConnector.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace DbConnector.Example.Repositories
{
    public abstract class EntityRepository<T> : IEntityRepository<T>
        where T : new()
    {
        protected readonly IDbConnector _dbConnector;

        public EntityRepository(IDbConnector dbConnector)
        {
            _dbConnector = dbConnector;
        }

        public virtual Task<IDbResult<List<T>>> GetAll()
        {
            return _dbConnector.ReadToList<T>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = "Select * from " + typeof(T).GetAttributeValue((TableAttribute ta) => ta.Name) ?? typeof(T).Name;

               }).ExecuteHandledAsync();
        }

        public virtual Task<IDbResult<T>> Get()
        {
            return _dbConnector.ReadFirstOrDefault<T>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = "Select * from "
                   + typeof(T).GetAttributeValue((TableAttribute ta) => ta.Name) ?? typeof(T).Name;

               }).ExecuteHandledAsync();
        }

    }
}

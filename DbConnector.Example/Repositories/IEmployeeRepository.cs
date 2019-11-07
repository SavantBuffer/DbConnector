using DbConnector.Example.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DbConnector.Example.Repositories
{
    public interface IEmployeeRepository : IEntityRepository<Employee>
    {
        Task<List<Employee>> GetAllSimple();

        Task<Employee> Get(int id);

        Task<DataTable> GetByDataTable();

        Task<(IEnumerable<Employee>, IEnumerable<Employee>)> GetEmployeesByMultiReaders();

        Task<(List<Employee>, List<Employee>)> GetEmployeesByBatch();

        Task<(Employee, Employee)> GetSingleEmployeesByBatch(int employeeId1, int employeeId2);

        Task<int?> Insert(Employee entity);
    }
}

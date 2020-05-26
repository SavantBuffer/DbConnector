using DbConnector.Core;
using DbConnector.Core.Extensions;
using DbConnector.Example.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;


namespace DbConnector.Example.Repositories
{
    /// <summary>
    /// This class architecture allows for the querying of any type of database.
    /// </summary>
    public class EmployeeRepository
        : EntityRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IDbConnector dbConnector)
            : base(dbConnector)
        {

        }

        public override Task<IDbResult<List<Employee>>> GetAll()
        {
            return _dbConnector.ReadToList<Employee>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = "SELECT * FROM Employees";

                   cmd.Parameters.AddFor(new { x = 2 });

               }).ExecuteHandledAsync();
        }

        public Task<List<Employee>> GetAllSimple()
        {
            return _dbConnector.ReadToList<Employee>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = "SELECT * FROM Employees";

               }).ExecuteAsync();
        }

        public Task<Employee> Get(int id)
        {
            return _dbConnector.ReadFirstOrDefault<Employee>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.StoredProcedure;
                   cmd.CommandText = "dbo.GetEmployee";

                   cmd.Parameters.AddWithValue("id", id);

               }).ExecuteAsync();
        }

        public Task<DataTable> GetByDataTable()
        {
            return _dbConnector.ReadToDataTable(
               onInit: (cmd) =>
               {
                   cmd.CommandText = "SELECT * FROM Employees";

               }).ExecuteAsync();
        }

        public Task<(IEnumerable<Employee>, IEnumerable<Employee>)> GetEmployeesByMultiReaders()
        {
            return _dbConnector.Read<Employee, Employee>(
               onInit: () => (
                (cmd1) =>
                {
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.CommandText = @"
                        SELECT * FROM Employees WHERE name LIKE 'a%'; 
                    ";
                }
               ,
                (cmd2) =>
                {
                    cmd2.CommandType = System.Data.CommandType.Text;
                    cmd2.CommandText = @"
                        SELECT * FROM Employees WHERE name LIKE 'b%';
                    ";
                }
               )).ExecuteAsync();
        }

        public Task<(List<Employee>, List<Employee>)> GetEmployeesByBatch()
        {
            return _dbConnector.ReadToList<Employee, Employee>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = @"
                                        SELECT * FROM Employees WHERE name LIKE 'a%'; 
                                        SELECT * FROM Employees WHERE name LIKE 'b%';
                                     ";
               }).ExecuteAsync();
        }

        public Task<(Employee, Employee)> GetSingleEmployeesByBatch(int employeeId1, int employeeId2)
        {
            return _dbConnector.ReadFirstOrDefault<Employee, Employee>(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = @"
                                        SELECT * FROM Employees WHERE id = @employeeId1; 
                                        SELECT * FROM Employees WHERE id = @employeeId2;
                                    ";

                   cmd.Parameters.AddFor(
                       new
                       {
                           employeeId1,
                           employeeId2
                       }
                   );

               }).ExecuteAsync();
        }

        public Task<int?> Insert(Employee entity)
        {
            return _dbConnector.NonQuery(
               onInit: (cmd) =>
               {
                   cmd.CommandType = System.Data.CommandType.Text;
                   cmd.CommandText = @"INSERT INTO "
                    + typeof(Employee).GetAttributeValue((TableAttribute ta) => ta.Name) ?? typeof(Employee).Name
                    + " (name, modified_date)"
                    + " VALUES(@Name, @ModifiedDate)";

                   cmd.Parameters.AddFor(new { entity.Name, entity.ModifiedDate });

               }).ExecuteAsync();
        }
    }
}

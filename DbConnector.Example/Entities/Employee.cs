using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DbConnector.Example.Entities
{
    public class Employee
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [NotMapped]
        public int NotMappedExample { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbConnector.Tests.Entities
{
    [Table("Employee", Schema = "HumanResources")]
    public partial class Employee
    {
        public Employee()
        {
            EmployeeDepartmentHistory = new HashSet<EmployeeDepartmentHistory>();
            EmployeePayHistory = new HashSet<EmployeePayHistory>();
            JobCandidate = new HashSet<JobCandidate>();
            PurchaseOrderHeader = new HashSet<PurchaseOrderHeader>();
        }

        [Column("BusinessEntityID")]
        public int BusinessEntityId { get; set; }
        [Required]
        [Column("NationalIDNumber")]
        [StringLength(15)]
        public string NationalIdnumber { get; set; }
        [Required]
        [Column("LoginID")]
        [StringLength(256)]
        public string LoginId { get; set; }
        public short? OrganizationLevel { get; set; }
        [Required]
        [StringLength(50)]
        public string JobTitle { get; set; }
        [Column(TypeName = "date")]
        public DateTime BirthDate { get; set; }
        [Required]
        [StringLength(1)]
        public string MaritalStatus { get; set; }
        [Required]
        [StringLength(1)]
        public string Gender { get; set; }
        [Column(TypeName = "date")]
        public DateTime HireDate { get; set; }
        [Required]
        public bool? SalariedFlag { get; set; }
        public short VacationHours { get; set; }
        public short SickLeaveHours { get; set; }
        [Required]
        public bool? CurrentFlag { get; set; }
        [Column("rowguid")]
        public Guid Rowguid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [ForeignKey("BusinessEntityId")]
        [InverseProperty("Employee")]
        public virtual Person BusinessEntity { get; set; }
        [InverseProperty("BusinessEntity")]
        public virtual SalesPerson SalesPerson { get; set; }
        [InverseProperty("BusinessEntity")]
        public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
        [InverseProperty("BusinessEntity")]
        public virtual ICollection<EmployeePayHistory> EmployeePayHistory { get; set; }
        [InverseProperty("BusinessEntity")]
        public virtual ICollection<JobCandidate> JobCandidate { get; set; }
        [InverseProperty("Employee")]
        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
    }

    public enum GenderType
    {
        F = 1,
        M = 2,
        None = 0
    }

    public enum MaritalStatusType
    {
        s = 1,
        M = 2,
        None = 0
    }

    public enum SalariedFlagType
    {
        T = 1,
        F = 0
    }

    [Table("Employee", Schema = "HumanResources")]
    public class IndirectMatchEmployeeTestModel
    {
        [Column("BusinessEntityID")]
        public uint BusinessEntityId { get; set; }

        public long VacationHours { get; set; }

        public int? SickLeaveHours { get; set; }

        public GenderType Gender { get; set; }

        public MaritalStatusType? MaritalStatus { get; set; }

        public SalariedFlagType SalariedFlag { get; set; }
    }

    public partial struct EmployeeStruct
    {
        [Column("BusinessEntityID")]
        public int BusinessEntityId { get; set; }
        [Required]
        [Column("NationalIDNumber")]
        [StringLength(15)]
        public string NationalIdnumber { get; set; }
        [Required]
        [Column("LoginID")]
        [StringLength(256)]
        public string LoginId { get; set; }
        public short? OrganizationLevel { get; set; }
        [Required]
        [StringLength(50)]
        public string JobTitle { get; set; }
        [Column(TypeName = "date")]
        public DateTime BirthDate { get; set; }
        [Required]
        [StringLength(1)]
        public string MaritalStatus { get; set; }
        [Required]
        [StringLength(1)]
        public string Gender { get; set; }
        [Column(TypeName = "date")]
        public DateTime HireDate { get; set; }
        [Required]
        public bool? SalariedFlag { get; set; }
        public short VacationHours { get; set; }
        public short SickLeaveHours { get; set; }
        [Required]
        public bool? CurrentFlag { get; set; }
        [Column("rowguid")]
        public Guid Rowguid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }
    }
}

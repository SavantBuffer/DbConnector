﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbConnector.Tests.Entities
{
    [Table("SalesOrderHeader", Schema = "Sales")]
    public partial class SalesOrderHeader
    {
        public SalesOrderHeader()
        {
            SalesOrderDetail = new HashSet<SalesOrderDetail>();
            SalesOrderHeaderSalesReason = new HashSet<SalesOrderHeaderSalesReason>();
        }

        [Column("SalesOrderID")]
        public int SalesOrderId { get; set; }
        public byte RevisionNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime OrderDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ShipDate { get; set; }
        public byte Status { get; set; }
        [Required]
        public bool? OnlineOrderFlag { get; set; }
        [Required]
        [StringLength(25)]
        public string SalesOrderNumber { get; set; }
        [StringLength(25)]
        public string PurchaseOrderNumber { get; set; }
        [StringLength(15)]
        public string AccountNumber { get; set; }
        [Column("CustomerID")]
        public int CustomerId { get; set; }
        [Column("SalesPersonID")]
        public int? SalesPersonId { get; set; }
        [Column("TerritoryID")]
        public int? TerritoryId { get; set; }
        [Column("BillToAddressID")]
        public int BillToAddressId { get; set; }
        [Column("ShipToAddressID")]
        public int ShipToAddressId { get; set; }
        [Column("ShipMethodID")]
        public int ShipMethodId { get; set; }
        [Column("CreditCardID")]
        public int? CreditCardId { get; set; }
        [StringLength(15)]
        public string CreditCardApprovalCode { get; set; }
        [Column("CurrencyRateID")]
        public int? CurrencyRateId { get; set; }
        [Column(TypeName = "money")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "money")]
        public decimal TaxAmt { get; set; }
        [Column(TypeName = "money")]
        public decimal Freight { get; set; }
        [Column(TypeName = "money")]
        public decimal TotalDue { get; set; }
        [StringLength(128)]
        public string Comment { get; set; }
        [Column("rowguid")]
        public Guid Rowguid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [ForeignKey("BillToAddressId")]
        [InverseProperty("SalesOrderHeaderBillToAddress")]
        public virtual Address BillToAddress { get; set; }
        [ForeignKey("CreditCardId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual CreditCard CreditCard { get; set; }
        [ForeignKey("CurrencyRateId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual CurrencyRate CurrencyRate { get; set; }
        [ForeignKey("CustomerId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual Customer Customer { get; set; }
        [ForeignKey("SalesPersonId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual SalesPerson SalesPerson { get; set; }
        [ForeignKey("ShipMethodId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual ShipMethod ShipMethod { get; set; }
        [ForeignKey("ShipToAddressId")]
        [InverseProperty("SalesOrderHeaderShipToAddress")]
        public virtual Address ShipToAddress { get; set; }
        [ForeignKey("TerritoryId")]
        [InverseProperty("SalesOrderHeader")]
        public virtual SalesTerritory Territory { get; set; }
        [InverseProperty("SalesOrder")]
        public virtual ICollection<SalesOrderDetail> SalesOrderDetail { get; set; }
        [InverseProperty("SalesOrder")]
        public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbConnector.Tests.Entities
{
    [Table("ProductReview", Schema = "Production")]
    public partial class ProductReview
    {
        [Column("ProductReviewID")]
        public int ProductReviewId { get; set; }
        [Column("ProductID")]
        public int ProductId { get; set; }
        [Required]
        [StringLength(50)]
        public string ReviewerName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ReviewDate { get; set; }
        [Required]
        [StringLength(50)]
        public string EmailAddress { get; set; }
        public int Rating { get; set; }
        [StringLength(3850)]
        public string Comments { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [ForeignKey("ProductId")]
        [InverseProperty("ProductReview")]
        public virtual Product Product { get; set; }
    }
}

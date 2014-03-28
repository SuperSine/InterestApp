using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InterestApp.Models
{
    public class TransactionDetail
    {
        public TransactionDetail() {
            this.EnableFlag = true;
            this.CreateTime = DateTime.Now;
            this.VailedTime = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        [Display(Name="业务类型")]
        public string Type { get; set; }
        
        [Display(Name = "业务金额")]
        public double Amount { get; set; }
        
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "生效时间")]
        public DateTime VailedTime { get; set; }

        public bool EnableFlag { get; set; }

        [Display(Name = "备注")]
        public string Mark { get; set; }

        [Display(Name="对应借款记录")]
        [ForeignKey("InterestMaster")]
        public int InterestMasterId { get; set; }
        public virtual InterestMaster InterestMaster { get; set; }
        public virtual TransactionDetail SubTransactionDetail { get; set; }
    }
}
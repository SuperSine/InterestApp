using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace InterestApp.Models
{
    public class RateDetail
    {

        public RateDetail() {
            this.CreateTime = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }
        public float Rate { get; set; }

        public DateTime Since { get; set; }
        public DateTime CreateTime { get; set; }

        [ForeignKey("InterestMaster")]
        public int InterestMasterId { get; set; }
        public virtual InterestMaster InterestMaster { get; set; }
    }
}
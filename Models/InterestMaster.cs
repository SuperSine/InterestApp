using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InterestApp.Models
{
    public class InterestMaster
    {
        public InterestMaster() {
            this.CreateTime = DateTime.Now;
            
            this.EnableFlag = true;
            
            this.TransactionDetails = new List<TransactionDetail>();
            this.RateDetails = new List<RateDetail>();
        }

        private float _mpr;
        private float _eMpr;

        [Key]
        public int Id { get; set; }

        [Display(Name = "标识名称")]
        [Required]
        public string ImName { get; set; }

        [Display(Name="放款单位")]
        public string LoanUnit { get; set; }
        
        [Display(Name = "单位类型")]
        public string LoanType { get; set; }
        
        [NotMapped]
        public SelectList LoanTypeList { get; set; }

        [Display(Name="合同编号")]
        public string ContractNumber { get; set; }
        
        [Display(Name = "收据编号")]
        public string ReceiptNumber { get; set; }
        
        [Display(Name = "汇款类型")]
        public string RemitType { get; set; }
        
        [Display(Name = "经办人")]
        public string Assignee { get; set; }
        
        [Display(Name = "汇款账户")]
        public string RemitAccount { get; set; }
        
        [Display(Name = "款项存入的账户名称")]
        public string SaveAccount { get; set; }

        [Display(Name = "开户银行")]
        public string HostBank { get; set; }
        
        [Display(Name = "账号/卡号")]
        public string AccountId { get; set; }

        [Display(Name = "货币类型")]
        public string CurrencyType { get; set; }

        [Display(Name = "借入本金")]
        [NotMapped]
        public double CurrencyAmount { get; set; }

        [Display(Name = "年利率")]
        [NotMapped]
        public float Apr { get; set; }
        [Display(Name = "月利率")]
        public float Mpr { 
            get {
                return this._mpr;
            }

            set {
                if (value != null) {
                    this.Dpr = value / DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                    this.Apr = value * 12;
                    this._mpr = value;
                }
            } 
        }
        [Display(Name = "日利率")]
        [NotMapped]
        public float Dpr { get; set; }
        [Display(Name = "还款周期")]
        public float CycleOfPayment { get; set; }
        [Display(Name = "约定还本日期")]
        public DateTime AgreedBackDate { get; set; }
        [Display(Name = "逾期月利率")]
        public float ExpiredMpr {
            get
            {
                return this._eMpr;
            }

            set
            {
                if (value != null)
                {
                    this.ExpiredDpr = value / DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                    this.ExpiredApr = value * 12;
                    this._eMpr = value;
                }
            }         
        }

        [NotMapped]
        public float ExpiredApr { get; set; }
        [NotMapped]
        public float ExpiredDpr { get; set; }

        [Display(Name = "应付利息")]
        [NotMapped]
        public double PayableInterest { get; set; }
        [Display(Name = "借入日期")]
        public DateTime BorrowedTime { get; set; }
        [Display(Name = "起息日期")]
        public DateTime StartTime { get; set; }
        [Display(Name="备注")]
        public string Mark { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        
        public DateTime? LastIncrIntrst { get; set; }
        
        public bool EnableFlag { get; set; }
        
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
        public virtual ICollection<RateDetail> RateDetails { get; set; }
    }

    public class TinyInterestMaster {
        public int Id { get; set; }

        [Display(Name = "标识名称")]
        public string ImName { get; set; }

        [Display(Name = "放款单位")]
        public string LoanUnit { get; set; }

        [Display(Name = "借入本金")]
        public double CapitalAmount { get; set; }

        [Display(Name = "月利率")]
        public float Mpr { get; set; }

        [Display(Name = "起息日期")]
        public DateTime StartTime { get; set; }

        [Display(Name = "应付利息")]
        public double PayableInterest { get; set; }

        [Display(Name = "增量利息")]
        public double DeltaInterest { get; set; }

        [Display(Name = "结算利息")]
        public double InterestAmount { get; set; }

    }

    public class InterestMasterDBContext : DbContext
    {
        public DbSet<InterestMaster> InterestMasters { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }

        public DbSet<RateDetail> RateDetails { get; set; }
    }

    public class General { 
        public const string IncrInt  = "I1";
        public const string IncrCptl = "I2";
        public const string DecrInt  = "D1";
        public const string DecrCptl = "D2";

        public static double getInterest(int iMid, InterestMasterDBContext db) {
            /*var query = from x in db.TransactionDetails.Where(x =>
                            x.InterestMasterId == iMid &&
                            (x.Type == General.IncrInt || x.Type == General.DecrInt)).Select(x => x.Type == General.IncrInt ? x.Amount : x.Amount * -1)
                        from y in db.TransactionDetails.Where (y =>
                                y.InterestMasterId == iMid
                                    && (y.Type == General.IncrCptl || y.Type == General.DecrCptl)
                              ).Select(y => y.Type == General.IncrCptl ? y.Amount : y.Amount * -1)
                        from z in db.InterestMasters.Where(o => o.Id == iMid).Select(o => 
                                       ((DateTime.Now > o.AgreedBackDate ? (DateTime.Now.Date - o.AgreedBackDate).Days : 0) * o.ExpiredDpr + 
                                       (DateTime.Now > o.AgreedBackDate ? (o.AgreedBackDate - o.StartTime).Days : (DateTime.Now.Date - o.StartTime).Days) * o.Dpr) 
                                       / (DateTime.Now.Date - o.StartTime).Days).Take(1)
                        from z1 in db.TransactionDetails.Where(o => o.Id == iMid && o.Type == General.IncrInt).OrderByDescending(o => o.VailedTime).Take(1)
                        select new { x+(DateTime.Now - z1 ? )};*/
            var now = DateTime.Now;
            var query = from im in db.InterestMasters
                        where im.Id == iMid
                        join itst in
                            (from it in db.TransactionDetails where it.Type == General.IncrInt || it.Type == General.DecrInt group it by it.InterestMasterId into i select new { key = i.Key, totalItst = i.Sum(x => x.Type == General.IncrInt ? x.Amount : x.Amount * -1) })
                        on im.Id equals itst.key into itstJoin
                        join cptl in
                            (from cp in db.TransactionDetails where cp.Type == General.IncrCptl || cp.Type == General.DecrCptl group cp by cp.InterestMasterId into i select new { key = i.Key, totalCpl = i.Sum(x => x.Type == General.IncrCptl ? x.Amount : x.Amount * -1) })
                        on im.Id equals cptl.key into cptlJoin
                        from day in db.TransactionDetails.Where(o => o.Id == iMid && o.Type == General.IncrInt).OrderByDescending(o => o.VailedTime).Select(o => o.VailedTime).Take(1)
                        from rate in db.InterestMasters.Where(o => o.Id == iMid ).Select(o =>
                                       ((now > o.AgreedBackDate ? DbFunctions.DiffDays(now,o.AgreedBackDate) : 0) * 0.1 +
                                       (now > o.AgreedBackDate ? DbFunctions.DiffDays(o.AgreedBackDate, o.StartTime) : DbFunctions.DiffDays(now.Date, o.StartTime)) * 0.2)
                                       / DbFunctions.DiffDays(now,o.StartTime)).Take(1)
                        select new
                        {
                            /*start = (day != null ? day : im.StartTime),*/
                            interest = itstJoin.Select(o=>o.totalItst).FirstOrDefault(),
                            capital = cptlJoin.Select(o => o.totalCpl).FirstOrDefault(),
                            rate,
                        };

            return query.Select(o => o.interest + (5 * (o.rate.HasValue ? (double)o.rate : 0.0) * (o.capital))).Single();
        }

        public static double getCapital(int iMid, InterestMasterDBContext db)
        {
            var query = from x in db.TransactionDetails 
                        where (
                                x.InterestMasterId == iMid 
                                    && (x.Type == General.IncrCptl || x.Type == General.DecrCptl)
                              ) 
                        select x;
            double cap = query.Sum(x => x.Type == General.IncrCptl ? x.Amount : x.Amount * -1);

            return cap;
        }

        public static int getDays(InterestMaster im) {
            int days = (DateTime.Now.Date - im.StartTime.Date).Days;

            return days;
        }

        public static TinyInterestMaster getDeltaRate(int iMid, DateTime dateTime,InterestMasterDBContext db) {
            var samples = db.Database.SqlQuery<TinyInterestMaster>(@"
SELECT (SUM(mt.rate * mt.duration) * cpint.CapitalAmount * cpint.InterestAmount) as PayableInterest,cpint.InterestAmount,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime,cpint.CapitalAmount,mt.Id
FROM (
	SELECT LastIncrIntrst
		,StartTime
		,base.Id
		,DATEDIFF(DAY, a.Since, isnull(b.Since, {1})) AS duration
		,a.Rate
		,ImName
		,LoanUnit
		,Mpr
	FROM InterestMasters base
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) AS oId
			,*
		FROM RateDetails
		) a ON a.InterestMasterId = base.Id
		AND a.Since >= isnull(LastIncrIntrst, StartTime)
	LEFT JOIN (
		SELECT ROW_NUMBER() OVER (
				ORDER BY [Id] ASC
				) - 1 AS oId
			,*
		FROM RateDetails
		) b ON a.oId = b.oId
		AND a.Since >= isnull(LastIncrIntrst, StartTime)
	) mt
LEFT JOIN (
	SELECT isnull(SUM(CASE 
					WHEN [Type] = 'I2'
						THEN Amount
					WHEN [Type] = 'D2'
						THEN Amount * - 1
					END), 0) AS CapitalAmount
		,isnull(SUM(CASE 
					WHEN [Type] = 'I1'
						THEN Amount
					WHEN [Type] = 'D1'
						THEN Amount * - 1
					END), 0) AS InterestAmount
		,InterestMasterId
	FROM TransactionDetails
	WHERE [Type] = 'I2'
		OR [Type] = 'D2'
		OR [Type] = 'I1'
		OR [Type] = 'D1'
	GROUP BY InterestMasterId
	) cpint ON cpint.InterestMasterId = mt.Id
 WHERE mt.Id = {0}
GROUP BY mt.Id
	,CapitalAmount,InterestAmount
	,mt.ImName,mt.LoanUnit,mt.Mpr,mt.StartTime
", iMid,dateTime).ToList();
            return samples.Single();

        }

        public static TinyInterestMaster getInterestByDate(int iMid, DateTime dateTime,InterestMasterDBContext db) {
            string path = HttpContext.Current.Server.MapPath("/App_Data/queryInterestByDate.sql");
            string query = File.ReadAllText(path);

            var samples = db.Database.SqlQuery<TinyInterestMaster>(query, iMid, dateTime);
            if (samples.Count() == 1) { 
                return samples.Single();
            }
            return new TinyInterestMaster(); 
        }

        public static TinyInterestMaster getTinyInstMst(int iMid, InterestMasterDBContext db) {
            string path = HttpContext.Current.Server.MapPath("/App_Data/queryTinyInstMst.sql");
            string query = File.ReadAllText(path);
            var samples = db.Database.SqlQuery<TinyInterestMaster>(query, iMid);
            return samples.Single();
        }

        public static IEnumerable<TinyInterestMaster> getTinyInstMstList(InterestMasterDBContext db)
        {
            string path = HttpContext.Current.Server.MapPath("/App_Data/queryTinyInstMstList.sql");
            string query = File.ReadAllText(path);
            var samples = db.Database.SqlQuery<TinyInterestMaster>(query).ToList();
            return samples.Select(x=>x);
        }

        public static float getRate(InterestMaster im)
        {
            float sumRate;
            float rate;
            int expiredDays = (DateTime.Now.Date - im.AgreedBackDate).Days;
            int agreedDays  = (im.AgreedBackDate - im.StartTime).Days;
            int totalDays   = (DateTime.Now.Date - im.StartTime).Days;

            sumRate = (totalDays > agreedDays ? expiredDays : 0) * im.ExpiredDpr + 
                      (totalDays > agreedDays ? agreedDays : totalDays) * im.Dpr;
            rate = sumRate / totalDays;

            return rate;

        }

    }
}
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
        private double _currencyAmount;
        private double _payableInterest;

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
        public double CurrencyAmount { get { return Math.Round(this._currencyAmount,General.RoundDigit); } set { this._currencyAmount = value; } }

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
        [Display(Name = "结息周期")]
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
        public double PayableInterest { get { return Math.Round(this._payableInterest, General.RoundDigit); } set { this._payableInterest = value; } }
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
        private double _capitalAmount;
        private double _paidCapital;
        private double _paiedInterest;
        private double _interestAmount;
        private double _deltaInterest;
        private double _payableInterest;
        public int Id { get; set; }

        [Display(Name = "标识名称")]
        public string ImName { get; set; }

        [Display(Name = "放款单位")]
        public string LoanUnit { get; set; }

        [Display(Name = "借入本金")]
        public double CapitalAmount { get { return Math.Round(this._capitalAmount, General.RoundDigit); } set { this._capitalAmount = value; } }

        [Display(Name = "月利率")]
        public float Mpr { get; set; }

        [Display(Name = "起息日期")]
        public DateTime StartTime { get; set; }

        [Display(Name = "应付利息")]
        public double PayableInterest { get { return Math.Round(this._payableInterest, General.RoundDigit); } set { this._payableInterest = value; } }

        [Display(Name = "增量利息")]
        public double DeltaInterest { get { return Math.Round(this._deltaInterest, General.RoundDigit); } set { this._deltaInterest = value; } }

        [Display(Name = "结算利息")]
        public double InterestAmount { get { return Math.Round(this._interestAmount, General.RoundDigit); } set { this._interestAmount = value; } }

        [Display(Name="最近结息日期")]
        public DateTime LastPayableDate { get; set; }

        [Display(Name="当前利率")]
        public float CurrentRate { get; set; }

        [Display(Name = "已付利息")]
        public double PaiedInterest { get { return Math.Round(this._paiedInterest, General.RoundDigit); } set { this._paiedInterest = value; } }

        [Display(Name = "已付本金")]
        public double PaiedCapital { get { return Math.Round(this._paidCapital, General.RoundDigit); } set { this._paidCapital = value; } }
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

        public const int RoundDigit = 2;

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

    }
}
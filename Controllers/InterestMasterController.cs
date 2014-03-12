using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InterestApp.Models;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;

namespace InterestApp.Controllers
{
    [Authorize]
    public class InterestMasterController : Controller
    {
        private InterestMasterDBContext db = new InterestMasterDBContext();
        private const string bindList = "Id,ImName,LoanUnit,LoanType,ContractNumber,ReceiptNumber,RemitType,Assignee,RemitAccount,SaveAccount,HostBank,AccountId,CurrencyType,Mpr,CycleOfPayment,AgreedBackDate,BorrowedTime,StartTime,Mark,ExpiredMpr,CurrencyAmount";

        private const int BaseNumber = 100;
        private string _userId;

        public InterestMasterController() {
            InitiliazeSelectList();
        }


        public  string GetPropertyName<TModel,Out>
           (Expression<Func<TModel, Out>> propertyNameExpr)
        {
            return ExpressionHelper.GetExpressionText(propertyNameExpr);
        }

        // GET: /InterestMaster/
        public ActionResult Index(DateTime? startDate,DateTime? endDate)
        {

            ViewBag.StartDate = startDate.Equals(null) ? null : startDate;
            ViewBag.EndDate = endDate.Equals(null) ? null : endDate;
            
            if (startDate == null) startDate = DateTime.MinValue;
            if (endDate == null) endDate = DateTime.MaxValue;

            
            
            this._userId = User.Identity.GetUserId();
            return View(General.getTinyInstMstList(db).Where(e=>e.LastPayableDate >= startDate && e.LastPayableDate <= endDate).ToList());
        }

        // GET: /InterestMaster/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InterestMaster interestmaster = db.InterestMasters.Find(id);

//            string propertyName = this.GetPropertyName<TransactionDetail,DateTime>(t=>t.VailedTime);

            if (interestmaster == null)
            {
                return HttpNotFound();
            }

            //double interest = General.getInterest(interestmaster.Id, db);
            TinyInterestMaster tinyInstMst = General.getTinyInstMst(interestmaster.Id, db);
            interestmaster.PayableInterest = tinyInstMst.PayableInterest;
            interestmaster.CurrencyAmount = tinyInstMst.CapitalAmount;
            //TinyInterestMaster rate = General.getDeltaRate(interestmaster.Id, db);

            return View(interestmaster);
        }

        // GET: /InterestMaster/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /InterestMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = bindList)] InterestMaster interestmaster)
        {
            if (ModelState.IsValid)
            {
                double amount = interestmaster.CurrencyAmount;
                interestmaster.Mpr = interestmaster.Mpr / BaseNumber;
                interestmaster.ExpiredMpr = interestmaster.ExpiredMpr / BaseNumber;

                interestmaster.TransactionDetails.Add(new TransactionDetail { Amount = amount, Type = General.IncrCptl});

                interestmaster.RateDetails.Add(new RateDetail { Rate = interestmaster.Dpr, Since = interestmaster.StartTime});
                interestmaster.RateDetails.Add(new RateDetail { Rate = interestmaster.ExpiredDpr, Since = interestmaster.AgreedBackDate});
                
                db.InterestMasters.Add(interestmaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(interestmaster);
        }

        // GET: /InterestMaster/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InterestMaster interestmaster = db.InterestMasters.Find(id);
            if (interestmaster == null)
            {
                return HttpNotFound();
            }
            return View(interestmaster);
        }

        // POST: /InterestMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = bindList)] InterestMaster interestmaster)
        {
            
            if (ModelState.IsValid)
            {
                db.Entry(interestmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(interestmaster);
        }

        // GET: /InterestMaster/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InterestMaster interestmaster = db.InterestMasters.Find(id);
            if (interestmaster == null)
            {
                return HttpNotFound();
            }
            return View(interestmaster);
        }

        // POST: /InterestMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InterestMaster interestmaster = db.InterestMasters.Find(id);
            db.InterestMasters.Remove(interestmaster);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitiliazeSelectList()
        {
            List<SelectListItem> loanTypeList = new List<SelectListItem>()
	        {
	            new SelectListItem(){Text = "个人",    Value = "L1"},
                new SelectListItem(){Text = "公司",    Value = "L2"},
                new SelectListItem(){Text = "金融机构", Value = "L3"},
	        };
            List<SelectListItem> remitTypeList = new List<SelectListItem>()
	        {
                new SelectListItem(){Text = "现金",Value = "R1"},
                new SelectListItem(){Text = "银行",Value = "R2"},
	        };

            List<SelectListItem> currencyTypeList = new List<SelectListItem>()
	        {
                new SelectListItem(){Text = "RMB",Value = "RMB"},
	        };

            ViewBag.LoanTypeList = new SelectList(loanTypeList, "Value", "Text");
            ViewBag.RemitTypeList = new SelectList(remitTypeList, "Value", "Text");
            ViewBag.CurrencyTypeList = new SelectList(currencyTypeList, "Value", "Text");
            ViewBag.BaseNumber = BaseNumber;
        }
    }
}

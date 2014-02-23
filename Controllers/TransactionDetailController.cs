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

namespace InterestApp.Controllers
{
    public static class MvcExtensions
    {
        public static string GetPropertyName<TModel,Out>
         (this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, Out>> propertyNameExpr)
        {
            var expression = ExpressionHelper.GetExpressionText(propertyNameExpr);
            return expression.Substring(expression.LastIndexOf('.') + 1);
        }
    }
    public class TransactionDetailController : Controller
    {
        private InterestMasterDBContext db = new InterestMasterDBContext();
        private const string bindList = "Id,Type,Amount,InterestMasterId,Mark,VailedTime";

        public TransactionDetailController() {
            InitiliazeSelectList();
        }

        public string GetPropertyName<TModel, Out>(Expression<Func<TModel, Out>> propertyNameExpr)
        {
            return ExpressionHelper.GetExpressionText(propertyNameExpr);
        }

        public JsonResult getInterestAmount(int iMid, DateTime dateTime)
        {
            TinyInterestMaster tinyInstMst = General.getInterestByDate(iMid, dateTime, db);

            return Json(new { 
                            success = true, 
                            capitalamount = tinyInstMst.CapitalAmount, 
                            payableinterest = tinyInstMst.PayableInterest
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: /TransactionDetail/
        public ActionResult Index()
        {
            return View(db.TransactionDetails.ToList());
        }

        // GET: /TransactionDetail/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransactionDetail paybackdetail = db.TransactionDetails.Find(id);
            if (paybackdetail == null)
            {
                return HttpNotFound();
            }
            return View(paybackdetail);
        }

        // GET: /TransactionDetail/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: /TransactionDetail/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = bindList)] TransactionDetail paybackdetail)
        {


            if (ModelState.IsValid)
            {
                List<TransactionDetail> paybackdetails = new List<TransactionDetail>();
                TransactionDetail balance = null;

                InterestMaster interestMaster = db.InterestMasters.Find(paybackdetail.InterestMasterId);

                double interest = 0;
                TinyInterestMaster tIntstMst = General.getInterestByDate(paybackdetail.InterestMasterId, paybackdetail.VailedTime, db);
                tIntstMst.DeltaInterest = tIntstMst.PayableInterest - tIntstMst.InterestAmount;
                interest = tIntstMst.DeltaInterest;
               // double interest = General.getRateEx(paybackdetail.InterestMasterId,db);

                switch(paybackdetail.Type){
                    case "1":
                        if (paybackdetail.VailedTime > DateTime.Now)
                        {
                            ModelState.AddModelError(this.GetPropertyName<TransactionDetail, DateTime>(t => t.VailedTime), "付息日期不能大于当前日期");
                            return View(paybackdetail);
                        }

                        if (paybackdetail.Amount > tIntstMst.PayableInterest) {
                            ModelState.AddModelError(this.GetPropertyName<TransactionDetail,double>(t=>t.Amount),"付息总额不能大于应付利息");
                            return View(paybackdetail);
                        }

                        paybackdetail.Type = General.DecrInt;
                        break;

                    case "2":
                        if (paybackdetail.Amount > tIntstMst.CapitalAmount)
                        {
                            ModelState.AddModelError(this.GetPropertyName<TransactionDetail, double>(t => t.Amount), "还本金额不能大于应付本金");
                            return View(paybackdetail);
                        }

                        balance = new TransactionDetail();
                        balance.Amount = interest;
                        balance.Type = General.IncrInt;

                        paybackdetail.Type = General.DecrCptl;
                        interestMaster.LastIncrIntrst = paybackdetail.VailedTime; 

                        db.InterestMasters.Attach(interestMaster);
                        var entry = db.Entry(interestMaster);
                        entry.Property(e => e.LastIncrIntrst).IsModified = true;
                        
                        break;

                    case "3":
                        if (paybackdetail.Amount > tIntstMst.PayableInterest) {
                            ModelState.AddModelError(this.GetPropertyName<TransactionDetail,double>(t=>t.Amount),"转利息金额不能大于应付利息");
                            return View(paybackdetail);
                        }

                        balance = new TransactionDetail();

                        balance.Amount = paybackdetail.Amount;
                        balance.Type = General.DecrInt;

                        paybackdetail.Type = General.IncrCptl;

                        break;

                    default: break;
                }


                if (balance != null) {
                    balance.InterestMasterId = paybackdetail.InterestMasterId;
                    paybackdetails.Add(balance); 
                }
                paybackdetails.Add(paybackdetail);                

                db.TransactionDetails.AddRange(paybackdetails);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paybackdetail);
        }

        // GET: /TransactionDetail/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransactionDetail paybackdetail = db.TransactionDetails.Find(id);
            if (paybackdetail == null)
            {
                return HttpNotFound();
            }
            return View(paybackdetail);
        }

        // POST: /TransactionDetail/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Type,Amount,InterestMasterId,Mark")] TransactionDetail paybackdetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(paybackdetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paybackdetail);
        }

        // GET: /TransactionDetail/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TransactionDetail paybackdetail = db.TransactionDetails.Find(id);
            if (paybackdetail == null)
            {
                return HttpNotFound();
            }
            return View(paybackdetail);
        }

        // POST: /TransactionDetail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TransactionDetail paybackdetail = db.TransactionDetails.Find(id);
            db.TransactionDetails.Remove(paybackdetail);
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

        private void InitiliazeSelectList() {
           // var query = from x in db.InterestMasters.ToList() select new SelectListItem{Text = x.AccountName,Value = x.Id.ToString()};
            var query = from x in db.InterestMasters.ToList() select new SelectListItem { Text = x.ImName, Value = x.Id.ToString() };
            var imList = query.Distinct();

            List<SelectListItem> paybackTypeList = new List<SelectListItem>()
	        {
	            new SelectListItem {Text = "还利息", Value = "1"},
	            new SelectListItem {Text = "还本金", Value = "2" },
                new SelectListItem {Text = "转本金", Value = "3" },
	        };

            ViewBag.TypeList = new SelectList(paybackTypeList,"Value","Text");
            ViewBag.ImSelectList = new SelectList(imList, "Value", "Text");
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using PayRoll.Models;
using PayRoll.Models.Repository;

namespace PayRoll.Controllers
{
    public class TimeOffRequestsController : Controller
    {
        private PayrollDbContext db = new PayrollDbContext();

        // GET: TimeOffRequests
        public ActionResult Index()
        {
			ViewData["typesOfTimeOff"] = db.TypesOfTimeOff.ToArray();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "StartDate,EndDate,Reason")] TimeOffRequest timeOffRequest)
        {
            if ((timeOffRequest.StartDate < DateTime.Now)
                || (timeOffRequest.EndDate < DateTime.Now)
                || (timeOffRequest.StartDate > timeOffRequest.EndDate))
			{
				ViewData["typesOfTimeOff"] = db.TypesOfTimeOff.ToArray();
				return View(timeOffRequest);
            }
            //TimeOffRequestId,WhenSent
            timeOffRequest.WhenSent = DateTime.Now;
            if (ModelState.IsValid)
            {
                try
                {
                    db.TimeOffRequests.Add(timeOffRequest);
                    db.SaveChanges();
                    db.Employees.Find("a00828729").TimeOffRequests.Add(timeOffRequest);
                    db.SaveChanges();
                    db.TypesOfTimeOff.Find(Request.Form.Get("Type")).TimeOffRequests.Add(timeOffRequest);
                    db.SaveChanges();
                } catch (Exception e) {
                    return RedirectToAction("Failure");
                }
                return RedirectToAction("Success");
            }

			ViewData["typesOfTimeOff"] = db.TypesOfTimeOff.ToArray();
			return View(timeOffRequest);
        }
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult Failure()
        {
            return View();
        }
        public ActionResult AdminApproval()
        {
            return View(db.TimeOffRequests.ToList());
        }
        public ActionResult Accept(int id)
        {
            Employee emp = null;
            TimeOffRequest req = db.TimeOffRequests.Find(id);

            foreach (Employee e in db.Employees)
            {
                foreach (TimeOffRequest tmp in e.TimeOffRequests)
                {
                    if (tmp == req)
                    {
                        emp = e;
                        string email = emp.Email;
                        SmtpClient client = new SmtpClient("smtp.live.com", 25);
                        client.Credentials = new System.Net.NetworkCredential("vpnprez@hotmail.com", "dudethatko1");
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.EnableSsl = true;
                        MailMessage msg = new MailMessage("vpnprez@hotmail.com", email, "Accepted", "Congrats bud");
                        client.Send(msg);
                        return View(emp);
                    }
                }
            }
            return RedirectToAction("AdminApproval");
        }
        [HttpPost, ActionName("Accept")]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptDelete(int id)
        {
            TimeOffRequest req = db.TimeOffRequests.Find(id);
            db.TimeOffRequests.Remove(req);
            db.SaveChanges();
            return RedirectToAction("AdminApproval");
        }
        public ActionResult Decline(int id)
        {
            Employee emp = null;
            TimeOffRequest req = db.TimeOffRequests.Find(id);

            foreach (Employee e in db.Employees)
            {
                foreach (TimeOffRequest tmp in e.TimeOffRequests)
                {
                    if (tmp == req)
                    {
                        emp = e;
                        string email = emp.Email;
                        SmtpClient client = new SmtpClient("smtp.live.com", 25);
                        client.Credentials = new System.Net.NetworkCredential("vpnprez@hotmail.com", "dudethatko1");
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.EnableSsl = true;
                        MailMessage msg = new MailMessage("vpnprez@hotmail.com", email, "Declined", "Sorry bud");
                        client.Send(msg);
                        return View(emp);
                    }
                }
            }
            return RedirectToAction("AdminApproval");
        }
        [HttpPost, ActionName("Decline")]
        [ValidateAntiForgeryToken]
        public ActionResult DeclineDelete(int id)
        {
            TimeOffRequest req = db.TimeOffRequests.Find(id);
            db.TimeOffRequests.Remove(req);
            db.SaveChanges();
            return RedirectToAction("AdminApproval");
        }
    }
}

﻿using CCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// CREATIVE CYBER SOLUTIONS
// CREATED 04/16/2018
// CREATED BY JOHN BELL contact@conquest-marketing.com
// UPDATED 04/16/2018
// UPDATED BY JOHN BELL contact@conquest-marketing.com

namespace CCS.Repositories
{
    public class ProjectRepo : IProjectRepository
    {

        private readonly AppIdentityDbContext context;
        private IProjectProductsRepository projProd;

        public ProjectRepo(AppIdentityDbContext repo, IProjectProductsRepository projprod)
        {
            context = repo;
            projProd = projprod;
        }

        public Project Add(Project p)
        {
            context.Project.Add(p);
            context.SaveChanges();
            return p;
        }

        public Project AddQuote(int id, double q)
        {
            Project oldP = ShowProjectByID(id);
            oldP.Quote = q;
            oldP.Progress = Status.Quoted;
            oldP.QuoteDate = DateTime.Now;
            context.Project.Update(oldP);
            context.SaveChanges();
            return oldP;
        }

        public int Remove(Project p)
        {
            context.Project.Remove(p);
            return context.SaveChanges();
        }

        public int Remove(int id)
        {
            var p = context.Project.FirstOrDefault(a => a.ID == id);
            context.Project.Remove(p);
                return context.SaveChanges();
        }

        public List<Project> ShowAllProjects() => context.Project.ToList();
        public Project ShowProjectByID(int id)
        {
            Project p = context.Project.FirstOrDefault(a => a.ID == id);
            p.Products = projProd.GetProjectProducts(id);
            return p;
        }
        public List<Project> ShowProjectsByCustomer(int custID) => context.Project.Where(a => a.CustomerID == custID).ToList();
        public List<Project> ShowProjectsByStatus(Status s) => context.Project.Where(a => a.Progress == s).ToList();
        public Project Update(Project p)
        {
            Project oldP = ShowProjectByID(p.ID);
            oldP.Description = p.Description;
            oldP.InvoiceDue = p.InvoiceDue;
            oldP.Quote = p.Quote;
            oldP.TotalDue = p.TotalDue;
            context.Project.Update(oldP);
            context.SaveChanges();
            return oldP;
        }

        public Project UpdateStatus(int id, Status s)
        {
            Project oldP = ShowProjectByID(id);
            oldP.Progress = s;
            context.Project.Update(oldP);
            return oldP;
        }
    }
}

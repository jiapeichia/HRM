using Meo.Web.Model;
using Meo.Web.ViewModels;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace Meo.Web.DBContext
{
    public class DBContext : DbContext
    {
        public DBContext()
            : base("DefaultConnection"/*, throwIfV1Schema: false*/)
        {
        }
        public void DisableProxies()
        {
            this.Configuration.ProxyCreationEnabled = false;
        }

        // General settings
        public DbSet<ForgetPasswordViewModels> ForgetPasswords { get; set; }

        // PIC Settings
        public DbSet<SalesViewModels> Saless { get; set; }
        public DbSet<SalesItemViewModels> SalesItems { get; set; }
        public DbSet<CustomerViewModels> Customers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceHistory> ServiceHistories { get; set; }

        // Stock Track 
        public DbSet<Stock> Stock { get; set; }
        public DbSet<StockReceives> StockReceives { get; set; }
        public DbSet<ItemReceive> ItemReceives { get; set; }
        public DbSet<StockReturns> StockReturns { get; set; }
        public DbSet<ItemReturn> ItemReturns { get; set; }

        // Master Settings
        public DbSet<CompanyProfile> CompanyProfile { get; set; }
        public DbSet<ProductViewModels> Products { get; set; }
        public DbSet<PackageViewModels> Packages { get; set; }
        public DbSet<PackageDetailsViewModels> PackageDetails { get; set; }
        public DbSet<PackageSoldViewModels> PackageSolds { get; set; }
        public DbSet<PackageSoldDetailsViewModels> PackageSoldDetails { get; set; }
        public DbSet<EmployeeViewModels> Employees { get; set; }
        public DbSet<PunchCardViewModels> PunchCard { get; set; }
        public DbSet<SupplierViewModels> Suppliers { get; set; }
        public DbSet<DBBackupViewModels> DBBackup { get; set; }

        // Super Administrator
        public DbSet<RoleViewModels> Roles { get; set; }   
        public DbSet<PageViewModels> Pages { get; set; }
        public DbSet<PageAccessViewModels> PageAccesses { get; set; }
        public DbSet<AspUserViewModels> AspUsers { get; set; }
        public DbSet<CounterViewModels> Counters { get; set; }
        public DbSet<TypeViewModels> Types { get; set; }

        // Exchange
        public DbSet<Exchange> Exchanges { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace UploadFile_MVC5.Models
{
    public class UploadDbContext : DbContext
    {
        public UploadDbContext() : base("UploadConnection")
        {
        }
        public static UploadDbContext Create()
        {
            return new UploadDbContext();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFiles> UserFiles { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
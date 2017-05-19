using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.Models
{
    public class DbReport : DbContext
    {
        public IDbSet<sys_users> sys_user { get; set; }

        public IDbSet<rpt_category> rpt_category { get; set; }

        public IDbSet<rpt_categorydetail> rpt_categorydetail { get; set; }

        public IDbSet<rpt_column> rpt_column { get; set; }

        public IDbSet<rpt_mobilecode> rpt_mobilecode { get; set; }

        //public IDbSet<ecash_orders> ecash_orders { get; set; }
    }
}
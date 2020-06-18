using System;
using System.Collections.Generic;

namespace WeddingPlanning.Models{
    public class DashboardWrapper
    {
        public User LoggedUser { get; set; }
        public List<Wedding> AllWeddings { get; set; }
    }
}
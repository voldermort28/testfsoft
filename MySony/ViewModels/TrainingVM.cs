using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;

namespace MySony.ViewModels
{
    public class TrainingVM
    {
        public List<RS_Images> lstImage { get; set; }
        public int Id { get; set; }
    }
}
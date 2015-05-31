using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;

namespace MySony.ViewModels
{
    public class XemTintucVM
    {
        public RS_Article article { get; set; }
        public List<RS_Article> lstSimilar { get; set; }
    }
}
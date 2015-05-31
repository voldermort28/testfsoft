using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;
namespace MySony.ViewModels
{
    public class RegisterVM
    {
        public List<city> cities { get; set; }
        public List<category> cartelogies { get; set; }
    }
}
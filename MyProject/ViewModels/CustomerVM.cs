using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MyProject.Functions;
using MyProject.Models;

namespace MyProject.ViewModels
{
    public class CustomerVM : customer
    {        
        public List<category> LstCategories { get; set; }
        public List<product> LstProducts { get; set; }
        public List<shop> LstShops { get; set; }
        public List<city> LstCity { get; set; }
        public List<CustomerProduct> LstCusPro { get; set; }
    }

    public class ManageCustomerViewModel : customer
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = Constant.CurrentPassword)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = Constant.NewPassword )]
        public  string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = Constant.ConfirmPassword)]
        [Compare("User", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string DistrictName { get; set; }
        public string CityName { get; set; }
        public string JobName { get; set; }
        public string EducationName { get; set; }
        public List<CustomerProduct> LstCustomerProducts { get; set; }
    }
}
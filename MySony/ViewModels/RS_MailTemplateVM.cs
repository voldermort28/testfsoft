using System.Collections.Generic;
using MySony.Models;

namespace MySony.ViewModels
{
    public class RsMailTemplateVm   : RS_mail_template
    {
        public List<city> LstCity { get; set; }
    }
}
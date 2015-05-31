using Kendo.Mvc.UI;
using System.IO;
using System.Web.Mvc;

namespace MySony.Controllers
{
    //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class SerialImageBrowserController : EditorImageBrowserController
    {
        private const string contentFolderRoot = "~/categoryimagesview/";
        public override string ContentPath
        {
            get { return CreateUserFolder(); }
        }
        public ActionResult Index()
        {
            return View();
        }
        private string CreateUserFolder()
        {
            var path = Server.MapPath(contentFolderRoot);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return contentFolderRoot;
        }
    }
}
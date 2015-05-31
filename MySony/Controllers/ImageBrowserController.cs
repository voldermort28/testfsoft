using Kendo.Mvc.UI;
using System.IO;
using System.Web.Mvc;

namespace MySony.Controllers
{
    public class ImageBrowserController : EditorImageBrowserController
    {
        private const string contentFolderRoot = "~/Galleries/";
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
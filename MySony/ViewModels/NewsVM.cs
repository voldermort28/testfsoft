using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;

namespace MySony.ViewModels
{
    public class NewsVM
    {
        public List<RS_Article> lstNews;
        public List<RS_Video> lstGame;
        public List<NewsVM_001> lstTraning;        
        public List<NewsVM_001> lstCategory;
        public List<NewsVM_001> lstQuaTang;
        public List<NewsVM_001> lstImageHome;
        public List<RS_Article> lstPromote;
        public string Image { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Thumbnail { get; set; }
        public string ArticleContent { get; set; }
    }
    public class NewsVM_001
    {      
        public string Image { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Thumbnail { get; set; }
        public string ArticleContent { get; set; }
        public string Slug { get; set; }
        public int ID { get; set; }

        public string Url { get; set; }      
    }
}

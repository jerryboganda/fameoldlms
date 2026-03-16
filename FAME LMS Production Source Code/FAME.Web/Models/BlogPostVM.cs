using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{

    public class BlogCategoy
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class BlogPostMetaData
    {
        public List<BlogCategoy> Categories { get; set; }
        public List<BlogPostVM> RecentPosts { get; set; }
        public BlogPostVM PrevBlog { get; set; }
        public BlogPostVM NextBlog { get; set; }
    }
    public class BlogPostResult
    {
        public List<BlogPostVM> List { get; set; }
        public int PageLength { get; set; }
        public int PageNo { get; set; }
        public int TotalCount { get; set; }

    }
    public class BlogPostVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        [AllowHtml]
        public string BlogContent { get; set; }
        public string Author { get; set; }
        public string PublishedDate { get { return string.Format("{0:yyyy-MM-dd}", PublishedDT); } }
        public DateTime? PublishedDT { get; set; }
        public bool IsPublished { get; set; } = true;
        public string Category { get; set; }
        public string Tags { get; set; }
        public string FeaturedImage { get; set; }
        public HttpPostedFileBase FeaturedImageFile { get; set; }
        public Nullable<int> SortID { get; set; }
        public Nullable<int> ViewCount { get; set; }
    }

}
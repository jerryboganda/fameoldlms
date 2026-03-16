using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace First_Aid_Made_Easy.BLL
{
    public class BlogPostRepository : IBlogPostRepository
    {
        public BlogPostResult GetPublicList(int pageNo = 1, int pagelength = 9, string Category = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var query = db.tbl_BlogPost
                    .Where(x => x.IsPublished == true)
                    .OrderByDescending(x => x.SortID)
                    .Select(x => new BlogPostVM
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Author = x.Author,
                        Category = x.Category,
                        PublishedDT = x.PublishedDate,
                        Tags = x.Tags,
                        ViewCount = x.ViewCount,
                        SortID = x.SortID,
                        Slug = x.Slug,
                        FeaturedImage = x.FeaturedImage,
                        IsPublished = x.IsPublished ?? false
                    });

                // Apply category filter if provided
                if (!string.IsNullOrEmpty(Category))
                {
                    query = query.Where(x => x.Category == Category);
                }

                int totalCount = query.Count();

                var pagedList = query
                    .Skip((pageNo - 1) * pagelength)
                    .Take(pagelength)
                    .ToList();

                return new BlogPostResult
                {
                    List = pagedList,
                    PageNo = pageNo,
                    PageLength = pagelength,
                    TotalCount = totalCount
                };
            }
        }


        public bool Delete(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_BlogPost.Find(id);
                    db.tbl_BlogPost.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public BlogPostVM Save(BlogPostVM d, string userId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_BlogPost model = new tbl_BlogPost();
                if (d.Id > 0)
                {
                    model = db.tbl_BlogPost.Find(d.Id);
                    model.UpdatedBy = userId;
                    model.UpdatedAt = Common.GetCurrentDate();
                }
                model.Title = d.Title;
                model.Description = d.Description;
                model.Category = d.Category;
                model.Author = d.Author;
                model.PublishedDate = d.PublishedDT;
                model.Tags = d.Tags;
                model.SortID = d.SortID;
                model.IsPublished = d.IsPublished;
                model.BlogContent = d.BlogContent;
                if (d.FeaturedImageFile != null)
                {
                    model.FeaturedImage = Common.SavePic(d.FeaturedImageFile, "Blog/Feature/", d.FeaturedImage);
                }

                if (d.Id == 0 || model.Slug == null || model.Title != d.Title)
                {
                    string baseSlug = Common.GenerateSlug(d.Title);
                    string uniqueSlug = baseSlug;
                    int i = 1;
                    while (db.tbl_BlogPost.Any(b => b.Slug == uniqueSlug && b.Id != d.Id))
                    {
                        uniqueSlug = $"{baseSlug}-{i++}";
                    }
                    model.Slug = uniqueSlug;
                }

                if (d.Id == 0)
                {
                    model.CreatedBy = userId;
                    model.CreatedAt = Common.GetCurrentDate();
                    db.tbl_BlogPost.Add(model);
                }
                db.SaveChanges();
                d.Id = model.Id;
                return d;
            }
        }

        public int GetCount()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_BlogPost.Count() + 1;
            }
        }
        public BlogPostVM GetSingle(int? id = 0, string slug = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_BlogPost model;
                if (slug != null)
                {
                    model = db.tbl_BlogPost.FirstOrDefault(x => x.Slug == slug);
                }
                else if (id > 0)
                {
                    model = db.tbl_BlogPost.Find(id);
                }
                else
                {
                    return new BlogPostVM();
                }

                BlogPostVM s = new BlogPostVM()
                {
                    Id = model.Id,
                    Slug = slug,
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    Author = model.Author,
                    Tags = model.Tags,
                    SortID = model.SortID,
                    IsPublished = model.IsPublished ?? false,
                    FeaturedImage = model.FeaturedImage,
                    BlogContent = model.BlogContent,
                    PublishedDT = model.PublishedDate
                };

                return s;
            }
        }
        public BlogPostMetaData GetBlogsMetaData(string slug)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var currentPost = db.tbl_BlogPost.FirstOrDefault(x => x.Slug == slug && x.IsPublished == true);
                if (currentPost == null)
                {
                    return null;
                }

                int? currentSortId = currentPost.SortID;

                // Categories with count
                var categories = db.tbl_BlogPost
                    .Where(x => x.IsPublished == true)
                    .GroupBy(x => x.Category)
                    .Select(g => new BlogCategoy
                    {
                        Name = g.Key,
                        Count = g.Count()
                    }).ToList();

                // Recent posts (latest 5)
                var recentPosts = db.tbl_BlogPost
                    .Where(x => x.IsPublished == true && x.Slug != slug)
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(5)
                    .Select(x => new BlogPostVM
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Author = x.Author,
                        Category = x.Category,
                        PublishedDT = x.PublishedDate,
                        Tags = x.Tags,
                        ViewCount = x.ViewCount,
                        SortID = x.SortID,
                        Slug = x.Slug,
                        FeaturedImage = x.FeaturedImage,
                        IsPublished = x.IsPublished ?? false
                    }).ToList();

                var prevBlog = db.tbl_BlogPost
                    .Where(x => x.SortID < currentSortId && x.IsPublished == true)
                    .OrderByDescending(x => x.SortID)
                    .Select(x => new BlogPostVM
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Slug = x.Slug,
                    }).FirstOrDefault();

                var nextBlog = db.tbl_BlogPost
                    .Where(x => x.SortID > currentSortId && x.IsPublished == true)
                    .OrderBy(x => x.SortID)
                    .Select(x => new BlogPostVM
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Slug = x.Slug,
                    }).FirstOrDefault();

                return new BlogPostMetaData
                {
                    Categories = categories,
                    RecentPosts = recentPosts,
                    PrevBlog = prevBlog,
                    NextBlog = nextBlog
                };
            }
        }
        public async System.Threading.Tasks.Task<DataTableResult<object>> GetBlogPostsAsync(string draw, int start, int length, string searchValue, string sortColumnIndex, string sortDirection)
        {
            using (var db = new FAMEEntities())
            {
                var query = db.tbl_BlogPost
                    .Select(p => new
                    {
                        p.Id,
                        p.Slug,
                        p.SortID,
                        p.Title,
                        p.Category,
                        p.Author,
                        p.IsPublished
                    });

                // Filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(p =>
                        p.Title.Contains(searchValue) ||
                        p.Category.Contains(searchValue) ||
                        p.Author.Contains(searchValue));
                }

                // Total records count (before filtering)
                var recordsTotal = await System.Data.Entity.QueryableExtensions.CountAsync(db.tbl_BlogPost);

                // Total records count (after filtering)
                var recordsFiltered = await System.Data.Entity.QueryableExtensions.CountAsync(query);

                switch (sortColumnIndex)
                {
                    case "SortID":
                        query = sortDirection == "asc" ? query.OrderBy(p => p.SortID) : query.OrderByDescending(p => p.SortID);
                        break;
                    case "Title":
                        query = sortDirection == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
                        break;
                    case "Category":
                        query = sortDirection == "asc" ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category);
                        break;
                    case "Author":
                        query = sortDirection == "asc" ? query.OrderBy(p => p.Author) : query.OrderByDescending(p => p.Author);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.SortID);
                        break;
                }

                // Paging
                var data = await System.Data.Entity.QueryableExtensions.ToListAsync(query.Skip(start).Take(length));

                return new DataTableResult<object>
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = data.Cast<object>().ToList()
                };
            }
        }

    }
}
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IBlogPostRepository
    {
        BlogPostResult GetPublicList(int pageNo = 1, int pagelength = 9, string Category = null);
        bool Delete(int id);
        BlogPostVM Save(BlogPostVM d, string userId);
        int GetCount();
        BlogPostVM GetSingle(int? id = 0, string slug = null);
        BlogPostMetaData GetBlogsMetaData(string slug);
        Task<DataTableResult<object>> GetBlogPostsAsync(string draw, int start, int length, string searchValue, string sortColumnName, string sortDirection);
    }
}

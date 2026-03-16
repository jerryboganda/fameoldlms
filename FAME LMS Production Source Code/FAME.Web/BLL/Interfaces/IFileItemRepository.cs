using First_Aid_Made_Easy.DAL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IFileItemRepository
    {
        Task<List<tbl_FileItems>> GetListAsync(string path);
        Task<List<tbl_FileItems>> GetListForPackageAsync(string path, List<int> packIDs);
        Task<tbl_FileItems> GetByIDAsync(int id);
        Task<bool> AddAsync(tbl_FileItems item);
        Task<bool> AddRangeAsync(IEnumerable<tbl_FileItems> items);
        Task<bool> UpdateAsync(tbl_FileItems item);
        Task<bool> DeleteAsync(int id);
        Task<bool> AnyAsync(string path, string name);
        Task<tbl_FileItems> GetParentAsync(string path);
        bool ExistsForPackage(int packID);
        Task<List<tbl_FileItems>> GetByFileIdAsync(int fileId);
        Task<List<tbl_FileItems>> GetByPathAsync(string path);
        Task RemoveRangeAsync(IEnumerable<tbl_FileItems> items);
        Task<int> SaveChangesAsync();
        void Remove(tbl_FileItems item);
    }
}

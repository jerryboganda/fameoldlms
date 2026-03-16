using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL
{
    public class FileItemRepository : IFileItemRepository
    {
        private readonly FAMEEntities _db;

        public FileItemRepository()
        {
            _db = new FAMEEntities();
        }

        public async Task<List<tbl_FileItems>> GetListAsync(string path)
        {
            return await _db.tbl_FileItems
                .Where(x => x.Path.Equals(path))
                .OrderByDescending(x => x.IsFolder)
                .ToListAsync();
        }

        public async Task<List<tbl_FileItems>> GetListForPackageAsync(string path, List<int> packIDs)
        {
            return await _db.tbl_FileItems
                .Where(x => x.Path.Equals(path) && (x.PackID == null || packIDs.Contains(x.PackID.Value)))
                .OrderByDescending(x => x.IsFolder)
                .ToListAsync();
        }

        public async Task<tbl_FileItems> GetByIDAsync(int id)
        {
            return await _db.tbl_FileItems.FindAsync(id);
        }

        public async Task<bool> AddAsync(tbl_FileItems item)
        {
            _db.tbl_FileItems.Add(item);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<tbl_FileItems> items)
        {
            _db.tbl_FileItems.AddRange(items);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(tbl_FileItems item)
        {
            _db.Entry(item).State = EntityState.Modified;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.tbl_FileItems.FindAsync(id);
            if (item == null) return false;
            _db.tbl_FileItems.Remove(item);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> AnyAsync(string path, string name)
        {
            return await _db.tbl_FileItems.AnyAsync(x => x.Path == path && x.Name == name);
        }

        public async Task<tbl_FileItems> GetParentAsync(string path)
        {
            var p = string.Join("/", path.Split('/').Reverse().Skip(1).Reverse().ToArray());
            return await _db.tbl_FileItems.FirstOrDefaultAsync(x => x.Path.Equals(p));
        }

        public bool ExistsForPackage(int packID)
        {
            return _db.tbl_FileItems.Any(x => x.PackID == packID);
        }

        public async Task<List<tbl_FileItems>> GetByFileIdAsync(int fileId)
        {
            return await _db.tbl_FileItems.Where(x => x.FileId == fileId).ToListAsync();
        }

        public async Task<List<tbl_FileItems>> GetByPathAsync(string path)
        {
            return await _db.tbl_FileItems.Where(x => x.Path == path).ToListAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<tbl_FileItems> items)
        {
            _db.tbl_FileItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public void Remove(tbl_FileItems item)
        {
            _db.tbl_FileItems.Remove(item);
        }
    }
}

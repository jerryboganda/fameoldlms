using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Areas.FileManager.Models;

namespace First_Aid_Made_Easy.Areas.FileManager.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private readonly IFileItemRepository _fileRepository;
        private string RootPath = "~/File-Repository/";
        private List<tbl_FileItems> Items;

        public MainController(IFileItemRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        // GET: FileManager/Main
        public ActionResult Index()
        {
            return View();
        }
        // GET: FileManager/Main/_Index
        public ActionResult _Index(string path)
        {
            return PartialView();
        }

        [HttpGet]
        public async Task<ActionResult> Update(string path)
        {
            try
            {
                path = path.Trim('/');
                // get current files & folders
                var items = await _fileRepository.GetByPathAsync(path);
                var resultItems = items.OrderByDescending(x => x.IsFolder).Select(x => new FileItemModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Path = x.Path,
                    MimeType = x.MimeType,
                    CDate = x.CDate,
                    MDate = x.MDate,
                    IsFolder = x.IsFolder
                }).ToList();

                return Json(new OperationResult
                {
                    Status = OperationStats.Success,
                    Items = resultItems
                }, JsonRequestBehavior.AllowGet);

            }
            catch 
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateFileItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new OperationResult
                {
                    Status = OperationStats.Error,
                    Errors = GetErrors(ModelState)
                });
            }

            model.Path = model.Path.Trim('/');
            var absPath = Server.MapPath(string.Concat(model.Path.Replace("ROOT", RootPath), "/", model.Name));
            var created = false;
            try
            {
                if (model.IsFolder)
                {
                    if (!Directory.Exists(absPath))
                    {
                        Directory.CreateDirectory(absPath);
                        created = true;
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(absPath))
                    {
                        System.IO.File.WriteAllBytes(absPath, new byte[0]);
                        created = true;
                    }
                }

                if (created)
                {
                    // add to database
                    tbl_FileItems newEntity = new tbl_FileItems
                    {
                        Name = model.Name,
                        MimeType = model.Name.Contains('.') ? model.Name.Split('.').LastOrDefault() : null,
                        Path = model.Path,
                        IsFolder = model.IsFolder,
                        CDate = DateTime.UtcNow,
                        MDate = DateTime.UtcNow,
                    };

                    // Check if any items exist and find parent
                    var parent = await _fileRepository.GetParentAsync(model.Path);
                    if (parent != null)
                        newEntity.FileId = parent.Id;

                    await _fileRepository.AddAsync(newEntity);
                }
            }
            catch 
            {
                throw;
            }

            return Json(new OperationResult
            {
                Status = OperationStats.Success,
                Message = StringResources.SuccessfullyCreated
            });
        }
        public async Task<bool> CreateForPackage(HttpServerUtility serv, string Name, int ID)
        {
            Name = string.Join("_", Name.Split(Path.GetInvalidFileNameChars()));
            var absPath = serv.MapPath(string.Concat(RootPath, "/", Name));
            var created = false;
            try
            {
                if (!_fileRepository.ExistsForPackage(ID))
                {
                    Directory.CreateDirectory(absPath);
                    created = true;
                }

                if (created)
                {
                    // add to database
                    tbl_FileItems newEntity = new tbl_FileItems
                    {
                        Name = Name,
                        MimeType = null,
                        Path = "ROOT",
                        IsFolder = true,
                        CDate = DateTime.UtcNow,
                        MDate = DateTime.UtcNow,
                        PackID = ID,
                    };
                    await _fileRepository.AddAsync(newEntity);
                    return true;
                }
            }
            catch
            {
                throw;
            }

            return false;
        }

        [HttpPost]
        public async Task<ActionResult> Upload(UploadFileItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new OperationResult
                {
                    Status = OperationStats.Error,
                    Errors = GetErrors(ModelState)
                });
            }

            model.Path = model.Path.Trim('/');
            List<tbl_FileItems> listToAdd = new List<tbl_FileItems>();
            try
            {
                var absPath = Server.MapPath(string.Concat(model.Path.Replace("ROOT", RootPath), "/", model.PostedFile.FileName));
                if (System.IO.File.Exists(absPath))
                {
                    return Json(new OperationResult
                    {
                        Status = OperationStats.Error,
                        Message = string.Format(StringResources.ItemAlreadyExists, model.PostedFile.FileName)
                    });
                }

                model.PostedFile.SaveAs(absPath);
                
                var parent = await _fileRepository.GetByPathAsync(model.Path);
                var sibling = parent.FirstOrDefault();

                listToAdd.Add(new tbl_FileItems
                {
                    Name = model.PostedFile.FileName,
                    MimeType = model.PostedFile.ContentType,
                    Path = model.Path.Trim('/'),
                    CDate = DateTime.UtcNow,
                    MDate = DateTime.UtcNow,
                    FileId = sibling?.FileId
                });

                if (!listToAdd.Any())
                    return Json(new OperationResult
                    {
                        Status = OperationStats.Error,
                        Message = StringResources.UnknownErrorOccurred
                    });

                await _fileRepository.AddRangeAsync(listToAdd);

                return Json(new OperationResult
                {
                    Status = OperationStats.Success,
                    Message = string.Format(StringResources.SuccessfullyUploaded, model.PostedFile.FileName)
                });
            }
            catch 
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                tbl_FileItems file = await _fileRepository.GetByIDAsync(id);

                if (file == null)
                {
                    return RedirectToAction("Index");
                }
                string path = Server.MapPath(string.Concat(file.Path.Replace("ROOT", RootPath), '/', file.Name));

                if (!System.IO.File.Exists(path))
                {
                    return RedirectToAction("Index");
                }

                var result = new EditFileItemModel
                {
                    Id = file.Id,
                    Path = file.Path,
                    Name = file.Name,
                    CDate = file.CDate,
                    MDate = file.MDate
                };

                using (StreamReader sr = new StreamReader(path))
                {
                    result.Content = await sr.ReadToEndAsync();
                }

                return View(result);
            }
            catch 
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult> _Edit(int id)
        {
            try
            {
                tbl_FileItems file = await _fileRepository.GetByIDAsync(id);

                if (file == null)
                {
                    return RedirectToAction("Index");
                }
                string path = Server.MapPath(string.Concat(file.Path.Replace("ROOT", RootPath), '/', file.Name));

                if (!System.IO.File.Exists(path))
                {
                    return RedirectToAction("Index");
                }

                var result = new EditFileItemModel
                {
                    Id = file.Id,
                    Path = file.Path,
                    Name = file.Name,
                    CDate = file.CDate,
                    MDate = file.MDate
                };

                using (StreamReader sr = new StreamReader(path))
                {
                    result.Content = await sr.ReadToEndAsync();
                }

                return View(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Rename(EditFileItemModel model)
        {
            try
            {
                tbl_FileItems file = await _fileRepository.GetByIDAsync(model.Id);

                if (file == null)
                {
                    return Json(new OperationResult
                    {
                        Status = OperationStats.Error,
                        Message = StringResources.NotFoundInDatabase,
                    });
                }

                string absPath = Server.MapPath(string.Concat(file.Path.Replace("ROOT", RootPath), '/', file.Name));
                if (model.IsFolder)
                {
                    if (!Directory.Exists(absPath))
                    {
                        return Json(new OperationResult
                        {
                            Status = OperationStats.Error,
                            Message = StringResources.NotFoundInFileSystem,
                        });
                    }
                    // Rename the name of current directory
                    // Rename in File System
                    Directory.Move(absPath, RenameFileOrDirectory(absPath, model.Name));
                    // Rename in Database
                    file.Name = model.Name;
                    file.MDate = DateTime.UtcNow;
                    await _fileRepository.UpdateAsync(file);

                    // Change sub directory and file pathes
                    var subItems = await _fileRepository.GetByFileIdAsync(file.Id);
                    await UpdateSubDirectoryPath(subItems);
                }
                else
                {
                    if (!System.IO.File.Exists(absPath))
                    {
                        return Json(new OperationResult
                        {
                            Status = OperationStats.Error,
                            Message = StringResources.NotFoundInFileSystem,
                        });
                    }
                    // Rename in File System
                    System.IO.File.Move(absPath, RenameFileOrDirectory(absPath, model.Name));

                    // Rename in Database
                    file.Name = model.Name;
                    file.MDate = DateTime.UtcNow;
                    await _fileRepository.UpdateAsync(file);
                }

                await _fileRepository.SaveChangesAsync();

                return Json(new OperationResult
                {
                    Status = OperationStats.Success,
                    Message = StringResources.NameChanged,
                });
            }
            catch 
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                tbl_FileItems file = await _fileRepository.GetByIDAsync(id);

                if (file == null)
                {
                    return Json(new OperationResult
                    {
                        Status = OperationStats.Error,
                        Message = StringResources.NotFoundInDatabase,
                    });
                }

                string path = Server.MapPath(string.Concat(file.Path.Replace("ROOT", RootPath), '/', file.Name));

                if (file.IsFolder)
                {
                    if (!Directory.Exists(path))
                    {
                        return Json(new OperationResult
                        {
                            Status = OperationStats.Error,
                            Message = StringResources.NotFoundInFileSystem,
                        });
                    }
                    // Remove it from File System
                    Directory.Delete(path, true);
                }
                else
                {
                    if (!System.IO.File.Exists(path))
                    {
                        return Json(new OperationResult
                        {
                            Status = OperationStats.Error,
                            Message = StringResources.NotFoundInFileSystem,
                        });
                    }
                    // Remove it from File System
                    System.IO.File.Delete(path);
                }

                // Remove it's sub items and itself from Database
                Items = new List<tbl_FileItems>();
                await GetSubItemsAsync(new List<tbl_FileItems> { file });
                Items.Reverse();
                foreach (var item in Items)
                {
                    _fileRepository.Remove(item);
                }

                await _fileRepository.SaveChangesAsync();

                return Json(new OperationResult
                {
                    Status = OperationStats.Success,
                    Message = StringResources.SuccessfullyDeleted
                });
            }
            catch
            {
                throw;
            }
        }

        private async Task GetSubItemsAsync(List<tbl_FileItems> items)
        {
            foreach (tbl_FileItems item in items)
            {
                Items.Add(item);
                await GetSubItemsAsync(item.Files.ToList());
            }
        }

        private async Task UpdateSubDirectoryPath(List<tbl_FileItems> items)
        {
            foreach (var item in items)
            {
                item.Path = string.Concat(item.File.Path, '/', item.File.Name);
                await _fileRepository.UpdateAsync(item);
                var subItems = await _fileRepository.GetByFileIdAsync(item.Id);
                await UpdateSubDirectoryPath(subItems);
            }
        }

        private string RenameFileOrDirectory(string path, string newName)
        {
            var dirName = string.Join("\\", path.Split('\\').Reverse().Skip(1).Reverse());

            return string.Concat(dirName, '\\', newName);
        }

        private List<ModelErrorCollection> GetErrors(ModelStateDictionary modelState)
        {
            return modelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
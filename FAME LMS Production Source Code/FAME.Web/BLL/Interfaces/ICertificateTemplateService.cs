using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateTemplateService
    {
        // CRUD
        tbl_CertificateTemplate GetById(int id);
        CertificateTemplateVM GetTemplateForEdit(int id);
        List<CertificateTemplateListVM> GetAllTemplates(string statusFilter = null);
        int SaveTemplate(CertificateTemplateVM model, string userId);
        void UpdateTemplateStatus(int id, string status, string userId);
        void DeleteTemplate(int id, string userId);

        // Versioning
        tbl_CertificateTemplateVersion GetVersion(int templateId, int version);
        tbl_CertificateTemplateVersion GetLatestVersion(int templateId);
        List<tbl_CertificateTemplateVersion> GetVersionHistory(int templateId);
        int CreateVersion(int templateId, string changeNotes, string userId);

        // Assets
        List<CertificateAssetVM> GetAssets(string assetType = null);
        CertificateAssetVM GetAssetById(int id);
        int SaveAsset(string assetType, string name, string filePath, string signatoryName, string signatoryTitle, string userId);
        void DeleteAsset(int id, string userId);

        // Dashboard
        CertificateAdminDashboardVM GetAdminDashboard();
    }
}

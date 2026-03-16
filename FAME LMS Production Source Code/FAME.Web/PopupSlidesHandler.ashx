<%@ WebHandler Language="C#" Class="PopupSlidesHandler" %>

using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;

public class PopupSlidesHandler : IHttpHandler
{
    private string GetConnectionString()
    {
        return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");

        string method = context.Request.HttpMethod;
        string action = context.Request.QueryString["action"] ?? "";

        // Public endpoint: get active slides (no auth needed for landing page)
        if (method == "GET" && action == "active")
        {
            GetActiveSlides(context);
            return;
        }

        // All other operations require Admin auth
        if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Admin"))
        {
            context.Response.StatusCode = 403;
            context.Response.Write("{\"success\":false,\"error\":\"Access denied\"}");
            return;
        }

        switch (method)
        {
            case "GET":
                GetAllSlides(context);
                break;
            case "POST":
                if (action == "toggle")
                    ToggleSlide(context);
                else
                    SaveSlide(context);
                break;
            case "DELETE":
                DeleteSlide(context);
                break;
            default:
                context.Response.StatusCode = 405;
                context.Response.Write("{\"success\":false,\"error\":\"Method not allowed\"}");
                break;
        }
    }

    private void GetActiveSlides(HttpContext context)
    {
        var slides = new List<Dictionary<string, object>>();
        string connStr = GetConnectionString();

        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                "SELECT Id, Title, ImagePath, LinkUrl, SortOrder FROM tbl_PopupSlides WHERE IsActive = 1 ORDER BY SortOrder, Id", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var slide = new Dictionary<string, object>();
                        slide["id"] = reader.GetInt32(0);
                        slide["title"] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        slide["imagePath"] = reader.GetString(2);
                        slide["linkUrl"] = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        slide["sortOrder"] = reader.GetInt32(4);
                        slides.Add(slide);
                    }
                }
            }
        }

        var serializer = new JavaScriptSerializer();
        var result = new Dictionary<string, object>();
        result["success"] = true;
        result["slides"] = slides;
        context.Response.Write(serializer.Serialize(result));
    }

    private void GetAllSlides(HttpContext context)
    {
        var slides = new List<Dictionary<string, object>>();
        string connStr = GetConnectionString();

        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                "SELECT Id, Title, ImagePath, LinkUrl, SortOrder, IsActive, CreatedDate, ModifiedDate FROM tbl_PopupSlides ORDER BY SortOrder, Id", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var slide = new Dictionary<string, object>();
                        slide["id"] = reader.GetInt32(0);
                        slide["title"] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        slide["imagePath"] = reader.GetString(2);
                        slide["linkUrl"] = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        slide["sortOrder"] = reader.GetInt32(4);
                        slide["isActive"] = reader.GetBoolean(5);
                        slide["createdDate"] = reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm");
                        slide["modifiedDate"] = reader.IsDBNull(7) ? "" : reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm");
                        slides.Add(slide);
                    }
                }
            }
        }

        var serializer = new JavaScriptSerializer();
        var result = new Dictionary<string, object>();
        result["success"] = true;
        result["slides"] = slides;
        context.Response.Write(serializer.Serialize(result));
    }

    private void SaveSlide(HttpContext context)
    {
        try
        {
            int id = 0;
            int.TryParse(context.Request.Form["id"], out id);
            string title = context.Request.Form["title"] ?? "";
            string linkUrl = context.Request.Form["linkUrl"] ?? "";
            int sortOrder = 0;
            int.TryParse(context.Request.Form["sortOrder"], out sortOrder);
            bool isActive = context.Request.Form["isActive"] == "true" || context.Request.Form["isActive"] == "1";
            string imagePath = context.Request.Form["existingImagePath"] ?? "";

            // Handle file upload
            if (context.Request.Files.Count > 0 && context.Request.Files[0].ContentLength > 0)
            {
                var file = context.Request.Files[0];
                string ext = Path.GetExtension(file.FileName).ToLower();

                // Validate file type
                var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                if (!allowedExtensions.Contains(ext))
                {
                    context.Response.Write("{\"success\":false,\"error\":\"Invalid file type. Only JPG, PNG, GIF, WebP, BMP allowed.\"}");
                    return;
                }

                // Validate file size (max 10MB)
                if (file.ContentLength > 10 * 1024 * 1024)
                {
                    context.Response.Write("{\"success\":false,\"error\":\"File too large. Maximum 10MB.\"}");
                    return;
                }

                string fileName = "slide_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
                string uploadDir = context.Server.MapPath("~/Images/Slides/");

                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                file.SaveAs(filePath);

                // Delete old image if updating
                if (id > 0 && !string.IsNullOrEmpty(imagePath) && imagePath != ("/Images/Slides/" + fileName))
                {
                    string oldPath = context.Server.MapPath("~" + imagePath);
                    if (File.Exists(oldPath))
                    {
                        try { File.Delete(oldPath); } catch { /* ignore */ }
                    }
                }

                imagePath = "/Images/Slides/" + fileName;
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                context.Response.Write("{\"success\":false,\"error\":\"Image is required\"}");
                return;
            }

            string connStr = GetConnectionString();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                if (id > 0)
                {
                    // Update existing slide
                    using (var cmd = new SqlCommand(
                        "UPDATE tbl_PopupSlides SET Title=@Title, ImagePath=@ImagePath, LinkUrl=@LinkUrl, SortOrder=@SortOrder, IsActive=@IsActive, ModifiedDate=GETDATE() WHERE Id=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(title) ? (object)DBNull.Value : title);
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                        cmd.Parameters.AddWithValue("@LinkUrl", string.IsNullOrEmpty(linkUrl) ? (object)DBNull.Value : linkUrl);
                        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insert new slide
                    using (var cmd = new SqlCommand(
                        "INSERT INTO tbl_PopupSlides (Title, ImagePath, LinkUrl, SortOrder, IsActive) VALUES (@Title, @ImagePath, @LinkUrl, @SortOrder, @IsActive); SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(title) ? (object)DBNull.Value : title);
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                        cmd.Parameters.AddWithValue("@LinkUrl", string.IsNullOrEmpty(linkUrl) ? (object)DBNull.Value : linkUrl);
                        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);
                        var newId = cmd.ExecuteScalar();
                        id = Convert.ToInt32(newId);
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            var result = new Dictionary<string, object>();
            result["success"] = true;
            result["id"] = id;
            context.Response.Write(serializer.Serialize(result));
        }
        catch (Exception ex)
        {
            context.Response.Write("{\"success\":false,\"error\":\"" + ex.Message.Replace("\"", "'").Replace("\r", "").Replace("\n", " ") + "\"}");
        }
    }

    private void DeleteSlide(HttpContext context)
    {
        try
        {
            int id = 0;
            int.TryParse(context.Request.QueryString["id"], out id);

            if (id <= 0)
            {
                context.Response.Write("{\"success\":false,\"error\":\"Invalid ID\"}");
                return;
            }

            string imagePath = "";
            string connStr = GetConnectionString();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Get image path before deleting
                using (var cmd = new SqlCommand("SELECT ImagePath FROM tbl_PopupSlides WHERE Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var result = cmd.ExecuteScalar();
                    imagePath = result != null ? result.ToString() : "";
                }

                // Delete the record
                using (var cmd = new SqlCommand("DELETE FROM tbl_PopupSlides WHERE Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            // Delete the image file
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = context.Server.MapPath("~" + imagePath);
                if (File.Exists(fullPath))
                {
                    try { File.Delete(fullPath); } catch { /* ignore cleanup errors */ }
                }
            }

            context.Response.Write("{\"success\":true}");
        }
        catch (Exception ex)
        {
            context.Response.Write("{\"success\":false,\"error\":\"" + ex.Message.Replace("\"", "'").Replace("\r", "").Replace("\n", " ") + "\"}");
        }
    }

    // Toggle active status
    private void ToggleSlide(HttpContext context)
    {
        try
        {
            int id = 0;
            int.TryParse(context.Request.QueryString["id"], out id);

            if (id <= 0)
            {
                context.Response.Write("{\"success\":false,\"error\":\"Invalid ID\"}");
                return;
            }

            string connStr = GetConnectionString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE tbl_PopupSlides SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, ModifiedDate = GETDATE() WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            context.Response.Write("{\"success\":true}");
        }
        catch (Exception ex)
        {
            context.Response.Write("{\"success\":false,\"error\":\"" + ex.Message.Replace("\"", "'").Replace("\r", "").Replace("\n", " ") + "\"}");
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}

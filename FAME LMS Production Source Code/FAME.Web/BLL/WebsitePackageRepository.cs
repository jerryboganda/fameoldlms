using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace First_Aid_Made_Easy.BLL
{
    public class WebsitePackageRepository : IWebsitePackageRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        /// <summary>
        /// Get only active packages for the public landing page
        /// </summary>
        public List<WebsitePackageVM> GetActivePackages()
        {
            return GetPackages(activeOnly: true);
        }

        /// <summary>
        /// Get all packages for the admin panel
        /// </summary>
        public List<WebsitePackageVM> GetAllPackages()
        {
            return GetPackages(activeOnly: false);
        }

        private List<WebsitePackageVM> GetPackages(bool activeOnly)
        {
            var packages = new List<WebsitePackageVM>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Load packages
                string packageSql = activeOnly
                    ? "SELECT * FROM tbl_WebsitePackage WHERE IsActive = 1 ORDER BY SortOrder, ID"
                    : "SELECT * FROM tbl_WebsitePackage ORDER BY SortOrder, ID";

                using (var cmd = new SqlCommand(packageSql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        packages.Add(MapPackage(reader));
                    }
                }

                // Load features for all packages
                string featureSql = activeOnly
                    ? @"SELECT f.* FROM tbl_WebsitePackageFeature f 
                        INNER JOIN tbl_WebsitePackage p ON f.PackageID = p.ID 
                        WHERE p.IsActive = 1 ORDER BY f.PackageID, f.SortOrder, f.ID"
                    : "SELECT * FROM tbl_WebsitePackageFeature ORDER BY PackageID, SortOrder, ID";

                var featureMap = new Dictionary<int, List<WebsitePackageFeatureVM>>();
                using (var cmd = new SqlCommand(featureSql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var feature = MapFeature(reader);
                        if (!featureMap.ContainsKey(feature.PackageID))
                            featureMap[feature.PackageID] = new List<WebsitePackageFeatureVM>();
                        featureMap[feature.PackageID].Add(feature);
                    }
                }

                // Load pricing for all packages
                string pricingSql = activeOnly
                    ? @"SELECT pr.* FROM tbl_WebsitePackagePricing pr 
                        INNER JOIN tbl_WebsitePackage p ON pr.PackageID = p.ID 
                        WHERE p.IsActive = 1 ORDER BY pr.PackageID, pr.SortOrder, pr.ID"
                    : "SELECT * FROM tbl_WebsitePackagePricing ORDER BY PackageID, SortOrder, ID";

                var pricingMap = new Dictionary<int, List<WebsitePackagePricingVM>>();
                using (var cmd = new SqlCommand(pricingSql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pricing = MapPricing(reader);
                        if (!pricingMap.ContainsKey(pricing.PackageID))
                            pricingMap[pricing.PackageID] = new List<WebsitePackagePricingVM>();
                        pricingMap[pricing.PackageID].Add(pricing);
                    }
                }

                // Assign features and pricing to packages
                foreach (var pkg in packages)
                {
                    pkg.Features = featureMap.ContainsKey(pkg.ID) ? featureMap[pkg.ID] : new List<WebsitePackageFeatureVM>();
                    pkg.Pricing = pricingMap.ContainsKey(pkg.ID) ? pricingMap[pkg.ID] : new List<WebsitePackagePricingVM>();
                }
            }

            return packages;
        }

        /// <summary>
        /// Get a single package by ID with all features and pricing
        /// </summary>
        public WebsitePackageVM GetById(int id)
        {
            WebsitePackageVM package = null;

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM tbl_WebsitePackage WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            package = MapPackage(reader);
                        }
                    }
                }

                if (package == null) return null;

                // Load features
                package.Features = new List<WebsitePackageFeatureVM>();
                using (var cmd = new SqlCommand("SELECT * FROM tbl_WebsitePackageFeature WHERE PackageID = @PackageID ORDER BY SortOrder, ID", conn))
                {
                    cmd.Parameters.AddWithValue("@PackageID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            package.Features.Add(MapFeature(reader));
                        }
                    }
                }

                // Load pricing
                package.Pricing = new List<WebsitePackagePricingVM>();
                using (var cmd = new SqlCommand("SELECT * FROM tbl_WebsitePackagePricing WHERE PackageID = @PackageID ORDER BY SortOrder, ID", conn))
                {
                    cmd.Parameters.AddWithValue("@PackageID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            package.Pricing.Add(MapPricing(reader));
                        }
                    }
                }
            }

            return package;
        }

        /// <summary>
        /// Save a package (insert or update) with features and pricing
        /// Returns the package ID
        /// </summary>
        public int Save(WebsitePackageVM model)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int packageId;

                        if (model.ID > 0)
                        {
                            // UPDATE existing package
                            string updateSql = @"UPDATE tbl_WebsitePackage SET 
                                PlanName = @PlanName, Subtitle = @Subtitle, Description = @Description,
                                BuyButtonUrl = @BuyButtonUrl, BuyButtonText = @BuyButtonText,
                                SortOrder = @SortOrder, IsActive = @IsActive,
                                LeftColumnWidth = @LeftColumnWidth, RightColumnWidth = @RightColumnWidth,
                                UseMultiColumnFeatures = @UseMultiColumnFeatures, ModifiedDate = GETDATE()
                                WHERE ID = @ID";

                            using (var cmd = new SqlCommand(updateSql, conn, transaction))
                            {
                                AddPackageParams(cmd, model);
                                cmd.Parameters.AddWithValue("@ID", model.ID);
                                cmd.ExecuteNonQuery();
                            }
                            packageId = model.ID;

                            // Delete existing features and pricing
                            using (var cmd = new SqlCommand("DELETE FROM tbl_WebsitePackageFeature WHERE PackageID = @PackageID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PackageID", packageId);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SqlCommand("DELETE FROM tbl_WebsitePackagePricing WHERE PackageID = @PackageID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PackageID", packageId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // INSERT new package
                            string insertSql = @"INSERT INTO tbl_WebsitePackage 
                                (PlanName, Subtitle, Description, BuyButtonUrl, BuyButtonText, SortOrder, IsActive, 
                                 LeftColumnWidth, RightColumnWidth, UseMultiColumnFeatures, CreatedDate, ModifiedDate)
                                VALUES (@PlanName, @Subtitle, @Description, @BuyButtonUrl, @BuyButtonText, @SortOrder, @IsActive,
                                 @LeftColumnWidth, @RightColumnWidth, @UseMultiColumnFeatures, GETDATE(), GETDATE());
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using (var cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                AddPackageParams(cmd, model);
                                packageId = (int)cmd.ExecuteScalar();
                            }
                        }

                        // Insert features
                        if (model.Features != null)
                        {
                            int sortOrder = 0;
                            foreach (var feature in model.Features)
                            {
                                if (string.IsNullOrWhiteSpace(feature.FeatureText)) continue;
                                sortOrder++;
                                string sql = @"INSERT INTO tbl_WebsitePackageFeature (PackageID, FeatureText, SortOrder) 
                                               VALUES (@PackageID, @FeatureText, @SortOrder)";
                                using (var cmd = new SqlCommand(sql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@PackageID", packageId);
                                    cmd.Parameters.AddWithValue("@FeatureText", feature.FeatureText.Trim());
                                    cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Insert pricing
                        if (model.Pricing != null)
                        {
                            int sortOrder = 0;
                            foreach (var pricing in model.Pricing)
                            {
                                if (string.IsNullOrWhiteSpace(pricing.DurationText) || string.IsNullOrWhiteSpace(pricing.PricePKR)) continue;
                                sortOrder++;
                                string sql = @"INSERT INTO tbl_WebsitePackagePricing (PackageID, DurationText, PricePKR, PriceUSD, SortOrder) 
                                               VALUES (@PackageID, @DurationText, @PricePKR, @PriceUSD, @SortOrder)";
                                using (var cmd = new SqlCommand(sql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@PackageID", packageId);
                                    cmd.Parameters.AddWithValue("@DurationText", pricing.DurationText.Trim());
                                    cmd.Parameters.AddWithValue("@PricePKR", pricing.PricePKR.Trim());
                                    cmd.Parameters.AddWithValue("@PriceUSD", (object)pricing.PriceUSD?.Trim() ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return packageId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Delete a package and all related features/pricing (CASCADE)
        /// </summary>
        public bool Delete(int id)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                // CASCADE delete handles features and pricing
                using (var cmd = new SqlCommand("DELETE FROM tbl_WebsitePackage WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Toggle active/inactive status
        /// </summary>
        public bool ToggleActive(int id)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE tbl_WebsitePackage SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, ModifiedDate = GETDATE() WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        #region Helpers

        private void AddPackageParams(SqlCommand cmd, WebsitePackageVM model)
        {
            cmd.Parameters.AddWithValue("@PlanName", model.PlanName ?? "");
            cmd.Parameters.AddWithValue("@Subtitle", (object)model.Subtitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BuyButtonUrl", model.BuyButtonUrl ?? "/Account/Register");
            cmd.Parameters.AddWithValue("@BuyButtonText", model.BuyButtonText ?? "Buy");
            cmd.Parameters.AddWithValue("@SortOrder", model.SortOrder);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            cmd.Parameters.AddWithValue("@LeftColumnWidth", model.LeftColumnWidth > 0 ? model.LeftColumnWidth : 6);
            cmd.Parameters.AddWithValue("@RightColumnWidth", model.RightColumnWidth > 0 ? model.RightColumnWidth : 6);
            cmd.Parameters.AddWithValue("@UseMultiColumnFeatures", model.UseMultiColumnFeatures);
        }

        private WebsitePackageVM MapPackage(SqlDataReader reader)
        {
            return new WebsitePackageVM
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                PlanName = reader["PlanName"]?.ToString(),
                Subtitle = reader["Subtitle"]?.ToString(),
                Description = reader["Description"]?.ToString(),
                BuyButtonUrl = reader["BuyButtonUrl"]?.ToString() ?? "/Account/Register",
                BuyButtonText = reader["BuyButtonText"]?.ToString() ?? "Buy",
                SortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0,
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                LeftColumnWidth = reader["LeftColumnWidth"] != DBNull.Value ? Convert.ToInt32(reader["LeftColumnWidth"]) : 6,
                RightColumnWidth = reader["RightColumnWidth"] != DBNull.Value ? Convert.ToInt32(reader["RightColumnWidth"]) : 6,
                UseMultiColumnFeatures = reader["UseMultiColumnFeatures"] != DBNull.Value && Convert.ToBoolean(reader["UseMultiColumnFeatures"]),
                CreatedDate = reader["CreatedDate"] != DBNull.Value ? (DateTime?)reader["CreatedDate"] : null,
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? (DateTime?)reader["ModifiedDate"] : null,
            };
        }

        private WebsitePackageFeatureVM MapFeature(SqlDataReader reader)
        {
            return new WebsitePackageFeatureVM
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                PackageID = Convert.ToInt32(reader["PackageID"]),
                FeatureText = reader["FeatureText"]?.ToString(),
                SortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0,
            };
        }

        private WebsitePackagePricingVM MapPricing(SqlDataReader reader)
        {
            return new WebsitePackagePricingVM
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                PackageID = Convert.ToInt32(reader["PackageID"]),
                DurationText = reader["DurationText"]?.ToString(),
                PricePKR = reader["PricePKR"]?.ToString(),
                PriceUSD = reader["PriceUSD"]?.ToString(),
                SortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0,
            };
        }

        #endregion
    }
}

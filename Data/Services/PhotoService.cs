using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fischbowl_Project.Data.Services
{
    public class PhotoMetaData
    {
        public string PeopleIdentified { get; set; }
        public string PhotoName { get; set; }
        public string BlobUrl { get; set; }
        public DateTime? DateTaken { get; set; }
    }

    public class PhotoService
    {
        private readonly string _sqlConnectionString;
        private readonly BlobStorageService _blobStorageService;

        public PhotoService(string sqlConnectionString, BlobStorageService blobStorageService)
        {
            _sqlConnectionString = sqlConnectionString;
            _blobStorageService = blobStorageService;
        }

        public async Task<List<PhotoMetaData>> GetPhotosAsync(string person = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var photos = new List<PhotoMetaData>();
            using (SqlConnection conn = new SqlConnection(_sqlConnectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT PeopleIdentified, PhotoName, BlobUrl, DateTaken FROM PhotoMetaData WHERE 1=1";
                if (!string.IsNullOrEmpty(person))
                {
                    query += " AND PeopleIdentified LIKE @Person";
                }
                if (startDate.HasValue)
                {
                    query += " AND DateTaken >= @StartDate";
                }
                if (endDate.HasValue)
                {
                    query += " AND DateTaken <= @EndDate";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(person))
                    {
                        cmd.Parameters.AddWithValue("@Person", $"%{person}%");
                    }
                    if (startDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
                    }

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var photoName = reader.GetString(1);
                            var blobUrl = _blobStorageService.GetBlobSasUrl("photos", photoName); // Assuming photos are stored in the 'photos' container

                            photos.Add(new PhotoMetaData
                            {
                                PeopleIdentified = reader.GetString(0),
                                PhotoName = photoName,
                                BlobUrl = blobUrl,
                                DateTaken = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            return photos;
        }
    }
}

using Humanizer;
using Microsoft.Data.SqlClient;
using ReportMeeting.Models;
using System.Collections.Generic;
using System.Data;

namespace ReportMeeting.Services
{
    public class ProjectService
    {
        private readonly string _connectionString;

        public ProjectService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<PaginationModel<Project>> GetProjectsAsync(int pageNumber, int pageSize, Users user)
        {
            // Calculate the offset for pagination
            int offset = (pageNumber - 1) * pageSize;

            // Construct the SQL query based on user role
            string sqlQuery = String.Empty;

            List<SqlParameter> parameters = new List<SqlParameter>();

            //  if admin then show all list
            if (user.roleId == 4)
            {
                sqlQuery = @"
                SELECT p.*, plat.Name AS PlatformName, arch.Name AS ArchitectName
                FROM (
                     SELECT pr.*,
                        ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
                    FROM Project pr
                ) AS p
                INNER JOIN Platform AS plat ON p.PlatformId = plat.Id
                INNER JOIN Users AS arch ON p.ArchitectId = arch.Id
                WHERE p.RowNumber BETWEEN @Offset + 1 AND @Offset + @PageSize
                ORDER BY p.Id;
            ";
                parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
                parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
            }
            // else show only projects according to user if it's an architect
            else if(user.roleId == 3)
            {
                sqlQuery = @"
                SELECT p.*, plat.Name AS PlatformName, arch.Name AS ArchitectName
                FROM (
                    SELECT pr.*,
                        ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
                    FROM Project pr
                    WHERE ArchitectId = @ArchitectId
                ) AS p
                INNER JOIN Platform AS plat ON p.PlatformId = plat.Id
                INNER JOIN Users AS arch ON p.ArchitectId = arch.Id
                WHERE p.RowNumber BETWEEN @Offset + 1 AND @Offset + @PageSize
                ORDER BY p.Id;";

                parameters.Add(new SqlParameter("@ArchitectId", SqlDbType.Int) { Value = user.id });
                parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
                parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
            }
            // else show only projects where user are concerned in tasks
            else
            {
                sqlQuery = @"
                SELECT p.*, plat.Name AS PlatformName, arch.Name AS ArchitectName
                FROM (
                    SELECT pr.*,
                        ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
                    FROM Project pr
                    WHERE Id in (
                        SELECT DISTINCT projectId
                        FROM Task
                        WHERE assigneeId = @UserId
                   )
                ) AS p
                INNER JOIN Platform AS plat ON p.PlatformId = plat.Id
                INNER JOIN Users AS arch ON p.ArchitectId = arch.Id
                WHERE p.RowNumber BETWEEN @Offset + 1 AND @Offset + @PageSize
                ORDER BY p.Id;";

                parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = user.id });
                parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
                parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
            }

            List<Project> projects = new List<Project>();

            // Create a connection to the database
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Create a command object with the SQL query and parameters
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    // Execute the query and retrieve the data
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Project project = new Project
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                details = reader.GetString(reader.GetOrdinal("Details")),
                                startDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                dueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                platformId = reader.GetInt32(reader.GetOrdinal("PlatformId")),
                                architectId = reader.GetInt32(reader.GetOrdinal("ArchitectId")),
                                jiraLink = reader.GetString(reader.GetOrdinal("JiraLink")),
                                platform = new Platform { name = reader.GetString(reader.GetOrdinal("PlatformName")) },
                                architect = new Users { name = reader.GetString(reader.GetOrdinal("ArchitectName")) }
                            };
                            projects.Add(project);
                        }
                    }
                }
            }

            // Calculate the total number of records
            int totalRecords;
            string countQuery = (user.roleId == 4) ? "SELECT COUNT(*) FROM Project;" : "SELECT COUNT(*) FROM Project WHERE ArchitectId = @ArchitectId;";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(countQuery, connection))
                {
                    if (user.roleId != 4)
                    {
                        command.Parameters.Add(new SqlParameter("@ArchitectId", SqlDbType.Int) { Value = user.id });
                    }
                    totalRecords = (int)await command.ExecuteScalarAsync();
                }
            }

            // Calculate the total number of pages
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Create a view model to pass the data and pagination details
            var viewModel = new PaginationModel<Project>
            {
                Model = projects,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return viewModel;
        }
    }
}

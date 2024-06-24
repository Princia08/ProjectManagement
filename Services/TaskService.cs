using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReportMeeting.Data;
using ReportMeeting.Models;

namespace ReportMeeting.Services
{
    public class TaskService
    {
        private readonly string _connectionString;

        public TaskService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<Models.Task>> getListByProject(int? projectId)
        {
            var tasks = new List<Models.Task>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT t.*,
                                 u.id as AssigneeId, u.name as AssigneeName, u.email as AssigneeEmail
                          FROM Task t
                          LEFT JOIN Users u ON t.assigneeId = u.id
                          WHERE t.projectId = @ProjectId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", projectId ?? (object)DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var task = new Models.Task
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                details = reader.IsDBNull(reader.GetOrdinal("details")) ? null : reader.GetString(reader.GetOrdinal("details")),
                                files = reader.IsDBNull(reader.GetOrdinal("files")) ? null : reader.GetString(reader.GetOrdinal("files")),
                                startDate = reader.IsDBNull(reader.GetOrdinal("startDate")) ? (DateOnly?)null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("startDate"))),
                                dueDate = reader.IsDBNull(reader.GetOrdinal("dueDate")) ? (DateOnly?)null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("dueDate"))),
                                projectId = reader.GetInt32(reader.GetOrdinal("projectId")),
                                assigneeId = reader.GetInt32(reader.GetOrdinal("assigneeId")),
                                status = reader.GetString(reader.GetOrdinal("status")),
                                blockingPoint = reader.IsDBNull(reader.GetOrdinal("blockingPoint")) ? null : reader.GetString(reader.GetOrdinal("blockingPoint")),
                                assignee = reader.IsDBNull(reader.GetOrdinal("AssigneeId")) ? null : new Users
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("AssigneeId")),
                                    name = reader.GetString(reader.GetOrdinal("AssigneeName")),
                                    email = reader.GetString(reader.GetOrdinal("AssigneeEmail"))
                                }
                            };
                            tasks.Add(task);
                        }
                    }
                }
            }

            return tasks;
        }
    }
}
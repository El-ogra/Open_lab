using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using System.Text.Json;

namespace Open_lab.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly OpenLabDbContext _db;

        public UserActivityService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserActivityRow>> GetRecentActivitiesAsync(int? userId = null, int count = 100)
        {
            var query = _db.AuditLogs.Include(a => a.User).AsNoTracking();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            var rows = new List<UserActivityRow>();
            foreach (var log in logs)
            {
                rows.Add(new UserActivityRow
                {
                    AuditLogId = log.AuditLogId,
                    Username = log.User.Username,
                    Action = log.Action,
                    TableName = log.TableName,
                    RecordId = log.RecordId ?? string.Empty,
                    Timestamp = log.Timestamp,
                    ActivityDescription = GenerateHumanReadableDescription(log)
                });
            }

            return rows;
        }

        public async Task<string> SimplifyAuditLogAsync(int auditLogId)
        {
            var log = await _db.AuditLogs.FindAsync(auditLogId);
            if (log == null) return "السجل غير موجود";
            return GenerateHumanReadableDescription(log);
        }

        private string GenerateHumanReadableDescription(Models.AuditLog log)
        {
            string actionAr = log.Action switch
            {
                "Insert" => "إضافة",
                "Update" => "تعديل",
                "Delete" => "حذف",
                _ => log.Action
            };

            string tableAr = log.TableName switch
            {
                "Patients" => "بيانات مريض",
                "Visits" => "زيارة",
                "Tests" => "تحليل",
                "Invoices" => "فاتورة",
                "ResultValues" => "نتيجة تحليل",
                _ => log.TableName
            };

            if (log.Action == "Update" && !string.IsNullOrEmpty(log.NewValues))
            {
                // Simple attempt to show what changed
                try
                {
                    var newVals = JsonSerializer.Deserialize<Dictionary<string, object>>(log.NewValues);
                    var oldVals = !string.IsNullOrEmpty(log.OldValues) 
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.OldValues) 
                        : new Dictionary<string, object>();

                    var changes = new List<string>();
                    foreach (var kvp in newVals!)
                    {
                        if (oldVals!.TryGetValue(kvp.Key, out var oldVal))
                        {
                            if (oldVal?.ToString() != kvp.Value?.ToString())
                            {
                                changes.Add($"تغيير {kvp.Key} من ({oldVal ?? "فارغ"}) إلى ({kvp.Value ?? "فارغ"})");
                            }
                        }
                    }

                    if (changes.Any())
                        return $"{actionAr} في {tableAr}: " + string.Join(" | ", changes);
                }
                catch { }
            }

            return $"{actionAr} في {tableAr} (كود السجل: {log.RecordId})";
        }
    }
}

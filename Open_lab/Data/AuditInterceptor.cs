using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Open_lab.Models;

namespace Open_lab.Data
{
    /// <summary>
    /// EF Core Interceptor for automatic audit trail logging.
    /// Captures all INSERT, UPDATE, DELETE operations with user context.
    /// </summary>
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly int? _currentUserId;

        public AuditInterceptor(int? currentUserId = null)
        {
            _currentUserId = currentUserId;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            CreateAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            CreateAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CreateAuditLogs(DbContext? context)
        {
            if (context == null) return;

            var auditEntries = OnBeforeSaveChanges(context);
            if (auditEntries == null || auditEntries.Count == 0) return;

            int userId = _currentUserId ?? 1;
            if (context is OpenLabDbContext openLabContext && openLabContext.CurrentUserId.HasValue)
            {
                userId = openLabContext.CurrentUserId.Value;
            }

            foreach (var auditEntry in auditEntries)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId, // Use dynamic user ID
                    Action = auditEntry.Action,
                    TableName = auditEntry.TableName,
                    RecordId = auditEntry.KeyValues?.FirstOrDefault().Value?.ToString() ?? "",
                    Timestamp = DateTime.UtcNow,
                    OldValues = auditEntry.OldValues?.Count > 0 ? JsonSerializer.Serialize(auditEntry.OldValues) : null,
                    NewValues = auditEntry.NewValues?.Count > 0 ? JsonSerializer.Serialize(auditEntry.NewValues) : null
                };

                context.Set<AuditLog>().Add(auditLog);
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges(DbContext context)
        {
            context.ChangeTracker.DetectChanges();
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog && 
                           (e.State == EntityState.Added || 
                            e.State == EntityState.Modified || 
                            e.State == EntityState.Deleted))
                .ToList();

            var auditEntries = new List<AuditEntry>();

            foreach (var entry in entries)
            {
                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString()
                };

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    
                    if (property.IsTemporary)
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue ?? "";
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue ?? "";
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue ?? "";
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue ?? "";
                                auditEntry.NewValues[propertyName] = property.CurrentValue ?? "";
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }
    }

    /// <summary>
    /// Helper class for tracking audit entries during SaveChanges.
    /// </summary>
    public class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
    }
}

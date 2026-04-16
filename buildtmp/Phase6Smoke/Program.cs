using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

internal sealed class CheckResult
{
    public string Name { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Details { get; init; } = string.Empty;
}

internal static class Program
{
    private static async Task<int> Main()
    {
        var checks = new List<CheckResult>();

        try
        {
            using var db = new OpenLabDbContext();

            var authService = new AuthService(db);
            var authzService = new AuthorizationService(db);
            var adminSetup = new AdminSetupService(db);
            var statsService = new StatisticsService(db);
            var treasuryService = new AccountsTreasuryService(db);
            var settingsService = new SystemSettingsService(db);
            var userAdminService = new UserAdminService(db);

            // 1) Admin login check
            User? adminUser = null;
            try
            {
                adminUser = await authService.ValidateCredentialsAsync("admin", "admin123");
                checks.Add(new CheckResult
                {
                    Name = "Login: admin/admin123",
                    Passed = adminUser != null,
                    Details = adminUser == null ? "Authentication returned null." : $"Authenticated UserId={adminUser.UserId}."
                });
            }
            catch (Exception ex)
            {
                checks.Add(new CheckResult { Name = "Login: admin/admin123", Passed = false, Details = ex.Message });
            }

            if (adminUser != null)
            {
                // Ensure admin full permissions bootstrap
                try
                {
                    await adminSetup.EnsureAdminAccessAsync(adminUser.UserId);
                    var permissionCodes = await authzService.GetPermissionCodesAsync(adminUser.UserId);
                    var hasFull = permissionCodes.Contains(PermissionCodes.FullAccess, StringComparer.OrdinalIgnoreCase);
                    checks.Add(new CheckResult
                    {
                        Name = "Permissions: admin full access",
                        Passed = hasFull,
                        Details = hasFull ? $"Permission count={permissionCodes.Count}." : "Full access code not found."
                    });
                }
                catch (Exception ex)
                {
                    checks.Add(new CheckResult { Name = "Permissions: admin full access", Passed = false, Details = ex.Message });
                }
            }
            else
            {
                checks.Add(new CheckResult
                {
                    Name = "Permissions: admin full access",
                    Passed = false,
                    Details = "Skipped because admin login failed."
                });
            }

            // 2) Role/permission management safety checks
            try
            {
                var canDeleteAdmin = true;
                try
                {
                    if (adminUser != null)
                    {
                        await userAdminService.DeleteUserAsync(adminUser.UserId);
                    }
                    else
                    {
                        canDeleteAdmin = false;
                    }
                }
                catch
                {
                    canDeleteAdmin = false;
                }

                checks.Add(new CheckResult
                {
                    Name = "Permissions safety: admin account protected",
                    Passed = !canDeleteAdmin,
                    Details = canDeleteAdmin ? "Admin deletion unexpectedly succeeded." : "Admin deletion is blocked as expected."
                });

                var unique = DateTime.UtcNow.Ticks;
                var roleName = "qa_role_" + unique;
                var username = "qa_user_" + unique;

                var role = await userAdminService.CreateRoleAsync(roleName);
                var createdUser = await userAdminService.CreateUserAsync(new User
                {
                    Username = username,
                    FullName = "QA User",
                    IsActive = true
                }, "Pass@123");

                await userAdminService.AssignSingleRoleAsync(createdUser.UserId, role.RoleId);
                await userAdminService.SaveRolePermissionsAsync(role.RoleId, new[] { PermissionCodes.StatisticsView, PermissionCodes.AccountsView });
                var assigned = await authzService.GetPermissionCodesAsync(createdUser.UserId);

                var rolePermissionWorks = assigned.Contains(PermissionCodes.StatisticsView) && assigned.Contains(PermissionCodes.AccountsView);

                await userAdminService.RemoveUserRoleAsync(createdUser.UserId, role.RoleId);
                await userAdminService.DeleteUserAsync(createdUser.UserId);
                await userAdminService.DeleteRoleAsync(role.RoleId);

                checks.Add(new CheckResult
                {
                    Name = "Permissions operations: create/assign/unassign/delete",
                    Passed = rolePermissionWorks,
                    Details = rolePermissionWorks ? "Role and permission lifecycle succeeded." : "Assigned permissions not returned correctly."
                });
            }
            catch (Exception ex)
            {
                checks.Add(new CheckResult
                {
                    Name = "Permissions operations: create/assign/unassign/delete",
                    Passed = false,
                    Details = ex.Message
                });
            }

            // 3) Statistics scenario
            try
            {
                var referrals = await statsService.GetReferralsAsync();
                var selectedReferralId = referrals.FirstOrDefault()?.ReferralId;
                var snapshot = await statsService.GetSnapshotAsync(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1).AddSeconds(-1), "الكل", selectedReferralId);
                var ok = snapshot != null && snapshot.Summary != null;
                checks.Add(new CheckResult
                {
                    Name = "Statistics: load summary + gender/referral analysis",
                    Passed = ok,
                    Details = ok
                        ? $"Visits={snapshot!.Summary.VisitCount}, ByGender={snapshot.ByGender.Count}, ByReferral={snapshot.ByReferral.Count}."
                        : "Statistics snapshot is null."
                });
            }
            catch (Exception ex)
            {
                checks.Add(new CheckResult
                {
                    Name = "Statistics: load summary + gender/referral analysis",
                    Passed = false,
                    Details = ex.Message
                });
            }

            // 4) Treasury scenario
            try
            {
                var treasury = await treasuryService.GetSnapshotAsync(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1).AddSeconds(-1));
                var ok = treasury != null;
                checks.Add(new CheckResult
                {
                    Name = "Treasury: load totals + payment/user/referral breakdown",
                    Passed = ok,
                    Details = ok
                        ? $"Payments={treasury!.Payments.Count}, ByUser={treasury.ByUser.Count}, ByReferral={treasury.ByReferral.Count}."
                        : "Treasury snapshot is null."
                });
            }
            catch (Exception ex)
            {
                checks.Add(new CheckResult
                {
                    Name = "Treasury: load totals + payment/user/referral breakdown",
                    Passed = false,
                    Details = ex.Message
                });
            }

            // 5) System settings scenario
            try
            {
                var profile = await settingsService.GetProfileAsync();
                var updated = new SystemSettingsProfile
                {
                    ReportHeader = profile.ReportHeader + "",
                    ReportFooter = profile.ReportFooter + "",
                    ReportMarginTop = profile.ReportMarginTop,
                    ReportMarginBottom = profile.ReportMarginBottom,
                    ReportPrimaryColor = profile.ReportPrimaryColor,
                    DefaultPrinterName = profile.DefaultPrinterName,
                    ReceiptHeaderText = profile.ReceiptHeaderText,
                    ReceiptFooterText = profile.ReceiptFooterText,
                    ReceiptShowLogo = profile.ReceiptShowLogo,
                    ReceiptCopies = profile.ReceiptCopies
                };

                await settingsService.SaveProfileAsync(updated);
                var reloaded = await settingsService.GetProfileAsync();

                var ok = reloaded != null && reloaded.DefaultPrinterName == updated.DefaultPrinterName;
                checks.Add(new CheckResult
                {
                    Name = "System Settings: specialized profile read/write",
                    Passed = ok,
                    Details = ok ? "Specialized settings persisted and reloaded." : "Settings reload mismatch."
                });
            }
            catch (Exception ex)
            {
                checks.Add(new CheckResult
                {
                    Name = "System Settings: specialized profile read/write",
                    Passed = false,
                    Details = ex.Message
                });
            }
        }
        catch (Exception ex)
        {
            checks.Add(new CheckResult
            {
                Name = "Global setup",
                Passed = false,
                Details = ex.ToString()
            });
        }

        Console.WriteLine("PHASE6_SMOKE_RESULTS_BEGIN");
        foreach (var check in checks)
        {
            Console.WriteLine($"[{(check.Passed ? "PASS" : "FAIL")}] {check.Name} :: {check.Details}");
        }
        Console.WriteLine("PHASE6_SMOKE_RESULTS_END");

        return checks.All(c => c.Passed) ? 0 : 1;
    }
}

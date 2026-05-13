using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AviationMaintenance.Application.DTOs.Bulletin;
using AviationMaintenance.Application.Interfaces;
using AviationMaintenance.Domain.Entities;
using AviationMaintenance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AviationMaintenance.Application.Services
{
    /// <summary>
    /// Service implementation for bulletin management operations
    /// </summary>
    public class BulletinService : IBulletinService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContextService _userContext;
        private readonly HierarchyService _hierarchyService;
        private readonly ILogger<BulletinService> _logger;
        private readonly IAuditLogger _auditLogger;

        public BulletinService(
            IApplicationDbContext context,
            IUserContextService userContext,
            HierarchyService hierarchyService,
            ILogger<BulletinService> logger,
            IAuditLogger auditLogger)
        {
            _context = context;
            _userContext = userContext;
            _hierarchyService = hierarchyService;
            _logger = logger;
            _auditLogger = auditLogger;
        }

        public async Task<BulletinResponse> CreateBulletinAsync(CreateBulletinRequest request)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            // Validate RBAC permissions
            await ValidateCreatePermissionsAsync(request.Scope, request.SectionId, request.HangarId, request.ShopId);

            var bulletin = new Bulletin
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Priority = request.Priority,
                Category = request.Category,
                Scope = request.Scope,
                CreatedByUserId = currentUserId,
                SectionId = request.SectionId,
                HangarId = request.HangarId,
                ShopId = request.ShopId,
                PublishedAt = request.PublishImmediately ? DateTime.UtcNow : DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate,
                IsActive = request.PublishImmediately,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bulletins.Add(bulletin);
            await _context.SaveChangesAsync();

            // Log audit event
            await _auditLogger.LogAsync(new AuditEvent
            {
                Action = "BulletinCreated",
                EntityId = bulletin.Id,
                EntityType = "Bulletin",
                Details = $"Bulletin '{bulletin.Title}' created with scope {bulletin.Scope}",
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Bulletin {BulletinId} created by user {UserId}", bulletin.Id, currentUserId);

            return await MapToBulletinResponseAsync(bulletin);
        }

        public async Task<BulletinResponse> UpdateBulletinAsync(UpdateBulletinRequest request)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await _context.Bulletins
                .Include(b => b.CreatedByUser)
                .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted);

            if (bulletin == null)
                throw new KeyNotFoundException($"Bulletin with ID {request.Id} not found");

            // Validate update permissions
            await ValidateUpdatePermissionsAsync(bulletin);

            // Update fields
            if (!string.IsNullOrEmpty(request.Title))
                bulletin.Title = request.Title;

            if (!string.IsNullOrEmpty(request.Content))
                bulletin.Content = request.Content;

            if (request.Priority.HasValue)
                bulletin.Priority = request.Priority.Value;

            if (request.Category.HasValue)
                bulletin.Category = request.Category.Value;

            if (request.ExpiryDate.HasValue)
                bulletin.ExpiryDate = request.ExpiryDate.Value;

            if (request.IsActive.HasValue)
                bulletin.IsActive = request.IsActive.Value;

            bulletin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit event
            await _auditLogger.LogAsync(new AuditEvent
            {
                Action = "BulletinUpdated",
                EntityId = bulletin.Id,
                EntityType = "Bulletin",
                Details = $"Bulletin '{bulletin.Title}' updated",
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Bulletin {BulletinId} updated by user {UserId}", bulletin.Id, currentUserId);

            return await MapToBulletinResponseAsync(bulletin);
        }

        public async Task<BulletinResponse?> GetBulletinByIdAsync(Guid id)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await GetAccessibleBulletinAsync(id);

            if (bulletin == null)
                return null;

            // Mark as read if not already read
            await MarkAsReadIfNotReadAsync(bulletin.Id, currentUserId);

            return await MapToBulletinResponseAsync(bulletin);
        }

        public async Task<BulletinListResponse> ListBulletinsAsync(BulletinListRequest request)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var query = _context.Bulletins
                .Include(b => b.CreatedByUser)
                .Include(b => b.Section)
                .Include(b => b.Hangar)
                .Include(b => b.Shop)
                .Include(b => b.Attachments)
                .Include(b => b.ReadStatuses.Where(rs => rs.UserId == currentUserId))
                .Where(b => !b.IsDeleted && b.IsActive && !b.IsExpired)
                .AsQueryable();

            // Apply RBAC filtering
            query = ApplyRbacFiltering(query, currentUser);

            // Apply filters
            if (request.Priority.HasValue)
                query = query.Where(b => b.Priority == request.Priority.Value);

            if (request.Category.HasValue)
                query = query.Where(b => b.Category == request.Category.Value);

            if (request.Scope.HasValue)
                query = query.Where(b => b.Scope == request.Scope.Value);

            if (request.SectionId.HasValue)
                query = query.Where(b => b.SectionId == request.SectionId.Value);

            if (request.HangarId.HasValue)
                query = query.Where(b => b.HangarId == request.HangarId.Value);

            if (request.ShopId.HasValue)
                query = query.Where(b => b.ShopId == request.ShopId.Value);

            if (request.CreatedByUserId.HasValue)
                query = query.Where(b => b.CreatedByUserId == request.CreatedByUserId.Value);

            if (request.IsActive.HasValue)
                query = query.Where(b => b.IsActive == request.IsActive.Value);

            if (request.IsExpired.HasValue)
                query = query.Where(b => b.IsExpired == request.IsExpired.Value);

            if (request.DateFrom.HasValue)
                query = query.Where(b => b.PublishedAt >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(b => b.PublishedAt <= request.DateTo.Value);

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || 
                                       b.Content.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var bulletins = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var bulletinResponses = new List<BulletinResponse>();
            foreach (var bulletin in bulletins)
            {
                bulletinResponses.Add(await MapToBulletinResponseAsync(bulletin));
            }

            // Get unread counts
            var unreadCount = await GetUnreadCountAsync();
            var highPriorityUnreadCount = await GetHighPriorityUnreadCountAsync();

            return new BulletinListResponse
            {
                Bulletins = bulletinResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1,
                UnreadCount = unreadCount,
                HighPriorityUnreadCount = highPriorityUnreadCount
            };
        }

        public async Task<bool> DeleteBulletinAsync(Guid id)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await _context.Bulletins
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (bulletin == null)
                return false;

            // Validate delete permissions
            await ValidateDeletePermissionsAsync(bulletin);

            bulletin.IsDeleted = true;
            bulletin.IsActive = false;
            bulletin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit event
            await _auditLogger.LogAsync(new AuditEvent
            {
                Action = "BulletinDeleted",
                EntityId = bulletin.Id,
                EntityType = "Bulletin",
                Details = $"Bulletin '{bulletin.Title}' deleted",
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Bulletin {BulletinId} deleted by user {UserId}", bulletin.Id, currentUserId);

            return true;
        }

        public async Task<bool> ActivateBulletinAsync(Guid id)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await _context.Bulletins
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (bulletin == null)
                return false;

            // Validate activation permissions
            await ValidateUpdatePermissionsAsync(bulletin);

            bulletin.IsActive = true;
            bulletin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit event
            await _auditLogger.LogAsync(new AuditEvent
            {
                Action = "BulletinActivated",
                EntityId = bulletin.Id,
                EntityType = "Bulletin",
                Details = $"Bulletin '{bulletin.Title}' activated",
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }

        public async Task<bool> DeactivateBulletinAsync(Guid id)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await _context.Bulletins
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (bulletin == null)
                return false;

            // Validate deactivation permissions
            await ValidateUpdatePermissionsAsync(bulletin);

            bulletin.IsActive = false;
            bulletin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit event
            await _auditLogger.LogAsync(new AuditEvent
            {
                Action = "BulletinDeactivated",
                EntityId = bulletin.Id,
                EntityType = "Bulletin",
                Details = $"Bulletin '{bulletin.Title}' deactivated",
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }

        public async Task<bool> MarkBulletinAsReadAsync(Guid bulletinId)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await GetAccessibleBulletinAsync(bulletinId);

            if (bulletin == null)
                return false;

            await MarkAsReadIfNotReadAsync(bulletinId, currentUserId);

            return true;
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var accessibleBulletinIds = await GetAccessibleBulletinIdsAsync(currentUser);

            var readBulletinIds = await _context.BulletinReadStatuses
                .Where(rs => rs.UserId == currentUserId && rs.IsRead)
                .Select(rs => rs.BulletinId)
                .ToListAsync();

            var unreadCount = accessibleBulletinIds.Count(id => !readBulletinIds.Contains(id));

            return unreadCount;
        }

        public async Task<int> GetHighPriorityUnreadCountAsync()
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var accessibleBulletinIds = await _context.Bulletins
                .Where(b => !b.IsDeleted && b.IsActive && !b.IsExpired && b.Priority == BulletinPriority.High)
                .ApplyRbacFiltering(currentUser)
                .Select(b => b.Id)
                .ToListAsync();

            var readBulletinIds = await _context.BulletinReadStatuses
                .Where(rs => rs.UserId == currentUserId && rs.IsRead)
                .Select(rs => rs.BulletinId)
                .ToListAsync();

            var unreadCount = accessibleBulletinIds.Count(id => !readBulletinIds.Contains(id));

            return unreadCount;
        }

        public async Task<List<BulletinResponse>> GetDashboardBulletinsAsync(int maxCount = 10)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var bulletins = await _context.Bulletins
                .Include(b => b.CreatedByUser)
                .Include(b => b.Section)
                .Include(b => b.Hangar)
                .Include(b => b.Shop)
                .Include(b => b.Attachments)
                .Include(b => b.ReadStatuses.Where(rs => rs.UserId == currentUserId))
                .Where(b => !b.IsDeleted && b.IsActive && !b.IsExpired)
                .ApplyRbacFiltering(currentUser)
                .OrderByDescending(b => b.Priority)
                .ThenByDescending(b => b.PublishedAt)
                .Take(maxCount)
                .ToListAsync();

            var bulletinResponses = new List<BulletinResponse>();
            foreach (var bulletin in bulletins)
            {
                bulletinResponses.Add(await MapToBulletinResponseAsync(bulletin));
            }

            return bulletinResponses;
        }

        public async Task<BulletinAnalyticsResponse?> GetBulletinAnalyticsAsync(Guid bulletinId)
        {
            var currentUserId = _userContext.EmployeeId;
            var bulletin = await GetAccessibleBulletinAsync(bulletinId);

            if (bulletin == null)
                return null;

            // Only allow analytics for bulletins the user created or for Directors
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (bulletin.CreatedByUserId != currentUserId && currentUser.Role != "Director")
                throw new UnauthorizedAccessException("Insufficient permissions to view analytics");

            var totalRecipients = await GetTotalRecipientsCountAsync(bulletin);
            var readCount = await _context.BulletinReadStatuses
                .Where(rs => rs.BulletinId == bulletinId && rs.IsRead)
                .CountAsync();

            var userReadStatuses = await _context.BulletinReadStatuses
                .Include(rs => rs.User)
                .Include(rs => rs.User.Section)
                .Include(rs => rs.User.Hangar)
                .Include(rs => rs.User.Shop)
                .Where(rs => rs.BulletinId == bulletinId)
                .Select(rs => new UserReadStatus
                {
                    UserId = rs.UserId,
                    UserName = rs.User.FullName,
                    UserRole = rs.User.Role,
                    SectionName = rs.User.Section != null ? rs.User.Section.Name : null,
                    HangarName = rs.User.Hangar != null ? rs.User.Hangar.Name : null,
                    ShopName = rs.User.Shop != null ? rs.User.Shop.Name : null,
                    IsRead = rs.IsRead,
                    ReadAt = rs.ReadAt
                })
                .ToListAsync();

            return new BulletinAnalyticsResponse
            {
                BulletinId = bulletin.Id,
                BulletinTitle = bulletin.Title,
                TotalRecipients = totalRecipients,
                ReadCount = readCount,
                UnreadCount = totalRecipients - readCount,
                UserReadStatuses = userReadStatuses,
                PublishedAt = bulletin.PublishedAt,
                ExpiryDate = bulletin.ExpiryDate,
                IsExpired = bulletin.IsExpired
            };
        }

        public async Task<BulletinListResponse> GetExpiredBulletinsAsync(BulletinListRequest request)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            // Only Directors can access expired bulletins
            if (currentUser.Role != "Director")
                throw new UnauthorizedAccessException("Only Directors can access expired bulletins");

            var query = _context.Bulletins
                .Include(b => b.CreatedByUser)
                .Include(b => b.Section)
                .Include(b => b.Hangar)
                .Include(b => b.Shop)
                .Include(b => b.Attachments)
                .Where(b => !b.IsDeleted && b.IsExpired)
                .AsQueryable();

            // Apply filters
            if (request.Priority.HasValue)
                query = query.Where(b => b.Priority == request.Priority.Value);

            if (request.Category.HasValue)
                query = query.Where(b => b.Category == request.Category.Value);

            if (request.Scope.HasValue)
                query = query.Where(b => b.Scope == request.Scope.Value);

            if (request.DateFrom.HasValue)
                query = query.Where(b => b.PublishedAt >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(b => b.PublishedAt <= request.DateTo.Value);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var bulletins = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var bulletinResponses = new List<BulletinResponse>();
            foreach (var bulletin in bulletins)
            {
                bulletinResponses.Add(await MapToBulletinResponseAsync(bulletin));
            }

            return new BulletinListResponse
            {
                Bulletins = bulletinResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1,
                UnreadCount = 0, // Expired bulletins don't count towards unread
                HighPriorityUnreadCount = 0
            };
        }

        public async Task<List<BulletinAnalyticsResponse>> GetHighPriorityBulletinsAnalyticsAsync()
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            // Only Team Leaders, Managers, and Directors can access analytics
            if (!new[] { "Team Leader", "Manager", "Director" }.Contains(currentUser.Role))
                throw new UnauthorizedAccessException("Insufficient permissions to view analytics");

            var highPriorityBulletins = await _context.Bulletins
                .Where(b => !b.IsDeleted && b.IsActive && !b.IsExpired && b.Priority == BulletinPriority.High)
                .ApplyRbacFiltering(currentUser)
                .ToListAsync();

            var analytics = new List<BulletinAnalyticsResponse>();
            foreach (var bulletin in highPriorityBulletins)
            {
                var bulletinAnalytics = await GetBulletinAnalyticsAsync(bulletin.Id);
                if (bulletinAnalytics != null)
                {
                    analytics.Add(bulletinAnalytics);
                }
            }

            return analytics;
        }

        #region Private Helper Methods

        private async Task ValidateCreatePermissionsAsync(BulletinScope scope, Guid? sectionId, Guid? hangarId, Guid? shopId)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            switch (scope)
            {
                case BulletinScope.Hangar:
                    if (currentUser.Role != "Team Leader")
                        throw new UnauthorizedAccessException("Only Team Leaders can create Hangar-level bulletins");
                    
                    if (!hangarId.HasValue)
                        throw new ArgumentException("Hangar ID is required for Hangar scope");

                    // Verify user belongs to the specified hangar
                    if (currentUser.HangarId != hangarId.Value)
                        throw new UnauthorizedAccessException("You can only create bulletins for your assigned hangar");
                    break;

                case BulletinScope.Section:
                    if (currentUser.Role != "Manager")
                        throw new UnauthorizedAccessException("Only Managers can create Section-level bulletins");
                    
                    if (!sectionId.HasValue)
                        throw new ArgumentException("Section ID is required for Section scope");

                    // Verify user belongs to the specified section
                    if (currentUser.SectionId != sectionId.Value)
                        throw new UnauthorizedAccessException("You can only create bulletins for your assigned section");
                    break;

                case BulletinScope.Global:
                    if (currentUser.Role != "Director")
                        throw new UnauthorizedAccessException("Only Directors can create Global bulletins");
                    break;
            }
        }

        private async Task ValidateUpdatePermissionsAsync(Bulletin bulletin)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            // Directors can update any bulletin
            if (currentUser.Role == "Director")
                return;

            // Users can only update their own bulletins
            if (bulletin.CreatedByUserId != currentUserId)
                throw new UnauthorizedAccessException("You can only update your own bulletins");
        }

        private async Task ValidateDeletePermissionsAsync(Bulletin bulletin)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            // Directors can delete any bulletin
            if (currentUser.Role == "Director")
                return;

            // Users can only delete their own draft bulletins
            if (bulletin.CreatedByUserId != currentUserId || bulletin.IsActive)
                throw new UnauthorizedAccessException("You can only delete your own draft bulletins");
        }

        private async Task<Bulletin?> GetAccessibleBulletinAsync(Guid id)
        {
            var currentUserId = _userContext.EmployeeId;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var bulletin = await _context.Bulletins
                .Include(b => b.CreatedByUser)
                .Include(b => b.Section)
                .Include(b => b.Hangar)
                .Include(b => b.Shop)
                .Include(b => b.Attachments)
                .Include(b => b.ReadStatuses.Where(rs => rs.UserId == currentUserId))
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (bulletin == null)
                return null;

            // Check RBAC permissions
            if (!CanAccessBulletin(bulletin, currentUser))
                return null;

            return bulletin;
        }

        private bool CanAccessBulletin(Bulletin bulletin, User currentUser)
        {
            switch (bulletin.Scope)
            {
                case BulletinScope.Global:
                    return true; // Everyone can access global bulletins

                case BulletinScope.Section:
                    return bulletin.SectionId == currentUser.SectionId ||
                           currentUser.Role == "Director" ||
                           currentUser.Role == "Manager";

                case BulletinScope.Hangar:
                    return bulletin.HangarId == currentUser.HangarId ||
                           currentUser.Role == "Director" ||
                           currentUser.Role == "Manager" ||
                           (currentUser.Role == "Team Leader" && bulletin.HangarId == currentUser.HangarId);

                default:
                    return false;
            }
        }

        private async Task<List<Guid>> GetAccessibleBulletinIdsAsync(User currentUser)
        {
            return await _context.Bulletins
                .Where(b => !b.IsDeleted && b.IsActive && !b.IsExpired)
                .ApplyRbacFiltering(currentUser)
                .Select(b => b.Id)
                .ToListAsync();
        }

        private async Task MarkAsReadIfNotReadAsync(Guid bulletinId, Guid userId)
        {
            var existingReadStatus = await _context.BulletinReadStatuses
                .FirstOrDefaultAsync(rs => rs.BulletinId == bulletinId && rs.UserId == userId);

            if (existingReadStatus == null)
            {
                var readStatus = new BulletinReadStatus
                {
                    Id = Guid.NewGuid(),
                    BulletinId = bulletinId,
                    UserId = userId,
                    IsRead = true,
                    ReadAt = DateTime.UtcNow
                };

                _context.BulletinReadStatuses.Add(readStatus);
                await _context.SaveChangesAsync();

                // Log audit event
                await _auditLogger.LogAsync(new AuditEvent
                {
                    Action = "BulletinRead",
                    EntityId = bulletinId,
                    EntityType = "Bulletin",
                    Details = $"Bulletin {bulletinId} marked as read",
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                });
            }
            else if (!existingReadStatus.IsRead)
            {
                existingReadStatus.IsRead = true;
                existingReadStatus.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log audit event
                await _auditLogger.LogAsync(new AuditEvent
                {
                    Action = "BulletinRead",
                    EntityId = bulletinId,
                    EntityType = "Bulletin",
                    Details = $"Bulletin {bulletinId} marked as read",
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        private async Task<int> GetTotalRecipientsCountAsync(Bulletin bulletin)
        {
            switch (bulletin.Scope)
            {
                case BulletinScope.Global:
                    return await _context.Users.CountAsync(u => u.IsActive);

                case BulletinScope.Section:
                    return await _context.Users
                        .CountAsync(u => u.IsActive && u.SectionId == bulletin.SectionId);

                case BulletinScope.Hangar:
                    return await _context.Users
                        .CountAsync(u => u.IsActive && u.HangarId == bulletin.HangarId);

                default:
                    return 0;
            }
        }

        private IQueryable<Bulletin> ApplyRbacFiltering(IQueryable<Bulletin> query, User currentUser)
        {
            switch (currentUser.Role)
            {
                case "Director":
                    // Directors can see all bulletins
                    return query;

                case "Manager":
                    // Managers can see section and global bulletins
                    return query.Where(b => b.Scope == BulletinScope.Global ||
                                         (b.Scope == BulletinScope.Section && b.SectionId == currentUser.SectionId));

                case "Team Leader":
                    // Team Leaders can see hangar, section, and global bulletins
                    return query.Where(b => b.Scope == BulletinScope.Global ||
                                         (b.Scope == BulletinScope.Section && b.SectionId == currentUser.SectionId) ||
                                         (b.Scope == BulletinScope.Hangar && b.HangarId == currentUser.HangarId));

                case "Employee":
                case "SAFA Inspector":
                default:
                    // Employees can see hangar, section, and global bulletins
                    return query.Where(b => b.Scope == BulletinScope.Global ||
                                         (b.Scope == BulletinScope.Section && b.SectionId == currentUser.SectionId) ||
                                         (b.Scope == BulletinScope.Hangar && b.HangarId == currentUser.HangarId));
            }
        }

        private IQueryable<Bulletin> ApplySorting(IQueryable<Bulletin> query, string? sortBy, string? sortDirection)
        {
            var isAscending = sortDirection?.Equals("asc", StringComparison.OrdinalIgnoreCase) ?? false;

            return sortBy?.ToLowerInvariant() switch
            {
                "title" => isAscending ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
                "priority" => isAscending ? query.OrderBy(b => b.Priority) : query.OrderByDescending(b => b.Priority),
                "category" => isAscending ? query.OrderBy(b => b.Category) : query.OrderByDescending(b => b.Category),
                "expirydate" => isAscending ? query.OrderBy(b => b.ExpiryDate) : query.OrderByDescending(b => b.ExpiryDate),
                "createdat" => isAscending ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt),
                "updatedat" => isAscending ? query.OrderBy(b => b.UpdatedAt) : query.OrderByDescending(b => b.UpdatedAt),
                _ => isAscending ? query.OrderBy(b => b.PublishedAt) : query.OrderByDescending(b => b.PublishedAt)
            };
        }

        private async Task<BulletinResponse> MapToBulletinResponseAsync(Bulletin bulletin)
        {
            var currentUserId = _userContext.EmployeeId;
            var readStatus = bulletin.ReadStatuses.FirstOrDefault(rs => rs.UserId == currentUserId);
            var totalRecipients = await GetTotalRecipientsCountAsync(bulletin);
            var readCount = await _context.BulletinReadStatuses
                .Where(rs => rs.BulletinId == bulletin.Id && rs.IsRead)
                .CountAsync();

            return new BulletinResponse
            {
                Id = bulletin.Id,
                Title = bulletin.Title,
                Content = bulletin.Content,
                Priority = bulletin.Priority,
                Category = bulletin.Category,
                Scope = bulletin.Scope,
                CreatedByUserId = bulletin.CreatedByUserId,
                CreatedByUserName = bulletin.CreatedByUser?.FullName ?? "Unknown",
                SectionId = bulletin.SectionId,
                SectionName = bulletin.Section?.Name,
                HangarId = bulletin.HangarId,
                HangarName = bulletin.Hangar?.Name,
                ShopId = bulletin.ShopId,
                ShopName = bulletin.Shop?.Name,
                PublishedAt = bulletin.PublishedAt,
                ExpiryDate = bulletin.ExpiryDate,
                IsActive = bulletin.IsActive,
                IsExpired = bulletin.IsExpired,
                IsRead = readStatus?.IsRead ?? false,
                ReadAt = readStatus?.ReadAt,
                AttachmentCount = bulletin.Attachments?.Count ?? 0,
                TotalRecipients = totalRecipients,
                ReadCount = readCount,
                UnreadCount = totalRecipients - readCount,
                CreatedAt = bulletin.CreatedAt,
                UpdatedAt = bulletin.UpdatedAt,
                CreatedByUserRole = bulletin.CreatedByUser?.Role ?? "Unknown"
            };
        }

        #endregion
    }
}

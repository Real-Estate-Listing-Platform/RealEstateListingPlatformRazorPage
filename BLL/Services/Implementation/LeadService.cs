using BLL.DTOs;
using BLL.Services;
using DAL.Models;
using DAL.Repositories;

namespace BLL.Services.Implementation
{
    public class LeadService : ILeadService

    {
        private readonly ILeadRepository _leadRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IListingViewRepository _listingViewRepository;
        private readonly IEmailService _emailService;

        public LeadService(
            ILeadRepository leadRepository, 
            IListingRepository listingRepository, 
            IUserRepository userRepository, 
            INotificationService notificationService,
            IListingViewRepository listingViewRepository,
            IEmailService emailService)
        {
            _leadRepository = leadRepository;
            _listingRepository = listingRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _listingViewRepository = listingViewRepository;
            _emailService = emailService;
        }


        public async Task<ServiceResult<Lead>> CreateLeadAsync(Guid listingId, Guid seekerId, string? message, DateTime? appointmentDate = null)
        {
            try
            {
                // Validate listing exists and is published
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null)
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "Listing not found."
                    };
                }

                if (listing.Status != "Published")
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "This listing is not available."
                    };
                }

                // Validate seeker exists
                var seeker = await _userRepository.GetUserById(seekerId);
                if (seeker == null)
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Check if lead already exists
                var existingLead = await _leadRepository.GetExistingLeadAsync(listingId, seekerId);
                if (existingLead != null)
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "You have already expressed interest in this property.",
                        Data = existingLead
                    };
                }

                // Create new lead
                var lead = new Lead
                {
                    Id = Guid.NewGuid(),
                    ListingId = listingId,
                    SeekerId = seekerId,
                    ListerId = listing.ListerId,
                    Message = message,
                    AppointmentDate = appointmentDate,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                };

                var createdLead = await _leadRepository.CreateLeadAsync(lead);

                // Get lister details for email
                var lister = await _userRepository.GetUserById(listing.ListerId);

                // Send notification to lister
                await _notificationService.NotifyNewLeadAsync(
                    listing.ListerId, 
                    createdLead.Id, 
                    listing.Title, 
                    seeker.DisplayName
                );

                // Send email notification to lister
                if (lister != null && !string.IsNullOrWhiteSpace(lister.Email))
                {
                    try
                    {
                        var emailSubject = $"New Lead: {seeker.DisplayName} is interested in your property";
                        var emailBody = BuildLeadNotificationEmail(
                            lister.DisplayName,
                            seeker.DisplayName,
                            seeker.Email,
                            seeker.Phone ?? "Not provided",
                            listing.Title,
                            message ?? "No message provided",
                            appointmentDate
                        );
                        
                        await _emailService.SendEmailAsync(lister.Email, emailSubject, emailBody);
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the lead creation
                        Console.WriteLine($"Failed to send email notification: {emailEx.Message}");
                    }
                }

                return new ServiceResult<Lead>
                {
                    Success = true,
                    Message = "Your interest has been sent to the property owner.",
                    Data = createdLead
                };

            }
            catch (Exception ex)
            {
                return new ServiceResult<Lead>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<Lead>> GetLeadByIdAsync(Guid id, Guid userId)
        {
            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(id);
                if (lead == null)
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "Lead not found."
                    };
                }

                // Verify user has access (either lister or seeker)
                if (lead.ListerId != userId && lead.SeekerId != userId)
                {
                    return new ServiceResult<Lead>
                    {
                        Success = false,
                        Message = "You do not have permission to view this lead."
                    };
                }

                return new ServiceResult<Lead>
                {
                    Success = true,
                    Data = lead
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Lead>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<Lead>>> GetMyLeadsAsListerAsync(Guid listerId, string? statusFilter = null)
        {
            try
            {
                var leads = await _leadRepository.GetLeadsByListerIdAsync(listerId);

                if (!string.IsNullOrWhiteSpace(statusFilter))
                {
                    leads = leads.Where(l => l.Status?.Equals(statusFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                return new ServiceResult<List<Lead>>
                {
                    Success = true,
                    Data = leads
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Lead>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<Lead>>> GetMyLeadsAsSeekerAsync(Guid seekerId)
        {
            try
            {
                var leads = await _leadRepository.GetLeadsBySeekerIdAsync(seekerId);
                return new ServiceResult<List<Lead>>
                {
                    Success = true,
                    Data = leads
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Lead>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<Lead>>> GetLeadsByListingIdAsync(Guid listingId, Guid listerId)
        {
            try
            {
                // Verify ownership
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null || listing.ListerId != listerId)
                {
                    return new ServiceResult<List<Lead>>
                    {
                        Success = false,
                        Message = "You do not have permission to view leads for this listing."
                    };
                }

                var leads = await _leadRepository.GetLeadsByListingIdAsync(listingId);
                return new ServiceResult<List<Lead>>
                {
                    Success = true,
                    Data = leads
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Lead>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateLeadStatusAsync(Guid leadId, Guid listerId, string newStatus, string? listerNote = null)
        {
            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Lead not found."
                    };
                }

                if (lead.ListerId != listerId)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "You do not have permission to update this lead."
                    };
                }

                // Validate status
                var validStatuses = new[] { "New", "Contacted", "Closed" };
                if (!validStatuses.Contains(newStatus))
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid status value."
                    };
                }

                lead.Status = newStatus;
                if (!string.IsNullOrWhiteSpace(listerNote))
                {
                    lead.ListerNote = listerNote;
                }

                await _leadRepository.UpdateLeadAsync(lead);

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Lead status updated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateAppointmentAsync(Guid leadId, Guid listerId, DateTime appointmentDate)
        {
            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Lead not found."
                    };
                }

                if (lead.ListerId != listerId)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "You do not have permission to update this lead."
                    };
                }

                lead.AppointmentDate = appointmentDate;
                await _leadRepository.UpdateLeadAsync(lead);

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Appointment date updated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteLeadAsync(Guid leadId, Guid userId)
        {
            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Lead not found."
                    };
                }

                // Only lister or seeker can delete
                if (lead.ListerId != userId && lead.SeekerId != userId)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "You do not have permission to delete this lead."
                    };
                }

                await _leadRepository.DeleteLeadAsync(lead);

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Lead deleted successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<LeadStatistics>> GetLeadStatisticsAsync(Guid listerId)
        {
            try
            {
                var totalLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId);
                var newLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "New");
                var contactedLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "Contacted");
                var closedLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "Closed");
                var recentLeads = await _leadRepository.GetRecentLeadsByListerIdAsync(listerId, 5);

                var statistics = new LeadStatistics
                {
                    TotalLeads = totalLeads,
                    NewLeads = newLeads,
                    ContactedLeads = contactedLeads,
                    ClosedLeads = closedLeads,
                    RecentLeads = recentLeads
                };

                return new ServiceResult<LeadStatistics>
                {
                    Success = true,
                    Data = statistics
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<LeadStatistics>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync(Guid listerId)
        {
            try
            {
                var stats = new DashboardStatsDto();
                var now = DateTime.UtcNow;

                // === SECTION 1: Listing Metrics ===
                var listings = await _listingRepository.GetListingsByListerIdAsync(listerId);
                stats.TotalListings = listings.Count;
                stats.ActiveListings = listings.Count(l => l.Status == "Published");
                stats.PendingReview = listings.Count(l => l.Status == "PendingReview");
                stats.DraftListings = listings.Count(l => l.Status == "Draft");
                stats.ExpiredListings = listings.Count(l => l.Status == "Expired" || (l.ExpirationDate.HasValue && l.ExpirationDate < now));
                stats.RejectedListings = listings.Count(l => l.Status == "Rejected");
                stats.PublishSuccessRate = stats.TotalListings > 0 
                    ? Math.Round((double)stats.ActiveListings / stats.TotalListings * 100, 2) 
                    : 0;

                // === SECTION 2: Lead/Customer Metrics ===
                stats.TotalLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId);
                stats.NewLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "New");
                stats.ContactedLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "Contacted");
                stats.ClosedLeads = await _leadRepository.GetLeadCountByListerIdAsync(listerId, "Closed");
                stats.ConversionRate = stats.TotalLeads > 0 
                    ? Math.Round((double)stats.ClosedLeads / stats.TotalLeads * 100, 2) 
                    : 0;

                // Get last lead received time
                var recentLeads = await _leadRepository.GetRecentLeadsByListerIdAsync(listerId, 1);
                stats.LastLeadReceivedAt = recentLeads.FirstOrDefault()?.CreatedAt;

                // === SECTION 3: View/Engagement Metrics (Last 30 Days) ===
                // Note: This requires IListingViewRepository which should be injected
                // For now, we'll set default values. You'll need to inject IListingViewRepository to get actual data
                stats.TotalViews = 0;
                stats.AverageViewsPerListing = 0;
                stats.MostViewedListingTitle = null;
                stats.MostViewedListingViews = 0;
                stats.MostViewedListingId = null;

                // === SECTION 4: Package/Subscription Metrics ===
                // Note: This requires IPackageService which should be injected
                // For now, we'll calculate basic package metrics
                stats.ActivePackages = 0;
                stats.BoostedListings = listings.Count(l => l.IsBoosted);
                stats.RemainingPhotoCapacity = 0;
                stats.DaysUntilNextExpiration = null;
                stats.NextPackageExpiration = null;

                // === Additional Helper Properties ===
                stats.ExpiringListingsSoon = listings.Count(l => 
                    l.ExpirationDate.HasValue && 
                    l.ExpirationDate > now && 
                    l.ExpirationDate <= now.AddDays(7));

                // === Chart Data (Last 30 Days) ===
                var allLeads = await _leadRepository.GetLeadsByListerIdAsync(listerId);
                
                // Generate chart data for the last 7 days
                for (int i = 6; i >= 0; i--)
                {
                    var date = now.AddDays(-i).Date;
                    var dateLabel = date.ToString("MMM dd");
                    
                    // Leads created on this day
                    var leadsOnDate = allLeads.Count(l => l.CreatedAt.HasValue && l.CreatedAt.Value.Date == date);
                    stats.LeadsChartData.Add(new ChartDataPoint
                    {
                        Label = dateLabel,
                        Value = leadsOnDate
                    });
                    
                    // Conversion rate on this day (closed leads / total leads that day)
                    var closedOnDate = allLeads.Count(l => l.CreatedAt.HasValue && l.CreatedAt.Value.Date == date && l.Status == "Closed");
                    var conversionRate = leadsOnDate > 0 ? (int)Math.Round((double)closedOnDate / leadsOnDate * 100) : 0;
                    stats.ConversionChartData.Add(new ChartDataPoint
                    {
                        Label = dateLabel,
                        Value = conversionRate
                    });

                    // Views placeholder (would need ListingViewRepository)
                    stats.ViewsChartData.Add(new ChartDataPoint
                    {
                        Label = dateLabel,
                        Value = 0  // TODO: Implement with IListingViewRepository
                    });
                }

                // === Top Performing Listings ===
                var publishedListings = listings.Where(l => l.Status == "Published").ToList();
                var topListings = new List<TopPerformingListingDto>();

                foreach (var listing in publishedListings)
                {
                    var leadCount = allLeads.Count(l => l.ListingId == listing.Id);
                    
                    // Get total view count for this listing (all time, not just daily)
                    var viewCount = 0;
                    try
                    {
                        viewCount = await _listingViewRepository.GetTotalViewsAsync(listing.Id);
                    }
                    catch (Exception)
                    {
                        // If view tracking fails, continue with 0
                        viewCount = 0;
                    }
                    
                    // Calculate engagement score (weighted: leads are more valuable than views)
                    var engagementScore = (leadCount * 10) + (viewCount * 1);
                    
                    topListings.Add(new TopPerformingListingDto
                    {
                        Id = listing.Id,
                        Title = listing.Title,
                        ViewCount = viewCount,
                        LeadCount = leadCount,
                        EngagementScore = engagementScore
                    });
                }

                // Sort by engagement score and take top 3
                stats.TopPerformingListings = topListings
                    .OrderByDescending(l => l.EngagementScore)
                    .ThenByDescending(l => l.LeadCount)
                    .Take(3)
                    .ToList();

                return new ServiceResult<DashboardStatsDto>
                {
                    Success = true,
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<DashboardStatsDto>
                {
                    Success = false,
                    Message = $"An error occurred while gathering dashboard statistics: {ex.Message}"
                };
            }
        }

        private string BuildLeadNotificationEmail(
            string listerName,
            string seekerName,
            string seekerEmail,
            string seekerPhone,
            string listingTitle,
            string message,
            DateTime? appointmentDate)
        {
            var appointmentHtml = appointmentDate.HasValue 
                ? $@"
                    <tr>
                        <td style=""padding: 15px; background-color: #fff3cd; border-radius: 8px; border-left: 4px solid #ffc107;"">
                            <p style=""margin: 0; color: #856404;"">
                                <strong>?? Preferred Appointment:</strong><br/>
                                {appointmentDate.Value:dddd, MMMM dd, yyyy 'at' hh:mm tt}
                            </p>
                        </td>
                    </tr>"
                : "";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Lead Notification</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); overflow: hidden;"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">
                                ?? New Lead Received!
                            </h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""font-size: 16px; color: #333; margin: 0 0 20px 0; line-height: 1.6;"">
                                Hi <strong>{listerName}</strong>,
                            </p>
                            
                            <p style=""font-size: 16px; color: #333; margin: 0 0 30px 0; line-height: 1.6;"">
                                Great news! Someone is interested in your property listing:
                            </p>
                            
                            <!-- Property Info Box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 25px;"">
                                <tr>
                                    <td style=""padding: 20px; background-color: #f8f9fa; border-radius: 8px; border-left: 4px solid #667eea;"">
                                        <h3 style=""margin: 0 0 10px 0; color: #667eea; font-size: 18px;"">
                                            {listingTitle}
                                        </h3>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Customer Details -->
                            <h3 style=""color: #333; font-size: 18px; margin: 0 0 15px 0; border-bottom: 2px solid #667eea; padding-bottom: 10px;"">
                                Customer Information
                            </h3>
                            
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 25px;"">
                                <tr>
                                    <td style=""padding: 12px 0; border-bottom: 1px solid #eeeeee;"">
                                        <strong style=""color: #666;"">Name:</strong>
                                        <span style=""color: #333; float: right;"">{seekerName}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 12px 0; border-bottom: 1px solid #eeeeee;"">
                                        <strong style=""color: #666;"">Email:</strong>
                                        <a href=""mailto:{seekerEmail}"" style=""color: #667eea; text-decoration: none; float: right;"">{seekerEmail}</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 12px 0; border-bottom: 1px solid #eeeeee;"">
                                        <strong style=""color: #666;"">Phone:</strong>
                                        <a href=""tel:{seekerPhone}"" style=""color: #667eea; text-decoration: none; float: right;"">{seekerPhone}</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Message -->
                            <h3 style=""color: #333; font-size: 18px; margin: 0 0 15px 0; border-bottom: 2px solid #667eea; padding-bottom: 10px;"">
                                Message from Customer
                            </h3>
                            
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 25px;"">
                                <tr>
                                    <td style=""padding: 15px; background-color: #f8f9fa; border-radius: 8px; border-left: 4px solid #667eea;"">
                                        <p style=""margin: 0; color: #333; font-style: italic; line-height: 1.6;"">
                                            {message}
                                        </p>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Appointment Date (if provided) -->
                            {appointmentHtml}
                            
                            <!-- Action Button -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""http://localhost:5191/Lister/Customers"" style=""display: inline-block; padding: 14px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 25px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);"">
                                            View Lead Details
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""font-size: 14px; color: #666; margin: 30px 0 0 0; line-height: 1.6; border-top: 1px solid #eeeeee; padding-top: 20px;"">
                                💡 <strong>Quick Tip:</strong> Responding quickly increases your chances of closing the deal. We recommend reaching out within 24 hours!
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #eeeeee;"">
                            <p style=""margin: 0 0 10px 0; color: #666; font-size: 14px;"">
                                <strong>Estately</strong> - Real Estate Listing Platform
                            </p>
                            <p style=""margin: 0; color: #999; font-size: 12px;"">
                                This is an automated notification. Please do not reply to this email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}

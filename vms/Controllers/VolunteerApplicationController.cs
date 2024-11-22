﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vms.Data;
using vms.Models;

namespace vms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VolunteerApplicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Apply for a volunteer opportunity (User)
        [HttpPost("apply")]
        public async Task<ActionResult<VolunteerApplication>> ApplyForOpportunity([FromBody] VolunteerApplication application)
        {
            var existingApplication = await _context.VolunteerApplications
                .FirstOrDefaultAsync(a => a.UserId == application.UserId && a.VolunteerOpportunityId == application.VolunteerOpportunityId);

            if (existingApplication != null)
            {
                return BadRequest("You have already applied for this opportunity.");
            }

            _context.VolunteerApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }

        // Get all applications for a specific volunteer opportunity (Organization)
        [HttpGet("applications/{opportunityId}")]
        public async Task<ActionResult<List<VolunteerApplication>>> GetApplicationsForOpportunity(int opportunityId)
        {
            var applications = await _context.VolunteerApplications
                .Include(a => a.User) // Include user details
                .Where(a => a.VolunteerOpportunityId == opportunityId)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
            {
                return NotFound("No applications found for this opportunity.");
            }

            return Ok(applications);
        }

        // Accept or Reject an application (Organization)
        [HttpPatch("accept/{applicationId}")]
        public async Task<ActionResult> AcceptApplication(int applicationId, [FromBody] bool isAccepted)
        {
            var application = await _context.VolunteerApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Application not found.");
            }

            application.IsAccepted = isAccepted;

            _context.VolunteerApplications.Update(application);
            await _context.SaveChangesAsync();

            return NoContent(); // Status code 204: No Content, successful request
        }

        // Get all applications for a specific organization
        [HttpGet("organization/{organizationId}/applications")]
        public async Task<ActionResult<List<VolunteerApplication>>> GetApplicationsForOrganization(int organizationId)
        {
            var applications = await _context.VolunteerApplications
                .Include(a => a.User)
                .Include(a => a.VolunteerOpportunity)
                .Where(a => a.VolunteerOpportunity.OrganizationId == organizationId)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
            {
                return NotFound("No applications found for this organization.");
            }

            return Ok(applications);
        }
        [HttpGet("user/{userId}/applications")]
        public async Task<ActionResult<List<VolunteerApplication>>> GetUserApplications(int userId)
        {
            var applications = await _context.VolunteerApplications
                .Include(a => a.VolunteerOpportunity) // Include opportunity details
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
            {
                return NotFound("No applications found for this user.");
            }

            return Ok(applications);
        }


        [HttpDelete("delete/{applicationId}")]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var application = await _context.VolunteerApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Application not found.");
            }

            _context.VolunteerApplications.Remove(application);
            await _context.SaveChangesAsync();

            return Ok("Applicant deleted successfully.");
        }
        [HttpGet("check")]
        public async Task<IActionResult> CheckApplication([FromQuery] int userId, [FromQuery] int opportunityId)
        {
            var applicationExists = await _context.VolunteerApplications
                .AnyAsync(a => a.UserId == userId && a.VolunteerOpportunityId == opportunityId);

            if (applicationExists)
            {
                return Ok(new { Applied = true, Message = "User has already applied for this opportunity." });
            }

            return Ok(new { Applied = false, Message = "User has not applied for this opportunity." });
        }

    }
}

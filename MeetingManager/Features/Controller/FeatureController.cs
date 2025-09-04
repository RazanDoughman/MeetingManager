using MeetingManager;
using MeetingManager.Features.Model;
using MeetingManager.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingManager.Features.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeatureController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        // GET: api/Feature
        [HttpGet]
        public async Task<ActionResult<List<Feature>>> GetAll()
        {
            return await _featureService.GetAllFeaturesAsync();
        }


        // GET: api/Feature/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Feature>> GetById(Guid id)
        {
            var feature = await _featureService.GetFeatureByIdAsync(id);
            if (feature == null)
                return NotFound();

            return feature;
        }

        // PUT: api/Feature/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(Guid id, Feature feature)
        {
            var updated = await _featureService.UpdateFeatureAsync(id, feature);
            if (!updated)
                return BadRequest();

            return NoContent();
        }

        // POST: api/Feature
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Feature>> Create(Feature feature)
        {
            var created = await _featureService.CreateFeatureAsync(feature);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // DELETE: api/Feature/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _featureService.DeleteFeatureAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

    }
}

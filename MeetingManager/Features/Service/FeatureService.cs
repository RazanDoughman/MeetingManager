using MeetingManager.Features.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingManager.Service
{
    public class FeatureService : IFeatureService
    {
        private readonly AppDbContext _context;

        public FeatureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await _context.Features.ToListAsync();
        }

        public async Task<Feature> GetFeatureByIdAsync(Guid id)
        {
            return await _context.Features.FindAsync(id);
        }

        public async Task<Feature> CreateFeatureAsync(Feature feature)
        {
            _context.Features.Add(feature);
            await _context.SaveChangesAsync();
            return feature;
        }

        public async Task<bool> UpdateFeatureAsync(Guid id, Feature feature)
        {
            if (id != feature.Id)
                return false;

            _context.Entry(feature).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFeatureAsync(Guid id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null)
                return false;

            _context.Features.Remove(feature);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

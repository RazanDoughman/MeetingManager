using MeetingManager.Features.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingManager.Service
{
    public interface IFeatureService
    {
        Task<List<Feature>> GetAllFeaturesAsync();
        Task<Feature> GetFeatureByIdAsync(Guid id);
        Task<Feature> CreateFeatureAsync(Feature feature);
        Task<bool> UpdateFeatureAsync(Guid id, Feature feature);
        Task<bool> DeleteFeatureAsync(Guid id);
    }
}

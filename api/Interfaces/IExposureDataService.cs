
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWD.VicExposureSites
{
    public interface IExposureDataService
    {
        Task<List<DiscoverDataRecord>> GetData();
    }
}
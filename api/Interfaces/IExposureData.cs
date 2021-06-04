
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWD.VicExposureSites
{
    public interface IExposureData
    {
        Task<List<DiscoverDataRecord>> GetData();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Service.BusinessLogic.Dtos;

namespace UTCert.Service.BusinessLogic.Interface
{
    public interface IHomeService
    {
        public Task<DashboardDto> GetDashboardDataAsync(Guid userId);
    }
}

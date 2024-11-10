using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;
using UTCert.Service.BusinessLogic.Common;
using UTCert.Service.BusinessLogic.Dtos;
using UTCert.Service.BusinessLogic.Interface;

namespace UTCert.Service.BusinessLogic
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public HomeService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }
        public async Task<DashboardDto> GetDashboardDataAsync(Guid userId)
        {
            var dto = new DashboardDto();

            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == userId)
                       ?? throw new Exception("User isn't exist !!");
            dto.Username = user.Name;
            dto.IsVerified = user.IsVerified;
            dto.AvatarUri = user.AvatarUri;

            var contactStats = await _unitOfWork.ContactRepository.GetAll()
                .Where(x => x.IssuerId == userId)
                .GroupBy(x => x.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            var certificateStats = await _unitOfWork.CertificateRepository.GetAll()
                .Where(x => x.IssuerId == userId)
                .GroupBy(x => x.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            var contactDict = contactStats.ToDictionary(x => x.Key, x => x.Count);
            var certificateDict = certificateStats.ToDictionary(x => x.Key, x => x.Count);

            dto.Pending = contactDict.GetValueOrDefault(ContactStatus.Pending, 0);
            dto.Accepted = contactDict.GetValueOrDefault(ContactStatus.Accepted, 0);
            dto.Draft = certificateDict.GetValueOrDefault((byte)CertificateStatus.Draft, 0);
            dto.Signed = certificateDict.GetValueOrDefault((byte)CertificateStatus.Signed, 0);
            dto.Banned = certificateDict.GetValueOrDefault((byte)CertificateStatus.Banned, 0);
            dto.Sent = certificateDict.GetValueOrDefault((byte)CertificateStatus.Sent, 0);
            dto.Received = await _unitOfWork.CertificateRepository.GetAll()
                .CountAsync(x => x.ReceiverId == userId && x.Status == (int)CertificateStatus.Signed);

            return dto;
        }

    }
}

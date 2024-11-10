
using Microsoft.AspNetCore.Http;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.Certificate;
using UTCert.Model.Web.Dtos;
using UTCert.Service.BusinessLogic.Dtos;

namespace UTCert.Service.BusinessLogic.Interface;

public interface ICertificateService
{
    Task<PagedResultDto<CertificateDto>> GetCertificateIssued(CertificateFilterInputDto input);
    Task<PagedResultDto<CertificateDto>> GetCertificateReceived(CertificateFilterInputDto input);
    Task<bool> DeleteCertificate(Guid certId); 
    Task<bool> SignCertificate(SignCertificateRequest input);
    Task<bool> SignMultipleCertificates(List<SignCertificateRequest> inputs);
    Task<bool> SendCertificate(Guid certificateId);
    Task<bool> SendMultipleCertificates(List<Guid> certificateIds);
    Task<bool> BanCertificate(Guid certificateId);
    Task<bool> BanMultipleCertificates(List<Guid> certificateIds);
    Task<Guid> Create(Guid issuerId, CertificateCreationDto certificate);
    Task<bool> CreateFromExcel(Guid issuerId, IFormFile certificate);
}
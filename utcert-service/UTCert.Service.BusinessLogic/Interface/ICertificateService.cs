
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
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
    Task<bool> BanCertificate(CertificateUploadDto input);
    Task<bool> BanMultipleCertificates(List<CertificateUploadDto> inputs);
    Task<Guid> Create(Guid issuerId, CertificateCreationDto certificate);
    Task<bool> CreateFromExcel(Guid issuerId, IFormFile certificate);
    Task<bool> CheckCertificateLegal(string identifyNumber);
    Task<bool> UploadAttachment(CertificateUploadDto input);

    Task<bool> DeleteMultipleCertificates(List<Guid> ids); 
}
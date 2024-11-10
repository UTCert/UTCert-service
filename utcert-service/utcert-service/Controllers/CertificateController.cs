using Microsoft.AspNetCore.Mvc;
using utcert_service.Authorization;
using utcert_service.ResponseModel;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.Certificate;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.BusinessLogic.Dtos;
using UTCert.Model.Web.Dtos;
using System.Drawing.Imaging;
using System.Drawing;
using UTCert.Model.Shared.Common;

namespace utcert_service.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CertificateController : BaseController
{
    private readonly ICertificateService _certificateService;
    private readonly IUnitOfWork _unitOfWork;

    public CertificateController(ICertificateService certificateService, IUnitOfWork unitOfWork)
    {
        _certificateService = certificateService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("get-certificate-issued")]
    public async Task<ApiResponse<PagedResultDto<CertificateDto>>> GetCertificateIssued(CertificateFilterInputDto input)
    {
        try
        {
            input.IssuerId = User.Id;
            input.IssuerAddress = User.ReceiveAddress; 
            var dataList = await _certificateService.GetCertificateIssued(input);

            return new ApiResponse<PagedResultDto<CertificateDto>>()
            {
                Data = dataList,
                Success = true,
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    [HttpPost("get-certificate-received")]
    public async Task<ApiResponse<PagedResultDto<CertificateDto>>> GetCertificateReceived(CertificateFilterInputDto input)
    {
        input.ReceiverId = User.Id;
        input.ReceiverAddress = User.ReceiveAddress;
        var certificates = await _certificateService.GetCertificateReceived(input);
        return new ApiResponse<PagedResultDto<CertificateDto>>(certificates);
    }

    [HttpPost("sign-certificate")]
    public async Task<ApiResponse<bool>> SignCertificate(SignCertificateRequest input)
    {
        var result = await _certificateService.SignCertificate(input);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("sign-multiple-certificates")]
    public async Task<ApiResponse<bool>> SignMultipleCertificates(List<SignCertificateRequest> inputs)
    {
        var result = await _certificateService.SignMultipleCertificates(inputs);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("send-certificate")]
    public async Task<ApiResponse<bool>> SendCertificate([FromBody] Guid certificateId)
    {
        var result = await _certificateService.SendCertificate(certificateId);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("send-multiple-certificates")]
    public async Task<ApiResponse<bool>> SendMultipleCertificates(List<Guid> certificateIds)
    {
        var result = await _certificateService.SendMultipleCertificates(certificateIds);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("ban-certificate")]
    public async Task<ApiResponse<bool>> BanCertificate(Guid certificateId)
    {
        var result = await _certificateService.BanCertificate(certificateId);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("ban-multiple-certificates")]
    public async Task<ApiResponse<bool>> BanMultipleCertificates(List<Guid> certificateIds)
    {
        var result = await _certificateService.BanMultipleCertificates(certificateIds);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("create")]
    public async Task<ApiResponse<Guid>> Create(CertificateCreationDto certificate)
    {
        var result = await _certificateService.Create(CurrentUserId, certificate);
        return new ApiResponse<Guid>(result);
    }

    [HttpPost("create-from-excel")]
    public async Task<ApiResponse<bool>> CreateFromExcel(IFormFile certificate)
    {
        var result = await _certificateService.CreateFromExcel(CurrentUserId, certificate);
        return new ApiResponse<bool>(result);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> DeleteCertificate(Guid id)
    {
        try
        {
            var res = await _certificateService.DeleteCertificate(id);
            return new ApiResponse<bool>
            {
                Success = res,
                Data = res,
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

}
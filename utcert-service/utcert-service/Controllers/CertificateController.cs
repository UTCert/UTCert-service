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
            return new ApiResponse<PagedResultDto<CertificateDto>>()
            {
                Message = ex.Message,
                Success = false,
            };
        }

    }

    [HttpPost("get-certificate-received")]
    public async Task<ApiResponse<PagedResultDto<CertificateDto>>> GetCertificateReceived(CertificateFilterInputDto input)
    {
        try
        {
            input.ReceiverId = User.Id;
            input.ReceiverAddress = User.ReceiveAddress;
            var certificates = await _certificateService.GetCertificateReceived(input);
            return new ApiResponse<PagedResultDto<CertificateDto>>(certificates);
        } catch(Exception ex)
        {
            return new ApiResponse<PagedResultDto<CertificateDto>>
            {
                Success = false,
                Message = ex.Message
            }; 
        }

    }

    [HttpPost("sign-certificate")]
    public async Task<ApiResponse<bool>> SignCertificate(SignCertificateRequest input)
    {
        try
        {
            var result = await _certificateService.SignCertificate(input);
            return new ApiResponse<bool>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            }; 
        }
      
    }

    [HttpPost("sign-multiple-certificates")]
    public async Task<ApiResponse<bool>> SignMultipleCertificates(List<SignCertificateRequest> inputs)
    {
        try
        {
            var result = await _certificateService.SignMultipleCertificates(inputs);
            return new ApiResponse<bool>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false, 
                Message = ex.Message
            };
        }
 ;
    }

    [HttpPost("send-certificate")]
    public async Task<ApiResponse<bool>> SendCertificate([FromBody] Guid certificateId)
    {
        try
        {
            var result = await _certificateService.SendCertificate(certificateId);
            return new ApiResponse<bool>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            };
        }
      
    }

    [HttpPost("send-multiple-certificates")]
    public async Task<ApiResponse<bool>> SendMultipleCertificates(List<Guid> certificateIds)
    {
        try
        {
            var result = await _certificateService.SendMultipleCertificates(certificateIds);
            return new ApiResponse<bool>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            }; 
        }       
    }

    [HttpPost("ban-certificate")]
    public async Task<ApiResponse<bool>> BanCertificate([FromBody] CertificateUploadDto input)
    {
        var result = await _certificateService.BanCertificate(input);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("ban-multiple-certificates")]
    public async Task<ApiResponse<bool>> BanMultipleCertificates([FromBody] List<CertificateUploadDto> inputs)
    {
        var result = await _certificateService.BanMultipleCertificates(inputs);
        return new ApiResponse<bool>(result);
    }

    [HttpPost("create")]
    public async Task<ApiResponse<Guid>> Create(CertificateCreationDto certificate)
    {
        try
        {
            var result = await _certificateService.Create(CurrentUserId, certificate);
            return new ApiResponse<Guid>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = ex.Message
            };
        }    
    }

    [HttpPost("create-from-excel")]
    public async Task<ApiResponse<bool>> CreateFromExcel(IFormFile certificate)
    {
        try
        {
            var result = await _certificateService.CreateFromExcel(CurrentUserId, certificate);
            return new ApiResponse<bool>(result);
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
            };
        }
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
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
            };
        }

    }

    [HttpGet("check-legal/{identifyNumber}")]
    [AllowAnonymous]
    public async Task<bool> CheckCertificateLegal(string identifyNumber)
    {
        return await _certificateService.CheckCertificateLegal(identifyNumber); 
    }

    [HttpPost("upload-attachment")]
    public async Task<ApiResponse<bool>> UploadAttachment(CertificateUploadDto input)
    {
        try
        {
            var res = await _certificateService.UploadAttachment(input);
            return new ApiResponse<bool>
            {
                Success = res,
                Data = res,
            };
        } catch(Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message, 
            };
        }
        
    }

    [HttpPost("delete-multiple-cert")]
    public async Task<ApiResponse<bool>> DeleteMultipleCertificates(List<Guid> ids)
    {
        try
        {
            var res = await _certificateService.DeleteMultipleCertificates(ids);
            return new ApiResponse<bool>
            {
                Success = res,
                Data = res,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

}
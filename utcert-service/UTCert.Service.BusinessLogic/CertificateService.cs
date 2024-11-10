using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using UTCert.Data.Repository.Common.ExtensionMethod;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Common;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.Certificate;
using UTCert.Model.Web.Dtos;
using UTCert.Service.BusinessLogic.Common;
using UTCert.Service.BusinessLogic.Dtos;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.Helper.Interface;

namespace UTCert.Service.BusinessLogic;

public class CertificateService : EntityService<Certificate>, ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPinataService _pinataService;
    private readonly ICloudinaryService _cloudinaryService;

    public CertificateService(IUnitOfWork unitOfWork, IPinataService pinataService, ICloudinaryService cloudinaryService) : base(unitOfWork, unitOfWork.CertificateRepository)
    {
        _unitOfWork = unitOfWork;
        _pinataService = pinataService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<PagedResultDto<CertificateDto>> GetCertificateIssued(CertificateFilterInputDto input)
    {
        var _certRepos = _unitOfWork.CertificateRepository.GetAll().AsNoTracking();

        var query = from cert in _certRepos.Where(x => x.IssuerId == input.IssuerId
                    || (!string.IsNullOrEmpty(x.MulSignJson) && x.MulSignJson.Contains(input.IssuerAddress)))
                    join contact in _unitOfWork.ContactRepository.GetAll().AsNoTracking()
                    on new { cert.IssuerId, cert.ReceiverId } equals new { contact.IssuerId, contact.ReceiverId } into ContactDefaults
                    from cd in ContactDefaults.DefaultIfEmpty()
                    select new CertificateDto
                    {
                        Id = cert.Id,
                        Code = cert.Code,
                        Name = cert.Name,
                        Receiver = cert.Receiver,
                        Issuer = cert.Issuer,
                        ReceiverId = cert.ReceiverId,
                        IssuerId = cert.IssuerId,
                        ReceiverName = cert.ReceiverName, 
                        IssuerName = cert.IssuerName, 
                        Status = cert.Status,
                        IpfsLink = cert.IpfsLink,
                        GraduationYear = cert.GraduationYear,
                        ImageLink = cert.ImageLink,
                        SentDate = cert.SentDate,
                        SignedDate = cert.SignedDate,
                        StudyMode = cert.StudyMode,
                        SigningType = cert.SigningType,
                        Classification = cert.Classification,
                        ReceiverIdentityNumber = cert.ReceiverIdentityNumber,
                        ReceiverAddressWallet = cert.ReceiverAddressWallet,
                        ReceiverDoB = cert.ReceiverDoB,
                        ContactId = cd != null ? cd.Id : null,
                        ContactStatus = cd != null ? (int?)cd.Status : null,
                        MulSignJson = cert.MulSignJson,
                        SignHash = cert.SignHash,
                        AttachmentJson = cert.AttachmentJson, 
                        ReceivedDate = cert.ReceivedDate, 
                    };


        query = query
            .WhereIf(!string.IsNullOrEmpty(input.CertificateName), x => x.Name.Contains(input.CertificateName))
            .WhereIf(!string.IsNullOrEmpty(input.ReceivedName), x =>  x.ReceiverName.Contains(input.ReceivedName))
            .WhereIf(input.CertificateStatus.HasValue, x => (int)x.Status == input.CertificateStatus);

        if (!string.IsNullOrEmpty(input.Sorting))
        {
            query = query.OrderByDynamic(input.Sorting);
        }
        else
        {
            query = query.OrderByDescending(x => x.Code);
        }

        var rowCount = await query.CountAsync();
        var dataList = await query
            .PageBy((input.PageNumber - 1) * input.PageSize, input.PageSize)
            .ToListAsync();

        return new PagedResultDto<CertificateDto>
        {
            Items = dataList,
            TotalCount = rowCount,
        };
    }

    public async Task<PagedResultDto<CertificateDto>> GetCertificateReceived(CertificateFilterInputDto input)
    {
        var _certRepos = _unitOfWork.CertificateRepository.GetAll().AsNoTracking();
        var query = from cert in _certRepos
                    .Where(x => x.ReceiverId == input.ReceiverId
                     && !x.IsDeleted && x.Status == (int)CertificateStatus.Sent)
                    join contact in _unitOfWork.ContactRepository.GetAll().AsNoTracking()
                    on new { cert.IssuerId, cert.ReceiverId } equals new { contact.IssuerId, contact.ReceiverId } into ContactDefaults
                    from cd in ContactDefaults.DefaultIfEmpty()
                    select new CertificateDto
                    {
                        Id = cert.Id,
                        Code = cert.Code,
                        Name = cert.Name,
                        Receiver = cert.Receiver,
                        Issuer = cert.Issuer,
                        ReceiverId = cert.ReceiverId,
                        IssuerId = cert.IssuerId,
                        ReceiverName = cert.ReceiverName, 
                        IssuerName = cert.IssuerName,
                        Status = cert.Status,
                        IpfsLink = cert.IpfsLink,
                        GraduationYear = cert.GraduationYear,
                        ImageLink = cert.ImageLink,
                        SentDate = cert.SentDate,
                        SignedDate = cert.SignedDate,
                        StudyMode = cert.StudyMode,
                        Classification = cert.Classification,
                        ReceiverIdentityNumber = cert.ReceiverIdentityNumber,
                        ReceiverAddressWallet = cert.ReceiverAddressWallet,
                        ReceiverDoB = cert.ReceiverDoB,
                        ContactId = cd != null ? cd.Id : null,
                        ContactStatus = cd != null ? (int?)cd.Status : null,
                        ReceivedDate = cert.ReceivedDate,
                        AttachmentJson = cert.AttachmentJson, 
                    };

        query = query
            .WhereIf(!string.IsNullOrEmpty(input.CertificateName), x => x.Name.Contains(input.CertificateName))
            .WhereIf(!string.IsNullOrEmpty(input.OrganizationName), x => x.Issuer != null && x.IssuerName.Contains(input.OrganizationName))
            .WhereIf(input.CertificateStatus.HasValue, x => (int)x.Status == input.CertificateStatus);

        if (!string.IsNullOrEmpty(input.Sorting))
        {
            query = query.OrderByDynamic(input.Sorting);
        } else
        {
            query = query.OrderByDescending(x => x.Code);
        }

        var rowCount = await query.CountAsync();
        var dataList = await query
            .PageBy((input.PageNumber - 1) * input.PageSize, input.PageSize)
            .ToListAsync();

        return new PagedResultDto<CertificateDto>
        {
            Items = dataList,
            TotalCount = rowCount,
        };
    }

    public async Task<bool> DeleteCertificate(Guid certId)
    {
        var errorMessage = "Something wrong!!";
        try
        {
            var cert = await _unitOfWork.CertificateRepository.FirstOrDefaultAsync(x => x.Id == certId);
            if(cert == null)
            {
                errorMessage = "Certificate isn't exist!!";
                throw new Exception();
            }

            if (cert.Status == (int)CertificateStatus.Sent)
            {
                errorMessage = "The certificate that has been sent cannot be deleted";
                throw new Exception();
            }

            _unitOfWork.CertificateRepository.Delete(cert);
            return await _unitOfWork.CommitAsync() > 0;
        } catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception(errorMessage);
        }


    }

    public async Task<bool> SignCertificate(SignCertificateRequest input)
    {
        var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(input.CertificateId) ?? throw new AppException("Certificate not found");
        var issuer = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.IssuerId);
        var receiver = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.ReceiverId); 
        certificate.Status = (byte)CertificateStatus.Signed;
        certificate.SignedDate = DateTime.UtcNow;
        certificate.ModifiedDate = DateTime.UtcNow;
        certificate.SigningType = input.SigningType;
        certificate.SignHash = input.SignHash;

        if (input.SigningType == SigningType.MultipleSigning)
        {
            var lstSignerState = JsonSerializer.Deserialize<List<CertificateMulSignDto>>(certificate.MulSignJson,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Nếu JSON sử dụng camelCase
            });
            if ((lstSignerState == null || !lstSignerState.Any(x => x.IssuerAddress == input.IssuerAddress)) 
                && input.IssuerAddress != issuer?.ReceiveAddress)
            {
                throw new AppException("Signer not exist");
            }

            foreach (var item in lstSignerState)
            {
                if (item.IssuerAddress == input.IssuerAddress)
                {
                    item.IsSigned = true;
                    item.SignedDate = DateTime.UtcNow;
                }
            }
            certificate.MulSignJson = JsonSerializer.Serialize(lstSignerState, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        if (input.IssuerAddress == issuer.ReceiveAddress)
        {
            var isExistContact = await _unitOfWork.ContactRepository.AnyAsync(x => x.IssuerId == certificate.IssuerId && x.ReceiverId == certificate.ReceiverId);
            if (!isExistContact)
            {
                var newContact = new Contact
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    IssuerId = certificate.IssuerId,                 
                    ReceiverId = certificate.ReceiverId,
                    IssuerName = issuer.Name,
                    ReceiverName = receiver.Name,
                    Status = ContactStatus.Pending,
                };
                await _unitOfWork.ContactRepository.AddAsync(newContact);
            }
        }

        _unitOfWork.CertificateRepository.UpdateAsync(certificate);
        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> SignMultipleCertificates(List<SignCertificateRequest> lstCert)
    {
        var certificateIds = lstCert.Select(c => c.CertificateId).ToList();
        var certificates = await _unitOfWork.CertificateRepository.GetAll()
            .Where(x => certificateIds.Contains(x.Id)) 
            .ToListAsync();

        var lstContact = new List<Contact>();
        foreach (var certificate in certificates)
        {
            var input = lstCert.FirstOrDefault(x => x.CertificateId == certificate.Id);
            var issuer = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.IssuerId);
            var receiver = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.ReceiverId);

            if (input.SigningType == SigningType.MultipleSigning)
            {
                var lstSignerState = JsonSerializer.Deserialize<List<CertificateMulSignDto>>(certificate.MulSignJson);
                if ((lstSignerState == null || !lstSignerState.Any(x => x.IssuerAddress == input.IssuerAddress))
                && input.IssuerAddress != issuer?.ReceiveAddress)
                {
                    throw new AppException("Signer not exist");
                }

                foreach (var item in lstSignerState)
                {
                    if (item.IssuerAddress == input.IssuerAddress)
                    {
                        item.IsSigned = true;
                        item.SignedDate = DateTime.UtcNow;
                    }
                }

                certificate.MulSignJson = JsonSerializer.Serialize(lstSignerState, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            if (input.IssuerAddress == issuer.ReceiveAddress)
            {
                var isExistContact = await _unitOfWork.ContactRepository.AnyAsync(x => x.IssuerId == certificate.IssuerId && x.ReceiverId == certificate.ReceiverId);
                if (!isExistContact)
                {
                    var newContact = new Contact
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.Now,
                        IssuerId = certificate.IssuerId,
                        ReceiverId = certificate.ReceiverId,
                        IssuerName = issuer.Name,
                        ReceiverName = receiver.Name,
                        Status = ContactStatus.Pending,
                    };
                    lstContact.Add(newContact);
                }
            }

            certificate.Status = (byte)CertificateStatus.Signed;
            certificate.SignedDate = DateTime.UtcNow;
            certificate.ModifiedDate = DateTime.UtcNow;
            certificate.SigningType = input.SigningType;
        }

        _unitOfWork.CertificateRepository.UpdateRangeAsync(certificates);
        if (lstContact.Count > 0)
        {
            _unitOfWork.ContactRepository.UpdateRangeAsync(lstContact);
        }
        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> SendCertificate(Guid certificateId)
    {
        var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId)
                ?? throw new AppException("Certificate not found");
        certificate.Status = (byte)CertificateStatus.Sent;
        certificate.SentDate = DateTime.UtcNow;
        certificate.ModifiedDate = DateTime.UtcNow;
        certificate.ReceivedDate = DateTime.UtcNow;

        _unitOfWork.CertificateRepository.UpdateAsync(certificate);

        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> SendMultipleCertificates(List<Guid> certificateIds)
    {
        var certificates = await _unitOfWork.CertificateRepository.FindBy(x => certificateIds.Contains(x.Id)).ToListAsync();

        if (certificates == null)
        {
            throw new AppException("Certificate not found");
        }

        foreach (var certificate in certificates)
        {
            certificate.Status = (byte)CertificateStatus.Sent;
            certificate.SentDate = DateTime.UtcNow;
            certificate.ModifiedDate = DateTime.UtcNow;
            certificate.ReceivedDate = DateTime.UtcNow;

        }

        _unitOfWork.CertificateRepository.UpdateRangeAsync(certificates);
        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> BanCertificate(Guid certificateId)
    {
        var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);

        if (certificate == null)
        {
            throw new AppException("Certificate not found");
        }

        certificate.Status = (byte)CertificateStatus.Banned;
        certificate.IsBanned = true;
        certificate.ModifiedDate = DateTime.UtcNow;

        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> BanMultipleCertificates(List<Guid> certificateIds)
    {
        var certificates = await _unitOfWork.CertificateRepository.FindBy(x => certificateIds.Contains(x.Id)).ToListAsync();

        if (certificates == null)
        {
            throw new AppException("Certificate not found");
        }

        foreach (var certificate in certificates)
        {
            certificate.Status = (byte)CertificateStatus.Banned;
            certificate.IsBanned = true;
            certificate.ModifiedDate = DateTime.UtcNow;
        }

        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<Guid> Create(Guid issuerId, CertificateCreationDto certificate)
    {
        try
        {
            var _userRepos = _unitOfWork.UserRepository.GetAll().AsNoTracking();
            var code = await _unitOfWork.CertificateRepository.CountCertificate() + 1;

            var lstSignerState = new List<CertificateMulSignDto>();
            if (certificate.SigningType == SigningType.MultipleSigning)
            {
                if (string.IsNullOrEmpty(certificate.SignerAddress))
                {
                    throw new AppException("The designated signer is missing.");
                }
                else
                {
                    var lstSignerAddress = certificate.SignerAddress.Split(',');
                    if (_userRepos.Any(x => !lstSignerAddress.Any(y => y.Equals(x.ReceiveAddress))))
                    {
                        throw new AppException("The designated signer is not found.");
                    }
                    else
                    {
                        var lstUser = _userRepos.Where(x => lstSignerAddress.Any(y => y.Equals(x.ReceiveAddress))).ToList();
                        foreach (var signer in lstUser)
                        {
                            lstSignerState.Add(new CertificateMulSignDto
                            {
                                IsSigned = false,
                                IssuerAddress = signer.StakeId,
                                IssuerName = signer.Name,
                                SignedDate = null, 
                            });
                        }
                    }
                }
            }

            await DrawCertificate(issuerId, certificate, code.ToString("D6"));
            var imagePath = Directory.GetFiles(Constants.CertificateTemporaryFolder).FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new AppException("Cannot draw certificate image");
            }

            var receiver = await _unitOfWork.UserRepository.GetUserByStakeId(certificate.StakeId);
            var issuer = await _unitOfWork.UserRepository.GetUserById(issuerId);

            var attachmentJson = "";
            if (!string.IsNullOrEmpty(certificate.Attachment))
            {
                var attachmentIpfsLink = await _pinataService.Upload(certificate.Attachment);
                if (!string.IsNullOrEmpty(attachmentIpfsLink))
                {
                    throw new Exception($"File with path {certificate.Attachment} is not exist!");
                }

                var attachmentHash = await HashFile(certificate.Attachment, attachmentIpfsLink);
                attachmentJson = JsonSerializer.Serialize(new
                {
                    ipfsLink = attachmentIpfsLink,
                    hash = attachmentHash,
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var newCertificate = new Certificate
            {
                Id = Guid.NewGuid(),
                Code = code,
                IssuerId = issuerId,
                ReceiverId = receiver.Id,
                IssuerName = issuer.Name, 
                ReceiverName = receiver.Name, 
                Name = certificate.CertificateName,
                IpfsLink = await _pinataService.Upload(imagePath),
                ImageLink = await _cloudinaryService.Upload(imagePath),
                AttachmentJson = attachmentJson,
                Status = (byte)CertificateStatus.Draft,
                ReceiverAddressWallet = certificate.AddressWallet,
                ReceiverIdentityNumber = certificate.IdentityNumber,
                ReceiverDoB = certificate.DateOfBirth,
                GraduationYear = certificate.GraduationYear,
                Classification = certificate.Classification,
                StudyMode = certificate.StudyMode,
                CreatedDate = DateTime.UtcNow,
                IsBanned = false,
                IsDeleted = false,
                SigningType = certificate.SigningType,
                MulSignJson = certificate.SigningType == SigningType.MultipleSigning
                        ? JsonSerializer.Serialize(lstSignerState, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }) : null,
            };
            await CreateAsync(newCertificate);
            return newCertificate.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> CreateFromExcel(Guid issuerId, IFormFile certificate)
    {
        try
        {
            var _userRepos = _unitOfWork.UserRepository.GetAll().AsNoTracking();
            var certificates = new List<Certificate>();
            var certificateList = await ReadExcelFile(certificate);
            var code = await _unitOfWork.CertificateRepository.CountCertificate();

            foreach (var cert in certificateList)
            {
                var lstSignerState = new List<CertificateMulSignDto>();
                if (cert.SigningType == SigningType.MultipleSigning)
                {
                    if (string.IsNullOrEmpty(cert.SignerAddress))
                    {
                        throw new AppException("The designated signer is missing.");
                    }
                    else
                    {
                        var lstSignerAddress = cert.SignerAddress.Split(',');
                        if (!lstSignerAddress.Any(signerAddress => _userRepos.Any(user => user.ReceiveAddress == signerAddress)))
                        {
                            throw new AppException("The designated signer is not found.");
                        }
                        else
                        {
                            var lstUser = _userRepos.Where(x => lstSignerAddress.Any(y => y.Equals(x.ReceiveAddress))).ToList();
                            foreach (var signer in lstUser)
                            {
                                lstSignerState.Add(new CertificateMulSignDto
                                {
                                    IsSigned = false,
                                    IssuerAddress = signer.ReceiveAddress,
                                    IssuerName = signer.Name,
                                    SignedDate = null, 
                                });
                            }
                        }
                    }
                }

                code += 1;
                await DrawCertificate(issuerId, cert, code.ToString("D6"));
                var imagePath = Directory.GetFiles(Constants.CertificateTemporaryFolder).FirstOrDefault() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    throw new AppException("Cannot draw certificate image");
                }

                var receiver = await _unitOfWork.UserRepository.GetUserByStakeId(cert.StakeId);
                var issuer = await _unitOfWork.UserRepository.GetUserById(issuerId);

                var attachmentJson = ""; 
                if(!string.IsNullOrEmpty(cert.Attachment))
                {
                    var attachmentIpfsLink = await _pinataService.Upload(cert.Attachment); 
                    if(string.IsNullOrEmpty(attachmentIpfsLink))
                    {
                        throw new Exception($"File with path {cert.Attachment} is not exist!"); 
                    }

                    var attachmentHash = await HashFile(cert.Attachment, attachmentIpfsLink);
                    attachmentJson = JsonSerializer.Serialize(new
                    {
                        ipfsLink = attachmentIpfsLink, 
                        hash = attachmentHash, 
                    }, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                var newCertificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    IssuerId = issuerId,
                    ReceiverId = receiver.Id,
                    IssuerName = issuer.Name,
                    ReceiverName = receiver.Name,
                    Name = cert.CertificateName,
                    IpfsLink = await _pinataService.Upload(imagePath),
                    ImageLink = await _cloudinaryService.Upload(imagePath),
                    AttachmentJson = attachmentJson, 
                    Status = (byte)CertificateStatus.Draft,
                    ReceiverAddressWallet = cert.AddressWallet,
                    ReceiverIdentityNumber = cert.IdentityNumber,
                    ReceiverDoB = cert.DateOfBirth,
                    GraduationYear = cert.GraduationYear,
                    Classification = cert.Classification,
                    StudyMode = cert.StudyMode,
                    CreatedDate = DateTime.UtcNow,
                    IsBanned = false,
                    IsDeleted = false,
                    SigningType = cert.SigningType,
                    MulSignJson = cert.SigningType == SigningType.MultipleSigning
                        ? JsonSerializer.Serialize(lstSignerState, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }) : null,
                };
                certificates.Add(newCertificate);

            }

            await _unitOfWork.CertificateRepository.AddRangeAsync(certificates);
            return await _unitOfWork.CommitAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<List<CertificateCreationDto>> ReadExcelFile(IFormFile  file)
    {
        var certificateList = new List<CertificateCreationDto>();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (var row = 2; row <= rowCount; row++)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
            {
                continue;
            }

            var studyModeText = worksheet.Cells[row, 9].Text;
            if (!Enum.TryParse<StudyMode>(studyModeText, out var studyMode))
            {
                throw new ArgumentException($"Invalid StudyMode value '{studyModeText}' at row {row}");
            }

            var certificate = new CertificateCreationDto
            {
                StakeId = worksheet.Cells[row, 1].Text,
                AddressWallet = worksheet.Cells[row, 2].Text,
                IdentityNumber = worksheet.Cells[row, 3].Text,
                CertificateName = worksheet.Cells[row, 4].Text,
                ReceiverName = worksheet.Cells[row, 5].Text,
                DateOfBirth = DateTime.ParseExact(worksheet.Cells[row, 6].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                GraduationYear = int.Parse(worksheet.Cells[row, 7].Text),
                Classification = worksheet.Cells[row, 8].Text,
                StudyMode = studyMode,
                SigningType = worksheet.Cells[row, 10].Text == "Multiple Sign"
                    ? SigningType.MultipleSigning : SigningType.SingleSigning,
                SignerAddress = worksheet.Cells[row, 11].Text,
                Attachment = worksheet.Cells[row, 12].Text, 
            };

            certificateList.Add(certificate);
        }

        return certificateList;
    }

    private async Task DrawCertificate(Guid issuerId, CertificateCreationDto certificate, string code)
    {
        //TODO: Seems there is a bottleneck here, we should consider to use temporary folder to save the image
        EmptyFolder(Constants.CertificateTemporaryFolder);
        var issuer = await _unitOfWork.UserRepository.GetUserById(issuerId);

#pragma warning disable CA1416
        var image = new Bitmap(928, 648);
        var graphics = Graphics.FromImage(image);

        try
        {
            var backgroundImage = Image.FromFile(Constants.CertificateTemplatePath);
            graphics.DrawImage(backgroundImage, 0, 0, image.Width, image.Height);

            var stringSize1 = graphics.MeasureString(issuer.Name, new Font(Constants.FontFamily1, 18));
            var x1 = (image.Width - stringSize1.Width) / 2;
            var y1 = 50;
            graphics.DrawString(issuer.Name, new Font(Constants.FontFamily1, 18), Brushes.DarkSlateGray, x1, y1);

            // Received Name
            var stringSize2 = graphics.MeasureString(certificate.ReceiverName, new Font(Constants.FontFamily1, 48));
            var x2 = (image.Width - stringSize2.Width) / 2;
            var y2 = 260;
            graphics.DrawString(certificate.ReceiverName, new Font(Constants.FontFamily1, 48), Brushes.DarkSlateGray, x2, y2);

            graphics.DrawString("Certificate Name: " + certificate.CertificateName, new Font(Constants.FontFamily1, 14), Brushes.DarkSlateGray, 180, 370);
            graphics.DrawString("Date Of Birth: " + certificate.DateOfBirth.ToString("dd/MM/yyyy"), new Font(Constants.FontFamily1, 14), Brushes.DarkSlateGray, 570, 370);
            graphics.DrawString("Year Of Graduation: " + certificate.GraduationYear, new Font(Constants.FontFamily1, 14), Brushes.DarkSlateGray, 180, 420);
            graphics.DrawString("Classification: " + certificate.Classification, new Font(Constants.FontFamily1, 14), Brushes.DarkSlateGray, 180, 470);
            graphics.DrawString("Mode Of Study: " + certificate.StudyMode, new Font(Constants.FontFamily1, 14), Brushes.DarkSlateGray, 180, 520);
            graphics.DrawString("Certificate number: " + code, new Font(Constants.FontFamily1, 12), Brushes.DarkSlateGray, 140, 585);

            if (!Directory.Exists(Constants.CertificateTemporaryFolder))
            {
                Directory.CreateDirectory(Constants.CertificateTemporaryFolder);
            }

            var imageFileName = $"Cert_{code}.png";
            var imagePath = Path.Combine(Constants.CertificateTemporaryFolder, imageFileName);
            image.Save(imagePath);
#pragma warning restore CA1416
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void EmptyFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        var files = Directory.GetFiles(folderPath);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    private async Task<string> HashFile(string filePath, string ipfsLink)
    {
        byte[] fileBytes;

        try
        {
            fileBytes = await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception("Error reading file: " + ex.Message);
        }

        // Convert IPFS link to bytes
        byte[] ipfsLinkBytes = Encoding.UTF8.GetBytes(ipfsLink);

        // Combine file bytes and IPFS link bytes
        byte[] combinedBytes = new byte[fileBytes.Length + ipfsLinkBytes.Length];
        Buffer.BlockCopy(fileBytes, 0, combinedBytes, 0, fileBytes.Length);
        Buffer.BlockCopy(ipfsLinkBytes, 0, combinedBytes, fileBytes.Length, ipfsLinkBytes.Length);

        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(combinedBytes);

        var hashHex = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            hashHex.Append(b.ToString("x2"));
        }

        return hashHex.ToString();
    }
}
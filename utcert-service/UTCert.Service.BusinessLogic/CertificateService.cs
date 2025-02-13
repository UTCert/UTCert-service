﻿using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
                        Note = cert.Note,
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
                     && !x.IsDeleted && (x.Status == (int)CertificateStatus.Sent || x.Status == (int)CertificateStatus.Banned))
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
                        Note = cert.Note,
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
        try
        {
            var cert = await _unitOfWork.CertificateRepository.FirstOrDefaultAsync(x => x.Id == certId);
            if(cert == null)
            {
                throw new AppException("Certificate isn't exist!!");
            }

            if (cert.Status == (int)CertificateStatus.Sent)
            {
                throw new AppException("The certificate that has been sent cannot be deleted");
            }

            _unitOfWork.CertificateRepository.Delete(cert);
            return await _unitOfWork.CommitAsync() > 0;
        } catch(Exception ex)
        {
            throw new AppException(ex.Message);
        }
    }

    public async Task<bool> SignCertificate(SignCertificateRequest input)
    {
        var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(input.CertificateId) ?? throw new AppException("Certificate not found");
        var issuer = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.IssuerId);
        var receiver = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == certificate.ReceiverId); 
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

            if(lstSignerState.All(x => x.IsSigned))
            {
                certificate.Status = (int)CertificateStatus.Signed; 
            } else
            {
                certificate.Status = (int)CertificateStatus.Pending; 
            }
        } else
        {
            certificate.Status = (int)CertificateStatus.Signed; 
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

    public async Task<bool> BanCertificate(CertificateUploadDto input)
    {
        var certificate = await _unitOfWork.CertificateRepository.FirstOrDefaultAsync(x => x.Id == input.Id);

        if (certificate == null)
        {
            throw new AppException("Certificate not found");
        }

        certificate.Status = (byte)CertificateStatus.Banned;
        certificate.IsBanned = true;
        certificate.ModifiedDate = DateTime.UtcNow;
        certificate.Note = input.Note;
        _unitOfWork.CertificateRepository.UpdateAsync(certificate);
        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> BanMultipleCertificates(List<CertificateUploadDto> inputs)
    {
        var ids = inputs.Select(x => x.Id).ToList();
        var certificates = await _unitOfWork.CertificateRepository
            .FindBy(x => ids.Contains(x.Id))
            .ToListAsync();

        if (certificates == null || certificates.Count == 0)
        {
            throw new AppException("Certificate not found");
        }

        foreach (var certificate in certificates)
        {
            var input = inputs.FirstOrDefault(x => x.Id == certificate.Id);

            if (input != null)
            {
                certificate.Status = (byte)CertificateStatus.Banned;
                certificate.IsBanned = true;
                certificate.ModifiedDate = DateTime.UtcNow;
                certificate.Note = input.Note;
            }
        }

        _unitOfWork.CertificateRepository.UpdateRangeAsync(certificates);
        return await _unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> CheckCertificateLegal(string indentifyNumber)
    {
        var certificate = await _unitOfWork.CertificateRepository.FirstOrDefaultAsync(x => x.ReceiverIdentityNumber == indentifyNumber);
        if(certificate != null)
        {
            return !certificate.IsBanned; 
        }

        return true; 
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
                    if (!lstSignerAddress.Any(signerAddress => _userRepos.Any(user => user.ReceiveAddress == signerAddress)))
                    {
                        throw new AppException("The designated signer is not found.");
                    }
                    else
                    {
                        var lstUser = _userRepos
                          .Where(x => lstSignerAddress.Contains(x.ReceiveAddress) || lstSignerAddress.Contains(x.StakeId))
                          .ToList();

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

            await DrawCertificate(issuerId, certificate, code.ToString("D6"));
            var imagePath = Directory.GetFiles(Constants.CertificateTemporaryFolder).FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new AppException("Cannot draw certificate image");
            }

            var receiver = await _unitOfWork.UserRepository.GetUserByStakeId(certificate.StakeId);
            var issuer = await _unitOfWork.UserRepository.GetUserById(issuerId);

            var attachmentJson = "";
            if (!string.IsNullOrEmpty(certificate.Attachment) && !string.IsNullOrEmpty(certificate.AttachmentName))
            {
                var filePath = UploadFileToTempFolder(certificate.Attachment, certificate.AttachmentName);
                if(string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Can't save file to temp folder"); 
                    throw new AppException("Save file error!"); 
                }
                var attachmentIpfsLink = await _pinataService.Upload(filePath);
                if (string.IsNullOrEmpty(attachmentIpfsLink))
                {
                    throw new AppException("Save file error!");
                }

                var attachmentHash = await HashFile(filePath, attachmentIpfsLink);
                attachmentJson = JsonSerializer.Serialize(new
                {
                    ipfsName = certificate.AttachmentName,
                    ipfsLink = attachmentIpfsLink,
                    hash = attachmentHash,
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // delete file path after upload successfully
                File.Delete(filePath);
            }

            var newCertificate = new Certificate
            {
                Id = Guid.NewGuid(),
                Code = code,
                IssuerId = issuerId,
                ReceiverId = receiver.Id,
                IssuerName = issuer.Name, 
                ReceiverName = certificate.ReceiverName ?? receiver.Name, 
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
            throw new AppException(e.Message);
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
              

                var newCertificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    IssuerId = issuerId,
                    ReceiverId = receiver.Id,
                    IssuerName = issuer.Name,
                    ReceiverName = cert.ReceiverName ?? receiver.Name,
                    Name = cert.CertificateName,
                    IpfsLink = await _pinataService.Upload(imagePath),
                    ImageLink = await _cloudinaryService.Upload(imagePath),
                    AttachmentJson = "", 
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
            throw new Exception(e.Message);
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
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> UploadAttachment(CertificateUploadDto cert)
    {
        var certificate = await _unitOfWork.CertificateRepository.FirstOrDefaultAsync(x => x.Id == cert.Id) ?? throw new AppException("Certificate is not exist or deleted");

        if (!string.IsNullOrEmpty(cert.Attachment) && !string.IsNullOrEmpty(cert.AttachmentName))
        {
            var filePath = UploadFileToTempFolder(cert.Attachment, cert.AttachmentName);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new AppException("Save file error!");
            }
            var attachmentIpfsLink = await _pinataService.Upload(filePath);
            if (string.IsNullOrEmpty(attachmentIpfsLink))
            {
                throw new AppException("Save file error!");
            }

            var attachmentJson = JsonSerializer.Serialize(new
            {
                ipfsName = cert.AttachmentName,
                ipfsLink = attachmentIpfsLink,
                hash = await HashFile(filePath, attachmentIpfsLink),
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
          
            certificate.AttachmentJson = attachmentJson;
            _unitOfWork.CertificateRepository.UpdateAsync(certificate);
            File.Delete(filePath);
            return await _unitOfWork.CommitAsync() > 0;
        }

        return false; 
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

    private string UploadFileToTempFolder(string base64, string fileName)
    {
        if (!Directory.Exists(Constants.CertificateTemporaryFolder))
        {
            throw new Exception("folder path not exist!"); 
        };

        string tempPath = Path.Combine(Constants.CertificateTemporaryFolder, fileName);

        byte[] imageBytes = Convert.FromBase64String(base64);

        File.WriteAllBytes(tempPath, imageBytes);

        return tempPath; 

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
            Console.WriteLine(ex.Message); 
            throw new Exception("Error reading file path:" + filePath);
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

    public async Task<bool> DeleteMultipleCertificates(List<Guid> ids)
    {
        var certificates = await _unitOfWork.CertificateRepository.GetAll()
           .Where(x => ids.Contains(x.Id))
           .ToListAsync();

        if(certificates.Count > 0)
        {
            _unitOfWork.CertificateRepository.DeleteRange(certificates);
            return await _unitOfWork.CommitAsync() > 0;
        }

        return false; 
    }
}
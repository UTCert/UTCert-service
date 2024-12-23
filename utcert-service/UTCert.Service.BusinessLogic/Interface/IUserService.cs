﻿using UTCert.Model.Web.User;

namespace UTCert.Service.BusinessLogic.Interface;

public interface IUserService
{
    Task<Guid> Register(RegisterDto model);
    Task<UserResponseDto> Authenticate(string stakeId, string ipAddress);
    Task<UserResponseDto> RefreshToken(string token, string ipAddress);
    Task<bool> RevokeToken(string token, string ipAddress);
    IEnumerable<UserResponseDto> GetAll();
    Task<bool> HasAccount(string stakeId);
    Task<UserResponseDto> GetById(Guid id);
    Task<bool> Update(Guid id, UserCrudDto input); 
}
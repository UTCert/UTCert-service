﻿using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Model.Database;

namespace UTCert.Data.Repository.Interface;

public interface ICertificateRepository : IGenericRepository<Certificate>
{
    Task<int> CountCertificate();
}
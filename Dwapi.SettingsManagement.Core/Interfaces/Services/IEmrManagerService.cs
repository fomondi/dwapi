﻿using System;
using System.Collections.Generic;
using Dwapi.SettingsManagement.Core.Model;

namespace Dwapi.SettingsManagement.Core.Interfaces.Services
{
    public interface IEmrManagerService
    {
        IEnumerable<EmrSystem> GetAllEmrs();
        void SaveEmr(EmrSystem emrSystem);
        void DeleteEmr(Guid emrId);
        void SaveProtocol(DatabaseProtocol protocol);
        void DeleteProtocol(Guid protocolId);
        bool VerifyConnection(DatabaseProtocol databaseProtocol);
    }
}
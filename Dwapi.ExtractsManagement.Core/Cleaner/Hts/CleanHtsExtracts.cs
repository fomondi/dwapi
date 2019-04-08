﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dwapi.ExtractsManagement.Core.Interfaces.Cleaner.Hts;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository.Hts;
using Dwapi.ExtractsManagement.Core.Model.Destination.Dwh;
using Dwapi.ExtractsManagement.Core.Model.Destination.Hts;
using Dwapi.ExtractsManagement.Core.Notifications;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Events;
using Dwapi.SharedKernel.Model;
using Serilog;

namespace Dwapi.ExtractsManagement.Core.Cleaner.Hts
{
    public class CleanHtsExtracts : ICleanHtsExtracts
    {
        private readonly ITempHTSClientExtractRepository _tempPatientExtractRepository;
        private readonly IExtractHistoryRepository _historyRepository;

        public CleanHtsExtracts(ITempHTSClientExtractRepository tempPatientExtractRepository, IExtractHistoryRepository historyRepository)
        {
            _tempPatientExtractRepository = tempPatientExtractRepository;
            _historyRepository = historyRepository;
        }

        public async Task Clean(List<Guid> extractIds)
        {
            Log.Debug($"Executing ClearHtsExtracts command...");

            DomainEvents.Dispatch(new HtsNotification(new ExtractProgress(nameof(HTSClientExtract), "clearing...")));
            DomainEvents.Dispatch(new HtsNotification(new ExtractProgress(nameof(HTSClientPartnerExtract), "clearing...")));
            DomainEvents.Dispatch(new HtsNotification(new ExtractProgress(nameof(HTSClientLinkageExtract), "clearing...")));

            foreach (var extractId in extractIds)
            {
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Clearing));
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Clearing));
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Clearing));
            }


            await _historyRepository.ClearHistory(extractIds);
            await _tempPatientExtractRepository.Clear();


            foreach (var extractId in extractIds)
            {
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Cleared));
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Cleared));
                DomainEvents.Dispatch(new CbsStatusNotification(extractId, ExtractStatus.Cleared));
            }
        }
    }
}
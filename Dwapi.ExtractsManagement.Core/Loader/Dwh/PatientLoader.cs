﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dwapi.ExtractsManagement.Core.Interfaces.Loaders.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository.Dwh;
using Dwapi.ExtractsManagement.Core.Model.Destination.Dwh;
using Dwapi.ExtractsManagement.Core.Model.Source.Dwh;
using Dwapi.ExtractsManagement.Core.Notifications;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Events;
using Dwapi.SharedKernel.Model;
using Dwapi.SharedKernel.Utility;
using Serilog;

namespace Dwapi.ExtractsManagement.Core.Loader.Dwh
{
    public class PatientLoader : IPatientLoader
    {
        private readonly IPatientExtractRepository _patientExtractRepository;
        private readonly ITempPatientExtractRepository _tempPatientExtractRepository;

        public PatientLoader(IPatientExtractRepository patientExtractRepository, ITempPatientExtractRepository tempPatientExtractRepository)
        {
            _patientExtractRepository = patientExtractRepository;
            _tempPatientExtractRepository = tempPatientExtractRepository;
        }

        public async Task<int> Load(Guid extractId, int found)
        {
            try
            {
                DomainEvents.Dispatch(
                    new ExtractActivityNotification(extractId, new DwhProgress(
                        nameof(PatientExtract),
                        nameof(ExtractStatus.Loading),
                        found, 0, 0, 0, 0)));

                //load temp extracts without errors
                var tempPatientExtracts = _tempPatientExtractRepository.GetAll().Where(a=>a.CheckError == false);

                const int take = 1000;
                int skip = 0;
                var count = tempPatientExtracts.Count();
                while (skip < count)
                {
                    var batch = tempPatientExtracts.Skip(skip).Take(take).ToList();
                    //Auto mapper
                    var extractRecords = Mapper.Map<List<TempPatientExtract>, List<PatientExtract>>(batch);
                    foreach (var record in extractRecords)
                    {
                        record.Id = LiveGuid.NewGuid();
                    }
                    //Batch Insert
                    var inserted = _patientExtractRepository.BatchInsert(extractRecords);
                    if (!inserted)
                    {
                        Log.Error($"Extract {nameof(PatientExtract)} not Loaded");
                        return 0;
                    }
                    Log.Debug("saved batch");
                    skip = skip + take;
                    DomainEvents.Dispatch(
                        new ExtractActivityNotification(extractId, new DwhProgress(
                            nameof(PatientExtract),
                            nameof(ExtractStatus.Loading),
                            found, skip, 0, 0, 0)));
                }
                return count;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Extract {nameof(PatientExtract)} not Loaded");
                throw;
            }
        }
    }
}

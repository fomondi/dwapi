﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class PatientStatusLoader : IPatientStatusLoader
    {
        private readonly IPatientStatusExtractRepository _patientStatusExtractRepository;
        private readonly ITempPatientStatusExtractRepository _tempPatientStatusExtractRepository;

        public PatientStatusLoader(IPatientStatusExtractRepository patientStatusExtractRepository, ITempPatientStatusExtractRepository tempPatientStatusExtractRepository)
        {
            _patientStatusExtractRepository = patientStatusExtractRepository;
            _tempPatientStatusExtractRepository = tempPatientStatusExtractRepository;
        }

        public async Task<int> Load(Guid extractId, int found)
        {
            try
            {
                DomainEvents.Dispatch(
                    new ExtractActivityNotification(extractId, new DwhProgress(
                        nameof(PatientStatusExtract),
                        nameof(ExtractStatus.Loading),
                        found, 0, 0, 0, 0)));

                //load temp extracts without errors
                StringBuilder query = new StringBuilder();
                query.Append($" SELECT * FROM {nameof(TempPatientStatusExtract)}s s");
                query.Append($" INNER JOIN PatientExtracts p ON ");
                query.Append($" s.PatientPK = p.PatientPK AND ");
                query.Append($" s.SiteCode = p.SiteCode ");
                //query.Append($" WHERE s.CheckError = 0"); //load all the takaka

                var tempPatientStatusExtracts =_tempPatientStatusExtractRepository.GetFromSql(query.ToString());

                const int take = 1000;
                int skip = 0;
                var count = tempPatientStatusExtracts.Count();
                while (skip < count)
                {
                    var batch = tempPatientStatusExtracts.Skip(skip).Take(take).ToList();
                    //Auto mapper
                    var extractRecords = Mapper.Map<List<TempPatientStatusExtract>, List<PatientStatusExtract>>(batch);
                    foreach (var record in extractRecords)
                    {
                        record.Id = LiveGuid.NewGuid();
                    }
                    //Batch Insert
                    var inserted = _patientStatusExtractRepository.BatchInsert(extractRecords);
                    if (!inserted)
                    {
                        Log.Error($"Extract {nameof(PatientStatusExtract)} not Loaded");
                        return 0;
                    }
                    Log.Debug("saved batch");
                    skip = skip + take;
                    DomainEvents.Dispatch(
                        new ExtractActivityNotification(extractId, new DwhProgress(
                            nameof(PatientStatusExtract),
                            nameof(ExtractStatus.Loading),
                            found, skip, 0, 0, 0)));
                }
                return count;

            }
            catch (Exception e)
            {
                Log.Error(e, $"Extract {nameof(PatientStatusExtract)} not Loaded");
                return 0;
            }
        }
    }
}

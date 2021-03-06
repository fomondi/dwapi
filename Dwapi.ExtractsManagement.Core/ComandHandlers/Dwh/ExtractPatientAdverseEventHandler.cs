﻿using System.Threading;
using System.Threading.Tasks;
using Dwapi.ExtractsManagement.Core.Commands.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Extratcors.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Loaders.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Validators;
using Dwapi.ExtractsManagement.Core.Model.Destination.Dwh;
using Dwapi.ExtractsManagement.Core.Model.Source.Dwh;
using Dwapi.ExtractsManagement.Core.Notifications;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Events;
using Dwapi.SharedKernel.Model;
using MediatR;

namespace Dwapi.ExtractsManagement.Core.ComandHandlers.Dwh
{
    public class ExtractPatientAdverseEventHandler : IRequestHandler<ExtractPatientAdverseEvent, bool>
    {
        private readonly IPatientAdverseEventSourceExtractor _patientAdverseEventSourceExtractor;
        private readonly IExtractValidator _extractValidator;
        private readonly IPatientAdverseEventLoader _patientAdverseEventLoader;

        public ExtractPatientAdverseEventHandler(IPatientAdverseEventSourceExtractor patientAdverseEventSourceExtractor, IExtractValidator extractValidator, IPatientAdverseEventLoader patientAdverseEventLoader)
        {
            _patientAdverseEventSourceExtractor = patientAdverseEventSourceExtractor;
            _extractValidator = extractValidator;
            _patientAdverseEventLoader = patientAdverseEventLoader;
        }

        public async Task<bool> Handle(ExtractPatientAdverseEvent request, CancellationToken cancellationToken)
        {
            //Extract
            int found = await _patientAdverseEventSourceExtractor.Extract(request.Extract, request.DatabaseProtocol);

            //Validate
            await _extractValidator.Validate(request.Extract.Id, found, nameof(PatientAdverseEventExtract), $"{nameof(TempPatientAdverseEventExtract)}s");

            //Load
            int loaded = await _patientAdverseEventLoader.Load(request.Extract.Id, found);

            int rejected = found - loaded;

            //notify loaded
            DomainEvents.Dispatch(
                new ExtractActivityNotification(request.Extract.Id, new DwhProgress(
                    nameof(PatientAdverseEventExtract),
                    nameof(ExtractStatus.Loaded),
                    found, loaded, rejected, loaded, 0)));

            return true;
        }
    }
}
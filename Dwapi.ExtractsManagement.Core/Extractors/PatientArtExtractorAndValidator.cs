﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dwapi.Domain;
using Dwapi.ExtractsManagement.Core.Commands;
using Dwapi.ExtractsManagement.Core.ExtractValidators;
using Dwapi.SharedKernel.Model;

namespace Dwapi.ExtractsManagement.Core.Extractors
{
    public class PatientArtExtractorAndValidator : PatientArtExtractor, IExtractorValidator
    {
        private readonly IValidator _validator;
        private readonly ProgressHub _progressHub;
        
        //public PatientArtExtractorAndValidator(IExtractUnitOfWork unitOfWork, IValidator validator, ProgressHub progressHub) 
        //    : base(unitOfWork, progressHub)
        //{
        //    _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        //    _progressHub = progressHub ?? throw new ArgumentNullException(nameof(progressHub));
        //}

        public PatientArtExtractorAndValidator(IExtractUnitOfWork unitOfWork, ProgressHub progressHub)
            : base(unitOfWork, progressHub)
        {
            _validator = new GenericValidator(unitOfWork, nameof(TempPatientArtExtract));
        }

        public async Task ExtractAndValidateAsync(DwhExtract extract, DbProtocol dbProtocol)
        {
            await base.ExtractAsync(extract, dbProtocol);
            await _validator.Validate();
        }
    }
}

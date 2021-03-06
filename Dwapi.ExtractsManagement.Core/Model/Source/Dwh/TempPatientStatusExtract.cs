using System;

namespace Dwapi.ExtractsManagement.Core.Model.Source.Dwh
{
    
    public class TempPatientStatusExtract : TempExtract
    {
        public string FacilityName { get; set; }
        public string ExitDescription { get; set; }
        public DateTime? ExitDate { get; set; }
        public string ExitReason { get; set; }
    }
}

// Program: CAB_FDSO_CONCAT_EXCLUSION, ID: 372667656, model: 746.
// Short name: SWE02371
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_FDSO_CONCAT_EXCLUSION.
/// </para>
/// <para>
/// CAB FDSO EXCLUSION TYPE
/// This CAB will determine the FDSO exclusion codes required for transmission.
/// </para>
/// </summary>
[Serializable]
public partial class CabFdsoConcatExclusion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FDSO_CONCAT_EXCLUSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFdsoConcatExclusion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFdsoConcatExclusion.
  /// </summary>
  public CabFdsoConcatExclusion(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ********************************
    // Concat Attribute does not require a comma.
    // For example ADM,PAS,TAX
    // ********************************
    local.ExclusionFlag.Flag = "";

    if (!IsEmpty(import.FederalDebtSetoff.EtypeAdmBankrupt) || !
      IsEmpty(import.FederalDebtSetoff.EtypeAdministrativeOffset))
    {
      local.ExclusionFlag.Flag = "Y";
      export.FdsoCertificationTapeRecord.OffsetExclusionType = "ADM";
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypeFinancialInstitution))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "FIN";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",FIN"
          ;
      }
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypeFederalSalary))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "SAL";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",SAL"
          ;
      }
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypeFederalRetirement))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "RET";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",RET"
          ;
      }
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypePassportDenial))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "PAS";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",PAS"
          ;
      }
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypeTaxRefund))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "TAX";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",TAX"
          ;
      }
    }

    if (!IsEmpty(import.FederalDebtSetoff.EtypeVendorPaymentOrMisc))
    {
      if (IsEmpty(local.ExclusionFlag.Flag))
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType = "VEN";
      }
      else
      {
        export.FdsoCertificationTapeRecord.OffsetExclusionType =
          TrimEnd(export.FdsoCertificationTapeRecord.OffsetExclusionType) + ",VEN"
          ;
      }
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
    }

    private AdministrativeActCertification federalDebtSetoff;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FdsoCertificationTapeRecord.
    /// </summary>
    [JsonPropertyName("fdsoCertificationTapeRecord")]
    public FdsoCertificationTapeRecord FdsoCertificationTapeRecord
    {
      get => fdsoCertificationTapeRecord ??= new();
      set => fdsoCertificationTapeRecord = value;
    }

    private FdsoCertificationTapeRecord fdsoCertificationTapeRecord;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExclusionFlag.
    /// </summary>
    [JsonPropertyName("exclusionFlag")]
    public Common ExclusionFlag
    {
      get => exclusionFlag ??= new();
      set => exclusionFlag = value;
    }

    private Common exclusionFlag;
  }
#endregion
}

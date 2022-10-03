// Program: CAB_PROCESS_COURT_ORDER_INFO, ID: 374394639, model: 746.
// Short name: SWE00135
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_PROCESS_COURT_ORDER_INFO.
/// </summary>
[Serializable]
public partial class CabProcessCourtOrderInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_PROCESS_COURT_ORDER_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabProcessCourtOrderInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabProcessCourtOrderInfo.
  /// </summary>
  public CabProcessCourtOrderInfo(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    switch(TrimEnd(import.HeaderRecord.ActionCode))
    {
      case "NCO":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import
          .CourtOrderRecord.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .CourtOrderRecord.CountyId + import.CourtOrderRecord.CityIndicator + import
          .CourtOrderRecord.ModificationDate + import.CourtOrderRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write NCP Debt Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.NcpDebtRecord.RecordType + import.NcpDebtRecord.CourtDebtId + import
          .NcpDebtRecord.DebtType + import.NcpDebtRecord.FeeClass + import
          .NcpDebtRecord.OverrideFeePercent + import
          .NcpDebtRecord.DebtFeeExemption + import.NcpDebtRecord.IntersateId + import
          .NcpDebtRecord.KessepMultiplePayerIndicator + import
          .NcpDebtRecord.CountyMultiplePayorIndicator + import
          .NcpDebtRecord.KpcDebtId + import.NcpDebtRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Obligation Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.ObligationRecord.RecordType + import
          .ObligationRecord.Amount + import.ObligationRecord.Frequency + import
          .ObligationRecord.StartDate + import.ObligationRecord.EndDate + import
          .ObligationRecord.SeasonalFlag + import.ObligationRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write NCP Participant Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(import.NcpParticipantRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            import.NcpParticipantRecord.RecordType + import
            .NcpParticipantRecord.Role + import.NcpParticipantRecord.Type1 + import
            .NcpParticipantRecord.Ssn + import.NcpParticipantRecord.LastName + import
            .NcpParticipantRecord.FirstName + import
            .NcpParticipantRecord.MiddleInitial + import
            .NcpParticipantRecord.Suffix + import
            .NcpParticipantRecord.Gender + import
            .NcpParticipantRecord.DateOfBirth + import
            .NcpParticipantRecord.SrsPersonNumber + import
            .NcpParticipantRecord.FamilyViolenceIndicator + import
            .NcpParticipantRecord.Pin + import.NcpParticipantRecord.Source + import
            .NcpParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
        else
        {
          local.PrintFileRecord.CourtOrderLine =
            import.NcpThirdPartyRecord.RecordType + import
            .NcpThirdPartyRecord.Role + import.NcpThirdPartyRecord.Type1 + import
            .NcpThirdPartyRecord.SrsPersonNumber + import
            .NcpThirdPartyRecord.AgencyName + import.NcpThirdPartyRecord.Pin + import
            .NcpThirdPartyRecord.Source + import.NcpThirdPartyRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ******************************************************
        // Write NCP Address Records to Output File
        // ******************************************************
        if (!IsEmpty(import.NcpAddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            import.NcpAddressRecord.RecordType + import
            .NcpAddressRecord.Street + import.NcpAddressRecord.Street2 + import
            .NcpAddressRecord.City + import.NcpAddressRecord.State + import
            .NcpAddressRecord.PostalCode + import.NcpAddressRecord.Country + import
            .NcpAddressRecord.PhoneNumber + import.NcpAddressRecord.Province + import
            .NcpAddressRecord.Source + import.NcpAddressRecord.Type1 + import
            .NcpAddressRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ******************************************************
        // Write CP Participant Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(import.CpParticipantRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            import.CpParticipantRecord.RecordType + import
            .CpParticipantRecord.Role + import.CpParticipantRecord.Type1 + import
            .CpParticipantRecord.Ssn + import.CpParticipantRecord.LastName + import
            .CpParticipantRecord.FirstName + import
            .CpParticipantRecord.MiddleInitial + import
            .CpParticipantRecord.Suffix + import.CpParticipantRecord.Gender + import
            .CpParticipantRecord.DateOfBirth + import
            .CpParticipantRecord.SrsPersonNumber + import
            .CpParticipantRecord.FamilyViolenceIndicator + import
            .CpParticipantRecord.Pin + import.CpParticipantRecord.Source + import
            .CpParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
        else
        {
          local.PrintFileRecord.CourtOrderLine =
            import.CpThirdPartyRecord.RecordType + import
            .CpThirdPartyRecord.Role + import.CpThirdPartyRecord.Type1 + import
            .CpThirdPartyRecord.SrsPersonNumber + import
            .CpThirdPartyRecord.AgencyName + import.CpThirdPartyRecord.Pin + import
            .CpThirdPartyRecord.Source + import.CpThirdPartyRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ******************************************************
        // Write CP Address Records to Output File
        // ******************************************************
        if (!IsEmpty(import.CpAddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            import.CpAddressRecord.RecordType + import
            .CpAddressRecord.Street + import.CpAddressRecord.Street2 + import
            .CpAddressRecord.City + import.CpAddressRecord.State + import
            .CpAddressRecord.PostalCode + import.CpAddressRecord.Country + import
            .CpAddressRecord.PhoneNumber + import.CpAddressRecord.Province + import
            .CpAddressRecord.Source + import.CpAddressRecord.Type1 + import
            .CpAddressRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        if (!Equal(import.CpThirdPartyRecord.AgencyName, "SRS"))
        {
          // ******************************************************
          // Write Third Party Participant Record to Output File
          // ******************************************************
          local.ThirdPartyRecord.RecordType = "5";
          local.ThirdPartyRecord.Role = "TPP";
          local.ThirdPartyRecord.Type1 = "A";
          local.ThirdPartyRecord.SrsPersonNumber = "";
          local.ThirdPartyRecord.AgencyName = "SRS";
          local.ThirdPartyRecord.Source = "SRS";
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            local.ThirdPartyRecord.RecordType + local.ThirdPartyRecord.Role + local
            .ThirdPartyRecord.Type1 + local.ThirdPartyRecord.SrsPersonNumber + local
            .ThirdPartyRecord.AgencyName + local.ThirdPartyRecord.Pin + local
            .ThirdPartyRecord.Source + local.ThirdPartyRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
          }
        }

        break;
      case "UCO":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Court Order Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.CourtOrderRecord.RecordType + import
          .CourtOrderRecord.CourtOrderNumber + import
          .CourtOrderRecord.CourtOrderType + import.CourtOrderRecord.FfpFlag + import
          .CourtOrderRecord.StartDate + import.CourtOrderRecord.EndDate + import
          .CourtOrderRecord.CountyId + import.CourtOrderRecord.CityIndicator + import
          .CourtOrderRecord.ModificationDate;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
        }

        break;
      case "UADD":
        local.Send.Parm1 = "GR";

        // ******************************************************
        // Write Header Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          import.HeaderRecord.RecordType + import.HeaderRecord.ActionCode + import
          .HeaderRecord.TransactionDate + import.HeaderRecord.Userid + import
          .HeaderRecord.Timestamp + import.HeaderRecord.Filler;
        UseFnExtWriteInterfaceFile();

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }

        // ******************************************************
        // Write Participant Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(import.NcpParticipantRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            import.NcpParticipantRecord.RecordType + import
            .NcpParticipantRecord.Role + import.NcpParticipantRecord.Type1 + import
            .NcpParticipantRecord.Ssn + import.NcpParticipantRecord.LastName + import
            .NcpParticipantRecord.FirstName + import
            .NcpParticipantRecord.MiddleInitial + import
            .NcpParticipantRecord.Suffix + import
            .NcpParticipantRecord.Gender + import
            .NcpParticipantRecord.DateOfBirth + import
            .NcpParticipantRecord.SrsPersonNumber + import
            .NcpParticipantRecord.FamilyViolenceIndicator + import
            .NcpParticipantRecord.Pin + import.NcpParticipantRecord.Source + import
            .NcpParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
        else
        {
          local.PrintFileRecord.CourtOrderLine =
            import.NcpThirdPartyRecord.RecordType + import
            .NcpThirdPartyRecord.Role + import.NcpThirdPartyRecord.Type1 + import
            .NcpThirdPartyRecord.SrsPersonNumber + import
            .NcpThirdPartyRecord.AgencyName + import.NcpThirdPartyRecord.Pin + import
            .NcpThirdPartyRecord.Source + import.NcpThirdPartyRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ******************************************************
        // Write Particpant Address Records to Output File
        // ******************************************************
        if (!IsEmpty(import.NcpAddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            import.NcpAddressRecord.RecordType + import
            .NcpAddressRecord.Street + import.NcpAddressRecord.Street2 + import
            .NcpAddressRecord.City + import.NcpAddressRecord.State + import
            .NcpAddressRecord.PostalCode + import.NcpAddressRecord.Country + import
            .NcpAddressRecord.PhoneNumber + import.NcpAddressRecord.Province + import
            .NcpAddressRecord.Source + import.NcpAddressRecord.Type1 + import
            .NcpAddressRecord.Filler;
          UseFnExtWriteInterfaceFile();

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    MoveKpcExternalParms(local.Send, useImport.KpcExternalParms);
    useImport.FileCount.Count = import.FileCount.Count;
    useImport.PrintFileRecord.CourtOrderLine =
      local.PrintFileRecord.CourtOrderLine;
    MoveKpcExternalParms(local.Return1, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.Return1);
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
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of CourtOrderRecord.
    /// </summary>
    [JsonPropertyName("courtOrderRecord")]
    public CourtOrderRecord CourtOrderRecord
    {
      get => courtOrderRecord ??= new();
      set => courtOrderRecord = value;
    }

    /// <summary>
    /// A value of NcpDebtRecord.
    /// </summary>
    [JsonPropertyName("ncpDebtRecord")]
    public NcpDebtRecord NcpDebtRecord
    {
      get => ncpDebtRecord ??= new();
      set => ncpDebtRecord = value;
    }

    /// <summary>
    /// A value of ObligationRecord.
    /// </summary>
    [JsonPropertyName("obligationRecord")]
    public ObligationRecord ObligationRecord
    {
      get => obligationRecord ??= new();
      set => obligationRecord = value;
    }

    /// <summary>
    /// A value of NcpParticipantRecord.
    /// </summary>
    [JsonPropertyName("ncpParticipantRecord")]
    public ParticipantRecord NcpParticipantRecord
    {
      get => ncpParticipantRecord ??= new();
      set => ncpParticipantRecord = value;
    }

    /// <summary>
    /// A value of NcpThirdPartyRecord.
    /// </summary>
    [JsonPropertyName("ncpThirdPartyRecord")]
    public ThirdPartyRecord NcpThirdPartyRecord
    {
      get => ncpThirdPartyRecord ??= new();
      set => ncpThirdPartyRecord = value;
    }

    /// <summary>
    /// A value of NcpAddressRecord.
    /// </summary>
    [JsonPropertyName("ncpAddressRecord")]
    public AddressRecord NcpAddressRecord
    {
      get => ncpAddressRecord ??= new();
      set => ncpAddressRecord = value;
    }

    /// <summary>
    /// A value of CpParticipantRecord.
    /// </summary>
    [JsonPropertyName("cpParticipantRecord")]
    public ParticipantRecord CpParticipantRecord
    {
      get => cpParticipantRecord ??= new();
      set => cpParticipantRecord = value;
    }

    /// <summary>
    /// A value of CpThirdPartyRecord.
    /// </summary>
    [JsonPropertyName("cpThirdPartyRecord")]
    public ThirdPartyRecord CpThirdPartyRecord
    {
      get => cpThirdPartyRecord ??= new();
      set => cpThirdPartyRecord = value;
    }

    /// <summary>
    /// A value of CpAddressRecord.
    /// </summary>
    [JsonPropertyName("cpAddressRecord")]
    public AddressRecord CpAddressRecord
    {
      get => cpAddressRecord ??= new();
      set => cpAddressRecord = value;
    }

    private Common fileCount;
    private HeaderRecord headerRecord;
    private CourtOrderRecord courtOrderRecord;
    private NcpDebtRecord ncpDebtRecord;
    private ObligationRecord obligationRecord;
    private ParticipantRecord ncpParticipantRecord;
    private ThirdPartyRecord ncpThirdPartyRecord;
    private AddressRecord ncpAddressRecord;
    private ParticipantRecord cpParticipantRecord;
    private ThirdPartyRecord cpThirdPartyRecord;
    private AddressRecord cpAddressRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public KpcExternalParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public KpcExternalParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of ThirdPartyRecord.
    /// </summary>
    [JsonPropertyName("thirdPartyRecord")]
    public ThirdPartyRecord ThirdPartyRecord
    {
      get => thirdPartyRecord ??= new();
      set => thirdPartyRecord = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    private KpcExternalParms send;
    private KpcExternalParms return1;
    private ThirdPartyRecord thirdPartyRecord;
    private PrintFileRecord printFileRecord;
  }
#endregion
}

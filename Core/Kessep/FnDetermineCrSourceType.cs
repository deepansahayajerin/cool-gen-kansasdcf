// Program: FN_DETERMINE_CR_SOURCE_TYPE, ID: 372405561, model: 746.
// Short name: SWE02417
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_CR_SOURCE_TYPE.
/// </summary>
[Serializable]
public partial class FnDetermineCrSourceType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_CR_SOURCE_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineCrSourceType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineCrSourceType.
  /// </summary>
  public FnDetermineCrSourceType(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // The Cash Receipt Check Date comes from the Effective Date unless the Cash
    // Receipt Source Type is FDSO or a Kansas Court.
    export.ForCheckDate.CheckDate =
      import.ElectronicFundTransmission.EffectiveEntryDate;

    // Determine the cash receipt source type.
    switch(TrimEnd(import.ElectronicFundTransmission.
      CompanyEntryDescription ?? ""))
    {
      case "EMPLOYER":
        // The cash receipt source type is EMP.
        export.CashReceiptSourceType.SystemGeneratedIdentifier =
          import.HardcodedEmployer.SystemGeneratedIdentifier;

        return;
      case "FDSO":
        // The cash receipt source type is FDSO.
        export.CashReceiptSourceType.SystemGeneratedIdentifier =
          import.HardcodedFdso.SystemGeneratedIdentifier;
        export.ForCheckDate.CheckDate =
          import.ElectronicFundTransmission.CompanyDescriptiveDate;

        return;
      case "INDIVIDUAL":
        // The cash receipt source type is MISC.
        export.CashReceiptSourceType.SystemGeneratedIdentifier =
          import.HardcodedMisc.SystemGeneratedIdentifier;

        return;
      default:
        break;
    }

    // The cash receipt source type may be a state or court.
    local.NumericInd.Count =
      Verify(Substring(
        import.ElectronicFundTransmission.CompanyEntryDescription, 10, 3, 2),
      import.NumericString.Text10);

    if (local.NumericInd.Count == 0)
    {
      // A state FIPS code was found.
      export.CashReceiptSourceType.State =
        (int?)StringToNumber(Substring(
          import.ElectronicFundTransmission.CompanyEntryDescription, 10, 3, 2));
        

      if (export.CashReceiptSourceType.State.GetValueOrDefault() == 20)
      {
        export.ForCheckDate.CheckDate =
          import.ElectronicFundTransmission.CompanyDescriptiveDate;

        // This is a Kansas FIPS code so this must be a court interface EFT.
        local.NumericInd.Count =
          Verify(Substring(
            import.ElectronicFundTransmission.CompanyEntryDescription, 10, 5,
          5), import.NumericString.Text10);

        if (local.NumericInd.Count == 0)
        {
          // Read the Cash Receipt Source Type by state, county and location 
          // fips codes.
          export.CashReceiptSourceType.County =
            (int?)StringToNumber(Substring(
              import.ElectronicFundTransmission.CompanyEntryDescription, 10, 5,
            3));
          export.CashReceiptSourceType.Location =
            (int?)StringToNumber(Substring(
              import.ElectronicFundTransmission.CompanyEntryDescription, 10, 8,
            2));
        }
        else
        {
          // We cannot determine the source of this EFT so we will put it in a 
          // PEND status and write out an error msg.
          export.EabReportSend.RptDetail = "ID " + NumberToString
            (import.ElectronicFundTransmission.TransmissionIdentifier, 7, 9) + " Pending Error: Company Entry Desciption has a non-numeric County/Location FIPS of " +
            Substring
            (import.ElectronicFundTransmission.CompanyEntryDescription, 10, 5, 5);
            
        }
      }
      else
      {
        // This is not a Kansas FIPS code so this is probably an interstate EFT.
        // Read the Cash Receipt Source Type by code (state abbrev) and state
        // number (2 char fips code).
        export.CashReceiptSourceType.Code =
          Substring(import.ElectronicFundTransmission.CompanyEntryDescription,
          10, 1, 2) + " STATE  ";
      }
    }
    else
    {
      // We cannot determine the source of this EFT so we will put it in a PEND 
      // status and write out an error msg.
      export.EabReportSend.RptDetail = "ID " + NumberToString
        (import.ElectronicFundTransmission.TransmissionIdentifier, 7, 9) + " Pending Error: Company Entry Description has a non-numeric State FIPS of " +
        Substring
        (import.ElectronicFundTransmission.CompanyEntryDescription, 10, 3, 2);
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of HardcodedEmployer.
    /// </summary>
    [JsonPropertyName("hardcodedEmployer")]
    public CashReceiptSourceType HardcodedEmployer
    {
      get => hardcodedEmployer ??= new();
      set => hardcodedEmployer = value;
    }

    /// <summary>
    /// A value of HardcodedFdso.
    /// </summary>
    [JsonPropertyName("hardcodedFdso")]
    public CashReceiptSourceType HardcodedFdso
    {
      get => hardcodedFdso ??= new();
      set => hardcodedFdso = value;
    }

    /// <summary>
    /// A value of HardcodedMisc.
    /// </summary>
    [JsonPropertyName("hardcodedMisc")]
    public CashReceiptSourceType HardcodedMisc
    {
      get => hardcodedMisc ??= new();
      set => hardcodedMisc = value;
    }

    /// <summary>
    /// A value of NumericString.
    /// </summary>
    [JsonPropertyName("numericString")]
    public WorkArea NumericString
    {
      get => numericString ??= new();
      set => numericString = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CashReceiptSourceType hardcodedEmployer;
    private CashReceiptSourceType hardcodedFdso;
    private CashReceiptSourceType hardcodedMisc;
    private WorkArea numericString;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ForCheckDate.
    /// </summary>
    [JsonPropertyName("forCheckDate")]
    public CashReceipt ForCheckDate
    {
      get => forCheckDate ??= new();
      set => forCheckDate = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt forCheckDate;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumericInd.
    /// </summary>
    [JsonPropertyName("numericInd")]
    public Common NumericInd
    {
      get => numericInd ??= new();
      set => numericInd = value;
    }

    private Common numericInd;
  }
#endregion
}

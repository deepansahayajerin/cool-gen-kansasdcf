// Program: OE_B491_VALIDATE_HINS_COMPANY, ID: 371175448, model: 746.
// Short name: SWE02489
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B491_VALIDATE_HINS_COMPANY.
/// </summary>
[Serializable]
public partial class OeB491ValidateHinsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B491_VALIDATE_HINS_COMPANY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB491ValidateHinsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB491ValidateHinsCompany.
  /// </summary>
  public OeB491ValidateHinsCompany(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************
    // * Errors are reported to Report 01 -- Report all errors.      *
    // ***************************************************************
    export.CompanyInfoValid.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";

    if (Lt(import.HealthInsuranceCompany.CarrierCode, "0000001") || Lt
      ("9999999", import.HealthInsuranceCompany.CarrierCode))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier code is either not greater than zero or not numeric.";
        
      UseCabBusinessReport01();
    }

    if (IsEmpty(import.HealthInsuranceCompany.InsurancePolicyCarrier))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier name is missing.";
        
      UseCabBusinessReport01();
    }

    if (Lt(import.HealthInsuranceCompanyAddress.ZipCode5, "00001") || Lt
      ("99999", import.HealthInsuranceCompanyAddress.ZipCode5))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier zip code is invalid";
        
      UseCabBusinessReport01();
    }

    if (IsEmpty(import.HealthInsuranceCompanyAddress.Street1))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier street1 is missing.";
        
      UseCabBusinessReport01();
    }

    if (IsEmpty(import.HealthInsuranceCompanyAddress.City))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier city is missing.";
        
      UseCabBusinessReport01();
    }

    if (!IsEmpty(import.HealthInsuranceCompanyAddress.State) && !
      Equal(import.HealthInsuranceCompanyAddress.State, "NK"))
    {
      if (!ReadCodeValue())
      {
        export.CompanyInfoValid.Flag = "N";
        local.NeededToWrite.RptDetail =
          (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
            import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier state code is invalid: " +
          (import.HealthInsuranceCompanyAddress.State ?? "");
        UseCabBusinessReport01();
      }
    }
    else
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail =
        (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + (
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "") + " Carrier state code is missing.";
        
      UseCabBusinessReport01();
    }
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.State.Id);
        db.SetNullableString(
          command, "state", import.HealthInsuranceCompanyAddress.State ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Code state;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CompanyInfoValid.
    /// </summary>
    [JsonPropertyName("companyInfoValid")]
    public Common CompanyInfoValid
    {
      get => companyInfoValid ??= new();
      set => companyInfoValid = value;
    }

    private Common companyInfoValid;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private CodeValue codeValue;
    private Code code;
  }
#endregion
}

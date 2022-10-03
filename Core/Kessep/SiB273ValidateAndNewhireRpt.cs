// Program: SI_B273_VALIDATE_AND_NEWHIRE_RPT, ID: 371060849, model: 746.
// Short name: SWE01279
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_VALIDATE_AND_NEWHIRE_RPT.
/// </summary>
[Serializable]
public partial class SiB273ValidateAndNewhireRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_VALIDATE_AND_NEWHIRE_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273ValidateAndNewhireRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273ValidateAndNewhireRpt.
  /// </summary>
  public SiB273ValidateAndNewhireRpt(IContext context, Import import,
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
    export.Valid.Flag = "N";
    local.Code.Id = 169;
    export.EabFileHandling.Status = "OK";

    if (AsChar(export.Valid.Flag) == 'N')
    {
      if (IsEmpty(import.EmployerAddress.ZipCode))
      {
        goto Test;
      }

      if (IsEmpty(import.Employer.Ein))
      {
        goto Test;
      }

      if (IsEmpty(import.Employer.Name))
      {
        goto Test;
      }

      if (IsEmpty(import.EmployerAddress.Street1) && IsEmpty
        (import.EmployerAddress.Street2))
      {
        // ****************************************************************
        // If street1 is spaces and street2 is populated, an action block
        // called later on, will move street2 to street1. PR150264
        // ****************************************************************
        goto Test;
      }

      if (!IsEmpty(import.EmployerAddress.State) && !
        Equal(import.EmployerAddress.State, "NK"))
      {
        if (ReadCodeValue())
        {
          export.Valid.Flag = "Y";

          return;
        }
      }
    }

Test:

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PERSON: " + import
      .CsePersonsWorkSet.Number;
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
      TrimEnd(import.CsePersonsWorkSet.LastName) + ", " + TrimEnd
      (import.CsePersonsWorkSet.FirstName) + " " + import
      .CsePersonsWorkSet.MiddleInitial;
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "EIN: " + (import.Employer.Ein ?? "");
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "EMPLOYER NAME AND ADDRESS:";
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "      " + (import.Employer.Name ?? "");
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "      " + (
      import.EmployerAddress.Street1 ?? "");
    UseCabBusinessReport01();

    if (!IsEmpty(import.EmployerAddress.Street2))
    {
      local.EabReportSend.RptDetail = "      " + (
        import.EmployerAddress.Street2 ?? "");
      UseCabBusinessReport01();
    }

    if (!IsEmpty(import.EmployerAddress.Street3))
    {
      local.EabReportSend.RptDetail = "      " + (
        import.EmployerAddress.Street3 ?? "");
      UseCabBusinessReport01();
    }

    if (!IsEmpty(import.EmployerAddress.Street4))
    {
      local.EabReportSend.RptDetail = "      " + (
        import.EmployerAddress.Street4 ?? "");
      UseCabBusinessReport01();
    }

    local.EabReportSend.RptDetail = "      " + TrimEnd
      (import.EmployerAddress.City) + ", " + (
        import.EmployerAddress.State ?? "");

    if (!IsEmpty(import.EmployerAddress.Zip4))
    {
      local.EabReportSend.RptDetail = "      " + TrimEnd
        (import.EmployerAddress.City) + ", " + (
          import.EmployerAddress.State ?? "") + " " + (
          import.EmployerAddress.ZipCode ?? "") + "-" + (
          import.EmployerAddress.Zip4 ?? "");
    }
    else if (!IsEmpty(import.EmployerAddress.ZipCode))
    {
      local.EabReportSend.RptDetail = "      " + TrimEnd
        (import.EmployerAddress.City) + ", " + (
          import.EmployerAddress.State ?? "") + " " + (
          import.EmployerAddress.ZipCode ?? "") + "" + "";
    }
    else
    {
      local.EabReportSend.RptDetail = "      " + TrimEnd
        (import.EmployerAddress.City) + ", " + (
          import.EmployerAddress.State ?? "");
    }

    UseCabBusinessReport01();

    if (!IsEmpty(import.EmployerAddress.Zip3))
    {
      local.EabReportSend.RptDetail = "ZIP3: " + (
        import.EmployerAddress.Zip3 ?? "");
      UseCabBusinessReport01();
    }

    // *************************  Foreign Address *******************
    if (!IsEmpty(import.EmployerAddress.Province))
    {
      local.EabReportSend.RptDetail = "PROVINCE: " + (
        import.EmployerAddress.Province ?? "");
      UseCabBusinessReport01();
    }

    if (!IsEmpty(import.EmployerAddress.PostalCode))
    {
      local.EabReportSend.RptDetail = "POSTAL CODE: " + (
        import.EmployerAddress.PostalCode ?? "");
      UseCabBusinessReport01();
    }

    if (!IsEmpty(import.EmployerAddress.Country))
    {
      local.EabReportSend.RptDetail = "COUNTRY: " + (
        import.EmployerAddress.Country ?? "");
      UseCabBusinessReport01();
    }

    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport01();
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport01();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", local.Code.Id);
        db.SetDate(
          command, "expirationDate", import.Process.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "state", import.EmployerAddress.State ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 3);
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private DateWorkArea process;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Employer employer;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
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

    private Common valid;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private Code code;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private Code code;
    private CodeValue codeValue;
  }
#endregion
}

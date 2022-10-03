// Program: FN_B621_GET_REFERRAL, ID: 373454872, model: 746.
// Short name: SWE00150
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B621_GET_REFERRAL.
/// </summary>
[Serializable]
public partial class FnB621GetReferral: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B621_GET_REFERRAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB621GetReferral(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB621GetReferral.
  /// </summary>
  public FnB621GetReferral(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 01-02-2008  SWCOAMX - Arun     Changed the read properties ( CQ#619 )
    // 10-13-2009  GVandy   CQ13338   Correct logic determining if a court order
    // has an ENF legal referral.
    // : Get the referral.  First read using court case number and a reason code
    // of 'ENF'.  If that does not provide results, try with court case number
    // and a reason code of 'CV'.  If still no results, read from legal action
    // through case role and case.
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Found.Flag = "N";

    // :Read Legal Referral qualifying on court case number and reason code '
    // ENF'.
    // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
    // Uncommitted Browse **
    if (ReadLegalReferralAssignment2())
    {
      local.Found.Flag = "Y";
    }
    else
    {
      // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
      // Uncommitted Browse **
      if (ReadLegalReferralAssignment1())
      {
        local.Found.Flag = "Y";
      }
    }

    if (AsChar(local.Found.Flag) == 'N')
    {
      // :Read from Legal Action through Legal Action Case Role, Case Role, and 
      // Case.
      // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
      // Uncommitted Browse **
      // -- 10/13/2009  GVandy CQ13338 Removed check for legal_action 
      // action_taken = 'JEF' and added check for legal_action classification =
      // 'J'.
      if (ReadLegalReferralAssignment3())
      {
        local.Found.Flag = "Y";
      }
    }

    if (AsChar(local.Found.Flag) == 'Y')
    {
      // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
      // Uncommitted Browse **
      if (ReadOfficeServiceProviderServiceProviderOffice())
      {
        export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        export.OfficeServiceProvider.RoleCode =
          entities.OfficeServiceProvider.RoleCode;
        MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);

        if (Equal(entities.OfficeServiceProvider.RoleCode, "CT"))
        {
          // : Contractor - Set to type B for sort order.
          export.ReferralType.Text2 = "B";
        }
        else
        {
          // : State of Kansas employee - Set to type A for sort order.
          export.ReferralType.Text2 = "A";
        }
      }
      else
      {
        // <===========Write error message to error report ========>
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Office Service Provider NF: " + (
          import.LegalAction.StandardNumber ?? "");
        UseCabErrorReport();
      }
    }
    else
    {
      // : No Referral - Set to type C for sort order.
      export.ReferralType.Text2 = "C";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadLegalReferralAssignment1()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment1",
      (db, command) =>
      {
        db.
          SetDate(command, "effectiveDate", import.Eom.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment2()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment2",
      (db, command) =>
      {
        db.
          SetDate(command, "effectiveDate", import.Eom.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment3()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment3",
      (db, command) =>
      {
        db.
          SetDate(command, "effectiveDate", import.Eom.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private LegalAction legalAction;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ReferralType.
    /// </summary>
    [JsonPropertyName("referralType")]
    public TextWorkArea ReferralType
    {
      get => referralType ??= new();
      set => referralType = value;
    }

    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private TextWorkArea referralType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of ReferralType.
    /// </summary>
    [JsonPropertyName("referralType")]
    public TextWorkArea ReferralType
    {
      get => referralType ??= new();
      set => referralType = value;
    }

    private DateWorkArea max;
    private Common found;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private TextWorkArea referralType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private LegalActionCaseRole legalActionCaseRole;
    private Office office;
    private ServiceProvider serviceProvider;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ap;
    private LegalActionPerson legalActionPerson;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}

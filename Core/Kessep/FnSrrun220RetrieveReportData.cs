// Program: FN_SRRUN220_RETRIEVE_REPORT_DATA, ID: 372799579, model: 746.
// Short name: SWE02578
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SRRUN220_RETRIEVE_REPORT_DATA.
/// </summary>
[Serializable]
public partial class FnSrrun220RetrieveReportData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SRRUN220_RETRIEVE_REPORT_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSrrun220RetrieveReportData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSrrun220RetrieveReportData.
  /// </summary>
  public FnSrrun220RetrieveReportData(IContext context, Import import,
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
    local.EabFileHandling.Action = "WRITE";
    local.Current.Date = Now().Date;
    local.CurrentTimestamp.Timestamp = Now();
    local.CanamSend.Parm2 = "";
    local.SuppressReason.Text30 =
      import.StmtCouponSuppStatusHist.ReasonText ?? Spaces(30);
    MoveStmtCouponSuppStatusHist(import.StmtCouponSuppStatusHist,
      local.StmtCouponSuppStatusHist);

    if (Lt(import.StmtCouponSuppStatusHist.CreatedTmst,
      import.StmtCouponSuppStatusHist.LastUpdatedTmst))
    {
      local.StmtCouponSuppStatusHist.CreatedBy =
        import.StmtCouponSuppStatusHist.LastUpdatedBy ?? Spaces(8);
    }

    if (AsChar(import.StmtCouponSuppStatusHist.Type1) == 'O')
    {
      local.SuppressType.Text10 = "Obligation";
    }
    else
    {
      local.SuppressType.Text10 = "Obligor";
    }

    if (!ReadLegalAction())
    {
      // ** OK **
    }

    if (!ReadObligationType())
    {
      ExitState = "OBLIGATION_TYPE_NF";

      return;
    }

    if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
    {
      // : This obligation has supported person(s).  We need to go through
      //   case unit and case to get the service provider.
      // : Set the debt type qualifer for 'A'ccrual instructions for an
      //   accruing obligation, and 'D'ebt otherwise.
      if (AsChar(entities.ObligationType.Classification) == 'A')
      {
        // : Obligation is accruing - set debt type to 'accrual instructions'.
        local.Debt.DebtType = "A";
      }
      else
      {
        // : Obligation is not accruing - set debt type to 'debt'
        local.Debt.DebtType = "D";
      }

      local.CaseFound.Flag = "N";

      foreach(var item in ReadCaseCsePerson())
      {
        local.CaseFound.Flag = "Y";

        if (Equal(entities.SupportedCase.Number, local.PrevSupportedCase.Number) ||
          Equal
          (entities.SupportedCsePerson.Number,
          local.PrevSupportedCsePerson.Number))
        {
          // : Already found service provider for this supported person,
          //   so get the next row.
          continue;
        }
        else
        {
          local.PrevSupportedCsePerson.Number =
            entities.SupportedCsePerson.Number;
          local.PrevSupportedCase.Number = entities.SupportedCase.Number;
          local.Found.Flag = "N";

          // : Get the service provider related to the case assignment with the
          //   most recent discontinue date.
          if (ReadCaseAssignmentOfficeServiceProviderServiceProvider())
          {
            local.Found.Flag = "Y";
          }

          if (AsChar(local.Found.Flag) != 'Y')
          {
            local.NeededToWrite.RptDetail =
              "Service Provider not found for case:  " + entities
              .SupportedCase.Number + "   Obligor:  " + import
              .PersistentCsePerson.Number + "   Obligation:  " + NumberToString
              (import.PersistentObligation.SystemGeneratedIdentifier, 15) + "   Supported Person:  " +
              entities.SupportedCsePerson.Number;
            UseCabErrorReport();

            continue;
          }

          if (ReadServiceProvider2())
          {
            MoveServiceProvider(entities.SupervisorServiceProvider,
              local.Supervisor);
          }
          else
          {
            local.Supervisor.LastName = "**Not Found**";
            local.NeededToWrite.RptDetail = "Supervisor nf for S.P.: " + entities
              .ServiceProvider.UserId + "  AP: " + import
              .PersistentCsePerson.Number + "   Oblig.: " + NumberToString
              (import.PersistentObligation.SystemGeneratedIdentifier, 15) + "   Supp. Person:  " +
              entities.SupportedCsePerson.Number + " Case: " + entities
              .SupportedCase.Number;
            UseCabErrorReport();
          }

          local.CanamSend.Parm1 = "GR";
          UseFnEabGenerateRpt220File();
        }
      }

      if (AsChar(local.CaseFound.Flag) != 'Y')
      {
        local.NeededToWrite.RptDetail = "Case nf for AP: " + import
          .PersistentCsePerson.Number + "  Supported Person: " + entities
          .SupportedCsePerson.Number + "   Oblig.: " + NumberToString
          (import.PersistentObligation.SystemGeneratedIdentifier, 15);
        UseCabErrorReport();
      }
    }
    else
    {
      // : No supported persons on the debt - use obligation assignment.
      local.Found.Flag = "N";
      local.OspFound.Flag = "N";

      if (ReadObligationAssignment())
      {
        local.Found.Flag = "Y";
      }

      if (AsChar(local.Found.Flag) == 'Y')
      {
        if (ReadOfficeServiceProvider())
        {
          local.OspFound.Flag = "Y";
        }
      }
      else
      {
        ExitState = "OBLIGATION_ASSIGNMENT_NF";

        return;
      }

      if (AsChar(local.OspFound.Flag) == 'Y')
      {
        if (!ReadServiceProvider1())
        {
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }
      }
      else
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      if (!ReadOffice())
      {
        ExitState = "FN0000_OFFICE_NF";

        return;
      }

      if (ReadServiceProvider2())
      {
        MoveServiceProvider(entities.SupervisorServiceProvider, local.Supervisor);
          
      }
      else
      {
        local.Supervisor.LastName = "*Not Found*";
        local.NeededToWrite.RptDetail = "Supervisor nf for S.P.: " + entities
          .ServiceProvider.UserId + "  AP: " + import
          .PersistentCsePerson.Number + "   Oblig.: " + NumberToString
          (import.PersistentObligation.SystemGeneratedIdentifier, 15);
        UseCabErrorReport();
      }

      local.CanamSend.Parm1 = "GR";
      UseFnEabGenerateRpt220File();
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private static void MoveStmtCouponSuppStatusHist(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnEabGenerateRpt220File()
  {
    var useImport = new FnEabGenerateRpt220File.Import();
    var useExport = new FnEabGenerateRpt220File.Export();

    useImport.ServiceProvider.Assign(entities.ServiceProvider);
    useImport.Office.Name = entities.Office.Name;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useImport.Supervisor.Assign(local.Supervisor);
    MoveStmtCouponSuppStatusHist(local.StmtCouponSuppStatusHist,
      useImport.StmtCouponSuppStatusHist);
    useImport.SuppressReason.Text30 = local.SuppressReason.Text30;
    useImport.SuppressType.Text10 = local.SuppressType.Text10;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabGenerateRpt220File.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private bool ReadCaseAssignmentOfficeServiceProviderServiceProvider()
  {
    entities.CaseAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNo", entities.SupportedCase.Number);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.ServiceProvider.LastName = db.GetString(reader, 11);
        entities.ServiceProvider.FirstName = db.GetString(reader, 12);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 13);
        entities.Office.Name = db.GetString(reader, 14);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 15);
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.PersistentObligation.Populated);
    entities.SupportedCsePerson.Populated = false;
    entities.SupportedCase.Populated = false;

    return ReadEach("ReadCaseCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", import.PersistentCsePerson.Number);
        db.SetInt32(
          command, "otyType", import.PersistentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.PersistentObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", import.PersistentObligation.CspNumber);
          
        db.SetString(command, "cpaType", import.PersistentObligation.CpaType);
        db.SetString(command, "debtTyp", local.Debt.DebtType);
      },
      (db, reader) =>
      {
        entities.SupportedCase.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedCsePerson.Populated = true;
        entities.SupportedCase.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(import.PersistentObligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          import.PersistentObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(import.PersistentObligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "obgId",
          import.PersistentObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", import.PersistentObligation.CspNumber);
        db.SetString(command, "cpaType", import.PersistentObligation.CpaType);
        db.
          SetInt32(command, "otyId", import.PersistentObligation.DtyGeneratedId);
          
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 7);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 8);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 10);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 11);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(import.PersistentObligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", import.PersistentObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(
          command, "effectiveDate1",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", entities.ObligationAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId", entities.ObligationAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.ObligationAssignment.SpdId);
        db.SetDate(command, "effectiveDate2", date);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.SupervisorServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "discontinueDate", date);
        db.SetString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.SupervisorServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.SupervisorServiceProvider.UserId = db.GetString(reader, 1);
        entities.SupervisorServiceProvider.LastName = db.GetString(reader, 2);
        entities.SupervisorServiceProvider.FirstName = db.GetString(reader, 3);
        entities.SupervisorServiceProvider.Populated = true;
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
    /// A value of PersistentCsePerson.
    /// </summary>
    [JsonPropertyName("persistentCsePerson")]
    public CsePerson PersistentCsePerson
    {
      get => persistentCsePerson ??= new();
      set => persistentCsePerson = value;
    }

    /// <summary>
    /// A value of PersistentObligation.
    /// </summary>
    [JsonPropertyName("persistentObligation")]
    public Obligation PersistentObligation
    {
      get => persistentObligation ??= new();
      set => persistentObligation = value;
    }

    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
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

    private CsePerson persistentCsePerson;
    private Obligation persistentObligation;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of PrevSupportedCsePerson.
    /// </summary>
    [JsonPropertyName("prevSupportedCsePerson")]
    public CsePerson PrevSupportedCsePerson
    {
      get => prevSupportedCsePerson ??= new();
      set => prevSupportedCsePerson = value;
    }

    /// <summary>
    /// A value of PrevSupportedCase.
    /// </summary>
    [JsonPropertyName("prevSupportedCase")]
    public Case1 PrevSupportedCase
    {
      get => prevSupportedCase ??= new();
      set => prevSupportedCase = value;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of OspFound.
    /// </summary>
    [JsonPropertyName("ospFound")]
    public Common OspFound
    {
      get => ospFound ??= new();
      set => ospFound = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    /// <summary>
    /// A value of ExitState.
    /// </summary>
    [JsonPropertyName("exitState")]
    public ExitStateWorkArea ExitState
    {
      get => exitState ??= new();
      set => exitState = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of SuppressReason.
    /// </summary>
    [JsonPropertyName("suppressReason")]
    public TextWorkArea SuppressReason
    {
      get => suppressReason ??= new();
      set => suppressReason = value;
    }

    /// <summary>
    /// A value of SuppressType.
    /// </summary>
    [JsonPropertyName("suppressType")]
    public TextWorkArea SuppressType
    {
      get => suppressType ??= new();
      set => suppressType = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
    }

    /// <summary>
    /// A value of CanamReturn.
    /// </summary>
    [JsonPropertyName("canamReturn")]
    public ReportParms CanamReturn
    {
      get => canamReturn ??= new();
      set => canamReturn = value;
    }

    /// <summary>
    /// A value of CanamSend.
    /// </summary>
    [JsonPropertyName("canamSend")]
    public ReportParms CanamSend
    {
      get => canamSend ??= new();
      set => canamSend = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private CsePerson prevSupportedCsePerson;
    private Case1 prevSupportedCase;
    private Common found;
    private Common caseFound;
    private Common ospFound;
    private ServiceProvider supervisor;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private ExitStateWorkArea exitState;
    private ObligationTransaction debt;
    private TextWorkArea suppressReason;
    private TextWorkArea suppressType;
    private DateWorkArea current;
    private DateWorkArea currentTimestamp;
    private ReportParms canamReturn;
    private ReportParms canamSend;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of SupervisorServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorServiceProvider")]
    public ServiceProvider SupervisorServiceProvider
    {
      get => supervisorServiceProvider ??= new();
      set => supervisorServiceProvider = value;
    }

    /// <summary>
    /// A value of SupervisorOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorOfficeServiceProvider")]
    public OfficeServiceProvider SupervisorOfficeServiceProvider
    {
      get => supervisorOfficeServiceProvider ??= new();
      set => supervisorOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCase.
    /// </summary>
    [JsonPropertyName("supportedCase")]
    public Case1 SupportedCase
    {
      get => supportedCase ??= new();
      set => supportedCase = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
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

    private CaseUnit caseUnit;
    private CaseAssignment caseAssignment;
    private CsePersonAccount supported;
    private ObligationType obligationType;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider supervisorServiceProvider;
    private OfficeServiceProvider supervisorOfficeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private Office office;
    private CsePerson supportedCsePerson;
    private Case1 supportedCase;
    private ObligationTransaction debt;
    private ObligationAssignment obligationAssignment;
    private LegalAction legalAction;
  }
#endregion
}

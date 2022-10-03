// Program: LE_ADM_EXCLUSION_UPDATE, ID: 371270004, model: 746.
// Short name: SWE02024
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_ADM_EXCLUSION_UPDATE.
/// </para>
/// <para>
/// Create FDSO certification new instances in the federal_debt_setoff entity 
/// type.
/// </para>
/// </summary>
[Serializable]
public partial class LeAdmExclusionUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_ADM_EXCLUSION_UPDATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAdmExclusionUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAdmExclusionUpdate.
  /// </summary>
  public LeAdmExclusionUpdate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------------------
    // --                         M A I N T E N A 
    // N C E    L O G
    // 
    // --
    // ------------------------------------------------------------------------------------------------------
    // -- DATE		PR/WR		DEVELOPER		
    // DESCRIPTION
    // 
    // --
    // ------------------------------------------------------------------------------------------------------
    // 03/14/2006	WR 258945	M.J. Quinn		Allow for FDSO certification of 
    // bankruptcy.
    // 							(Initial Development)
    // 06/20/2007	PR 311323	GVandy			Re-written to eliminate abends.
    // ------------------------------------------------------------------------------------------------------
    if (!ReadAdministrativeAction())
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";

      return;
    }

    switch(TrimEnd(import.AdmBankruptyStatus.Text10))
    {
      case "ADM_ADD":
        if (Equal(import.Bankruptcy.ExpectedBkrpDischargeDate, local.Null1.Date))
          
        {
          // --  Set end date to max system date (i.e. 2099-12-31)
          local.ObligationAdmActionExemption.EndDate =
            UseCabSetMaximumDiscontinueDate();
        }
        else
        {
          local.ObligationAdmActionExemption.EndDate =
            import.Bankruptcy.ExpectedBkrpDischargeDate;
        }

        foreach(var item in ReadObligation())
        {
          // -- Create the ADM exclusion for the EXGR screen.  The values set 
          // for each attribute were not changed from the prior version of this
          // cab.
          // -- The prior version of this cab set the effective date to the null
          // date.  This is presumably so that no more
          //    than one ADM exclusion will exist for an obligation (i.e. an 
          // already exists will occur when trying to add a
          //    subsequent one).  A discussion with Jennifer Elliott did not 
          // seem to indicate that this was a problem.
          //    Therefore, I am continuing to set the effective date to the null
          // date.
          try
          {
            CreateObligationAdmActionExemption();

            // -- Continue.
            local.EabReportSend.RptDetail = "New ADM exclusion: CSE No. " + import
              .Obligor.Number + "  AP name: " + import.Obligor.FormattedName + " SSN: " +
              import.Obligor.Ssn;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                // -- This is OK since the obligation may have been previously 
                // exempted if the the obligor had a
                //    prior FDSO certification, was subsequently decertified, 
                // and is now being certified again.
                //    The ADM exemption already exists, so we don't need to do 
                // anything else.
                //    See the note above concerning how the effective date is 
                // being set.
                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        break;
      case "ADM_DELETE":
        // -- Remove the exclusions from the EXGR screen
        foreach(var item in ReadObligationAdmActionExemption())
        {
          DeleteObligationAdmActionExemption();
          local.EabReportSend.RptDetail = "Removing ADM exclusion: CSE No. " + import
            .Obligor.Number + "  AP name: " + import.Obligor.FormattedName + " SSN: " +
            import.Obligor.Ssn;
        }

        break;
      default:
        break;
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -- Log the Addition or Removal of the ADM exclusion to the Bankruptcy 
      // report.
      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error writing to the Bankruptcy Report in cab le_adm_exclusion_update.";
          
        UseCabErrorReport();
        ExitState = "ERROR_WRITING_TO_REPORT_AB";
      }
    }
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var aatType = entities.AdministrativeAction.Type1;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var effectiveDate = local.Null1.Date;
    var endDate = local.ObligationAdmActionExemption.EndDate;
    var lastName = "SRRUN001";
    var firstName = "SWEL500B";
    var reason = "BANKRUPT ADM EXCL";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var description = Spaces(240);

    CheckValid<ObligationAdmActionExemption>("CpaType", cpaType);
    entities.ObligationAdmActionExemption.Populated = false;
    Update("CreateObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "aatType", aatType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetDate(command, "endDt", endDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInitial", "");
        db.SetNullableString(command, "suffix", "");
        db.SetString(command, "reason", reason);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.ObligationAdmActionExemption.OtyType = otyType;
    entities.ObligationAdmActionExemption.AatType = aatType;
    entities.ObligationAdmActionExemption.ObgGeneratedId = obgGeneratedId;
    entities.ObligationAdmActionExemption.CspNumber = cspNumber;
    entities.ObligationAdmActionExemption.CpaType = cpaType;
    entities.ObligationAdmActionExemption.EffectiveDate = effectiveDate;
    entities.ObligationAdmActionExemption.EndDate = endDate;
    entities.ObligationAdmActionExemption.LastName = lastName;
    entities.ObligationAdmActionExemption.FirstName = firstName;
    entities.ObligationAdmActionExemption.MiddleInitial = "";
    entities.ObligationAdmActionExemption.Suffix = "";
    entities.ObligationAdmActionExemption.Reason = reason;
    entities.ObligationAdmActionExemption.CreatedBy = createdBy;
    entities.ObligationAdmActionExemption.CreatedTstamp = createdTstamp;
    entities.ObligationAdmActionExemption.Description = description;
    entities.ObligationAdmActionExemption.Populated = true;
  }

  private void DeleteObligationAdmActionExemption()
  {
    Update("DeleteObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ObligationAdmActionExemption.OtyType);
        db.SetString(
          command, "aatType", entities.ObligationAdmActionExemption.AatType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationAdmActionExemption.ObgGeneratedId);
        db.SetString(
          command, "cspNumber",
          entities.ObligationAdmActionExemption.CspNumber);
        db.SetString(
          command, "cpaType", entities.ObligationAdmActionExemption.CpaType);
        db.SetDate(
          command, "effectiveDt",
          entities.ObligationAdmActionExemption.EffectiveDate.
            GetValueOrDefault());
      });
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationAdmActionExemption()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return ReadEach("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetString(command, "aatType", entities.AdministrativeAction.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.LastName =
          db.GetString(reader, 7);
        entities.ObligationAdmActionExemption.FirstName =
          db.GetString(reader, 8);
        entities.ObligationAdmActionExemption.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ObligationAdmActionExemption.Suffix =
          db.GetNullableString(reader, 10);
        entities.ObligationAdmActionExemption.Reason = db.GetString(reader, 11);
        entities.ObligationAdmActionExemption.CreatedBy =
          db.GetString(reader, 12);
        entities.ObligationAdmActionExemption.CreatedTstamp =
          db.GetDateTime(reader, 13);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 14);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);

        return true;
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
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of AdmBankruptyStatus.
    /// </summary>
    [JsonPropertyName("admBankruptyStatus")]
    public TextWorkArea AdmBankruptyStatus
    {
      get => admBankruptyStatus ??= new();
      set => admBankruptyStatus = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private Bankruptcy bankruptcy;
    private TextWorkArea admBankruptyStatus;
    private CsePersonsWorkSet obligor;
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
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private ObligationAdmActionExemption obligationAdmActionExemption;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea null1;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligor;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private Obligation obligation;
    private CsePerson csePerson;
  }
#endregion
}

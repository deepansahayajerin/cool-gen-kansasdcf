// Program: SP_DTR_CU_OBLG_STATUS, ID: 372070764, model: 746.
// Short name: SWE02124
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
/// A program: SP_DTR_CU_OBLG_STATUS.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrCuOblgStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_CU_OBLG_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrCuOblgStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrCuOblgStatus.
  /// </summary>
  public SpDtrCuOblgStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial development - October 9, 1997
    // Developer - Jack Rookard, MTW
    // This action block is invoked when an Infrastructrue occurrence is 
    // processed by the Event Processor and that Infrastructure occurrence
    // specifies that an Obligation has been deleted.  This action block
    // examines the database to determine whether or not Obligations exist for
    // the AP-Child pair specified by the Infrastructure Case Unit.  If the Case
    // Unit State is in sync with the database, no infrastructure occurrences
    // are created.  Otherwise, Case Unit transforming and non-transforming
    // Infrastructure occurrences are created.
    // Maureen Brown, April, 1999 - Replaced usage of obligation totals with 
    // retrieval of debt details.  The code looks for a balance due debt details
    // to determine whether or not the AP is obligated.
    // Maureen Brown, May, 1999 - Removed code that creates finance events.  
    // Removed case unit transformation of the 'is obligated' flag.  We now only
    // write out events for each case unit found for the AP/child combination.
    // Either event 'oblpaidothrcase' or event 'oblg_notexist' will be
    // written.
    // 05/13/2010 GVandy  CQ966  Return infrastructure records to be created in 
    // a group view rather than create in this cab.  Same for case_units to be
    // updated.
    local.Current.Date = Now().Date;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReferenceDate = Now().Date;
    local.Infrastructure.UserId = "SWEPB301";
    local.HardcodeCs.SystemGeneratedIdentifier = 1;
    local.HardcodeSp.SystemGeneratedIdentifier = 2;
    local.HardcodeMs.SystemGeneratedIdentifier = 3;
    local.HardcodeMc.SystemGeneratedIdentifier = 19;

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCaseUnit())
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    if (!ReadCsePerson1())
    {
      ExitState = "CO0000_AP_CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePerson2())
    {
      ExitState = "CO0000_CHILD_NF";

      return;
    }

    // *** Case Unit AP/Child "is Obligated" determination processing.
    // : First, look for active accrual instructions.
    if (ReadAccrualInstructions())
    {
      // : No need to continue - this AP is still obligated, and we know the 
      // case unit state 'is obligated' flag is already 'Y', since the
      // obligation that was just deleted would have caused that.  No need to
      // sync the flags.
      return;
    }
    else
    {
      // : continue
    }

    foreach(var item in ReadObligationDebtDetailObligationType())
    {
      // : Only the following obligation types are considered for 'is obligated
      // '.
      switch(TrimEnd(entities.ObligationType.Code))
      {
        case "CS":
          break;
        case "MS":
          break;
        case "SP":
          break;
        case "AJ":
          break;
        case "CRCH":
          break;
        case "MC":
          break;
        case "IJ":
          break;
        case "MJ":
          break;
        case "SAJ":
          break;
        case "%UME":
          break;
        case "718B":
          break;
        default:
          continue;
      }

      local.ObligationExists.Flag = "Y";

      // **** check if the obligation is paid off from obligation due buckets **
      // *
      if (entities.DebtDetail.BalanceDueAmt + entities
        .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
      {
        // : No need to continue - this AP is still obligated, and we know the 
        // case unit state 'is obligated' flag is already 'Y', since the
        // obligation that was just deleted would have caused that.  No need to
        // sync the flags.
        return;
      }
      else
      {
        // : Continue
      }
    }

    // Initialize the appropriate local Infrastructure views.
    local.Infrastructure.CsePersonNumber = entities.Ap.Number;
    local.Infrastructure.UserId = global.UserId;

    // : Go through all the case units for the AP and child, writing events
    //   to transform the 'is obligated' to the correct value.
    foreach(var item in ReadCaseUnitCase())
    {
      if (ReadInterstateRequest())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      // : Extract the 'is obligated' flag from the case unit state attribute.
      local.CaseUnitIsObgFlag.FuncText1 =
        Substring(entities.CaseUnit.State, 5, 1);
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      local.HoldCuNum.Text3 = NumberToString(entities.CaseUnit.CuNumber, 13, 3);

      if (AsChar(local.CaseUnitIsObgFlag.FuncText1) == 'Y')
      {
        if (AsChar(local.ObligationExists.Flag) == 'Y')
        {
          // : The AP has one or more paid off obligations.
          local.Infrastructure.BusinessObjectCd = "CAU";
          local.Infrastructure.EventId = 11;
          local.Infrastructure.ReasonCode = "OBLPAIDOTHRCASE";
          local.Infrastructure.Detail = "Disc or pd oblig for AP" + entities
            .Ap.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " CH";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
            .Ch.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " found on Case ";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
            .Case1.Number;
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

          export.ImportExportEvents.Index = export.ImportExportEvents.Count;
          export.ImportExportEvents.CheckSize();

          MoveInfrastructure(local.Infrastructure,
            export.ImportExportEvents.Update.G);

          // mfb07/99: We currently have no event to handle a lifecycle 
          // transformation of the 'is obligated' flag from 'Y' to 'N' in this
          // situation.  Therefore it is being done here.  In the future, we
          // will create an event to handle the update.
          export.ImportExportCaseUnits.Index =
            export.ImportExportCaseUnits.Count;
          export.ImportExportCaseUnits.CheckSize();

          export.ImportExportCaseUnits.Update.Case1.Number =
            entities.Case1.Number;
          export.ImportExportCaseUnits.Update.CaseUnit.CuNumber =
            entities.CaseUnit.CuNumber;
          export.ImportExportCaseUnits.Update.CaseUnit.State =
            Substring(entities.CaseUnit.State, CaseUnit.State_MaxLength, 1, 4) +
            "N";
        }
        else
        {
          // No obligations exist for the current Case Unit, however, the Case 
          // Unit state
          // "is obligated" flag is set to Y.
          // This can occur when an Obligation is created, which transforms the 
          // Case Unit state to Y, and that transforming Obligation is then
          // deleted.
          // When an obligation is deleted, an infrastructure occurrence is 
          // created, which specifies an Event-Event Detail combination which
          // triggers this
          // action block.
          // The database is examined by this action block for occurrences of 
          // Obligations
          // for the AP-Child combination specified by the Case Unit.  If no 
          // Obligation
          // occurrences are found and the Case Unit state "is obligated" flag 
          // is Y,
          // this action block raises the following infrastructure occurrence to
          // transform the Case Unit state to the correct value of U (unknown).
          local.Infrastructure.BusinessObjectCd = "CAU";
          local.Infrastructure.EventId = 47;
          local.Infrastructure.ReasonCode = "OBLG_NOTEXIST";
          local.Infrastructure.Detail = "No Obligations exist for Case " + entities
            .Case1.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " Case Unit ";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
            .HoldCuNum.Text3;

          export.ImportExportEvents.Index = export.ImportExportEvents.Count;
          export.ImportExportEvents.CheckSize();

          MoveInfrastructure(local.Infrastructure,
            export.ImportExportEvents.Update.G);
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(command, "discontinueDt", date);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetNullableString(command, "cspSupNumber", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(
          command, "cuNumber",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CaseUnit.CasNo = db.GetString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.CaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitCase()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseUnitCase",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.Ap.Number);
        db.SetNullableString(command, "cspNoChild", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CaseUnit.CasNo = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Type1 = db.GetString(reader, 1);
        entities.Ap.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ap.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoChild ?? "");
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Type1 = db.GetString(reader, 1);
        entities.Ch.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ch.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationDebtDetailObligationType()
  {
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationDebtDetailObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableString(command, "cspSupNumber", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ObligationType.Code = db.GetString(reader, 9);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportCaseUnitsGroup group.</summary>
    [Serializable]
    public class ImportExportCaseUnitsGroup
    {
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
      /// A value of CaseUnit.
      /// </summary>
      [JsonPropertyName("caseUnit")]
      public CaseUnit CaseUnit
      {
        get => caseUnit ??= new();
        set => caseUnit = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 case1;
      private CaseUnit caseUnit;
    }

    /// <summary>A ImportExportEventsGroup group.</summary>
    [Serializable]
    public class ImportExportEventsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Infrastructure G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure g;
    }

    /// <summary>
    /// Gets a value of ImportExportCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportCaseUnitsGroup> ImportExportCaseUnits =>
      importExportCaseUnits ??= new(ImportExportCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("importExportCaseUnits")]
    [Computed]
    public IList<ImportExportCaseUnitsGroup> ImportExportCaseUnits_Json
    {
      get => importExportCaseUnits;
      set => ImportExportCaseUnits.Assign(value);
    }

    /// <summary>
    /// Gets a value of ImportExportEvents.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportEventsGroup> ImportExportEvents =>
      importExportEvents ??= new(ImportExportEventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportEvents for json serialization.
    /// </summary>
    [JsonPropertyName("importExportEvents")]
    [Computed]
    public IList<ImportExportEventsGroup> ImportExportEvents_Json
    {
      get => importExportEvents;
      set => ImportExportEvents.Assign(value);
    }

    private Array<ImportExportCaseUnitsGroup> importExportCaseUnits;
    private Array<ImportExportEventsGroup> importExportEvents;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodeMc.
    /// </summary>
    [JsonPropertyName("hardcodeMc")]
    public ObligationType HardcodeMc
    {
      get => hardcodeMc ??= new();
      set => hardcodeMc = value;
    }

    /// <summary>
    /// A value of HardcodeMs.
    /// </summary>
    [JsonPropertyName("hardcodeMs")]
    public ObligationType HardcodeMs
    {
      get => hardcodeMs ??= new();
      set => hardcodeMs = value;
    }

    /// <summary>
    /// A value of HardcodeSp.
    /// </summary>
    [JsonPropertyName("hardcodeSp")]
    public ObligationType HardcodeSp
    {
      get => hardcodeSp ??= new();
      set => hardcodeSp = value;
    }

    /// <summary>
    /// A value of HardcodeCs.
    /// </summary>
    [JsonPropertyName("hardcodeCs")]
    public ObligationType HardcodeCs
    {
      get => hardcodeCs ??= new();
      set => hardcodeCs = value;
    }

    /// <summary>
    /// A value of ObligationExists.
    /// </summary>
    [JsonPropertyName("obligationExists")]
    public Common ObligationExists
    {
      get => obligationExists ??= new();
      set => obligationExists = value;
    }

    /// <summary>
    /// A value of CaseUnitIsObgFlag.
    /// </summary>
    [JsonPropertyName("caseUnitIsObgFlag")]
    public CaseFuncWorkSet CaseUnitIsObgFlag
    {
      get => caseUnitIsObgFlag ??= new();
      set => caseUnitIsObgFlag = value;
    }

    /// <summary>
    /// A value of HoldCuNum.
    /// </summary>
    [JsonPropertyName("holdCuNum")]
    public WorkArea HoldCuNum
    {
      get => holdCuNum ??= new();
      set => holdCuNum = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private ObligationType hardcodeMc;
    private ObligationType hardcodeMs;
    private ObligationType hardcodeSp;
    private ObligationType hardcodeCs;
    private Common obligationExists;
    private CaseFuncWorkSet caseUnitIsObgFlag;
    private WorkArea holdCuNum;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private CsePersonAccount supported;
    private CsePersonAccount obligor;
    private DebtDetail debtDetail;
    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private InterstateRequest interstateRequest;
    private CsePerson ch;
    private CsePerson ap;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}

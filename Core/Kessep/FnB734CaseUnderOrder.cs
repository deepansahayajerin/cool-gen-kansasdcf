// Program: FN_B734_CASE_UNDER_ORDER, ID: 945153813, model: 746.
// Short name: SWE03699
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
/// A program: FN_B734_CASE_UNDER_ORDER.
/// </para>
/// <para>
/// Determine if a case is under order.
/// </para>
/// </summary>
[Serializable]
public partial class FnB734CaseUnderOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_CASE_UNDER_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734CaseUnderOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734CaseUnderOrder.
  /// </summary>
  public FnB734CaseUnderOrder(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 05/30/13  GVandy	CQ36547		Initial Development.  This action block was
    // 			Segment A	cloned from logic in fn_b734_priority_1_1.
    // ---------------------------------------------------------------------------------------------------
    // -- Default the export flag to "N", signifying case is not under order.
    export.CaseUnderOrder.Flag = "N";

    // ------------------------------------------------------------------------------
    // -- Read each open case.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadCaseCaseAssignment())
    {
      // -------------------------------------------------------------------------------------
      // -- N U M E R A T O R  (Number of Cases with an Order) (OCSE157 Line 2)
      // -------------------------------------------------------------------------------------
      local.NonFinLdet.Flag = "N";
      local.FinLdet.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH combos - active or not.
      // Date checks will ensure we read overlapping AP/CH roles only.
      // If fin ldet found, count in line 2. Don't count case in 2c.
      // If non-fin ldet found, count in line 2. We still need to check if
      // fin LDET exists. This is necessary for line 2c.
      // -----------------------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // ----------------------------------------------------------------------
        // Using LROL, read J-class HIC or UM ldet - active or not.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.NonFinLdet.Flag) == 'N')
        {
          if (ReadLegalActionDetailLegalAction1())
          {
            local.NonFinLdet.Flag = "Y";

            break;
          }
        }

        // ----------------------------------------------------------------------
        // Using LOPS, read all J-class fin LDETs - active or not.
        // Read for Obligations with specific ob types.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.FinLdet.Flag) == 'N')
        {
          foreach(var item2 in ReadLegalActionDetailLegalAction2())
          {
            foreach(var item3 in ReadObligationTypeObligation())
            {
              // -------------------------------------------------------------------
              // We found a finance LDET with desired Ob types.
              // Now check if Accrual Instructions were 'ever' setup for Current
              // Obligation.
              // Qualify by supported person.
              // --------------------------------------------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (ReadAccrualInstructions())
                {
                  local.FinLdet.Flag = "Y";

                  // ----------------------------------------------------------------------
                  // We found a fin-LDET for this case.
                  // No further processing is necessary for this case.
                  // ----------------------------------------------------------------------
                  goto ReadEach;
                }
              }

              // -------------------------------------------------------------------
              // We got here because Accrual Instructions were never setup
              // on current Obligation.
              // Now check if debt was 'ever' owed on this obligation.
              // -------------------------------------------------------------------
              // ----------------------------------------------
              // Qualify Debts by Supp person. 7/18/01
              // Only read debts created before FY end.
              // ----------------------------------------------
              foreach(var item4 in ReadDebtDebtDetail())
              {
                // -----------------------------------------------
                // Skip MJ AF/FC.
                // -----------------------------------------------
                if (Equal(entities.ObligationType.Code, "MJ"))
                {
                  // -----------------------------------------------
                  // CAB defaults Coll date to Current date. So don't pass 
                  // anything.
                  // -----------------------------------------------
                  UseFnDeterminePgmForDebtDetail();

                  if (Equal(local.Program.Code, "AF") || Equal
                    (local.Program.Code, "AFI") || Equal
                    (local.Program.Code, "FC") || Equal
                    (local.Program.Code, "FCI"))
                  {
                    // -----------------------------------------------
                    // Skip this debt detail.
                    // -----------------------------------------------
                    continue;
                  }
                }

                local.FinLdet.Flag = "Y";

                // ----------------------------------------------------------------------
                // We found a fin LDET for this case.
                // No further processing is necessary for this case.
                // ----------------------------------------------------------------------
                goto ReadEach;
              }
            }
          }
        }
      }

ReadEach:

      if (AsChar(local.FinLdet.Flag) == 'N' && AsChar
        (local.NonFinLdet.Flag) == 'N')
      {
        // -- Case is not under order.  Continue.  Export flag was defaulted to
        // "N" at beginning of cab.
      }
      else
      {
        // -- Case is under order.  Set export flag to "Y" indicating that case 
        // is under order.
        export.CaseUnderOrder.Flag = "Y";
      }
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ChCsePerson.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.ChCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChCsePerson.Number = db.GetString(reader, 8);
        entities.ChCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 13);
        entities.ChCsePerson.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.ChCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadLegalActionDetailLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction2()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumber1", entities.ApCsePerson.Number);
          
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.
          SetNullableString(command, "cspNumber2", entities.ChCsePerson.Number);
          
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligation()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationTypeObligation",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private Case1 case1;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseUnderOrder.
    /// </summary>
    [JsonPropertyName("caseUnderOrder")]
    public Common CaseUnderOrder
    {
      get => caseUnderOrder ??= new();
      set => caseUnderOrder = value;
    }

    private Common caseUnderOrder;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NonFinLdet.
    /// </summary>
    [JsonPropertyName("nonFinLdet")]
    public Common NonFinLdet
    {
      get => nonFinLdet ??= new();
      set => nonFinLdet = value;
    }

    /// <summary>
    /// A value of FinLdet.
    /// </summary>
    [JsonPropertyName("finLdet")]
    public Common FinLdet
    {
      get => finLdet ??= new();
      set => finLdet = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private Common nonFinLdet;
    private Common finLdet;
    private DateWorkArea null1;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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

    private Case1 case1;
    private CsePerson chCsePerson;
    private CsePerson apCsePerson;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole apCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private ObligationType obligationType;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private DebtDetail debtDetail;
    private LegalActionPerson obligor;
    private LegalActionPerson supp;
    private CaseAssignment caseAssignment;
  }
#endregion
}

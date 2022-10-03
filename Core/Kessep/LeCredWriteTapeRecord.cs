// Program: LE_CRED_WRITE_TAPE_RECORD, ID: 372739685, model: 746.
// Short name: SWE00749
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CRED_WRITE_TAPE_RECORD.
/// </summary>
[Serializable]
public partial class LeCredWriteTapeRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CRED_WRITE_TAPE_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCredWriteTapeRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCredWriteTapeRecord.
  /// </summary>
  public LeCredWriteTapeRecord(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 04/30/96  H HOOKS			INITIAL DEVELOPMENT
    // 02/11/99  PMcElderry			Removed uneccessary READs; added new code
    // 					commiserate w/certification requirements.
    // 02/20/99  P McElderry			Added logic for Id Numbers for TRW (2990091),
    // 					TransUnion (00V2269), and CBI (3482688).
    // 					Moved UPDATE logic to PRAD so that if an
    // 					error occurs during the writing to tape the
    // 					job can be resubmitted w/out having to alter
    // 					DB.
    // 04/12/99  PMcElderry			Inserted 15 day rule; revamped code
    // 06/18/99  PMcElderry			Added KESSEP specific collection types.  The
    // 					old types have been left in the code as a
    // 					reference if needed.  Added new marriage
    // 					types.  Added logic for OBLs w/out a payment
    // 					schedule.
    // 07/13/99  PMcElderry			Various performance enhancements
    // 11/08/99  PMcElderry			Changed format for Cra_Referral_Date,
    // 					Last_Account_Date, and Last_Pay_Date from
    // 					YYMMDD to MMDDYY format
    // 05/15/01  E.Shirk	PR119218	Process should no longer send any amounts on
    // 					a delete (D04) transaction.   Previously only
    // 					sending highest and current amount.
    // 06/01/01  E.Shirk	PR119216	altered read each of the credit reporting
    // 					action entity to include CAN transactions.
    // 07/13/01  E.Shirk	PR123643	Moved the portion of logic that formatted and
    // 					built the CR tape within a conditiional that
    // 					checks whether a CRA row exists.   This
    // 					prevents every transaction from being sent on
    // 					the tape.
    // 09/04/01  E.Shirk	PR123643	Altered logic to begin sending a 71 acct
    // 					status code instead of the current 70.   The
    // 					70 acct. status code was removed by the Metro
    // 					2 standard.
    // 06/27/02  E.Shirk	PR139555	Removed the logic that skipped sending an
    // 					update  transaction if 10 days had passed, or
    // 					35 days on a delete, fron the date it was
    // 					created.
    // 06/18/07  Raj S		WR 303198 	Modified to apply the new business rule (CRED
    // 					need not have a verified/Domestic address)
    // 					while selecting the person address.  If there
    // 					is no Verified Domestic/Domestic Address not
    // 					found in the system, the program will select
    // 					any recent address (i.e. can by any address).
    // 08/04/17  GVandy	CQ56369		Changes for Metro2 file/record layouts.
    // 					Restructured code to cleanup formatting and
    // 					simplify logic.
    // 05/15/18  GVandy	CQ62291		Send Payment Rating '0' when Account Status is
    // '13'.
    // ---------------------------------------------------------------------------------------------
    export.Metro2.Assign(import.Metro2);

    // -------------------------------------------------------------------------------------
    // -- Get NCP name, ssn, and date of birth.
    // -------------------------------------------------------------------------------------
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseSiReadCsePersonBatch();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Find start date of the earliest AP case role.
    // -------------------------------------------------------------------------------------
    ReadCaseRole();

    if (!entities.CaseRole.Populated)
    {
      ExitState = "CASE_ROLE_NF_RB";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Get NCP address.
    // -------------------------------------------------------------------------------------
    ReadCsePersonAddress1();

    if (!entities.CsePersonAddress.Populated)
    {
      ReadCsePersonAddress2();
    }

    // -------------------------------------------------------------------------------------
    // Who: Raj When: 06/18/2007 Ref: WR 303198 Select any recent address Change
    // Start here
    // -------------------------------------------------------------------------------------
    if (!entities.CsePersonAddress.Populated)
    {
      ReadCsePersonAddress3();
    }

    // -------------------------------------------------------------------------------------
    // Who: Raj When: 06/18/2007 Ref: WR 303198 Select any recent address Change
    // Ends here
    // -------------------------------------------------------------------------------------
    local.FirstDayOfMonth.Date =
      AddDays(import.ProgramProcessingInfo.ProcessDate,
      -(Day(import.ProgramProcessingInfo.ProcessDate) - 1));

    // -- Find the most recent date from
    //     a) due date of non accruing arrears debts
    //     b) due date of most recent accruing obligation prior to 1st of the 
    // month
    foreach(var item in ReadDebtDetailObligationType())
    {
      if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
        .ObligationType.SystemGeneratedIdentifier == 2 || entities
        .ObligationType.SystemGeneratedIdentifier == 3 || entities
        .ObligationType.SystemGeneratedIdentifier == 19)
      {
        if (Lt(entities.DebtDetail.DueDt, local.FirstDayOfMonth.Date))
        {
          // -- Reportable arrears debt
        }
        else
        {
          continue;
        }
      }

      local.DtOf1StDelinquency.Date = entities.DebtDetail.DueDt;

      break;
    }

    // -------------------------------------------------------------------------------------
    // Format the Metro2 Base Record.
    // -------------------------------------------------------------------------------------
    local.Metro2CraBaseRecord.RecordDescriptorWord = "0426";
    local.Metro2CraBaseRecord.ProcessingIndicator = "1";
    local.Metro2CraBaseRecord.Timestamp =
      NumberToString(Now().Date.Month, 14, 2) + NumberToString
      (Now().Date.Day, 14, 2) + NumberToString(Now().Date.Year, 12, 4) + NumberToString
      (Time(Now()).Hours, 14, 2) + NumberToString
      (Time(Now()).Minutes, 14, 2) + NumberToString
      (Time(Now()).Seconds, 14, 2);
    local.Metro2CraBaseRecord.Reserved4 = "0";
    local.Metro2CraBaseRecord.IdentificationNumber = "20 000 00";
    local.Metro2CraBaseRecord.CycleIdentifier = "";
    local.Metro2CraBaseRecord.ConsumerAccountNumber = import.CsePerson.Number;
    local.Metro2CraBaseRecord.PortfolioType = "O";
    local.Metro2CraBaseRecord.AccountType = "50";
    local.Metro2CraBaseRecord.DateOpened =
      NumberToString(Month(entities.CaseRole.StartDate), 14, 2) + NumberToString
      (Day(entities.CaseRole.StartDate), 14, 2) + NumberToString
      (Year(entities.CaseRole.StartDate), 12, 4);
    local.Metro2CraBaseRecord.CreditLimit = "000000000";
    local.Metro2CraBaseRecord.HighestCreditAmount = "000000000";
    local.Metro2CraBaseRecord.TermsDuration = "001";
    local.Metro2CraBaseRecord.TermsFrequency = "";
    local.Metro2CraBaseRecord.ScheduledMonthlyPaymentAmount = "000000000";
    local.Metro2CraBaseRecord.ActualPaymentAmount = "000000000";
    local.Metro2CraBaseRecord.AccountStatus =
      Substring(import.CreditReportingAction.CraTransCode, 2, 2);

    // -- (This should never be needed) Substitute new Metro2 status codes for 
    // old no longer
    //    supported codes.
    switch(TrimEnd(local.Metro2CraBaseRecord.AccountStatus))
    {
      case "04":
        // -- 04 is no longer supported in Metro2 format.  Use DA instead.
        local.Metro2CraBaseRecord.AccountStatus = "DA";

        break;
      case "70":
        // -- 70 is no longer supported in Metro2 format.  Use 71 instead.
        local.Metro2CraBaseRecord.AccountStatus = "71";

        break;
      case "11":
        break;
      case "13":
        break;
      case "64":
        break;
      case "71":
        break;
      case "93":
        break;
      case "DA":
        break;
      default:
        // -- Write to the error report and skip the NCP.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Skipped CSP Number " + import
          .CsePerson.Number + "  Invalid Account Status " + local
          .Metro2CraBaseRecord.AccountStatus;
        UseCabErrorReport();

        return;
    }

    if (Equal(local.Metro2CraBaseRecord.AccountStatus, "13"))
    {
      // 05/15/18 GVandy  CQ62291  Send Payment Rating '0' when Account Status 
      // is '13'.
      local.Metro2CraBaseRecord.PaymentRating = "0";
    }
    else
    {
      local.Metro2CraBaseRecord.PaymentRating = "";
    }

    local.Metro2CraBaseRecord.PaymentHistoryProfile =
      "BBBBBBBBBBBBBBBBBBBBBBBB";
    local.Metro2CraBaseRecord.SpecialComment = "CS";
    local.Metro2CraBaseRecord.ComplianceConditionCode = "";
    local.Metro2CraBaseRecord.CurrentBalance =
      NumberToString((long)import.CreditReportingAction.CurrentAmount.
        GetValueOrDefault(), 7, 9);
    local.Metro2CraBaseRecord.AmountPastDue =
      NumberToString((long)import.CreditReportingAction.CurrentAmount.
        GetValueOrDefault(), 7, 9);
    local.Metro2CraBaseRecord.OriginalChargeOffAmount = "000000000";
    local.Metro2CraBaseRecord.DateOfAccountInformation =
      NumberToString(Month(import.CreditReportingAction.DateSentToCra), 14, 2) +
      NumberToString(Day(import.CreditReportingAction.DateSentToCra), 14, 2) + NumberToString
      (Year(import.CreditReportingAction.DateSentToCra), 12, 4);
    local.Metro2CraBaseRecord.DateOfFirstDelinquency =
      NumberToString(Month(local.DtOf1StDelinquency.Date), 14, 2) + NumberToString
      (Day(local.DtOf1StDelinquency.Date), 14, 2) + NumberToString
      (Year(local.DtOf1StDelinquency.Date), 12, 4);

    if (Equal(local.Metro2CraBaseRecord.AccountStatus, "13") || Equal
      (local.Metro2CraBaseRecord.AccountStatus, "64"))
    {
      // -- Report most recent case closure date
      ReadCase();

      if (entities.Case1.Populated)
      {
        local.Metro2CraBaseRecord.DateClosed =
          NumberToString(Month(entities.Case1.StatusDate), 14, 2) + NumberToString
          (Day(entities.Case1.StatusDate), 14, 2) + NumberToString
          (Year(entities.Case1.StatusDate), 12, 4);
      }
      else
      {
        local.Metro2CraBaseRecord.DateClosed =
          NumberToString(Month(import.ProgramProcessingInfo.ProcessDate), 14, 2) +
          NumberToString
          (Day(import.ProgramProcessingInfo.ProcessDate), 14, 2) + NumberToString
          (Year(import.ProgramProcessingInfo.ProcessDate), 12, 4);
      }
    }
    else
    {
      local.Metro2CraBaseRecord.DateClosed = "00000000";
    }

    local.Metro2CraBaseRecord.DateOfLastPayment = "00000000";
    local.Metro2CraBaseRecord.InterestTypeIndicator = "";
    local.Metro2CraBaseRecord.Reserved29 = "";
    local.Metro2CraBaseRecord.Surname = local.CsePersonsWorkSet.LastName;
    local.Metro2CraBaseRecord.FirstName = local.CsePersonsWorkSet.FirstName;
    local.Metro2CraBaseRecord.MiddleName =
      local.CsePersonsWorkSet.MiddleInitial;
    local.Metro2CraBaseRecord.GenerationCode = "";
    local.Metro2CraBaseRecord.SocialSecurityNumber =
      local.CsePersonsWorkSet.Ssn;
    local.Metro2CraBaseRecord.DateOfBirth =
      NumberToString(Month(local.CsePersonsWorkSet.Dob), 14, 2) + NumberToString
      (Day(local.CsePersonsWorkSet.Dob), 14, 2) + NumberToString
      (Year(local.CsePersonsWorkSet.Dob), 12, 4);

    if (Equal(local.Metro2CraBaseRecord.DateOfBirth, "01010001"))
    {
      local.Metro2CraBaseRecord.DateOfBirth = "";
    }

    local.Metro2CraBaseRecord.TelephoneNumber = "0000000000";
    local.Metro2CraBaseRecord.EcoaCode = "1";
    local.Metro2CraBaseRecord.ConsumerInformationIndicator = "";

    if (entities.CsePersonAddress.Populated)
    {
      local.Metro2CraBaseRecord.CountryCode = "US";
      local.Metro2CraBaseRecord.FirstLineOfAddress =
        entities.CsePersonAddress.Street1 ?? Spaces(32);
      local.Metro2CraBaseRecord.SecondLineOfAddress =
        entities.CsePersonAddress.Street2 ?? Spaces(32);
      local.Metro2CraBaseRecord.City = entities.CsePersonAddress.City ?? Spaces
        (20);
      local.Metro2CraBaseRecord.State = entities.CsePersonAddress.State ?? Spaces
        (2);
      local.Metro2CraBaseRecord.PostalZipCode =
        entities.CsePersonAddress.ZipCode + entities.CsePersonAddress.Zip4;
    }

    local.Metro2CraBaseRecord.AddressIndicator = "";
    local.Metro2CraBaseRecord.ResidenceCode = "";

    // -------------------------------------------------------------------------------------
    // Write the Base Record to the Credit Reporting File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "BASE";
    UseExtWriteCredToTape();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing BASE Record to Credit File...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Update counts for the trailer record.
    // -------------------------------------------------------------------------------------
    ++export.Metro2.BaseRecordCount;

    if (!IsEmpty(local.Metro2CraBaseRecord.DateOfBirth))
    {
      ++export.Metro2.DobBaseSegmentCount;
      ++export.Metro2.DobAllSegmentsCount;
    }

    if (!IsEmpty(local.Metro2CraBaseRecord.SocialSecurityNumber))
    {
      ++export.Metro2.SsnBaseSegmentCount;
      ++export.Metro2.SsnAllSegmentsCount;
    }

    switch(TrimEnd(local.Metro2CraBaseRecord.AccountStatus))
    {
      case "11":
        ++export.Metro2.Status11Count;

        break;
      case "13":
        ++export.Metro2.Status13Count;

        break;
      case "64":
        ++export.Metro2.Status64Count;

        break;
      case "71":
        ++export.Metro2.Status71Count;

        break;
      case "93":
        ++export.Metro2.Status93Count;

        break;
      case "DA":
        ++export.Metro2.StatusDaCount;

        break;
      default:
        break;
    }
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

  private void UseExtWriteCredToTape()
  {
    var useImport = new ExtWriteCredToTape.Import();
    var useExport = new ExtWriteCredToTape.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Metro2CraBaseRecord.Assign(local.Metro2CraBaseRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(ExtWriteCredToTape.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.ReadCsePersonAdabas.Type1 = useExport.AbendData.Type1;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
          local.NullDateCsePersonAddress.VerifiedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 10);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 10);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress3()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 10);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ObligationType.Classification = db.GetString(reader, 9);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 10);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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
    /// A value of Metro2.
    /// </summary>
    [JsonPropertyName("metro2")]
    public Metro2 Metro2
    {
      get => metro2 ??= new();
      set => metro2 = value;
    }

    private CreditReportingAction creditReportingAction;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private Metro2 metro2;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Metro2.
    /// </summary>
    [JsonPropertyName("metro2")]
    public Metro2 Metro2
    {
      get => metro2 ??= new();
      set => metro2 = value;
    }

    private Metro2 metro2;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DtOf1StDelinquency.
    /// </summary>
    [JsonPropertyName("dtOf1StDelinquency")]
    public DateWorkArea DtOf1StDelinquency
    {
      get => dtOf1StDelinquency ??= new();
      set => dtOf1StDelinquency = value;
    }

    /// <summary>
    /// A value of Metro2CraBaseRecord.
    /// </summary>
    [JsonPropertyName("metro2CraBaseRecord")]
    public Metro2CraBaseRecord Metro2CraBaseRecord
    {
      get => metro2CraBaseRecord ??= new();
      set => metro2CraBaseRecord = value;
    }

    /// <summary>
    /// A value of NullDateCreditReporting.
    /// </summary>
    [JsonPropertyName("nullDateCreditReporting")]
    public AdministrativeActCertification NullDateCreditReporting
    {
      get => nullDateCreditReporting ??= new();
      set => nullDateCreditReporting = value;
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
    /// A value of ReadCsePersonAdabas.
    /// </summary>
    [JsonPropertyName("readCsePersonAdabas")]
    public AbendData ReadCsePersonAdabas
    {
      get => readCsePersonAdabas ??= new();
      set => readCsePersonAdabas = value;
    }

    /// <summary>
    /// A value of NullDateCsePersonAddress.
    /// </summary>
    [JsonPropertyName("nullDateCsePersonAddress")]
    public CsePersonAddress NullDateCsePersonAddress
    {
      get => nullDateCsePersonAddress ??= new();
      set => nullDateCsePersonAddress = value;
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

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
    }

    private DateWorkArea dtOf1StDelinquency;
    private Metro2CraBaseRecord metro2CraBaseRecord;
    private AdministrativeActCertification nullDateCreditReporting;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData readCsePersonAdabas;
    private CsePersonAddress nullDateCsePersonAddress;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea firstDayOfMonth;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}

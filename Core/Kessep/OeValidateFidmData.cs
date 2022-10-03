// Program: OE_VALIDATE_FIDM_DATA, ID: 374390446, model: 746.
// Short name: SWE02608
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VALIDATE_FIDM_DATA.
/// </para>
/// <para>
/// VALIDATE THE DATA RETURNED FROM THE FIDM FLAT FILE.
/// </para>
/// </summary>
[Serializable]
public partial class OeValidateFidmData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VALIDATE_FIDM_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeValidateFidmData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeValidateFidmData.
  /// </summary>
  public OeValidateFidmData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************
    // MAINTENANCE LOG
    // ******************************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // P.Phinney       09/04/00  H00102664 Removed validate of 
    // matched_payee_state.
    // Done because of Military Addressess and
    // inconsistancies in Address fields supplied to us.
    // P.Phinney	03/25/02  I0140394  Add Account_Status_Indicator to FIDM Entity
    // DDupree       12/28/2005  WR00258947 FCR minor release
    // Added new account type to the validation check.
    // ******************************
    // END MAINTENANCE LOG
    // **************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.FinancialInstitutionDataMatch.Assign(
      import.FinancialInstitutionDataMatch);

    // Validation for Method 2 (SWEEB427)
    // Validation for MSFIDM   (SWEEB425)
    if (Equal(global.UserId, "SWEEB427") || Equal(global.UserId, "SWEEB425"))
    {
      // MSFI Case_ID (cse_person_number)
      local.CsePersonsWorkSet.Number =
        import.FinancialInstitutionDataMatch.CsePersonNumber;
      UseSiReadCsePersonBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // MSFI Payee_Indicator (payee_indicator)
      switch(AsChar(import.FinancialInstitutionDataMatch.PayeeIndicator))
      {
        case '0':
          // * * *
          // *   Only ONE Account holder and it is our AP
          // * * *
          // MSFI Matched_Account_Primary_SSN (primary_ssn)
          export.FinancialInstitutionDataMatch.PrimarySsn =
            export.FinancialInstitutionDataMatch.MatchedPayeeSsn;

          if (Verify(export.FinancialInstitutionDataMatch.PrimarySsn,
            "0123456789") != 0)
          {
            ExitState = "OE_INVALID_PRIMARY_SSN";

            return;
          }

          break;
        case '1':
          // * * *
          // *   Multiple Account Holders - AP is Secondary
          // * * *
          // *   Primary Account holder is OPTIONAL
          // * * *
          // MSFI Matched_Account_Primary_SSN (primary_ssn)
          if (!IsEmpty(import.FinancialInstitutionDataMatch.PrimarySsn))
          {
            if (Verify(import.FinancialInstitutionDataMatch.PrimarySsn,
              "0123456789") != 0)
            {
              ExitState = "OE_INVALID_PRIMARY_SSN";

              return;
            }
          }

          // * * *
          // *   Our MATCHED AP is the SECONDARY Account holder
          // * * *
          // MSFI Matched_Account_Second_Payee_SSN (second_payee_ssn)
          export.FinancialInstitutionDataMatch.SecondPayeeSsn =
            export.FinancialInstitutionDataMatch.MatchedPayeeSsn;

          if (Verify(export.FinancialInstitutionDataMatch.SecondPayeeSsn,
            "0123456789") != 0)
          {
            ExitState = "OE_INVALID_SECONDARY_SSN";

            return;
          }

          break;
        case '2':
          // * * *
          // *   Multiple Account Holders - AP is Primary
          // * * *
          // *   Our MATCHED AP is the PRIMARY  Account holder
          // * * *
          // MSFI Matched_Account_Primary_SSN (primary_ssn)
          export.FinancialInstitutionDataMatch.PrimarySsn =
            export.FinancialInstitutionDataMatch.MatchedPayeeSsn;

          if (Verify(export.FinancialInstitutionDataMatch.PrimarySsn,
            "0123456789") != 0)
          {
            ExitState = "OE_INVALID_PRIMARY_SSN";

            return;
          }

          // * * *
          // *   Secondary Account holder is OPTIONAL
          // * * *
          // MSFI Matched_Account_Second_Payee_SSN (second_payee_ssn)
          if (!IsEmpty(import.FinancialInstitutionDataMatch.SecondPayeeSsn))
          {
            if (Verify(import.FinancialInstitutionDataMatch.SecondPayeeSsn,
              "0123456789") != 0)
            {
              ExitState = "OE_INVALID_SECONDARY_SSN";

              return;
            }
          }

          break;
        default:
          ExitState = "OE_INVALID_PAYEE_IND";

          return;
      }
    }

    // Validation for Method 2 (SWEEB427)
    // Validation for Method 1 (SWEEB428)
    if (Equal(global.UserId, "SWEEB427") || Equal(global.UserId, "SWEEB428"))
    {
      // Reporting_Agent/Transmitter_TIN (transmitter_tin)
      if (!IsEmpty(import.FinancialInstitutionDataMatch.TransmitterName))
      {
        if (Verify(import.FinancialInstitutionDataMatch.TransmitterTin,
          "0123456789") != 0)
        {
          ExitState = "OE_INVALID_TRANSMITTER_TIN";

          return;
        }
      }

      // Transmitter_State (transmitter_state)
      if (!IsEmpty(import.FinancialInstitutionDataMatch.TransmitterStreetAddress))
        
      {
        // Transmitter_State (transmitter_State)
        if (!ReadFips2())
        {
          ExitState = "OE_TRANSMITTER_STATE_INV";

          return;
        }
      }
    }

    // * * * * * * * * * * * * * *
    // Validation for ALL Methods
    // * * * * * * * * * * * * * *
    // MSFI TIN Validation
    if (Verify(import.FinancialInstitutionDataMatch.InstitutionTin, "0123456789")
      != 0)
    {
      ExitState = "OE_INVALID_TIN";

      return;
    }

    // MSFI Match Year/Month
    if (!Equal(import.FinancialInstitutionDataMatch.MatchRunDate, 1, 2, "20"))
    {
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    if (CharAt(import.FinancialInstitutionDataMatch.MatchRunDate, 5) != '0' && CharAt
      (import.FinancialInstitutionDataMatch.MatchRunDate, 5) != '1')
    {
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    if (CharAt(import.FinancialInstitutionDataMatch.MatchRunDate, 6) < '0' || CharAt
      (import.FinancialInstitutionDataMatch.MatchRunDate, 6) > '9')
    {
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    if (CharAt(import.FinancialInstitutionDataMatch.MatchRunDate, 5) == '1' && CharAt
      (import.FinancialInstitutionDataMatch.MatchRunDate, 6) != '0' && CharAt
      (import.FinancialInstitutionDataMatch.MatchRunDate, 6) != '1' && CharAt
      (import.FinancialInstitutionDataMatch.MatchRunDate, 6) != '2')
    {
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    // MSFI State (institution_State)
    if (!IsEmpty(import.FinancialInstitutionDataMatch.InstitutionState))
    {
      if (!ReadFips1())
      {
        ExitState = "OE_MFSI_STATE_INV";

        return;
      }
    }

    // Matched Account Payee State (matched_payee_state)
    // H00102664   09/04/00   PPhinney   Removed validate of 
    // matched_payee_state.
    // MSFI Matched_Account_Foreign_Country Indicator (
    // payee_foreign_country_indicator)
    switch(AsChar(import.FinancialInstitutionDataMatch.
      PayeeForeignCountryIndicator))
    {
      case '1':
        break;
      case ' ':
        break;
      default:
        ExitState = "OE_INVALID_FOREIGN_COUNTRY_IND";

        return;
    }

    // MSFI Name_Match_Flag (match_flag)
    switch(AsChar(import.FinancialInstitutionDataMatch.MatchFlag))
    {
      case '1':
        break;
      case '2':
        break;
      case '0':
        break;
      default:
        ExitState = "OE_INVALID_NAME_MATCH_FLAG";

        return;
    }

    // MSFI Trust_fund_indicator (trust_fund_indicator)
    switch(AsChar(import.FinancialInstitutionDataMatch.TrustFundIndicator))
    {
      case '0':
        break;
      case '1':
        break;
      case '2':
        break;
      case '3':
        break;
      case '4':
        break;
      case '5':
        break;
      case '6':
        break;
      default:
        ExitState = "OE_INVALID_TRUST_INDICATOR";

        return;
    }

    // MSFI Account_Balance_Indicator (account_balance_indicator)
    switch(AsChar(import.FinancialInstitutionDataMatch.AccountBalanceIndicator))
    {
      case '1':
        break;
      case '2':
        break;
      case '0':
        break;
      default:
        ExitState = "OE_INVALID_ACCOUNT_BALANCE_IND";

        return;
    }

    // **********************************************************************************************
    // 12/28/2005                    DDupree                 WR 00258947 FCR 
    // minor release
    // Added new account '06' (collateral payee accounts) as part of work order.
    // **********************************************************************************************
    // MSFI Account_Type (account_type)
    switch(TrimEnd(import.FinancialInstitutionDataMatch.AccountType))
    {
      case "00":
        break;
      case "01":
        break;
      case "04":
        break;
      case "05":
        break;
      case "06":
        break;
      case "11":
        break;
      case "12":
        break;
      case "14":
        break;
      case "16":
        break;
      case "17":
        break;
      case "18":
        break;
      default:
        ExitState = "OE_INVALID_ACCOUNT_TYPE_IND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          import.FinancialInstitutionDataMatch.InstitutionState ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          import.FinancialInstitutionDataMatch.TransmitterState ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private Common common;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }
#endregion
}

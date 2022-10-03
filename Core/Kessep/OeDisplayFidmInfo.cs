// Program: OE_DISPLAY_FIDM_INFO, ID: 374389696, model: 746.
// Short name: SWE02609
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_DISPLAY_FIDM_INFO.
/// </para>
/// <para>
/// Read FIDM information using passed in KEY.
/// </para>
/// </summary>
[Serializable]
public partial class OeDisplayFidmInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DISPLAY_FIDM_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDisplayFidmInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDisplayFidmInfo.
  /// </summary>
  public OeDisplayFidmInfo(IContext context, Import import, Export export):
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
    // P.Phinney	05/10/00   Initial Code
    // G.Vandy		07/18/00   PR 98226 - Modifications for the FIDM identifier 
    // change.
    // P. Phinney  	03/19/02   PR 140394 - IF MSFIDM - Display Account Status
    // J. Harden      09/12/2017    CQ58517 add a note line to FIDM screen.
    // **************************** END MAINTENANCE LOG 
    // ****************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ------------------------------------------------
    // Validate KEY_INFORMATION and Retrieve for Display
    // ------------------------------------------------
    if (ReadFinancialInstitutionDataMatch())
    {
      export.PassFinancialInstitutionDataMatch.Assign(
        entities.FinancialInstitutionDataMatch);
    }
    else
    {
      ExitState = "OE_FINANCIAL_INST_DATA_MATCH_NF";

      return;
    }

    // ------------------------------------------------
    // Get NAME for CSE_Person
    // ------------------------------------------------
    if (!IsEmpty(entities.FinancialInstitutionDataMatch.CsePersonNumber))
    {
      local.CsePersonsWorkSet.Number =
        entities.FinancialInstitutionDataMatch.CsePersonNumber;
      UseSiReadCsePerson();
      export.PassCsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    }

    // ------------------------------------------------
    // Convert Payee_Indicator
    // ------------------------------------------------
    switch(AsChar(entities.FinancialInstitutionDataMatch.PayeeIndicator))
    {
      case '0':
        export.PrimarySecondaryOwner1.SelectChar = "P";
        export.PrimarySecondaryOwner2.SelectChar = "";

        if (IsEmpty(export.PassFinancialInstitutionDataMatch.PrimarySsn))
        {
          export.PassFinancialInstitutionDataMatch.PrimarySsn =
            entities.FinancialInstitutionDataMatch.MatchedPayeeSsn;
        }

        break;
      case '2':
        export.PrimarySecondaryOwner1.SelectChar = "P";
        export.PrimarySecondaryOwner2.SelectChar = "S";

        if (IsEmpty(export.PassFinancialInstitutionDataMatch.PrimarySsn))
        {
          export.PassFinancialInstitutionDataMatch.PrimarySsn =
            entities.FinancialInstitutionDataMatch.MatchedPayeeSsn;
        }

        break;
      case '1':
        // P. Phinney  	06/19/00   Modify logic to make Method 1 Fidm records 
        // display correctly
        if (Equal(entities.FinancialInstitutionDataMatch.CreatedBy, "SWEEB428"))
        {
          export.PrimarySecondaryOwner1.SelectChar = "P";
          export.PrimarySecondaryOwner2.SelectChar = "S";

          if (IsEmpty(export.PassFinancialInstitutionDataMatch.PrimarySsn))
          {
            export.PassFinancialInstitutionDataMatch.PrimarySsn =
              entities.FinancialInstitutionDataMatch.MatchedPayeeSsn;
          }

          break;
        }

        export.PrimarySecondaryOwner1.SelectChar = "S";
        export.PrimarySecondaryOwner2.SelectChar = "P";

        // G.Vandy		05/25/00   Modify the data returned from the display cab
        // 			   when payee indicator='1'.
        export.PassFinancialInstitutionDataMatch.PrimarySsn =
          entities.FinancialInstitutionDataMatch.MatchedPayeeSsn;
        export.PassFinancialInstitutionDataMatch.SecondPayeeSsn =
          entities.FinancialInstitutionDataMatch.PrimarySsn;

        break;
      default:
        export.PrimarySecondaryOwner1.SelectChar = "*";
        export.PrimarySecondaryOwner2.SelectChar = "*";

        if (IsEmpty(export.PassFinancialInstitutionDataMatch.PrimarySsn))
        {
          export.PassFinancialInstitutionDataMatch.PrimarySsn =
            entities.FinancialInstitutionDataMatch.MatchedPayeeSsn;
        }

        break;
    }

    if (!IsEmpty(entities.FinancialInstitutionDataMatch.MatchFlag))
    {
      // ------------------------------------------------
      // Validate the Name/SSN Match Field
      // ------------------------------------------------
      local.Code.CodeName = "FIDM SSN/NAME MATCH";
      local.CodeValue.Cdvalue =
        entities.FinancialInstitutionDataMatch.MatchFlag ?? Spaces(10);
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.FidmResourceTypeDesc.Description =
          Substring(local.CodeValue.Description, 1, 8);
      }
      else
      {
        export.FidmResourceTypeDesc.Description =
          entities.FinancialInstitutionDataMatch.PayeeIndicator + " - is INVALID";
          
      }
    }

    // ------------------------------------------------
    // Convert Foreign Country Indicator
    // ------------------------------------------------
    if (AsChar(entities.FinancialInstitutionDataMatch.
      PayeeForeignCountryIndicator) == '1')
    {
      export.ForeignCountry.Flag = "Y";
    }
    else
    {
      export.ForeignCountry.Flag = "";
    }

    if (!IsEmpty(entities.FinancialInstitutionDataMatch.AccountType))
    {
      // ------------------------------------------------
      // Validate the Account Type
      // ------------------------------------------------
      local.Code.CodeName = "FIDM RESOURCE TYPE";
      local.CodeValue.Cdvalue =
        entities.FinancialInstitutionDataMatch.AccountType;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.FidmAccountType.Description =
          Substring(local.CodeValue.Description, 6, 31);
        export.FidmAccountType2.Description = local.CodeValue.Description;
      }
      else
      {
        export.FidmAccountType.Description =
          entities.FinancialInstitutionDataMatch.AccountType + " - is INVALID";
        export.FidmAccountType2.Description =
          export.FidmAccountType.Description;
      }
    }

    if (!IsEmpty(entities.FinancialInstitutionDataMatch.AccountBalanceIndicator))
      
    {
      // ------------------------------------------------
      // Validate the Account Balance Indicator
      // ------------------------------------------------
      local.Code.CodeName = "FIDM ACCOUNT BALANCE";
      local.CodeValue.Cdvalue =
        entities.FinancialInstitutionDataMatch.AccountBalanceIndicator ?? Spaces
        (10);
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.FidmBalanceIndicator.Description =
          Substring(local.CodeValue.Description, 1, 3);
      }
      else
      {
        export.FidmBalanceIndicator.Description = "***";
      }
    }

    if (!IsEmpty(entities.FinancialInstitutionDataMatch.TrustFundIndicator))
    {
      // ------------------------------------------------
      // Validate the Trust Fund Indicator
      // ------------------------------------------------
      local.Code.CodeName = "FIDM TRUST FUND INDICATOR";
      local.CodeValue.Cdvalue =
        entities.FinancialInstitutionDataMatch.TrustFundIndicator ?? Spaces
        (10);
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.FidmTrustFundType.Description =
          Substring(local.CodeValue.Description, 1, 26);
      }
      else
      {
        export.FidmTrustFundType.Description =
          entities.FinancialInstitutionDataMatch.TrustFundIndicator + " - is INVALID";
          
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
    local.ValidCode.Flag = useExport.ValidCode.Flag;
    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadFinancialInstitutionDataMatch()
  {
    entities.FinancialInstitutionDataMatch.Populated = false;

    return Read("ReadFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetString(command, "cseNumber", import.FidmKey.CsePersonNumber);
        db.SetString(command, "institutionTin", import.FidmKey.InstitutionTin);
        db.SetString(
          command, "matchPayAcctNum", import.FidmKey.MatchedPayeeAccountNumber);
          
        db.SetString(command, "accountType", import.FidmKey.AccountType);
        db.SetString(command, "matchRunDate", import.FidmKey.MatchRunDate);
      },
      (db, reader) =>
      {
        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.MatchedPayeeSsn =
          db.GetString(reader, 4);
        entities.FinancialInstitutionDataMatch.MatchedPayeeName =
          db.GetNullableString(reader, 5);
        entities.FinancialInstitutionDataMatch.MatchedPayeeStreetAddress =
          db.GetString(reader, 6);
        entities.FinancialInstitutionDataMatch.MatchedPayeeCity =
          db.GetNullableString(reader, 7);
        entities.FinancialInstitutionDataMatch.MatchedPayeeState =
          db.GetNullableString(reader, 8);
        entities.FinancialInstitutionDataMatch.MatchedPayeeZipCode =
          db.GetNullableString(reader, 9);
        entities.FinancialInstitutionDataMatch.MatchedPayeeZip4 =
          db.GetNullableString(reader, 10);
        entities.FinancialInstitutionDataMatch.PayeeForeignCountryIndicator =
          db.GetNullableString(reader, 11);
        entities.FinancialInstitutionDataMatch.MatchFlag =
          db.GetNullableString(reader, 12);
        entities.FinancialInstitutionDataMatch.AccountBalance =
          db.GetNullableDecimal(reader, 13);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 14);
        entities.FinancialInstitutionDataMatch.TrustFundIndicator =
          db.GetNullableString(reader, 15);
        entities.FinancialInstitutionDataMatch.AccountBalanceIndicator =
          db.GetNullableString(reader, 16);
        entities.FinancialInstitutionDataMatch.DateOfBirth =
          db.GetNullableDate(reader, 17);
        entities.FinancialInstitutionDataMatch.PayeeIndicator =
          db.GetNullableString(reader, 18);
        entities.FinancialInstitutionDataMatch.AccountFullLegalTitle =
          db.GetNullableString(reader, 19);
        entities.FinancialInstitutionDataMatch.PrimarySsn =
          db.GetNullableString(reader, 20);
        entities.FinancialInstitutionDataMatch.SecondPayeeName =
          db.GetNullableString(reader, 21);
        entities.FinancialInstitutionDataMatch.SecondPayeeSsn =
          db.GetNullableString(reader, 22);
        entities.FinancialInstitutionDataMatch.MsfidmIndicator =
          db.GetNullableString(reader, 23);
        entities.FinancialInstitutionDataMatch.InstitutionName =
          db.GetNullableString(reader, 24);
        entities.FinancialInstitutionDataMatch.InstitutionStreetAddress =
          db.GetNullableString(reader, 25);
        entities.FinancialInstitutionDataMatch.InstitutionCity =
          db.GetNullableString(reader, 26);
        entities.FinancialInstitutionDataMatch.InstitutionState =
          db.GetNullableString(reader, 27);
        entities.FinancialInstitutionDataMatch.InstitutionZipCode =
          db.GetNullableString(reader, 28);
        entities.FinancialInstitutionDataMatch.InstitutionZip4 =
          db.GetNullableString(reader, 29);
        entities.FinancialInstitutionDataMatch.SecondInstitutionName =
          db.GetNullableString(reader, 30);
        entities.FinancialInstitutionDataMatch.CreatedBy =
          db.GetNullableString(reader, 31);
        entities.FinancialInstitutionDataMatch.AccountStatusIndicator =
          db.GetNullableString(reader, 32);
        entities.FinancialInstitutionDataMatch.Note =
          db.GetNullableString(reader, 33);
        entities.FinancialInstitutionDataMatch.Populated = true;
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
    /// A value of FidmKey.
    /// </summary>
    [JsonPropertyName("fidmKey")]
    public FinancialInstitutionDataMatch FidmKey
    {
      get => fidmKey ??= new();
      set => fidmKey = value;
    }

    private FinancialInstitutionDataMatch fidmKey;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FidmBalanceIndicator.
    /// </summary>
    [JsonPropertyName("fidmBalanceIndicator")]
    public CodeValue FidmBalanceIndicator
    {
      get => fidmBalanceIndicator ??= new();
      set => fidmBalanceIndicator = value;
    }

    /// <summary>
    /// A value of FidmTrustFundType.
    /// </summary>
    [JsonPropertyName("fidmTrustFundType")]
    public CodeValue FidmTrustFundType
    {
      get => fidmTrustFundType ??= new();
      set => fidmTrustFundType = value;
    }

    /// <summary>
    /// A value of FidmAccountType2.
    /// </summary>
    [JsonPropertyName("fidmAccountType2")]
    public CodeValue FidmAccountType2
    {
      get => fidmAccountType2 ??= new();
      set => fidmAccountType2 = value;
    }

    /// <summary>
    /// A value of FidmAccountType.
    /// </summary>
    [JsonPropertyName("fidmAccountType")]
    public CodeValue FidmAccountType
    {
      get => fidmAccountType ??= new();
      set => fidmAccountType = value;
    }

    /// <summary>
    /// A value of FidmResourceTypeDesc.
    /// </summary>
    [JsonPropertyName("fidmResourceTypeDesc")]
    public CodeValue FidmResourceTypeDesc
    {
      get => fidmResourceTypeDesc ??= new();
      set => fidmResourceTypeDesc = value;
    }

    /// <summary>
    /// A value of PayeeIndicator.
    /// </summary>
    [JsonPropertyName("payeeIndicator")]
    public Common PayeeIndicator
    {
      get => payeeIndicator ??= new();
      set => payeeIndicator = value;
    }

    /// <summary>
    /// A value of ForeignCountry.
    /// </summary>
    [JsonPropertyName("foreignCountry")]
    public Common ForeignCountry
    {
      get => foreignCountry ??= new();
      set => foreignCountry = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner1.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner1")]
    public Common PrimarySecondaryOwner1
    {
      get => primarySecondaryOwner1 ??= new();
      set => primarySecondaryOwner1 = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner2.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner2")]
    public Common PrimarySecondaryOwner2
    {
      get => primarySecondaryOwner2 ??= new();
      set => primarySecondaryOwner2 = value;
    }

    /// <summary>
    /// A value of PassCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("passCsePersonsWorkSet")]
    public CsePersonsWorkSet PassCsePersonsWorkSet
    {
      get => passCsePersonsWorkSet ??= new();
      set => passCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PassFinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("passFinancialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch PassFinancialInstitutionDataMatch
    {
      get => passFinancialInstitutionDataMatch ??= new();
      set => passFinancialInstitutionDataMatch = value;
    }

    private CodeValue fidmBalanceIndicator;
    private CodeValue fidmTrustFundType;
    private CodeValue fidmAccountType2;
    private CodeValue fidmAccountType;
    private CodeValue fidmResourceTypeDesc;
    private Common payeeIndicator;
    private Common foreignCountry;
    private Common primarySecondaryOwner1;
    private Common primarySecondaryOwner2;
    private CsePersonsWorkSet passCsePersonsWorkSet;
    private FinancialInstitutionDataMatch passFinancialInstitutionDataMatch;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common returnCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}

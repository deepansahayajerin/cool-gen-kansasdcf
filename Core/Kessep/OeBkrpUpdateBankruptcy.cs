// Program: OE_BKRP_UPDATE_BANKRUPTCY, ID: 372034248, model: 746.
// Short name: SWE00867
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_BKRP_UPDATE_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block updates Bankruptcy record.
/// </para>
/// </summary>
[Serializable]
public partial class OeBkrpUpdateBankruptcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_UPDATE_BANKRUPTCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpUpdateBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpUpdateBankruptcy.
  /// </summary>
  public OeBkrpUpdateBankruptcy(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 12/17/98 R.Jean        Set new attribute Dismiss/Withdrawn date; remove 
    // extraneous views
    // ---------------------------------------------
    // ---------------------------------------------
    // User ID and timestamps for creation and update to be fixed.
    // ---------------------------------------------
    if (!ReadBankruptcy())
    {
      ExitState = "BANKRUPTCY_NF";

      return;
    }

    try
    {
      UpdateBankruptcy();
      export.Bankruptcy.Assign(entities.ExistingBankruptcy);
      export.UpdateStamp.LastUpdatedBy =
        entities.ExistingBankruptcy.LastUpdatedBy;
      export.UpdateStamp.LastUpdatedTimestamp =
        export.Bankruptcy.LastUpdatedTimestamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "BANKRUPTCY_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "BANKRUPTCY_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadBankruptcy()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", import.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingBankruptcy.CspNumber = db.GetString(reader, 0);
        entities.ExistingBankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.ExistingBankruptcy.BankruptcyCourtActionNo =
          db.GetString(reader, 2);
        entities.ExistingBankruptcy.BankruptcyType = db.GetString(reader, 3);
        entities.ExistingBankruptcy.BankruptcyFilingDate =
          db.GetDate(reader, 4);
        entities.ExistingBankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingBankruptcy.BankruptcyConfirmationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingBankruptcy.ProofOfClaimFiledDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingBankruptcy.TrusteeLastName =
          db.GetNullableString(reader, 8);
        entities.ExistingBankruptcy.TrusteeFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingBankruptcy.TrusteeMiddleInt =
          db.GetNullableString(reader, 10);
        entities.ExistingBankruptcy.TrusteeSuffix =
          db.GetNullableString(reader, 11);
        entities.ExistingBankruptcy.BtoFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.ExistingBankruptcy.BtoPhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ExistingBankruptcy.BdcPhoneAreaCode =
          db.GetNullableInt32(reader, 14);
        entities.ExistingBankruptcy.ApAttorneyFaxAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingBankruptcy.ApAttorneyPhoneAreaCode =
          db.GetNullableInt32(reader, 16);
        entities.ExistingBankruptcy.BtoPhoneExt =
          db.GetNullableString(reader, 17);
        entities.ExistingBankruptcy.BtoFaxExt =
          db.GetNullableString(reader, 18);
        entities.ExistingBankruptcy.BdcPhoneExt =
          db.GetNullableString(reader, 19);
        entities.ExistingBankruptcy.ApAttorneyFaxExt =
          db.GetNullableString(reader, 20);
        entities.ExistingBankruptcy.ApAttorneyPhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingBankruptcy.DateRequestedMotionToLift =
          db.GetNullableDate(reader, 22);
        entities.ExistingBankruptcy.DateMotionToLiftGranted =
          db.GetNullableDate(reader, 23);
        entities.ExistingBankruptcy.BtoPhoneNo =
          db.GetNullableInt32(reader, 24);
        entities.ExistingBankruptcy.BtoFax = db.GetNullableInt32(reader, 25);
        entities.ExistingBankruptcy.BtoAddrStreet1 =
          db.GetNullableString(reader, 26);
        entities.ExistingBankruptcy.BtoAddrStreet2 =
          db.GetNullableString(reader, 27);
        entities.ExistingBankruptcy.BtoAddrCity =
          db.GetNullableString(reader, 28);
        entities.ExistingBankruptcy.BtoAddrState =
          db.GetNullableString(reader, 29);
        entities.ExistingBankruptcy.BtoAddrZip5 =
          db.GetNullableString(reader, 30);
        entities.ExistingBankruptcy.BtoAddrZip4 =
          db.GetNullableString(reader, 31);
        entities.ExistingBankruptcy.BtoAddrZip3 =
          db.GetNullableString(reader, 32);
        entities.ExistingBankruptcy.BankruptcyDistrictCourt =
          db.GetString(reader, 33);
        entities.ExistingBankruptcy.BdcPhoneNo =
          db.GetNullableInt32(reader, 34);
        entities.ExistingBankruptcy.BdcAddrStreet1 =
          db.GetNullableString(reader, 35);
        entities.ExistingBankruptcy.BdcAddrStreet2 =
          db.GetNullableString(reader, 36);
        entities.ExistingBankruptcy.BdcAddrCity =
          db.GetNullableString(reader, 37);
        entities.ExistingBankruptcy.BdcAddrState =
          db.GetNullableString(reader, 38);
        entities.ExistingBankruptcy.BdcAddrZip5 =
          db.GetNullableString(reader, 39);
        entities.ExistingBankruptcy.BdcAddrZip4 =
          db.GetNullableString(reader, 40);
        entities.ExistingBankruptcy.BdcAddrZip3 =
          db.GetNullableString(reader, 41);
        entities.ExistingBankruptcy.ApAttorneyFirmName =
          db.GetNullableString(reader, 42);
        entities.ExistingBankruptcy.ApAttorneyLastName =
          db.GetNullableString(reader, 43);
        entities.ExistingBankruptcy.ApAttorneyFirstName =
          db.GetNullableString(reader, 44);
        entities.ExistingBankruptcy.ApAttorneyMi =
          db.GetNullableString(reader, 45);
        entities.ExistingBankruptcy.ApAttorneySuffix =
          db.GetNullableString(reader, 46);
        entities.ExistingBankruptcy.ApAttorneyPhoneNo =
          db.GetNullableInt32(reader, 47);
        entities.ExistingBankruptcy.ApAttorneyFax =
          db.GetNullableInt32(reader, 48);
        entities.ExistingBankruptcy.ApAttrAddrStreet1 =
          db.GetNullableString(reader, 49);
        entities.ExistingBankruptcy.ApAttrAddrStreet2 =
          db.GetNullableString(reader, 50);
        entities.ExistingBankruptcy.ApAttrAddrCity =
          db.GetNullableString(reader, 51);
        entities.ExistingBankruptcy.ApAttrAddrState =
          db.GetNullableString(reader, 52);
        entities.ExistingBankruptcy.ApAttrAddrZip5 =
          db.GetNullableString(reader, 53);
        entities.ExistingBankruptcy.ApAttrAddrZip4 =
          db.GetNullableString(reader, 54);
        entities.ExistingBankruptcy.ApAttrAddrZip3 =
          db.GetNullableString(reader, 55);
        entities.ExistingBankruptcy.CreatedBy = db.GetString(reader, 56);
        entities.ExistingBankruptcy.CreatedTimestamp =
          db.GetDateTime(reader, 57);
        entities.ExistingBankruptcy.LastUpdatedBy = db.GetString(reader, 58);
        entities.ExistingBankruptcy.LastUpdatedTimestamp =
          db.GetDateTime(reader, 59);
        entities.ExistingBankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 60);
        entities.ExistingBankruptcy.Narrative =
          db.GetNullableString(reader, 61);
        entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 62);
        entities.ExistingBankruptcy.Populated = true;
      });
  }

  private void UpdateBankruptcy()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingBankruptcy.Populated);

    var bankruptcyCourtActionNo = import.Bankruptcy.BankruptcyCourtActionNo;
    var bankruptcyType = import.Bankruptcy.BankruptcyType;
    var bankruptcyFilingDate = import.Bankruptcy.BankruptcyFilingDate;
    var bankruptcyDischargeDate = import.Bankruptcy.BankruptcyDischargeDate;
    var bankruptcyConfirmationDate =
      import.Bankruptcy.BankruptcyConfirmationDate;
    var proofOfClaimFiledDate = import.Bankruptcy.ProofOfClaimFiledDate;
    var trusteeLastName = import.Bankruptcy.TrusteeLastName ?? "";
    var trusteeFirstName = import.Bankruptcy.TrusteeFirstName ?? "";
    var trusteeMiddleInt = import.Bankruptcy.TrusteeMiddleInt ?? "";
    var trusteeSuffix = import.Bankruptcy.TrusteeSuffix ?? "";
    var btoFaxAreaCode = import.Bankruptcy.BtoFaxAreaCode.GetValueOrDefault();
    var btoPhoneAreaCode =
      import.Bankruptcy.BtoPhoneAreaCode.GetValueOrDefault();
    var bdcPhoneAreaCode =
      import.Bankruptcy.BdcPhoneAreaCode.GetValueOrDefault();
    var apAttorneyFaxAreaCode =
      import.Bankruptcy.ApAttorneyFaxAreaCode.GetValueOrDefault();
    var apAttorneyPhoneAreaCode =
      import.Bankruptcy.ApAttorneyPhoneAreaCode.GetValueOrDefault();
    var btoPhoneExt = import.Bankruptcy.BtoPhoneExt ?? "";
    var btoFaxExt = import.Bankruptcy.BtoFaxExt ?? "";
    var bdcPhoneExt = import.Bankruptcy.BdcPhoneExt ?? "";
    var apAttorneyFaxExt = import.Bankruptcy.ApAttorneyFaxExt ?? "";
    var apAttorneyPhoneExt = import.Bankruptcy.ApAttorneyPhoneExt ?? "";
    var dateRequestedMotionToLift = import.Bankruptcy.DateRequestedMotionToLift;
    var dateMotionToLiftGranted = import.Bankruptcy.DateMotionToLiftGranted;
    var btoPhoneNo = import.Bankruptcy.BtoPhoneNo.GetValueOrDefault();
    var btoFax = import.Bankruptcy.BtoFax.GetValueOrDefault();
    var btoAddrStreet1 = import.Bankruptcy.BtoAddrStreet1 ?? "";
    var btoAddrStreet2 = import.Bankruptcy.BtoAddrStreet2 ?? "";
    var btoAddrCity = import.Bankruptcy.BtoAddrCity ?? "";
    var btoAddrState = import.Bankruptcy.BtoAddrState ?? "";
    var btoAddrZip5 = import.Bankruptcy.BtoAddrZip5 ?? "";
    var btoAddrZip4 = import.Bankruptcy.BtoAddrZip4 ?? "";
    var btoAddrZip3 = import.Bankruptcy.BtoAddrZip3 ?? "";
    var bankruptcyDistrictCourt = import.Bankruptcy.BankruptcyDistrictCourt;
    var bdcPhoneNo = import.Bankruptcy.BdcPhoneNo.GetValueOrDefault();
    var bdcAddrStreet1 = import.Bankruptcy.BdcAddrStreet1 ?? "";
    var bdcAddrStreet2 = import.Bankruptcy.BdcAddrStreet2 ?? "";
    var bdcAddrCity = import.Bankruptcy.BdcAddrCity ?? "";
    var bdcAddrState = import.Bankruptcy.BdcAddrState ?? "";
    var bdcAddrZip5 = import.Bankruptcy.BdcAddrZip5 ?? "";
    var bdcAddrZip4 = import.Bankruptcy.BdcAddrZip4 ?? "";
    var bdcAddrZip3 = import.Bankruptcy.BdcAddrZip3 ?? "";
    var apAttorneyFirmName = import.Bankruptcy.ApAttorneyFirmName ?? "";
    var apAttorneyLastName = import.Bankruptcy.ApAttorneyLastName ?? "";
    var apAttorneyFirstName = import.Bankruptcy.ApAttorneyFirstName ?? "";
    var apAttorneyMi = import.Bankruptcy.ApAttorneyMi ?? "";
    var apAttorneySuffix = import.Bankruptcy.ApAttorneySuffix ?? "";
    var apAttorneyPhoneNo =
      import.Bankruptcy.ApAttorneyPhoneNo.GetValueOrDefault();
    var apAttorneyFax = import.Bankruptcy.ApAttorneyFax.GetValueOrDefault();
    var apAttrAddrStreet1 = import.Bankruptcy.ApAttrAddrStreet1 ?? "";
    var apAttrAddrStreet2 = import.Bankruptcy.ApAttrAddrStreet2 ?? "";
    var apAttrAddrCity = import.Bankruptcy.ApAttrAddrCity ?? "";
    var apAttrAddrState = import.Bankruptcy.ApAttrAddrState ?? "";
    var apAttrAddrZip5 = import.Bankruptcy.ApAttrAddrZip5 ?? "";
    var apAttrAddrZip4 = import.Bankruptcy.ApAttrAddrZip4 ?? "";
    var apAttrAddrZip3 = import.Bankruptcy.ApAttrAddrZip3 ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var expectedBkrpDischargeDate = import.Bankruptcy.ExpectedBkrpDischargeDate;
    var narrative = import.Bankruptcy.Narrative ?? "";
    var bankruptcyDismissWithdrawDate =
      import.Bankruptcy.BankruptcyDismissWithdrawDate;

    entities.ExistingBankruptcy.Populated = false;
    Update("UpdateBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "courtActionNo", bankruptcyCourtActionNo);
        db.SetString(command, "type", bankruptcyType);
        db.SetDate(command, "filingDate", bankruptcyFilingDate);
        db.SetNullableDate(command, "dischargeDate", bankruptcyDischargeDate);
        db.SetNullableDate(
          command, "confirmationDate", bankruptcyConfirmationDate);
        db.SetNullableDate(command, "prfClaimFiledDt", proofOfClaimFiledDate);
        db.SetNullableString(command, "trusteeLastName", trusteeLastName);
        db.SetNullableString(command, "trusteeFirstName", trusteeFirstName);
        db.SetNullableString(command, "trusteeMiddleInt", trusteeMiddleInt);
        db.SetNullableString(command, "trusteeSuffix", trusteeSuffix);
        db.SetNullableInt32(command, "btoFaxArea", btoFaxAreaCode);
        db.SetNullableInt32(command, "btoPhArea", btoPhoneAreaCode);
        db.SetNullableInt32(command, "bdcPhArea", bdcPhoneAreaCode);
        db.SetNullableInt32(command, "apAttrFaxArea", apAttorneyFaxAreaCode);
        db.SetNullableInt32(command, "apAttrPhArea", apAttorneyPhoneAreaCode);
        db.SetNullableString(command, "btoPhoneExt", btoPhoneExt);
        db.SetNullableString(command, "btoFaxExt", btoFaxExt);
        db.SetNullableString(command, "bdcPhoneExt", bdcPhoneExt);
        db.SetNullableString(command, "apAttrFaxExt", apAttorneyFaxExt);
        db.SetNullableString(command, "apAttrPhoneExt", apAttorneyPhoneExt);
        db.
          SetNullableDate(command, "reqMtnToLiftDt", dateRequestedMotionToLift);
          
        db.SetNullableDate(command, "motionGrantedDt", dateMotionToLiftGranted);
        db.SetNullableInt32(command, "btoPhoneNo", btoPhoneNo);
        db.SetNullableInt32(command, "btoFax", btoFax);
        db.SetNullableString(command, "btoAddrStreet1", btoAddrStreet1);
        db.SetNullableString(command, "btoAddrStreet2", btoAddrStreet2);
        db.SetNullableString(command, "btoAddrCity", btoAddrCity);
        db.SetNullableString(command, "btoAddrState", btoAddrState);
        db.SetNullableString(command, "btoAddrZip5", btoAddrZip5);
        db.SetNullableString(command, "btoAddrZip4", btoAddrZip4);
        db.SetNullableString(command, "btoAddrZip3", btoAddrZip3);
        db.SetString(command, "districtCourt", bankruptcyDistrictCourt);
        db.SetNullableInt32(command, "bdcPhoneNo", bdcPhoneNo);
        db.SetNullableString(command, "bdcAddrStreet1", bdcAddrStreet1);
        db.SetNullableString(command, "bdcAddrStreet2", bdcAddrStreet2);
        db.SetNullableString(command, "bdcAddrCity", bdcAddrCity);
        db.SetNullableString(command, "bdcAddrState", bdcAddrState);
        db.SetNullableString(command, "bdcAddrZip5", bdcAddrZip5);
        db.SetNullableString(command, "bdcAddrZip4", bdcAddrZip4);
        db.SetNullableString(command, "bdcAddrZip3", bdcAddrZip3);
        db.SetNullableString(command, "apAttrFirmName", apAttorneyFirmName);
        db.SetNullableString(command, "apAttrLastName", apAttorneyLastName);
        db.SetNullableString(command, "apAttrFirstName", apAttorneyFirstName);
        db.SetNullableString(command, "apAttorneyMi", apAttorneyMi);
        db.SetNullableString(command, "apAttorneySuffix", apAttorneySuffix);
        db.SetNullableInt32(command, "apAttrPhone", apAttorneyPhoneNo);
        db.SetNullableInt32(command, "apAttorneyFax", apAttorneyFax);
        db.SetNullableString(command, "apAttrAddrSt1", apAttrAddrStreet1);
        db.SetNullableString(command, "apAttrAddrSt2", apAttrAddrStreet2);
        db.SetNullableString(command, "apAttrAddrCity", apAttrAddrCity);
        db.SetNullableString(command, "apAttrAddrState", apAttrAddrState);
        db.SetNullableString(command, "apAttrAddrZip5", apAttrAddrZip5);
        db.SetNullableString(command, "apAttrAddrZip4", apAttrAddrZip4);
        db.SetNullableString(command, "apAttrAddrZip3", apAttrAddrZip3);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.
          SetNullableDate(command, "expBkrpDisDate", expectedBkrpDischargeDate);
          
        db.SetNullableString(command, "narrative", narrative);
        db.SetNullableDate(
          command, "bkrpDisWthdrwDt", bankruptcyDismissWithdrawDate);
        db.
          SetString(command, "cspNumber", entities.ExistingBankruptcy.CspNumber);
          
        db.SetInt32(
          command, "identifier", entities.ExistingBankruptcy.Identifier);
      });

    entities.ExistingBankruptcy.BankruptcyCourtActionNo =
      bankruptcyCourtActionNo;
    entities.ExistingBankruptcy.BankruptcyType = bankruptcyType;
    entities.ExistingBankruptcy.BankruptcyFilingDate = bankruptcyFilingDate;
    entities.ExistingBankruptcy.BankruptcyDischargeDate =
      bankruptcyDischargeDate;
    entities.ExistingBankruptcy.BankruptcyConfirmationDate =
      bankruptcyConfirmationDate;
    entities.ExistingBankruptcy.ProofOfClaimFiledDate = proofOfClaimFiledDate;
    entities.ExistingBankruptcy.TrusteeLastName = trusteeLastName;
    entities.ExistingBankruptcy.TrusteeFirstName = trusteeFirstName;
    entities.ExistingBankruptcy.TrusteeMiddleInt = trusteeMiddleInt;
    entities.ExistingBankruptcy.TrusteeSuffix = trusteeSuffix;
    entities.ExistingBankruptcy.BtoFaxAreaCode = btoFaxAreaCode;
    entities.ExistingBankruptcy.BtoPhoneAreaCode = btoPhoneAreaCode;
    entities.ExistingBankruptcy.BdcPhoneAreaCode = bdcPhoneAreaCode;
    entities.ExistingBankruptcy.ApAttorneyFaxAreaCode = apAttorneyFaxAreaCode;
    entities.ExistingBankruptcy.ApAttorneyPhoneAreaCode =
      apAttorneyPhoneAreaCode;
    entities.ExistingBankruptcy.BtoPhoneExt = btoPhoneExt;
    entities.ExistingBankruptcy.BtoFaxExt = btoFaxExt;
    entities.ExistingBankruptcy.BdcPhoneExt = bdcPhoneExt;
    entities.ExistingBankruptcy.ApAttorneyFaxExt = apAttorneyFaxExt;
    entities.ExistingBankruptcy.ApAttorneyPhoneExt = apAttorneyPhoneExt;
    entities.ExistingBankruptcy.DateRequestedMotionToLift =
      dateRequestedMotionToLift;
    entities.ExistingBankruptcy.DateMotionToLiftGranted =
      dateMotionToLiftGranted;
    entities.ExistingBankruptcy.BtoPhoneNo = btoPhoneNo;
    entities.ExistingBankruptcy.BtoFax = btoFax;
    entities.ExistingBankruptcy.BtoAddrStreet1 = btoAddrStreet1;
    entities.ExistingBankruptcy.BtoAddrStreet2 = btoAddrStreet2;
    entities.ExistingBankruptcy.BtoAddrCity = btoAddrCity;
    entities.ExistingBankruptcy.BtoAddrState = btoAddrState;
    entities.ExistingBankruptcy.BtoAddrZip5 = btoAddrZip5;
    entities.ExistingBankruptcy.BtoAddrZip4 = btoAddrZip4;
    entities.ExistingBankruptcy.BtoAddrZip3 = btoAddrZip3;
    entities.ExistingBankruptcy.BankruptcyDistrictCourt =
      bankruptcyDistrictCourt;
    entities.ExistingBankruptcy.BdcPhoneNo = bdcPhoneNo;
    entities.ExistingBankruptcy.BdcAddrStreet1 = bdcAddrStreet1;
    entities.ExistingBankruptcy.BdcAddrStreet2 = bdcAddrStreet2;
    entities.ExistingBankruptcy.BdcAddrCity = bdcAddrCity;
    entities.ExistingBankruptcy.BdcAddrState = bdcAddrState;
    entities.ExistingBankruptcy.BdcAddrZip5 = bdcAddrZip5;
    entities.ExistingBankruptcy.BdcAddrZip4 = bdcAddrZip4;
    entities.ExistingBankruptcy.BdcAddrZip3 = bdcAddrZip3;
    entities.ExistingBankruptcy.ApAttorneyFirmName = apAttorneyFirmName;
    entities.ExistingBankruptcy.ApAttorneyLastName = apAttorneyLastName;
    entities.ExistingBankruptcy.ApAttorneyFirstName = apAttorneyFirstName;
    entities.ExistingBankruptcy.ApAttorneyMi = apAttorneyMi;
    entities.ExistingBankruptcy.ApAttorneySuffix = apAttorneySuffix;
    entities.ExistingBankruptcy.ApAttorneyPhoneNo = apAttorneyPhoneNo;
    entities.ExistingBankruptcy.ApAttorneyFax = apAttorneyFax;
    entities.ExistingBankruptcy.ApAttrAddrStreet1 = apAttrAddrStreet1;
    entities.ExistingBankruptcy.ApAttrAddrStreet2 = apAttrAddrStreet2;
    entities.ExistingBankruptcy.ApAttrAddrCity = apAttrAddrCity;
    entities.ExistingBankruptcy.ApAttrAddrState = apAttrAddrState;
    entities.ExistingBankruptcy.ApAttrAddrZip5 = apAttrAddrZip5;
    entities.ExistingBankruptcy.ApAttrAddrZip4 = apAttrAddrZip4;
    entities.ExistingBankruptcy.ApAttrAddrZip3 = apAttrAddrZip3;
    entities.ExistingBankruptcy.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingBankruptcy.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingBankruptcy.ExpectedBkrpDischargeDate =
      expectedBkrpDischargeDate;
    entities.ExistingBankruptcy.Narrative = narrative;
    entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate =
      bankruptcyDismissWithdrawDate;
    entities.ExistingBankruptcy.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Bankruptcy bankruptcy;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public Bankruptcy UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    private Bankruptcy updateStamp;
    private Bankruptcy bankruptcy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingBankruptcy.
    /// </summary>
    [JsonPropertyName("existingBankruptcy")]
    public Bankruptcy ExistingBankruptcy
    {
      get => existingBankruptcy ??= new();
      set => existingBankruptcy = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private Bankruptcy existingBankruptcy;
    private CsePerson existingCsePerson;
  }
#endregion
}

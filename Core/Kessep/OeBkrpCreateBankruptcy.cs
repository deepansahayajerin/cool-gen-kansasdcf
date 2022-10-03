// Program: OE_BKRP_CREATE_BANKRUPTCY, ID: 372034232, model: 746.
// Short name: SWE00864
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_BKRP_CREATE_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates an instance of BANKRUPTCY.
/// </para>
/// </summary>
[Serializable]
public partial class OeBkrpCreateBankruptcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_CREATE_BANKRUPTCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpCreateBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpCreateBankruptcy.
  /// </summary>
  public OeBkrpCreateBankruptcy(IContext context, Import import, Export export):
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
    local.Current.Timestamp = Now();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.Last.Identifier = 0;

    if (ReadBankruptcy())
    {
      local.Last.Identifier = entities.ExistingLast.Identifier;
    }

    try
    {
      CreateBankruptcy();
      export.Bankruptcy.Assign(entities.New1);
      export.UpdateStamp.LastUpdatedBy = global.UserId;
      export.UpdateStamp.LastUpdatedTimestamp = local.Current.Timestamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_CONTENTION_IN_GENKEY";

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

  private void CreateBankruptcy()
  {
    var cspNumber = entities.Existing.Number;
    var identifier = local.Last.Identifier + 1;
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
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var expectedBkrpDischargeDate = import.Bankruptcy.ExpectedBkrpDischargeDate;
    var narrative = import.Bankruptcy.Narrative ?? "";
    var bankruptcyDismissWithdrawDate =
      import.Bankruptcy.BankruptcyDismissWithdrawDate;

    entities.New1.Populated = false;
    Update("CreateBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "discDateModInd", "");
        db.
          SetNullableDate(command, "expBkrpDisDate", expectedBkrpDischargeDate);
          
        db.SetNullableString(command, "narrative", narrative);
        db.SetNullableDate(
          command, "bkrpDisWthdrwDt", bankruptcyDismissWithdrawDate);
      });

    entities.New1.CspNumber = cspNumber;
    entities.New1.Identifier = identifier;
    entities.New1.BankruptcyCourtActionNo = bankruptcyCourtActionNo;
    entities.New1.BankruptcyType = bankruptcyType;
    entities.New1.BankruptcyFilingDate = bankruptcyFilingDate;
    entities.New1.BankruptcyDischargeDate = bankruptcyDischargeDate;
    entities.New1.BankruptcyConfirmationDate = bankruptcyConfirmationDate;
    entities.New1.ProofOfClaimFiledDate = proofOfClaimFiledDate;
    entities.New1.TrusteeLastName = trusteeLastName;
    entities.New1.TrusteeFirstName = trusteeFirstName;
    entities.New1.TrusteeMiddleInt = trusteeMiddleInt;
    entities.New1.TrusteeSuffix = trusteeSuffix;
    entities.New1.BtoFaxAreaCode = btoFaxAreaCode;
    entities.New1.BtoPhoneAreaCode = btoPhoneAreaCode;
    entities.New1.BdcPhoneAreaCode = bdcPhoneAreaCode;
    entities.New1.ApAttorneyFaxAreaCode = apAttorneyFaxAreaCode;
    entities.New1.ApAttorneyPhoneAreaCode = apAttorneyPhoneAreaCode;
    entities.New1.BtoPhoneExt = btoPhoneExt;
    entities.New1.BtoFaxExt = btoFaxExt;
    entities.New1.BdcPhoneExt = bdcPhoneExt;
    entities.New1.ApAttorneyFaxExt = apAttorneyFaxExt;
    entities.New1.ApAttorneyPhoneExt = apAttorneyPhoneExt;
    entities.New1.DateRequestedMotionToLift = dateRequestedMotionToLift;
    entities.New1.DateMotionToLiftGranted = dateMotionToLiftGranted;
    entities.New1.BtoPhoneNo = btoPhoneNo;
    entities.New1.BtoFax = btoFax;
    entities.New1.BtoAddrStreet1 = btoAddrStreet1;
    entities.New1.BtoAddrStreet2 = btoAddrStreet2;
    entities.New1.BtoAddrCity = btoAddrCity;
    entities.New1.BtoAddrState = btoAddrState;
    entities.New1.BtoAddrZip5 = btoAddrZip5;
    entities.New1.BtoAddrZip4 = btoAddrZip4;
    entities.New1.BtoAddrZip3 = btoAddrZip3;
    entities.New1.BankruptcyDistrictCourt = bankruptcyDistrictCourt;
    entities.New1.BdcPhoneNo = bdcPhoneNo;
    entities.New1.BdcAddrStreet1 = bdcAddrStreet1;
    entities.New1.BdcAddrStreet2 = bdcAddrStreet2;
    entities.New1.BdcAddrCity = bdcAddrCity;
    entities.New1.BdcAddrState = bdcAddrState;
    entities.New1.BdcAddrZip5 = bdcAddrZip5;
    entities.New1.BdcAddrZip4 = bdcAddrZip4;
    entities.New1.BdcAddrZip3 = bdcAddrZip3;
    entities.New1.ApAttorneyFirmName = apAttorneyFirmName;
    entities.New1.ApAttorneyLastName = apAttorneyLastName;
    entities.New1.ApAttorneyFirstName = apAttorneyFirstName;
    entities.New1.ApAttorneyMi = apAttorneyMi;
    entities.New1.ApAttorneySuffix = apAttorneySuffix;
    entities.New1.ApAttorneyPhoneNo = apAttorneyPhoneNo;
    entities.New1.ApAttorneyFax = apAttorneyFax;
    entities.New1.ApAttrAddrStreet1 = apAttrAddrStreet1;
    entities.New1.ApAttrAddrStreet2 = apAttrAddrStreet2;
    entities.New1.ApAttrAddrCity = apAttrAddrCity;
    entities.New1.ApAttrAddrState = apAttrAddrState;
    entities.New1.ApAttrAddrZip5 = apAttrAddrZip5;
    entities.New1.ApAttrAddrZip4 = apAttrAddrZip4;
    entities.New1.ApAttrAddrZip3 = apAttrAddrZip3;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.ExpectedBkrpDischargeDate = expectedBkrpDischargeDate;
    entities.New1.Narrative = narrative;
    entities.New1.BankruptcyDismissWithdrawDate = bankruptcyDismissWithdrawDate;
    entities.New1.Populated = true;
  }

  private bool ReadBankruptcy()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Existing.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Existing.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Bankruptcy Last
    {
      get => last ??= new();
      set => last = value;
    }

    private DateWorkArea current;
    private Bankruptcy last;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Bankruptcy New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Bankruptcy ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CsePerson Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Bankruptcy new1;
    private Bankruptcy existingLast;
    private CsePerson existing;
  }
#endregion
}

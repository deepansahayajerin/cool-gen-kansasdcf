// Program: OE_BKRP_DISPLAY_BANKRUPTCY, ID: 372034230, model: 746.
// Short name: SWE00866
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
/// A program: OE_BKRP_DISPLAY_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block reads and populates export views of Bankruptcy for 
/// display.
/// </para>
/// </summary>
[Serializable]
public partial class OeBkrpDisplayBankruptcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_DISPLAY_BANKRUPTCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpDisplayBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpDisplayBankruptcy.
  /// </summary>
  public OeBkrpDisplayBankruptcy(IContext context, Import import, Export export):
    
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
    // 12/17/98 R.Jean        Remove unneeded read od CSE Person; re-write 
    // inefficient reads; remove USE CAB GET CLIENT DETAILS; remove extraneous
    // views; add new attribute to views Dismiss/Withdrawn Date
    // 06/08/11 T. Pierce	Modify exit states returned on PREV and NEXT
    // ---------------------------------------------
    export.Bankruptcy.Assign(import.Bankruptcy);
    export.UpdateStamp.LastUpdatedBy = import.Bankruptcy.LastUpdatedBy;
    export.UpdateStamp.LastUpdatedTimestamp =
      import.Bankruptcy.LastUpdatedTimestamp;
    export.BankruptcyDisplayed.Flag = "N";

    if (IsEmpty(import.CsePerson.Number))
    {
      ExitState = "CSE_PERSON_NO_REQUIRED";

      return;
    }

    if (!IsEmpty(import.Case1.Number))
    {
      if (ReadCase())
      {
        if (ReadCsePerson1())
        {
          UseCabGetClientDetails();
        }
        else
        {
          ExitState = "OE0000_CASE_CSE_PERSON_NF";

          return;
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else if (ReadCsePerson2())
    {
      UseCabGetClientDetails();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Command DISPLAY displays given Bankruptcy rec
    // Command PREV displays previous Bankruptcy rec
    // Command NEXT displays next Bankruptcy rec
    // ---------------------------------------------
    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        local.BankruptcyFound.Flag = "N";

        foreach(var item in ReadBankruptcy5())
        {
          if (import.Bankruptcy.Identifier > 0 && entities
            .ExistingBankruptcy.Identifier > import.Bankruptcy.Identifier)
          {
            continue;
          }

          export.Bankruptcy.Assign(entities.ExistingBankruptcy);
          export.UpdateStamp.LastUpdatedBy =
            entities.ExistingBankruptcy.CreatedBy;
          export.UpdateStamp.LastUpdatedTimestamp =
            entities.ExistingBankruptcy.CreatedTimestamp;

          if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
            entities.ExistingBankruptcy.LastUpdatedTimestamp))
          {
            export.UpdateStamp.LastUpdatedBy =
              entities.ExistingBankruptcy.LastUpdatedBy;
            export.UpdateStamp.LastUpdatedTimestamp =
              entities.ExistingBankruptcy.LastUpdatedTimestamp;
          }

          local.BankruptcyFound.Flag = "Y";

          break;
        }

        if (import.Bankruptcy.Identifier == 0)
        {
          if (AsChar(local.BankruptcyFound.Flag) == 'N')
          {
            ExitState = "CO0000_NO_BANKRUPTCY_TO_DISPLAY";

            return;
          }
        }
        else if (AsChar(local.BankruptcyFound.Flag) == 'N')
        {
          ExitState = "BANKRUPTCY_NF";

          return;
        }

        break;
      case "PREV":
        local.BankruptcyFound.Flag = "N";

        if (ReadBankruptcy2())
        {
          export.Bankruptcy.Assign(entities.ExistingBankruptcy);
          export.UpdateStamp.LastUpdatedBy =
            entities.ExistingBankruptcy.CreatedBy;
          export.UpdateStamp.LastUpdatedTimestamp =
            entities.ExistingBankruptcy.CreatedTimestamp;

          if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
            entities.ExistingBankruptcy.LastUpdatedTimestamp))
          {
            export.UpdateStamp.LastUpdatedBy =
              entities.ExistingBankruptcy.LastUpdatedBy;
            export.UpdateStamp.LastUpdatedTimestamp =
              entities.ExistingBankruptcy.LastUpdatedTimestamp;
          }

          local.BankruptcyFound.Flag = "Y";
        }

        if (AsChar(local.BankruptcyFound.Flag) == 'N')
        {
          // CQ27198 T. Pierce
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        break;
      case "NEXT":
        local.BankruptcyFound.Flag = "N";

        if (ReadBankruptcy1())
        {
          export.Bankruptcy.Assign(entities.ExistingBankruptcy);
          export.UpdateStamp.LastUpdatedBy =
            entities.ExistingBankruptcy.CreatedBy;
          export.UpdateStamp.LastUpdatedTimestamp =
            entities.ExistingBankruptcy.CreatedTimestamp;

          if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
            entities.ExistingBankruptcy.LastUpdatedTimestamp))
          {
            export.UpdateStamp.LastUpdatedBy =
              entities.ExistingBankruptcy.LastUpdatedBy;
            export.UpdateStamp.LastUpdatedTimestamp =
              entities.ExistingBankruptcy.LastUpdatedTimestamp;
          }

          local.BankruptcyFound.Flag = "Y";
        }

        if (AsChar(local.BankruptcyFound.Flag) == 'N')
        {
          // CQ27198 T. Pierce
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        break;
      default:
        break;
    }

    if (AsChar(local.BankruptcyFound.Flag) == 'Y')
    {
      export.BankruptcyDisplayed.Flag = "Y";
    }

    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (ReadBankruptcy4())
    {
      export.ScrollingAttributes.MinusFlag = "-";
    }

    if (ReadBankruptcy3())
    {
      export.ScrollingAttributes.PlusFlag = "+";
    }
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadBankruptcy1()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
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

  private bool ReadBankruptcy2()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
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

  private bool ReadBankruptcy3()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadBankruptcy3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", export.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.Identifier = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private bool ReadBankruptcy4()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadBankruptcy4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", export.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.Identifier = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private IEnumerable<bool> ReadBankruptcy5()
  {
    entities.ExistingBankruptcy.Populated = false;

    return ReadEach("ReadBankruptcy5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
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

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Case1 case1;
    private Common userAction;
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
    /// A value of BankruptcyDisplayed.
    /// </summary>
    [JsonPropertyName("bankruptcyDisplayed")]
    public Common BankruptcyDisplayed
    {
      get => bankruptcyDisplayed ??= new();
      set => bankruptcyDisplayed = value;
    }

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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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

    private Common bankruptcyDisplayed;
    private Bankruptcy updateStamp;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ScrollingAttributes scrollingAttributes;
    private Bankruptcy bankruptcy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public Bankruptcy Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of BankruptcyFound.
    /// </summary>
    [JsonPropertyName("bankruptcyFound")]
    public Common BankruptcyFound
    {
      get => bankruptcyFound ??= new();
      set => bankruptcyFound = value;
    }

    private Bankruptcy init1;
    private Common bankruptcyFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingPrevOrNext.
    /// </summary>
    [JsonPropertyName("existingPrevOrNext")]
    public Bankruptcy ExistingPrevOrNext
    {
      get => existingPrevOrNext ??= new();
      set => existingPrevOrNext = value;
    }

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

    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private Bankruptcy existingPrevOrNext;
    private Bankruptcy existingBankruptcy;
    private CsePerson existingCsePerson;
  }
#endregion
}

// Program: FN_CHECK_FOR_SYSTEM_SUPPRESSION, ID: 1625344202, model: 746.
// Short name: SWE02650
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
/// A program: FN_CHECK_FOR_SYSTEM_SUPPRESSION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will be used to check to determine if a disbursement 
/// suppresion is turned on at the collection type level.  A table of the
/// suppressed collection types is returned.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForSystemSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_SYSTEM_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForSystemSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForSystemSuppression.
  /// </summary>
  public FnCheckForSystemSuppression(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 07/02/19  GVandy	CQ65423		Apply new system suppressions for Deceased
    // 					Payees and Payees without addresses.
    // 11/08/19  GVandy	CQ66641		For interstate payments, only check that an
    // 					interstate payment address has been entered
    // 					for the interstate request.  Do not check for
    // 					an ADDR address nor for Date of Death.
    // --------------------------------------------------------------------------------------------------
    local.Max.Date = new DateTime(2099, 12, 31);

    if (Equal(import.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }
    else
    {
      local.ProgramProcessingInfo.ProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
    }

    local.DisbSuppressionStatusHistory.Type1 = "";
    local.DisbSuppressionStatusHistory.ReasonText =
      Spaces(DisbSuppressionStatusHistory.ReasonText_MaxLength);

    if (!ReadCsePerson1())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePersonAccount())
    {
      ExitState = "CSE_PERSON_ACCOUNT_NF";

      return;
    }

    // -- Determine if there is a designated payee for the AR.
    //    The following checks should be for the Payee to whom warrants will be 
    // sent.
    if (ReadCsePerson2())
    {
      local.CsePerson.Assign(entities.Payee);
    }
    else
    {
      local.CsePerson.Assign(entities.Ar);
    }

    if (AsChar(import.Persistent.InterstateInd) == 'Y')
    {
      // --CQ66641 For interstate disbursements, only check that the associated 
      // interstate
      // request has a payment address.  ADDR addresses are not entered for CPs 
      // on incoming
      // interstate cases.  Also, no date of death check will be done for 
      // interstate
      // disbursements.
      if (!ReadInterstatePaymentAddress())
      {
        // -- No active address is the Z suppression type.
        local.DisbSuppressionStatusHistory.Type1 = "Z";
        local.DisbSuppressionStatusHistory.ReasonText =
          "Address was not found.";
      }
    }
    else if (Lt(local.Null1.Date, local.CsePerson.DateOfDeath))
    {
      // -- Deceased is the Y type suppression.
      local.DisbSuppressionStatusHistory.Type1 = "Y";
      local.DisbSuppressionStatusHistory.ReasonText =
        "Payee/Designated Payee is deceased";
    }
    else
    {
      UseSiGetCsePersonMailingAddr();

      if (IsEmpty(local.CsePersonAddress.LocationType))
      {
        // -- No active address is the Z suppression type.
        local.DisbSuppressionStatusHistory.Type1 = "Z";
        local.DisbSuppressionStatusHistory.ReasonText =
          "Address was not found.";
      }
      else if (Lt(local.CsePersonAddress.EndDate, Now().Date))
      {
        local.CsePersonsWorkSet.Number = local.CsePerson.Number;
        UseSiReadCsePerson();

        if (IsEmpty(local.CsePersonsWorkSet.Ssn) || !
          Lt("000000000", local.CsePersonsWorkSet.Ssn) || !
          Lt(local.Null1.Date, local.CsePersonsWorkSet.Dob))
        {
          // -- No active address is the Z suppression type.
          local.DisbSuppressionStatusHistory.Type1 = "Z";
          local.DisbSuppressionStatusHistory.ReasonText =
            "Active Address/SSN/DOB was not found";
        }
      }
    }

    if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
    {
      // --There should not be any system suppression types active.  End date 
      // any that are currently active.
      foreach(var item in ReadDisbSuppressionStatusHistory4())
      {
        try
        {
          UpdateDisbSuppressionStatusHistory1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_SUPP_STAT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_SUPP_STAT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // --There should be one of the system suppression types active.
      export.DisbSuppressionStatusHistory.Type1 =
        local.DisbSuppressionStatusHistory.Type1;
      export.DisbSuppressionStatusHistory.DiscontinueDate = local.Max.Date;
      export.DisbSuppressionStatusHistory.ReasonText =
        local.DisbSuppressionStatusHistory.ReasonText ?? "";

      // --End date any active system suppressions other than the one that 
      // should be active.
      foreach(var item in ReadDisbSuppressionStatusHistory3())
      {
        try
        {
          UpdateDisbSuppressionStatusHistory1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_SUPP_STAT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_SUPP_STAT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // --Determine if the system suppression that should be active is actually
      // active.
      if (ReadDisbSuppressionStatusHistory1())
      {
        if (!Equal(entities.DisbSuppressionStatusHistory.ReasonText,
          local.DisbSuppressionStatusHistory.ReasonText))
        {
          try
          {
            UpdateDisbSuppressionStatusHistory2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_SUPP_STAT_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_SUPP_STAT_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        ReadDisbSuppressionStatusHistory2();

        switch(AsChar(local.DisbSuppressionStatusHistory.Type1))
        {
          case 'Y':
            try
            {
              CreateDeceasedSuppression();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DISB_SUPP_STAT_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DISB_SUPP_STAT_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            break;
          case 'Z':
            try
            {
              CreateNoActiveAddressSuppression();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DISB_SUPP_STAT_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DISB_SUPP_STAT_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            break;
          default:
            break;
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.DateOfDeath = source.DateOfDeath;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.EndDate = source.EndDate;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.CsePerson);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateDeceasedSuppression()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var cpaType = entities.Obligee.Type1;
    var cspNumber = entities.Obligee.CspNumber;
    var systemGeneratedIdentifier =
      local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = local.DisbSuppressionStatusHistory.Type1;
    var reasonText = local.DisbSuppressionStatusHistory.ReasonText ?? "";

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.Deceased.Populated = false;
    Update("CreateDeceasedSuppression",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "excessUraFiller", "");
        db.SetNullableString(command, "deceasedFiller", "");
      });

    entities.Deceased.CpaType = cpaType;
    entities.Deceased.CspNumber = cspNumber;
    entities.Deceased.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Deceased.EffectiveDate = effectiveDate;
    entities.Deceased.DiscontinueDate = discontinueDate;
    entities.Deceased.CreatedBy = createdBy;
    entities.Deceased.CreatedTimestamp = createdTimestamp;
    entities.Deceased.LastUpdatedBy = "";
    entities.Deceased.LastUpdatedTmst = null;
    entities.Deceased.Type1 = type1;
    entities.Deceased.ReasonText = reasonText;
    entities.Deceased.ExcessUraFiller = "";
    entities.Deceased.DeceasedFiller = "";
    entities.Deceased.Populated = true;
  }

  private void CreateNoActiveAddressSuppression()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var cpaType = entities.Obligee.Type1;
    var cspNumber = entities.Obligee.CspNumber;
    var systemGeneratedIdentifier =
      local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = local.DisbSuppressionStatusHistory.Type1;
    var reasonText = local.DisbSuppressionStatusHistory.ReasonText ?? "";

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.NoAddress.Populated = false;
    Update("CreateNoActiveAddressSuppression",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "excessUraFiller", "");
        db.SetNullableString(command, "noAddressFiller", "");
      });

    entities.NoAddress.CpaType = cpaType;
    entities.NoAddress.CspNumber = cspNumber;
    entities.NoAddress.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.NoAddress.EffectiveDate = effectiveDate;
    entities.NoAddress.DiscontinueDate = discontinueDate;
    entities.NoAddress.CreatedBy = createdBy;
    entities.NoAddress.CreatedTimestamp = createdTimestamp;
    entities.NoAddress.LastUpdatedBy = "";
    entities.NoAddress.LastUpdatedTmst = null;
    entities.NoAddress.Type1 = type1;
    entities.NoAddress.ReasonText = reasonText;
    entities.NoAddress.ExcessUraFiller = "";
    entities.NoAddress.NoActiveAddressFiller = "";
    entities.NoAddress.Populated = true;
  }

  private bool ReadCsePerson1()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Payee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", entities.Ar.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Payee.Number = db.GetString(reader, 0);
        entities.Payee.Type1 = db.GetString(reader, 1);
        entities.Payee.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Payee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Payee.Type1);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.Obligee.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetString(command, "type", local.DisbSuppressionStatusHistory.Type1);
        db.SetDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    local.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
      },
      (db, reader) =>
      {
        local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        local.DisbSuppressionStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistory3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistory3",
      (db, command) =>
      {
        db.SetString(command, "type", local.DisbSuppressionStatusHistory.Type1);
        db.SetDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistory4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistory4",
      (db, command) =>
      {
        db.SetDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "addressStartDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId",
          import.Persistent.IntInterId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 4);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 5);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 6);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
      });
  }

  private void UpdateDisbSuppressionStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var discontinueDate = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.LastUpdatedBy = lastUpdatedBy;
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void UpdateDisbSuppressionStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var reasonText = local.DisbSuppressionStatusHistory.ReasonText ?? "";

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.LastUpdatedBy = lastUpdatedBy;
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbSuppressionStatusHistory.ReasonText = reasonText;
    entities.DisbSuppressionStatusHistory.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DisbursementTransaction Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private CsePerson ar;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementTransaction persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbSuppressionStatusHistory Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DisbSuppressionStatusHistory initialized;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson csePerson;
    private DateWorkArea null1;
    private CsePersonAddress csePersonAddress;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
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
    /// A value of NoAddress.
    /// </summary>
    [JsonPropertyName("noAddress")]
    public DisbSuppressionStatusHistory NoAddress
    {
      get => noAddress ??= new();
      set => noAddress = value;
    }

    /// <summary>
    /// A value of Deceased.
    /// </summary>
    [JsonPropertyName("deceased")]
    public DisbSuppressionStatusHistory Deceased
    {
      get => deceased ??= new();
      set => deceased = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateRequest interstateRequest;
    private DisbSuppressionStatusHistory noAddress;
    private DisbSuppressionStatusHistory deceased;
    private CsePerson payee;
    private CsePerson ar;
    private CsePersonAccount obligee;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonDesigPayee csePersonDesigPayee;
  }
#endregion
}

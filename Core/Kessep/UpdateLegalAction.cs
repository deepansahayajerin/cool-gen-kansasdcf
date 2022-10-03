// Program: UPDATE_LEGAL_ACTION, ID: 371985177, model: 746.
// Short name: SWE01485
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
/// A program: UPDATE_LEGAL_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates LEGAL ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateLegalAction.
  /// </summary>
  public UpdateLegalAction(IContext context, Import import, Export export):
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
    // 04/27/95  Dave Allen			Initial Code
    // 02/11/98  Rod Grey			If legal action is end dated, discontinue the
    // 					legal action assignment.
    // 09/11/00  GVandy	PR102972	Do not allow tribunal update.
    // 02/06/02  GVandy	WR010505	Set new attribute FILED_DT_ENTRED_ON.
    // 09/12/03  GVandy	PR186785	Set date_patern_estab when a legal action
    // 					containing an EP legal detail is filed.
    // 05/10/17  GVandy	CQ48108		IV-D PEP changes.
    // ---------------------------------------------------------------------------------------------
    export.LegalAction.Assign(import.LegalAction);
    local.CurrentDate.Date = Now().Date;
    local.CurrentDate.Timestamp = Now();
    local.Datenum.Date = null;
    local.MaximumDate.Date = UseCabSetMaximumDiscontinueDate();
    local.Oblg.Flag = "N";

    if (Equal(import.LegalAction.EndDate, local.Datenum.Date))
    {
      local.LegalAction.EndDate = local.MaximumDate.Date;
    }
    else
    {
      local.LegalAction.EndDate = import.LegalAction.EndDate;
    }

    if (Equal(import.LegalAction.LastModificationReviewDate, local.Datenum.Date))
      
    {
      local.LegalAction.LastModificationReviewDate = local.MaximumDate.Date;
    }
    else
    {
      local.LegalAction.LastModificationReviewDate =
        import.LegalAction.LastModificationReviewDate;
    }

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (!Equal(entities.LegalAction.FiledDate, import.LegalAction.FiledDate) &&
      !Equal(import.LegalAction.FiledDate, local.Datenum.Date))
    {
      export.FiledDateChanged.Flag = "Y";
    }

    // --  02/06/02  GVandy  WR010505  Set new attribute FILED_DT_ENTRED_ON.  If
    // the filed date was
    // previously populated then do not change the FILED_DT_ENTRED_ON.
    if (Equal(entities.LegalAction.FiledDate, local.Null1.Date))
    {
      if (Equal(import.LegalAction.FiledDate, local.Null1.Date))
      {
        // -- No previous filed date and no filed date entered now.  Set the 
        // filed date entered on to null.
        local.LegalAction.FiledDtEntredOn = local.Null1.Date;
      }
      else if (Equal(entities.LegalAction.FiledDtEntredOn, local.Null1.Date))
      {
        // -- No previous filed date but a filed date is entered now.  Set the 
        // filed date entered on to today.
        local.LegalAction.FiledDtEntredOn = local.CurrentDate.Date;
      }
      else
      {
        // -- This scenario is that the user entered a filed date, removed the 
        // filed date in a subsequent month, and is now adding a filed date
        // again.  Do not change the filed date
        // entered on value because the legal action was already reported during
        // the month the filed date was originally entered.
        local.LegalAction.FiledDtEntredOn =
          entities.LegalAction.FiledDtEntredOn;
      }
    }
    else if (Equal(import.LegalAction.FiledDate, local.Null1.Date))
    {
      // -- If the filed_date is removed in the same calendar month in which it 
      // was entered then set the
      // filed date entered on to null.  Otherwise do not change the filed date 
      // entered on since the
      // legal action will already have been reported.
      if (Month(local.CurrentDate.Date) == Month
        (entities.LegalAction.FiledDtEntredOn) && Year
        (local.CurrentDate.Date) == Year(entities.LegalAction.FiledDtEntredOn))
      {
        local.LegalAction.FiledDtEntredOn = local.Null1.Date;
      }
      else
      {
        local.LegalAction.FiledDtEntredOn =
          entities.LegalAction.FiledDtEntredOn;
      }
    }
    else
    {
      // -- A previous filed date was entered and there is still a filed date.  
      // Do not change the filed date
      // entered on value.
      local.LegalAction.FiledDtEntredOn = entities.LegalAction.FiledDtEntredOn;
    }

    // -- 09/12/2003 GVandy PR 186785 Set the date paternity established to the 
    // filed date
    //    of J class legal actions which contain an 'EP' legal detail.
    if (!Equal(import.LegalAction.FiledDate, local.Null1.Date) && !
      Equal(import.LegalAction.FiledDate, entities.LegalAction.FiledDate) && AsChar
      (import.LegalAction.Classification) == 'J')
    {
      foreach(var item in ReadLegalActionDetail2())
      {
        foreach(var item1 in ReadLegalActionPersonCsePerson1())
        {
          local.CsePerson.Assign(entities.CsePerson);

          // -- Raise the ZCS event.
          local.Infrastructure.UserId = "LACT";
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.EventId = 45;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Infrastructure.BusinessObjectCd = "LEA";
          local.Infrastructure.ReasonCode = "OBLGZCSRECKS";
          local.Infrastructure.Detail =
            "ZCS OBLIGATION TRANSACTION CREATED FOR AP: " + entities
            .CsePerson.Number;
          local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
          UseOeCabRaiseEvent();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

            return;
          }
        }
      }

      foreach(var item in ReadLegalActionDetail1())
      {
        foreach(var item1 in ReadLegalActionPersonCsePerson2())
        {
          local.CsePerson.Assign(entities.CsePerson);

          // -- Set cse to estab = N if the legal action filed date is prior to 
          // the case open date.
          if (ReadCase())
          {
            local.CsePerson.CseToEstblPaternity = "N";
          }

          // -- Verify that setting the paternity established indicator to 'Y' 
          // will
          //    not result in an invalid combination of born out of wedlock, cse
          // to
          //    establish paternity, and paternity established indicators.
          local.CsePerson.PaternityEstablishedIndicator = "Y";
          UseSiCabValidatePaternityComb();

          if (IsExitState("SI0000_INV_PATERNITY_IND_COMB"))
          {
            ExitState = "LE0000_INVALID_PATERNITY_FLAGS";

            return;
          }

          // -- 05/10/17 GVandy CQ48108 (IV-D PEP changes) Remove edit for 
          // multiple APs.
          // -- Set the paternity established indicator to 'Y' and the date 
          // paternity
          //    established to the filed date of the legal action.
          try
          {
            UpdateCsePerson();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // -- Raise the paternity established event.
          local.Infrastructure.EventId = 11;
          local.Infrastructure.ReasonCode = "PATESTAB";
          local.Infrastructure.Detail =
            "Journal Entry establishing paternity filed on " + NumberToString
            (Month(import.LegalAction.FiledDate), 14, 2) + NumberToString
            (Day(import.LegalAction.FiledDate), 14, 2) + NumberToString
            (Year(import.LegalAction.FiledDate), 12, 4);
          UseSiChdsRaiseEvent();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

            return;
          }
        }
      }
    }

    try
    {
      UpdateLegalAction1();
      export.LegalAction.Assign(entities.LegalAction);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "LEGAL_ACTION_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (Equal(export.LegalAction.LastModificationReviewDate,
      local.MaximumDate.Date))
    {
      export.LegalAction.LastModificationReviewDate = local.Datenum.Date;
    }

    if (Equal(export.LegalAction.EndDate, local.MaximumDate.Date))
    {
      export.LegalAction.EndDate = local.Datenum.Date;
    }
    else
    {
      // ********************************************
      // RCG  02/11/98	If the legal action is end dated, discontinue the legal 
      // action assignment.
      // ********************************************
      if (Lt(export.LegalAction.EndDate, local.CurrentDate.Date))
      {
        // *******************************************
        // Assumption is that this legal action was end dated on a previous 
        // update.
        // Only update legal action assignment if end dated on this legal action
        // update.
        // *******************************************
      }
      else
      {
        foreach(var item in ReadLegalActionAssigment())
        {
          try
          {
            UpdateLegalActionAssigment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    if (ReadCsePerson1())
    {
      if (Equal(entities.ExistingCurrentAltBillAddr.Number,
        import.AltBillingLocn.Number))
      {
      }
      else
      {
        DisassociateLegalAction();

        if (!IsEmpty(import.AltBillingLocn.Number))
        {
          if (ReadCsePerson3())
          {
            UseFnCabCheckAltAddr();

            if (IsExitState("FN0000_ALTERNATE_ADD_NF"))
            {
              return;
            }

            AssociateLegalAction();
          }
          else
          {
            ExitState = "LE0000_ALT_BILL_LOCN_CSE_PERS_NF";
          }
        }
      }
    }
    else if (!IsEmpty(import.AltBillingLocn.Number))
    {
      if (ReadCsePerson3())
      {
        UseFnCabCheckAltAddr();

        if (IsExitState("FN0000_ALTERNATE_ADD_NF"))
        {
          return;
        }

        AssociateLegalAction();
      }
      else
      {
        ExitState = "LE0000_ALT_BILL_LOCN_CSE_PERS_NF";
      }
    }

    foreach(var item in ReadObligation())
    {
      if (ReadCsePerson2())
      {
        if (Equal(entities.ExistingCurrentAltBillAddr.Number,
          import.AltBillingLocn.Number))
        {
        }
        else
        {
          DisassociateCsePerson();

          if (!IsEmpty(import.AltBillingLocn.Number))
          {
            if (ReadCsePerson3())
            {
              UseFnCabCheckAltAddr();

              if (IsExitState("FN0000_ALTERNATE_ADD_NF"))
              {
                return;
              }

              AssociateCsePerson();
            }
          }
        }
      }
      else if (!IsEmpty(import.AltBillingLocn.Number))
      {
        if (ReadCsePerson3())
        {
          UseFnCabCheckAltAddr();

          if (IsExitState("FN0000_ALTERNATE_ADD_NF"))
          {
            return;
          }

          AssociateCsePerson();
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Alternate.Number = import.AltBillingLocn.Number;
    useImport.Current.Date = local.CurrentDate.Date;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseSiCabValidatePaternityComb()
  {
    var useImport = new SiCabValidatePaternityComb.Import();
    var useExport = new SiCabValidatePaternityComb.Export();

    useImport.CsePerson.Assign(local.CsePerson);

    Call(SiCabValidatePaternityComb.Execute, useImport, useExport);
  }

  private void UseSiChdsRaiseEvent()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    useImport.Ch.Number = entities.CsePerson.Number;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);
  }

  private void AssociateCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cspPNumber = entities.ExistingNewAltBillLocn.Number;

    entities.Obligation.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.CspPNumber = cspPNumber;
    entities.Obligation.Populated = true;
  }

  private void AssociateLegalAction()
  {
    var cspNumber = entities.ExistingNewAltBillLocn.Number;

    entities.LegalAction.Populated = false;
    Update("AssociateLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.CspNumber = cspNumber;
    entities.LegalAction.Populated = true;
  }

  private void DisassociateCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cpaType = entities.Obligation.CpaType;
    var cspNumber = entities.Obligation.CspNumber;

    entities.Obligation.Populated = false;

    bool exists;

    Update("DisassociateCsePerson#1",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNumber1", cspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    exists = Read("DisassociateCsePerson#2",
      (db, command) =>
      {
        db.SetString(command, "cpaType2", cpaType);
        db.SetString(command, "cspNumber2", cspNumber);
      },
      null);

    if (!exists)
    {
      Update("DisassociateCsePerson#3",
        (db, command) =>
        {
          db.SetString(command, "cpaType2", cpaType);
          db.SetString(command, "cspNumber2", cspNumber);
        });
    }

    entities.Obligation.CspPNumber = null;
    entities.Obligation.Populated = true;
  }

  private void DisassociateLegalAction()
  {
    entities.LegalAction.Populated = false;
    Update("DisassociateLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.CspNumber = null;
    entities.LegalAction.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "filedDate",
          export.LegalAction.FiledDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.ExistingCurrentAltBillAddr.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCurrentAltBillAddr.Number = db.GetString(reader, 0);
        entities.ExistingCurrentAltBillAddr.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ExistingCurrentAltBillAddr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspPNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCurrentAltBillAddr.Number = db.GetString(reader, 0);
        entities.ExistingCurrentAltBillAddr.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ExistingNewAltBillLocn.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.AltBillingLocn.Number);
      },
      (db, reader) =>
      {
        entities.ExistingNewAltBillLocn.Number = db.GetString(reader, 0);
        entities.ExistingNewAltBillLocn.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 28);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 29);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 30);
        entities.LegalAction.ForeignFipsState = db.GetNullableInt32(reader, 31);
        entities.LegalAction.ForeignFipsCounty =
          db.GetNullableInt32(reader, 32);
        entities.LegalAction.ForeignFipsLocation =
          db.GetNullableInt32(reader, 33);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 34);
        entities.LegalAction.NonCsePetitioner =
          db.GetNullableString(reader, 35);
        entities.LegalAction.DateNonCpReqIwoBegin =
          db.GetNullableDate(reader, 36);
        entities.LegalAction.DateCpReqIwoBegin = db.GetNullableDate(reader, 37);
        entities.LegalAction.CtOrdAltBillingAddrInd =
          db.GetNullableString(reader, 38);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 39);
        entities.LegalAction.InitiatingCountry =
          db.GetNullableString(reader, 40);
        entities.LegalAction.RespondingCountry =
          db.GetNullableString(reader, 41);
        entities.LegalAction.FiledDtEntredOn = db.GetNullableDate(reader, 42);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigment()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDt", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.LgaIdentifier = db.GetNullableInt32(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.ReasonCode = db.GetString(reader, 3);
        entities.Existing.OverrideInd = db.GetString(reader, 4);
        entities.Existing.CreatedBy = db.GetString(reader, 5);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Existing.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 10);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 11);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 12);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 13);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 10);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 11);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 12);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 13);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var cseToEstblPaternity = local.CsePerson.CseToEstblPaternity ?? "";
    var paternityEstablishedIndicator = "Y";
    var datePaternEstab = import.LegalAction.FiledDate;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "cseToEstPatr", cseToEstblPaternity);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
        db.SetDate(command, "datePaternEstab", datePaternEstab);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.CseToEstblPaternity = cseToEstblPaternity;
    entities.CsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.CsePerson.DatePaternEstab = datePaternEstab;
    entities.CsePerson.Populated = true;
  }

  private void UpdateLegalAction1()
  {
    var lastModificationReviewDate =
      local.LegalAction.LastModificationReviewDate;
    var attorneyApproval = import.LegalAction.AttorneyApproval ?? "";
    var approvalSentDate = import.LegalAction.ApprovalSentDate;
    var petitionerApproval = import.LegalAction.PetitionerApproval ?? "";
    var approvalReceivedDate = import.LegalAction.ApprovalReceivedDate;
    var classification = import.LegalAction.Classification;
    var actionTaken = import.LegalAction.ActionTaken;
    var type1 = import.LegalAction.Type1;
    var filedDate = import.LegalAction.FiledDate;
    var foreignOrderRegistrationDate =
      import.LegalAction.ForeignOrderRegistrationDate;
    var uresaSentDate = import.LegalAction.UresaSentDate;
    var uresaAcknowledgedDate = import.LegalAction.UresaAcknowledgedDate;
    var uifsaSentDate = import.LegalAction.UifsaSentDate;
    var uifsaAcknowledgedDate = import.LegalAction.UifsaAcknowledgedDate;
    var initiatingState = import.LegalAction.InitiatingState ?? "";
    var initiatingCounty = import.LegalAction.InitiatingCounty ?? "";
    var respondingState = import.LegalAction.RespondingState ?? "";
    var respondingCounty = import.LegalAction.RespondingCounty ?? "";
    var orderAuthority = import.LegalAction.OrderAuthority;
    var courtCaseNumber = import.LegalAction.CourtCaseNumber ?? "";
    var refileDate = import.LegalAction.RefileDate;
    var endDate = local.LegalAction.EndDate;
    var paymentLocation = import.LegalAction.PaymentLocation ?? "";
    var dismissedWithoutPrejudiceInd =
      import.LegalAction.DismissedWithoutPrejudiceInd ?? "";
    var standardNumber = import.LegalAction.StandardNumber ?? "";
    var longArmStatuteIndicator =
      import.LegalAction.LongArmStatuteIndicator ?? "";
    var dismissalCode = import.LegalAction.DismissalCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = local.CurrentDate.Timestamp;
    var establishmentCode = import.LegalAction.EstablishmentCode ?? "";
    var foreignFipsState =
      import.LegalAction.ForeignFipsState.GetValueOrDefault();
    var foreignFipsCounty =
      import.LegalAction.ForeignFipsCounty.GetValueOrDefault();
    var foreignFipsLocation =
      import.LegalAction.ForeignFipsLocation.GetValueOrDefault();
    var foreignOrderNumber = import.LegalAction.ForeignOrderNumber ?? "";
    var nonCsePetitioner = import.LegalAction.NonCsePetitioner ?? "";
    var dateNonCpReqIwoBegin = import.LegalAction.DateNonCpReqIwoBegin;
    var dateCpReqIwoBegin = import.LegalAction.DateCpReqIwoBegin;
    var ctOrdAltBillingAddrInd = import.LegalAction.CtOrdAltBillingAddrInd ?? ""
      ;
    var initiatingCountry = import.LegalAction.InitiatingCountry ?? "";
    var respondingCountry = import.LegalAction.RespondingCountry ?? "";
    var filedDtEntredOn = local.LegalAction.FiledDtEntredOn;

    entities.LegalAction.Populated = false;
    Update("UpdateLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "lastModReviewDt", lastModificationReviewDate);
        db.SetNullableString(command, "attorneyApproval", attorneyApproval);
        db.SetNullableDate(command, "approvalSentDt", approvalSentDate);
        db.SetNullableString(command, "petitionerApprval", petitionerApproval);
        db.SetNullableDate(command, "approvalRecdDt", approvalReceivedDate);
        db.SetString(command, "classification", classification);
        db.SetString(command, "actionTaken", actionTaken);
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "filedDt", filedDate);
        db.SetNullableDate(
          command, "foreignOrdRegDt", foreignOrderRegistrationDate);
        db.SetNullableDate(command, "uresaSentDt", uresaSentDate);
        db.SetNullableDate(command, "uresaAcknowldgDt", uresaAcknowledgedDate);
        db.SetNullableDate(command, "uifsaSentDt", uifsaSentDate);
        db.SetNullableDate(command, "uifsaAcknowldgDt", uifsaAcknowledgedDate);
        db.SetNullableString(command, "initiatingState", initiatingState);
        db.SetNullableString(command, "initiatingCounty", initiatingCounty);
        db.SetNullableString(command, "respondingState", respondingState);
        db.SetNullableString(command, "respondingCounty", respondingCounty);
        db.SetString(command, "orderAuthority", orderAuthority);
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableDate(command, "refileDt", refileDate);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "paymentLocation", paymentLocation);
        db.SetNullableString(
          command, "dismissedInd", dismissedWithoutPrejudiceInd);
        db.SetNullableString(command, "standardNo", standardNumber);
        db.
          SetNullableString(command, "longArmStatInd", longArmStatuteIndicator);
          
        db.SetNullableString(command, "dismissalCd", dismissalCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "establishmentCd", establishmentCode);
        db.SetNullableInt32(command, "foreignFipsSt", foreignFipsState);
        db.SetNullableInt32(command, "foreignFipsCount", foreignFipsCounty);
        db.SetNullableInt32(command, "foreignFipsLo", foreignFipsLocation);
        db.SetNullableString(command, "foreignOrderNo", foreignOrderNumber);
        db.SetNullableString(command, "nonCsePetitioner", nonCsePetitioner);
        db.SetNullableDate(command, "dtNcpReqIwoBgn", dateNonCpReqIwoBegin);
        db.SetNullableDate(command, "dtCpReqIwoBgn", dateCpReqIwoBegin);
        db.SetNullableString(command, "ctOrdAltBaInd", ctOrdAltBillingAddrInd);
        db.SetNullableString(command, "initiatingCountry", initiatingCountry);
        db.SetNullableString(command, "respondingCountry", respondingCountry);
        db.SetNullableDate(command, "filedDtEntredOn", filedDtEntredOn);
        db.SetInt32(command, "legalActionId", entities.LegalAction.Identifier);
      });

    entities.LegalAction.LastModificationReviewDate =
      lastModificationReviewDate;
    entities.LegalAction.AttorneyApproval = attorneyApproval;
    entities.LegalAction.ApprovalSentDate = approvalSentDate;
    entities.LegalAction.PetitionerApproval = petitionerApproval;
    entities.LegalAction.ApprovalReceivedDate = approvalReceivedDate;
    entities.LegalAction.Classification = classification;
    entities.LegalAction.ActionTaken = actionTaken;
    entities.LegalAction.Type1 = type1;
    entities.LegalAction.FiledDate = filedDate;
    entities.LegalAction.ForeignOrderRegistrationDate =
      foreignOrderRegistrationDate;
    entities.LegalAction.UresaSentDate = uresaSentDate;
    entities.LegalAction.UresaAcknowledgedDate = uresaAcknowledgedDate;
    entities.LegalAction.UifsaSentDate = uifsaSentDate;
    entities.LegalAction.UifsaAcknowledgedDate = uifsaAcknowledgedDate;
    entities.LegalAction.InitiatingState = initiatingState;
    entities.LegalAction.InitiatingCounty = initiatingCounty;
    entities.LegalAction.RespondingState = respondingState;
    entities.LegalAction.RespondingCounty = respondingCounty;
    entities.LegalAction.OrderAuthority = orderAuthority;
    entities.LegalAction.CourtCaseNumber = courtCaseNumber;
    entities.LegalAction.RefileDate = refileDate;
    entities.LegalAction.EndDate = endDate;
    entities.LegalAction.PaymentLocation = paymentLocation;
    entities.LegalAction.DismissedWithoutPrejudiceInd =
      dismissedWithoutPrejudiceInd;
    entities.LegalAction.StandardNumber = standardNumber;
    entities.LegalAction.LongArmStatuteIndicator = longArmStatuteIndicator;
    entities.LegalAction.DismissalCode = dismissalCode;
    entities.LegalAction.LastUpdatedBy = lastUpdatedBy;
    entities.LegalAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalAction.EstablishmentCode = establishmentCode;
    entities.LegalAction.ForeignFipsState = foreignFipsState;
    entities.LegalAction.ForeignFipsCounty = foreignFipsCounty;
    entities.LegalAction.ForeignFipsLocation = foreignFipsLocation;
    entities.LegalAction.ForeignOrderNumber = foreignOrderNumber;
    entities.LegalAction.NonCsePetitioner = nonCsePetitioner;
    entities.LegalAction.DateNonCpReqIwoBegin = dateNonCpReqIwoBegin;
    entities.LegalAction.DateCpReqIwoBegin = dateCpReqIwoBegin;
    entities.LegalAction.CtOrdAltBillingAddrInd = ctOrdAltBillingAddrInd;
    entities.LegalAction.InitiatingCountry = initiatingCountry;
    entities.LegalAction.RespondingCountry = respondingCountry;
    entities.LegalAction.FiledDtEntredOn = filedDtEntredOn;
    entities.LegalAction.Populated = true;
  }

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = export.LegalAction.EndDate;
    var lastUpdatedBy = "LACT1485";
    var lastUpdatedTimestamp = local.CurrentDate.Timestamp;

    entities.Existing.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Existing.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Existing.DiscontinueDate = discontinueDate;
    entities.Existing.LastUpdatedBy = lastUpdatedBy;
    entities.Existing.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Existing.Populated = true;
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
    /// A value of AltBillingLocn.
    /// </summary>
    [JsonPropertyName("altBillingLocn")]
    public CsePersonsWorkSet AltBillingLocn
    {
      get => altBillingLocn ??= new();
      set => altBillingLocn = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Tribunal Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private CsePersonsWorkSet altBillingLocn;
    private Tribunal tribunal;
    private Tribunal hidden;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FiledDateChanged.
    /// </summary>
    [JsonPropertyName("filedDateChanged")]
    public Common FiledDateChanged
    {
      get => filedDateChanged ??= new();
      set => filedDateChanged = value;
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

    private Common filedDateChanged;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of ActiveApOnCase.
    /// </summary>
    [JsonPropertyName("activeApOnCase")]
    public Common ActiveApOnCase
    {
      get => activeApOnCase ??= new();
      set => activeApOnCase = value;
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
    /// A value of Oblg.
    /// </summary>
    [JsonPropertyName("oblg")]
    public Common Oblg
    {
      get => oblg ??= new();
      set => oblg = value;
    }

    /// <summary>
    /// A value of Datenum.
    /// </summary>
    [JsonPropertyName("datenum")]
    public DateWorkArea Datenum
    {
      get => datenum ??= new();
      set => datenum = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of MaximumDate.
    /// </summary>
    [JsonPropertyName("maximumDate")]
    public DateWorkArea MaximumDate
    {
      get => maximumDate ??= new();
      set => maximumDate = value;
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

    private Infrastructure infrastructure;
    private Common activeApOnCase;
    private CsePerson csePerson;
    private DateWorkArea null1;
    private Common oblg;
    private DateWorkArea datenum;
    private DateWorkArea currentDate;
    private DateWorkArea maximumDate;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public LegalActionAssigment Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingNewAltBillLocn.
    /// </summary>
    [JsonPropertyName("existingNewAltBillLocn")]
    public CsePerson ExistingNewAltBillLocn
    {
      get => existingNewAltBillLocn ??= new();
      set => existingNewAltBillLocn = value;
    }

    /// <summary>
    /// A value of ExistingCurrentAltBillAddr.
    /// </summary>
    [JsonPropertyName("existingCurrentAltBillAddr")]
    public CsePerson ExistingCurrentAltBillAddr
    {
      get => existingCurrentAltBillAddr ??= new();
      set => existingCurrentAltBillAddr = value;
    }

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public Tribunal ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Tribunal New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private LegalActionAssigment existing;
    private CsePerson existingNewAltBillLocn;
    private CsePerson existingCurrentAltBillAddr;
    private Tribunal existingCurrent;
    private Tribunal new1;
    private LegalAction legalAction;
  }
#endregion
}

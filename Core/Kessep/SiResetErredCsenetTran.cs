// Program: SI_RESET_ERRED_CSENET_TRAN, ID: 373440605, model: 746.
// Short name: SWE02754
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RESET_ERRED_CSENET_TRAN.
/// </summary>
[Serializable]
public partial class SiResetErredCsenetTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RESET_ERRED_CSENET_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiResetErredCsenetTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiResetErredCsenetTran.
  /// </summary>
  public SiResetErredCsenetTran(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Date		Developer	Request		Description
    // ----------------------------------------------------------------
    // 05/24/2002	M Ramirez	WR010502 Seg C	Init Dev
    // ----------------------------------------------------------------
    if (Lt(local.Current.Date, import.Current.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    if (ReadInterstateCase())
    {
      // ------------------------------------------------------------------------
      // Control count initialization
      // ------------------------------------------------------------------------
      MoveInterstateCase4(entities.PreviousInterstateCase, export.Controls);
    }
    else
    {
      export.WriteError.Flag = "Y";
      export.AbendRollbackRequired.Flag = "A";
      export.Error.RptDetail = "Previous INTERSTATE_CASE not found";

      return;
    }

    if (Length(TrimEnd(entities.PreviousInterstateCase.KsCaseId)) == 15)
    {
      local.Case1.Number =
        Substring(entities.PreviousInterstateCase.KsCaseId, 6, 10);
    }
    else
    {
      local.Case1.Number = entities.PreviousInterstateCase.KsCaseId ?? Spaces
        (10);
    }

    UseSiGetDataInterstateCaseDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("SI0000_PAYMENT_ADDRESS_NF_RB"))
      {
        export.WriteError.Flag = "Y";
        export.Error.RptDetail =
          "PAYMENT_ADDRESS not found for ENF-R, ENF-U, EST-R, EST-U, PAT-R or PAT-U";
          

        return;
      }
      else if (AsChar(entities.PreviousInterstateCase.ActionCode) == 'P')
      {
        if (Equal(entities.PreviousInterstateCase.FunctionalTypeCode, "CSI") &&
          CharAt(entities.PreviousInterstateCase.ActionReasonCode, 2) != 'S')
        {
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail = "Error compiling Case Datablock; Case = " + local
            .Case1.Number;

          return;
        }
      }
      else if (AsChar(entities.PreviousInterstateCase.ActionCode) == 'R' || AsChar
        (entities.PreviousInterstateCase.ActionCode) == 'U')
      {
        if (Equal(entities.PreviousInterstateCase.FunctionalTypeCode, "CSI") ||
          Equal(entities.PreviousInterstateCase.FunctionalTypeCode, "MSC"))
        {
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail = "Error compiling Case Datablock; Case = " + local
            .Case1.Number;

          return;
        }
      }
      else
      {
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.InterstateCase.TransSerialNumber =
      entities.PreviousInterstateCase.TransSerialNumber;
    local.InterstateCase.TransactionDate =
      entities.PreviousInterstateCase.TransactionDate;
    local.InterstateCase.KsCaseId = entities.PreviousInterstateCase.KsCaseId;

    // ----------------------------------------------------------------
    // Find primary AP with matching Interstate Request History (IREQ )
    // ----------------------------------------------------------------
    if (ReadCsePerson1())
    {
      local.PrimaryApCsePersonsWorkSet.Number = entities.CsePerson.Number;
    }
    else
    {
      // ----------------------------------------------------------------
      // Some types of transactions will not have IREQ records.
      // Find primary AP using CSE Case and Other State FIPS
      // ----------------------------------------------------------------
      if (ReadCsePerson3())
      {
        local.PrimaryApCsePersonsWorkSet.Number = entities.CsePerson.Number;
      }
      else
      {
        // ----------------------------------------------------------------
        // Active Interstate Request not found for that state.
        // Find primary AP using CSE Case and Other State FIPS that
        // was inactivated on the same day the transaction was sent.
        // (For GSC0x - Case Closure transactions)
        // ----------------------------------------------------------------
        if (ReadCsePerson2())
        {
          local.PrimaryApCsePersonsWorkSet.Number = entities.CsePerson.Number;
        }
        else
        {
          // ----------------------------------------------------------------
          // Primary AP was still not found.  Determine the reason to give
          // an appropriate error message
          // ----------------------------------------------------------------
          if (ReadInterstateRequest())
          {
            local.EabConvertNumeric.SendNonSuppressPos = 8;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.InterstateRequest.IntHGeneratedId, 15);
            UseEabConvertNumeric1();
            export.Error.RptDetail =
              "Primary AP not found because Interstate Request is closed; Case = " +
              local.Case1.Number + ", Interstate Request Id = " + Substring
              (local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
              EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 8, 8);
          }
          else
          {
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.InterstateCase.OtherFipsState, 15);
            UseEabConvertNumeric1();
            export.Error.RptDetail =
              "Primary AP not found because the Interstate Request not found for Case and State; Case = " +
              local.Case1.Number + ", State = " + Substring
              (local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
              EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 14, 2);
          }

          export.WriteError.Flag = "Y";

          return;
        }
      }
    }

    UseSiGetDataInterstateApIdDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "A";
        export.Error.RptDetail = "ADABAS Unavailable";

        return;
      }
      else if (Equal(local.InterstateCase.FunctionalTypeCode, "LO1") || Equal
        (local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
        (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
        (local.InterstateCase.FunctionalTypeCode, "PAT"))
      {
        export.WriteError.Flag = "Y";
        export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
          .PrimaryApCsePersonsWorkSet.Number;

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
    {
      if (Equal(local.InterstateCase.FunctionalTypeCode, "LO1") && AsChar
        (local.InterstateCase.ActionCode) == 'P')
      {
        local.VerifiedSince.Date = AddYears(local.Current.Date, -1);
      }

      UseSiGetDataInterstateApLocDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "ADABAS Unavailable";

          return;
        }
        else if (AsChar(local.InterstateCase.ActionCode) == 'P')
        {
          if (Equal(local.InterstateCase.FunctionalTypeCode, "LO1") && CharAt
            (local.InterstateCase.ActionReasonCode, 2) == 'S')
          {
            export.WriteError.Flag = "Y";
            export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
              .PrimaryApCsePersonsWorkSet.Number;

            return;
          }
        }
        else if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
          (local.InterstateCase.ActionCode) == 'U')
        {
          if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
            (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
            (local.InterstateCase.FunctionalTypeCode, "PAT"))
          {
            export.WriteError.Flag = "Y";
            export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
              .PrimaryApCsePersonsWorkSet.Number;

            return;
          }
        }
        else
        {
        }

        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (AsChar(local.InterstateCase.ActionCode) == 'P' && Equal
        (local.InterstateCase.FunctionalTypeCode, "LO1"))
      {
        if (AsChar(local.ApIsDeceased.Flag) == 'Y')
        {
          if (!IsEmpty(local.InterstateApLocate.ResidentialAddressLine1))
          {
            local.InterstateApLocate.ResidentialAddressEndDate =
              local.Current.Date;
          }

          if (!IsEmpty(local.InterstateApLocate.MailingAddressLine1))
          {
            local.InterstateApLocate.MailingAddressEndDate = local.Current.Date;
          }
        }
      }
    }

    UseSiGetDataInterstatePartDbs();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "A";
        export.Error.RptDetail = "ADABAS Unavailable";

        return;
      }
      else if (AsChar(local.InterstateCase.ActionCode) == 'P')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "CSI") && CharAt
          (local.InterstateCase.ActionReasonCode, 2) == 'S')
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail =
            "ADABAS Error compiling Participant Datablock for CSI-P";

          return;
        }
      }
      else if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
        (local.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (local.InterstateCase.FunctionalTypeCode, "PAT"))
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail =
            "ADABAS Error compiling Participant Datablock for CSI-P";

          return;
        }
      }
      else
      {
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
      (local.InterstateCase.ActionCode) == 'U')
    {
      if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
        (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
        (local.InterstateCase.FunctionalTypeCode, "PAT"))
      {
        if (AsChar(local.ArFound.Flag) != 'Y' || AsChar(local.ChFound.Flag) != 'Y'
          )
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail =
            "AR or CH not found when compiling Participant Datablock for ENF-R, ENF-U, EST-R, EST-U, PAT-R or PAT-U";
            

          return;
        }
      }
    }

    local.LegalActions.Index = -1;

    foreach(var item in ReadInterstateSupportOrder())
    {
      // -------------------------------------------------------------
      // No check for overflow since the most we should find on an
      // existing transaction should be 9, which is the size of the
      // group view
      // -------------------------------------------------------------
      ++local.LegalActions.Index;
      local.LegalActions.CheckSize();

      local.LegalActions.Update.G.Identifier =
        entities.PreviousInterstateSupportOrder.LegalActionId.
          GetValueOrDefault();
    }

    UseSiGetDataInterstateOrderDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
        (local.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF"))
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail =
            "Error compiling Order Datablock for ENF-R or ENF-U";

          return;
        }
      }
      else
      {
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.Count.Count = 0;

    foreach(var item in ReadInterstateCollection())
    {
      ++local.Count.Count;
    }

    local.InterstateCase.CollectionDataInd = local.Count.Count;

    if (!IsEmpty(local.FamilyViolence.Flag))
    {
      if (ReadInterstateMiscellaneous())
      {
        local.InterstateMiscellaneous.Assign(
          entities.PreviousInterstateMiscellaneous);

        if (Find(entities.PreviousInterstateMiscellaneous.InformationTextLine1,
          "family violence") > 0)
        {
          goto Read;
        }
        else if (Find(entities.PreviousInterstateMiscellaneous.
          InformationTextLine2, "family violence") > 0)
        {
          goto Read;
        }
        else if (Find(entities.PreviousInterstateMiscellaneous.
          InformationTextLine3, "family violence") > 0)
        {
          goto Read;
        }
        else if (Find(entities.PreviousInterstateMiscellaneous.
          InformationTextLine4, "family violence") > 0)
        {
          goto Read;
        }
        else if (Find(entities.PreviousInterstateMiscellaneous.
          InformationTextLine5, "family violence") > 0)
        {
          goto Read;
        }
        else
        {
        }

        if (IsEmpty(entities.PreviousInterstateMiscellaneous.
          InformationTextLine1))
        {
          local.InterstateMiscellaneous.InformationTextLine1 =
            "Protected due to family violence";
        }
        else if (IsEmpty(entities.PreviousInterstateMiscellaneous.
          InformationTextLine2))
        {
          local.InterstateMiscellaneous.InformationTextLine2 =
            "Protected due to family violence";
        }
        else if (IsEmpty(entities.PreviousInterstateMiscellaneous.
          InformationTextLine3))
        {
          local.InterstateMiscellaneous.InformationTextLine3 =
            "Protected due to family violence";
        }
        else if (IsEmpty(entities.PreviousInterstateMiscellaneous.
          InformationTextLine4))
        {
          local.InterstateMiscellaneous.InformationTextLine4 =
            "Protected due to family violence";
        }
        else if (IsEmpty(entities.PreviousInterstateMiscellaneous.
          InformationTextLine5))
        {
          local.InterstateMiscellaneous.InformationTextLine5 =
            "Protected due to family violence";
        }
        else
        {
        }
      }
      else
      {
        local.InterstateMiscellaneous.InformationTextLine1 =
          "Protected due to family violence";
      }

Read:

      local.InterstateCase.InformationInd = 1;
      local.InterstateMiscellaneous.StatusChangeCode =
        local.InterstateCase.CaseStatus;
    }
    else if (ReadInterstateMiscellaneous())
    {
      local.InterstateMiscellaneous.Assign(
        entities.PreviousInterstateMiscellaneous);
      local.InterstateMiscellaneous.StatusChangeCode =
        local.InterstateCase.CaseStatus;
      local.InterstateCase.InformationInd = 1;
    }

    // -------------------------------------------------------------------
    // Delete datablocks which were used for the previous
    // transaction but are not being used for the revised transaction
    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    // Do not delete the information datablock since it is user entered text
    // -------------------------------------------------------------------
    if (Lt(local.InterstateCase.OrderDataInd.GetValueOrDefault(),
      entities.PreviousInterstateCase.OrderDataInd))
    {
      local.Count.Count =
        entities.PreviousInterstateCase.OrderDataInd.GetValueOrDefault();

      for(var limit = local.InterstateCase.OrderDataInd.GetValueOrDefault() + 1;
         local.Count.Count >= limit; local.Count.Count += -1)
      {
        local.DeleteInterstateSupportOrder.SystemGeneratedSequenceNum =
          local.Count.Count;
        UseSiDeleteInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error deleting INTERSTATE_ORDER";

          return;
        }

        export.Deletes.OrderDataInd =
          export.Deletes.OrderDataInd.GetValueOrDefault() + 1;
      }
    }

    if (Lt(local.InterstateCase.ParticipantDataInd.GetValueOrDefault(),
      entities.PreviousInterstateCase.ParticipantDataInd))
    {
      local.Count.Count =
        entities.PreviousInterstateCase.ParticipantDataInd.GetValueOrDefault();

      for(var limit =
        local.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1; local
        .Count.Count >= limit; local.Count.Count += -1)
      {
        local.DeleteInterstateParticipant.SystemGeneratedSequenceNum =
          local.Count.Count;
        UseSiDeleteInterstateParticipant();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error deleting INTERSTATE_PARTICIPANT";

          return;
        }

        export.Deletes.ParticipantDataInd =
          export.Deletes.ParticipantDataInd.GetValueOrDefault() + 1;
      }
    }

    if (Lt(local.InterstateCase.ApLocateDataInd.GetValueOrDefault(),
      entities.PreviousInterstateCase.ApLocateDataInd))
    {
      UseSiDeleteInterstateApLocate();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsExitState("CSENET_AP_ID_NF"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error deleting INTERSTATE_AP_LOCATE";

          return;
        }

        // -----------------------------------------------------------
        // If the exitstate is AP ID NF then the previous header was
        // marked incorrectly.  Not a problem for our transaction
        // -----------------------------------------------------------
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        export.Deletes.ApLocateDataInd = 1;
      }
    }

    if (Lt(local.InterstateCase.ApIdentificationInd.GetValueOrDefault(),
      entities.PreviousInterstateCase.ApIdentificationInd))
    {
      UseSiDeleteInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "A";
        export.Error.RptDetail = "Error deleting INTERSTATE_AP_ID";

        return;
      }

      export.Deletes.ApIdentificationInd = 1;
    }

    // -------------------------------------------------------------------
    // Update / Create datablocks for the revised transaction.
    // -------------------------------------------------------------------
    UseSiUpdateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.WriteError.Flag = "Y";
      export.AbendRollbackRequired.Flag = "A";
      export.Error.RptDetail = "Error creating INTERSTATE_CASE";

      return;
    }
    else
    {
      export.Updates.CaseDataInd = 1;
    }

    if (Lt(entities.PreviousInterstateCase.ApIdentificationInd,
      local.InterstateCase.ApIdentificationInd.GetValueOrDefault()))
    {
      UseSiCreateInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SI0000_INTERSTATE_AP_IDENT_AE"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform update instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiUpdateInterstateApId();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error creating INTERSTATE_AP_ID";

            return;
          }

          export.Updates.ApIdentificationInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error creating INTERSTATE_AP_ID";

          return;
        }
      }
      else
      {
        export.Creates.ApIdentificationInd = 1;
      }
    }
    else
    {
      UseSiUpdateInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSENET_AP_ID_NF"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform create instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiCreateInterstateApId();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error updating INTERSTATE_AP_ID";

            return;
          }

          export.Creates.ApIdentificationInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error updating INTERSTATE_AP_ID";

          return;
        }
      }
      else
      {
        export.Updates.ApIdentificationInd = 1;
      }
    }

    if (Lt(entities.PreviousInterstateCase.ApLocateDataInd,
      local.InterstateCase.ApLocateDataInd.GetValueOrDefault()))
    {
      UseSiCreateInterstateApLocate();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SI0000_INTERSTATE_AP_LOCATE_AE"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform update instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiUpdateInterstateApLocate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error creating INTERSTATE_AP_LOCATE";

            return;
          }

          export.Updates.ApLocateDataInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error creating INTERSTATE_AP_LOCATE";

          return;
        }
      }
      else
      {
        export.Creates.ApLocateDataInd = 1;
      }
    }
    else
    {
      UseSiUpdateInterstateApLocate();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSENET_AP_LOCATE_NF"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform create instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiCreateInterstateApLocate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error updating INTERSTATE_AP_ID";

            return;
          }

          export.Creates.ApLocateDataInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error updating INTERSTATE_AP_ID";

          return;
        }
      }
      else
      {
        export.Updates.ApLocateDataInd = 1;
      }
    }

    local.Participants.Index = 0;

    for(var limit = local.Participants.Count; local.Participants.Index < limit; ++
      local.Participants.Index)
    {
      if (!local.Participants.CheckSize())
      {
        break;
      }

      if (Lt(entities.PreviousInterstateCase.ParticipantDataInd,
        local.Participants.Index + 1))
      {
        UseSiCreateInterstateParticipant();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error creating INTERSTATE_PARTICIPANT";

          return;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
          export.Creates.ParticipantDataInd =
            export.Creates.ParticipantDataInd.GetValueOrDefault() + 1;
        }
      }
      else
      {
        UseSiUpdateInterstateParticipant();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("SI0000_INTERSTATE_PART_NF"))
          {
            // -----------------------------------------------------------
            // Previous Header was marked incorrectly.
            // Perform create instead
            // -----------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
            UseSiCreateInterstateParticipant();

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              export.WriteError.Flag = "Y";
              export.AbendRollbackRequired.Flag = "A";
              export.Error.RptDetail = "Error updating INTERSTATE_PARTICIPANT";

              return;
            }
            else
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.Creates.ParticipantDataInd =
                export.Creates.ParticipantDataInd.GetValueOrDefault() + 1;
            }
          }
          else
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error updating INTERSTATE_PARTICIPANT";

            return;
          }
        }
        else
        {
          export.Updates.ParticipantDataInd =
            export.Updates.ParticipantDataInd.GetValueOrDefault() + 1;
        }
      }
    }

    local.Participants.CheckIndex();
    local.Orders.Index = 0;

    for(var limit = local.Orders.Count; local.Orders.Index < limit; ++
      local.Orders.Index)
    {
      if (!local.Orders.CheckSize())
      {
        break;
      }

      if (Lt(entities.PreviousInterstateCase.OrderDataInd, local.Orders.Index +
        1))
      {
        UseSiCreateInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("INTERSTATE_SUPPORT_ORDER_AE"))
          {
            // -----------------------------------------------------------
            // Previous Header was marked incorrectly.
            // Perform update instead
            // -----------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
            UseSiUpdateInterstateOrder();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.WriteError.Flag = "Y";
              export.AbendRollbackRequired.Flag = "A";
              export.Error.RptDetail = "Error creating INTERSTATE_ORDER";

              return;
            }

            export.Updates.OrderDataInd =
              export.Updates.OrderDataInd.GetValueOrDefault() + 1;
          }
          else
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error creating INTERSTATE_ORDER";

            return;
          }
        }
        else
        {
          export.Creates.OrderDataInd =
            export.Creates.OrderDataInd.GetValueOrDefault() + 1;
        }
      }
      else
      {
        UseSiUpdateInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("INTERSTATE_SUPPORT_ORDER_NF"))
          {
            // -----------------------------------------------------------
            // Previous Header was marked incorrectly.
            // Perform create instead
            // -----------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
            UseSiCreateInterstateOrder();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.WriteError.Flag = "Y";
              export.AbendRollbackRequired.Flag = "A";
              export.Error.RptDetail = "Error updating INTERSTATE_ORDER";

              return;
            }

            export.Creates.OrderDataInd =
              export.Creates.OrderDataInd.GetValueOrDefault() + 1;
          }
          else
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error updating INTERSTATE_ORDER";

            return;
          }
        }
        else
        {
          export.Updates.OrderDataInd =
            export.Updates.OrderDataInd.GetValueOrDefault() + 1;
        }
      }
    }

    local.Orders.CheckIndex();

    if (Lt(entities.PreviousInterstateCase.InformationInd,
      local.InterstateCase.InformationInd.GetValueOrDefault()))
    {
      UseSiCreateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SI0000_INTERSTATE_MISC_AE"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform update instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiUpdateInterstateMisc();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error creating INTERSTATE_MISC";

            return;
          }

          export.Updates.InformationInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error creating INTERSTATE_MISC";
        }
      }
      else
      {
        export.Creates.InformationInd = 1;
      }
    }
    else
    {
      UseSiUpdateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SI0000_INTERSTATE_MISC_NF"))
        {
          // -----------------------------------------------------------
          // Previous Header was marked incorrectly.
          // Perform update instead
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiCreateInterstateMisc();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.WriteError.Flag = "Y";
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "Error updating INTERSTATE_MISC";

            return;
          }

          export.Creates.InformationInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "Error updating INTERSTATE_MISC";
        }
      }
      else
      {
        export.Updates.InformationInd = 1;
      }
    }
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveGroupToOrders(SiGetDataInterstateOrderDb.Export.
    GroupGroup source, Local.OrdersGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroupToParticipants(SiGetDataInterstatePartDbs.Export.
    GroupGroup source, Local.ParticipantsGroup target)
  {
    target.GlocalParticipant.Number = source.GexportParticipant.Number;
    target.G.Assign(source.G);
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.ActionReasonCode = source.ActionReasonCode;
  }

  private static void MoveInterstateSupportOrder(InterstateSupportOrder source,
    InterstateSupportOrder target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.FipsState = source.FipsState;
    target.FipsCounty = source.FipsCounty;
    target.FipsLocation = source.FipsLocation;
    target.Number = source.Number;
    target.OrderFilingDate = source.OrderFilingDate;
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
    target.PaymentFreq = source.PaymentFreq;
    target.AmountOrdered = source.AmountOrdered;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.CancelDate = source.CancelDate;
    target.ArrearsFreq = source.ArrearsFreq;
    target.ArrearsFreqAmount = source.ArrearsFreqAmount;
    target.ArrearsTotalAmount = source.ArrearsTotalAmount;
    target.ArrearsAfdcFromDate = source.ArrearsAfdcFromDate;
    target.ArrearsAfdcThruDate = source.ArrearsAfdcThruDate;
    target.ArrearsAfdcAmount = source.ArrearsAfdcAmount;
    target.ArrearsNonAfdcFromDate = source.ArrearsNonAfdcFromDate;
    target.ArrearsNonAfdcThruDate = source.ArrearsNonAfdcThruDate;
    target.ArrearsNonAfdcAmount = source.ArrearsNonAfdcAmount;
    target.FosterCareFromDate = source.FosterCareFromDate;
    target.FosterCareThruDate = source.FosterCareThruDate;
    target.FosterCareAmount = source.FosterCareAmount;
    target.MedicalFromDate = source.MedicalFromDate;
    target.MedicalThruDate = source.MedicalThruDate;
    target.MedicalAmount = source.MedicalAmount;
    target.MedicalOrderedInd = source.MedicalOrderedInd;
    target.TribunalCaseNumber = source.TribunalCaseNumber;
    target.DateOfLastPayment = source.DateOfLastPayment;
    target.ControllingOrderFlag = source.ControllingOrderFlag;
    target.NewOrderFlag = source.NewOrderFlag;
    target.DocketNumber = source.DocketNumber;
  }

  private static void MoveLegalActions(Local.LegalActionsGroup source,
    SiGetDataInterstateOrderDb.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.G.Identifier;
  }

  private static void MoveParticipants(Local.ParticipantsGroup source,
    SiGetDataInterstateOrderDb.Import.ParticipantsGroup target)
  {
    target.GimportParticipant.Number = source.GlocalParticipant.Number;
    target.G.Assign(source.G);
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateOrder()
  {
    var useImport = new SiCreateInterstateOrder.Import();
    var useExport = new SiCreateInterstateOrder.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateSupportOrder.Assign(local.Orders.Item.G);

    Call(SiCreateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateParticipant.Assign(local.Participants.Item.G);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiDeleteInterstateApId()
  {
    var useImport = new SiDeleteInterstateApId.Import();
    var useExport = new SiDeleteInterstateApId.Export();

    MoveInterstateCase2(entities.PreviousInterstateCase,
      useImport.InterstateCase);

    Call(SiDeleteInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiDeleteInterstateApLocate()
  {
    var useImport = new SiDeleteInterstateApLocate.Import();
    var useExport = new SiDeleteInterstateApLocate.Export();

    MoveInterstateCase2(entities.PreviousInterstateCase,
      useImport.InterstateCase);

    Call(SiDeleteInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiDeleteInterstateOrder()
  {
    var useImport = new SiDeleteInterstateOrder.Import();
    var useExport = new SiDeleteInterstateOrder.Export();

    useImport.InterstateSupportOrder.SystemGeneratedSequenceNum =
      local.DeleteInterstateSupportOrder.SystemGeneratedSequenceNum;
    MoveInterstateCase2(entities.PreviousInterstateCase,
      useImport.InterstateCase);

    Call(SiDeleteInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiDeleteInterstateParticipant()
  {
    var useImport = new SiDeleteInterstateParticipant.Import();
    var useExport = new SiDeleteInterstateParticipant.Export();

    useImport.InterstateParticipant.SystemGeneratedSequenceNum =
      local.DeleteInterstateParticipant.SystemGeneratedSequenceNum;
    MoveInterstateCase2(entities.PreviousInterstateCase,
      useImport.InterstateCase);

    Call(SiDeleteInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiGetDataInterstateApIdDb()
  {
    var useImport = new SiGetDataInterstateApIdDb.Import();
    var useExport = new SiGetDataInterstateApIdDb.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Ap.Number = local.PrimaryApCsePersonsWorkSet.Number;

    Call(SiGetDataInterstateApIdDb.Execute, useImport, useExport);

    local.ApIsDeceased.Flag = useExport.ApIsDeceased.Flag;
    local.InterstateCase.ApIdentificationInd =
      useExport.InterstateCase.ApIdentificationInd;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
  }

  private void UseSiGetDataInterstateApLocDb()
  {
    var useImport = new SiGetDataInterstateApLocDb.Import();
    var useExport = new SiGetDataInterstateApLocDb.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.VerifiedSince.Date = local.VerifiedSince.Date;
    useImport.Ap.Number = local.PrimaryApCsePersonsWorkSet.Number;

    Call(SiGetDataInterstateApLocDb.Execute, useImport, useExport);

    local.FamilyViolence.Flag = useExport.FamilyViolence.Flag;
    local.InterstateCase.ApLocateDataInd =
      useExport.InterstateCase.ApLocateDataInd;
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    MoveInterstateCase1(entities.PreviousInterstateCase,
      useImport.InterstateCase);
    useImport.Current.Date = local.Current.Date;
    useImport.Case1.Number = local.Case1.Number;

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiGetDataInterstateOrderDb()
  {
    var useImport = new SiGetDataInterstateOrderDb.Import();
    var useExport = new SiGetDataInterstateOrderDb.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Case1.Number = local.Case1.Number;
    local.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions);
    useImport.ZdelImportPrimaryAp.Number = local.PrimaryApCsePerson.Number;
    local.Participants.CopyTo(useImport.Participants, MoveParticipants);

    Call(SiGetDataInterstateOrderDb.Execute, useImport, useExport);

    local.InterstateCase.OrderDataInd = useExport.InterstateCase.OrderDataInd;
    useExport.Group.CopyTo(local.Orders, MoveGroupToOrders);
  }

  private void UseSiGetDataInterstatePartDbs()
  {
    var useImport = new SiGetDataInterstatePartDbs.Import();
    var useExport = new SiGetDataInterstatePartDbs.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.Case1.Number = local.Case1.Number;
    useImport.PrimaryAp.Number = local.PrimaryApCsePersonsWorkSet.Number;
    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstatePartDbs.Execute, useImport, useExport);

    local.ArFound.Flag = useExport.ArFound.Flag;
    local.ChFound.Flag = useExport.ChFound.Flag;
    local.InterstateCase.ParticipantDataInd =
      useExport.InterstateCase.ParticipantDataInd;
    useExport.Group.CopyTo(local.Participants, MoveGroupToParticipants);
  }

  private void UseSiUpdateInterstateApId()
  {
    var useImport = new SiUpdateInterstateApId.Import();
    var useExport = new SiUpdateInterstateApId.Export();

    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateApLocate()
  {
    var useImport = new SiUpdateInterstateApLocate.Import();
    var useExport = new SiUpdateInterstateApLocate.Export();

    useImport.InterstateApLocate.Assign(local.InterstateApLocate);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateCase()
  {
    var useImport = new SiUpdateInterstateCase.Import();
    var useExport = new SiUpdateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiUpdateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateMisc()
  {
    var useImport = new SiUpdateInterstateMisc.Import();
    var useExport = new SiUpdateInterstateMisc.Export();

    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateOrder()
  {
    var useImport = new SiUpdateInterstateOrder.Import();
    var useExport = new SiUpdateInterstateOrder.Export();

    MoveInterstateSupportOrder(local.Orders.Item.G,
      useImport.InterstateSupportOrder);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiUpdateInterstateParticipant()
  {
    var useImport = new SiUpdateInterstateParticipant.Import();
    var useExport = new SiUpdateInterstateParticipant.Export();

    useImport.InterstateParticipant.Assign(local.Participants.Item.G);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateParticipant.Execute, useImport, useExport);
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", local.Case1.Number);
        db.SetInt64(
          command, "transactionSerial", local.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          local.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", local.Case1.Number);
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateCase.OtherFipsState);
        db.SetNullableDate(
          command, "othStateClsDte",
          local.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", local.Case1.Number);
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateCase.OtherFipsState);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.PreviousInterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PreviousInterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.PreviousInterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.PreviousInterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.PreviousInterstateCase.TransSerialNumber =
          db.GetInt64(reader, 3);
        entities.PreviousInterstateCase.ActionCode = db.GetString(reader, 4);
        entities.PreviousInterstateCase.FunctionalTypeCode =
          db.GetString(reader, 5);
        entities.PreviousInterstateCase.TransactionDate = db.GetDate(reader, 6);
        entities.PreviousInterstateCase.KsCaseId =
          db.GetNullableString(reader, 7);
        entities.PreviousInterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 8);
        entities.PreviousInterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 9);
        entities.PreviousInterstateCase.CaseDataInd =
          db.GetNullableInt32(reader, 10);
        entities.PreviousInterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 11);
        entities.PreviousInterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 12);
        entities.PreviousInterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 13);
        entities.PreviousInterstateCase.OrderDataInd =
          db.GetNullableInt32(reader, 14);
        entities.PreviousInterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 15);
        entities.PreviousInterstateCase.InformationInd =
          db.GetNullableInt32(reader, 16);
        entities.PreviousInterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCollection()
  {
    entities.PreviousInterstateCollection.Populated = false;

    return ReadEach("ReadInterstateCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.PreviousInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.PreviousInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.PreviousInterstateCollection.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.PreviousInterstateCollection.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.PreviousInterstateCollection.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 2);
        entities.PreviousInterstateCollection.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateMiscellaneous()
  {
    entities.PreviousInterstateMiscellaneous.Populated = false;

    return Read("ReadInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.PreviousInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.PreviousInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.PreviousInterstateMiscellaneous.StatusChangeCode =
          db.GetString(reader, 0);
        entities.PreviousInterstateMiscellaneous.NewCaseId =
          db.GetNullableString(reader, 1);
        entities.PreviousInterstateMiscellaneous.InformationTextLine1 =
          db.GetNullableString(reader, 2);
        entities.PreviousInterstateMiscellaneous.InformationTextLine2 =
          db.GetNullableString(reader, 3);
        entities.PreviousInterstateMiscellaneous.InformationTextLine3 =
          db.GetNullableString(reader, 4);
        entities.PreviousInterstateMiscellaneous.CcaTransSerNum =
          db.GetInt64(reader, 5);
        entities.PreviousInterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 6);
        entities.PreviousInterstateMiscellaneous.InformationTextLine4 =
          db.GetNullableString(reader, 7);
        entities.PreviousInterstateMiscellaneous.InformationTextLine5 =
          db.GetNullableString(reader, 8);
        entities.PreviousInterstateMiscellaneous.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateCase.OtherFipsState);
        db.SetNullableString(command, "casINumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateSupportOrder()
  {
    entities.PreviousInterstateSupportOrder.Populated = false;

    return ReadEach("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.PreviousInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTranSerNum",
          entities.PreviousInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.PreviousInterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.PreviousInterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.PreviousInterstateSupportOrder.CcaTranSerNum =
          db.GetInt64(reader, 2);
        entities.PreviousInterstateSupportOrder.LegalActionId =
          db.GetNullableInt32(reader, 3);
        entities.PreviousInterstateSupportOrder.Populated = true;

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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    private InterstateCase interstateCase;
    private DateWorkArea current;
    private Common batch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public EabReportSend Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of AbendRollbackRequired.
    /// </summary>
    [JsonPropertyName("abendRollbackRequired")]
    public Common AbendRollbackRequired
    {
      get => abendRollbackRequired ??= new();
      set => abendRollbackRequired = value;
    }

    /// <summary>
    /// A value of Controls.
    /// </summary>
    [JsonPropertyName("controls")]
    public InterstateCase Controls
    {
      get => controls ??= new();
      set => controls = value;
    }

    /// <summary>
    /// A value of Deletes.
    /// </summary>
    [JsonPropertyName("deletes")]
    public InterstateCase Deletes
    {
      get => deletes ??= new();
      set => deletes = value;
    }

    /// <summary>
    /// A value of Updates.
    /// </summary>
    [JsonPropertyName("updates")]
    public InterstateCase Updates
    {
      get => updates ??= new();
      set => updates = value;
    }

    /// <summary>
    /// A value of Creates.
    /// </summary>
    [JsonPropertyName("creates")]
    public InterstateCase Creates
    {
      get => creates ??= new();
      set => creates = value;
    }

    private Common writeError;
    private EabReportSend error;
    private Common abendRollbackRequired;
    private InterstateCase controls;
    private InterstateCase deletes;
    private InterstateCase updates;
    private InterstateCase creates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalAction G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private LegalAction g;
    }

    /// <summary>A ParticipantsGroup group.</summary>
    [Serializable]
    public class ParticipantsGroup
    {
      /// <summary>
      /// A value of GlocalParticipant.
      /// </summary>
      [JsonPropertyName("glocalParticipant")]
      public CsePerson GlocalParticipant
      {
        get => glocalParticipant ??= new();
        set => glocalParticipant = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson glocalParticipant;
      private InterstateParticipant g;
    }

    /// <summary>A OrdersGroup group.</summary>
    [Serializable]
    public class OrdersGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of DeleteInterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("deleteInterstateSupportOrder")]
    public InterstateSupportOrder DeleteInterstateSupportOrder
    {
      get => deleteInterstateSupportOrder ??= new();
      set => deleteInterstateSupportOrder = value;
    }

    /// <summary>
    /// A value of DeleteInterstateParticipant.
    /// </summary>
    [JsonPropertyName("deleteInterstateParticipant")]
    public InterstateParticipant DeleteInterstateParticipant
    {
      get => deleteInterstateParticipant ??= new();
      set => deleteInterstateParticipant = value;
    }

    /// <summary>
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
    }

    /// <summary>
    /// A value of ApIsDeceased.
    /// </summary>
    [JsonPropertyName("apIsDeceased")]
    public Common ApIsDeceased
    {
      get => apIsDeceased ??= new();
      set => apIsDeceased = value;
    }

    /// <summary>
    /// A value of ArFound.
    /// </summary>
    [JsonPropertyName("arFound")]
    public Common ArFound
    {
      get => arFound ??= new();
      set => arFound = value;
    }

    /// <summary>
    /// A value of ChFound.
    /// </summary>
    [JsonPropertyName("chFound")]
    public Common ChFound
    {
      get => chFound ??= new();
      set => chFound = value;
    }

    /// <summary>
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    public Common FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    /// <summary>
    /// A value of VerifiedSince.
    /// </summary>
    [JsonPropertyName("verifiedSince")]
    public DateWorkArea VerifiedSince
    {
      get => verifiedSince ??= new();
      set => verifiedSince = value;
    }

    /// <summary>
    /// A value of PrimaryApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primaryApCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimaryApCsePersonsWorkSet
    {
      get => primaryApCsePersonsWorkSet ??= new();
      set => primaryApCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PrimaryApCsePerson.
    /// </summary>
    [JsonPropertyName("primaryApCsePerson")]
    public CsePerson PrimaryApCsePerson
    {
      get => primaryApCsePerson ??= new();
      set => primaryApCsePerson = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of Participants.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantsGroup> Participants => participants ??= new(
      ParticipantsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Participants for json serialization.
    /// </summary>
    [JsonPropertyName("participants")]
    [Computed]
    public IList<ParticipantsGroup> Participants_Json
    {
      get => participants;
      set => Participants.Assign(value);
    }

    /// <summary>
    /// Gets a value of Orders.
    /// </summary>
    [JsonIgnore]
    public Array<OrdersGroup> Orders => orders ??= new(OrdersGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Orders for json serialization.
    /// </summary>
    [JsonPropertyName("orders")]
    [Computed]
    public IList<OrdersGroup> Orders_Json
    {
      get => orders;
      set => Orders.Assign(value);
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private EabConvertNumeric2 eabConvertNumeric;
    private Common count;
    private DateWorkArea current;
    private Case1 case1;
    private InterstateSupportOrder deleteInterstateSupportOrder;
    private InterstateParticipant deleteInterstateParticipant;
    private Array<LegalActionsGroup> legalActions;
    private Common apIsDeceased;
    private Common arFound;
    private Common chFound;
    private Common familyViolence;
    private DateWorkArea verifiedSince;
    private CsePersonsWorkSet primaryApCsePersonsWorkSet;
    private CsePerson primaryApCsePerson;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantsGroup> participants;
    private Array<OrdersGroup> orders;
    private InterstateMiscellaneous interstateMiscellaneous;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of PreviousInterstateCase.
    /// </summary>
    [JsonPropertyName("previousInterstateCase")]
    public InterstateCase PreviousInterstateCase
    {
      get => previousInterstateCase ??= new();
      set => previousInterstateCase = value;
    }

    /// <summary>
    /// A value of PreviousInterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("previousInterstateSupportOrder")]
    public InterstateSupportOrder PreviousInterstateSupportOrder
    {
      get => previousInterstateSupportOrder ??= new();
      set => previousInterstateSupportOrder = value;
    }

    /// <summary>
    /// A value of PreviousInterstateCollection.
    /// </summary>
    [JsonPropertyName("previousInterstateCollection")]
    public InterstateCollection PreviousInterstateCollection
    {
      get => previousInterstateCollection ??= new();
      set => previousInterstateCollection = value;
    }

    /// <summary>
    /// A value of PreviousInterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("previousInterstateMiscellaneous")]
    public InterstateMiscellaneous PreviousInterstateMiscellaneous
    {
      get => previousInterstateMiscellaneous ??= new();
      set => previousInterstateMiscellaneous = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private InterstateCase previousInterstateCase;
    private InterstateSupportOrder previousInterstateSupportOrder;
    private InterstateCollection previousInterstateCollection;
    private InterstateMiscellaneous previousInterstateMiscellaneous;
  }
#endregion
}

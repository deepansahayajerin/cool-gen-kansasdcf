// Program: SI_CREATE_CSENET_TRANS, ID: 373448974, model: 746.
// Short name: SWE02790
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_CSENET_TRANS.
/// </summary>
[Serializable]
public partial class SiCreateCsenetTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CSENET_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCsenetTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCsenetTrans.
  /// </summary>
  public SiCreateCsenetTrans(IContext context, Import import, Export export):
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
    // Date		Developer	Request
    // ----------------------------------------------------------------
    // 10/21/2002	M Ramirez	114395		Initial development
    // ----------------------------------------------------------------
    // ------------------------------------------------------------------
    // KS_CASE_IND values:
    // <Y>	Outgoing Interstate Involvement
    // <N>	Incoming Interstate Involvement
    // <space>	Neither (used for LO1, CSI and MSC correspondence)
    // ------------------------------------------------------------------
    // mlb - PR0166518 - 06/21/2004 - Change message to reflect that record was 
    // built,
    // but transaction was not sent.
    // 11/12/10  RMathews  CQ18038  Retrieve FIPS info for responses to incoming
    //                              interstate requests so reply goes to correct
    // person
    ExitState = "ACO_NN0000_ALL_OK";

    if (Lt(local.Current.Date, import.Batch.ProcessDate))
    {
      local.Current.Date = import.Batch.ProcessDate;
      local.Batch.Flag = "Y";
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    if (import.InterstateRequest.IntHGeneratedId == 0)
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    local.InterstateCase.OtherFipsState =
      import.InterstateRequest.OtherStateFips;
    local.InterstateCase.OtherFipsCounty = 0;
    local.InterstateCase.OtherFipsLocation = 0;

    // CQ18038 Overlay FIPS county and location if available from an incoming 
    // interstate request for that state
    if (AsChar(import.InterstateRequest.KsCaseInd) == 'N' && local
      .InterstateCase.OtherFipsState > 0 && !IsEmpty(import.Case1.Number))
    {
      foreach(var item in ReadInterstateCaseInterstateRequestInterstateRequestHistory())
        
      {
        if (entities.InterstateRequest.OtherStateFips == local
          .InterstateCase.OtherFipsState && AsChar
          (entities.InterstateRequest.KsCaseInd) == 'N')
        {
          local.InterstateCase.OtherFipsCounty =
            entities.InterstateCase.OtherFipsCounty;
          local.InterstateCase.OtherFipsLocation =
            entities.InterstateCase.OtherFipsLocation;

          break;
        }
      }
    }

    local.InterstateCase.InterstateCaseId =
      import.InterstateRequest.OtherStateCaseId ?? "";
    local.InterstateCase.ActionCode =
      import.InterstateRequestHistory.ActionCode;
    local.InterstateCase.ActionReasonCode =
      import.InterstateRequestHistory.ActionReasonCode ?? "";
    local.InterstateCase.FunctionalTypeCode =
      import.InterstateRequestHistory.FunctionalTypeCode;

    if (AsChar(import.InterstateRequestHistory.AttachmentIndicator) == 'Y')
    {
      local.InterstateCase.AttachmentsInd = "Y";
    }
    else
    {
      local.InterstateCase.AttachmentsInd = "N";
    }

    // ------------------------------------------------------
    // ENF, EST and PAT transactions cannot be sent if the
    // interstate involvement is limited to LO1s, CSIs or MSCs.
    // ------------------------------------------------------
    if (IsEmpty(import.InterstateRequest.KsCaseInd))
    {
      if (!Equal(local.InterstateCase.FunctionalTypeCode, "CSI") && !
        Equal(local.InterstateCase.FunctionalTypeCode, "LO1") && !
        Equal(local.InterstateCase.FunctionalTypeCode, "MSC"))
      {
        // mlb - PR00166518 - change message to reflect that the request and the
        // request history tables have
        // been updated, but did not build CSENet transaction.
        ExitState = "CO0000_STATE_NOT_CSENET_READY";

        return;
      }
    }

    // ------------------------------------------------------
    // Verify this state accepts CSENet transactions
    // ------------------------------------------------------
    if (ReadFips())
    {
      if (Equal(entities.Fips.StateAbbreviation, "KS"))
      {
        ExitState = "SI0000_OG_TRAN_CANT_BE_SEND_4_KS";

        return;
      }

      local.CsenetStateTable.StateCode = entities.Fips.StateAbbreviation;
      UseSiReadCsenetStateTable();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(local.CsenetStateTable.RecStateInd) == 'N')
      {
        // mlb - PR00166518 - 03/01/2004 - Change message to reflect that the 
        // request and the request history
        // have been added, but CSENet transaction has not.
        ExitState = "CO0000_STATE_NOT_CSENET_READY";

        return;
      }

      switch(TrimEnd(local.InterstateCase.FunctionalTypeCode))
      {
        case "LO1":
          if (AsChar(local.CsenetStateTable.QuickLocate) == 'N')
          {
            ExitState = "CO0000_STATE_DOES_ACCEPT_LO1";

            return;
          }

          break;
        case "CSI":
          local.Code.CodeName = "CSI READY";
          local.CodeValue.Cdvalue =
            NumberToString(local.InterstateCase.OtherFipsState, 14, 2);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            // mlb - PR00166518 - 03/01/2004 - Change message to reflect that 
            // request and request history was built,
            // but not CSENet transaction.
            ExitState = "CO0000_STATE_NOT_CSENET_READY";

            return;
          }

          break;
        default:
          if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'N')
          {
            // mlb - PR00166518 - 03/01/2004 - Change message to reflect that 
            // request and request history was built,
            // but not CSENet transaction.
            ExitState = "CO0000_STATE_NOT_CSENET_READY";

            return;
          }

          break;
      }
    }
    else
    {
      ExitState = "ACO_NE0000_INVALID_STATE_CODE";

      return;
    }

    // ------------------------------------------------------
    // Confirm that this type of transaction (Function, Action,
    // Reason) is valid for this type of interstate involvement
    // ------------------------------------------------------
    switch(AsChar(import.InterstateRequest.KsCaseInd))
    {
      case 'Y':
        local.IncomingInsterstate.Flag = "N";

        break;
      case 'N':
        local.IncomingInsterstate.Flag = "Y";

        break;
      default:
        local.IncomingInsterstate.Flag = "";

        break;
    }

    UseSiValidateCsenetTransType();

    if (!IsEmpty(local.Error.Text10))
    {
      switch(TrimEnd(local.Error.Text10))
      {
        case "FUNCTIONAL":
          ExitState = "SP0000_INVALID_FUNCTION";

          break;
        case "ACTION":
          ExitState = "ACO_NE0000_INVALID_ACTION";

          break;
        case "ACTION RSN":
          ExitState = "SI0000_INVALID_OG_CASE_REASON";

          break;
        default:
          // "COMBO"
          ExitState = "SI0000_REASON_INVALID_WITH_FUNCT";

          break;
      }

      return;
    }

    if (!IsEmpty(import.Case1.Number))
    {
      local.Case1.Number = import.Case1.Number;
    }
    else if (ReadCase())
    {
      local.Case1.Number = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    UseSiGetDataInterstateCaseDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------
    // Determine primary AP
    // ------------------------------------------------------
    if (!IsEmpty(import.Ap.Number))
    {
      local.PrimaryAp.Number = import.Ap.Number;
    }
    else if (ReadCsePerson())
    {
      local.PrimaryAp.Number = entities.CsePerson.Number;
    }
    else
    {
      ExitState = "AP_NF_RB";

      return;
    }

    UseSiGetDataInterstateApIdDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 0)
    {
      if (!Equal(local.InterstateCase.FunctionalTypeCode, "COL") && !
        Equal(local.InterstateCase.FunctionalTypeCode, "CSI") && !
        Equal(local.InterstateCase.FunctionalTypeCode, "MSC"))
      {
        ExitState = "CSENET_AP_ID_NF";

        return;
      }
    }

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
    {
      local.FviBypass.Flag = "Y";
      UseSiGetDataInterstateApLocDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

    if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 0)
    {
      if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
        (local.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (local.InterstateCase.FunctionalTypeCode, "PAT"))
        {
          ExitState = "CSENET_AP_LOCATE_NF";

          return;
        }
      }
      else if (AsChar(local.InterstateCase.ActionCode) == 'P')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "LO1") && CharAt
          (local.InterstateCase.ActionReasonCode, 2) == 'S')
        {
          ExitState = "CSENET_AP_LOCATE_NF";

          return;
        }
      }
    }

    UseSiGetDataInterstatePartDbs();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (IsEmpty(local.ArFound.Flag) || IsEmpty(local.ChFound.Flag))
    {
      if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
        (local.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (local.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (local.InterstateCase.FunctionalTypeCode, "PAT"))
        {
          ExitState = "SI0000_INTERSTATE_PART_NF";

          return;
        }
      }
    }

    if (local.Participants.Count == 0)
    {
      if (Equal(local.InterstateCase.FunctionalTypeCode, "CSI") && AsChar
        (local.InterstateCase.ActionCode) == 'P' && CharAt
        (local.InterstateCase.ActionReasonCode, 2) == 'S')
      {
        ExitState = "SI0000_INTERSTATE_PART_NF";

        return;
      }
    }

    // mjr--->  Set attributes for Interstate Order
    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
    {
      UseSiGetDataInterstateOrderDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

    if (local.Orders.Count == 0)
    {
      if (Equal(local.InterstateCase.FunctionalTypeCode, "ENF"))
      {
        if (AsChar(local.InterstateCase.ActionCode) == 'R' || AsChar
          (local.InterstateCase.ActionCode) == 'U')
        {
          ExitState = "INTERSTATE_SUPPORT_ORDER_NF";

          return;
        }
      }
    }

    // mjr--->  Set attributes for Interstate Miscellaneous
    if (!IsEmpty(import.InterstateRequestHistory.Note))
    {
      local.Length.Count = Length(import.InterstateRequestHistory.Note);

      if (local.Length.Count > 80)
      {
        local.InterstateMiscellaneous.InformationTextLine2 =
          Substring(import.InterstateRequestHistory.Note, 81, 80);

        if (local.Length.Count > 160)
        {
          local.InterstateMiscellaneous.InformationTextLine3 =
            Substring(import.InterstateRequestHistory.Note, 161, 80);

          if (local.Length.Count > 240)
          {
            local.InterstateMiscellaneous.InformationTextLine4 =
              Substring(import.InterstateRequestHistory.Note, 241, 80);

            if (local.Length.Count > 320)
            {
              local.InterstateMiscellaneous.InformationTextLine5 =
                Substring(import.InterstateRequestHistory.Note, 321, 80);
            }
          }
        }
      }

      local.InterstateMiscellaneous.StatusChangeCode = "O";
      local.InterstateMiscellaneous.NewCaseId =
        local.InterstateCase.KsCaseId ?? "";
      local.InterstateMiscellaneous.InformationTextLine1 =
        import.InterstateRequestHistory.Note ?? "";
      local.InterstateCase.InformationInd = 1;
    }
    else if (AsChar(local.InterstateCase.AttachmentsInd) == 'Y')
    {
      ExitState = "SI0000_NOTE_REQD_FOR_ATTACHMENT";

      return;
    }

    // mjr--->  Set attributes for Interstate Request History
    local.InterstateRequestHistory.TransactionDirectionInd = "O";
    local.InterstateRequestHistory.TransactionSerialNum =
      local.InterstateCase.TransSerialNumber;
    local.InterstateRequestHistory.ActionCode = local.InterstateCase.ActionCode;
    local.InterstateRequestHistory.FunctionalTypeCode =
      local.InterstateCase.FunctionalTypeCode;
    local.InterstateRequestHistory.TransactionDate =
      local.InterstateCase.TransactionDate;
    local.InterstateRequestHistory.ActionReasonCode =
      local.InterstateCase.ActionReasonCode ?? "";
    local.InterstateRequestHistory.ActionResolutionDate =
      local.InterstateCase.ActionResolutionDate;

    if (!IsEmpty(import.Batch.Name))
    {
      local.InterstateRequestHistory.CreatedBy = import.Batch.Name;
    }
    else
    {
      local.InterstateRequestHistory.CreatedBy = global.UserId;
    }

    local.InterstateRequestHistory.AttachmentIndicator =
      local.InterstateCase.AttachmentsInd;
    local.InterstateRequestHistory.Note =
      import.InterstateRequestHistory.Note ?? "";

    // mjr--->  Create datablocks
    UseSiCreateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseSiCreateOgCsenetEnvelop();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      if (AsChar(local.Batch.Flag) != 'Y')
      {
        UseEabRollbackCics();
      }

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.Batch.Flag) != 'Y')
        {
          UseEabRollbackCics();
        }

        return;
      }
    }

    if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateApLocate();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.Batch.Flag) != 'Y')
        {
          UseEabRollbackCics();
        }

        return;
      }
    }

    if (local.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
    {
      local.Participants.Index = 0;

      for(var limit = local.Participants.Count; local.Participants.Index < limit
        ; ++local.Participants.Index)
      {
        if (!local.Participants.CheckSize())
        {
          break;
        }

        UseSiCreateInterstateParticipant();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          if (AsChar(local.Batch.Flag) != 'Y')
          {
            UseEabRollbackCics();
          }

          return;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      local.Participants.CheckIndex();
    }

    if (local.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
    {
      local.Orders.Index = 0;

      for(var limit = local.Orders.Count; local.Orders.Index < limit; ++
        local.Orders.Index)
      {
        if (!local.Orders.CheckSize())
        {
          break;
        }

        UseSiCreateInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.Batch.Flag) != 'Y')
          {
            UseEabRollbackCics();
          }

          return;
        }
      }

      local.Orders.CheckIndex();
    }

    if (local.InterstateCase.InformationInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.Batch.Flag) != 'Y')
        {
          UseEabRollbackCics();
        }

        return;
      }
    }

    // mjr--->  Create Interstate Request History
    UseSiCabCreateIsRequestHistory();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Batch.Flag) != 'Y')
      {
        UseEabRollbackCics();
      }

      return;
    }

    MoveInterstateCase3(local.InterstateCase, export.InterstateCase);
  }

  private static void MoveChildren(Import.ChildrenGroup source,
    SiGetDataInterstatePartDbs.Import.ChildrenGroup target)
  {
    target.GimportChild.Number = source.GimportChild.Number;
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
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.AssnDeactDt = source.AssnDeactDt;
    target.AssnDeactInd = source.AssnDeactInd;
    target.LastDeferDt = source.LastDeferDt;
    target.Memo = source.Memo;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.InitiatingDocketNumber = source.InitiatingDocketNumber;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
    target.NondisclosureFinding = source.NondisclosureFinding;
    target.RespondingDocketNumber = source.RespondingDocketNumber;
    target.StateWithCej = source.StateWithCej;
    target.PaymentFipsCounty = source.PaymentFipsCounty;
    target.PaymentFipsState = source.PaymentFipsState;
    target.PaymentFipsLocation = source.PaymentFipsLocation;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
  }

  private static void MoveInterstateCase5(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.ActionReasonCode = source.ActionReasonCode;
  }

  private static void MoveLegalActions1(Import.LegalActionsGroup source,
    SiGetDataInterstateOrderDb.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.G.Identifier;
  }

  private static void MoveLegalActions2(Import.LegalActionsGroup source,
    SiGetDataInterstateCaseDb.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.G.Identifier;
  }

  private static void MoveParticipants(Local.ParticipantsGroup source,
    SiGetDataInterstateOrderDb.Import.ParticipantsGroup target)
  {
    target.GimportParticipant.Number = source.GlocalParticipant.Number;
    target.G.Assign(source.G);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSiCabCreateIsRequestHistory()
  {
    var useImport = new SiCabCreateIsRequestHistory.Import();
    var useExport = new SiCabCreateIsRequestHistory.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCabCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateOrder()
  {
    var useImport = new SiCreateInterstateOrder.Import();
    var useExport = new SiCreateInterstateOrder.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateSupportOrder.Assign(local.Orders.Item.G);

    Call(SiCreateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateParticipant.Assign(local.Participants.Item.G);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGetDataInterstateApIdDb()
  {
    var useImport = new SiGetDataInterstateApIdDb.Import();
    var useExport = new SiGetDataInterstateApIdDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Number = local.PrimaryAp.Number;

    Call(SiGetDataInterstateApIdDb.Execute, useImport, useExport);

    local.InterstateCase.ApIdentificationInd =
      useExport.InterstateCase.ApIdentificationInd;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
  }

  private void UseSiGetDataInterstateApLocDb()
  {
    var useImport = new SiGetDataInterstateApLocDb.Import();
    var useExport = new SiGetDataInterstateApLocDb.Export();

    useImport.FviBypass.Flag = local.FviBypass.Flag;
    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.Ap.Number = local.PrimaryAp.Number;

    Call(SiGetDataInterstateApLocDb.Execute, useImport, useExport);

    local.InterstateCase.ApLocateDataInd =
      useExport.InterstateCase.ApLocateDataInd;
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    import.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions2);
    useImport.Case1.Number = local.Case1.Number;
    useImport.Current.Date = local.Current.Date;
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    MoveInterstateCase1(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiGetDataInterstateOrderDb()
  {
    var useImport = new SiGetDataInterstateOrderDb.Import();
    var useExport = new SiGetDataInterstateOrderDb.Export();

    import.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions1);
    useImport.Case1.Number = local.Case1.Number;
    useImport.Current.Date = local.Current.Date;
    local.Participants.CopyTo(useImport.Participants, MoveParticipants);
    useImport.PrimaryAp.Number = local.PrimaryAp.Number;

    Call(SiGetDataInterstateOrderDb.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Orders, MoveGroupToOrders);
    local.InterstateCase.OrderDataInd = useExport.InterstateCase.OrderDataInd;
  }

  private void UseSiGetDataInterstatePartDbs()
  {
    var useImport = new SiGetDataInterstatePartDbs.Import();
    var useExport = new SiGetDataInterstatePartDbs.Export();

    import.Children.CopyTo(useImport.Children, MoveChildren);
    useImport.Case1.Number = local.Case1.Number;
    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.PrimaryAp.Number = local.PrimaryAp.Number;
    MoveInterstateCase4(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstatePartDbs.Execute, useImport, useExport);

    local.ArFound.Flag = useExport.ArFound.Flag;
    local.ChFound.Flag = useExport.ChFound.Flag;
    useExport.Group.CopyTo(local.Participants, MoveGroupToParticipants);
    local.InterstateCase.ParticipantDataInd =
      useExport.InterstateCase.ParticipantDataInd;
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void UseSiValidateCsenetTransType()
  {
    var useImport = new SiValidateCsenetTransType.Import();
    var useExport = new SiValidateCsenetTransType.Export();

    useImport.Automatic.Flag = import.AutomaticTrans.Flag;
    useImport.IncomingCase.Flag = local.IncomingInsterstate.Flag;
    MoveInterstateCase5(local.InterstateCase, useImport.InterstateCase);

    Call(SiValidateCsenetTransType.Execute, useImport, useExport);

    local.Error.Text10 = useExport.Error.Text10;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.InterstateRequest.OtherStateFips);
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

  private IEnumerable<bool>
    ReadInterstateCaseInterstateRequestInterstateRequestHistory()
  {
    entities.InterstateCase.Populated = false;
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach(
      "ReadInterstateCaseInterstateRequestInterstateRequestHistory",
      (db, command) =>
      {
        db.SetString(command, "number", import.Case1.Number);
        db.SetInt32(
          command, "otherFipsState", import.InterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 0);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 3);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 4);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 5);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 6);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 7);
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 7);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 8);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 9);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 10);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 11);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 15);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 17);
        entities.InterstateCase.Populated = true;
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

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
    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GimportChild.
      /// </summary>
      [JsonPropertyName("gimportChild")]
      public CsePerson GimportChild
      {
        get => gimportChild ??= new();
        set => gimportChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePerson gimportChild;
    }

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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public ProgramProcessingInfo Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of AutomaticTrans.
    /// </summary>
    [JsonPropertyName("automaticTrans")]
    public Common AutomaticTrans
    {
      get => automaticTrans ??= new();
      set => automaticTrans = value;
    }

    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private CsePerson ap;
    private Array<ChildrenGroup> children;
    private Array<LegalActionsGroup> legalActions;
    private ProgramProcessingInfo batch;
    private Common automaticTrans;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of FviBypass.
    /// </summary>
    [JsonPropertyName("fviBypass")]
    public Common FviBypass
    {
      get => fviBypass ??= new();
      set => fviBypass = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public WorkArea Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of IncomingInsterstate.
    /// </summary>
    [JsonPropertyName("incomingInsterstate")]
    public Common IncomingInsterstate
    {
      get => incomingInsterstate ??= new();
      set => incomingInsterstate = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of PrimaryAp.
    /// </summary>
    [JsonPropertyName("primaryAp")]
    public CsePersonsWorkSet PrimaryAp
    {
      get => primaryAp ??= new();
      set => primaryAp = value;
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
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
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

    private Common fviBypass;
    private WorkArea error;
    private Common incomingInsterstate;
    private Common arFound;
    private Common chFound;
    private Common validCode;
    private Common length;
    private Code code;
    private CodeValue codeValue;
    private Case1 case1;
    private Common batch;
    private DateWorkArea current;
    private CsenetStateTable csenetStateTable;
    private Array<ParticipantsGroup> participants;
    private Array<OrdersGroup> orders;
    private CsePerson csePerson;
    private CsePersonsWorkSet primaryAp;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private InterstateRequest interstateRequest;
    private Fips fips;
  }
#endregion
}

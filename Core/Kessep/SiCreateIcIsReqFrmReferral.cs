// Program: SI_CREATE_IC_IS_REQ_FRM_REFERRAL, ID: 372513212, model: 746.
// Short name: SWE01611
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_IC_IS_REQ_FRM_REFERRAL.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the Interstate Request for an Incoming CSENet referral 
/// after a KESSEP Case has been registered.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateIcIsReqFrmReferral: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_IC_IS_REQ_FRM_REFERRAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateIcIsReqFrmReferral(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateIcIsReqFrmReferral.
  /// </summary>
  public SiCreateIcIsReqFrmReferral(IContext context, Import import,
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
    // --------------------------------------------------
    //   DATE   DEVELOPER         DESCRIPTION
    // ??/??/?? ??????????        Original Development.
    // --------------------------------------------------
    // 03/23/99 W.Campbell        Added logic to
    //                            concat text_line_4 & 5
    //                            to the note.
    // --------------------------------------------------
    // 05/18/99 M.Lachowicz       Replace existing READ
    //                            of absent parent to get active
    //                            absent parent for today's date.
    // -----------------------------------------------
    // 06/11/99 W.Campbell        Created local view
    //                            of interstate_request containing
    //                            other_state_case_closure_date,
    //                            initialized it to current date and
    //                            view matched it to the import
    //                            view in SI_CREATE_IS_REQUEST.
    // -----------------------------------------------
    // 06/23/99  M. Lachowicz     Change property of READ
    //                            (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 11/17/99 W.Campbell        Modified an
    //                            IF stmt to also chk for
    //                            successful add.  Work
    //                            done on PR# 1002.
    // ------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!IsEmpty(import.Ap.Number))
    {
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson())
      {
        export.Ap.Number = import.Ap.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // --------------------------------------------------
      // 05/18/99 M.Lachowicz       Replace existing READ
      //                            of absent parent to get active
      //                            absent parent for today's date.
      // -----------------------------------------------
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePersonAbsentParent())
      {
        export.Ap.Number = entities.Ap.Number;
      }
      else
      {
        ExitState = "CO0000_ABSENT_PARENT_NF";

        return;
      }
    }

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.IncomingInterstateCase);
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    if (ReadInterstateMiscellaneous())
    {
      local.NotesExists.Flag = "Y";
    }

    // *************************************************************
    // Create Interstate Request for the Incoming CSENet Referral.
    // *************************************************************
    // --------------------------------------------------
    // 06/11/99 W.Campbell - Created local view
    // of interstate_request containing
    // other_state_case_closure_date, initialized
    // it to current date and view matched it to the
    // import view in SI_CREATE_IS_REQUEST.
    // -----------------------------------------------
    local.InterstateRequest.OtherStateCaseClosureDate = local.Current.Date;
    UseSiRegiCreateIsRequest();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *************************************************************
    // Create Interstate Request History for the Incoming CSENet Referral.
    // *************************************************************
    export.InterstateRequestHistory.TransactionDirectionInd = "I";
    export.InterstateRequestHistory.TransactionDate =
      export.InterstateCase.TransactionDate;

    if (AsChar(local.NotesExists.Flag) == 'Y')
    {
      export.InterstateRequestHistory.Note =
        TrimEnd(entities.IncomingInterstateMiscellaneous.InformationTextLine1) +
        TrimEnd
        (entities.IncomingInterstateMiscellaneous.InformationTextLine2) + TrimEnd
        (entities.IncomingInterstateMiscellaneous.InformationTextLine3);

      // ----------------------------------------
      // 03/23/99 W.Campbell - Added logic to
      // concat text_line_4 & 5 to the note.
      // ----------------------------------------
      export.InterstateRequestHistory.Note =
        TrimEnd(export.InterstateRequestHistory.Note) + TrimEnd
        (entities.IncomingInterstateMiscellaneous.InformationTextLine4) + TrimEnd
        (entities.IncomingInterstateMiscellaneous.InformationTextLine5);
    }

    UseSiCreateIsRequestHistory();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      return;
    }

    // *************************************************************
    // Create Interstate Contact & Address for the Incoming CSENet Referral.
    // *************************************************************
    UseSiRegiCreateIcReqContact();

    // ------------------------------------
    // 11/17/99 W.Campbell - Modified the following
    // IF stmt to also chk for successful add.
    // ------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      return;
    }

    // *************************************************************
    // Create Interstate Payment Address for the Incoming CSENet Referral.
    // *************************************************************
    UseSiRegiCreateIcReqPayAddr();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.TransactionDate = source.TransactionDate;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.TransactionDate = source.TransactionDate;
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
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Ap.Number = entities.Ap.Number;
    useImport.Case1.Number = entities.NewlyCreated.Number;
    useImport.InterstateRequestHistory.Assign(export.InterstateRequestHistory);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    export.InterstateRequestHistory.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSiRegiCreateIcReqContact()
  {
    var useImport = new SiRegiCreateIcReqContact.Import();
    var useExport = new SiRegiCreateIcReqContact.Export();

    MoveInterstateCase4(export.InterstateCase, useImport.InterstateCase);
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiRegiCreateIcReqContact.Execute, useImport, useExport);

    export.InterstateContact.Assign(useExport.InterstateContact);
    export.InterstateContactAddress.Assign(useExport.InterstateContactAddress);
  }

  private void UseSiRegiCreateIcReqPayAddr()
  {
    var useImport = new SiRegiCreateIcReqPayAddr.Import();
    var useExport = new SiRegiCreateIcReqPayAddr.Export();

    MoveInterstateCase3(export.InterstateCase, useImport.InterstateCase);
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiRegiCreateIcReqPayAddr.Execute, useImport, useExport);

    export.InterstatePaymentAddress.Assign(useExport.InterstatePaymentAddress);
  }

  private void UseSiRegiCreateIsRequest()
  {
    var useImport = new SiRegiCreateIsRequest.Import();
    var useExport = new SiRegiCreateIsRequest.Export();

    MoveInterstateRequest2(local.InterstateRequest, useImport.InterstateRequest);
      
    MoveInterstateCase1(export.InterstateCase, useImport.Incoming);
    useImport.Case1.Number = import.NewlyCreated.Number;
    useImport.Ap.Number = entities.Ap.Number;

    Call(SiRegiCreateIsRequest.Execute, useImport, useExport);

    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private bool ReadCase()
  {
    entities.NewlyCreated.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.NewlyCreated.Number);
      },
      (db, reader) =>
      {
        entities.NewlyCreated.Number = db.GetString(reader, 0);
        entities.NewlyCreated.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePersonAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.NewlyCreated.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateCase()
  {
    entities.IncomingInterstateCase.Populated = false;

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
        entities.IncomingInterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.IncomingInterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.IncomingInterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.IncomingInterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.IncomingInterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.IncomingInterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.IncomingInterstateCase.TransSerialNumber =
          db.GetInt64(reader, 6);
        entities.IncomingInterstateCase.ActionCode = db.GetString(reader, 7);
        entities.IncomingInterstateCase.FunctionalTypeCode =
          db.GetString(reader, 8);
        entities.IncomingInterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.IncomingInterstateCase.KsCaseId =
          db.GetNullableString(reader, 10);
        entities.IncomingInterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.IncomingInterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.IncomingInterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.IncomingInterstateCase.AttachmentsInd =
          db.GetString(reader, 14);
        entities.IncomingInterstateCase.CaseDataInd =
          db.GetNullableInt32(reader, 15);
        entities.IncomingInterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.IncomingInterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.IncomingInterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.IncomingInterstateCase.OrderDataInd =
          db.GetNullableInt32(reader, 19);
        entities.IncomingInterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.IncomingInterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.IncomingInterstateCase.SentDate =
          db.GetNullableDate(reader, 22);
        entities.IncomingInterstateCase.SentTime =
          db.GetNullableTimeSpan(reader, 23);
        entities.IncomingInterstateCase.DueDate =
          db.GetNullableDate(reader, 24);
        entities.IncomingInterstateCase.OverdueInd =
          db.GetNullableInt32(reader, 25);
        entities.IncomingInterstateCase.DateReceived =
          db.GetNullableDate(reader, 26);
        entities.IncomingInterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.IncomingInterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.IncomingInterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.IncomingInterstateCase.CaseType = db.GetString(reader, 30);
        entities.IncomingInterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.IncomingInterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.IncomingInterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.IncomingInterstateCase.PaymentCity =
          db.GetNullableString(reader, 34);
        entities.IncomingInterstateCase.PaymentState =
          db.GetNullableString(reader, 35);
        entities.IncomingInterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.IncomingInterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.IncomingInterstateCase.ZdelCpAddrLine1 =
          db.GetNullableString(reader, 38);
        entities.IncomingInterstateCase.ZdelCpAddrLine2 =
          db.GetNullableString(reader, 39);
        entities.IncomingInterstateCase.ZdelCpCity =
          db.GetNullableString(reader, 40);
        entities.IncomingInterstateCase.ZdelCpState =
          db.GetNullableString(reader, 41);
        entities.IncomingInterstateCase.ZdelCpZipCode5 =
          db.GetNullableString(reader, 42);
        entities.IncomingInterstateCase.ZdelCpZipCode4 =
          db.GetNullableString(reader, 43);
        entities.IncomingInterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.IncomingInterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.IncomingInterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.IncomingInterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.IncomingInterstateCase.ContactAddressLine1 =
          db.GetString(reader, 48);
        entities.IncomingInterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.IncomingInterstateCase.ContactCity =
          db.GetNullableString(reader, 50);
        entities.IncomingInterstateCase.ContactState =
          db.GetNullableString(reader, 51);
        entities.IncomingInterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.IncomingInterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.IncomingInterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.IncomingInterstateCase.AssnDeactDt =
          db.GetNullableDate(reader, 55);
        entities.IncomingInterstateCase.AssnDeactInd =
          db.GetNullableString(reader, 56);
        entities.IncomingInterstateCase.LastDeferDt =
          db.GetNullableDate(reader, 57);
        entities.IncomingInterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.IncomingInterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 59);
        entities.IncomingInterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 60);
        entities.IncomingInterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 61);
        entities.IncomingInterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 62);
        entities.IncomingInterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 63);
        entities.IncomingInterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 64);
        entities.IncomingInterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 65);
        entities.IncomingInterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 66);
        entities.IncomingInterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 67);
        entities.IncomingInterstateCase.StateWithCej =
          db.GetNullableString(reader, 68);
        entities.IncomingInterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 69);
        entities.IncomingInterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 70);
        entities.IncomingInterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 71);
        entities.IncomingInterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 72);
        entities.IncomingInterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateMiscellaneous()
  {
    entities.IncomingInterstateMiscellaneous.Populated = false;

    return Read("ReadInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.IncomingInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.IncomingInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.IncomingInterstateMiscellaneous.InformationTextLine1 =
          db.GetNullableString(reader, 0);
        entities.IncomingInterstateMiscellaneous.InformationTextLine2 =
          db.GetNullableString(reader, 1);
        entities.IncomingInterstateMiscellaneous.InformationTextLine3 =
          db.GetNullableString(reader, 2);
        entities.IncomingInterstateMiscellaneous.CcaTransSerNum =
          db.GetInt64(reader, 3);
        entities.IncomingInterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 4);
        entities.IncomingInterstateMiscellaneous.InformationTextLine4 =
          db.GetNullableString(reader, 5);
        entities.IncomingInterstateMiscellaneous.InformationTextLine5 =
          db.GetNullableString(reader, 6);
        entities.IncomingInterstateMiscellaneous.Populated = true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of NewlyCreated.
    /// </summary>
    [JsonPropertyName("newlyCreated")]
    public Case1 NewlyCreated
    {
      get => newlyCreated ??= new();
      set => newlyCreated = value;
    }

    private CsePerson ap;
    private InterstateCase interstateCase;
    private Case1 newlyCreated;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsePerson ap;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContact interstateContact;
    private InterstateContactAddress interstateContactAddress;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NotesExists.
    /// </summary>
    [JsonPropertyName("notesExists")]
    public Common NotesExists
    {
      get => notesExists ??= new();
      set => notesExists = value;
    }

    private InterstateRequest interstateRequest;
    private DateWorkArea current;
    private Common notesExists;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of IncomingInterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("incomingInterstateMiscellaneous")]
    public InterstateMiscellaneous IncomingInterstateMiscellaneous
    {
      get => incomingInterstateMiscellaneous ??= new();
      set => incomingInterstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of IncomingInterstateCase.
    /// </summary>
    [JsonPropertyName("incomingInterstateCase")]
    public InterstateCase IncomingInterstateCase
    {
      get => incomingInterstateCase ??= new();
      set => incomingInterstateCase = value;
    }

    /// <summary>
    /// A value of NewlyCreated.
    /// </summary>
    [JsonPropertyName("newlyCreated")]
    public Case1 NewlyCreated
    {
      get => newlyCreated ??= new();
      set => newlyCreated = value;
    }

    private CaseRole absentParent;
    private CsePerson ap;
    private InterstateMiscellaneous incomingInterstateMiscellaneous;
    private InterstateCase incomingInterstateCase;
    private Case1 newlyCreated;
  }
#endregion
}

// Program: SI_UPDT_IC_REF_WITH_NEW_KS_CASE, ID: 372711808, model: 746.
// Short name: SWE02554
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDT_IC_REF_WITH_NEW_KS_CASE.
/// </summary>
[Serializable]
public partial class SiUpdtIcRefWithNewKsCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDT_IC_REF_WITH_NEW_KS_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdtIcRefWithNewKsCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdtIcRefWithNewKsCase.
  /// </summary>
  public SiUpdtIcRefWithNewKsCase(IContext context, Import import, Export export)
    :
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
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer       Description
    // 03/24/99 W.Campbell      Initial development
    // ----------------------------------------------
    // 04/16/99 W.Campbell      Added an IF stmt
    //                          to reset the exit state to ALL OK
    //                          on a successful add, otherwise
    //                          some kind of problem exit state
    //                          is returned.
    // ---------------------------------------------
    // 04/17/99 W.Campbell      Modified logic
    //                          to use new definition for
    //                          contact_phone_extension which was
    //                          changed from numeric to text by
    //                          IDCR#521.
    // ---------------------------------------------
    // 05/14/99 W.Campbell      Added logic to
    //                          update the interstate_case with the
    //                          new KESSEP case number for the
    //                          ks_case_id.
    // --------------------------------------------------
    // 06/04/99 W.Campbell             Renamed CAB
    //                                 
    // si_create_is_ack_for_new_ks_case
    // TO
    //                                 
    // si_updt_ic_ref_with_new_ks_case.
    //                                 
    // Also within the CAB, disabled
    //                                 
    // logic to create an
    // acknowledgment
    //                                 
    // going back to the State which
    //                                 
    // requested that we establish this
    //                                 
    // case.  As per request by Curtis
    //                                 
    // Scroggins based on what he
    //                                 
    // learned at the ACF conference
    //                                 
    // in order to satisfy CSEnet
    //                                 
    // requirements.
    // --------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 03/12/01 swsrchf  PR# 00104264  Added a read of Case to retrieve the 
    // status.
    //                                 
    // Removed old 'Commented OUT'
    // code.
    // ------------------------------------------------------------
    // *** Problem report 00104264
    // *** 03/12/01 swsrchf
    // *** start
    if (ReadCase())
    {
      local.Case1.Status = entities.Existing.Status;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // *** end
    // *** 03/12/01 swsrchf
    // *** Problem report 00104264
    // ---------------------------------------------
    // Read the requesting State's interstate case data.
    // ---------------------------------------------
    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadInterstateCase())
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    // ---------------------------------------------
    // 05/14/99 W.Campbell - Added logic to
    // update the interstate_case with the
    // new KESSEP case number for the
    // ks_case_id.
    // ---------------------------------------------
    // --------------------------------------------------------------
    // 03/12/01 swsrchf  PR# 00104264  Added set statement for Status
    // --------------------------------------------------------------
    try
    {
      UpdateInterstateCase();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_CASE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "INTERSTATE_CASE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadCase()
  {
    entities.Existing.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Status = db.GetNullableString(reader, 1);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 6);
        entities.InterstateCase.ActionCode = db.GetString(reader, 7);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 10);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 14);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 15);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 22);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 23);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 24);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 26);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.InterstateCase.CaseType = db.GetString(reader, 30);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 34);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 35);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 40);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 41);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 42);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 48);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 49);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 50);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 51);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 55);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 56);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 58);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 59);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 60);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 62);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 66);
        entities.InterstateCase.Populated = true;
      });
  }

  private void UpdateInterstateCase()
  {
    var ksCaseId = import.Case1.Number;
    var caseStatus = local.Case1.Status ?? "";

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(command, "ksCaseId", ksCaseId);
        db.SetString(command, "caseStatus", caseStatus);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.KsCaseId = ksCaseId;
    entities.InterstateCase.CaseStatus = caseStatus;
    entities.InterstateCase.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private InterstateCase interstateCase;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of Ack.
    /// </summary>
    [JsonPropertyName("ack")]
    public InterstateCase Ack
    {
      get => ack ??= new();
      set => ack = value;
    }

    private Case1 case1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeAddress officeAddress;
    private InterstateCase ack;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private InterstateCase interstateCase;
    private Case1 existing;
  }
#endregion
}

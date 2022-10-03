// Program: SI_READ_CSENET_CASE_DATA, ID: 372497571, model: 746.
// Short name: SWE01214
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_CSENET_CASE_DATA.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD reads CSENet Referral Case Data records and returns them for 
/// further processing.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsenetCaseData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_CASE_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetCaseData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetCaseData.
  /// </summary>
  public SiReadCsenetCaseData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // * This PAD reads one CSENet Referral Case   *
    // * records based on the Transaction Serial   *
    // * Number passed by the PRAD.                *
    // * After the Referral Case is read, the PAD  *
    // * reads the address records associated with *
    // * it.
    // 
    // *
    // *********************************************
    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.InterstateCase);

      if (export.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        export.ApidInd.Flag = "Y";
      }
      else
      {
        export.ApidInd.Flag = "N";
      }

      if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
      {
        export.AplocInd.Flag = "Y";
      }
      else
      {
        export.AplocInd.Flag = "N";
      }

      if (export.InterstateCase.InformationInd.GetValueOrDefault() > 0)
      {
        export.MiscInd.Flag = "Y";
      }
      else
      {
        export.MiscInd.Flag = "N";
      }

      if (export.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
      {
        export.SupordInd.Flag = "Y";
      }
      else
      {
        export.SupordInd.Flag = "N";
      }

      if (export.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
      {
        export.PartInd.Flag = "Y";
      }
      else
      {
        export.PartInd.Flag = "N";
      }

      if (Equal(export.InterstateCase.ContactPhoneExtension, "000000"))
      {
        export.InterstateCase.ContactPhoneExtension = "";
      }

      local.CsePersonsWorkSet.FirstName =
        entities.InterstateCase.ContactNameFirst ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.InterstateCase.ContactNameLast ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.InterstateCase.ContactNameMiddle ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.Ctc.FormattedName = local.CsePersonsWorkSet.FormattedName;
      local.Code.CodeName = "INTERSTATE REASON";
      local.CodeValue.Cdvalue = entities.InterstateCase.ActionReasonCode ?? Spaces
        (10);
      UseCabGetCodeValueDescription();

      // ********************************************************************
      // PR 00125413
      // 08/28.01 T.Bobb If interstate case is an outgoing, set the
      // deact_ind to spaces so when returning to the procedure the deactivated 
      // exitstate message does not display.
      // ********************************************************************
      if (ReadCsenetTransactionEnvelop())
      {
        if (AsChar(entities.CsenetTransactionEnvelop.DirectionInd) == 'O')
        {
          export.InterstateCase.AssnDeactInd = "";
        }
      }
    }
    else
    {
      ExitState = "CSENET_REFERRAL_CASE_NF";
    }
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

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

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ctc.
    /// </summary>
    [JsonPropertyName("ctc")]
    public CsePersonsWorkSet Ctc
    {
      get => ctc ??= new();
      set => ctc = value;
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
    /// A value of ApidInd.
    /// </summary>
    [JsonPropertyName("apidInd")]
    public Common ApidInd
    {
      get => apidInd ??= new();
      set => apidInd = value;
    }

    /// <summary>
    /// A value of AplocInd.
    /// </summary>
    [JsonPropertyName("aplocInd")]
    public Common AplocInd
    {
      get => aplocInd ??= new();
      set => aplocInd = value;
    }

    /// <summary>
    /// A value of MiscInd.
    /// </summary>
    [JsonPropertyName("miscInd")]
    public Common MiscInd
    {
      get => miscInd ??= new();
      set => miscInd = value;
    }

    /// <summary>
    /// A value of SupordInd.
    /// </summary>
    [JsonPropertyName("supordInd")]
    public Common SupordInd
    {
      get => supordInd ??= new();
      set => supordInd = value;
    }

    /// <summary>
    /// A value of PartInd.
    /// </summary>
    [JsonPropertyName("partInd")]
    public Common PartInd
    {
      get => partInd ??= new();
      set => partInd = value;
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

    private CsePersonsWorkSet ctc;
    private InterstateCase interstateCase;
    private Common apidInd;
    private Common aplocInd;
    private Common miscInd;
    private Common supordInd;
    private Common partInd;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Code code;
    private CodeValue codeValue;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
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

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
  }
#endregion
}

// Program: FN_EHST_LST_EFT_STATUS_HIST, ID: 372157652, model: 746.
// Short name: SWEEHSTP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EHST_LST_EFT_STATUS_HIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnEhstLstEftStatusHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EHST_LST_EFT_STATUS_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEhstLstEftStatusHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEhstLstEftStatusHist.
  /// </summary>
  public FnEhstLstEftStatusHist(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************
    // Procedure : List EFT Status History
    // Developed by : A Samuels, MTW
    // Change Log :
    // 07/02/1997	A Samuels		Initial Development
    // 01/06/1999      G Sharp                 Phase 2 changes.
    // 05/11/1999      Fangman               Changed process to be driven by EFT
    // # rather than Payment Request #.  Added Settlement Date to screen.
    // General reformatting of the screen.
    // 07/17/1999     Fangman                Added check digit behind the 
    // Routing number.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.ElectronicFundTransmission.TransmissionIdentifier =
      import.ElectronicFundTransmission.TransmissionIdentifier;

    if (Equal(global.Command, "DISPLAY"))
    {
      // *** Continue
    }
    else
    {
      export.ElectronicFundTransmission.
        Assign(import.ElectronicFundTransmission);
      MovePaymentRequest(import.PaymentRequest, export.PaymentRequest);
      MoveCsePersonsWorkSet(import.Payee, export.Payee);
      MoveCsePersonsWorkSet(import.DesignatedPayee, export.DesignatedPayee);
      export.PayeePassThruFlow.Number = import.PayeePassThruFlow.Number;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailPaymentStatus.Code =
          import.Import1.Item.DetailPaymentStatus.Code;
        MovePaymentStatusHistory(import.Import1.Item.DetailPaymentStatusHistory,
          export.Export1.Update.DetailPaymentStatusHistory);
        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumberObligee = export.Payee.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "PACC") || Equal(global.Command, "EFTL") || Equal
      (global.Command, "EDTL") || Equal(global.Command, "PPMT"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ****** MAIN  CASE-OF-COMMAND Structure
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        if (!IsEmpty(import.EftPrompt.SelectChar))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.EftPrompt, "selectChar");

          field.Error = true;

          return;
        }

        if (export.ElectronicFundTransmission.TransmissionIdentifier == 0)
        {
          ExitState = "SP0000_REQUIRED_FIELD_MISSING";

          var field =
            GetField(export.ElectronicFundTransmission, "transmissionIdentifier");
            

          field.Error = true;

          return;
        }

        if (ReadElectronicFundTransmission())
        {
          export.ElectronicFundTransmission.Assign(
            entities.ElectronicFundTransmission);
        }
        else
        {
          var field =
            GetField(export.ElectronicFundTransmission, "transmissionIdentifier");
            

          field.Error = true;

          ExitState = "FN0000_EFT_NF";

          return;
        }

        if (ReadPaymentRequest())
        {
          export.PaymentRequest.Assign(entities.PaymentRequest);

          if (!Equal(entities.PaymentRequest.Type1, "EFT"))
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NOT_EFT";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_PAYMENT_REQUEST_NF";

          return;
        }

        // *** Finding the Payee Cse_Person Name ***
        local.CsePersonsWorkSet.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseSiReadCsePerson();
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.Payee);
        export.PayeePassThruFlow.Number = local.CsePersonsWorkSet.Number;

        if (IsExitState("CSE_PERSON_NF"))
        {
          ExitState = "FN0000_PAYEE_CSE_PERSON_NF";

          return;
        }

        // *** Finding the Designated Payee Cse_Person Name ***
        if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.CsePersonsWorkSet.Number =
            entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseSiReadCsePerson();
          MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.DesignatedPayee);
            

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

            return;
          }
        }

        // *** Getting the Detail Lines comprising of Payment_Status_History 
        // Effective date & Created_by and the associated Payment_Status Code **
        // *
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadPaymentStatusHistory())
        {
          MovePaymentStatusHistory(entities.PaymentStatusHistory,
            export.Export1.Update.DetailPaymentStatusHistory);

          if (ReadPaymentStatus())
          {
            export.Export1.Update.DetailPaymentStatus.Code =
              entities.PaymentStatus.Code;
          }
          else
          {
            ExitState = "FN0000_PYMNT_STAT_NF";
            export.Export1.Next();

            return;
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        // *** Processing for PROMPT ***
        // Set Exit_State to flow to 'List EFT Number' Procedure
        if (AsChar(import.EftPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.EftPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "ECO_LNK_LST_EFT";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        // *** Set Exit_State to flow to Signoff Procedure ***
        UseScCabSignoff();

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "EFTL":
        ExitState = "ECO_LNK_LST_EFT";

        break;
      case "EDTL":
        ExitState = "ECO_LNK_LST_EFT_DETAIL";

        break;
      case "PPMT":
        ExitState = "ECO_XFR_TO_MTN_PREF_PMNT_METHOD";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MovePaymentStatusHistory(PaymentStatusHistory source,
    PaymentStatusHistory target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId",
          export.ElectronicFundTransmission.TransmissionIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 0);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 1);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 4);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 5);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 6);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(
      entities.ElectronicFundTransmission.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.ElectronicFundTransmission.PrqGeneratedId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatusHistory.PstGeneratedId);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory()
  {
    return ReadEach("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 4);
        entities.PaymentStatusHistory.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of EftPrompt.
    /// </summary>
    [JsonPropertyName("eftPrompt")]
    public Common EftPrompt
    {
      get => eftPrompt ??= new();
      set => eftPrompt = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PayeePassThruFlow.
    /// </summary>
    [JsonPropertyName("payeePassThruFlow")]
    public CsePerson PayeePassThruFlow
    {
      get => payeePassThruFlow ??= new();
      set => payeePassThruFlow = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet designatedPayee;
    private Common eftPrompt;
    private PaymentRequest paymentRequest;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson payeePassThruFlow;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of EftPrompt.
    /// </summary>
    [JsonPropertyName("eftPrompt")]
    public Common EftPrompt
    {
      get => eftPrompt ??= new();
      set => eftPrompt = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PayeePassThruFlow.
    /// </summary>
    [JsonPropertyName("payeePassThruFlow")]
    public CsePerson PayeePassThruFlow
    {
      get => payeePassThruFlow ??= new();
      set => payeePassThruFlow = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet designatedPayee;
    private Common eftPrompt;
    private PaymentRequest paymentRequest;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson payeePassThruFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    private PaymentRequest paymentRequest;
    private ElectronicFundTransmission electronicFundTransmission;
    private CsePerson csePerson;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentMethodType paymentMethodType;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
  }
#endregion
}

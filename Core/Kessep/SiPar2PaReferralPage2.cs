// Program: SI_PAR2_PA_REFERRAL_PAGE_2, ID: 371760579, model: 746.
// Short name: SWEPAR2P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PAR2_PA_REFERRAL_PAGE_2.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPar2PaReferralPage2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR2_PA_REFERRAL_PAGE_2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar2PaReferralPage2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar2PaReferralPage2.
  /// </summary>
  public SiPar2PaReferralPage2(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //      M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03-01-95  J.W. Hays		Initial development
    // 08-29-95  Ken Evans		Continue development
    // 11/03/96  G. Lofton - MTW	Add new security.
    // 08/07/97  Sid			Cleanup.
    // ------------------------------------------------------------
    // *********************************************
    // * This PRAD completes the display of PA     *
    // * Referral data.  There are no enterable or *
    // * selectable fields on the screen.          *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.PaReferral.Assign(import.PaReferral);
    export.Employer.Assign(import.Employer);
    export.ApEmpPhone.Assign(import.ApEmpPhone);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
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

    if (Equal(global.Command, "RETPAREF"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PAR3"))
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

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseSiReadPaReferral2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.PaReferral, "number");

          field.Error = true;
        }

        if (export.PaReferral.ApEmployerPhone.GetValueOrDefault() != 0)
        {
          export.ApEmpPhone.Text11 =
            NumberToString(export.PaReferral.ApEmployerPhone.
              GetValueOrDefault(), 11);
          export.ApEmpPhone.Text3 = Substring(export.ApEmpPhone.Text11, 2, 3);
          export.ApEmpPhone.Text9 = Substring(export.ApEmpPhone.Text11, 5, 7);
        }

        // *********************************************
        // * If the AP Employer and AP Employer Address*
        // * are found, they will be displayed.  It is *
        // * NOT an error if they are not found.       *
        // *********************************************
        break;
      case "PAR3":
        if (Equal(import.PaReferral.PgmCode, "FC"))
        {
          ExitState = "ECO_LNK_TO_PA_REFERRAL_FC";
        }
        else
        {
          ExitState = "NO_FC_DATA_FOR_REFERRAL";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private static void MovePaReferral1(PaReferral source, PaReferral target)
  {
    target.CsOrderPlace = source.CsOrderPlace;
    target.CsOrderState = source.CsOrderState;
    target.CsFreq = source.CsFreq;
    target.From = source.From;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Note = source.Note;
    target.CaseNumber = source.CaseNumber;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.MedicalPaymentDueDate = source.MedicalPaymentDueDate;
    target.MedicalAmt = source.MedicalAmt;
    target.MedicalFreq = source.MedicalFreq;
    target.MedicalLastPayment = source.MedicalLastPayment;
    target.MedicalLastPaymentDate = source.MedicalLastPaymentDate;
    target.MedicalOrderEffectiveDate = source.MedicalOrderEffectiveDate;
    target.MedicalOrderState = source.MedicalOrderState;
    target.MedicalOrderPlace = source.MedicalOrderPlace;
    target.MedicalArrearage = source.MedicalArrearage;
    target.MedicalPaidTo = source.MedicalPaidTo;
    target.MedicalPaymentType = source.MedicalPaymentType;
    target.MedicalInsuranceCo = source.MedicalInsuranceCo;
    target.MedicalPolicyNumber = source.MedicalPolicyNumber;
    target.MedicalOrderNumber = source.MedicalOrderNumber;
    target.MedicalOrderInd = source.MedicalOrderInd;
    target.ArRetainedInd = source.ArRetainedInd;
    target.PgmCode = source.PgmCode;
    target.PaymentMadeTo = source.PaymentMadeTo;
    target.CsArrearageAmt = source.CsArrearageAmt;
    target.CsLastPaymentAmt = source.CsLastPaymentAmt;
    target.LastPaymentDate = source.LastPaymentDate;
    target.GoodCauseCode = source.GoodCauseCode;
    target.GoodCauseDate = source.GoodCauseDate;
    target.CsPaymentAmount = source.CsPaymentAmount;
    target.OrderEffectiveDate = source.OrderEffectiveDate;
    target.PaymentDueDate = source.PaymentDueDate;
    target.SupportOrderId = source.SupportOrderId;
    target.LastApContactDate = source.LastApContactDate;
    target.VoluntarySupportInd = source.VoluntarySupportInd;
    target.ApEmployerPhone = source.ApEmployerPhone;
    target.ApEmployerName = source.ApEmployerName;
  }

  private static void MovePaReferral2(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
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

  private void UseSiReadPaReferral2()
  {
    var useImport = new SiReadPaReferral2.Import();
    var useExport = new SiReadPaReferral2.Export();

    MovePaReferral2(import.PaReferral, useImport.PaReferral);

    Call(SiReadPaReferral2.Execute, useImport, useExport);

    export.Employer.Assign(useExport.Employer);
    MovePaReferral1(useExport.PaReferral, export.PaReferral);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ApEmpPhone.
    /// </summary>
    [JsonPropertyName("apEmpPhone")]
    public WorkArea ApEmpPhone
    {
      get => apEmpPhone ??= new();
      set => apEmpPhone = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public PaParticipantAddress Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private WorkArea apEmpPhone;
    private PaParticipantAddress employer;
    private PaReferral paReferral;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ApEmpPhone.
    /// </summary>
    [JsonPropertyName("apEmpPhone")]
    public WorkArea ApEmpPhone
    {
      get => apEmpPhone ??= new();
      set => apEmpPhone = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public PaParticipantAddress Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private WorkArea apEmpPhone;
    private PaParticipantAddress employer;
    private PaReferral paReferral;
    private NextTranInfo hidden;
    private Standard standard;
  }
#endregion
}

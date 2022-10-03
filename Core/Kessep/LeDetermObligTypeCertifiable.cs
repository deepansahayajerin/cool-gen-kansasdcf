// Program: LE_DETERM_OBLIG_TYPE_CERTIFIABLE, ID: 372661649, model: 746.
// Short name: SWE00760
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_DETERM_OBLIG_TYPE_CERTIFIABLE.
/// </summary>
[Serializable]
public partial class LeDetermObligTypeCertifiable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DETERM_OBLIG_TYPE_CERTIFIABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDetermObligTypeCertifiable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDetermObligTypeCertifiable.
  /// </summary>
  public LeDetermObligTypeCertifiable(IContext context, Import import,
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
    // -----------------------------------------------------------------------
    // MAINTENANCE LOG
    // DATE	DEVELOPER	DESCRIPTION
    // 01/21/96   H HOOKS    INITIAL DEVELOPMENT
    // 04/21/97   Govind     Fixed for Ob Types  ARRJ, AP FEE, NA CRCH. Also 
    // included MC
    // 09/07/10  GVandy    CQ 20550 - Both Fee and Recovery obligations should 
    // be classified as recovery amounts for SDSO.
    // -----------------------------------------------------------------------
    export.ObligationTypeCertifiab.Flag = "N";
    export.RecoveryObligIndicator.Flag = "N";

    switch(TrimEnd(import.AdministrativeActCertification.Type1))
    {
      case "FDSO":
        // ----------------------------------------------------
        // THE FOLLOWING OBLIGATION TYPES ARE FDSO
        // CERTIFIABLE:
        // %UME:	PERCENT UNINSURED MED EXP PAYMENT
        // 718B:	718B URS JUDGEMENT
        // BDCK RC:BAD CHECK
        // CS:	CHILD SUPPORT
        // IJ:	INTEREST JUDGEMENT
        // MJ:	MEDICAL JUDGEMENT
        // MS:	MEDICAL SUPPORT
        // CRCH:	NON-ADC COST OF RAISING CHILD
        // AJ:	ARREARS JUDGEMENT
        // IF THE IMPORTED OBLIGATION TYPE IS ONE OF THESE,
        // THEN SET THE EXPORT CERTIFIABLE INDICATOR TO 'Y'.
        // ----------------------------------------------------
        if (Equal(import.ObligationType.Code, "%UME") || Equal
          (import.ObligationType.Code, "718B") || Equal
          (import.ObligationType.Code, "BDCK RC") || Equal
          (import.ObligationType.Code, "CS") || Equal
          (import.ObligationType.Code, "IJ") || Equal
          (import.ObligationType.Code, "MJ") || Equal
          (import.ObligationType.Code, "MS") || Equal
          (import.ObligationType.Code, "MC") || Equal
          (import.ObligationType.Code, "CRCH") || Equal
          (import.ObligationType.Code, "AJ"))
        {
          export.ObligationTypeCertifiab.Flag = "Y";
        }

        if (AsChar(import.CourtOrdSuppIndicator.Flag) == 'Y')
        {
          // ---------------------------------------------
          // THE FOLLOWING OBLIGATION TYPES ARE FDSO
          // CERTIFIABLE IF SUPPORTED BY A COURT ORDER:
          // IVD RC:	IV-D RECOVERY
          // SP:	SPOUSAL SUPPORT
          // SAJ:	SPOUSAL ARREARS JUDGEMENT
          // IF THE IMPORTED COURT ORDER SUPPORT INDICATOR
          // IS EQUAL TO 'Y', AND THE IMPORTED OBLIGATION
          // TYPE IS ONE OF THESE, THEN SET THE EXPORT
          // CERTIFIABLE INDICATOR TO 'Y'.
          // NOTE THAT SPOUSAL SUPPORT AND SPOUSAL ARREARS
          // JUDGEMENT ADDITIONALLY REQUIRED THAT A CHILD
          // SUPPORT OBLIGATION BE CONTAINED AS PART OF
          // THE SAME COURT ORDER.  THIS VALIDATION MUST
          // BE DONE BY THE CALLING PROCEDURE WHENEVER THE
          // OBLIGATION TYPE IS EQUAL TO 'SP' OR 'SAJ'.
          // ---------------------------------------------
          if (Equal(import.ObligationType.Code, "IVD RC") || Equal
            (import.ObligationType.Code, "SP") || Equal
            (import.ObligationType.Code, "SAJ"))
          {
            export.ObligationTypeCertifiab.Flag = "Y";
          }
        }

        break;
      case "SDSO":
        // ---------------------------------------------
        // THE FOLLOWING OBLIGATION TYPES ARE SDSO
        // CERTIFIABLE:
        // %UME:	PERCENT UNINSURED MED EXP PAYMENT
        // 718B:	718B URS JUDGEMENT
        // FEE:	ABSENT PARENT FEE
        // BDCK RC:BAD CHECK
        // CS:	CHILD SUPPORT
        // IJ:	INTEREST JUDGEMENT
        // IRS NEG:1040X RECOVERY
        // IVD RC:	IV-D RECOVERY
        // MIS AP:	AP MISDIRECTED
        // MIS AR:	AR MISDIRECTED
        // MIS NON:NON-CSE MISDIRECTED PAYMENT
        // MJ:	MEDICAL JUDGEMENT
        // MS:	MEDICAL SUPPORT
        // MC:	Medical Costs
        // CRCH:	NON-ADC COST OF RAISING CHILD
        // SP:	SPOUSAL SUPPORT
        // AJ:	ARREARS JUDGEMENT
        // SAJ:	SPOUSAL ARREARS JUDGEMENT
        // IF THE IMPORTED OBLIGATION TYPE IS ONE OF THESE,
        // THEN SET THE EXPORT CERTIFIABLE INDICATOR TO 'Y'.
        // ---------------------------------------------
        if (Equal(import.ObligationType.Code, "%UME") || Equal
          (import.ObligationType.Code, "718B") || Equal
          (import.ObligationType.Code, "FEE") || Equal
          (import.ObligationType.Code, "BDCK RC") || Equal
          (import.ObligationType.Code, "CS") || Equal
          (import.ObligationType.Code, "IJ") || Equal
          (import.ObligationType.Code, "IRS NEG") || Equal
          (import.ObligationType.Code, "IVD RC") || Equal
          (import.ObligationType.Code, "MIS AP") || Equal
          (import.ObligationType.Code, "MIS AR") || Equal
          (import.ObligationType.Code, "MIS NON") || Equal
          (import.ObligationType.Code, "MJ") || Equal
          (import.ObligationType.Code, "MS") || Equal
          (import.ObligationType.Code, "MC") || Equal
          (import.ObligationType.Code, "CRCH") || Equal
          (import.ObligationType.Code, "SP") || Equal
          (import.ObligationType.Code, "AJ") || Equal
          (import.ObligationType.Code, "SAJ"))
        {
          export.ObligationTypeCertifiab.Flag = "Y";
        }

        // ---------------------------------------------
        // THE FOLLOWING OBLIGATION TYPES ARE RECOVERY
        // RELATED FOR SDSO CERTIFICATIONS:
        // IRS NEG - 1040X RECOVERY
        // IVD RC  - IV-D RECOVERY
        // IF THE IMPORTED OBLIGATION TYPE IS ONE OF
        // THESE, SET THE EXPORT RECOVERY OBLIGATION
        // INDICATOR TO 'Y'.
        // ---------------------------------------------
        // =================================================
        // 7/8/99 - b adams  -  Removed Read of Obligation_Type.
        //   imported classification.  no need
        // =================================================
        // 09/07/10  GVandy    CQ 20550 - Both Fee and Recovery obligations 
        // should be classified as recovery amounts for SDSO.
        if (AsChar(import.ObligationType.Classification) == 'R' || AsChar
          (import.ObligationType.Classification) == 'F')
        {
          export.RecoveryObligIndicator.Flag = "Y";
          export.CsObligIndicator.Flag = "";
        }
        else
        {
          export.CsObligIndicator.Flag = "Y";
          export.RecoveryObligIndicator.Flag = "";
        }

        break;
      case "CRED":
        export.ObligationTypeCertifiab.Flag = "Y";

        break;
      case "COAG":
        export.ObligationTypeCertifiab.Flag = "Y";

        break;
      default:
        break;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of CourtOrdSuppIndicator.
    /// </summary>
    [JsonPropertyName("courtOrdSuppIndicator")]
    public Common CourtOrdSuppIndicator
    {
      get => courtOrdSuppIndicator ??= new();
      set => courtOrdSuppIndicator = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private AdministrativeActCertification administrativeActCertification;
    private Common courtOrdSuppIndicator;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsObligIndicator.
    /// </summary>
    [JsonPropertyName("csObligIndicator")]
    public Common CsObligIndicator
    {
      get => csObligIndicator ??= new();
      set => csObligIndicator = value;
    }

    /// <summary>
    /// A value of RecoveryObligIndicator.
    /// </summary>
    [JsonPropertyName("recoveryObligIndicator")]
    public Common RecoveryObligIndicator
    {
      get => recoveryObligIndicator ??= new();
      set => recoveryObligIndicator = value;
    }

    /// <summary>
    /// A value of ObligationTypeCertifiab.
    /// </summary>
    [JsonPropertyName("obligationTypeCertifiab")]
    public Common ObligationTypeCertifiab
    {
      get => obligationTypeCertifiab ??= new();
      set => obligationTypeCertifiab = value;
    }

    private Common csObligIndicator;
    private Common recoveryObligIndicator;
    private Common obligationTypeCertifiab;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private ObligationType obligationType;
  }
#endregion
}

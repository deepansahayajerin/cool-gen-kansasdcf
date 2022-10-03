// Program: SP_DOC_SET_LITERALS, ID: 371812853, model: 746.
// Short name: SWE02275
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DOC_SET_LITERALS.
/// </summary>
[Serializable]
public partial class SpDocSetLiterals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_SET_LITERALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocSetLiterals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocSetLiterals.
  /// </summary>
  public SpDocSetLiterals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date      Developer           Request #         Description
    // ----------------------------------------------------------------------
    // 12/16/98  Michael Ramirez                       Initial Development
    // ----------------------------------------------------------------------
    // mjr
    // -----------------------------------------------------
    // Literals which show on DKEY
    // --------------------------------------------------------
    export.SpDocLiteral.ScreenAdminAction = "ADMIN ACTION TYPE:";
    export.SpDocLiteral.ScreenAdminActCert =
      "ADMIN ACTION CERTFCTN TAKEN DATE:";
    export.SpDocLiteral.ScreenAdminAppeal = "ADMIN APPEAL IDENTIFIER:";
    export.SpDocLiteral.ScreenApNumber = "AP PERSON NUMBER:";
    export.SpDocLiteral.ScreenAppointment = "APPOINTMENT IDENTIFIER:";
    export.SpDocLiteral.ScreenArNumber = "AR PERSON NUMBER:";
    export.SpDocLiteral.ScreenBankruptcy = "BANKRUPTCY IDENTIFIER:";
    export.SpDocLiteral.ScreenCaseNumber = "CASE NUMBER:";
    export.SpDocLiteral.ScreenCashRcptDetail = "CASH RECEIPT DETAIL:";
    export.SpDocLiteral.ScreenCashRcptEvent = "CASH RECEIPT EVENT:";
    export.SpDocLiteral.ScreenCashRcptSource = "CASH RECEIPT SOURCE:";
    export.SpDocLiteral.ScreenCashRcptType = "CASH RECEIPT TYPE:";
    export.SpDocLiteral.ScreenChNumber = "CH PERSON NUMBER:";
    export.SpDocLiteral.ScreenContact = "CONTACT NUMBER:";
    export.SpDocLiteral.ScreenGenetic = "GENETIC TEST NUMBER:";
    export.SpDocLiteral.ScreenHealthInsCoverage = "HEALTH INS COVERAGE:";
    export.SpDocLiteral.ScreenIncomeSource = "INCOME SOURCE IDENTIFIER:";
    export.SpDocLiteral.ScreenInfoRequest = "INFORMATION REQUEST IDENTIFIER:";
    export.SpDocLiteral.ScreenInterstateRequest = "INTERSTATE REQUEST:";
    export.SpDocLiteral.ScreenJail = "INCARCERATION INDENTIFIER:";
    export.SpDocLiteral.ScreenLegalAction = "LEGAL ACTION IDENTIFIER:";
    export.SpDocLiteral.ScreenLegalActionDetail = "LEGAL DETAIL:";
    export.SpDocLiteral.ScreenLegalReferral = "LEGAL REFERRAL:";
    export.SpDocLiteral.ScreenLocateRequestAgency =
      "LOCATE REQUEST AGENCY IDENTIFIER:";
    export.SpDocLiteral.ScreenLocateRequestSource =
      "LOCATE REQUEST SOURCE IDENTIFIER:";
    export.SpDocLiteral.ScreenMilitary = "MILITARY IDENTIFIER:";
    export.SpDocLiteral.ScreenObligation = "OBLIGATION ID:";
    export.SpDocLiteral.ScreenObligationAdminAction =
      "OBLIGATION ADMIN ACTION TAKEN DT:";
    export.SpDocLiteral.ScreenObligationType = "OBLIGATION TYPE ID:";
    export.SpDocLiteral.ScreenObligor = "OBLIGOR NUMBER:";
    export.SpDocLiteral.ScreenPersonAcct = "OBLIGOR/OBLIGEE/SUPPORTED:";
    export.SpDocLiteral.ScreenPersonAddress = "PERSON ADDRESS:";
    export.SpDocLiteral.ScreenPrNumber = "PR PERSON NUMBER:";
    export.SpDocLiteral.ScreenRecaptureRule = "RECAPTURE RULE:";
    export.SpDocLiteral.ScreenResource = "RESOURCE NUMBER:";
    export.SpDocLiteral.ScreenTribunal = "TRIBUNAL IDENTIFIER:";
    export.SpDocLiteral.ScreenWorkerComp = "WORKER COMP INSURANCE:";
    export.SpDocLiteral.ScreenWorksheet = "CHILD SUPPORT WORKSHEET:";

    // mjr
    // -----------------------------------------------------
    // Literals which mark identifiers in misc_text_1
    // --------------------------------------------------------
    export.SpDocLiteral.IdAdminAction = "ADMACT:";
    export.SpDocLiteral.IdAdminActCert = "AACTAKEN:";
    export.SpDocLiteral.IdAppointment = "APPT:";
    export.SpDocLiteral.IdBankruptcy = "BKRP:";
    export.SpDocLiteral.IdCashRcptDetail = "CASHDETL:";
    export.SpDocLiteral.IdCashRcptEvent = "CASHEVTN:";
    export.SpDocLiteral.IdCashRcptSource = "CASHSRCE:";
    export.SpDocLiteral.IdCashRcptType = "CASHTYPE:";
    export.SpDocLiteral.IdChNumber = "CHILD:";
    export.SpDocLiteral.IdContact = "CONTACT:";
    export.SpDocLiteral.IdDocument = "PRINT:";
    export.SpDocLiteral.IdGenetic = "GENETIC:";
    export.SpDocLiteral.IdHealthInsCoverage = "HINS:";
    export.SpDocLiteral.IdIncomeSource = "INCS:";
    export.SpDocLiteral.IdInfoRequest = "INRD:";
    export.SpDocLiteral.IdInterstateRequest = "INTSTREQ:";
    export.SpDocLiteral.IdJail = "JAIL:";
    export.SpDocLiteral.IdLegalActionDetail = "LDET:";
    export.SpDocLiteral.IdLegalReferral = "LGLREF:";
    export.SpDocLiteral.IdLocateRequestAgency = "LOCRQAGN:";
    export.SpDocLiteral.IdLocateRequestSource = "LOCRQSRC:";
    export.SpDocLiteral.IdMilitary = "MILITARY:";
    export.SpDocLiteral.IdObligationAdminAction = "OOATAKEN:";
    export.SpDocLiteral.IdObligationType = "OBLTYPE:";
    export.SpDocLiteral.IdPersonAcct = "PERSACCT:";
    export.SpDocLiteral.IdPersonAddress = "PRADD:";
    export.SpDocLiteral.IdPrNumber = "PERSON:";
    export.SpDocLiteral.IdRecaptureRule = "RCAPTURE:";
    export.SpDocLiteral.IdResource = "RESOURCE:";
    export.SpDocLiteral.IdTribunal = "TRIBUNAL:";
    export.SpDocLiteral.IdWorkerComp = "WKRCMP:";
    export.SpDocLiteral.IdWorksheet = "WRKSHEET:";
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    private SpDocLiteral spDocLiteral;
  }
#endregion
}

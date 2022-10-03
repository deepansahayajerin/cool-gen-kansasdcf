// Program: SI_QUICK_FINANCIAL_RETR_MAIN, ID: 374541607, model: 746.
// Short name: SWEQKFNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_FINANCIAL_RETR_MAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class SiQuickFinancialRetrMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_FINANCIAL_RETR_MAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickFinancialRetrMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickFinancialRetrMain.
  /// </summary>
  public SiQuickFinancialRetrMain(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    // This procedure step functions as a non-display server
    // and is the target of a QUICK application COM proxy.
    // Any changes to the import/export views of this procedure
    // step MUST be coordinated, as such changes will impact the
    // calling COM proxy.
    // ------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 01/2010    	T. Pierce	# 211		Initial development
    // ----------------------------------------------------------------------------
    UseSiQuickFinancial();
  }

  private static void MoveDisbursements(SiQuickFinancial.Export.
    DisbursementsGroup source, Export.DisbursementsGroup target)
  {
    target.QuickFinanceDisbursement.Assign(source.QuickFinanceDisbursement);
  }

  private static void MovePayments(SiQuickFinancial.Export.PaymentsGroup source,
    Export.PaymentsGroup target)
  {
    target.QuickFinancePayment.Assign(source.QuickFinancePayment);
  }

  private static void MoveQuickErrorMessages(QuickErrorMessages source,
    QuickErrorMessages target)
  {
    target.ErrorMessage = source.ErrorMessage;
    target.ErrorCode = source.ErrorCode;
  }

  private void UseSiQuickFinancial()
  {
    var useImport = new SiQuickFinancial.Import();
    var useExport = new SiQuickFinancial.Export();

    useImport.QuickInQuery.Assign(import.QuickInQuery);

    Call(SiQuickFinancial.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
    MoveQuickErrorMessages(useExport.QuickErrorMessages,
      export.QuickErrorMessages);
    export.QuickFinanceSummary.Assign(useExport.QuickFinanceSummary);
    useExport.Disbursements.CopyTo(export.Disbursements, MoveDisbursements);
    useExport.Payments.CopyTo(export.Payments, MovePayments);
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PaymentsGroup group.</summary>
    [Serializable]
    public class PaymentsGroup
    {
      /// <summary>
      /// A value of QuickFinancePayment.
      /// </summary>
      [JsonPropertyName("quickFinancePayment")]
      public QuickFinancePayment QuickFinancePayment
      {
        get => quickFinancePayment ??= new();
        set => quickFinancePayment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinancePayment quickFinancePayment;
    }

    /// <summary>A DisbursementsGroup group.</summary>
    [Serializable]
    public class DisbursementsGroup
    {
      /// <summary>
      /// A value of QuickFinanceDisbursement.
      /// </summary>
      [JsonPropertyName("quickFinanceDisbursement")]
      public QuickFinanceDisbursement QuickFinanceDisbursement
      {
        get => quickFinanceDisbursement ??= new();
        set => quickFinanceDisbursement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinanceDisbursement quickFinanceDisbursement;
    }

    /// <summary>
    /// Gets a value of Payments.
    /// </summary>
    [JsonIgnore]
    public Array<PaymentsGroup> Payments => payments ??= new(
      PaymentsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Payments for json serialization.
    /// </summary>
    [JsonPropertyName("payments")]
    [Computed]
    public IList<PaymentsGroup> Payments_Json
    {
      get => payments;
      set => Payments.Assign(value);
    }

    /// <summary>
    /// Gets a value of Disbursements.
    /// </summary>
    [JsonIgnore]
    public Array<DisbursementsGroup> Disbursements => disbursements ??= new(
      DisbursementsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Disbursements for json serialization.
    /// </summary>
    [JsonPropertyName("disbursements")]
    [Computed]
    public IList<DisbursementsGroup> Disbursements_Json
    {
      get => disbursements;
      set => Disbursements.Assign(value);
    }

    /// <summary>
    /// A value of QuickFinanceSummary.
    /// </summary>
    [JsonPropertyName("quickFinanceSummary")]
    public QuickFinanceSummary QuickFinanceSummary
    {
      get => quickFinanceSummary ??= new();
      set => quickFinanceSummary = value;
    }

    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
    }

    /// <summary>
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
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

    private Array<PaymentsGroup> payments;
    private Array<DisbursementsGroup> disbursements;
    private QuickFinanceSummary quickFinanceSummary;
    private QuickErrorMessages quickErrorMessages;
    private QuickCpHeader quickCpHeader;
    private Case1 case1;
  }
#endregion
}

// Program: GENERATE_FUND_TRANSACTION_ID, ID: 371725881, model: 746.
// Short name: SWE00700
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GENERATE_FUND_TRANSACTION_ID.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This action block will generate the system identifer for a fund transaction.
/// </para>
/// </summary>
[Serializable]
public partial class GenerateFundTransactionId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GENERATE_FUND_TRANSACTION_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GenerateFundTransactionId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GenerateFundTransactionId.
  /// </summary>
  public GenerateFundTransactionId(IContext context, Import import,
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
    export.FundTransaction.SystemGeneratedIdentifier =
      UseGenerate9DigitRandomNumber();
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    private FundTransaction fundTransaction;
  }
#endregion
}

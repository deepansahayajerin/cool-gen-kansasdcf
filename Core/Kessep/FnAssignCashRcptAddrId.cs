// Program: FN_ASSIGN_CASH_RCPT_ADDR_ID, ID: 371726407, model: 746.
// Short name: SWE00267
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_CASH_RCPT_ADDR_ID.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This action block assigns the next system id for a cash receipt detail 
/// address.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignCashRcptAddrId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_CASH_RCPT_ADDR_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignCashRcptAddrId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignCashRcptAddrId.
  /// </summary>
  public FnAssignCashRcptAddrId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CashReceiptDetailAddress.SystemGeneratedIdentifier = Now();
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }
#endregion
}

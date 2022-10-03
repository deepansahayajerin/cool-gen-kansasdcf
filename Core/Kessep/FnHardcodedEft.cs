// Program: FN_HARDCODED_EFT, ID: 372407789, model: 746.
// Short name: SWE02048
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_HARDCODED_EFT.
/// </summary>
[Serializable]
public partial class FnHardcodedEft: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODED_EFT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodedEft(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodedEft.
  /// </summary>
  public FnHardcodedEft(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****
    // EFT umb bank number and name
    // *****
    export.StateRouting.CompanyName = "State of KANSAS";
    export.StateRouting.OriginatingDfiIdentification = 999999999;

    // *****
    // EFT statuses
    // *****
    export.Req.TransmissionStatusCode = "REQ";
    export.Paid.TransmissionStatusCode = "PAID";
    export.Error.TransmissionStatusCode = "ERROR";
    export.Retr.TransmissionStatusCode = "RETR";
    export.Chwar.TransmissionStatusCode = "CHWAR";
    export.Can.TransmissionStatusCode = "CAN";

    // *****
    // EFT TRANSMISSION TYPES
    // *****
    export.Inbound.TransmissionType = "I";
    export.Outbound.TransmissionType = "O";

    // *****
    // Set EFT Source Type
    // *****
    export.Eft.SystemGeneratedIdentifier = 306;
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
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public CashReceiptSourceType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of Outbound.
    /// </summary>
    [JsonPropertyName("outbound")]
    public ElectronicFundTransmission Outbound
    {
      get => outbound ??= new();
      set => outbound = value;
    }

    /// <summary>
    /// A value of Inbound.
    /// </summary>
    [JsonPropertyName("inbound")]
    public ElectronicFundTransmission Inbound
    {
      get => inbound ??= new();
      set => inbound = value;
    }

    /// <summary>
    /// A value of Can.
    /// </summary>
    [JsonPropertyName("can")]
    public ElectronicFundTransmission Can
    {
      get => can ??= new();
      set => can = value;
    }

    /// <summary>
    /// A value of Chwar.
    /// </summary>
    [JsonPropertyName("chwar")]
    public ElectronicFundTransmission Chwar
    {
      get => chwar ??= new();
      set => chwar = value;
    }

    /// <summary>
    /// A value of Retr.
    /// </summary>
    [JsonPropertyName("retr")]
    public ElectronicFundTransmission Retr
    {
      get => retr ??= new();
      set => retr = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ElectronicFundTransmission Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public ElectronicFundTransmission Paid
    {
      get => paid ??= new();
      set => paid = value;
    }

    /// <summary>
    /// A value of Req.
    /// </summary>
    [JsonPropertyName("req")]
    public ElectronicFundTransmission Req
    {
      get => req ??= new();
      set => req = value;
    }

    /// <summary>
    /// A value of StateRouting.
    /// </summary>
    [JsonPropertyName("stateRouting")]
    public ElectronicFundTransmission StateRouting
    {
      get => stateRouting ??= new();
      set => stateRouting = value;
    }

    private CashReceiptSourceType eft;
    private ElectronicFundTransmission outbound;
    private ElectronicFundTransmission inbound;
    private ElectronicFundTransmission can;
    private ElectronicFundTransmission chwar;
    private ElectronicFundTransmission retr;
    private ElectronicFundTransmission error;
    private ElectronicFundTransmission paid;
    private ElectronicFundTransmission req;
    private ElectronicFundTransmission stateRouting;
  }
#endregion
}

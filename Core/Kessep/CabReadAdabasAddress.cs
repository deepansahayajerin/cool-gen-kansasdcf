// Program: CAB_READ_ADABAS_ADDRESS, ID: 371727806, model: 746.
// Short name: SWE00067
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADABAS_ADDRESS.
/// </para>
/// <para>
/// This AB calls the EAB which retrieves a person's address from AE if it 
/// exists.  Any error codes are interpreted here
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdabasAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADABAS_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdabasAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdabasAddress.
  /// </summary>
  public CabReadAdabasAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Current.Date = Now().Date;
    UseEabReadAdabasAddress();

    switch(AsChar(export.AbendData.Type1))
    {
      case 'A':
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
          case "0150":
            ExitState = "ADABAS_ADDRESS_NF_150";

            break;
          default:
            break;
        }

        break;
      case 'C':
        ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

        break;
      default:
        break;
    }
  }

  private void UseEabReadAdabasAddress()
  {
    var useImport = new EabReadAdabasAddress.Import();
    var useExport = new EabReadAdabasAddress.Export();

    useImport.AddressType.Flag = import.AddressType.Flag;
    useImport.Current.Date = local.Current.Date;
    useImport.Ae.Number = import.CsePersonsWorkSet.Number;
    useExport.Ae.Assign(export.Ae);

    Call(EabReadAdabasAddress.Execute, useImport, useExport);

    export.Ae.Assign(useExport.Ae);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of AddressType.
    /// </summary>
    [JsonPropertyName("addressType")]
    public Common AddressType
    {
      get => addressType ??= new();
      set => addressType = value;
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

    private Common addressType;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public CsePersonAddress Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    private AbendData abendData;
    private CsePersonAddress ae;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }
#endregion
}

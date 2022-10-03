// Program: CAB_READ_ADABAS_PERSON_BATCH, ID: 371790661, model: 746.
// Short name: SWE00069
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADABAS_PERSON_BATCH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// 5/27/96     Use for Batch Processing Only
///             ______________________________
/// This Common Action Block(CAB) is designed to call an External Action Block(
/// EAB) which reads a CSE_PERSON from the ADABAS system and provides an
/// interpretation of any error information returned.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdabasPersonBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADABAS_PERSON_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdabasPersonBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdabasPersonBatch.
  /// </summary>
  public CabReadAdabasPersonBatch(IContext context, Import import, Export export)
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
    // ************************************************
    // *         M A I N T E N A N C E   L O G        *
    // * Date   Developer	Description
    // *
    // * 5-28-96 Tom Redmond	Initial Development
    // * 3-18-98  R Grey	Modify interpretation of ADABAS response
    // 			codes.
    // ************************************************
    UseEabReadCsePersonBatch();

    // ************************************************
    // *Interpret the error codes returned from ADABAS*
    // *and set an appropriate exit state.            *
    // ************************************************
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // ************************************************
        // *Successfull Adabas Read Occurred.             *
        // ************************************************
        break;
      case 'A':
        // ************************************************
        // *Unsuccessfull ADABAS Read Occurred.           *
        // ************************************************
        if (Equal(export.AbendData.AdabasResponseCd, "0148"))
        {
          export.CsePersonsWorkSet.LastName = "*ADABAS UNAVAIL*";
          export.CsePersonsWorkSet.FormattedName = "** ADABAS UNAVAILABLE **";
          ExitState = "ADABAS_UNAVAILABLE_RB";
        }
        else
        {
          ExitState = "ADABAS_READ_UNSUCCESSFUL";
        }

        break;
      case 'C':
        // ************************************************
        // *CICS action Failed. A reason code should be   *
        // *interpreted.
        // 
        // *
        // ************************************************
        if (IsEmpty(export.AbendData.CicsResponseCd))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
        }

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useExport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);
    useExport.Ae.Flag = export.Ae.Flag;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.AbendData.Assign(useExport.AbendData);
    export.Ae.Flag = useExport.Ae.Flag;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errOnAdabasUnavailable;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common ae;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
  }
#endregion
}

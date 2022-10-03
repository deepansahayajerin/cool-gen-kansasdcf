// Program: CAB_READ_ADABAS_PERSON, ID: 371730106, model: 746.
// Short name: SWE00068
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADABAS_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will call the EAB which reads a CSE Person from ADABAS and
/// interpret any error information returned.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdabasPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADABAS_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdabasPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdabasPerson.
  /// </summary>
  public CabReadAdabasPerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date     Developer		Request #  Description
    // ??/??/??  Unknown		        0  Initial Development
    // 06/29/96  G. Lofton			   Added eab to pad # with zeroes
    // ---------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------
    // 09/16/04    M Quinn   206830    Removed code that was causing a KsCares 
    // open case to be
    // 
    // given the same update protection as an AE case.
    // 
    // -------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = import.CsePersonsWorkSet.Number;
    }
    else
    {
      export.CsePersonsWorkSet.Number = "";
      ExitState = "CSE_PERSON_NF";

      return;
    }

    UseEabPadLeftWithZeros();
    export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    UseEabReadCsePerson();

    // ---------------------------------------------
    // Interpret the error code returned from ADABAS
    // and set the appropriate exit state.
    // ---------------------------------------------
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // --------------------------------------------
        // 	Successful read of data
        // --------------------------------------------
        // 09/16/2004  M Quinn   Removed the code that was setting export_ae 
        // ief_supplied flag
        // 
        // to O if export_kscares ief_supplied flag was O
        break;
      case 'A':
        // --------------------------------------------
        // ADABAS Read failed.  A reason code should be
        // interpreted.
        // --------------------------------------------
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            if (AsChar(import.ErrOnAdabasUnavailable.Flag) == 'Y')
            {
              ExitState = "ACO_ADABAS_UNAVAILABLE";
            }
            else
            {
              export.CsePersonsWorkSet.LastName = "*ADABAS UNAVAIL*";
              export.CsePersonsWorkSet.FormattedName =
                "** ADABAS UNAVAILABLE **";
            }

            break;
          case "0113":
            ExitState = "ACO_ADABAS_PERSON_NF_113";

            break;
          case "0114":
            ExitState = "ACO_ADABAS_PERSON_NF_114";

            break;
          case "0149":
            ExitState = "ACO_ADABAS_PERSON_NF_149";

            break;
          case "0154":
            ExitState = "ACO_ADABAS_PERSON_NF_154";

            break;
          case "0161":
            ExitState = "ACO_ADABAS_PERSON_NF_161";

            break;
          default:
            ExitState = "ADABAS_INVALID_RETURN_CODE";

            break;
        }

        break;
      case 'C':
        // --------------------------------------------
        // CICS action failed.  A reason code should be
        // interpreted.
        // --------------------------------------------
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
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Current.Date = local.Current.Date;
    useExport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);
    useExport.Ae.Flag = export.Ae.Flag;
    useExport.Cse.Flag = export.Cse.Flag;
    useExport.Kanpay.Flag = export.Kanpay.Flag;
    useExport.Kscares.Flag = export.Kscares.Flag;

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.AbendData.Assign(useExport.AbendData);
    export.Ae.Flag = useExport.Ae.Flag;
    export.Cse.Flag = useExport.Cse.Flag;
    export.Kanpay.Flag = useExport.Kanpay.Flag;
    export.Kscares.Flag = useExport.Kscares.Flag;
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
    /// A value of Kanpay.
    /// </summary>
    [JsonPropertyName("kanpay")]
    public Common Kanpay
    {
      get => kanpay ??= new();
      set => kanpay = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

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

    private Common kanpay;
    private Common cse;
    private Common kscares;
    private Common ae;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
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

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private DateWorkArea current;
    private TextWorkArea textWorkArea;
  }
#endregion
}

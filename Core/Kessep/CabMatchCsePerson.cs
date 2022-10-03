// Program: CAB_MATCH_CSE_PERSON, ID: 371455758, model: 746.
// Short name: SWE00066
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_MATCH_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block calls the external to ADABAS which searches for potential 
/// matches using either a Name or a Social Security Number.
/// Abend information is also interpreted here.
/// </para>
/// </summary>
[Serializable]
public partial class CabMatchCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_MATCH_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabMatchCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabMatchCsePerson.
  /// </summary>
  public CabMatchCsePerson(IContext context, Import import, Export export):
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
    UseEabMatchCsePersons();

    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // ---------------------------------------------
        // Matches found
        // ---------------------------------------------
        break;
      case 'A':
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
          case "0149":
            switch(TrimEnd(export.AbendData.AdabasFileAction))
            {
              case "AVF":
                ExitState = "ACO_ADABAS_NO_EXACT_MATCHES";

                break;
              case "BVF":
                ExitState = "ACO_ADABAS_NO_PHONETIC_MATCH";

                break;
              case "CVF":
                ExitState = "ACO_ADABAS_NO_SSN_MATCH";

                break;
              default:
                break;
            }

            break;
          default:
            break;
        }

        break;
      case 'C':
        ExitState = "ACO_RE0000_CICS_UNAVAILABLE_RB";

        break;
      default:
        break;
    }
  }

  private static void MoveExport2(Export.ExportGroup source,
    EabMatchCsePersons.Export.ExportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Ae.Flag = source.Ae.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Kscares.Flag = source.Kscares.Flag;
    target.Alt.Flag = source.Alt.Flag;
  }

  private static void MoveExport3(EabMatchCsePersons.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Ae.Flag = source.Ae.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Kscares.Flag = source.Kscares.Flag;
    target.Alt.Flag = source.Alt.Flag;
  }

  private void UseEabMatchCsePersons()
  {
    var useImport = new EabMatchCsePersons.Import();
    var useExport = new EabMatchCsePersons.Export();

    useImport.Start.UniqueKey = import.Start.UniqueKey;
    useImport.Phonetic.Percentage = import.Phonetic.Percentage;
    useImport.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    useImport.Search.Flag = import.Search.Flag;
    useImport.Current.Date = local.Current.Date;
    useExport.Next.UniqueKey = export.Next.UniqueKey;
    useExport.AbendData.Assign(export.AbendData);
    export.Export1.CopyTo(useExport.Export1, MoveExport2);

    Call(EabMatchCsePersons.Execute, useImport, useExport);

    export.Next.UniqueKey = useExport.Next.UniqueKey;
    export.AbendData.Assign(useExport.AbendData);
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public CsePersonsWorkSet Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
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

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    private CsePersonsWorkSet start;
    private Common search;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common phonetic;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
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
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

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
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
      private Common alt;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public CsePersonsWorkSet Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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

    private CsePersonsWorkSet next;
    private Array<ExportGroup> export1;
    private AbendData abendData;
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

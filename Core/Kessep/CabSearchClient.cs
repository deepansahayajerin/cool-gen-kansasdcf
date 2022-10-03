// Program: CAB_SEARCH_CLIENT, ID: 373427464, model: 746.
// Short name: SWE00380
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_SEARCH_CLIENT.
/// </summary>
[Serializable]
public partial class CabSearchClient: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SEARCH_CLIENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSearchClient(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSearchClient.
  /// </summary>
  public CabSearchClient(IContext context, Import import, Export export):
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
    UseEabSearchClient();

    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // ---------------------------------------------
        // Matches found
        // ---------------------------------------------
        break;
      case 'C':
        ExitState = "ACO_RE0000_CICS_UNAVAILABLE_RB";

        break;
      case 'D':
        ExitState = "DB2_ERROR_RETURNED";

        break;
      case 'A':
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
          case "DVF":
            break;
          case "INI":
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
          default:
            break;
        }

        break;
      default:
        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveExport2(Export.ExportGroup source,
    EabSearchClient.Export.ExportGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.Detail.Assign(source.Detail);
    MoveCommon(source.Alt, target.Alt);
    target.Kscares.Flag = source.Kscares.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Ae.Flag = source.Ae.Flag;
    target.Facts.Flag = source.Facts.Flag;
  }

  private static void MoveExport3(EabSearchClient.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.Detail.Assign(source.Detail);
    MoveCommon(source.Alt, target.Alt);
    target.Kscares.Flag = source.Kscares.Flag;
    target.Kanpay.Flag = source.Kanpay.Flag;
    target.Cse.Flag = source.Cse.Flag;
    target.Ae.Flag = source.Ae.Flag;
    target.Facts.Flag = source.Facts.Flag;
  }

  private void UseEabSearchClient()
  {
    var useImport = new EabSearchClient.Import();
    var useExport = new EabSearchClient.Export();

    useImport.Start.UniqueKey = import.Start.UniqueKey;
    useImport.Phonetic.Percentage = import.Phonetic.Percentage;
    useImport.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    useImport.Search.Flag = import.Search.Flag;
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useExport.Next.UniqueKey = export.Next.UniqueKey;
    useExport.AbendData.Assign(export.AbendData);
    export.Export1.CopyTo(useExport.Export1, MoveExport2);

    Call(EabSearchClient.Execute, useImport, useExport);

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
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private CsePersonsWorkSet start;
    private Common phonetic;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common search;
    private Office office;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

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
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
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
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Facts.
      /// </summary>
      [JsonPropertyName("facts")]
      public Common Facts
      {
        get => facts ??= new();
        set => facts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 117;

      private Common g;
      private CsePersonsWorkSet detail;
      private Common alt;
      private Common kscares;
      private Common kanpay;
      private Common cse;
      private Common ae;
      private Common facts;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private CsePersonsWorkSet next;
    private AbendData abendData;
    private Array<ExportGroup> export1;
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

// Program: EAB_SEARCH_CLIENT_BATCH, ID: 1902497595, model: 746.
// Short name: SWEXGR09
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_SEARCH_CLIENT_BATCH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB retrieves all CSE PERSON matches from ADABAS, based on either a SSN
/// or a Last Name and First Name search.
/// An indicator should be sent to ADABAS to determine which type of search is 
/// required.
/// 1 - SSN search
/// 2 - Name search
/// </para>
/// </summary>
[Serializable]
public partial class EabSearchClientBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_SEARCH_CLIENT_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabSearchClientBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabSearchClientBatch.
  /// </summary>
  public EabSearchClientBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXGR09", context, import, export, EabOptions.Hpvp);
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    [Member(Index = 1, Members = new[] { "UniqueKey" })]
    public CsePersonsWorkSet Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    [Member(Index = 2, Members = new[] { "Percentage" })]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 3, Members = new[]
    {
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Sex",
      "Dob",
      "Number"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    [Member(Index = 4, Members = new[] { "Flag" })]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
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
      [Member(Index = 1, AccessFields = false, Members = new[] { "SelectChar" })]
        
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      [Member(Index = 2, Members = new[]
      {
        "Number",
        "Sex",
        "Dob",
        "Ssn",
        "FirstName",
        "MiddleInitial",
        "LastName",
        "FormattedName",
        "ReplicationIndicator"
      })]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      [Member(Index = 3, Members = new[] { "Flag", "SelectChar" })]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      [Member(Index = 4, Members = new[] { "Flag" })]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      [Member(Index = 5, Members = new[] { "Flag" })]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      [Member(Index = 6, Members = new[] { "Flag" })]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      [Member(Index = 7, Members = new[] { "Flag" })]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Facts.
      /// </summary>
      [JsonPropertyName("facts")]
      [Member(Index = 8, AccessFields = false, Members = new[] { "Flag" })]
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
    [Member(Index = 1, Members = new[] { "UniqueKey" })]
    public CsePersonsWorkSet Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 2, Members = new[]
    {
      "Type1",
      "AdabasFileNumber",
      "AdabasFileAction",
      "AdabasResponseCd",
      "CicsResourceNm",
      "CicsFunctionCd",
      "CicsResponseCd"
    })]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 3)]
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
#endregion
}

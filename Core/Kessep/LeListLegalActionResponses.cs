// Program: LE_LIST_LEGAL_ACTION_RESPONSES, ID: 371981366, model: 746.
// Short name: SWE00802
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LIST_LEGAL_ACTION_RESPONSES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This will list the Legal Action Responses related to a specific Legal 
/// Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeListLegalActionResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_LEGAL_ACTION_RESPONSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListLegalActionResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListLegalActionResponses.
  /// </summary>
  public LeListLegalActionResponses(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/20/95	Dave Allen			Initial Code
    // 11/18/98	P McElderry	None listed
    // Altered READ EACH statement so that timestamp is not part
    // of the qualifier and entities LEGAL_ACTION, TRIBUNAL, and
    // FIPS as they were already being read from the PSTEP.
    // Removed corresponding export views 12/08/98.
    // ------------------------------------------------------------
    if (Equal(import.CutOffDate.Date, null))
    {
      local.CutOffDate.Date = new DateTime(2099, 12, 31);
    }

    local.Common.Count = 0;

    export.LegalActResponse.Index = 0;
    export.LegalActResponse.Clear();

    foreach(var item in ReadLegalActionResponse())
    {
      if (Lt(local.CutOffDate.Date,
        Date(entities.LegalActionResponse.CreatedTstamp)))
      {
        export.LegalActResponse.Next();

        continue;
      }

      export.LegalActResponse.Update.LegalActionResponse.Assign(
        entities.LegalActionResponse);
      local.Common.Count = (int)((long)local.Common.Count + 1);
      export.LegalActResponse.Next();
    }

    if (local.Common.Count > 0)
    {
    }
    else
    {
      ExitState = "LEGAL_RESPONSE_NOT_AVAIL";
    }
  }

  private IEnumerable<bool> ReadLegalActionResponse()
  {
    return ReadEach("ReadLegalActionResponse",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        if (export.LegalActResponse.IsFull)
        {
          return false;
        }

        entities.LegalActionResponse.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionResponse.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.LegalActionResponse.Type1 = db.GetString(reader, 2);
        entities.LegalActionResponse.ReceivedDate = db.GetDate(reader, 3);
        entities.LegalActionResponse.LastName = db.GetString(reader, 4);
        entities.LegalActionResponse.FirstName = db.GetString(reader, 5);
        entities.LegalActionResponse.MiddleInitial =
          db.GetNullableString(reader, 6);
        entities.LegalActionResponse.Relationship =
          db.GetNullableString(reader, 7);
        entities.LegalActionResponse.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionResponse.RespForFirstName =
          db.GetNullableString(reader, 9);
        entities.LegalActionResponse.RespForMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.LegalActionResponse.RespForLastName =
          db.GetNullableString(reader, 11);
        entities.LegalActionResponse.Narrative =
          db.GetNullableString(reader, 12);
        entities.LegalActionResponse.Populated = true;

        return true;
      });
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
    /// <summary>
    /// A value of CutOffDate.
    /// </summary>
    [JsonPropertyName("cutOffDate")]
    public DateWorkArea CutOffDate
    {
      get => cutOffDate ??= new();
      set => cutOffDate = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private DateWorkArea cutOffDate;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActResponseGroup group.</summary>
    [Serializable]
    public class LegalActResponseGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of LegalActionResponse.
      /// </summary>
      [JsonPropertyName("legalActionResponse")]
      public LegalActionResponse LegalActionResponse
      {
        get => legalActionResponse ??= new();
        set => legalActionResponse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private Common promptType;
      private LegalActionResponse legalActionResponse;
    }

    /// <summary>
    /// Gets a value of LegalActResponse.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActResponseGroup> LegalActResponse =>
      legalActResponse ??= new(LegalActResponseGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActResponse for json serialization.
    /// </summary>
    [JsonPropertyName("legalActResponse")]
    [Computed]
    public IList<LegalActResponseGroup> LegalActResponse_Json
    {
      get => legalActResponse;
      set => LegalActResponse.Assign(value);
    }

    private Array<LegalActResponseGroup> legalActResponse;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of CutOffDate.
    /// </summary>
    [JsonPropertyName("cutOffDate")]
    public DateWorkArea CutOffDate
    {
      get => cutOffDate ??= new();
      set => cutOffDate = value;
    }

    private Common common;
    private DateWorkArea cutOffDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
    }

    private LegalAction legalAction;
    private LegalActionResponse legalActionResponse;
  }
#endregion
}

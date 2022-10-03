// Program: LE_DISC_DISPLAY_DISCOVERY, ID: 372025875, model: 746.
// Short name: SWE00762
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DISC_DISPLAY_DISCOVERY.
/// </para>
/// <para>
/// RESP: LGLENFAC
///   Display discovery information about a particular court case number and 
/// classification.
/// </para>
/// </summary>
[Serializable]
public partial class LeDiscDisplayDiscovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISC_DISPLAY_DISCOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDiscDisplayDiscovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDiscDisplayDiscovery.
  /// </summary>
  public LeDiscDisplayDiscovery(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date		Developer       Description
    // 11/04/98        R. Jean         Eliminated unnecessary reads
    // -------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.DiscoveryFound.Flag = "N";

        if (ReadDiscovery4())
        {
          export.Discovery.Assign(entities.ExistingDiscovery);
          local.DiscoveryFound.Flag = "Y";
        }

        if (AsChar(local.DiscoveryFound.Flag) == 'N')
        {
          if (Equal(import.Discovery.RequestedDate,
            local.InitialisedToZeros.RequestedDate))
          {
            ExitState = "LE0000_NO_DISC_TO_DISPLAY";
          }
          else
          {
            ExitState = "DISCOVERY_NF";
          }

          return;
        }

        break;
      case "PREV":
        local.DiscoveryFound.Flag = "N";

        if (ReadDiscovery5())
        {
          export.Discovery.Assign(entities.ExistingDiscovery);
          local.DiscoveryFound.Flag = "Y";
        }

        if (AsChar(local.DiscoveryFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_MORE_DISC_TO_DISP";

          return;
        }

        break;
      case "NEXT":
        local.DiscoveryFound.Flag = "N";

        if (ReadDiscovery2())
        {
          export.Discovery.Assign(entities.ExistingDiscovery);
          local.DiscoveryFound.Flag = "Y";
        }

        if (AsChar(local.DiscoveryFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_MORE_DISC_TO_DISP";

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (AsChar(local.DiscoveryFound.Flag) == 'Y')
    {
      if (ReadDiscovery3())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }

      if (ReadDiscovery1())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }
    }
  }

  private bool ReadDiscovery1()
  {
    entities.ExistingDiscovery.Populated = false;

    return Read("ReadDiscovery1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "requestedDt",
          export.Discovery.RequestedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.CreatedBy = db.GetString(reader, 7);
        entities.ExistingDiscovery.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 15);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingDiscovery.Populated = true;
      });
  }

  private bool ReadDiscovery2()
  {
    entities.ExistingDiscovery.Populated = false;

    return Read("ReadDiscovery2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "requestedDate1",
          import.Discovery.RequestedDate.GetValueOrDefault());
        db.SetDate(
          command, "requestedDate2",
          local.InitialisedToZeros.RequestedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.CreatedBy = db.GetString(reader, 7);
        entities.ExistingDiscovery.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 15);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingDiscovery.Populated = true;
      });
  }

  private bool ReadDiscovery3()
  {
    entities.ExistingDiscovery.Populated = false;

    return Read("ReadDiscovery3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "requestedDt",
          export.Discovery.RequestedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.CreatedBy = db.GetString(reader, 7);
        entities.ExistingDiscovery.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 15);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingDiscovery.Populated = true;
      });
  }

  private bool ReadDiscovery4()
  {
    entities.ExistingDiscovery.Populated = false;

    return Read("ReadDiscovery4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "requestedDate1",
          import.Discovery.RequestedDate.GetValueOrDefault());
        db.SetDate(
          command, "requestedDate2",
          local.InitialisedToZeros.RequestedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.CreatedBy = db.GetString(reader, 7);
        entities.ExistingDiscovery.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 15);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingDiscovery.Populated = true;
      });
  }

  private bool ReadDiscovery5()
  {
    entities.ExistingDiscovery.Populated = false;

    return Read("ReadDiscovery5",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "requestedDate1",
          import.Discovery.RequestedDate.GetValueOrDefault());
        db.SetDate(
          command, "requestedDate2",
          local.InitialisedToZeros.RequestedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.CreatedBy = db.GetString(reader, 7);
        entities.ExistingDiscovery.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 15);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingDiscovery.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    private Common userAction;
    private LegalAction legalAction;
    private Discovery discovery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    private ScrollingAttributes scrollingAttributes;
    private Discovery discovery;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DiscoveryFound.
    /// </summary>
    [JsonPropertyName("discoveryFound")]
    public Common DiscoveryFound
    {
      get => discoveryFound ??= new();
      set => discoveryFound = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public Discovery InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private Common discoveryFound;
    private Discovery initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingDiscovery.
    /// </summary>
    [JsonPropertyName("existingDiscovery")]
    public Discovery ExistingDiscovery
    {
      get => existingDiscovery ??= new();
      set => existingDiscovery = value;
    }

    private LegalAction existingLegalAction;
    private Discovery existingDiscovery;
  }
#endregion
}

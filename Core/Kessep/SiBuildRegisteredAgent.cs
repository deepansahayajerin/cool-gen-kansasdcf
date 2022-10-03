// Program: SI_BUILD_REGISTERED_AGENT, ID: 371767445, model: 746.
// Short name: SWE01108
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
/// A program: SI_BUILD_REGISTERED_AGENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiBuildRegisteredAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_REGISTERED_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildRegisteredAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildRegisteredAgent.
  /// </summary>
  public SiBuildRegisteredAgent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 10-24-95 K Evans     Initial Development
    // ---------------------------------------------
    export.Group.Index = -1;

    // *********************************************
    // The following will first determine the length
    // of the entered name field, then pad the
    // remaining spaces with percent sign(s)
    // *********************************************
    local.Blanks.Count = Length(TrimEnd(import.SearchRegisteredAgent.Name));
    local.FillerRegisteredAgent.Name = "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%";

    if (local.Blanks.Count > 0)
    {
      local.WorkName.Name =
        Substring(import.SearchRegisteredAgent.Name, 33, 1, local.Blanks.Count) +
        Substring
        (local.FillerRegisteredAgent.Name, 33, local.Blanks.Count,
        RegisteredAgent.Name_MaxLength - local.Blanks.Count);
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(import.RegisteredAgent.Name) && !
        IsEmpty(import.RegisteredAgentAddress.City))
      {
        foreach(var item in ReadRegisteredAgentRegisteredAgentAddress3())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.GdetailRegisteredAgent.Assign(
            entities.RegisteredAgent);
          export.Group.Update.GdetailRegisteredAgentAddress.Assign(
            entities.RegisteredAgentAddress);

          if (export.Group.IsFull)
          {
            return;
          }
        }

        return;
      }

      if (!IsEmpty(import.RegisteredAgent.Name) && IsEmpty
        (import.RegisteredAgentAddress.City))
      {
        foreach(var item in ReadRegisteredAgentRegisteredAgentAddress1())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.GdetailRegisteredAgent.Assign(
            entities.RegisteredAgent);
          export.Group.Update.GdetailRegisteredAgentAddress.Assign(
            entities.RegisteredAgentAddress);

          if (export.Group.IsFull)
          {
            return;
          }
        }
      }
    }
    else if (!IsEmpty(import.SearchRegisteredAgentAddress.City))
    {
      foreach(var item in ReadRegisteredAgentRegisteredAgentAddress2())
      {
        if (Lt(entities.RegisteredAgent.Name, import.RegisteredAgent.Name))
        {
          continue;
        }

        if (Equal(entities.RegisteredAgent.Name, import.RegisteredAgent.Name))
        {
          if (!Lt(import.RegisteredAgent.Identifier,
            entities.RegisteredAgent.Identifier))
          {
            continue;
          }
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GdetailRegisteredAgent.Assign(
          entities.RegisteredAgent);
        export.Group.Update.GdetailRegisteredAgentAddress.Assign(
          entities.RegisteredAgentAddress);

        if (export.Group.IsFull)
        {
          return;
        }
      }
    }
    else
    {
      foreach(var item in ReadRegisteredAgentRegisteredAgentAddress1())
      {
        if (Lt(entities.RegisteredAgent.Name, import.RegisteredAgent.Name))
        {
          continue;
        }

        if (Equal(entities.RegisteredAgent.Name, import.RegisteredAgent.Name))
        {
          if (Lt(entities.RegisteredAgentAddress.City,
            import.RegisteredAgentAddress.City))
          {
            continue;
          }

          if (Equal(entities.RegisteredAgentAddress.City,
            import.RegisteredAgentAddress.City))
          {
            if (!Lt(import.RegisteredAgent.Identifier,
              entities.RegisteredAgent.Identifier))
            {
              continue;
            }
          }
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GdetailRegisteredAgent.Assign(
          entities.RegisteredAgent);
        export.Group.Update.GdetailRegisteredAgentAddress.Assign(
          entities.RegisteredAgentAddress);

        if (export.Group.IsFull)
        {
          return;
        }
      }
    }
  }

  private IEnumerable<bool> ReadRegisteredAgentRegisteredAgentAddress1()
  {
    entities.RegisteredAgentAddress.Populated = false;
    entities.RegisteredAgent.Populated = false;

    return ReadEach("ReadRegisteredAgentRegisteredAgentAddress1",
      (db, command) =>
      {
        db.SetNullableString(command, "name", local.WorkName.Name ?? "");
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 0);
        entities.RegisteredAgent.PhoneNumber = db.GetNullableInt32(reader, 1);
        entities.RegisteredAgent.AreaCode = db.GetInt32(reader, 2);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 3);
        entities.RegisteredAgentAddress.Identifier = db.GetString(reader, 4);
        entities.RegisteredAgentAddress.Street1 =
          db.GetNullableString(reader, 5);
        entities.RegisteredAgentAddress.Street2 =
          db.GetNullableString(reader, 6);
        entities.RegisteredAgentAddress.City = db.GetNullableString(reader, 7);
        entities.RegisteredAgentAddress.State = db.GetNullableString(reader, 8);
        entities.RegisteredAgentAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.RegisteredAgentAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.RegisteredAgentAddress.Populated = true;
        entities.RegisteredAgent.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadRegisteredAgentRegisteredAgentAddress2()
  {
    entities.RegisteredAgentAddress.Populated = false;
    entities.RegisteredAgent.Populated = false;

    return ReadEach("ReadRegisteredAgentRegisteredAgentAddress2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "city", import.SearchRegisteredAgentAddress.City ?? "");
        db.SetNullableString(command, "name", local.WorkName.Name ?? "");
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 0);
        entities.RegisteredAgent.PhoneNumber = db.GetNullableInt32(reader, 1);
        entities.RegisteredAgent.AreaCode = db.GetInt32(reader, 2);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 3);
        entities.RegisteredAgentAddress.Identifier = db.GetString(reader, 4);
        entities.RegisteredAgentAddress.Street1 =
          db.GetNullableString(reader, 5);
        entities.RegisteredAgentAddress.Street2 =
          db.GetNullableString(reader, 6);
        entities.RegisteredAgentAddress.City = db.GetNullableString(reader, 7);
        entities.RegisteredAgentAddress.State = db.GetNullableString(reader, 8);
        entities.RegisteredAgentAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.RegisteredAgentAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.RegisteredAgentAddress.Populated = true;
        entities.RegisteredAgent.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadRegisteredAgentRegisteredAgentAddress3()
  {
    entities.RegisteredAgentAddress.Populated = false;
    entities.RegisteredAgent.Populated = false;

    return ReadEach("ReadRegisteredAgentRegisteredAgentAddress3",
      (db, command) =>
      {
        db.SetNullableString(command, "name", local.WorkName.Name ?? "");
        db.SetNullableString(
          command, "city", import.RegisteredAgentAddress.City ?? "");
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 0);
        entities.RegisteredAgent.PhoneNumber = db.GetNullableInt32(reader, 1);
        entities.RegisteredAgent.AreaCode = db.GetInt32(reader, 2);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 3);
        entities.RegisteredAgentAddress.Identifier = db.GetString(reader, 4);
        entities.RegisteredAgentAddress.Street1 =
          db.GetNullableString(reader, 5);
        entities.RegisteredAgentAddress.Street2 =
          db.GetNullableString(reader, 6);
        entities.RegisteredAgentAddress.City = db.GetNullableString(reader, 7);
        entities.RegisteredAgentAddress.State = db.GetNullableString(reader, 8);
        entities.RegisteredAgentAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.RegisteredAgentAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.RegisteredAgentAddress.Populated = true;
        entities.RegisteredAgent.Populated = true;

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
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgent.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgent")]
    public RegisteredAgent SearchRegisteredAgent
    {
      get => searchRegisteredAgent ??= new();
      set => searchRegisteredAgent = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgentAddress")]
    public RegisteredAgentAddress SearchRegisteredAgentAddress
    {
      get => searchRegisteredAgentAddress ??= new();
      set => searchRegisteredAgentAddress = value;
    }

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private RegisteredAgent registeredAgent;
    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent searchRegisteredAgent;
    private RegisteredAgentAddress searchRegisteredAgentAddress;
    private Common common;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgent.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgent")]
      public RegisteredAgent GdetailRegisteredAgent
      {
        get => gdetailRegisteredAgent ??= new();
        set => gdetailRegisteredAgent = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgentAddress.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgentAddress")]
      public RegisteredAgentAddress GdetailRegisteredAgentAddress
      {
        get => gdetailRegisteredAgentAddress ??= new();
        set => gdetailRegisteredAgentAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gdetailCommon;
      private RegisteredAgent gdetailRegisteredAgent;
      private RegisteredAgentAddress gdetailRegisteredAgentAddress;
      private Common gdetailPrompt;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FillerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("fillerRegisteredAgent")]
    public RegisteredAgent FillerRegisteredAgent
    {
      get => fillerRegisteredAgent ??= new();
      set => fillerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of WorkName.
    /// </summary>
    [JsonPropertyName("workName")]
    public RegisteredAgent WorkName
    {
      get => workName ??= new();
      set => workName = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgent.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgent")]
    public RegisteredAgent SearchRegisteredAgent
    {
      get => searchRegisteredAgent ??= new();
      set => searchRegisteredAgent = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgentAddress")]
    public RegisteredAgentAddress SearchRegisteredAgentAddress
    {
      get => searchRegisteredAgentAddress ??= new();
      set => searchRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of Blanks.
    /// </summary>
    [JsonPropertyName("blanks")]
    public Common Blanks
    {
      get => blanks ??= new();
      set => blanks = value;
    }

    /// <summary>
    /// A value of FillerEmployer.
    /// </summary>
    [JsonPropertyName("fillerEmployer")]
    public Employer FillerEmployer
    {
      get => fillerEmployer ??= new();
      set => fillerEmployer = value;
    }

    private RegisteredAgent fillerRegisteredAgent;
    private RegisteredAgent workName;
    private RegisteredAgent searchRegisteredAgent;
    private RegisteredAgentAddress searchRegisteredAgentAddress;
    private Common blanks;
    private Employer fillerEmployer;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent registeredAgent;
  }
#endregion
}

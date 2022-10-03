// Program: SP_DELETE_CSE_ORG_RELATION, ID: 371780060, model: 746.
// Short name: SWE01326
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DELETE_CSE_ORG_RELATION.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// We need to delete this action block.
/// </para>
/// </summary>
[Serializable]
public partial class SpDeleteCseOrgRelation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_CSE_ORG_RELATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteCseOrgRelation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteCseOrgRelation.
  /// </summary>
  public SpDeleteCseOrgRelation(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCseOrganization2())
    {
      if (ReadCseOrganization1())
      {
        if (ReadCseOrganizationRelationship())
        {
          export.CseOrganizationRelationship.Assign(
            entities.CseOrganizationRelationship);
          DeleteCseOrganizationRelationship();
        }
        else
        {
          ExitState = "CSE_ORGANIZATION_RELATIONSHI_NF";
        }
      }
      else
      {
        ExitState = "STATE_ORGANIZATION_NF";
      }
    }
    else
    {
      ExitState = "STATE_ORGANIZATION_NF";
    }
  }

  private void DeleteCseOrganizationRelationship()
  {
    Update("DeleteCseOrganizationRelationship",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentCode",
          entities.CseOrganizationRelationship.CogParentCode);
        db.SetString(
          command, "cogParentType",
          entities.CseOrganizationRelationship.CogParentType);
        db.SetString(
          command, "cogChildCode",
          entities.CseOrganizationRelationship.CogChildCode);
        db.SetString(
          command, "cogChildType",
          entities.CseOrganizationRelationship.CogChildType);
      });
  }

  private bool ReadCseOrganization1()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Child.Type1);
        db.SetString(command, "organztnId", import.Child.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadCseOrganization2()
  {
    entities.Parent.Populated = false;

    return Read("ReadCseOrganization2",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Parent.Type1);
        db.SetString(command, "organztnId", import.Parent.Code);
      },
      (db, reader) =>
      {
        entities.Parent.Code = db.GetString(reader, 0);
        entities.Parent.Type1 = db.GetString(reader, 1);
        entities.Parent.Populated = true;
      });
  }

  private bool ReadCseOrganizationRelationship()
  {
    entities.CseOrganizationRelationship.Populated = false;

    return Read("ReadCseOrganizationRelationship",
      (db, command) =>
      {
        db.SetString(command, "cogChildType", entities.Parent.Type1);
        db.SetString(command, "cogChildCode", entities.Parent.Code);
        db.SetString(command, "cogParentType", entities.CseOrganization.Type1);
        db.SetString(command, "cogParentCode", entities.CseOrganization.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganizationRelationship.CogParentCode =
          db.GetString(reader, 0);
        entities.CseOrganizationRelationship.CogParentType =
          db.GetString(reader, 1);
        entities.CseOrganizationRelationship.CogChildCode =
          db.GetString(reader, 2);
        entities.CseOrganizationRelationship.CogChildType =
          db.GetString(reader, 3);
        entities.CseOrganizationRelationship.ReasonCode =
          db.GetString(reader, 4);
        entities.CseOrganizationRelationship.CreatedBy =
          db.GetString(reader, 5);
        entities.CseOrganizationRelationship.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CseOrganizationRelationship.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CseOrganization Child
    {
      get => child ??= new();
      set => child = value;
    }

    private CseOrganization parent;
    private CseOrganization child;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    private CseOrganization parent;
    private CseOrganization cseOrganization;
    private CseOrganizationRelationship cseOrganizationRelationship;
  }
#endregion
}

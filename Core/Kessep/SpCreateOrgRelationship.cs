// Program: SP_CREATE_ORG_RELATIONSHIP, ID: 371780062, model: 746.
// Short name: SWE01316
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_ORG_RELATIONSHIP.
/// </summary>
[Serializable]
public partial class SpCreateOrgRelationship: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_ORG_RELATIONSHIP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOrgRelationship(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOrgRelationship.
  /// </summary>
  public SpCreateOrgRelationship(IContext context, Import import, Export export):
    
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
      MoveCseOrganization(entities.Parent, export.Parent);

      if (ReadCseOrganization1())
      {
        export.Child.Assign(entities.Child);

        try
        {
          CreateCseOrganizationRelationship();
          export.CseOrganizationRelationship.Assign(
            entities.CseOrganizationRelationship);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "STATE_ORGANIZATION_RELATIONS_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "STATE_ORGANIZATION_RELATIONS_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        MoveCseOrganization(import.Child, export.Child);
        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
      }
    }
    else
    {
      MoveCseOrganization(import.Parent, export.Parent);
      ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private void CreateCseOrganizationRelationship()
  {
    var cogParentCode = entities.Child.Code;
    var cogParentType = entities.Child.Type1;
    var cogChildCode = entities.Parent.Code;
    var cogChildType = entities.Parent.Type1;
    var reasonCode = import.CseOrganizationRelationship.ReasonCode;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.CseOrganizationRelationship.Populated = false;
    Update("CreateCseOrganizationRelationship",
      (db, command) =>
      {
        db.SetString(command, "cogParentCode", cogParentCode);
        db.SetString(command, "cogParentType", cogParentType);
        db.SetString(command, "cogChildCode", cogChildCode);
        db.SetString(command, "cogChildType", cogChildType);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
      });

    entities.CseOrganizationRelationship.CogParentCode = cogParentCode;
    entities.CseOrganizationRelationship.CogParentType = cogParentType;
    entities.CseOrganizationRelationship.CogChildCode = cogChildCode;
    entities.CseOrganizationRelationship.CogChildType = cogChildType;
    entities.CseOrganizationRelationship.ReasonCode = reasonCode;
    entities.CseOrganizationRelationship.CreatedBy = createdBy;
    entities.CseOrganizationRelationship.CreatedTimestamp = createdTimestamp;
    entities.CseOrganizationRelationship.Populated = true;
  }

  private bool ReadCseOrganization1()
  {
    entities.Child.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Child.Type1);
        db.SetString(command, "organztnId", import.Child.Code);
      },
      (db, reader) =>
      {
        entities.Child.Code = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.Name = db.GetString(reader, 2);
        entities.Child.Populated = true;
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
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
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

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization child;
    private CseOrganization parent;
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

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CseOrganization Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization child;
    private CseOrganization parent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CseOrganization Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization child;
    private CseOrganization parent;
  }
#endregion
}

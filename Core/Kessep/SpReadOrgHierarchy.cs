// Program: SP_READ_ORG_HIERARCHY, ID: 371780061, model: 746.
// Short name: SWE01417
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_READ_ORG_HIERARCHY.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// This action block validates the Parent CSE_Organization Code and Type 
/// received and then reads for each Child CSE_Organization associated with the
/// Parent.
/// </para>
/// </summary>
[Serializable]
public partial class SpReadOrgHierarchy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_READ_ORG_HIERARCHY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReadOrgHierarchy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReadOrgHierarchy.
  /// </summary>
  public SpReadOrgHierarchy(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // Date      Developer         Request #  	Description
    // --------  ----------------  ---------  	------------------------
    // 05/18/95  R GREY			CREATE CAB
    // 09/02/11  GVandy	   CQ29124	Modifications to support multiple types of
    // 					user defined hierarchies.  Also restructured
    // 					existing logic.
    // ***************************************************************************************
    export.Parent.Assign(import.Parent);

    if (ReadCseOrganization())
    {
      export.Parent.Assign(entities.Parent);
    }
    else
    {
      ExitState = "CSE_ORGANIZATION_NF";

      return;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCseOrganizationRelationshipCseOrganization())
    {
      export.Export1.Update.CseOrganization.Assign(entities.Child);
      export.Export1.Next();
    }
  }

  private bool ReadCseOrganization()
  {
    entities.Parent.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Parent.Type1);
        db.SetString(command, "organztnId", import.Parent.Code);
      },
      (db, reader) =>
      {
        entities.Parent.Code = db.GetString(reader, 0);
        entities.Parent.Type1 = db.GetString(reader, 1);
        entities.Parent.Name = db.GetString(reader, 2);
        entities.Parent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCseOrganizationRelationshipCseOrganization()
  {
    return ReadEach("ReadCseOrganizationRelationshipCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "reasonCode", import.CseOrganizationRelationship.ReasonCode);
          
        db.SetString(command, "cogChildType", entities.Parent.Type1);
        db.SetString(command, "cogChildCode", entities.Parent.Code);
        db.SetString(command, "organztnId", import.StartCode.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CseOrganizationRelationship.CogParentCode =
          db.GetString(reader, 0);
        entities.Child.Code = db.GetString(reader, 0);
        entities.CseOrganizationRelationship.CogParentType =
          db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.CseOrganizationRelationship.CogChildCode =
          db.GetString(reader, 2);
        entities.CseOrganizationRelationship.CogChildType =
          db.GetString(reader, 3);
        entities.CseOrganizationRelationship.ReasonCode =
          db.GetString(reader, 4);
        entities.Child.Name = db.GetString(reader, 5);
        entities.CseOrganizationRelationship.Populated = true;
        entities.Child.Populated = true;

        return true;
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
    /// A value of StartCode.
    /// </summary>
    [JsonPropertyName("startCode")]
    public CseOrganization StartCode
    {
      get => startCode ??= new();
      set => startCode = value;
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
    private CseOrganization startCode;
    private CseOrganization parent;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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
      /// A value of ChildTypePrompt.
      /// </summary>
      [JsonPropertyName("childTypePrompt")]
      public Common ChildTypePrompt
      {
        get => childTypePrompt ??= new();
        set => childTypePrompt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CseOrganization Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common sel;
      private CseOrganization cseOrganization;
      private Common childTypePrompt;
      private CseOrganization hidden;
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

    private CseOrganization parent;
    private Array<ExportGroup> export1;
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

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization parent;
    private CseOrganization child;
  }
#endregion
}

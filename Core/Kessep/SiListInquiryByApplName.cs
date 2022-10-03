// Program: SI_LIST_INQUIRY_BY_APPL_NAME, ID: 371426307, model: 746.
// Short name: SWE01184
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
/// A program: SI_LIST_INQUIRY_BY_APPL_NAME.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiListInquiryByApplName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_LIST_INQUIRY_BY_APPL_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiListInquiryByApplName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiListInquiryByApplName.
  /// </summary>
  public SiListInquiryByApplName(IContext context, Import import, Export export):
    
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
    //        M A I N T E N A N C E   L O G
    //  Date   Developer    Description
    // 7-10-95 Ken Evans    Initial Development
    // ---------------------------------------------
    // *********************************************
    // This AB will provide a list of all requests
    // for a specified applicant name.
    // *********************************************
    // *********************************************
    // Add % to end of names
    // *********************************************
    UseSiLikeNameSearch();

    if (IsEmpty(import.InformationRequest.ApplicantMiddleInitial))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadInformationRequest2())
      {
        if (!IsEmpty(entities.InformationRequest.ApplicantLastName))
        {
          export.Group.Update.GinformationRequest.Assign(
            entities.InformationRequest);
        }

        export.Group.Next();
      }
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadInformationRequest1())
      {
        if (!IsEmpty(entities.InformationRequest.ApplicantLastName))
        {
          export.Group.Update.GinformationRequest.Assign(
            entities.InformationRequest);
        }

        export.Group.Next();
      }
    }

    if (export.Group.IsEmpty)
    {
      ExitState = "NO_MATCHES_FOUND";
    }
  }

  private static void MoveInformationRequest(InformationRequest source,
    InformationRequest target)
  {
    target.ApplicantLastName = source.ApplicantLastName;
    target.ApplicantFirstName = source.ApplicantFirstName;
  }

  private void UseSiLikeNameSearch()
  {
    var useImport = new SiLikeNameSearch.Import();
    var useExport = new SiLikeNameSearch.Export();

    MoveInformationRequest(import.InformationRequest,
      useImport.InformationRequest);

    Call(SiLikeNameSearch.Execute, useImport, useExport);

    MoveInformationRequest(useExport.InformationRequest,
      local.InformationRequest);
  }

  private IEnumerable<bool> ReadInformationRequest1()
  {
    return ReadEach("ReadInformationRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "applicantLastName",
          local.InformationRequest.ApplicantLastName ?? "");
        db.SetNullableString(
          command, "applicantFirstName",
          local.InformationRequest.ApplicantFirstName ?? "");
        db.SetNullableString(
          command, "applMi",
          import.InformationRequest.ApplicantMiddleInitial ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.ApplicationSentIndicator =
          db.GetString(reader, 1);
        entities.InformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 2);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 4);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.InformationRequest.DateApplicationSent =
          db.GetNullableDate(reader, 6);
        entities.InformationRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInformationRequest2()
  {
    return ReadEach("ReadInformationRequest2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "applicantLastName",
          local.InformationRequest.ApplicantLastName ?? "");
        db.SetNullableString(
          command, "applicantFirstName",
          local.InformationRequest.ApplicantFirstName ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.ApplicationSentIndicator =
          db.GetString(reader, 1);
        entities.InformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 2);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 4);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.InformationRequest.DateApplicationSent =
          db.GetNullableDate(reader, 6);
        entities.InformationRequest.Populated = true;

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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GinformationRequest.
      /// </summary>
      [JsonPropertyName("ginformationRequest")]
      public InformationRequest GinformationRequest
      {
        get => ginformationRequest ??= new();
        set => ginformationRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common gcommon;
      private InformationRequest ginformationRequest;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Blanks.
    /// </summary>
    [JsonPropertyName("blanks")]
    public Common Blanks
    {
      get => blanks ??= new();
      set => blanks = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private Common blanks;
    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }
#endregion
}

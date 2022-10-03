// Program: OE_FCRV_BUILD_FCR_CASE_VIEW, ID: 374573869, model: 746.
// Short name: SWE00029
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCRV_BUILD_FCR_CASE_VIEW.
/// </summary>
[Serializable]
public partial class OeFcrvBuildFcrCaseView: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCRV_BUILD_FCR_CASE_VIEW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrvBuildFcrCaseView(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrvBuildFcrCaseView.
  /// </summary>
  public OeFcrvBuildFcrCaseView(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ---------------------------------------------------------------
    // 08/25/2009	M Fan		CQ7190	        Initial Dev
    // ---------------------------------------------------------------
    // ---------------------------------------------------------------
    //         ***  Read by case number entered  ***
    // ---------------------------------------------------------------
    if (!ReadFcrCaseMaster())
    {
      ExitState = "OE_0205_FCR_CASE_NOT_FOUND";

      return;
    }

    export.FcrMemberList.Index = 0;
    export.FcrMemberList.Clear();

    foreach(var item in ReadFcrCaseMembers())
    {
      export.FcrMemberList.Update.SelChar.SelectChar = "";
      export.FcrMemberList.Update.FcrMember.Assign(
        entities.ExistingFcrCaseMembers);
      export.FcrMemberList.Update.FcrCase.CaseSentDateToFcr =
        entities.ExistingFcrCaseMaster.CaseSentDateToFcr;
      export.FcrMemberList.Update.FcrCase.FcrCaseResponseDate =
        entities.ExistingFcrCaseMaster.FcrCaseResponseDate;
      export.FcrMemberList.Update.Comma.SelectChar = ",";
      export.FcrMemberList.Next();
    }

    if (export.FcrMemberList.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
    }
    else if (export.FcrMemberList.IsFull)
    {
      ExitState = "ACO_NI0000_LST_RETURNED_FULL";
    }
    else
    {
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
    }
  }

  private bool ReadFcrCaseMaster()
  {
    entities.ExistingFcrCaseMaster.Populated = false;

    return Read("ReadFcrCaseMaster",
      (db, command) =>
      {
        db.SetString(command, "caseId", import.FcrCaseId.CaseId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 1);
        entities.ExistingFcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingFcrCaseMaster.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMembers()
  {
    return ReadEach("ReadFcrCaseMembers",
      (db, command) =>
      {
        db.
          SetString(command, "fcmCaseId", entities.ExistingFcrCaseMaster.CaseId);
          
      },
      (db, reader) =>
      {
        if (export.FcrMemberList.IsFull)
        {
          return false;
        }

        entities.ExistingFcrCaseMembers.FcmCaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMembers.MemberId = db.GetString(reader, 1);
        entities.ExistingFcrCaseMembers.ActionTypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrCaseMembers.LocateRequestType =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrCaseMembers.ParticipantType =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrCaseMembers.SexCode =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrCaseMembers.DateOfBirth =
          db.GetNullableDate(reader, 6);
        entities.ExistingFcrCaseMembers.Ssn = db.GetNullableString(reader, 7);
        entities.ExistingFcrCaseMembers.FirstName =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrCaseMembers.MiddleName =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrCaseMembers.LastName =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrCaseMembers.PreviousSsn =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrCaseMembers.AdditionalSsn1 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrCaseMembers.AdditionalSsn2 =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrCaseMembers.SsnValidityCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrCaseMembers.MultipleSsn1 =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrCaseMembers.MultipleSsn2 =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrCaseMembers.MultipleSsn3 =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrCaseMembers.BatchNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrCaseMembers.AcknowledgementCode =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrCaseMembers.ErrorCode1 =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrCaseMembers.ErrorCode2 =
          db.GetNullableString(reader, 21);
        entities.ExistingFcrCaseMembers.ErrorCode3 =
          db.GetNullableString(reader, 22);
        entities.ExistingFcrCaseMembers.ErrorCode4 =
          db.GetNullableString(reader, 23);
        entities.ExistingFcrCaseMembers.ErrorCode5 =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrCaseMembers.AdditionalSsn1ValidityCode =
          db.GetNullableString(reader, 25);
        entities.ExistingFcrCaseMembers.AdditionalSsn2ValidityCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrCaseMembers.Populated = true;

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
    /// A value of FcrCaseId.
    /// </summary>
    [JsonPropertyName("fcrCaseId")]
    public FcrCaseMaster FcrCaseId
    {
      get => fcrCaseId ??= new();
      set => fcrCaseId = value;
    }

    private FcrCaseMaster fcrCaseId;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FcrMemberListGroup group.</summary>
    [Serializable]
    public class FcrMemberListGroup
    {
      /// <summary>
      /// A value of FcrCase.
      /// </summary>
      [JsonPropertyName("fcrCase")]
      public FcrCaseMaster FcrCase
      {
        get => fcrCase ??= new();
        set => fcrCase = value;
      }

      /// <summary>
      /// A value of Comma.
      /// </summary>
      [JsonPropertyName("comma")]
      public Common Comma
      {
        get => comma ??= new();
        set => comma = value;
      }

      /// <summary>
      /// A value of SelChar.
      /// </summary>
      [JsonPropertyName("selChar")]
      public Common SelChar
      {
        get => selChar ??= new();
        set => selChar = value;
      }

      /// <summary>
      /// A value of FcrMember.
      /// </summary>
      [JsonPropertyName("fcrMember")]
      public FcrCaseMembers FcrMember
      {
        get => fcrMember ??= new();
        set => fcrMember = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selChar;
      private FcrCaseMembers fcrMember;
    }

    /// <summary>
    /// Gets a value of FcrMemberList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrMemberListGroup> FcrMemberList => fcrMemberList ??= new(
      FcrMemberListGroup.Capacity);

    /// <summary>
    /// Gets a value of FcrMemberList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrMemberList")]
    [Computed]
    public IList<FcrMemberListGroup> FcrMemberList_Json
    {
      get => fcrMemberList;
      set => FcrMemberList.Assign(value);
    }

    private Array<FcrMemberListGroup> fcrMemberList;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrCaseMembers.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMembers")]
    public FcrCaseMembers ExistingFcrCaseMembers
    {
      get => existingFcrCaseMembers ??= new();
      set => existingFcrCaseMembers = value;
    }

    /// <summary>
    /// A value of ExistingFcrCaseMaster.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMaster")]
    public FcrCaseMaster ExistingFcrCaseMaster
    {
      get => existingFcrCaseMaster ??= new();
      set => existingFcrCaseMaster = value;
    }

    private FcrCaseMembers existingFcrCaseMembers;
    private FcrCaseMaster existingFcrCaseMaster;
  }
#endregion
}

// Program: TEST_INFRASTRUCTURE_RECORD, ID: 372451807, model: 746.
// Short name: SWERPTNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: TEST_INFRASTRUCTURE_RECORD.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class TestInfrastructureRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the TEST_INFRASTRUCTURE_RECORD program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new TestInfrastructureRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of TestInfrastructureRecord.
  /// </summary>
  public TestInfrastructureRecord(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // PR's 133601, 133602, & 133853  provide for the installation of the 
    // security logic. 12-22-2001. L. Bachura
    // PR138315. Remove PF3 key from screen and the code. LBachura 2-6-02
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------
    // Security Logic
    // ------------------------------------
    // Installed security cab 12-22-2001 per PR's listed above. L. Bachura 12-22
    // -2001
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "REFRESH":
        return;
      case "EXIT":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      default:
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Details.Assign(import.Group.Item.Details);
          export.Group.Update.Sel.SelectChar = import.Group.Item.Sel.SelectChar;
          export.Group.Next();
        }

        export.SearchCase.Number = import.SearchCase.Number;
        export.SearchCsePerson.Number = import.SearchCsePerson.Number;
        export.SearchInfrastructure.UserId = import.SearchInfrastructure.UserId;

        if (!IsEmpty(import.SearchCase.Number))
        {
          local.ZeroFill.Text10 = import.SearchCase.Number;
          UseEabPadLeftWithZeros();
          export.SearchCase.Number = local.ZeroFill.Text10;
        }

        if (!IsEmpty(import.SearchCsePerson.Number))
        {
          local.ZeroFill.Text10 = import.SearchCsePerson.Number;
          UseEabPadLeftWithZeros();
          export.SearchCsePerson.Number = local.ZeroFill.Text10;
        }

        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(import.SearchCsePerson.Number) && IsEmpty
          (import.SearchCase.Number))
        {
          var field1 = GetField(export.SearchCase, "number");

          field1.Error = true;

          var field2 = GetField(export.SearchCsePerson, "number");

          field2.Error = true;

          ExitState = "OE0000_ENTER_ATLEAST_ONE_VALUE";

          return;
        }

        if (IsEmpty(import.SearchInfrastructure.UserId))
        {
          var field = GetField(export.SearchInfrastructure, "userId");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";

          return;
        }

        if (!IsEmpty(import.SearchCase.Number))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadInfrastructure2())
          {
            export.Group.Update.Details.Assign(entities.Infrastructure);
            export.Group.Next();
          }
        }
        else if (!IsEmpty(import.SearchCsePerson.Number))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadInfrastructure3())
          {
            export.Group.Update.Details.Assign(entities.Infrastructure);
            export.Group.Next();
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            if (ReadInfrastructure1())
            {
              try
              {
                UpdateInfrastructure();
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                return;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
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
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }
          }
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            if (ReadInfrastructure1())
            {
              DeleteInfrastructure();
              ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

              return;
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }
          }
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void DeleteInfrastructure()
  {
    Update("DeleteInfrastructure#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.Details.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure2()
  {
    return ReadEach("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SearchInfrastructure.UserId);
        db.SetNullableString(command, "caseNumber", export.SearchCase.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure3()
  {
    return ReadEach("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SearchInfrastructure.UserId);
        db.SetNullableString(
          command, "csePersonNum", export.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private void UpdateInfrastructure()
  {
    var situationNumber = export.Group.Item.Details.SituationNumber;
    var processStatus = export.Group.Item.Details.ProcessStatus;
    var eventId = export.Group.Item.Details.EventId;
    var eventType = export.Group.Item.Details.EventType;
    var eventDetailName = export.Group.Item.Details.EventDetailName;
    var reasonCode = export.Group.Item.Details.ReasonCode;
    var businessObjectCd = export.Group.Item.Details.BusinessObjectCd;
    var denormNumeric12 =
      export.Group.Item.Details.DenormNumeric12.GetValueOrDefault();
    var denormText12 = export.Group.Item.Details.DenormText12 ?? "";
    var denormDate = export.Group.Item.Details.DenormDate;
    var denormTimestamp = export.Group.Item.Details.DenormTimestamp;
    var initiatingStateCode = export.Group.Item.Details.InitiatingStateCode;
    var csenetInOutCode = export.Group.Item.Details.CsenetInOutCode;
    var caseNumber = export.Group.Item.Details.CaseNumber ?? "";
    var csePersonNumber = export.Group.Item.Details.CsePersonNumber ?? "";
    var caseUnitNumber =
      export.Group.Item.Details.CaseUnitNumber.GetValueOrDefault();
    var userId = export.Group.Item.Details.UserId;
    var lastUpdatedBy = export.Group.Item.Details.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = export.Group.Item.Details.LastUpdatedTimestamp;
    var referenceDate = export.Group.Item.Details.ReferenceDate;
    var function = export.Group.Item.Details.Function ?? "";
    var detail = export.Group.Item.Details.Detail ?? "";

    entities.Infrastructure.Populated = false;
    Update("UpdateInfrastructure",
      (db, command) =>
      {
        db.SetInt32(command, "situationNumber", situationNumber);
        db.SetString(command, "processStatus", processStatus);
        db.SetInt32(command, "eventId", eventId);
        db.SetString(command, "eventType", eventType);
        db.SetString(command, "eventDetailName", eventDetailName);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "businessObjectCd", businessObjectCd);
        db.SetNullableInt64(command, "denormNumeric12", denormNumeric12);
        db.SetNullableString(command, "denormText12", denormText12);
        db.SetNullableDate(command, "denormDate", denormDate);
        db.SetNullableDateTime(command, "denormTimestamp", denormTimestamp);
        db.SetString(command, "initiatingStCd", initiatingStateCode);
        db.SetString(command, "csenetInOutCode", csenetInOutCode);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "csePersonNum", csePersonNumber);
        db.SetNullableInt32(command, "caseUnitNum", caseUnitNumber);
        db.SetString(command, "userId", userId);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDate(command, "referenceDate", referenceDate);
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "detail", detail);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.SituationNumber = situationNumber;
    entities.Infrastructure.ProcessStatus = processStatus;
    entities.Infrastructure.EventId = eventId;
    entities.Infrastructure.EventType = eventType;
    entities.Infrastructure.EventDetailName = eventDetailName;
    entities.Infrastructure.ReasonCode = reasonCode;
    entities.Infrastructure.BusinessObjectCd = businessObjectCd;
    entities.Infrastructure.DenormNumeric12 = denormNumeric12;
    entities.Infrastructure.DenormText12 = denormText12;
    entities.Infrastructure.DenormDate = denormDate;
    entities.Infrastructure.DenormTimestamp = denormTimestamp;
    entities.Infrastructure.InitiatingStateCode = initiatingStateCode;
    entities.Infrastructure.CsenetInOutCode = csenetInOutCode;
    entities.Infrastructure.CaseNumber = caseNumber;
    entities.Infrastructure.CsePersonNumber = csePersonNumber;
    entities.Infrastructure.CaseUnitNumber = caseUnitNumber;
    entities.Infrastructure.UserId = userId;
    entities.Infrastructure.LastUpdatedBy = lastUpdatedBy;
    entities.Infrastructure.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Infrastructure.ReferenceDate = referenceDate;
    entities.Infrastructure.Function = function;
    entities.Infrastructure.Detail = detail;
    entities.Infrastructure.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public Infrastructure Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common sel;
      private Infrastructure details;
    }

    /// <summary>
    /// A value of SearchInfrastructure.
    /// </summary>
    [JsonPropertyName("searchInfrastructure")]
    public Infrastructure SearchInfrastructure
    {
      get => searchInfrastructure ??= new();
      set => searchInfrastructure = value;
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

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    private Infrastructure searchInfrastructure;
    private Array<GroupGroup> group;
    private CsePerson searchCsePerson;
    private Case1 searchCase;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public Infrastructure Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common sel;
      private Infrastructure details;
    }

    /// <summary>
    /// A value of SearchInfrastructure.
    /// </summary>
    [JsonPropertyName("searchInfrastructure")]
    public Infrastructure SearchInfrastructure
    {
      get => searchInfrastructure ??= new();
      set => searchInfrastructure = value;
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

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    private Infrastructure searchInfrastructure;
    private Array<GroupGroup> group;
    private CsePerson searchCsePerson;
    private Case1 searchCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private TextWorkArea zeroFill;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }
#endregion
}

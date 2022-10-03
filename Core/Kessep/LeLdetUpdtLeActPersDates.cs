// Program: LE_LDET_UPDT_LE_ACT_PERS_DATES, ID: 371994261, model: 746.
// Short name: SWE02120
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
/// A program: LE_LDET_UPDT_LE_ACT_PERS_DATES.
/// </para>
/// <para>
/// RESP: LEGAL
/// When the effective date for a legal detail is modified, this action block is
/// called to update the associated legal action person effective date (dates
/// displayed from LDET and LOPS will be in sync).
/// </para>
/// </summary>
[Serializable]
public partial class LeLdetUpdtLeActPersDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LDET_UPDT_LE_ACT_PERS_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLdetUpdtLeActPersDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLdetUpdtLeActPersDates.
  /// </summary>
  public LeLdetUpdtLeActPersDates(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // 		Maintenance Log
    // ---------------------------------------------
    //   Date		Developer	Description
    //  10/08/97	R Grey		Initial code
    // ---------------------------------------------
    if (ReadLegalActionDetail())
    {
      if (!Lt(local.InitialisedToZeros.EndDate, import.LegalActionDetail.EndDate))
        
      {
        local.LegalActionPerson.EndDate = UseCabSetMaximumDiscontinueDate();
      }
      else
      {
        local.LegalActionPerson.EndDate = entities.LegalActionDetail.EndDate;
      }

      local.LegalActionPerson.EffectiveDate =
        entities.LegalActionDetail.EffectiveDate;
    }
    else if (ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadLegalActionPerson())
    {
      local.Group.Update.LegalActionPerson.EndDate =
        entities.LegalActionPerson.EndDate;

      try
      {
        UpdateLegalActionPerson();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LE0000_LE_ACT_PERSON_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LE0000_LE_ACT_PERSON_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.Group.Next();
    }

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      local.Test.EndDate = local.Group.Item.LegalActionPerson.EndDate;

      break;
    }

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!Equal(local.Group.Item.LegalActionPerson.EndDate, local.Test.EndDate))
        
      {
        return;
      }
    }

    if (!Equal(import.LegalActionDetail.EndDate, local.Test.EndDate) && !
      Equal(local.Test.EndDate, new DateTime(2099, 12, 31)))
    {
      ExitState = "LE0000_LDET_ENDT_NE_LOPS_ENDT";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    return ReadEach("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionPerson.Role = db.GetString(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 6);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private void UpdateLegalActionPerson()
  {
    var effectiveDate = local.LegalActionPerson.EffectiveDate;

    entities.LegalActionPerson.Populated = false;
    Update("UpdateLegalActionPerson",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.
          SetInt32(command, "laPersonId", entities.LegalActionPerson.Identifier);
          
      });

    entities.LegalActionPerson.EffectiveDate = effectiveDate;
    entities.LegalActionPerson.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private LegalActionPerson legalActionPerson;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public LegalActionPerson Test
    {
      get => test ??= new();
      set => test = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public LegalActionDetail InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private LegalActionPerson test;
    private Array<GroupGroup> group;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}

// Program: OE_HICV_UPDATE_HINS_COV_VIABLTY, ID: 371850661, model: 746.
// Short name: SWE00931
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
/// A program: OE_HICV_UPDATE_HINS_COV_VIABLTY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block updates HEALTH INSURANCE VIABILITY record.
/// </para>
/// </summary>
[Serializable]
public partial class OeHicvUpdateHinsCovViablty: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICV_UPDATE_HINS_COV_VIABLTY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicvUpdateHinsCovViablty(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicvUpdateHinsCovViablty.
  /// </summary>
  public OeHicvUpdateHinsCovViablty(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block UPDATEs HEALTH INSURANCE VIABILITY record
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  CASE_ROLE CHILD                - R - -
    //  HEALTH_INSURANCE_VIABILITY     - R U -
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	05/16/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	05/16/95			Initial coding
    // G P Kim	04/30/97			Change Current Date
    // ********************************************
    // 	
    local.Current.Date = Now().Date;

    if (!ReadCsePerson2())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCsePerson1())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (!ReadChild())
    {
      ExitState = "OE0065_NF_CASE_ROLE_CHILD";

      return;
    }

    if (ReadHealthInsuranceViability())
    {
      export.HealthInsuranceViability.Assign(
        entities.ExistingHealthInsuranceViability);

      if (entities.ExistingHealthInsuranceViability.Identifier != import
        .HealthInsuranceViability.Identifier)
      {
        ExitState = "OE0116_NOT_THE_LATEST_HINSVIAB";

        return;
      }

      if (!Equal(entities.ExistingHealthInsuranceViability.
        HinsViableIndWorkerId, global.UserId))
      {
        ExitState = "OE0117_HICV_NOT_CREATED_BY_USER";

        return;
      }

      if (AsChar(import.HealthInsuranceViability.HinsViableInd) != 'Y' && AsChar
        (import.HealthInsuranceViability.HinsViableInd) != 'N' && !
        IsEmpty(import.HealthInsuranceViability.HinsViableInd))
      {
        ExitState = "OE0121_INVALID_HINS_VIABILITY";

        return;
      }

      try
      {
        UpdateHealthInsuranceViability();
        export.HealthInsuranceViability.Assign(
          entities.ExistingHealthInsuranceViability);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HINS_VIABILITY_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      foreach(var item in ReadHinsViabNote())
      {
        DeleteHinsViabNote();
      }

      local.HinsViabNote.Identifier = 0;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        ++local.HinsViabNote.Identifier;

        try
        {
          CreateHinsViabNote();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "HINS_VIAB_NOTE_AE";

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
    }

    if (export.HealthInsuranceViability.Identifier == 0)
    {
      ExitState = "HINS_VIABILITY_NF";
    }
  }

  private void CreateHinsViabNote()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingHealthInsuranceViability.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var identifier = local.HinsViabNote.Identifier;
    var note = import.Import1.Item.HinsViabNote.Note ?? "";
    var croId = entities.ExistingHealthInsuranceViability.CroIdentifier;
    var croType = entities.ExistingHealthInsuranceViability.CroType;
    var cspNumber = entities.ExistingHealthInsuranceViability.CspNumber;
    var casNumber = entities.ExistingHealthInsuranceViability.CasNumber;
    var hivId = entities.ExistingHealthInsuranceViability.Identifier;

    CheckValid<HinsViabNote>("CroType", croType);
    entities.HinsViabNote.Populated = false;
    Update("CreateHinsViabNote",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "note", note);
        db.SetInt32(command, "croId", croId);
        db.SetString(command, "croType", croType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "casNumber", casNumber);
        db.SetInt32(command, "hivId", hivId);
      });

    entities.HinsViabNote.CreatedBy = createdBy;
    entities.HinsViabNote.CreatedTimestamp = createdTimestamp;
    entities.HinsViabNote.LastUpdatedBy = "";
    entities.HinsViabNote.LastUpdatedTimestamp = createdTimestamp;
    entities.HinsViabNote.Identifier = identifier;
    entities.HinsViabNote.Note = note;
    entities.HinsViabNote.CroId = croId;
    entities.HinsViabNote.CroType = croType;
    entities.HinsViabNote.CspNumber = cspNumber;
    entities.HinsViabNote.CasNumber = casNumber;
    entities.HinsViabNote.HivId = hivId;
    entities.HinsViabNote.Populated = true;
  }

  private void DeleteHinsViabNote()
  {
    Update("DeleteHinsViabNote",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.HinsViabNote.Identifier);
        db.SetInt32(command, "croId", entities.HinsViabNote.CroId);
        db.SetString(command, "croType", entities.HinsViabNote.CroType);
        db.SetString(command, "cspNumber", entities.HinsViabNote.CspNumber);
        db.SetString(command, "casNumber", entities.HinsViabNote.CasNumber);
        db.SetInt32(command, "hivId", entities.HinsViabNote.HivId);
      });
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingChild1.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingChild2.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChild1.CasNumber = db.GetString(reader, 0);
        entities.ExistingChild1.CspNumber = db.GetString(reader, 1);
        entities.ExistingChild1.Type1 = db.GetString(reader, 2);
        entities.ExistingChild1.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild1.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild1.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild1.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingChild2.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChild2.Number = db.GetString(reader, 0);
        entities.ExistingChild2.Populated = true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChild1.Populated);
    entities.ExistingHealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.
          SetInt32(command, "croIdentifier", entities.ExistingChild1.Identifier);
          
        db.SetString(command, "croType", entities.ExistingChild1.Type1);
        db.SetString(command, "casNumber", entities.ExistingChild1.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingChild1.CspNumber);
        db.SetNullableString(command, "cspNum", entities.ExistingAp.Number);
      },
      (db, reader) =>
      {
        entities.ExistingHealthInsuranceViability.CroType =
          db.GetString(reader, 0);
        entities.ExistingHealthInsuranceViability.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingHealthInsuranceViability.CasNumber =
          db.GetString(reader, 2);
        entities.ExistingHealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingHealthInsuranceViability.Identifier =
          db.GetInt32(reader, 4);
        entities.ExistingHealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.ExistingHealthInsuranceViability.HinsViableIndWorkerId =
          db.GetNullableString(reader, 6);
        entities.ExistingHealthInsuranceViability.HinsViableIndUpdatedDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingHealthInsuranceViability.CreatedBy =
          db.GetString(reader, 8);
        entities.ExistingHealthInsuranceViability.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingHealthInsuranceViability.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.ExistingHealthInsuranceViability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ExistingHealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 12);
        entities.ExistingHealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.ExistingHealthInsuranceViability.CroType);
      });
  }

  private IEnumerable<bool> ReadHinsViabNote()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingHealthInsuranceViability.Populated);
    entities.HinsViabNote.Populated = false;

    return ReadEach("ReadHinsViabNote",
      (db, command) =>
      {
        db.SetInt32(
          command, "hivId",
          entities.ExistingHealthInsuranceViability.Identifier);
        db.SetString(
          command, "casNumber",
          entities.ExistingHealthInsuranceViability.CasNumber);
        db.SetInt32(
          command, "croId",
          entities.ExistingHealthInsuranceViability.CroIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingHealthInsuranceViability.CspNumber);
        db.SetString(
          command, "croType",
          entities.ExistingHealthInsuranceViability.CroType);
      },
      (db, reader) =>
      {
        entities.HinsViabNote.CreatedBy = db.GetString(reader, 0);
        entities.HinsViabNote.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.HinsViabNote.LastUpdatedBy = db.GetString(reader, 2);
        entities.HinsViabNote.LastUpdatedTimestamp = db.GetDateTime(reader, 3);
        entities.HinsViabNote.Identifier = db.GetInt32(reader, 4);
        entities.HinsViabNote.Note = db.GetNullableString(reader, 5);
        entities.HinsViabNote.CroId = db.GetInt32(reader, 6);
        entities.HinsViabNote.CroType = db.GetString(reader, 7);
        entities.HinsViabNote.CspNumber = db.GetString(reader, 8);
        entities.HinsViabNote.CasNumber = db.GetString(reader, 9);
        entities.HinsViabNote.HivId = db.GetInt32(reader, 10);
        entities.HinsViabNote.Populated = true;
        CheckValid<HinsViabNote>("CroType", entities.HinsViabNote.CroType);

        return true;
      });
  }

  private void UpdateHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingHealthInsuranceViability.Populated);

    var hinsViableInd = import.HealthInsuranceViability.HinsViableInd ?? "";
    var hinsViableIndWorkerId = global.UserId;
    var hinsViableIndUpdatedDate = local.Current.Date;
    var lastUpdatedTimestamp = Now();

    entities.ExistingHealthInsuranceViability.Populated = false;
    Update("UpdateHealthInsuranceViability",
      (db, command) =>
      {
        db.SetNullableString(command, "hinsViableInd", hinsViableInd);
        db.SetNullableString(command, "hinsVindWorker", hinsViableIndWorkerId);
        db.SetNullableDate(command, "hinsVindUpdDt", hinsViableIndUpdatedDate);
        db.SetString(command, "lastUpdatedBy", hinsViableIndWorkerId);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(
          command, "croType",
          entities.ExistingHealthInsuranceViability.CroType);
        db.SetString(
          command, "cspNumber",
          entities.ExistingHealthInsuranceViability.CspNumber);
        db.SetString(
          command, "casNumber",
          entities.ExistingHealthInsuranceViability.CasNumber);
        db.SetInt32(
          command, "croIdentifier",
          entities.ExistingHealthInsuranceViability.CroIdentifier);
        db.SetInt32(
          command, "hinsvId",
          entities.ExistingHealthInsuranceViability.Identifier);
      });

    entities.ExistingHealthInsuranceViability.HinsViableInd = hinsViableInd;
    entities.ExistingHealthInsuranceViability.HinsViableIndWorkerId =
      hinsViableIndWorkerId;
    entities.ExistingHealthInsuranceViability.HinsViableIndUpdatedDate =
      hinsViableIndUpdatedDate;
    entities.ExistingHealthInsuranceViability.LastUpdatedBy =
      hinsViableIndWorkerId;
    entities.ExistingHealthInsuranceViability.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingHealthInsuranceViability.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of HinsViabNote.
      /// </summary>
      [JsonPropertyName("hinsViabNote")]
      public HinsViabNote HinsViabNote
      {
        get => hinsViabNote ??= new();
        set => hinsViabNote = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote hinsViabNote;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePerson Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private CsePerson ap;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson child1;
    private Case1 case1;
    private CaseRole child2;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    private HealthInsuranceViability healthInsuranceViability;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public HealthInsuranceViability Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of HinsViabNote.
    /// </summary>
    [JsonPropertyName("hinsViabNote")]
    public HinsViabNote HinsViabNote
    {
      get => hinsViabNote ??= new();
      set => hinsViabNote = value;
    }

    private DateWorkArea current;
    private HealthInsuranceViability last;
    private HinsViabNote hinsViabNote;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of HinsViabNote.
    /// </summary>
    [JsonPropertyName("hinsViabNote")]
    public HinsViabNote HinsViabNote
    {
      get => hinsViabNote ??= new();
      set => hinsViabNote = value;
    }

    /// <summary>
    /// A value of ExistingHealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("existingHealthInsuranceViability")]
    public HealthInsuranceViability ExistingHealthInsuranceViability
    {
      get => existingHealthInsuranceViability ??= new();
      set => existingHealthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ExistingChild1.
    /// </summary>
    [JsonPropertyName("existingChild1")]
    public CaseRole ExistingChild1
    {
      get => existingChild1 ??= new();
      set => existingChild1 = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingChild2.
    /// </summary>
    [JsonPropertyName("existingChild2")]
    public CsePerson ExistingChild2
    {
      get => existingChild2 ??= new();
      set => existingChild2 = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    private CsePerson existingAp;
    private HinsViabNote hinsViabNote;
    private HealthInsuranceViability existingHealthInsuranceViability;
    private CaseRole existingChild1;
    private Case1 existingCase;
    private CsePerson existingChild2;
    private CaseRole existingAbsentParent;
  }
#endregion
}

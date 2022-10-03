// Program: ESTAB_LEGAL_ACTION_PERS_RESRC, ID: 372029006, model: 746.
// Short name: SWE00234
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
/// A program: ESTAB_LEGAL_ACTION_PERS_RESRC.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates LEGAL ACTION PERSON RESOURCE and associates it to LEGAL
/// ACTION and CSE PERSON RESOURCE.
/// </para>
/// </summary>
[Serializable]
public partial class EstabLegalActionPersResrc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ESTAB_LEGAL_ACTION_PERS_RESRC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EstabLegalActionPersResrc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EstabLegalActionPersResrc.
  /// </summary>
  public EstabLegalActionPersResrc(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/29/95	Dave Allen			Initial Code
    // ------------------------------------------------------------
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (!Lt(local.Zero.Date, import.LienLegalActionPersonResource.EndDate))
    {
      local.LegalActionPersonResource.EndDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.LegalActionPersonResource.EndDate =
        import.LienLegalActionPersonResource.EndDate;
    }

    if (ReadCsePersonResource())
    {
      // ------------------------------------------------------------
      // Don't allow a duplicate to be added, even though the data
      // model allows it. (Business Rule)
      // ------------------------------------------------------------
      // ---------------------------------------------
      // The following condition ensures that there is no overlapping liens.
      // possible existing lien(s)
      //     ¦---------¦             ¦--------¦
      //    ¦
      // ------------------------------------
      // ¦
      // new lien:
      //           ¦=====================¦
      // ---------------------------------------------
      foreach(var item in ReadLegalActionPersonResource2())
      {
        // ****************************************************************
        // * If there exists an IWGL record (same source, same legal action
        // * don't allow a new IWGL record to be added if the existing
        // * IWGL record's end date is zero (still TBD) or equal to or greater 
        // than
        // * the current date (still active).  In addition, if an existing IWGL
        // * record's end date is less than current date (expired) don't
        // * allow an effective date to be less or equal to it; unless the 
        // effective
        // * date is zero (which case the eff/end dates are TBD).
        // ****************************************************************
        if (Equal(entities.LegalActionPersonResource.EndDate, local.Zero.Date) ||
          Equal
          (entities.LegalActionPersonResource.EndDate,
          new DateTime(2099, 12, 31)))
        {
          // ** Open-ended IWGL exists
          ExitState = "LE0000_LIEN_ON_RESO_AE";

          return;
        }
        else if (Lt(entities.LegalActionPersonResource.EndDate, Now().Date))
        {
          // ** Defined ended and expired IWGL exists
          if (Lt(local.Zero.Date,
            import.LienLegalActionPersonResource.EffectiveDate) && !
            Lt(entities.LegalActionPersonResource.EndDate,
            import.LienLegalActionPersonResource.EffectiveDate))
          {
            ExitState = "LE0000_LIEN_ON_RESO_AE";

            return;
          }
        }
        else
        {
          // ** Defined ended (in the future) IWGL exists
          ExitState = "LE0000_LIEN_ON_RESO_AE";

          return;
        }
      }

      if (ReadLegalActionPersonResource1())
      {
        local.LastSeqNo.Identifier =
          entities.LegalActionPersonResource.Identifier;
      }

      try
      {
        CreateLegalActionPersonResource();

        try
        {
          UpdateCsePersonResource();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_RESOURCE_NU";

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
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LE0000_LIEN_ON_RESO_AE";

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
      ExitState = "LE0000_LIEN_RESO_NOT_SELECTED";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateLegalActionPersonResource()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);

    var cspNumber = entities.CsePersonResource.CspNumber;
    var cprResourceNo = entities.CsePersonResource.ResourceNo;
    var lgaIdentifier = entities.LegalAction.Identifier;
    var effectiveDate = import.LienLegalActionPersonResource.EffectiveDate;
    var lienType = import.LienLegalActionPersonResource.LienType ?? "";
    var endDate = local.LegalActionPersonResource.EndDate;
    var createdTstamp = Now();
    var createdBy = global.UserId;
    var identifier = local.LastSeqNo.Identifier + 1;

    entities.LegalActionPersonResource.Populated = false;
    Update("CreateLegalActionPersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "cprResourceNo", cprResourceNo);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableString(command, "lienType", lienType);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetInt32(command, "identifier", identifier);
      });

    entities.LegalActionPersonResource.CspNumber = cspNumber;
    entities.LegalActionPersonResource.CprResourceNo = cprResourceNo;
    entities.LegalActionPersonResource.LgaIdentifier = lgaIdentifier;
    entities.LegalActionPersonResource.EffectiveDate = effectiveDate;
    entities.LegalActionPersonResource.LienType = lienType;
    entities.LegalActionPersonResource.EndDate = endDate;
    entities.LegalActionPersonResource.CreatedTstamp = createdTstamp;
    entities.LegalActionPersonResource.CreatedBy = createdBy;
    entities.LegalActionPersonResource.Identifier = identifier;
    entities.LegalActionPersonResource.Populated = true;
  }

  private bool ReadCsePersonResource()
  {
    entities.CsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo", import.LienCsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.CsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 2);
        entities.CsePersonResource.LienIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 4);
        entities.CsePersonResource.Populated = true;
      });
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

  private bool ReadLegalActionPersonResource1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResource1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "cprResourceNo", entities.CsePersonResource.ResourceNo);
        db.
          SetString(command, "cspNumber", entities.CsePersonResource.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 8);
        entities.LegalActionPersonResource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonResource2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);
    entities.LegalActionPersonResource.Populated = false;

    return ReadEach("ReadLegalActionPersonResource2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo", entities.CsePersonResource.ResourceNo);
        db.
          SetString(command, "cspNumber", entities.CsePersonResource.CspNumber);
          
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 8);
        entities.LegalActionPersonResource.Populated = true;

        return true;
      });
  }

  private void UpdateCsePersonResource()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);

    var lienHolderStateOfKansasInd =
      import.LienCsePersonResource.LienHolderStateOfKansasInd ?? "";
    var lienIndicator = import.LienCsePersonResource.LienIndicator ?? "";
    var cseActionTakenCode =
      import.LienCsePersonResource.CseActionTakenCode ?? "";

    entities.CsePersonResource.Populated = false;
    Update("UpdateCsePersonResource",
      (db, command) =>
      {
        db.SetNullableString(
          command, "lienHolderKsInd", lienHolderStateOfKansasInd);
        db.SetNullableString(command, "lienIndicator", lienIndicator);
        db.SetNullableString(command, "cseActionCode", cseActionTakenCode);
        db.
          SetString(command, "cspNumber", entities.CsePersonResource.CspNumber);
          
        db.
          SetInt32(command, "resourceNo", entities.CsePersonResource.ResourceNo);
          
      });

    entities.CsePersonResource.LienHolderStateOfKansasInd =
      lienHolderStateOfKansasInd;
    entities.CsePersonResource.LienIndicator = lienIndicator;
    entities.CsePersonResource.CseActionTakenCode = cseActionTakenCode;
    entities.CsePersonResource.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LienLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("lienLegalActionPersonResource")]
    public LegalActionPersonResource LienLegalActionPersonResource
    {
      get => lienLegalActionPersonResource ??= new();
      set => lienLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of LienCsePersonResource.
    /// </summary>
    [JsonPropertyName("lienCsePersonResource")]
    public CsePersonResource LienCsePersonResource
    {
      get => lienCsePersonResource ??= new();
      set => lienCsePersonResource = value;
    }

    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionPersonResource lienLegalActionPersonResource;
    private CsePersonResource lienCsePersonResource;
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
    /// <summary>
    /// A value of LastSeqNo.
    /// </summary>
    [JsonPropertyName("lastSeqNo")]
    public LegalActionPersonResource LastSeqNo
    {
      get => lastSeqNo ??= new();
      set => lastSeqNo = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    private LegalActionPersonResource lastSeqNo;
    private DateWorkArea zero;
    private LegalActionPersonResource legalActionPersonResource;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private CsePersonResource csePersonResource;
    private LegalActionPersonResource legalActionPersonResource;
  }
#endregion
}

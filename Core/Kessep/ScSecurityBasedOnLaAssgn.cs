// Program: SC_SECURITY_BASED_ON_LA_ASSGN, ID: 371456978, model: 746.
// Short name: SWE01666
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SC_SECURITY_BASED_ON_LA_ASSGN.
/// </summary>
[Serializable]
public partial class ScSecurityBasedOnLaAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_SECURITY_BASED_ON_LA_ASSGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScSecurityBasedOnLaAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScSecurityBasedOnLaAssgn.
  /// </summary>
  public ScSecurityBasedOnLaAssgn(IContext context, Import import, Export export)
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
    // *** PR#87599 - Correct authorization for contractors.
    export.Auth.Flag = "N";

    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (import.LegalAction.Identifier > 0)
    {
      if (!ReadLegalAction1())
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }

      if (ReadLegalActionAssigmentOfficeServiceProvider1())
      {
        export.Auth.Flag = "Y";
      }
      else
      {
        // *** PR#87599 - Correct authorization for contractors.
        foreach(var item in ReadLegalActionAssigmentOfficeServiceProvider2())
        {
          if (ReadOfficeServiceProvider())
          {
            export.Auth.Flag = "Y";

            return;
          }
        }
      }
    }
    else if (!IsEmpty(import.LegalAction.CourtCaseNumber) && !
      IsEmpty(import.LegalAction.Classification))
    {
      foreach(var item in ReadLegalAction3())
      {
        local.LegalAction.Assign(entities.Search);

        if (!ReadLegalAction2())
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        if (ReadLegalActionAssigmentOfficeServiceProvider1())
        {
          export.Auth.Flag = "Y";
        }
        else
        {
          // *** PR#87599 - Correct authorization for contractors.
          foreach(var item1 in ReadLegalActionAssigmentOfficeServiceProvider2())
          {
            if (ReadOfficeServiceProvider())
            {
              export.Auth.Flag = "Y";

              break;
            }
          }
        }
      }
    }
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.Search.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetString(
          command, "classification", import.LegalAction.Classification);
      },
      (db, reader) =>
      {
        entities.Search.Identifier = db.GetInt32(reader, 0);
        entities.Search.Classification = db.GetString(reader, 1);
        entities.Search.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.Search.StandardNumber = db.GetNullableString(reader, 3);
        entities.Search.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionAssigmentOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigmentOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.OfficeServiceProvider.Populated = true;
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigmentOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.LegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigmentOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.LegalActionAssigment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.Assigned.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.Assigned.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Assigned.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Assigned.RoleCode = db.GetString(reader, 2);
        entities.Assigned.EffectiveDate = db.GetDate(reader, 3);
        entities.Assigned.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    private Common auth;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    private LegalAction legalAction;
    private DateWorkArea currentDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Assigned.
    /// </summary>
    [JsonPropertyName("assigned")]
    public OfficeServiceProvider Assigned
    {
      get => assigned ??= new();
      set => assigned = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    private OfficeServiceProvider assigned;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private LegalAction legalAction;
    private LegalAction search;
    private LegalActionAssigment legalActionAssigment;
  }
#endregion
}

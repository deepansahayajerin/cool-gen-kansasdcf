// Program: FN_UPDATE_EMPLOYER_ONLY, ID: 1902467268, model: 746.
// Short name: SWE03743
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_EMPLOYER_ONLY.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateEmployerOnly: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_EMPLOYER_ONLY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateEmployerOnly(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateEmployerOnly.
  /// </summary>
  public FnUpdateEmployerOnly(IContext context, Import import, Export export):
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
    //     M A I N T E N A N C E    L O G
    //   Date     Developer   Description
    // 05-16-2015  D Dupree     Initial Development
    // ---------------------------------------------
    // Business rules for EIWO (cq 22212) state the process will only update the
    // employer fields and no employer address fiels will be updated so this
    // cab was greated to only update employer
    if (ReadEmployer2())
    {
      if (IsEmpty(import.Employer.Ein))
      {
        local.Employer.EiwoStartDate = local.DateWorkArea.Date;
        local.Employer.EiwoEndDate = local.DateWorkArea.Date;
      }
      else if (Equal(entities.Employer.Ein, import.Employer.Ein))
      {
        local.Employer.EiwoStartDate = import.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = import.Employer.EiwoEndDate;
      }
      else if (ReadEmployer1())
      {
        local.Employer.EiwoStartDate = entities.N2dRead.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.N2dRead.EiwoEndDate;
      }
      else
      {
        local.Employer.EiwoStartDate = entities.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.Employer.EiwoEndDate;
      }

      try
      {
        UpdateEmployer();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_PV";

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
      ExitState = "EMPLOYER_NF";
    }
  }

  private bool ReadEmployer1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", import.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.N2dRead.Identifier = db.GetInt32(reader, 0);
        entities.N2dRead.Ein = db.GetNullableString(reader, 1);
        entities.N2dRead.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.N2dRead.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 5);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 6);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 8);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 9);
        entities.Employer.Populated = true;
      });
  }

  private void UpdateEmployer()
  {
    var ein = import.Employer.Ein ?? "";
    var kansasId = import.Employer.KansasId ?? "";
    var name = import.Employer.Name ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var phoneNo = import.Employer.PhoneNo ?? "";
    var areaCode = import.Employer.AreaCode.GetValueOrDefault();
    var eiwoEndDate = local.Employer.EiwoEndDate;
    var eiwoStartDate = local.Employer.EiwoStartDate;

    entities.Employer.Populated = false;
    Update("UpdateEmployer",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", ein);
        db.SetNullableString(command, "kansasId", kansasId);
        db.SetNullableString(command, "name", name);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "phoneNo", phoneNo);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableDate(command, "eiwoEndDate", eiwoEndDate);
        db.SetNullableDate(command, "eiwoStartDate", eiwoStartDate);
        db.SetInt32(command, "identifier", entities.Employer.Identifier);
      });

    entities.Employer.Ein = ein;
    entities.Employer.KansasId = kansasId;
    entities.Employer.Name = name;
    entities.Employer.LastUpdatedBy = lastUpdatedBy;
    entities.Employer.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Employer.PhoneNo = phoneNo;
    entities.Employer.AreaCode = areaCode;
    entities.Employer.EiwoEndDate = eiwoEndDate;
    entities.Employer.EiwoStartDate = eiwoStartDate;
    entities.Employer.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Employer employer;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Employer employer;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public Employer N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Employer n2dRead;
    private Employer employer;
  }
#endregion
}

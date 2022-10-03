// Program: SP_UPDATE_OFFICE, ID: 371782002, model: 746.
// Short name: SWE01441
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE.
/// </summary>
[Serializable]
public partial class SpUpdateOffice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOffice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOffice.
  /// </summary>
  public SpUpdateOffice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************************************************
    // 12/02/2004     M J Quinn   WR040802    Expanded the Garden City
    // Customer Service Center pilot.                   Modules SWE02231,
    // SWEOFFCS, SWEOFFCP, SWE01441, SWE01311, SWE00091 are included in this
    // work request.
    // ******************************************************************************************************
    if (!ReadOffice1())
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    try
    {
      UpdateOffice();

      if (import.CustomerServiceCenter.SystemGeneratedId > 0)
      {
        if (ReadOffice3())
        {
          ExitState = "FN_0000_OFFICE_IS_A_CUST_SER_CNT";

          return;
        }

        if (ReadOffice2())
        {
          AssociateOffice2();
          MoveOffice(entities.CustomerServiceCenter,
            export.CustomerServiceCenter);
        }
        else
        {
          ExitState = "FN0000_CUSTOMER_SERVICE_CNTR_NF";

          return;
        }
      }
      else if (ReadOffice4())
      {
        DisassociateOffice1();
        export.CustomerServiceCenter.Name = "";
        export.CustomerServiceCenter.SystemGeneratedId = 0;
      }

      if (import.Fips.State != import.OldFips.State || import.Fips.County != import
        .OldFips.County || import.Fips.Location != import.OldFips.Location)
      {
        if (import.OldFips.County == 0 && import.OldFips.Location == 0 && import
          .OldFips.State == 0)
        {
          AssociateOffice3();
        }
        else if (ReadFips())
        {
          DisassociateOffice2();

          if (import.Fips.State != 0 && import.Fips.County != 0)
          {
            AssociateOffice3();
          }
        }
      }

      if (!Equal(import.CseOrganization.Code, import.OldCseOrganization.Code))
      {
        if (ReadCseOrganization())
        {
          AssociateOffice1();
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OFFICE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void AssociateOffice1()
  {
    var cogTypeCode = import.CseOrganization.Type1;
    var cogCode = import.CseOrganization.Code;

    entities.Office.Populated = false;
    Update("AssociateOffice1",
      (db, command) =>
      {
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetInt32(command, "officeId", entities.Office.SystemGeneratedId);
      });

    entities.Office.CogTypeCode = cogTypeCode;
    entities.Office.CogCode = cogCode;
    entities.Office.Populated = true;
  }

  private void AssociateOffice2()
  {
    var offOffice = entities.CustomerServiceCenter.SystemGeneratedId;

    entities.Office.Populated = false;
    Update("AssociateOffice2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "offOffice", offOffice);
        db.SetInt32(command, "officeId", entities.Office.SystemGeneratedId);
      });

    entities.Office.OffOffice = offOffice;
    entities.Office.Populated = true;
  }

  private void AssociateOffice3()
  {
    var offIdentifier = entities.Office.SystemGeneratedId;

    import.Fips.Populated = false;
    Update("AssociateOffice3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "offIdentifier", offIdentifier);
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      });

    import.Fips.OffIdentifier = offIdentifier;
    import.Fips.Populated = true;
  }

  private void DisassociateOffice1()
  {
    entities.Office.Populated = false;
    Update("DisassociateOffice1",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", entities.Office.SystemGeneratedId);
      });

    entities.Office.OffOffice = null;
    entities.Office.Populated = true;
  }

  private void DisassociateOffice2()
  {
    entities.Fips.Populated = false;
    Update("DisassociateOffice2",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Fips.State);
        db.SetInt32(command, "county", entities.Fips.County);
        db.SetInt32(command, "location", entities.Fips.Location);
      });

    entities.Fips.OffIdentifier = null;
    entities.Fips.Populated = true;
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", import.OldCseOrganization.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.OldFips.State);
        db.SetInt32(command, "location", import.OldFips.Location);
        db.SetInt32(command, "county", import.OldFips.County);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.OffIdentifier = db.GetNullableInt32(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadOffice1()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Office.LastUpdatdTstamp = db.GetNullableDateTime(reader, 6);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 7);
        entities.Office.CogCode = db.GetNullableString(reader, 8);
        entities.Office.EffectiveDate = db.GetDate(reader, 9);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 10);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 11);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 12);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 13);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", import.CustomerServiceCenter.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.MainPhoneNumber =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.TypeCode = db.GetString(reader, 3);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 4);
        entities.CustomerServiceCenter.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CustomerServiceCenter.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.CustomerServiceCenter.CreatedBy = db.GetString(reader, 7);
        entities.CustomerServiceCenter.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.CustomerServiceCenter.EffectiveDate = db.GetDate(reader, 9);
        entities.CustomerServiceCenter.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.CustomerServiceCenter.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.CustomerServiceCenter.MainFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 13);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offOffice", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.MainPhoneNumber =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.TypeCode = db.GetString(reader, 3);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 4);
        entities.CustomerServiceCenter.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CustomerServiceCenter.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.CustomerServiceCenter.CreatedBy = db.GetString(reader, 7);
        entities.CustomerServiceCenter.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.CustomerServiceCenter.EffectiveDate = db.GetDate(reader, 9);
        entities.CustomerServiceCenter.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.CustomerServiceCenter.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.CustomerServiceCenter.MainFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 13);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOffice4()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice4",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.Office.OffOffice.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.MainPhoneNumber =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.TypeCode = db.GetString(reader, 3);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 4);
        entities.CustomerServiceCenter.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CustomerServiceCenter.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.CustomerServiceCenter.CreatedBy = db.GetString(reader, 7);
        entities.CustomerServiceCenter.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.CustomerServiceCenter.EffectiveDate = db.GetDate(reader, 9);
        entities.CustomerServiceCenter.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.CustomerServiceCenter.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.CustomerServiceCenter.MainFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 13);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private void UpdateOffice()
  {
    var mainPhoneNumber = import.Office.MainPhoneNumber.GetValueOrDefault();
    var mainFaxPhoneNumber =
      import.Office.MainFaxPhoneNumber.GetValueOrDefault();
    var typeCode = import.Office.TypeCode;
    var name = import.Office.Name;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var effectiveDate = import.Office.EffectiveDate;
    var discontinueDate = import.Office.DiscontinueDate;
    var mainPhoneAreaCode = import.Office.MainPhoneAreaCode.GetValueOrDefault();
    var mainFaxAreaCode = import.Office.MainFaxAreaCode.GetValueOrDefault();

    entities.Office.Populated = false;
    Update("UpdateOffice",
      (db, command) =>
      {
        db.SetNullableInt32(command, "mainPhoneNumber", mainPhoneNumber);
        db.SetNullableInt32(command, "mainFaxNumber", mainFaxPhoneNumber);
        db.SetString(command, "typeCode", typeCode);
        db.SetString(command, "name", name);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableInt32(command, "mainPhoneAreaCd", mainPhoneAreaCode);
        db.SetNullableInt32(command, "faxAreaCd", mainFaxAreaCode);
        db.SetInt32(command, "officeId", entities.Office.SystemGeneratedId);
      });

    entities.Office.MainPhoneNumber = mainPhoneNumber;
    entities.Office.MainFaxPhoneNumber = mainFaxPhoneNumber;
    entities.Office.TypeCode = typeCode;
    entities.Office.Name = name;
    entities.Office.LastUpdatedBy = lastUpdatedBy;
    entities.Office.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Office.EffectiveDate = effectiveDate;
    entities.Office.DiscontinueDate = discontinueDate;
    entities.Office.MainPhoneAreaCode = mainPhoneAreaCode;
    entities.Office.MainFaxAreaCode = mainFaxAreaCode;
    entities.Office.Populated = true;
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
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of OldCseOrganization.
    /// </summary>
    [JsonPropertyName("oldCseOrganization")]
    public CseOrganization OldCseOrganization
    {
      get => oldCseOrganization ??= new();
      set => oldCseOrganization = value;
    }

    /// <summary>
    /// A value of OldFips.
    /// </summary>
    [JsonPropertyName("oldFips")]
    public Fips OldFips
    {
      get => oldFips ??= new();
      set => oldFips = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office customerServiceCenter;
    private Fips fips;
    private CseOrganization oldCseOrganization;
    private Fips oldFips;
    private CseOrganization cseOrganization;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    private Office customerServiceCenter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office customerServiceCenter;
    private Fips fips;
    private CseOrganization cseOrganization;
    private Office office;
  }
#endregion
}

// Program: SP_CREATE_OFFICE, ID: 371781650, model: 746.
// Short name: SWE01311
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE.
/// </summary>
[Serializable]
public partial class SpCreateOffice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOffice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOffice.
  /// </summary>
  public SpCreateOffice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // PR138223.  Switched the control identifier  set conditions in statements 
    // 1 thru 6 immediately below. LBachura 2-6-02
    // ******************************************************************************************************
    // 12/02/2004     M J Quinn   WR040802    Expanded the Garden City
    // Customer Service Center pilot.                   Modules SWE02231,
    // SWEOFFCS, SWEOFFCP, SWE01441, SWE01311, SWE00091 are included in this
    // work request.
    // ******************************************************************************************************
    if (AsChar(import.Office.TypeCode) == 'E')
    {
      local.ControlTable.Identifier = "CSE OFFICE ATTORNEY";
    }
    else
    {
      local.ControlTable.Identifier = "CSE OFFICE";
    }

    UseAccessControlTable();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    try
    {
      CreateOffice();
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;

      if (import.Fips.State != 0 && import.Fips.County != 0)
      {
        AssociateOffice2();
      }

      if (import.CustomerServiceCenter.SystemGeneratedId > 0)
      {
        if (ReadOffice2())
        {
          ExitState = "FN_0000_OFFICE_IS_A_CUST_SER_CNT";

          return;
        }

        if (ReadOffice1())
        {
          AssociateOffice1();
        }
        else
        {
          ExitState = "FN0000_CSC_OFFICE_NF";
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_AE";

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

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void AssociateOffice1()
  {
    var offOffice = entities.CustomerServiceCenter.SystemGeneratedId;

    entities.Office.Populated = false;
    Update("AssociateOffice1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "offOffice", offOffice);
        db.SetInt32(command, "officeId", entities.Office.SystemGeneratedId);
      });

    entities.Office.OffOffice = offOffice;
    entities.Office.Populated = true;
  }

  private void AssociateOffice2()
  {
    var offIdentifier = entities.Office.SystemGeneratedId;

    import.Fips.Populated = false;
    Update("AssociateOffice2",
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

  private void CreateOffice()
  {
    var systemGeneratedId = local.ControlTable.LastUsedNumber;
    var mainPhoneNumber = import.Office.MainPhoneNumber.GetValueOrDefault();
    var mainFaxPhoneNumber =
      import.Office.MainFaxPhoneNumber.GetValueOrDefault();
    var typeCode = import.Office.TypeCode;
    var name = import.Office.Name;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cogTypeCode = import.CseOrganization.Type1;
    var cogCode = import.CseOrganization.Code;
    var effectiveDate = import.Office.EffectiveDate;
    var discontinueDate = import.Office.DiscontinueDate;
    var mainPhoneAreaCode = import.Office.MainPhoneAreaCode.GetValueOrDefault();
    var mainFaxAreaCode = import.Office.MainFaxAreaCode.GetValueOrDefault();

    entities.Office.Populated = false;
    Update("CreateOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", systemGeneratedId);
        db.SetNullableInt32(command, "mainPhoneNumber", mainPhoneNumber);
        db.SetNullableInt32(command, "mainFaxNumber", mainFaxPhoneNumber);
        db.SetString(command, "typeCode", typeCode);
        db.SetString(command, "name", name);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableInt32(command, "mainPhoneAreaCd", mainPhoneAreaCode);
        db.SetNullableInt32(command, "faxAreaCd", mainFaxAreaCode);
      });

    entities.Office.SystemGeneratedId = systemGeneratedId;
    entities.Office.MainPhoneNumber = mainPhoneNumber;
    entities.Office.MainFaxPhoneNumber = mainFaxPhoneNumber;
    entities.Office.TypeCode = typeCode;
    entities.Office.Name = name;
    entities.Office.CreatedBy = createdBy;
    entities.Office.CreatedTimestamp = createdTimestamp;
    entities.Office.CogTypeCode = cogTypeCode;
    entities.Office.CogCode = cogCode;
    entities.Office.EffectiveDate = effectiveDate;
    entities.Office.DiscontinueDate = discontinueDate;
    entities.Office.MainPhoneAreaCode = mainPhoneAreaCode;
    entities.Office.MainFaxAreaCode = mainFaxAreaCode;
    entities.Office.OffOffice = null;
    entities.Office.Populated = true;
  }

  private bool ReadOffice1()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", import.CustomerServiceCenter.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offOffice", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 1);
        entities.CustomerServiceCenter.Populated = true;
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

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office customerServiceCenter;
    private Office office;
  }
#endregion
}

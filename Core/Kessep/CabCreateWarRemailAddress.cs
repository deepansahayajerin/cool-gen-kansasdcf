// Program: CAB_CREATE_WAR_REMAIL_ADDRESS, ID: 371866721, model: 746.
// Short name: SWE00038
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_CREATE_WAR_REMAIL_ADDRESS.
/// </para>
/// <para>
/// This screen will be used to perform a number of functions relating to 
/// warrants. It will allow the user to update the &quot;Mailed to Address&quot;
/// ( in the case of incorrectly entering a remail address). It will allow the
/// user to change the status of a warrant, enter a remail address for the
/// warrant.
/// </para>
/// </summary>
[Serializable]
public partial class CabCreateWarRemailAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CREATE_WAR_REMAIL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCreateWarRemailAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCreateWarRemailAddress.
  /// </summary>
  public CabCreateWarRemailAddress(IContext context, Import import,
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
    // =================================================
    //          Developed by : R.B.Mohapatra
    //          Date started : 10-12-1995
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(import.WarrantRemailAddress.RemailDate, null))
    {
      local.DateWorkArea.Date = Now().Date;
    }
    else
    {
      local.DateWorkArea.Date = import.WarrantRemailAddress.RemailDate;
    }

    if (Equal(import.PaymentStatus.Code, "REIS"))
    {
      try
      {
        CreateWarrantRemailAddress2();

        // *** Continue Processing
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_WARRANT_REMAIL_ADDR_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_WARRANT_REMAIL_ADDR_PV";

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
      try
      {
        CreateWarrantRemailAddress1();

        // *** Continue Processing
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_WARRANT_REMAIL_ADDR_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_WARRANT_REMAIL_ADDR_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateWarrantRemailAddress1()
  {
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var street1 = import.WarrantRemailAddress.Street1;
    var street2 = import.WarrantRemailAddress.Street2 ?? "";
    var city = import.WarrantRemailAddress.City;
    var state = import.WarrantRemailAddress.State;
    var zipCode4 = import.WarrantRemailAddress.ZipCode4 ?? "";
    var zipCode5 = import.WarrantRemailAddress.ZipCode5;
    var zipCode3 = import.WarrantRemailAddress.ZipCode3 ?? "";
    var name = import.WarrantRemailAddress.Name ?? "";
    var remailDate = local.DateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var prqId = import.PaymentRequest.SystemGeneratedIdentifier;

    entities.WarrantRemailAddress.Populated = false;
    Update("CreateWarrantRemailAddress1",
      (db, command) =>
      {
        db.SetInt32(command, "warrantRemailId", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableString(command, "name", name);
        db.SetDate(command, "remailDate", remailDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "prqId", prqId);
      });

    entities.WarrantRemailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.WarrantRemailAddress.Street1 = street1;
    entities.WarrantRemailAddress.Street2 = street2;
    entities.WarrantRemailAddress.City = city;
    entities.WarrantRemailAddress.State = state;
    entities.WarrantRemailAddress.ZipCode4 = zipCode4;
    entities.WarrantRemailAddress.ZipCode5 = zipCode5;
    entities.WarrantRemailAddress.ZipCode3 = zipCode3;
    entities.WarrantRemailAddress.Name = name;
    entities.WarrantRemailAddress.RemailDate = remailDate;
    entities.WarrantRemailAddress.CreatedBy = createdBy;
    entities.WarrantRemailAddress.CreatedTimestamp = createdTimestamp;
    entities.WarrantRemailAddress.LastUpdatedBy = "";
    entities.WarrantRemailAddress.LastUpdatedTmst = null;
    entities.WarrantRemailAddress.PrqId = prqId;
    entities.WarrantRemailAddress.Populated = true;
  }

  private void CreateWarrantRemailAddress2()
  {
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var street1 = import.WarrantRemailAddress.Street1;
    var street2 = import.WarrantRemailAddress.Street2 ?? "";
    var city = import.WarrantRemailAddress.City;
    var state = import.WarrantRemailAddress.State;
    var zipCode4 = import.WarrantRemailAddress.ZipCode4 ?? "";
    var zipCode5 = import.WarrantRemailAddress.ZipCode5;
    var zipCode3 = import.WarrantRemailAddress.ZipCode3 ?? "";
    var name = import.WarrantRemailAddress.Name ?? "";
    var remailDate = local.DateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var prqId = import.Reissued.SystemGeneratedIdentifier;

    entities.WarrantRemailAddress.Populated = false;
    Update("CreateWarrantRemailAddress2",
      (db, command) =>
      {
        db.SetInt32(command, "warrantRemailId", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableString(command, "name", name);
        db.SetDate(command, "remailDate", remailDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "prqId", prqId);
      });

    entities.WarrantRemailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.WarrantRemailAddress.Street1 = street1;
    entities.WarrantRemailAddress.Street2 = street2;
    entities.WarrantRemailAddress.City = city;
    entities.WarrantRemailAddress.State = state;
    entities.WarrantRemailAddress.ZipCode4 = zipCode4;
    entities.WarrantRemailAddress.ZipCode5 = zipCode5;
    entities.WarrantRemailAddress.ZipCode3 = zipCode3;
    entities.WarrantRemailAddress.Name = name;
    entities.WarrantRemailAddress.RemailDate = remailDate;
    entities.WarrantRemailAddress.CreatedBy = createdBy;
    entities.WarrantRemailAddress.CreatedTimestamp = createdTimestamp;
    entities.WarrantRemailAddress.LastUpdatedBy = "";
    entities.WarrantRemailAddress.LastUpdatedTmst = null;
    entities.WarrantRemailAddress.PrqId = prqId;
    entities.WarrantRemailAddress.Populated = true;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of Reissued.
    /// </summary>
    [JsonPropertyName("reissued")]
    public PaymentRequest Reissued
    {
      get => reissued ??= new();
      set => reissued = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
    }

    private PaymentStatus paymentStatus;
    private PaymentRequest reissued;
    private PaymentRequest paymentRequest;
    private WarrantRemailAddress warrantRemailAddress;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
    }

    private WarrantRemailAddress warrantRemailAddress;
  }
#endregion
}

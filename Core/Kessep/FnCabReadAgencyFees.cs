// Program: FN_CAB_READ_AGENCY_FEES, ID: 371803716, model: 746.
// Short name: SWE01628
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
/// A program: FN_CAB_READ_AGENCY_FEES.
/// </para>
/// <para>
/// Resp: FNMGMT
/// </para>
/// </summary>
[Serializable]
public partial class FnCabReadAgencyFees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_READ_AGENCY_FEES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReadAgencyFees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReadAgencyFees.
  /// </summary>
  public FnCabReadAgencyFees(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Office.Assign(import.Office);
    UseCabSetMaximumDiscontinueDate();

    if (ReadOffice())
    {
      export.Office.Assign(entities.ExistingOffice);

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadContractorFeeInformation())
      {
        if (ReadObligationType())
        {
          export.Export1.Update.DetailObligationType.Code =
            entities.ExistingObligationType.Code;
        }

        export.Export1.Update.DetailCommon.SelectChar = "";
        export.Export1.Update.DetailContractorFeeInformation.Assign(
          entities.ExistingContractorFeeInformation);

        if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
          local.MaxDate.Date))
        {
          export.Export1.Update.DetailContractorFeeInformation.DiscontinueDate =
            local.ZeroValue.DiscontinueDate;
        }

        export.Export1.Next();
      }
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";
    }
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadContractorFeeInformation()
  {
    return ReadEach("ReadContractorFeeInformation",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingContractorFeeInformation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.ExistingContractorFeeInformation.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 1);
        entities.ExistingOffice.Name = db.GetString(reader, 2);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 3);
        entities.ExistingOffice.Populated = true;
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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
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

    private Common showHistory;
    private Vendor vendor;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailContractorFeeInformation.
      /// </summary>
      [JsonPropertyName("detailContractorFeeInformation")]
      public ContractorFeeInformation DetailContractorFeeInformation
      {
        get => detailContractorFeeInformation ??= new();
        set => detailContractorFeeInformation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType detailObligationType;
      private Common detailCommon;
      private ContractorFeeInformation detailContractorFeeInformation;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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

    private Vendor vendor;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroValue.
    /// </summary>
    [JsonPropertyName("zeroValue")]
    public ContractorFeeInformation ZeroValue
    {
      get => zeroValue ??= new();
      set => zeroValue = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private ContractorFeeInformation zeroValue;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingVendor.
    /// </summary>
    [JsonPropertyName("existingVendor")]
    public Vendor ExistingVendor
    {
      get => existingVendor ??= new();
      set => existingVendor = value;
    }

    /// <summary>
    /// A value of ExistingContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("existingContractorFeeInformation")]
    public ContractorFeeInformation ExistingContractorFeeInformation
    {
      get => existingContractorFeeInformation ??= new();
      set => existingContractorFeeInformation = value;
    }

    private Office existingOffice;
    private ObligationType existingObligationType;
    private Vendor existingVendor;
    private ContractorFeeInformation existingContractorFeeInformation;
  }
#endregion
}

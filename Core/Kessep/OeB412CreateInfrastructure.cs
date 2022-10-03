// Program: OE_B412_CREATE_INFRASTRUCTURE, ID: 373344115, model: 746.
// Short name: SWE01960
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B412_CREATE_INFRASTRUCTURE.
/// </summary>
[Serializable]
public partial class OeB412CreateInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B412_CREATE_INFRASTRUCTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB412CreateInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB412CreateInfrastructure.
  /// </summary>
  public OeB412CreateInfrastructure(IContext context, Import import,
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
    export.ItemsCreated.Count = import.ItemsCreated.Count;

    if (!IsEmpty(import.Infrastructure.CsePersonNumber) && import
      .Infrastructure.CaseUnitNumber.GetValueOrDefault() == 0)
    {
      if (ReadInfrastructure())
      {
        return;
      }
    }

    MoveInfrastructure2(import.Infrastructure, local.Infrastructure);
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.UserId = "FCR";
    local.Infrastructure.EventId = 10;
    local.Infrastructure.BusinessObjectCd = "FPL";
    local.Infrastructure.ReferenceDate =
      import.ProgramProcessingInfo.ProcessDate;
    ++export.ItemsCreated.Count;
    UseSpCabCreateInfrastructure();
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetNullableString(
          command, "csePersonNum", import.Infrastructure.CsePersonNumber ?? ""
          );
        db.SetNullableString(
          command, "detail", import.Infrastructure.Detail ?? "");
        db.SetString(command, "reasonCode", import.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 1);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 4);
        entities.Infrastructure.Populated = true;
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
    /// A value of ItemsCreated.
    /// </summary>
    [JsonPropertyName("itemsCreated")]
    public Common ItemsCreated
    {
      get => itemsCreated ??= new();
      set => itemsCreated = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private Common itemsCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ItemsCreated.
    /// </summary>
    [JsonPropertyName("itemsCreated")]
    public Common ItemsCreated
    {
      get => itemsCreated ??= new();
      set => itemsCreated = value;
    }

    private Common itemsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

// Program: SP_CAB_CREATE_NARRATIVE_DETAIL, ID: 370960546, model: 746.
// Short name: SWE00360
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_NARRATIVE_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabCreateNarrativeDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_NARRATIVE_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateNarrativeDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateNarrativeDetail.
  /// </summary>
  public SpCabCreateNarrativeDetail(IContext context, Import import,
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
    try
    {
      CreateNarrativeDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_NARRATIVE_DETAIL_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SP0000_NARRATIVE_DETAIL_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateNarrativeDetail()
  {
    var infrastructureId = import.NarrativeDetail.InfrastructureId;
    var createdTimestamp = import.NarrativeDetail.CreatedTimestamp;
    var createdBy = import.NarrativeDetail.CreatedBy ?? "";
    var caseNumber = import.NarrativeDetail.CaseNumber ?? "";
    var narrativeText = import.NarrativeDetail.NarrativeText ?? "";
    var lineNumber = import.NarrativeDetail.LineNumber;

    entities.New1.Populated = false;
    Update("CreateNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(command, "infrastructureId", infrastructureId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "narrativeText", narrativeText);
        db.SetInt32(command, "lineNumber", lineNumber);
      });

    entities.New1.InfrastructureId = infrastructureId;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.NarrativeText = narrativeText;
    entities.New1.LineNumber = lineNumber;
    entities.New1.Populated = true;
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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    private NarrativeDetail narrativeDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private NarrativeDetail new1;
  }
#endregion
}

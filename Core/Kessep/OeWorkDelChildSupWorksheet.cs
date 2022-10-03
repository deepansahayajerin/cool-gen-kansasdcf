// Program: OE_WORK_DEL_CHILD_SUP_WORKSHEET, ID: 371897920, model: 746.
// Short name: SWE00977
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_WORK_DEL_CHILD_SUP_WORKSHEET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeWorkDelChildSupWorksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_DEL_CHILD_SUP_WORKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkDelChildSupWorksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkDelChildSupWorksheet.
  /// </summary>
  public OeWorkDelChildSupWorksheet(IContext context, Import import,
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
    if (ReadChildSupportWorksheet())
    {
      DeleteChildSupportWorksheet();
    }
    else
    {
      ExitState = "CHILD_SUPPORT_WORKSHEET_NF";
    }
  }

  private void DeleteChildSupportWorksheet()
  {
    Update("DeleteChildSupportWorksheet",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", entities.ChildSupportWorksheet.Identifier);
        db.SetInt32(
          command, "csGuidelineYear",
          entities.ChildSupportWorksheet.CsGuidelineYear);
      });
  }

  private bool ReadChildSupportWorksheet()
  {
    entities.ChildSupportWorksheet.Populated = false;

    return Read("ReadChildSupportWorksheet",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", import.ChildSupportWorksheet.Identifier);
      },
      (db, reader) =>
      {
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 0);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp3 =
          db.GetNullableInt32(reader, 1);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp2 =
          db.GetNullableInt32(reader, 2);
        entities.ChildSupportWorksheet.NoOfChildrenInAgeGrp1 =
          db.GetNullableInt32(reader, 3);
        entities.ChildSupportWorksheet.AdditionalNoOfChildren =
          db.GetNullableInt32(reader, 4);
        entities.ChildSupportWorksheet.Status = db.GetString(reader, 5);
        entities.ChildSupportWorksheet.CostOfLivingDiffAdjInd =
          db.GetNullableString(reader, 6);
        entities.ChildSupportWorksheet.MultipleFamilyAdjInd =
          db.GetNullableString(reader, 7);
        entities.ChildSupportWorksheet.CreatedBy = db.GetString(reader, 8);
        entities.ChildSupportWorksheet.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ChildSupportWorksheet.LastUpdatedBy = db.GetString(reader, 10);
        entities.ChildSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ChildSupportWorksheet.CsGuidelineYear =
          db.GetInt32(reader, 12);
        entities.ChildSupportWorksheet.Populated = true;
      });
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    private ChildSupportWorksheet childSupportWorksheet;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    private ChildSupportWorksheet childSupportWorksheet;
  }
#endregion
}

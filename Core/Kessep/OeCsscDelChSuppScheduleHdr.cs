// Program: OE_CSSC_DEL_CH_SUPP_SCHEDULE_HDR, ID: 371909260, model: 746.
// Short name: SWE02302
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CSSC_DEL_CH_SUPP_SCHEDULE_HDR.
/// </summary>
[Serializable]
public partial class OeCsscDelChSuppScheduleHdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_DEL_CH_SUPP_SCHEDULE_HDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscDelChSuppScheduleHdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscDelChSuppScheduleHdr.
  /// </summary>
  public OeCsscDelChSuppScheduleHdr(IContext context, Import import,
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
    if (!ReadChildSupportSchedule())
    {
      ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

      return;
    }

    if (ReadAgeGroupSupportScheduleCsGrossMonthlyIncSched())
    {
      ExitState = "OE0177_AGE_GROUP_EXIST_DEL_ERR";

      return;
    }

    foreach(var item in ReadAgeGroupSupportSchedule())
    {
      DeleteAgeGroupSupportSchedule();
    }

    DeleteChildSupportSchedule();
    ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
  }

  private void DeleteAgeGroupSupportSchedule()
  {
    Update("DeleteAgeGroupSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier",
          entities.AgeGroupSupportSchedule.CssIdentifier);
        db.SetInt32(
          command, "maxAgeInRange",
          entities.AgeGroupSupportSchedule.MaximumAgeInARange);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.AgeGroupSupportSchedule.CssGuidelineYr);
      });
  }

  private void DeleteChildSupportSchedule()
  {
    Update("DeleteChildSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "csGuidelineYear",
          entities.ChildSupportSchedule.CsGuidelineYear);
      });
  }

  private IEnumerable<bool> ReadAgeGroupSupportSchedule()
  {
    entities.AgeGroupSupportSchedule.Populated = false;

    return ReadEach("ReadAgeGroupSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.AgeGroupSupportSchedule.CssIdentifier = db.GetInt32(reader, 0);
        entities.AgeGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.AgeGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 2);
        entities.AgeGroupSupportSchedule.Populated = true;

        return true;
      });
  }

  private bool ReadAgeGroupSupportScheduleCsGrossMonthlyIncSched()
  {
    entities.CsGrossMonthlyIncSched.Populated = false;
    entities.AgeGroupSupportSchedule.Populated = false;

    return Read("ReadAgeGroupSupportScheduleCsGrossMonthlyIncSched",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.AgeGroupSupportSchedule.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.AgeGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.AgeGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.Populated = true;
        entities.AgeGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadChildSupportSchedule()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "csGuidelineYear",
          import.ChildSupportSchedule.CsGuidelineYear);
        db.SetInt32(
          command, "identifier", import.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "noOfChInFamily",
          import.ChildSupportSchedule.NumberOfChildrenInFamily);
      },
      (db, reader) =>
      {
        entities.ChildSupportSchedule.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportSchedule.ExpirationDate = db.GetDate(reader, 1);
        entities.ChildSupportSchedule.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ChildSupportSchedule.NumberOfChildrenInFamily =
          db.GetInt32(reader, 3);
        entities.ChildSupportSchedule.CsGuidelineYear = db.GetInt32(reader, 4);
        entities.ChildSupportSchedule.Populated = true;
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
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    private ChildSupportSchedule childSupportSchedule;
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
    /// A value of CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("csGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched CsGrossMonthlyIncSched
    {
      get => csGrossMonthlyIncSched ??= new();
      set => csGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of AgeGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("ageGroupSupportSchedule")]
    public AgeGroupSupportSchedule AgeGroupSupportSchedule
    {
      get => ageGroupSupportSchedule ??= new();
      set => ageGroupSupportSchedule = value;
    }

    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    private CsGrossMonthlyIncSched csGrossMonthlyIncSched;
    private AgeGroupSupportSchedule ageGroupSupportSchedule;
    private ChildSupportSchedule childSupportSchedule;
  }
#endregion
}

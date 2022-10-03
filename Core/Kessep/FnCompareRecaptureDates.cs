// Program: FN_COMPARE_RECAPTURE_DATES, ID: 372128804, model: 746.
// Short name: SWE02172
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMPARE_RECAPTURE_DATES.
/// </summary>
[Serializable]
public partial class FnCompareRecaptureDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPARE_RECAPTURE_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCompareRecaptureDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCompareRecaptureDates.
  /// </summary>
  public FnCompareRecaptureDates(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *** Exitstate may not be ALL OK coming in here.
    // ***
    local.HighDate.Date = UseCabSetMaximumDiscontinueDate();
    local.New1.Assign(import.New1);
    export.Error.Count = import.Error.Count;

    // *** Check for New discontinue date < New effective date.
    if (Lt(local.New1.DiscontinueDate, local.New1.EffectiveDate))
    {
      ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";
      export.EffectiveError.Flag = "Y";
      export.ExpireError.Flag = "Y";
      ++export.Error.Count;

      return;
    }

    // *** Check for New effective date < Current date for non-active recapture 
    // rule.
    if (Lt(local.New1.EffectiveDate, Now().Date))
    {
      if (AsChar(import.Active.Flag) != 'Y')
      {
        export.EffectiveError.Flag = "Y";
        ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
        ++export.Error.Count;

        return;
      }
    }

    // *** Check for New discontinue date < Current date
    if (Lt(local.New1.DiscontinueDate, Now().Date))
    {
      ExitState = "FN0000_DISCONTINUE_DT_LT_CURRENT";
      export.ExpireError.Flag = "Y";
      ++export.Error.Count;

      return;
    }

    // *** Get the rules with the next highest and the next lowest start dates, 
    // and perform overlap edits.
    foreach(var item in ReadObligorRule())
    {
      // *** Make sure the recapture rule is not the one being updated
      if (entities.ObligorRule.SystemGeneratedIdentifier == local
        .New1.SystemGeneratedIdentifier)
      {
        // *** If update, this is the same rule as that being updated.  Skip it.
        if (Equal(global.Command, "UPDATE"))
        {
          continue;
        }
      }

      // *** If it's expired the same day, skip it.
      if (Equal(entities.ObligorRule.DiscontinueDate,
        entities.ObligorRule.EffectiveDate))
      {
        continue;
      }

      if (Lt(local.New1.EffectiveDate, entities.ObligorRule.EffectiveDate))
      {
        // *** Check for New expiry date > start date of  future rule.
        if (Lt(entities.ObligorRule.EffectiveDate, local.New1.DiscontinueDate))
        {
          export.ExpireError.Flag = "Y";
          export.EffectiveError.Flag = "Y";
          ++export.Error.Count;
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
      }
      else
      {
        // *** Obligor rule effective date is less or equal to the new obligor 
        // rule effective date.
        if (Equal(entities.ObligorRule.EffectiveDate, local.New1.EffectiveDate))
        {
          if (Equal(entities.ObligorRule.EffectiveDate, Now().Date))
          {
            // : This situation is ok.  We want to allow update of default 
            // instructions the day a recovery obligation is created.  The
            // create and update cabs will set the discontinue date of the
            // default instructions to the same date.
            return;
          }

          // *** Error: New effective date = Old effective date.
          export.EffectiveError.Flag = "Y";
          ++export.Error.Count;
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }

        if (Lt(local.New1.EffectiveDate, entities.ObligorRule.DiscontinueDate) &&
          !Equal(entities.ObligorRule.DiscontinueDate, local.HighDate.Date))
        {
          // *** Error: New Effective date is less than existing discontinue 
          // date.
          export.EffectiveError.Flag = "Y";
          ++export.Error.Count;
          ExitState = "ACO_NE0000_DATE_OVERLAP";
        }

        // *** Stay in the loop until the effective date is less than new
        //  effective date (usually will go thru the loop once only).
        if (Lt(entities.ObligorRule.EffectiveDate, Now().Date))
        {
          return;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadObligorRule()
  {
    entities.ObligorRule.Populated = false;

    return ReadEach("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableString(command, "cspDNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.Type1 = db.GetString(reader, 5);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.ObligorRule.Type1);

        return true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public RecaptureRule New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of ZdelImportCommandAttribute.
    /// </summary>
    [JsonPropertyName("zdelImportCommandAttribute")]
    public WorkArea ZdelImportCommandAttribute
    {
      get => zdelImportCommandAttribute ??= new();
      set => zdelImportCommandAttribute = value;
    }

    /// <summary>
    /// A value of ZdelImportValidCodeAttribute.
    /// </summary>
    [JsonPropertyName("zdelImportValidCodeAttribute")]
    public Common ZdelImportValidCodeAttribute
    {
      get => zdelImportValidCodeAttribute ??= new();
      set => zdelImportValidCodeAttribute = value;
    }

    /// <summary>
    /// A value of ZdelImportNew.
    /// </summary>
    [JsonPropertyName("zdelImportNew")]
    public ExpireEffectiveDateAttributes ZdelImportNew
    {
      get => zdelImportNew ??= new();
      set => zdelImportNew = value;
    }

    /// <summary>
    /// A value of ZdelImportOld.
    /// </summary>
    [JsonPropertyName("zdelImportOld")]
    public ExpireEffectiveDateAttributes ZdelImportOld
    {
      get => zdelImportOld ??= new();
      set => zdelImportOld = value;
    }

    private CsePerson csePerson;
    private RecaptureRule new1;
    private Common error;
    private Common active;
    private WorkArea zdelImportCommandAttribute;
    private Common zdelImportValidCodeAttribute;
    private ExpireEffectiveDateAttributes zdelImportNew;
    private ExpireEffectiveDateAttributes zdelImportOld;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ExpireError.
    /// </summary>
    [JsonPropertyName("expireError")]
    public Common ExpireError
    {
      get => expireError ??= new();
      set => expireError = value;
    }

    /// <summary>
    /// A value of EffectiveError.
    /// </summary>
    [JsonPropertyName("effectiveError")]
    public Common EffectiveError
    {
      get => effectiveError ??= new();
      set => effectiveError = value;
    }

    private Common error;
    private Common expireError;
    private Common effectiveError;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public RecaptureRule New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of InitializedToZero.
    /// </summary>
    [JsonPropertyName("initializedToZero")]
    public DateWorkArea InitializedToZero
    {
      get => initializedToZero ??= new();
      set => initializedToZero = value;
    }

    /// <summary>
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    private RecaptureRule new1;
    private DateWorkArea initializedToZero;
    private DateWorkArea highDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private RecaptureRule obligorRule;
  }
#endregion
}

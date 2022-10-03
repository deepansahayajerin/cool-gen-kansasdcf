// Program: SI_INCS_END_DT_PRIOR_EMPLOYMENT, ID: 1625333400, model: 746.
// Short name: SWE01133
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_INCS_END_DT_PRIOR_EMPLOYMENT.
/// </summary>
[Serializable]
public partial class SiIncsEndDtPriorEmployment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_END_DT_PRIOR_EMPLOYMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsEndDtPriorEmployment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsEndDtPriorEmployment.
  /// </summary>
  public SiIncsEndDtPriorEmployment(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 08/14/18  GVandy	CQ61457		Initial Code.
    // -------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Overview
    // When a new income source with Type 'E', Return Code 'E', and no end date 
    // is entered
    // then end date all previously existing income sources with Type 'E', 
    // Return Code 'E'
    // and end date '12-31-2099'.  The return code will be changed to 'N' and 
    // the end date
    // will be set to the start date of the new income source.
    // --------------------------------------------------------------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (AsChar(import.IncomeSource.Type1) == 'E' && AsChar
      (import.IncomeSource.ReturnCd) == 'E' && (
        Equal(import.IncomeSource.EndDt, local.Null1.Date) || Equal
      (import.IncomeSource.EndDt, local.Max.Date)))
    {
      // --Continue
    }
    else
    {
      return;
    }

    foreach(var item in ReadIncomeSource())
    {
      try
      {
        UpdateIncomeSource();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.
          SetNullableDate(command, "endDt", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 2);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      });
  }

  private void UpdateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var returnCd = "N";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var endDt = import.IncomeSource.StartDt;

    entities.IncomeSource.Populated = false;
    Update("UpdateIncomeSource",
      (db, command) =>
      {
        db.SetNullableString(command, "returnCd", returnCd);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.ReturnCd = returnCd;
    entities.IncomeSource.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Populated = true;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DateWorkArea max;
    private DateWorkArea null1;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
  }
#endregion
}

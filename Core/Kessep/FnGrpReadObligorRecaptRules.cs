// Program: FN_GRP_READ_OBLIGOR_RECAPT_RULES, ID: 372128807, model: 746.
// Short name: SWE00480
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
/// A program: FN_GRP_READ_OBLIGOR_RECAPT_RULES.
/// </para>
/// <para>
/// This Action Block reads the CSE_PERSON table using the inputed key, then 
/// reads all RECAPTURE_RULES associated.
/// </para>
/// </summary>
[Serializable]
public partial class FnGrpReadObligorRecaptRules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GRP_READ_OBLIGOR_RECAPT_RULES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGrpReadObligorRecaptRules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGrpReadObligorRecaptRules.
  /// </summary>
  public FnGrpReadObligorRecaptRules(IContext context, Import import,
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
    // --------------------------------------------
    // FN Grp Read Obligor Recapt Rules
    // Date Created    Created by
    // 07/24/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 03/19/1996      R.B.Mohapatra - MTW
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCsePerson())
    {
      if (ReadObligor())
      {
        export.Group.Index = -1;

        foreach(var item in ReadObligorRule())
        {
          if (export.Group.IsFull)
          {
            ExitState = "FN0000_MORE_DATA_EXISTS";

            return;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Detail.Assign(entities.ObligorRule);

          if (Lt(local.InitialisedToZeros.Date,
            entities.ObligorRule.NegotiatedDate))
          {
            export.Group.Update.DetailNeg.Flag = "Y";
          }
          else
          {
            export.Group.Update.DetailNeg.Flag = "";
          }

          if (Equal(export.Group.Item.Detail.DiscontinueDate, local.MaxDate.Date))
            
          {
            export.Group.Update.Detail.DiscontinueDate =
              local.InitialisedToZeros.Date;
          }

          // *** Set a flag to be used in protection logic.  IF discontinue date
          // is less than current date, set to "X" - all fields on the
          // occurrence should be protected.  If only effective date is less
          // than current date, all fields except discontinue date should be
          // protected.
          if (Lt(entities.ObligorRule.EffectiveDate, Now().Date))
          {
            if (Lt(entities.ObligorRule.DiscontinueDate, Now().Date))
            {
              // *** Protect all fields on screen for this recapture rule.
              export.Group.Update.Select.Flag = "X";
            }
            else
            {
              // *** Protect all fields for this recapture rule,  except 
              // discontinue date.
              export.Group.Update.Select.Flag = "Y";
            }
          }

          // *** If no updates have been done on the rule, set the last update 
          // timestamp and updated by to created date and created by.
          if (IsEmpty(export.Group.Item.Detail.LastUpdatedBy))
          {
            export.Group.Update.Detail.LastUpdatedBy =
              entities.ObligorRule.CreatedBy;
            export.Group.Update.Detail.LastUpdatedTmst =
              entities.ObligorRule.CreatedTimestamp;
          }
        }
      }
      else
      {
        ExitState = "FN0000_INVALID_OBLIGOR";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGOR_NF";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private IEnumerable<bool> ReadObligorRule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ObligorRule.Populated = false;

    return ReadEach("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaDType", entities.Obligor.Type1);
        db.SetNullableString(command, "cspDNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.NegotiatedDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.ObligorRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 8);
        entities.ObligorRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ObligorRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ObligorRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 11);
        entities.ObligorRule.PassthruPercentage =
          db.GetNullableInt32(reader, 12);
        entities.ObligorRule.PassthruAmount = db.GetNullableDecimal(reader, 13);
        entities.ObligorRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 14);
        entities.ObligorRule.CreatedBy = db.GetString(reader, 15);
        entities.ObligorRule.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.ObligorRule.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.ObligorRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.ObligorRule.Type1 = db.GetString(reader, 19);
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

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public RecaptureRule Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailNeg.
      /// </summary>
      [JsonPropertyName("detailNeg")]
      public Common DetailNeg
      {
        get => detailNeg ??= new();
        set => detailNeg = value;
      }

      /// <summary>
      /// A value of PrintRcaprpay.
      /// </summary>
      [JsonPropertyName("printRcaprpay")]
      public Common PrintRcaprpay
      {
        get => printRcaprpay ??= new();
        set => printRcaprpay = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common select;
      private RecaptureRule detail;
      private Common detailNeg;
      private Common printRcaprpay;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligor;
    private RecaptureRule obligorRule;
    private CsePerson csePerson;
  }
#endregion
}

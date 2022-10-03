// Program: FN_BFXF_BUILD_HOUSEHOLD_HIST_TBL, ID: 374497604, model: 746.
// Short name: SWE02901
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXF_BUILD_HOUSEHOLD_HIST_TBL.
/// </summary>
[Serializable]
public partial class FnBfxfBuildHouseholdHistTbl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXF_BUILD_HOUSEHOLD_HIST_TBL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxfBuildHouseholdHistTbl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxfBuildHouseholdHistTbl.
  /// </summary>
  public FnBfxfBuildHouseholdHistTbl(IContext context, Import import,
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
    // --------------------------------
    // Cloned from fn_build_household_hist_table.
    // Tailored to fit BFXF
    // --------------------------------
    local.Collection.Year = Year(import.CashReceiptDetail.CollectionDate);
    local.Collection.Month = Month(import.CashReceiptDetail.CollectionDate);

    if (AsChar(import.MedicalObligationType.Flag) != 'Y')
    {
      ReadImHouseholdMbrMnthlySum3();
    }
    else
    {
      ReadImHouseholdMbrMnthlySum4();
    }

    if (import.Collection.Amount > local.SuppPersonTotalUra.TotalCurrency)
    {
      // : Build the list of Supported Persons that are related to the Obligor.
      foreach(var item in ReadCsePersonObligation())
      {
        // : If the Supported Person has never been in a Household, bypass that 
        // person.
        if (!ReadImHouseholdMbrMnthlySum1())
        {
          continue;
        }

        if (export.Legal.IsEmpty)
        {
          export.Legal.Index = 0;
          export.Legal.CheckSize();

          export.Legal.Update.LegalSuppPrsn.Number =
            entities.ExistingSuppPrsnDebt.Number;

          continue;
        }

        for(export.Legal.Index = 0; export.Legal.Index < export.Legal.Count; ++
          export.Legal.Index)
        {
          if (!export.Legal.CheckSize())
          {
            break;
          }

          if (Equal(export.Legal.Item.LegalSuppPrsn.Number,
            entities.ExistingSuppPrsnDebt.Number))
          {
            goto ReadEach1;
          }
        }

        export.Legal.CheckIndex();

        export.Legal.Index = export.Legal.Count;
        export.Legal.CheckSize();

        export.Legal.Update.LegalSuppPrsn.Number =
          entities.ExistingSuppPrsnDebt.Number;

        if (export.Legal.Count == Export.LegalGroup.Capacity)
        {
          break;
        }

ReadEach1:
        ;
      }

      if (export.Legal.IsEmpty)
      {
        return;
      }

      // : Build the temporary list of all of the associated court orders.
      if (IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
      {
        for(export.Legal.Index = 0; export.Legal.Index < export.Legal.Count; ++
          export.Legal.Index)
        {
          if (!export.Legal.CheckSize())
          {
            break;
          }

          foreach(var item in ReadLegalAction2())
          {
            if (local.Legal.IsEmpty)
            {
              local.Legal.Index = 0;
              local.Legal.CheckSize();

              local.Legal.Update.Legal1.StandardNumber =
                entities.ExistingLegalAction.StandardNumber;

              continue;
            }

            for(local.Legal.Index = 0; local.Legal.Index < local.Legal.Count; ++
              local.Legal.Index)
            {
              if (!local.Legal.CheckSize())
              {
                break;
              }

              if (Equal(local.Legal.Item.Legal1.StandardNumber,
                entities.ExistingLegalAction.StandardNumber))
              {
                goto ReadEach2;
              }
            }

            local.Legal.CheckIndex();

            local.Legal.Index = local.Legal.Count;
            local.Legal.CheckSize();

            local.Legal.Update.Legal1.StandardNumber =
              entities.ExistingLegalAction.StandardNumber;

            if (local.Legal.Index + 1 == Local.LegalGroup.Capacity)
            {
              goto AfterCycle1;
            }

ReadEach2:
            ;
          }
        }

AfterCycle1:

        export.Legal.CheckIndex();
      }
      else if (ReadLegalAction1())
      {
        local.Legal.Index = 0;
        local.Legal.CheckSize();

        local.Legal.Update.Legal1.StandardNumber =
          entities.ExistingLegalAction.StandardNumber;
      }

      // : Use the temporary list of court orders to drive the addition of legal
      // action related persons.
      if (!local.Legal.IsEmpty)
      {
        for(local.Legal.Index = 0; local.Legal.Index < local.Legal.Count; ++
          local.Legal.Index)
        {
          if (!local.Legal.CheckSize())
          {
            break;
          }

          foreach(var item in ReadCsePerson2())
          {
            for(export.Legal.Index = 0; export.Legal.Index < export
              .Legal.Count; ++export.Legal.Index)
            {
              if (!export.Legal.CheckSize())
              {
                break;
              }

              if (Equal(export.Legal.Item.LegalSuppPrsn.Number,
                entities.ExistingSuppPrsnLegal.Number))
              {
                goto ReadEach3;
              }
            }

            export.Legal.CheckIndex();

            if (!ReadImHouseholdMbrMnthlySum2())
            {
              continue;
            }

            export.Legal.Index = export.Legal.Count;
            export.Legal.CheckSize();

            export.Legal.Update.LegalSuppPrsn.Number =
              entities.ExistingSuppPrsnLegal.Number;

            if (export.Legal.Count == Export.LegalGroup.Capacity)
            {
              goto Test;
            }

ReadEach3:
            ;
          }
        }

        local.Legal.CheckIndex();
      }

Test:

      // : Build the Legal Action details for each Supported Person.
      for(export.Legal.Index = 0; export.Legal.Index < export.Legal.Count; ++
        export.Legal.Index)
      {
        if (!export.Legal.CheckSize())
        {
          break;
        }

        foreach(var item in ReadLegalAction2())
        {
          if (export.Legal.Item.LegalDtl.IsEmpty)
          {
            export.Legal.Item.LegalDtl.Index = 0;
            export.Legal.Item.LegalDtl.CheckSize();
          }
          else
          {
            export.Legal.Item.LegalDtl.Index = export.Legal.Index + 1;
            export.Legal.Item.LegalDtl.CheckSize();
          }

          export.Legal.Update.LegalDtl.Update.LegalDtl1.StandardNumber =
            entities.ExistingLegalAction.StandardNumber;

          if (export.Legal.Item.LegalDtl.Index + 1 == Export
            .LegalDtlGroup.Capacity)
          {
            goto Next1;
          }
        }

Next1:
        ;
      }

      export.Legal.CheckIndex();

      // : Build a temporary table of all Members involved in the household's 
      // that the original Supported Person or Legal Action Person's are related
      // too.
      for(export.Legal.Index = 0; export.Legal.Index < export.Legal.Count; ++
        export.Legal.Index)
      {
        if (!export.Legal.CheckSize())
        {
          break;
        }

        local.SuppPrsn.Index = export.Legal.Index;
        local.SuppPrsn.CheckSize();

        local.SuppPrsn.Update.SuppPrsn1.Number =
          export.Legal.Item.LegalSuppPrsn.Number;
      }

      export.Legal.CheckIndex();

      for(export.Legal.Index = 0; export.Legal.Index < export.Legal.Count; ++
        export.Legal.Index)
      {
        if (!export.Legal.CheckSize())
        {
          break;
        }

        foreach(var item in ReadImHousehold())
        {
          foreach(var item1 in ReadCsePerson1())
          {
            for(local.SuppPrsn.Index = 0; local.SuppPrsn.Index < local
              .SuppPrsn.Count; ++local.SuppPrsn.Index)
            {
              if (!local.SuppPrsn.CheckSize())
              {
                break;
              }

              if (Equal(local.SuppPrsn.Item.SuppPrsn1.Number,
                entities.ExistingSuppPrsnHh.Number))
              {
                goto ReadEach4;
              }
            }

            local.SuppPrsn.CheckIndex();

            local.SuppPrsn.Index = local.SuppPrsn.Count;
            local.SuppPrsn.CheckSize();

            local.SuppPrsn.Update.SuppPrsn1.Number =
              entities.ExistingSuppPrsnHh.Number;

            if (local.SuppPrsn.Index + 1 == Local.SuppPrsnGroup.Capacity)
            {
              goto AfterCycle2;
            }

ReadEach4:
            ;
          }
        }
      }

AfterCycle2:

      export.Legal.CheckIndex();
    }
    else
    {
      export.Legal.Index = 0;
      export.Legal.CheckSize();

      export.Legal.Update.LegalSuppPrsn.Number = import.SuppPerson.Number;

      local.SuppPrsn.Index = 0;
      local.SuppPrsn.CheckSize();

      local.SuppPrsn.Update.SuppPrsn1.Number = import.SuppPerson.Number;
    }

    // : Build the Household History for each Member.
    for(local.SuppPrsn.Index = 0; local.SuppPrsn.Index < local.SuppPrsn.Count; ++
      local.SuppPrsn.Index)
    {
      if (!local.SuppPrsn.CheckSize())
      {
        break;
      }

      export.HhHist.Index = local.SuppPrsn.Index;
      export.HhHist.CheckSize();

      export.HhHist.Update.HhHistSuppPrsn.Number =
        local.SuppPrsn.Item.SuppPrsn1.Number;

      foreach(var item in ReadImHouseholdImHouseholdMbrMnthlySum())
      {
        if (export.HhHist.Item.HhHistDtl.IsEmpty)
        {
          export.HhHist.Item.HhHistDtl.Index = 0;
          export.HhHist.Item.HhHistDtl.CheckSize();
        }
        else
        {
          ++export.HhHist.Item.HhHistDtl.Index;
          export.HhHist.Item.HhHistDtl.CheckSize();
        }

        export.HhHist.Update.HhHistDtl.Update.HhHistDtlImHousehold.AeCaseNo =
          entities.ExistingImHousehold.AeCaseNo;
        export.HhHist.Update.HhHistDtl.Update.HhHistDtlImHouseholdMbrMnthlySum.
          Assign(entities.ExistingImHouseholdMbrMnthlySum);

        if (export.HhHist.Item.HhHistDtl.Index + 1 == Export
          .HhHistDtlGroup.Capacity)
        {
          goto Next2;
        }
      }

Next2:
      ;
    }

    local.SuppPrsn.CheckIndex();
  }

  private IEnumerable<bool> ReadCsePerson1()
  {
    entities.ExistingSuppPrsnHh.Populated = false;

    return ReadEach("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "imhAeCaseNo", entities.ExistingGroupHhImHousehold.AeCaseNo);
          
      },
      (db, reader) =>
      {
        entities.ExistingSuppPrsnHh.Number = db.GetString(reader, 0);
        entities.ExistingSuppPrsnHh.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.ExistingSuppPrsnLegal.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.Legal.Item.Legal1.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSuppPrsnLegal.Number = db.GetString(reader, 0);
        entities.ExistingSuppPrsnLegal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligation()
  {
    entities.ExistingSuppPrsnDebt.Populated = false;
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadCsePersonObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSuppPrsnDebt.Number = db.GetString(reader, 0);
        entities.ExistingObligation.CpaType = db.GetString(reader, 1);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 2);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingSuppPrsnDebt.Populated = true;
        entities.ExistingObligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadImHousehold()
  {
    entities.ExistingGroupHhImHousehold.Populated = false;

    return ReadEach("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Legal.Item.LegalSuppPrsn.Number);
      },
      (db, reader) =>
      {
        entities.ExistingGroupHhImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ExistingGroupHhImHousehold.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdImHouseholdMbrMnthlySum()
  {
    entities.ExistingImHousehold.Populated = false;
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.HhHist.Item.HhHistSuppPrsn.Number);
        db.SetInt32(command, "year0", local.Collection.Year);
        db.SetInt32(command, "month0", local.Collection.Month);
      },
      (db, reader) =>
      {
        entities.ExistingImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 6);
        entities.ExistingImHousehold.Populated = true;
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum1()
  {
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.ExistingSuppPrsnDebt.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 6);
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum2()
  {
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.ExistingSuppPrsnLegal.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 6);
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum3()
  {
    return Read("ReadImHouseholdMbrMnthlySum3",
      (db, command) =>
      {
        db.SetInt32(command, "year0", local.Collection.Year);
        db.SetInt32(command, "month0", local.Collection.Month);
        db.SetString(command, "cspNumber", import.SuppPerson.Number);
      },
      (db, reader) =>
      {
        local.SuppPersonTotalUra.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadImHouseholdMbrMnthlySum4()
  {
    return Read("ReadImHouseholdMbrMnthlySum4",
      (db, command) =>
      {
        db.SetInt32(command, "year0", local.Collection.Year);
        db.SetInt32(command, "month0", local.Collection.Month);
        db.SetString(command, "cspNumber", import.SuppPerson.Number);
      },
      (db, reader) =>
      {
        local.SuppPersonTotalUra.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", export.Legal.Item.LegalSuppPrsn.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;

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
    /// A value of SuppPerson.
    /// </summary>
    [JsonPropertyName("suppPerson")]
    public CsePerson SuppPerson
    {
      get => suppPerson ??= new();
      set => suppPerson = value;
    }

    /// <summary>
    /// A value of MedicalObligationType.
    /// </summary>
    [JsonPropertyName("medicalObligationType")]
    public Common MedicalObligationType
    {
      get => medicalObligationType ??= new();
      set => medicalObligationType = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CsePerson suppPerson;
    private Common medicalObligationType;
    private Collection collection;
    private CsePerson obligor;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn")]
      public CsePerson LegalSuppPrsn
      {
        get => legalSuppPrsn ??= new();
        set => legalSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of Legal1.
      /// </summary>
      [JsonPropertyName("legal1")]
      public LegalAction Legal1
      {
        get => legal1 ??= new();
        set => legal1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private LegalAction legal1;
    }

    /// <summary>A SuppPrsnGroup group.</summary>
    [Serializable]
    public class SuppPrsnGroup
    {
      /// <summary>
      /// A value of SuppPrsn1.
      /// </summary>
      [JsonPropertyName("suppPrsn1")]
      public CsePerson SuppPrsn1
      {
        get => suppPrsn1 ??= new();
        set => suppPrsn1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson suppPrsn1;
    }

    /// <summary>A DelMeLocalGroupHhHistDtlGroup group.</summary>
    [Serializable]
    public class DelMeLocalGroupHhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtl1.
      /// </summary>
      [JsonPropertyName("hhHistDtl1")]
      public ImHousehold HhHistDtl1
      {
        get => hhHistDtl1 ??= new();
        set => hhHistDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ImHousehold hhHistDtl1;
    }

    /// <summary>
    /// A value of SuppPersonTotalUra.
    /// </summary>
    [JsonPropertyName("suppPersonTotalUra")]
    public Common SuppPersonTotalUra
    {
      get => suppPersonTotalUra ??= new();
      set => suppPersonTotalUra = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of SuppPrsn.
    /// </summary>
    [JsonIgnore]
    public Array<SuppPrsnGroup> SuppPrsn => suppPrsn ??= new(
      SuppPrsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SuppPrsn for json serialization.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    [Computed]
    public IList<SuppPrsnGroup> SuppPrsn_Json
    {
      get => suppPrsn;
      set => SuppPrsn.Assign(value);
    }

    /// <summary>
    /// Gets a value of DelMeLocalGroupHhHistDtl.
    /// </summary>
    [JsonIgnore]
    public Array<DelMeLocalGroupHhHistDtlGroup> DelMeLocalGroupHhHistDtl =>
      delMeLocalGroupHhHistDtl ??= new(DelMeLocalGroupHhHistDtlGroup.Capacity, 0);
      

    /// <summary>
    /// Gets a value of DelMeLocalGroupHhHistDtl for json serialization.
    /// </summary>
    [JsonPropertyName("delMeLocalGroupHhHistDtl")]
    [Computed]
    public IList<DelMeLocalGroupHhHistDtlGroup> DelMeLocalGroupHhHistDtl_Json
    {
      get => delMeLocalGroupHhHistDtl;
      set => DelMeLocalGroupHhHistDtl.Assign(value);
    }

    private Common suppPersonTotalUra;
    private DateWorkArea collection;
    private Array<LegalGroup> legal;
    private Array<SuppPrsnGroup> suppPrsn;
    private Array<DelMeLocalGroupHhHistDtlGroup> delMeLocalGroupHhHistDtl;
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
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingSuppPrsnHh.
    /// </summary>
    [JsonPropertyName("existingSuppPrsnHh")]
    public CsePerson ExistingSuppPrsnHh
    {
      get => existingSuppPrsnHh ??= new();
      set => existingSuppPrsnHh = value;
    }

    /// <summary>
    /// A value of ExistingSuppPrsnDebt.
    /// </summary>
    [JsonPropertyName("existingSuppPrsnDebt")]
    public CsePerson ExistingSuppPrsnDebt
    {
      get => existingSuppPrsnDebt ??= new();
      set => existingSuppPrsnDebt = value;
    }

    /// <summary>
    /// A value of ExistingSuppPrsnLegal.
    /// </summary>
    [JsonPropertyName("existingSuppPrsnLegal")]
    public CsePerson ExistingSuppPrsnLegal
    {
      get => existingSuppPrsnLegal ??= new();
      set => existingSuppPrsnLegal = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlySupported.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySupported")]
    public CsePersonAccount ExistingKeyOnlySupported
    {
      get => existingKeyOnlySupported ??= new();
      set => existingKeyOnlySupported = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingGroupHhImHousehold.
    /// </summary>
    [JsonPropertyName("existingGroupHhImHousehold")]
    public ImHousehold ExistingGroupHhImHousehold
    {
      get => existingGroupHhImHousehold ??= new();
      set => existingGroupHhImHousehold = value;
    }

    /// <summary>
    /// A value of ExistingGroupHhImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("existingGroupHhImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ExistingGroupHhImHouseholdMbrMnthlySum
    {
      get => existingGroupHhImHouseholdMbrMnthlySum ??= new();
      set => existingGroupHhImHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ExistingImHousehold.
    /// </summary>
    [JsonPropertyName("existingImHousehold")]
    public ImHousehold ExistingImHousehold
    {
      get => existingImHousehold ??= new();
      set => existingImHousehold = value;
    }

    /// <summary>
    /// A value of ExistingImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("existingImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ExistingImHouseholdMbrMnthlySum
    {
      get => existingImHouseholdMbrMnthlySum ??= new();
      set => existingImHouseholdMbrMnthlySum = value;
    }

    private CsePerson csePerson;
    private CsePerson existingObligor;
    private CsePersonAccount existingKeyOnlyObligor;
    private CsePerson existingSuppPrsnHh;
    private CsePerson existingSuppPrsnDebt;
    private CsePerson existingSuppPrsnLegal;
    private CsePersonAccount existingKeyOnlySupported;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private LegalAction existingLegalAction;
    private LegalActionPerson existingLegalActionPerson;
    private ImHousehold existingGroupHhImHousehold;
    private ImHouseholdMbrMnthlySum existingGroupHhImHouseholdMbrMnthlySum;
    private ImHousehold existingImHousehold;
    private ImHouseholdMbrMnthlySum existingImHouseholdMbrMnthlySum;
  }
#endregion
}

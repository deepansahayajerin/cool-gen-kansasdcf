// Program: FN_B717_LINE_2_6, ID: 373354045, model: 746.
// Short name: SWE03009
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_2_6.
/// </summary>
[Serializable]
public partial class FnB717Line4: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_2_6 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line4(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line4.
  /// </summary>
  public FnB717Line4(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveStatsVerifi2(import.Create, local.Create);

    foreach(var item in ReadCsePerson())
    {
      foreach(var item1 in ReadAccrualInstructionsObligationType())
      {
        if (!ReadDebtCsePerson2())
        {
          continue;
        }

        // -----------------------------------
        // Is SP on current case?
        // -----------------------------------
        if (ReadCaseRoleCaseRole())
        {
          local.Subscript.Count = 2;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 2;
            local.Create.SuppPersonNumber = entities.Supp.Number;
            local.Create.CaseRoleType = entities.ChOrAr.Type1;
            local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
            local.Create.ObligationType = entities.ObligationType.Code;
            UseFnB717CreateStatsVerifi();
          }

          return;
        }
      }
    }

    // -------------------------------------------------------------------
    // We got here because Accr Inst are not open.
    // Now check for outstanding balance.
    // -------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      foreach(var item1 in ReadDebtDetailCsePersonObligationType())
      {
        // -------------------------------------------------------------------
        // Is SP on current case?
        // --------------------------------------------------------------------
        if (ReadCaseRoleCaseRole())
        {
          local.Subscript.Count = 2;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 2;
            local.Create.SuppPersonNumber = entities.Supp.Number;
            local.Create.CaseRoleType = entities.ChOrAr.Type1;
            local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
            local.Create.Dddd = entities.DebtDetail.DueDt;
            local.Create.DebtDetailBaldue = entities.DebtDetail.BalanceDueAmt;
            local.Create.ObligationType = entities.ObligationType.Code;
            UseFnB717CreateStatsVerifi();
          }

          local.Subscript.Count = 6;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 6;
            UseFnB717CreateStatsVerifi();
          }

          return;
        }
      }

      // -------------------------------------------------------------------
      // We got here because Accr Inst are not open and there is no
      // outstanding balance. Check for collections after the report period.
      // -------------------------------------------------------------------
      foreach(var item1 in ReadCollectionObligationType())
      {
        if (!ReadDebtCsePerson1())
        {
          continue;
        }

        // -------------------------------------------------------------------
        // Is SP on current case?
        // --------------------------------------------------------------------
        if (ReadCaseRoleCaseRole())
        {
          local.Subscript.Count = 2;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 2;
            local.Create.SuppPersonNumber = entities.Supp.Number;
            local.Create.CaseRoleType = entities.ChOrAr.Type1;
            local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
            local.Create.CollCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.Create.CollectionDate = entities.Collection.CollectionDt;
            local.Create.CollectionAmount = entities.Collection.Amount;
            local.Create.ObligationType = entities.ObligationType.Code;
            UseFnB717CreateStatsVerifi();
          }

          local.Subscript.Count = 6;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 6;
            UseFnB717CreateStatsVerifi();
          }

          return;
        }
      }
    }

    // -------------------------------------------------------------------
    // Fees are counted in 6 but not in 2. Check if fees are outstanding.
    // -------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      if (ReadDebtDetail())
      {
        local.Subscript.Count = 6;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 6;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.Dddd = entities.DebtDetail.DueDt;
          local.Create.DebtDetailBaldue = entities.DebtDetail.BalanceDueAmt;
          local.Create.ObligationType = "FEE";
          UseFnB717CreateStatsVerifi();
        }

        return;
      }

      foreach(var item1 in ReadCollection())
      {
        if (!ReadDebt())
        {
          continue;
        }

        local.Subscript.Count = 6;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 6;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.CollCreatedDate = Date(entities.Collection.CreatedTmst);
          local.Create.CollectionDate = entities.Collection.CollectionDt;
          local.Create.CollectionAmount = entities.Collection.Amount;
          local.Create.ObligationType = "FEE";
          UseFnB717CreateStatsVerifi();
        }

        return;
      }
    }
  }

  private static void MoveGroup1(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStatsVerifi1(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.Dddd = source.Dddd;
    target.DebtDetailBaldue = source.DebtDetailBaldue;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
    target.CaseRoleType = source.CaseRoleType;
  }

  private static void MoveStatsVerifi2(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi1(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717InflateGv()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveProgram(import.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private IEnumerable<bool> ReadAccrualInstructionsObligationType()
  {
    entities.AccrualInstructions.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadAccrualInstructionsObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(
          command, "asOfDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ObligationType.Code = db.GetString(reader, 9);
        entities.AccrualInstructions.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRoleCaseRole()
  {
    entities.ApCaseRole.Populated = false;
    entities.ChOrAr.Populated = false;

    return Read("ReadCaseRoleCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.Supp.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChOrAr.CasNumber = db.GetString(reader, 6);
        entities.ChOrAr.CspNumber = db.GetString(reader, 7);
        entities.ChOrAr.Type1 = db.GetString(reader, 8);
        entities.ChOrAr.Identifier = db.GetInt32(reader, 9);
        entities.ChOrAr.StartDate = db.GetNullableDate(reader, 10);
        entities.ChOrAr.EndDate = db.GetNullableDate(reader, 11);
        entities.ApCaseRole.Populated = true;
        entities.ChOrAr.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionObligationType",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.ObligationType.Code = db.GetString(reader, 19);
        entities.ObligationType.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Supp.Number = db.GetString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Supp.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.Supp.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePerson2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "obTrnTyp", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyType", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obTrnId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Supp.Number = db.GetString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Supp.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCsePersonObligationType()
  {
    entities.Supp.Populated = false;
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCsePersonObligationType",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 8);
        entities.Supp.Number = db.GetString(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligationType.Code = db.GetString(reader, 11);
        entities.Supp.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 35;

      private StatsReport statsReport;
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

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    private Array<GroupGroup> group;
    private Program program;
    private Case1 case1;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private StatsVerifi create;
    private Common displayInd;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    private Common subscript;
    private StatsVerifi create;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ChOrAr.
    /// </summary>
    [JsonPropertyName("chOrAr")]
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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

    private CsePersonAccount obligor;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chOrAr;
    private CsePerson supp;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private Collection collection;
  }
#endregion
}

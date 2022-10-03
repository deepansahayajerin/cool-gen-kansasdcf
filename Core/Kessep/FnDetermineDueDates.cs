// Program: FN_DETERMINE_DUE_DATES, ID: 371968289, model: 746.
// Short name: SWE00436
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
/// A program: FN_DETERMINE_DUE_DATES.
/// </para>
/// <para>
/// RESP: FINANCE
/// This Common Action Block will accept the accrual instructions and frequency 
/// period and compute the due dates during any given month to be used in the
/// creation of a debt detail.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetermineDueDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_DUE_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineDueDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineDueDates.
  /// </summary>
  public FnDetermineDueDates(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // 4/17/99 - bud adams  -  removed 16 ROUNDED functions;
    //   got rid of 'number-of-reads' counter (zdel the view)
    // 6/3/99 - bud adams  -  Export_Valid_Date_Ind has 3 values:
    //   Y - valid date;  N - not valid;  S - totally suspended
    //   Process_current date might be changed in Veify_Due_Date
    //   if the obligation is totally suspended, to be the Accrual_
    //   Suspension Resume_Dt to prevent looping through here
    //   for nothing.
    // =================================================
    export.ProcessCurrent.Date = import.ProcessCurrent.Date;

    // ***** HARD CODE AREA *****
    local.DateValidation.Year = Year(import.ProcessCurrent.Date);
    local.DateValidation.Month = Month(import.ProcessCurrent.Date);

    // *****  Determine the maximum number of days in the processing month.
    UseCabFirstAndLastDateOfMonth1();
    local.DateValidation.MaximumNumberOfDaysInMonth =
      Day(local.LastDateOfMonth.Date);

    // ***** MAIN-LINE AREA *****
    local.Finished.Flag = "N";

    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "M":
        // <<<<< MONTHLY >>>>>
        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (local.DateValidation.MaximumNumberOfDaysInMonth < import
            .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault())
          {
            local.DateValidation.Day =
              local.DateValidation.MaximumNumberOfDaysInMonth;
          }
          else
          {
            local.DateValidation.Day =
              import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
          }

          UseFnAssembleDate2();
          UseFnVerifyDueDate();

          if (AsChar(export.ValidDateInd.Flag) == 'Y')
          {
            export.Export1.Update.Debt.Amount = local.Debt.Amount;
            export.Export1.Update.DebtDetail.DueDt = local.DebtDetail.DueDt;
          }

          local.Finished.Flag = "Y";
          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      case "SM":
        // <<<<< Semi-Monthly >>>>>
        local.Loop.Count = 0;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          ++local.Loop.Count;

          if (local.Loop.Count == 1)
          {
            if (local.DateValidation.MaximumNumberOfDaysInMonth < import
              .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault())
            {
              local.DateValidation.Day =
                local.DateValidation.MaximumNumberOfDaysInMonth;
            }
            else
            {
              local.DateValidation.Day =
                import.ObligationPaymentSchedule.DayOfMonth1.
                  GetValueOrDefault();
            }
          }
          else
          {
            if (local.DateValidation.MaximumNumberOfDaysInMonth < import
              .ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault())
            {
              local.DateValidation.Day =
                local.DateValidation.MaximumNumberOfDaysInMonth;
            }
            else
            {
              local.DateValidation.Day =
                import.ObligationPaymentSchedule.DayOfMonth2.
                  GetValueOrDefault();
            }

            local.Finished.Flag = "Y";
          }

          UseFnAssembleDate2();
          UseFnVerifyDueDate();

          if (AsChar(export.ValidDateInd.Flag) == 'Y')
          {
            export.Export1.Update.Debt.Amount = local.Debt.Amount;
            export.Export1.Update.DebtDetail.DueDt = local.DebtDetail.DueDt;
          }

          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      case "BM":
        // <<<<< Bi-Monthly >>>>>
        if (local.DateValidation.MaximumNumberOfDaysInMonth < import
          .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault())
        {
          local.DateValidation.Day =
            local.DateValidation.MaximumNumberOfDaysInMonth;
        }
        else
        {
          local.DateValidation.Day =
            import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
        }

        // **** find the max no of days in the month of accrual start date ****
        local.FirstPmtDue.Date = import.Persistent.AsOfDt;
        UseCabFirstAndLastDateOfMonth2();
        local.FirstPaymentDue.MaximumNumberOfDaysInMonth =
          Day(local.LastDateOfMonth.Date);

        // **** Calculate first payment due date ****
        if (local.FirstPaymentDue.MaximumNumberOfDaysInMonth < import
          .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault())
        {
          local.FirstPaymentDue.Day =
            local.DateValidation.MaximumNumberOfDaysInMonth;
        }
        else
        {
          local.FirstPaymentDue.Day =
            import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
        }

        local.FirstPaymentDue.Month = Month(import.Persistent.AsOfDt);
        local.FirstPaymentDue.Year = Year(import.Persistent.AsOfDt);
        UseFnAssembleDate1();

        if (!Lt(local.FirstDueDate.Date, import.Persistent.AsOfDt))
        {
        }
        else
        {
          local.FirstDueDate.Date = AddMonths(local.FirstDueDate.Date, 1);
        }

        // **** find out all the months in the year on which payment needs to be
        // made ****
        local.Grp.Index = 0;
        local.Grp.CheckSize();

        local.Grp.Update.Common.Count = Month(local.FirstDueDate.Date);
        local.NextPmtDueMth.Count = local.Grp.Item.Common.Count;

        while(local.NextPmtDueMth.Count <= 12)
        {
          local.NextPmtDueMth.Count += 2;

          if (local.NextPmtDueMth.Count <= 12)
          {
            ++local.Grp.Index;
            local.Grp.CheckSize();

            local.Grp.Update.Common.Count = local.NextPmtDueMth.Count;
          }
        }

        local.NextPmtDueMth.Count = Month(local.FirstDueDate.Date);

        while(local.NextPmtDueMth.Count >= 1)
        {
          local.NextPmtDueMth.Count -= 2;

          if (local.NextPmtDueMth.Count >= 1)
          {
            ++local.Grp.Index;
            local.Grp.CheckSize();

            local.Grp.Update.Common.Count = local.NextPmtDueMth.Count;
          }
        }

        // **** Find out next pay due date ****
        UseFnAssembleDate2();

        // **** Verify if the month of the current processing_date is a payable 
        // month ****
        export.ValidDateInd.Flag = "N";

        for(local.Grp.Index = 0; local.Grp.Index < 7; ++local.Grp.Index)
        {
          if (!local.Grp.CheckSize())
          {
            break;
          }

          if (local.Grp.Item.Common.Count > 0)
          {
            if (local.Grp.Item.Common.Count == Month(local.Due.Date))
            {
              export.ValidDateInd.Flag = "Y";

              break;
            }
          }
        }

        local.Grp.CheckIndex();

        local.Grp.Index = 0;
        local.Grp.CheckSize();

        if (AsChar(export.ValidDateInd.Flag) == 'Y')
        {
          UseFnVerifyDueDate();

          export.Export1.Index = 0;
          export.Export1.Clear();

          do
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            if (AsChar(export.ValidDateInd.Flag) == 'Y')
            {
              export.Export1.Update.Debt.Amount = local.Debt.Amount;
              export.Export1.Update.DebtDetail.DueDt = local.DebtDetail.DueDt;
            }

            local.Finished.Flag = "Y";
            export.Export1.Next();
          }
          while(AsChar(local.Finished.Flag) != 'Y');
        }

        break;
      case "W":
        // <<<<< Weekly >>>>>
        // *****  Set the Text of the day of the week that the obligation is 
        // due.
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.AccrualWorkArea.DayOfWeekText = "SUNDAY";

            break;
          case 2:
            local.AccrualWorkArea.DayOfWeekText = "MONDAY";

            break;
          case 3:
            local.AccrualWorkArea.DayOfWeekText = "TUESDAY";

            break;
          case 4:
            local.AccrualWorkArea.DayOfWeekText = "WEDNESDAY";

            break;
          case 5:
            local.AccrualWorkArea.DayOfWeekText = "THURSDAY";

            break;
          case 6:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
          case 7:
            local.AccrualWorkArea.DayOfWeekText = "SATURDAY";

            break;
          default:
            ExitState = "INVALID_DAY_OF_WEEK";

            return;
        }

        // *****  Find the date of each "Day of the Week" during the processing 
        // month.
        local.DateValidation.Day = 1;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          UseFnAssembleDate2();

          if (Equal(DayOfWeek(local.Due.Date),
            local.AccrualWorkArea.DayOfWeekText))
          {
            UseFnVerifyDueDate();

            if (AsChar(export.ValidDateInd.Flag) == 'Y')
            {
              export.Export1.Update.Debt.Amount = local.Debt.Amount;
              export.Export1.Update.DebtDetail.DueDt = local.DebtDetail.DueDt;
            }

            // *****  Once the first due date is found, increment by a week to 
            // the next due date.
            local.DateValidation.Day += 7;
          }
          else
          {
            ++local.DateValidation.Day;
          }

          if (local.DateValidation.Day > local
            .DateValidation.MaximumNumberOfDaysInMonth)
          {
            local.Finished.Flag = "Y";
          }

          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      case "BW":
        // <<<<< Bi-Weekly >>>>>
        // *****  Set the Text of the day of the week that the obligation is 
        // due.
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.AccrualWorkArea.DayOfWeekText = "SUNDAY";

            break;
          case 2:
            local.AccrualWorkArea.DayOfWeekText = "MONDAY";

            break;
          case 3:
            local.AccrualWorkArea.DayOfWeekText = "TUESDAY";

            break;
          case 4:
            local.AccrualWorkArea.DayOfWeekText = "WEDNESDAY";

            break;
          case 5:
            local.AccrualWorkArea.DayOfWeekText = "THURSDAY";

            break;
          case 6:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
          case 7:
            local.AccrualWorkArea.DayOfWeekText = "SATURDAY";

            break;
          default:
            ExitState = "INVALID_DAY_OF_WEEK";

            return;
        }

        // *****  Find the first payment due date.
        local.FirstDueDate.Date = import.ObligationPaymentSchedule.StartDt;

        while(!Equal(DayOfWeek(local.FirstDueDate.Date),
          local.AccrualWorkArea.DayOfWeekText))
        {
          local.FirstDueDate.Date = AddDays(local.FirstDueDate.Date, 1);
        }

        // *****  Find each of the due dates during the processing month.
        local.DateValidation.Day = 1;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          UseFnAssembleDate2();

          if (Equal(DayOfWeek(local.Due.Date),
            local.AccrualWorkArea.DayOfWeekText))
          {
            // ****  If the number of days since the first due date is divisible
            // by 14, then this date is a due date.  Otherwise, the following
            // week is a due date.
            local.DaysSinceFirstDue.TotalInteger =
              (long)DaysFromAD(local.Due.Date) - DaysFromAD
              (local.FirstDueDate.Date);
            local.Common.TotalInteger =
              (int)(local.DaysSinceFirstDue.TotalInteger / 14);
            local.Common.TotalInteger = local.DaysSinceFirstDue.TotalInteger - local
              .Common.TotalInteger * 14;

            if (local.Common.TotalInteger == 0)
            {
              UseFnVerifyDueDate();

              if (AsChar(export.ValidDateInd.Flag) == 'Y')
              {
                export.Export1.Update.Debt.Amount = local.Debt.Amount;
                export.Export1.Update.DebtDetail.DueDt = local.DebtDetail.DueDt;
              }

              // *****  Once the first due date is found, increment by two weeks
              // to the next due date.
              local.DateValidation.Day += 14;
            }
            else
            {
              // *****  The next due date is one week from this date.
              local.DateValidation.Day += 7;
            }
          }
          else
          {
            ++local.DateValidation.Day;
          }

          if (local.DateValidation.Day > local
            .DateValidation.MaximumNumberOfDaysInMonth)
          {
            local.Finished.Flag = "Y";
          }

          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      default:
        ExitState = "FN0000_INVALID_FREQ_CODE";

        break;
    }
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.ProcessCurrent.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.LastDateOfMonth.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.FirstPmtDue.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.LastDateOfMonth.Date = useExport.Last.Date;
  }

  private void UseFnAssembleDate1()
  {
    var useImport = new FnAssembleDate.Import();
    var useExport = new FnAssembleDate.Export();

    useImport.DateValidation.Assign(local.FirstPaymentDue);

    Call(FnAssembleDate.Execute, useImport, useExport);

    local.FirstDueDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnAssembleDate2()
  {
    var useImport = new FnAssembleDate.Import();
    var useExport = new FnAssembleDate.Export();

    useImport.DateValidation.Assign(local.DateValidation);

    Call(FnAssembleDate.Execute, useImport, useExport);

    local.Due.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnVerifyDueDate()
  {
    var useImport = new FnVerifyDueDate.Import();
    var useExport = new FnVerifyDueDate.Export();

    useImport.Persistent.Assign(import.Persistent);
    useImport.AccrualInstructions.Amount = import.AccrualInstructions.Amount;
    useImport.Due.Date = local.Due.Date;
    useImport.ProcessFrom.Date = import.ProcessFrom.Date;
    useImport.ProcessThru.Date = import.ProcessThru.Date;

    Call(FnVerifyDueDate.Execute, useImport, useExport);

    local.Debt.Amount = useExport.Debt.Amount;
    local.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    export.ValidDateInd.Flag = useExport.ValidDateInd.Flag;
    export.AccrualSuspension.SuspendDt = useExport.AccrualSuspension.SuspendDt;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of FirstAccrual.
    /// </summary>
    [JsonPropertyName("firstAccrual")]
    public Common FirstAccrual
    {
      get => firstAccrual ??= new();
      set => firstAccrual = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public AccrualInstructions Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public ObligationTransaction AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ProcessFrom.
    /// </summary>
    [JsonPropertyName("processFrom")]
    public DateWorkArea ProcessFrom
    {
      get => processFrom ??= new();
      set => processFrom = value;
    }

    /// <summary>
    /// A value of ProcessThru.
    /// </summary>
    [JsonPropertyName("processThru")]
    public DateWorkArea ProcessThru
    {
      get => processThru ??= new();
      set => processThru = value;
    }

    /// <summary>
    /// A value of ProcessCurrent.
    /// </summary>
    [JsonPropertyName("processCurrent")]
    public DateWorkArea ProcessCurrent
    {
      get => processCurrent ??= new();
      set => processCurrent = value;
    }

    private Common firstAccrual;
    private AccrualInstructions persistent;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction accrualInstructions;
    private DateWorkArea processFrom;
    private DateWorkArea processThru;
    private DateWorkArea processCurrent;
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private ObligationTransaction debt;
      private DebtDetail debtDetail;
    }

    /// <summary>
    /// A value of ZdelExportReadCounts.
    /// </summary>
    [JsonPropertyName("zdelExportReadCounts")]
    public Common ZdelExportReadCounts
    {
      get => zdelExportReadCounts ??= new();
      set => zdelExportReadCounts = value;
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
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of ValidDateInd.
    /// </summary>
    [JsonPropertyName("validDateInd")]
    public Common ValidDateInd
    {
      get => validDateInd ??= new();
      set => validDateInd = value;
    }

    /// <summary>
    /// A value of ProcessCurrent.
    /// </summary>
    [JsonPropertyName("processCurrent")]
    public DateWorkArea ProcessCurrent
    {
      get => processCurrent ??= new();
      set => processCurrent = value;
    }

    private Common zdelExportReadCounts;
    private Array<ExportGroup> export1;
    private AccrualSuspension accrualSuspension;
    private Common validDateInd;
    private DateWorkArea processCurrent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Mth.
      /// </summary>
      [JsonPropertyName("mth")]
      public Common Mth
      {
        get => mth ??= new();
        set => mth = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common mth;
    }

    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common common;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of FirstPmtDue.
    /// </summary>
    [JsonPropertyName("firstPmtDue")]
    public DateWorkArea FirstPmtDue
    {
      get => firstPmtDue ??= new();
      set => firstPmtDue = value;
    }

    /// <summary>
    /// A value of NextPmtDueMth.
    /// </summary>
    [JsonPropertyName("nextPmtDueMth")]
    public Common NextPmtDueMth
    {
      get => nextPmtDueMth ??= new();
      set => nextPmtDueMth = value;
    }

    /// <summary>
    /// A value of FirstPaymentDue.
    /// </summary>
    [JsonPropertyName("firstPaymentDue")]
    public DateValidation FirstPaymentDue
    {
      get => firstPaymentDue ??= new();
      set => firstPaymentDue = value;
    }

    /// <summary>
    /// A value of AccrualMonth.
    /// </summary>
    [JsonPropertyName("accrualMonth")]
    public Common AccrualMonth
    {
      get => accrualMonth ??= new();
      set => accrualMonth = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// A value of FirstDueDate.
    /// </summary>
    [JsonPropertyName("firstDueDate")]
    public DateWorkArea FirstDueDate
    {
      get => firstDueDate ??= new();
      set => firstDueDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of DaysSinceFirstDue.
    /// </summary>
    [JsonPropertyName("daysSinceFirstDue")]
    public Common DaysSinceFirstDue
    {
      get => daysSinceFirstDue ??= new();
      set => daysSinceFirstDue = value;
    }

    /// <summary>
    /// A value of FirstDateOfMonth.
    /// </summary>
    [JsonPropertyName("firstDateOfMonth")]
    public DateWorkArea FirstDateOfMonth
    {
      get => firstDateOfMonth ??= new();
      set => firstDateOfMonth = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of LastDateOfMonth.
    /// </summary>
    [JsonPropertyName("lastDateOfMonth")]
    public DateWorkArea LastDateOfMonth
    {
      get => lastDateOfMonth ??= new();
      set => lastDateOfMonth = value;
    }

    /// <summary>
    /// A value of DateValidation.
    /// </summary>
    [JsonPropertyName("dateValidation")]
    public DateValidation DateValidation
    {
      get => dateValidation ??= new();
      set => dateValidation = value;
    }

    /// <summary>
    /// A value of Due.
    /// </summary>
    [JsonPropertyName("due")]
    public DateWorkArea Due
    {
      get => due ??= new();
      set => due = value;
    }

    /// <summary>
    /// A value of AccrualWorkArea.
    /// </summary>
    [JsonPropertyName("accrualWorkArea")]
    public AccrualWorkArea AccrualWorkArea
    {
      get => accrualWorkArea ??= new();
      set => accrualWorkArea = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public Common Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of ValidDateInd.
    /// </summary>
    [JsonPropertyName("validDateInd")]
    public Common ValidDateInd
    {
      get => validDateInd ??= new();
      set => validDateInd = value;
    }

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of Finished.
    /// </summary>
    [JsonPropertyName("finished")]
    public Common Finished
    {
      get => finished ??= new();
      set => finished = value;
    }

    private Array<LocalGroup> local1;
    private DateWorkArea firstPmtDue;
    private Common nextPmtDueMth;
    private DateValidation firstPaymentDue;
    private Common accrualMonth;
    private Array<GrpGroup> grp;
    private DateWorkArea firstDueDate;
    private Common common;
    private Common daysSinceFirstDue;
    private DateWorkArea firstDateOfMonth;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DateWorkArea lastDateOfMonth;
    private DateValidation dateValidation;
    private DateWorkArea due;
    private AccrualWorkArea accrualWorkArea;
    private Common month;
    private Common validDateInd;
    private Common loop;
    private Common finished;
  }
#endregion
}

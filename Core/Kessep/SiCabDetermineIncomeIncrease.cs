// Program: SI_CAB_DETERMINE_INCOME_INCREASE, ID: 371766298, model: 746.
// Short name: SWE01921
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
/// A program: SI_CAB_DETERMINE_INCOME_INCREASE.
/// </para>
/// <para>
/// Computes the average monthly income for a given income amount and frequency.
/// </para>
/// </summary>
[Serializable]
public partial class SiCabDetermineIncomeIncrease: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_DETERMINE_INCOME_INCREASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabDetermineIncomeIncrease(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabDetermineIncomeIncrease.
  /// </summary>
  public SiCabDetermineIncomeIncrease(IContext context, Import import,
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
    // ***	Find out new monthly total.	***
    export.New1.TotalCurrency = 0;
    export.Total.TotalCurrency = 0;
    local.HighDate.Date = new DateTime(2099, 12, 31);
    local.IsToCalculateNew.Flag = "N";

    if (ReadIncomeSource1())
    {
      switch(AsChar(entities.ExistingIncomeSource.Type1))
      {
        case 'E':
          if (AsChar(entities.ExistingIncomeSource.ReturnCd) == 'E')
          {
            local.IsToCalculateNew.Flag = "Y";
          }

          break;
        case 'M':
          if (AsChar(entities.ExistingIncomeSource.ReturnCd) == 'A' || AsChar
            (entities.ExistingIncomeSource.ReturnCd) == 'R')
          {
            local.IsToCalculateNew.Flag = "Y";
          }

          break;
        default:
          break;
      }

      if (AsChar(local.IsToCalculateNew.Flag) == 'Y')
      {
        switch(AsChar(import.PersonIncomeHistory.Freq))
        {
          case 'M':
            // -------
            // Monthly
            // -------
            export.New1.TotalCurrency =
              import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault();

            break;
          case 'W':
            // -------
            // Weekly
            // -------
            export.New1.TotalCurrency =
              import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault() * 52 / 12
              ;

            break;
          case 'B':
            // ---------
            // Bi-Weekly
            // ---------
            export.New1.TotalCurrency =
              import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault() * 26 / 12
              ;

            break;
          case 'S':
            // ------------
            // Semi-Monthly
            // ------------
            export.New1.TotalCurrency =
              import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault() * 2;

            break;
          default:
            break;
        }

        export.New1.TotalCurrency += import.PersonIncomeHistory.
          MilitaryBaqAllotment.GetValueOrDefault();
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";

      return;
    }

    // ******************************************
    // PR# 80718    Date : 01-28-2000
    // Sree :
    // In order to calculate the total monthly income, read all the the income 
    // sources with end date eaqal to high date.
    // If End-dat is not equal to HIGH-DATE, Check for income source type and 
    // return code. The following are the conditions for adding the income to
    // total monthly income.
    // TYPE                    RETURN CODES
    // ---------              -----------------------------
    // M                            A, R
    // E                            E
    // ********************************************
    foreach(var item in ReadIncomeSource2())
    {
      local.IsToAddTotal.Flag = "N";

      switch(AsChar(entities.ExistingIncomeSource.Type1))
      {
        case 'E':
          if (AsChar(entities.ExistingIncomeSource.ReturnCd) == 'E')
          {
            local.IsToAddTotal.Flag = "Y";
          }

          break;
        case 'M':
          if (AsChar(entities.ExistingIncomeSource.ReturnCd) == 'A' || AsChar
            (entities.ExistingIncomeSource.ReturnCd) == 'R')
          {
            local.IsToAddTotal.Flag = "Y";
          }

          break;
        default:
          break;
      }

      if (AsChar(local.IsToAddTotal.Flag) == 'Y')
      {
        if (ReadPersonIncomeHistory())
        {
          switch(AsChar(entities.ExistingPersonIncomeHistory.Freq))
          {
            case 'M':
              // -------
              // Monthly
              // -------
              export.Total.TotalCurrency =
                entities.ExistingPersonIncomeHistory.IncomeAmt.
                  GetValueOrDefault() + export.Total.TotalCurrency;

              break;
            case 'W':
              // -------
              // Weekly
              // -------
              export.Total.TotalCurrency =
                entities.ExistingPersonIncomeHistory.IncomeAmt.
                  GetValueOrDefault() * 52 / 12 + export.Total.TotalCurrency;

              break;
            case 'B':
              // ---------
              // Bi-Weekly
              // ---------
              export.Total.TotalCurrency =
                entities.ExistingPersonIncomeHistory.IncomeAmt.
                  GetValueOrDefault() * 26 / 12 + export.Total.TotalCurrency;

              break;
            case 'S':
              // ------------
              // Semi-Monthly
              // ------------
              export.Total.TotalCurrency =
                entities.ExistingPersonIncomeHistory.IncomeAmt.
                  GetValueOrDefault() * 2 + export.Total.TotalCurrency;

              break;
            default:
              break;
          }

          export.Total.TotalCurrency += entities.ExistingPersonIncomeHistory.
            MilitaryBaqAllotment.GetValueOrDefault();
        }
      }
    }
  }

  private bool ReadIncomeSource1()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.ReturnCd =
          db.GetNullableString(reader, 2);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 3);
        entities.ExistingIncomeSource.EndDt = db.GetNullableDate(reader, 4);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private IEnumerable<bool> ReadIncomeSource2()
  {
    entities.ExistingIncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDt", local.HighDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.ReturnCd =
          db.GetNullableString(reader, 2);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 3);
        entities.ExistingIncomeSource.EndDt = db.GetNullableDate(reader, 4);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);

        return true;
      });
  }

  private bool ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.ExistingPersonIncomeHistory.Populated = false;

    return Read("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPersonIncomeHistory.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonIncomeHistory.IsrIdentifier =
          db.GetDateTime(reader, 1);
        entities.ExistingPersonIncomeHistory.Identifier =
          db.GetDateTime(reader, 2);
        entities.ExistingPersonIncomeHistory.IncomeEffDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingPersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingPersonIncomeHistory.Freq =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonIncomeHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.ExistingPersonIncomeHistory.CspINumber =
          db.GetString(reader, 7);
        entities.ExistingPersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingPersonIncomeHistory.Populated = true;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    private IncomeSource incomeSource;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PersonIncomeHistory personIncomeHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Common New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Common total;
    private Common new1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IsToCalculateNew.
    /// </summary>
    [JsonPropertyName("isToCalculateNew")]
    public Common IsToCalculateNew
    {
      get => isToCalculateNew ??= new();
      set => isToCalculateNew = value;
    }

    /// <summary>
    /// A value of IsToAddTotal.
    /// </summary>
    [JsonPropertyName("isToAddTotal")]
    public Common IsToAddTotal
    {
      get => isToAddTotal ??= new();
      set => isToAddTotal = value;
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

    private Common isToCalculateNew;
    private Common isToAddTotal;
    private DateWorkArea highDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("existingPersonIncomeHistory")]
    public PersonIncomeHistory ExistingPersonIncomeHistory
    {
      get => existingPersonIncomeHistory ??= new();
      set => existingPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private PersonIncomeHistory existingPersonIncomeHistory;
    private IncomeSource existingIncomeSource;
    private CsePerson existingCsePerson;
  }
#endregion
}

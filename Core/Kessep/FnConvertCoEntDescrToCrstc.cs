// Program: FN_CONVERT_CO_ENT_DESCR_TO_CRSTC, ID: 372414840, model: 746.
// Short name: SWE02439
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CONVERT_CO_ENT_DESCR_TO_CRSTC.
/// </summary>
[Serializable]
public partial class FnConvertCoEntDescrToCrstc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CONVERT_CO_ENT_DESCR_TO_CRSTC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnConvertCoEntDescrToCrstc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnConvertCoEntDescrToCrstc.
  /// </summary>
  public FnConvertCoEntDescrToCrstc(IContext context, Import import,
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
    // *********************************************************************************
    // DATE		WHO		DESCRIPTION
    // 02/01/99	M Fangman    	Rewritten
    // 01/04/00  M Fangman             PR 82289 - pass flag back to indicate 
    // that a particular source uses the addendum record (employers, individuals
    // & other states).
    // *********************************************************************************
    // This action block will convert a Company Entry Description into a Cash 
    // Receipt Source Type Code and read to ensure that it is valid.
    export.ValidCoEntryDesc.Flag = "N";
    export.SourceUsesAddendum.Flag = "N";

    if (Equal(import.ElectronicFundTransmission.CompanyEntryDescription,
      "EMPLOYER") || Equal
      (import.ElectronicFundTransmission.CompanyEntryDescription, "FDSO") || Equal
      (import.ElectronicFundTransmission.CompanyEntryDescription, "INDIVIDUAL"))
    {
      switch(TrimEnd(import.ElectronicFundTransmission.
        CompanyEntryDescription ?? ""))
      {
        case "EMPLOYER":
          local.CashReceiptSourceType.SystemGeneratedIdentifier = 212;

          break;
        case "FDSO":
          local.CashReceiptSourceType.SystemGeneratedIdentifier = 1;

          break;
        case "INDIVIDUAL":
          local.CashReceiptSourceType.SystemGeneratedIdentifier = 4;

          break;
        default:
          break;
      }

      if (ReadCashReceiptSourceType3())
      {
        export.ValidCoEntryDesc.Flag = "Y";

        if (Equal(import.ElectronicFundTransmission.CompanyEntryDescription,
          "EMPLOYER") || Equal
          (import.ElectronicFundTransmission.CompanyEntryDescription,
          "INDIVIDUAL"))
        {
          export.SourceUsesAddendum.Flag = "Y";
        }
      }
      else
      {
        // Not a valid Company Entry Description.  Default to flag of "N".
      }

      return;
    }

    local.Numeric.Text10 = "0123456789";
    local.NumericInd.Count =
      Verify(Substring(
        import.ElectronicFundTransmission.CompanyEntryDescription, 10, 3, 2),
      local.Numeric.Text10);

    if (local.NumericInd.Count == 0)
    {
      // A FIPS code was found in the State positions (3 & 4).
      local.CashReceiptSourceType.State =
        (int?)StringToNumber(Substring(
          import.ElectronicFundTransmission.CompanyEntryDescription, 10, 3, 2));
        

      if (local.CashReceiptSourceType.State.GetValueOrDefault() == 20)
      {
        // Verify the state abbreviation against the Cash_Receipt_Source_Type 
        // Code.
        if (!Equal(import.ElectronicFundTransmission.CompanyEntryDescription, 1,
          2, "KS"))
        {
          // Not a valid Company Entry Description.  Default to flag of "N".
          return;
        }

        // This is a Kansas FIPS code so this must be a court interface source 
        // code.
        local.NumericInd.Count =
          Verify(Substring(
            import.ElectronicFundTransmission.CompanyEntryDescription, 10, 5,
          5), local.Numeric.Text10);

        if (local.NumericInd.Count == 0)
        {
          // The county and location FIPS codes are numeric.
          local.CashReceiptSourceType.County =
            (int?)StringToNumber(Substring(
              import.ElectronicFundTransmission.CompanyEntryDescription, 10, 5,
            3));
          local.CashReceiptSourceType.Location =
            (int?)StringToNumber(Substring(
              import.ElectronicFundTransmission.CompanyEntryDescription, 10, 8,
            2));

          if (ReadCashReceiptSourceType1())
          {
            export.ValidCoEntryDesc.Flag = "Y";
          }
          else
          {
            // Not a valid Company Entry Description.  Default to flag of "N".
          }
        }
        else
        {
          // Not a valid Company Entry Description.  Default to flag of "N".
        }
      }
      else
      {
        // This is not a Kansas FIPS code so this must be an interstate source 
        // code.
        local.CashReceiptSourceType.Code =
          Substring(import.ElectronicFundTransmission.CompanyEntryDescription,
          10, 1, 2) + " STATE  ";

        if (ReadCashReceiptSourceType2())
        {
          export.ValidCoEntryDesc.Flag = "Y";
          export.SourceUsesAddendum.Flag = "Y";
        }
        else
        {
          // Not a valid Company Entry Description.  Default to flag of "N".
        }
      }
    }
    else
    {
      // Not a valid Company Entry Description.  Default to flag of "N".
    }
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
        db.SetNullableInt32(
          command, "county",
          local.CashReceiptSourceType.County.GetValueOrDefault());
        db.SetNullableInt32(
          command, "location",
          local.CashReceiptSourceType.Location.GetValueOrDefault());
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
        db.SetString(command, "code", local.CashReceiptSourceType.Code);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType3()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType3",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "crSrceTypeId",
          local.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptSourceType.Populated = true;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ValidCoEntryDesc.
    /// </summary>
    [JsonPropertyName("validCoEntryDesc")]
    public Common ValidCoEntryDesc
    {
      get => validCoEntryDesc ??= new();
      set => validCoEntryDesc = value;
    }

    /// <summary>
    /// A value of SourceUsesAddendum.
    /// </summary>
    [JsonPropertyName("sourceUsesAddendum")]
    public Common SourceUsesAddendum
    {
      get => sourceUsesAddendum ??= new();
      set => sourceUsesAddendum = value;
    }

    private Common validCoEntryDesc;
    private Common sourceUsesAddendum;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Numeric.
    /// </summary>
    [JsonPropertyName("numeric")]
    public TextWorkArea Numeric
    {
      get => numeric ??= new();
      set => numeric = value;
    }

    /// <summary>
    /// A value of NumericInd.
    /// </summary>
    [JsonPropertyName("numericInd")]
    public Common NumericInd
    {
      get => numericInd ??= new();
      set => numericInd = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private TextWorkArea numeric;
    private Common numericInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
  }
#endregion
}

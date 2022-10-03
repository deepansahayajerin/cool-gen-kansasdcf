// Program: SI_READ_AP_LOCATE_HISTORY, ID: 372512670, model: 746.
// Short name: SWE01199
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_AP_LOCATE_HISTORY.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD will read the CSENet AP Locate History entity and will populate the
/// CSENet AP Locate History Screen.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadApLocateHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_AP_LOCATE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadApLocateHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadApLocateHistory.
  /// </summary>
  public SiReadApLocateHistory(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date     Developer    Request #  Description
    // 5/3/95   Sherri Newman        0  Initial Dev.
    // 3/15/99 P. Sharp  Added new fields to views. Added transaction_date to 
    // reads.
    // ---------------------------------------------
    if (ReadInterstateApLocateInterstateApIdentification())
    {
      // ---------------------------------------------
      // Format AP'S Name.
      // ---------------------------------------------
      local.CsePersonsWorkSet.LastName =
        entities.InterstateApIdentification.NameLast ?? Spaces(17);
      local.CsePersonsWorkSet.FirstName =
        entities.InterstateApIdentification.NameFirst;
      local.CsePersonsWorkSet.MiddleInitial =
        Substring(entities.InterstateApIdentification.MiddleName, 1, 1);
      UseSiFormatCsePersonName1();

      // ---------------------------------------------
      // Format Alias1 name.
      // ---------------------------------------------
      if (!IsEmpty(entities.InterstateApLocate.Alias1LastName))
      {
        local.CsePersonsWorkSet.LastName =
          entities.InterstateApLocate.Alias1LastName ?? Spaces(17);
        local.CsePersonsWorkSet.FirstName =
          entities.InterstateApLocate.Alias1FirstName ?? Spaces(12);
        local.CsePersonsWorkSet.MiddleInitial =
          Substring(entities.InterstateApLocate.Alias1MiddleName, 1, 1);
        UseSiFormatCsePersonName2();
      }

      // ---------------------------------------------
      // Format Alias2 name.
      // ---------------------------------------------
      if (!IsEmpty(entities.InterstateApLocate.Alias2LastName))
      {
        local.CsePersonsWorkSet.LastName =
          entities.InterstateApLocate.Alias2LastName ?? Spaces(17);
        local.CsePersonsWorkSet.FirstName =
          entities.InterstateApLocate.Alias2FirstName ?? Spaces(12);
        local.CsePersonsWorkSet.MiddleInitial =
          Substring(entities.InterstateApLocate.Alias2MiddleName, 1, 1);
        UseSiFormatCsePersonName3();
      }

      // ---------------------------------------------
      // Format Alias3 name.
      // ---------------------------------------------
      if (!IsEmpty(entities.InterstateApLocate.Alias3LastName))
      {
        local.CsePersonsWorkSet.LastName =
          entities.InterstateApLocate.Alias3LastName ?? Spaces(17);
        local.CsePersonsWorkSet.FirstName =
          entities.InterstateApLocate.Alias3FirstName ?? Spaces(12);
        local.CsePersonsWorkSet.MiddleInitial =
          Substring(entities.InterstateApLocate.Alias3MiddleName, 1, 1);
        UseSiFormatCsePersonName4();
      }

      MoveInterstateApLocate(entities.InterstateApLocate,
        export.InterstateApLocate);
      MoveInterstateApIdentification(entities.InterstateApIdentification,
        export.InterstateApIdentification);
    }
    else
    {
      ExitState = "CSENET_AP_LOCATE_NF";
    }
  }

  private static void MoveInterstateApIdentification(
    InterstateApIdentification source, InterstateApIdentification target)
  {
    target.Ssn = source.Ssn;
    target.DateOfBirth = source.DateOfBirth;
  }

  private static void MoveInterstateApLocate(InterstateApLocate source,
    InterstateApLocate target)
  {
    target.LastResAddressLine1 = source.LastResAddressLine1;
    target.LastResAddressLine2 = source.LastResAddressLine2;
    target.LastResCity = source.LastResCity;
    target.LastResState = source.LastResState;
    target.LastResZipCode5 = source.LastResZipCode5;
    target.LastResZipCode4 = source.LastResZipCode4;
    target.LastResAddressDate = source.LastResAddressDate;
    target.LastMailAddressLine1 = source.LastMailAddressLine1;
    target.LastMailAddressLine2 = source.LastMailAddressLine2;
    target.LastMailCity = source.LastMailCity;
    target.LastMailState = source.LastMailState;
    target.LastMailZipCode5 = source.LastMailZipCode5;
    target.LastMailZipCode4 = source.LastMailZipCode4;
    target.LastMailAddressDate = source.LastMailAddressDate;
    target.LastEmployerName = source.LastEmployerName;
    target.LastEmployerDate = source.LastEmployerDate;
    target.LastEmployerAddressLine1 = source.LastEmployerAddressLine1;
    target.LastEmployerAddressLine2 = source.LastEmployerAddressLine2;
    target.LastEmployerCity = source.LastEmployerCity;
    target.LastEmployerState = source.LastEmployerState;
    target.LastEmployerZipCode5 = source.LastEmployerZipCode5;
    target.LastEmployerZipCode4 = source.LastEmployerZipCode4;
    target.LastEmployerEndDate = source.LastEmployerEndDate;
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Ap.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Alias1.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName3()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Alias2.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName4()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Alias3.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadInterstateApLocateInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocateInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApLocate.Alias1FirstName =
          db.GetNullableString(reader, 2);
        entities.InterstateApLocate.Alias1MiddleName =
          db.GetNullableString(reader, 3);
        entities.InterstateApLocate.Alias1LastName =
          db.GetNullableString(reader, 4);
        entities.InterstateApLocate.Alias2FirstName =
          db.GetNullableString(reader, 5);
        entities.InterstateApLocate.Alias2MiddleName =
          db.GetNullableString(reader, 6);
        entities.InterstateApLocate.Alias2LastName =
          db.GetNullableString(reader, 7);
        entities.InterstateApLocate.Alias3FirstName =
          db.GetNullableString(reader, 8);
        entities.InterstateApLocate.Alias3MiddleName =
          db.GetNullableString(reader, 9);
        entities.InterstateApLocate.Alias3LastName =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.LastResAddressLine1 =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.LastResAddressLine2 =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.LastResCity =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.LastResState =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.LastResZipCode5 =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.LastResZipCode4 =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.LastResAddressDate =
          db.GetNullableDate(reader, 17);
        entities.InterstateApLocate.LastMailAddressLine1 =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.LastMailAddressLine2 =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.LastMailCity =
          db.GetNullableString(reader, 20);
        entities.InterstateApLocate.LastMailState =
          db.GetNullableString(reader, 21);
        entities.InterstateApLocate.LastMailZipCode5 =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.LastMailZipCode4 =
          db.GetNullableString(reader, 23);
        entities.InterstateApLocate.LastMailAddressDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateApLocate.LastEmployerName =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.LastEmployerDate =
          db.GetNullableDate(reader, 26);
        entities.InterstateApLocate.LastEmployerAddressLine1 =
          db.GetNullableString(reader, 27);
        entities.InterstateApLocate.LastEmployerAddressLine2 =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.LastEmployerCity =
          db.GetNullableString(reader, 29);
        entities.InterstateApLocate.LastEmployerState =
          db.GetNullableString(reader, 30);
        entities.InterstateApLocate.LastEmployerZipCode5 =
          db.GetNullableString(reader, 31);
        entities.InterstateApLocate.LastEmployerZipCode4 =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.LastEmployerEndDate =
          db.GetNullableDate(reader, 33);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 34);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 35);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 36);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 37);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 38);
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateApLocate.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Alias1.
    /// </summary>
    [JsonPropertyName("alias1")]
    public CsePersonsWorkSet Alias1
    {
      get => alias1 ??= new();
      set => alias1 = value;
    }

    /// <summary>
    /// A value of Alias2.
    /// </summary>
    [JsonPropertyName("alias2")]
    public CsePersonsWorkSet Alias2
    {
      get => alias2 ??= new();
      set => alias2 = value;
    }

    /// <summary>
    /// A value of Alias3.
    /// </summary>
    [JsonPropertyName("alias3")]
    public CsePersonsWorkSet Alias3
    {
      get => alias3 ??= new();
      set => alias3 = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet alias1;
    private CsePersonsWorkSet alias2;
    private CsePersonsWorkSet alias3;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private InterstateApLocate interstateApLocate;
  }
#endregion
}
